using System.Collections.Generic;
using System.Linq;
using CupkekGames.TimeSystem;
using UnityEngine;

using CupkekGames.VFX;

namespace CupkekGames.Combat
{
    public class CombatUltimateManager
    {
        private ICombatSettings _combatSettings;
        private TimeManager _timeManager;
        private ICombatUnitManager _combatUnitManager;
        private RenderFeatureManager _renderFeatureManager;
        private Queue<CombatUnit> _combatUnits = new();
        private bool _effectsActive = false;
        public bool HasNext => _combatUnits.Count > 0;

        public CombatUltimateManager(ICombatSettings combatSettings, TimeManager timeManager,
            ICombatUnitManager combatUnitManager, RenderFeatureManager renderFeatureManager)
        {
            _combatSettings = combatSettings;
            _timeManager = timeManager;
            _combatUnitManager = combatUnitManager;
            _renderFeatureManager = renderFeatureManager;
        }

        public void Enqueue(CombatUnit combatUnit)
        {
            if (combatUnit != null)
            {
                _combatUnits.Enqueue(combatUnit);

                if (_combatUnits.Count == 1)
                {
                    OnCastingStart(combatUnit);
                }

                combatUnit.OnDeathEvent += RemoveFromQueue;
            }
        }

        public CombatUnit Dequeue()
        {
            if (_combatUnits.Count == 0)
            {
                return null;
            }

            CombatUnit finishedUnit = _combatUnits.Dequeue();

            // Check if there's a next unit that should start casting
            if (_combatUnits.Count > 0)
            {
                CombatUnit nextUnit = _combatUnits.Peek();
                OnCastingStart(nextUnit);
            }
            else
            {
                OnCastingEnd(finishedUnit);
            }

            finishedUnit.OnDeathEvent -= RemoveFromQueue;

            return finishedUnit;
        }

        public CombatUnit Peek()
        {
            return _combatUnits.Count > 0 ? _combatUnits.Peek() : null;
        }

        public void StopEffects()
        {
            if (_effectsActive)
            {
                _effectsActive = false;
                _timeManager.Global.TimeScale = 1f;
                _combatUnitManager.SetTimeScale(1f, null);

                _renderFeatureManager?.UnDarkenEverythingAsync(true);
            }
        }

        private void ActivateEffects(CombatUnit combatUnit)
        {
            _effectsActive = true;
            combatUnit.TimeBundle.TimeContext.TimeScale = 1f;

            _timeManager.Global.TimeScale = 0f;
            _combatUnitManager.SetTimeScale(0f, combatUnit);

            if (combatUnit.CombatUnitGameObject != null)
            {
                _renderFeatureManager?.DarkenEverythingExceptAsync(combatUnit.CombatUnitGameObject.Renderers, true);
            }
        }

        private bool ShouldApplyUltimateEffects(CombatUnit combatUnit)
        {
            return _combatUnitManager.CombatUnitsAlly.Contains(combatUnit) || (combatUnit.CombatData?.Tier != null && combatUnit.CombatData.Tier.SortOrder >= _combatSettings.BossBarMinTier);
        }

        private void OnCastingStart(CombatUnit combatUnit)
        {
            if (!ShouldApplyUltimateEffects(combatUnit))
            {
                StopEffects();
                // For units that don't apply effects, don't activate visual effects
                // The AI will handle them normally through the regular Dequeue process
                return;
            }


            ActivateEffects(combatUnit);
        }

        private void OnCastingEnd(CombatUnit combatUnit)
        {
            if (!ShouldApplyUltimateEffects(combatUnit))
            {
                return;
            }

            StopEffects();
        }

        public void RemoveFromQueue(CombatUnit combatUnit)
        {
            if (combatUnit == null)
            {
                return;
            }

            combatUnit.OnDeathEvent -= RemoveFromQueue;

            // Quick check if unit is even in the queue
            if (!_combatUnits.Contains(combatUnit))
            {
                return;
            }

            // Check if the unit was the first in queue (currently casting)
            bool wasFirstInQueue = _combatUnits.Count > 0 && _combatUnits.Peek() == combatUnit;

            // Efficiently rebuild queue without the target unit
            _combatUnits = new Queue<CombatUnit>(_combatUnits.Where(unit => unit != combatUnit));

            // Only proceed with next steps if the removed unit was first in queue
            if (wasFirstInQueue)
            {
                Debug.Log($"OnDeath: {combatUnit.DataReference.Key} was FIRST IN QUEUE");

                StopEffects();

                // Start next unit if available
                if (_combatUnits.Count > 0)
                {
                    CombatUnit nextUnit = _combatUnits.Peek();
                    OnCastingStart(nextUnit);
                }
            }
        }
    }
}