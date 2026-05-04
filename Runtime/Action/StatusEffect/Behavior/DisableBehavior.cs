using System;
using UnityEngine;
using CupkekGames.TextPopup;

namespace CupkekGames.Combat
{
    [Serializable]
    public class DisableBehavior : IStatusEffectBehaviorFeature
    {
        [SerializeField] private string _popupText = "DISABLED";
        [SerializeField] private bool _stun;
        [SerializeField] private bool _silence;
        [SerializeField] private bool _root;

        public void OnStart(ICombatSettings combatSettings, ICombatManager manager,
            CombatUnit caster, CombatUnit target, int skillLevel)
        {
            if (_stun) target.StopAI(false, true);
            if (_silence) target.SetSilenced(true);
            if (_root) target.SetRooted(true);

            manager.PopupManager.Show(
                PopupKinds.StatusNegative,
                target.CombatUnitGameObject.HealthBarTransform.position,
                0,
                new TextPopupContext { LeftText = _popupText });
        }

        public void OnTick(ICombatSettings combatSettings, ICombatManager manager,
            CombatUnit caster, CombatUnit target, int skillLevel) { }

        public void OnEnd(ICombatSettings combatSettings, ICombatManager manager,
            CombatUnit caster, CombatUnit target, int skillLevel)
        {
            if (target == null || target.CombatUnitGameObject == null) return;

            if (_stun) target.StartAI();
            if (_silence) target.SetSilenced(false);
            if (_root) target.SetRooted(false);
        }
    }
}
