using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEditor;
using UnityEngine;
using CupkekGames.RPGStats;
using CupkekGames.ShapeDrawing;
using CupkekGames.TimeSystem;
using CupkekGames.Units;

namespace CupkekGames.Combat.Tests
{
    /// <summary>
    /// A scene-free combat world for the package's tests: a clock, an attribute
    /// registry (HP, MP, ATK), an element table and settings, and units built
    /// from in-memory definitions. Assets the runtime keeps private are authored
    /// through <see cref="SerializedObject"/>, as the inspector would.
    /// </summary>
    internal sealed class CombatTestWorld
    {
        public const int MaxMP = 100;

        public readonly FakeCombatSettings Settings = new();
        public AttributeDefinitionSO HP { get; }
        public AttributeDefinitionSO MP { get; }
        public AttributeDefinitionSO ATK { get; }

        private readonly TimeManager _time;
        private readonly List<Object> _owned = new();

        public CombatTestWorld()
        {
            GameObject clock = new GameObject("CombatTestWorld.Time");
            _owned.Add(clock);
            _time = clock.AddComponent<TimeManager>();

            HP = Own(Attribute("HP"));
            MP = Own(Attribute("MP"));
            ATK = Own(Attribute("ATK"));

            CombatAttributeRegistrySO registry = Own(ScriptableObject.CreateInstance<CombatAttributeRegistrySO>());
            SerializedObject so = new SerializedObject(registry);
            SerializedProperty attributes = so.FindProperty("_attributes");
            SerializedProperty roles = so.FindProperty("_roles");
            (string role, AttributeDefinitionSO attribute)[] slots =
            {
                (CombatRoles.HP, HP), (CombatRoles.MP, MP), (CombatRoles.ATK, ATK),
            };
            attributes.arraySize = slots.Length;
            roles.arraySize = slots.Length;
            for (int i = 0; i < slots.Length; i++)
            {
                attributes.GetArrayElementAtIndex(i).objectReferenceValue = slots[i].attribute;
                SerializedProperty entry = roles.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("Role").stringValue = slots[i].role;
                entry.FindPropertyRelative("Attribute").objectReferenceValue = slots[i].attribute;
            }
            so.ApplyModifiedPropertiesWithoutUndo();

            Settings.Attributes = registry;
            Settings.ElementRelationshipTable = Own(ScriptableObject.CreateInstance<ElementRelationshipTableSO>());
        }

        public T Own<T>(T asset) where T : Object
        {
            _owned.Add(asset);
            return asset;
        }

        public ElementTypeDefinitionSO Element(string name)
        {
            ElementTypeDefinitionSO element = Own(ScriptableObject.CreateInstance<ElementTypeDefinitionSO>());
            element.name = name;
            return element;
        }

        /// <summary>Attacks in <paramref name="attacker"/> deal <paramref name="multiplier"/> against <paramref name="defender"/>.</summary>
        public void Relate(ElementTypeDefinitionSO attacker, ElementTypeDefinitionSO defender, float multiplier)
        {
            SerializedObject so = new SerializedObject(Settings.ElementRelationshipTable);
            SerializedProperty relationships = so.FindProperty("_relationships");
            int index = relationships.arraySize;
            relationships.arraySize = index + 1;
            SerializedProperty entry = relationships.GetArrayElementAtIndex(index);
            entry.FindPropertyRelative("Attacker").objectReferenceValue = attacker;
            entry.FindPropertyRelative("Defender").objectReferenceValue = defender;
            entry.FindPropertyRelative("DamageMultiplier").floatValue = multiplier;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>An initialised unit of <paramref name="team"/> with its base health and attack, in its element.</summary>
        public CombatUnit Unit(string key, int team = 0, int level = 1, float hp = 100f, float atk = 20f,
            ElementTypeDefinitionSO element = null, params IUnitFeature[] features)
        {
            CombatAttributesDefinition combat = new CombatAttributesDefinition { Element = element };
            combat.BaseAttributes.SetValue(HP, hp);
            combat.BaseAttributes.SetValue(ATK, atk);

            UnitDefinitionSO definition = Own(ScriptableObject.CreateInstance<UnitDefinitionSO>());
            definition.name = key;
            definition.AddDefinition(combat);

            CombatUnit unit = new CombatUnit(new CombatUnitReference(team, key, level), Settings, null, _time, definition);
            foreach (IUnitFeature feature in features) unit.AddFeature(feature);
            unit.Initialize();
            return unit;
        }

        /// <summary>An effect that multiplies one attribute, authored as the inspector would.</summary>
        public AttributeEffect Multiply(AttributeDefinitionSO attribute, float multiplier)
        {
            EffectHost host = Own(ScriptableObject.CreateInstance<EffectHost>());
            SerializedObject so = new SerializedObject(host);
            SerializedProperty entries = so.FindProperty("Effect._entries");
            entries.arraySize = 1;
            SerializedProperty entry = entries.GetArrayElementAtIndex(0);
            entry.FindPropertyRelative("AttributeKey.Key").stringValue = attribute.name;
            entry.FindPropertyRelative("Multiplier").floatValue = multiplier;
            so.ApplyModifiedPropertiesWithoutUndo();
            return host.Effect;
        }

        public void Dispose()
        {
            foreach (Object owned in _owned)
            {
                if (owned != null) Object.DestroyImmediate(owned);
            }

            _owned.Clear();
        }

        private static AttributeDefinitionSO Attribute(string name)
        {
            AttributeDefinitionSO attribute = ScriptableObject.CreateInstance<AttributeDefinitionSO>();
            attribute.name = name;
            return attribute;
        }
    }

    /// <summary>Plain settings: no base values, full damage through defense, 100 mana.</summary>
    internal sealed class FakeCombatSettings : ICombatSettings
    {
        public CombatAttributeRegistrySO Attributes { get; set; }
        public ElementRelationshipTableSO ElementRelationshipTable { get; set; }

        public int AttackSpeedBase => 1000;
        public AttributeDisplayConfigSO AttributeDisplayConfig => null;
        public CombatDescriptionStyleSO DescriptionStyle => null;
        public IReadOnlyList<IDamageModifier> DamageModifiers { get; } = new List<IDamageModifier>();
        public float GetDamageTakenMultiplier(float totalDefense) => 1f;
        public float GetBaseValue(AttributeDefinitionSO attribute) => 0f;

        public Color HitColor => Color.white;
        public Color HitColorEmission => Color.white;
        public int HitColorWeight => 0;
        public int HitColorDurationMS => 0;
        public float HitSquashAndStretchBumpAmount => 0f;
        public float CritCameraShakeIntensity => 0f;
        public float HoverOutlineWidth => 0f;
        public float HoverOutlineFadeInDuration => 0f;
        public float HoverOutlineFadeOutDuration => 0f;
        public int BossBarMinTier => int.MaxValue;
        public IndicatorSettings IndicatorSettings => null;
        public float WorldSpaceMinDistance => 0f;
        public float WorldSpaceMaxDistance => 0f;
        public float WorldSpaceMinScale => 1f;
        public float WorldSpaceMaxScale => 1f;
        public float HealthBarGapPerHealth => 0f;

        public int MaxRosterSize => 5;
        public int MaxMP => CombatTestWorld.MaxMP;
        public int DefaultActionTypeId => 0;
        public int FullManaActionTypeId => 1;
        public IReadOnlyList<ActionManaEffect> ActionManaEffects { get; } = new List<ActionManaEffect>();
        public int TakeDamageManaInterval => int.MaxValue;
        public float ThreatTableCheckInterval => 1f;
        public int DistanceThreat => 10;
        public float AIColliderRadius => 0.5f;
        public float AIColliderHeight => 2f;
        public Vector3 AIColliderCenter => Vector3.zero;
    }

    /// <summary>A field with the given allies and enemies; time scale changes are recorded.</summary>
    internal sealed class FakeUnitManager : ICombatUnitManager
    {
        public readonly List<CombatUnit> Allies = new();
        public readonly List<CombatUnit> Enemies = new();
        public readonly List<float> TimeScales = new();

        public ReadOnlyCollection<CombatUnit> CombatUnitsAlly => Allies.AsReadOnly();
        public ReadOnlyCollection<CombatUnit> CombatUnitsEnemy => Enemies.AsReadOnly();
        public void SetTimeScale(float timeScale, CombatUnit except) => TimeScales.Add(timeScale);
        public void SpawnEnemy(CombatUnitReference enemy, Vector2Int? position = null) { }
        public ICombatSpace Space { get; } = new FakeSpace();
    }

    /// <summary>A space that measures between the views in metres (one combat unit is one metre) and covers nothing.</summary>
    internal sealed class FakeSpace : ICombatSpace
    {
        public float Distance(CombatUnit a, CombatUnit b) => Vector3.Distance(a.View.transform.position, b.View.transform.position);
        public bool InRange(CombatUnit caster, CombatUnit target, float range) => Distance(caster, target) <= range;
        public void Collect(in CombatArea area, List<CombatUnit> results) => results.Clear();
        public float ToWorld(float units) => units;
        public ICombatMover CreateMover(CombatUnitView view) => throw new System.NotSupportedException("The test world moves nothing.");
    }
}
