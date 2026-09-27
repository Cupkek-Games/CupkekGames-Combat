using System;
using UnityEngine;
using CupkekGames.TextPopup;

namespace CupkekGames.Combat
{
    /// <summary>
    /// Stun, silence and root. A stun or a silence drops an ultimate the wearer
    /// has selected but not started (<see cref="CombatUnitAI.CancelSelectedUltimate"/>);
    /// a stun also stops one already casting.
    /// </summary>
    [Serializable]
    public class DisableBehavior : IStatusEffectBehaviorFeature
    {
        [SerializeField] private string _popupText = "DISABLED";
        [SerializeField] private bool _stun;
        [SerializeField] private bool _silence;
        [SerializeField] private bool _root;

        public void OnStart(ICombatSettings combatSettings, ICombatManager manager, StatusEffect effect, CombatUnit wearer)
        {
            if (_stun) wearer.StopAI(false, true);
            if (_silence) wearer.SetSilenced(true);
            if (_root) wearer.SetRooted(true);

            manager.PopupManager.Show(
                PopupKinds.StatusNegative,
                wearer.View.HealthBarTransform.position,
                0,
                new TextPopupContext { LeftText = _popupText });
        }

        public void OnTick(ICombatSettings combatSettings, ICombatManager manager, StatusEffect effect, CombatUnit wearer) { }

        public void OnEnd(ICombatSettings combatSettings, ICombatManager manager, StatusEffect effect, CombatUnit wearer)
        {
            if (wearer == null || wearer.View == null) return;

            if (_stun) wearer.StartAI();
            if (_silence) wearer.SetSilenced(false);
            if (_root) wearer.SetRooted(false);
        }
    }
}
