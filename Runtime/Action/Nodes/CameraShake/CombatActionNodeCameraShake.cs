using CupkekGames.BehaviourTrees;
using CupkekGames.Cameras;
using CupkekGames.Graphs;
using UnityEngine;

namespace CupkekGames.Combat
{
  public class CombatActionNodeCameraShake : BTNodeAction
  {
    [SerializeField] private string _kind = CinemachineScreenShakeKinds.Critical;
    [SerializeField] private float _intensity = 0.2f;
    [SerializeField] private float _duration = 0.4f;

    protected override BTNodeRuntimeState OnUpdate(GraphFrame frame, float deltaTime)
    {
      var ctx = CombatActionContext.From(frame);

      ctx.CombatManager.CinemachineManager.ShakeCamera(_kind, _intensity, _duration);

      return BTNodeRuntimeState.Success;
    }

    protected override void OnReset()
    {

    }
  }
}
