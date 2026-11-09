using System;
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
        private SceneSpotlight _spotlight;
        private Queue<CombatUnit> _combatUnits = new();
        // The freeze is two layers this manager owns: one on global time, one on every unit but the
        // caster. Other holders' layers (a potion's aim, a staged take) stay untouched.
        private IDisposable _globalFreeze;
        private IDisposable _unitFreeze;

        /// <summary>
        /// An ultimate's freeze is on: global time stands still for everyone but
        /// its caster, from the moment it is selected (its queue turn) to its end.
        /// A unit that falls during the freeze leaves it at once.
        /// </summary>
        public bool EffectsActive => _unitFreeze != null;

        public bool HasNext => _combatUnits.Count > 0;

        public CombatUltimateManager(ICombatSettings combatSettings, TimeManager timeManager,
            ICombatUnitManager combatUnitManager, SceneSpotlight spotlight)
        {
            _combatSettings = combatSettings;
            _timeManager = timeManager;
            _combatUnitManager = combatUnitManager;
            _spotlight = spotlight;
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
            if (!EffectsActive) return;

            ReleaseFreeze();
            _spotlight?.ClearSpotlight();
        }

        private void ReleaseFreeze()
        {
            _globalFreeze?.Dispose();
            _globalFreeze = null;
            _unitFreeze?.Dispose();
            _unitFreeze = null;
        }

        private void ActivateEffects(CombatUnit combatUnit)
        {
            // A caster next in the queue takes the freeze over: the last one's layers go first, so
            // the new caster runs and the last one freezes with the rest.
            ReleaseFreeze();
            _globalFreeze = _timeManager.Global.Scale(this, 0f);
            _unitFreeze = _combatUnitManager.HoldUnits(this,
                _combatUnitManager.CombatUnitsAlly.Concat(_combatUnitManager.CombatUnitsEnemy).Where(unit => unit != combatUnit), 0f);

            // Everything darkens but the caster and the effects. A caster next in the queue takes
            // the spotlight over from the last one; its renderers are read now (an accessory
            // switched on since its body woke counts).
            if (combatUnit.View != null && _spotlight != null)
            {
                combatUnit.View.RefreshRendererCache();
                _spotlight.Spotlight(combatUnit.View.Renderers);
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