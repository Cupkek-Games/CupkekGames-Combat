using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using CupkekGames.InventorySystem;
using CupkekGames.TimeSystem;
using CupkekGames.RPGStats;
using CupkekGames.Units;
using CupkekGames.Character;

namespace CupkekGames.Combat
{
    public class CombatUnit
    {
        // ── Unit core (identity + features) ──
        private readonly Unit _unit = new();

        public Guid ID
        {
            get => Guid.TryParse(_unit.ID, out var g) ? g : Guid.Empty;
            set => _unit.ID = value.ToString();
        }

        public string Key { get => _unit.Key; set => _unit.Key = value; }

        public void AddFeature(IUnitFeature feature) => _unit.AddFeature(feature);
        public T GetFeature<T>() where T : class, IUnitFeature => _unit.GetFeature<T>();
        public bool HasFeature<T>() where T : class, IUnitFeature => _unit.HasFeature<T>();
        public IReadOnlyList<IUnitFeature> Features => _unit.Features;

        // ── Definition data ──
        private CombatUnitReference _dataReference;
        public CombatUnitReference DataReference => _dataReference;
        private UnitDefinitionSO _data;
        public UnitDefinitionSO Data => _data;

        /// <summary>Combat-specific definition data via GetDefinition.</summary>
        public CombatAttributesDefinition CombatData => _data?.GetDefinition<CombatAttributesDefinition>();

        /// <summary>Character/model definition data via GetDefinition.</summary>
        public CharacterDefinition CharacterData => _data?.GetDefinition<CharacterDefinition>();

        public AttributeSet EquipmentBonus;
        public int Level = 1;

        // ── Sub-systems ──
        public CombatUnitHealth Health { get; private set; }
        public CombatUnitMana Mana { get; private set; }
        public CombatUnitBuffSystem Buffs { get; private set; }
        public CombatUnitStatusSystem StatusEffects { get; private set; }
        public IPowerLevelCalculator PowerLevel { get; set; }
        public CombatUnitShield Shield;

        // ── References ──
        protected ICombatSettings _combatSettings;
        public CombatAttributeRegistrySO Attributes => _combatSettings.Attributes;
        private IUnitSOProvider _unitSOProvider;

        // ── Events ──
        public event Action<CombatUnit> OnDeathEvent;

        // ── State ──
        public int TeamId => DataReference?.TeamId ?? -1;

        public bool IsAllyOf(CombatUnit other)
        {
            return TeamId >= 0 && TeamId == other.TeamId;
        }

        private CombatUnitView _combatUnitGameObject = null;
        public CombatUnitView CombatUnitGameObject => _combatUnitGameObject;
        private CancellationTokenSource _deathToken;
        public CancellationTokenSource DeathToken => _deathToken;
        private CancellationTokenSource _interruptToken;
        public CancellationTokenSource InterruptToken => _interruptToken;

        public bool IsAlive => _deathToken != null && !_deathToken.IsCancellationRequested;

        // Time scale
        private TimeManager _timeManager;
        private TimeBundle _timeBundle;
        public TimeBundle TimeBundle => _timeBundle;


        public CombatUnit(CombatUnitReference combatUnitReference, ICombatSettings combatSettings, IUnitSOProvider unitSOProvider, TimeManager timeManager, UnitDefinitionSO data = null)
        {
            _dataReference = combatUnitReference;
            _data = data;

            _unit.Key = combatUnitReference?.Key;

            _combatSettings = combatSettings;
            _unitSOProvider = unitSOProvider;
            _timeManager = timeManager;
            _timeBundle = new TimeBundle(_timeManager);
        }

        public virtual void Initialize()
        {
            Health = new CombatUnitHealth(this);
            Mana = new CombatUnitMana(this, _combatSettings);
            Buffs = new CombatUnitBuffSystem(this);
            StatusEffects = new CombatUnitStatusSystem(this, _combatSettings);
            PowerLevel = new CombatUnitPowerLevel(this, _combatSettings);

            Shield = new CombatUnitShield(this);

            if (_dataReference != null && _data == null)
            {
                _data = _dataReference.GetUnitDefinition(_unitSOProvider);
            }

            EquipmentBonus = new AttributeSet();

            Level = _dataReference.Level > 0 ? _dataReference.Level : 1;

            ResetHealthAndMana();

            _unit.Initialize();
        }

        public virtual void ResetHealthAndMana()
        {
            Health.Reset();
            Mana.Reset();
        }

        public virtual float GetAttributeValue(AttributeDefinitionSO attribute, int bonusLevel = 0)
        {
            if (attribute == null) return 0;

            CombatAttributesDefinition combatData = CombatData;
            if (combatData == null) return 0;

            float baseValueSettings = _combatSettings.GetBaseValue(attribute);
            float baseValueChar = combatData.BaseAttributes.GetValue(attribute);

            float tierMultiplier = combatData.Tier != null ? combatData.Tier.CombatAttributeScalingMultiplier : 1f;
            float levelMultiplier = combatData.LevelScaling.GetStatMultiplier(attribute, Level + bonusLevel, tierMultiplier);

            float total = baseValueSettings + (baseValueChar * levelMultiplier);

            foreach (CombatAttributeDataEffectRuntime effect in Buffs.All)
                total *= effect.Data.GetMultiplier(attribute);

            foreach (CombatUnitShieldNode shieldNode in Shield.Shields)
                total *= shieldNode.Buff.Data.GetMultiplier(attribute);

            foreach (CombatAttributeDataEffectRuntime effect in Buffs.All)
                total += effect.Data.GetAdditive(attribute);

            foreach (CombatUnitShieldNode shieldNode in Shield.Shields)
                total += shieldNode.Buff.Data.GetAdditive(attribute);

            foreach (var feature in _unit.Features)
                if (feature is IAttributeModifier modifier)
                    total = modifier.ModifyAttribute(this, attribute, total);

            return total;
        }

        public CombatActionSO GetCombatAction(int actionType, ICombatManager manager, CombatUnit caster,
            CombatUnit primaryTarget)
        {
            return CombatData?.GetCombatAction(actionType, manager, caster, primaryTarget);
        }

        public void RegisterCombatUnitGameObject(CombatUnitView combatUnitGameObject)
        {
            _combatUnitGameObject = combatUnitGameObject;
            _combatUnitGameObject.RegisterCombatUnit(this);
        }

        // ── AI / View delegation ─────────────────────────────────────────

        public bool IsAIRunning => _combatUnitGameObject != null
            && _combatUnitGameObject.CombatUnitAI != null
            && _combatUnitGameObject.CombatUnitAI.IsRunning;

        public void StopAI(bool dead, bool pauseAnimation)
        {
            if (_combatUnitGameObject == null) return;
            var ai = _combatUnitGameObject.CombatUnitAI;
            if (ai != null && ai.IsSetup) ai.StopAI(dead);
            if (pauseAnimation) _combatUnitGameObject.AnimationTimeController?.Pause();
        }

        public void StartAI()
        {
            if (_combatUnitGameObject == null) return;
            var ai = _combatUnitGameObject.CombatUnitAI;
            if (ai != null && ai.IsSetup) ai.StartAI();
            _combatUnitGameObject.AnimationTimeController?.Resume();
        }

        public void SetSilenced(bool silence) => _combatUnitGameObject?.CombatUnitAI?.SetSilenced(silence);
        public void SetRooted(bool root) => _combatUnitGameObject?.CombatUnitAI?.NavMeshAgentController.Root(root);
        public void AddThreat(CombatUnit source, int threatAmount) => _combatUnitGameObject?.CombatUnitAI?.CombatUnitThreatTable.AddThreat(source, threatAmount);

        public void UnregisterCombatUnitGameObject()
        {
            if (_combatUnitGameObject != null)
            {
                _combatUnitGameObject.UnRegisterCombatUnit();
                _combatUnitGameObject = null;
            }
        }

        private async UniTaskVoid DisableCombatUnitGameObject(CombatUnitView combatUnitGameObject, float delay)
        {
            if (combatUnitGameObject != null)
            {
                try { await _timeManager.Global.DelayAsync(delay, combatUnitGameObject.destroyCancellationToken); }
                catch (OperationCanceledException) { }
                finally
                {
                    if (combatUnitGameObject != null && combatUnitGameObject.gameObject != null)
                        combatUnitGameObject.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>Clean up features and sub-systems. Call when unit is no longer needed (combat end, scene unload).</summary>
        public void Dispose()
        {
            _unit.Dispose();
            StatusEffects?.DisposeAll();
            Buffs?.DisposeAll();
            KillAI();
        }

        public void OnDeath()
        {
            DisableCombatUnitGameObject(_combatUnitGameObject, 0.7f).Forget();
            Dispose();
            OnDeathEvent?.Invoke(this);
        }

        public void SetupAI()
        {
            KillAI();
            _interruptToken = new();
            _deathToken = new();
        }

        public void InterruptAI()
        {
            _interruptToken?.Cancel();
            _interruptToken?.Dispose();
            _interruptToken = new();
        }

        public void KillAI()
        {
            _interruptToken?.Cancel();
            _interruptToken?.Dispose();
            _interruptToken = null;
            _deathToken?.Cancel();
            _deathToken?.Dispose();
            _deathToken = null;
            _timeBundle.Clear();
        }

        public void DestroyAllThenReleaseCombatUnitGameObject()
        {
            if (_combatUnitGameObject != null)
            {
                CharacterData?.Model?.DestroyAllThenRelease();
                _combatUnitGameObject = null;
            }
        }

        public float GetActionCooldownSeconds()
        {
            float speed = GetAttributeValue(Attributes.SPEED);
            return GetActionCooldownSeconds(_combatSettings, speed);
        }

        public static float GetActionCooldownSeconds(ICombatRules combatRules, float speed)
        {
            return combatRules.AttackSpeedBase / speed;
        }

        public AttributeSet GetAttributeSet(int bonusLevel)
        {
            var result = new AttributeSet();
            foreach (AttributeDefinitionSO attr in Attributes.All)
                result.SetValue(attr, GetAttributeValue(attr, bonusLevel));
            return result;
        }
    }
}
