using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System;
using CupkekGames.BehaviourTrees;
using CupkekGames.Graphs;
using CupkekGames.TimeSystem;
using System.Linq;

namespace CupkekGames.Combat
{
    public class CombatActionRunner
    {
        private BehaviourTreeRunner _runner;
        public BehaviourTreeRunner Runner => _runner;
        private CombatUnit _caster;
        private CombatActionSO _actionSO;
        private CombatActionContext _context;
        public BTNodeRuntimeState State => _runner.RuntimeClone.State;
        private bool _firstTargetsCalculated = false;
        public event Action OnComplete;

        public CombatActionRunner(CombatUnit caster, CombatActionSO actionSO)
        {
            _caster = caster;
            _actionSO = actionSO;

            _runner = new BehaviourTreeRunner(actionSO);
            if (_caster == null)
            {
                Debug.LogError("CombatActionRunner caster is null");
                return;
            }

            if (_caster.CombatUnitGameObject == null)
            {
                Debug.LogError("CombatActionRunner caster.CombatUnitGameObject is null");
                return;
            }

            _runner.Prewarm(_caster.CombatUnitGameObject.gameObject);
        }

        public void Setup(
            CombatUnit caster,
            CombatUnit primaryTarget,
            int skillLevel,
            ICombatSettings combatSettings,
            ICombatManager combatManager)
        {
            _caster = caster;

            _runner.ResetTree();

            var bb = _runner.Blackboard;
            bb["CombatActionSO"] = _actionSO;
            bb["CombatSettings"] = combatSettings;
            bb["CombatManager"] = combatManager;
            bb["Caster"] = _caster;
            bb["PrimaryTarget"] = primaryTarget;
            bb["SkillLevel"] = skillLevel;

            if (combatManager?.CancelToken != null)
            {
                bb["CancellationToken"] = combatManager.CancelToken.Token;
            }

            bb["CancellationTokenCasterDeath"] = _caster.DeathToken.Token;
            bb["CancellationTokenCasterInterrupt"] = _caster.InterruptToken.Token;

            _context?.Dispose();
            _context = new CombatActionContext(_runner.RootFrame);
            bb["Context"] = _context;

            _firstTargetsCalculated = false;
        }

        public void Dispose()
        {
            _context?.Dispose();
            _context = null;
            _runner.Dispose();
            _firstTargetsCalculated = false;
        }

        public void ResetTree()
        {
            _runner.ResetTree();
            _firstTargetsCalculated = false;
        }

        public void SetTargetList(List<CombatUnit> targets)
        {
            // Initial seed lives in globals so every branch sees it until a
            // scoping decorator (TargetSelection / TargetUpdate) shadows it.
            _runner.Blackboard["TargetList"] = targets;
        }

        public List<CombatUnit> GetTargets(ICombatUnitManager combatUnitManager, CombatUnit primaryTarget, bool debug)
        {
            return _actionSO.GetTargets(combatUnitManager, _caster, primaryTarget, debug);
        }

        public BTNodeRuntimeState UpdateTree(ICombatUnitManager combatUnitManager, CombatUnit primaryTarget,
            float deltaTime, bool debug)
        {
            if (!_firstTargetsCalculated)
            {
                List<CombatUnit> targets = GetTargets(combatUnitManager, primaryTarget, debug);
                if (targets.Count > 0)
                {
                    SetTargetList(targets);
                    _firstTargetsCalculated = true;
                    if (debug)
                    {
                        Debug.Log(
                            $"Targets found for {_actionSO.name}: {string.Join(", ", targets.Select(t => t.DataReference.Key))}");
                    }
                }
                else
                {
                    if (debug)
                    {
                        Debug.Log($"No targets found for {_actionSO.name}");
                    }

                    return BTNodeRuntimeState.Fail;
                }
            }

            BTNodeRuntimeState state = _runner.UpdateTree(deltaTime);

            if (state == BTNodeRuntimeState.Success || state == BTNodeRuntimeState.Fail)
            {
                _firstTargetsCalculated = false;
            }

            return state;
        }

        public void RunOnceUntilComplete(ICombatUnitManager combatUnitManager, CombatUnit primaryTarget, bool debug)
        {
            _caster.CombatUnitGameObject.StartCoroutine(RunOnceUntilCompleteRoutine(combatUnitManager, primaryTarget,
                debug));
        }

        private IEnumerator RunOnceUntilCompleteRoutine(ICombatUnitManager combatUnitManager, CombatUnit primaryTarget,
            bool debug)
        {
            TimeContext timeContext = _caster.TimeBundle.TimeContext;
            while (true)
            {
                BTNodeRuntimeState state = UpdateTree(combatUnitManager, primaryTarget, timeContext.DeltaTime, debug);

                if (state == BTNodeRuntimeState.Success || state == BTNodeRuntimeState.Fail)
                {
                    break;
                }

                yield return null;
            }

            OnComplete?.Invoke();

            Dispose();
        }
    }
}
