using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using CupkekGames.RPGStats;

namespace CupkekGames.Combat.Tests
{
    /// <summary>
    /// Skill descriptions read on more than one surface: the rules' style is the default, a style
    /// the caller names recolours the roles and the damage types it lists for its surface, a damage
    /// type it does not list keeps its own colour, and buff colours come from the style's display
    /// config when it has one.
    /// </summary>
    public class DescriptionStyleTests
    {
        private CombatTestWorld _world;
        private DamageTypeDefinitionSO _physical;
        private DamageTypeDefinitionSO _magical;

        [SetUp]
        public void SetUp()
        {
            _world = new CombatTestWorld();
            _physical = DamageType("Physical", "<color=#F56969>");
            _magical = DamageType("Magical", "<color=#698CF5>");
        }

        [TearDown]
        public void TearDown() => _world.Dispose();

        private DamageTypeDefinitionSO DamageType(string displayName, string richTextColor)
        {
            DamageTypeDefinitionSO type = _world.Own(ScriptableObject.CreateInstance<DamageTypeDefinitionSO>());
            SerializedObject so = new SerializedObject(type);
            so.FindProperty("_displayName").stringValue = displayName;
            so.FindProperty("_richTextColor").stringValue = richTextColor;
            so.ApplyModifiedPropertiesWithoutUndo();
            return type;
        }

        private CombatDescriptionStyleSO Style(Color heal, (DamageTypeDefinitionSO type, Color color)[] damage = null,
            AttributeDisplayConfigSO attributes = null)
        {
            CombatDescriptionStyleSO style = _world.Own(ScriptableObject.CreateInstance<CombatDescriptionStyleSO>());
            SerializedObject so = new SerializedObject(style);
            so.FindProperty("_heal").FindPropertyRelative("Color").colorValue = heal;
            so.FindProperty("_attributeDisplayConfig").objectReferenceValue = attributes;

            SerializedProperty types = so.FindProperty("_damageTypes");
            damage ??= new (DamageTypeDefinitionSO, Color)[0];
            types.arraySize = damage.Length;
            for (int i = 0; i < damage.Length; i++)
            {
                SerializedProperty entry = types.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("DamageType").objectReferenceValue = damage[i].type;
                entry.FindPropertyRelative("Color").colorValue = damage[i].color;
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            return style;
        }

        /// <summary>An action with the given description over one node worth a flat 10.</summary>
        private CombatActionSO ActionOf<TNode>(string description, DamageTypeDefinitionSO type) where TNode : CombatActionNodeWithTarget
        {
            TNode node = _world.Own(ScriptableObject.CreateInstance<TNode>());
            SerializedObject so = new SerializedObject(node);
            SerializedProperty values = so.FindProperty("_damage");
            values.arraySize = 1;
            values.GetArrayElementAtIndex(0).FindPropertyRelative("Flat").floatValue = 10f;
            so.FindProperty("_damageType").objectReferenceValue = type;
            so.ApplyModifiedPropertiesWithoutUndo();

            CombatActionSO action = _world.Own(ScriptableObject.CreateInstance<CombatActionSO>());
            action.Description = description;
            action.AddNode(node);
            return action;
        }

        [Test]
        public void ADamageFragment_TakesTheSurfacesShade_AndATypeItDoesNotList_KeepsItsOwn()
        {
            _world.Settings.DescriptionStyle = Style(Color.white);
            CombatDescriptionStyleSO cream = Style(Color.white, new[] { (_physical, (Color)new Color32(0xB0, 0x3A, 0x3A, 0xFF)) });

            Assert.AreEqual("Deals <color=#F56969><b>10</b> physical damage</color>.",
                ActionOf<CombatActionNodeDamage>("Deals {node0}.", _physical).GetDescription(1, _world.Settings),
                "no style named: the rules' style, and the type's own colour");
            Assert.AreEqual("Deals <color=#B03A3A><b>10</b> physical damage</color>.",
                ActionOf<CombatActionNodeDamage>("Deals {node0}.", _physical).GetDescription(1, _world.Settings, style: cream),
                "the surface's shade for the type");
            Assert.AreEqual("Deals <color=#698CF5><b>10</b> magical damage</color>.",
                ActionOf<CombatActionNodeDamage>("Deals {node0}.", _magical).GetDescription(1, _world.Settings, style: cream),
                "a type the surface does not list keeps its own colour");
        }

        [Test]
        public void ARoleFragment_TakesTheColourOfTheStyleItIsReadOn()
        {
            _world.Settings.DescriptionStyle = Style(Color.green);
            CombatDescriptionStyleSO cream = Style(new Color32(0x3E, 0x7A, 0x2E, 0xFF));
            CombatActionSO mend = ActionOf<CombatActionNodeHeal>("Mend: {node0}.", _magical);

            Assert.AreEqual("Mend: <color=#00FF00>restores <b>10</b> health</color>.", mend.GetDescription(1, _world.Settings),
                "no style named: the rules' style");
            Assert.AreEqual("Mend: <color=#3E7A2E>restores <b>10</b> health</color>.",
                mend.GetDescription(1, _world.Settings, style: cream));
        }

        [Test]
        public void BuffColours_ComeFromTheStylesDisplayConfig_ElseTheRules()
        {
            AttributeDisplayConfigSO dark = _world.Own(ScriptableObject.CreateInstance<AttributeDisplayConfigSO>());
            AttributeDisplayConfigSO light = _world.Own(ScriptableObject.CreateInstance<AttributeDisplayConfigSO>());
            _world.Settings.AttributeDisplayConfig = dark;

            Assert.AreSame(dark, Style(Color.white).GetAttributeDisplayConfig(_world.Settings), "none of its own: the rules' config");
            Assert.AreSame(light, Style(Color.white, attributes: light).GetAttributeDisplayConfig(_world.Settings));
        }
    }
}
