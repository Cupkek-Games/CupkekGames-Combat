using System.Collections.Generic;
using CupkekGames.BehaviourTrees;
using CupkekGames.Cameras;
using UnityEngine;

namespace CupkekGames.Combat
{
  public class CombatActionNodeCameraShake : BTNodeAction
  {
    [SerializeField] private CinemachineScreenShakeType _preset = CinemachineScreenShakeType.Critical;
    [SerializeField] private float _intensity = 0.2f;
    [SerializeField] private float _duration = 0.4f;

    protected override BTNodeRuntimeState OnUpdate(ref Dictionary<string, object> Blackboard, float deltaTime)
    {
      var ctx = CombatActionContext.From(Blackboard);

      ctx.CombatManager.CinemachineManager.ShakeCamera(_preset, _intensity, _duration);

      return BTNodeRuntimeState.Success;
    }

    protected override void OnReset()
    {

    }
  }
}