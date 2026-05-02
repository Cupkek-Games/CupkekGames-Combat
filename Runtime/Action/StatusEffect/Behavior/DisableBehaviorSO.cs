using UnityEngine;

namespace CupkekGames.Combat
{
  [CreateAssetMenu(fileName = "DisableBehavior", menuName = "CupkekGames/Combat/StatusEffect/Behavior/Disable")]
  public class DisableBehaviorSO : StatusEffectBehaviorSO
  {
    [SerializeField] private string _popupText = "DISABLED";
    [SerializeField] private bool _stun;
    [SerializeField] private bool _silence;
    [SerializeField] private bool _root;

    public override void OnStart(ICombatSettings combatSettings, ICombatManager manager,
      CombatUnit caster, CombatUnit target, int skillLevel)
    {
      if (_stun) target.StopAI(false, true);
      if (_silence) target.SetSilenced(true);
      if (_root) target.SetRooted(true);

      manager.StatusPopup.ShowStatusEffect(
        target.CombatUnitGameObject.HealthBarTransform.position, false, _popupText);
    }

    public override void OnEnd(ICombatSettings combatSettings, ICombatManager manager,
      CombatUnit caster, CombatUnit target, int skillLevel)
    {
      if (target == null || target.CombatUnitGameObject == null) return;

      if (_stun) target.StartAI();
      if (_silence) target.SetSilenced(false);
      if (_root) target.SetRooted(false);
    }
  }
}
