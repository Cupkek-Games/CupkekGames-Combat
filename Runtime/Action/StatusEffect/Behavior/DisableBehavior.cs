using System;
using UnityEngine;
using CupkekGames.TextPopup;

namespace CupkekGames.Combat
{
    /// <summary>
    /// Stun, silence and root, counted on the wearer (<see cref="CombatUnit.Stun"/>): overlapping
    /// ones hold until the last ends. A stun or a silence drops an ultimate the wearer has
    /// selected but not started (<see cref="CombatUnitAI.CancelSelectedUltimate"/>); a stun also
    /// stops one already casting.
    /// </summary>
    [Serializable]
    public class DisableBehavior : IStatusEffectBehaviorFeature
    {
        [SerializeField] private string _popupText = "DISABLED";
        [SerializeField] private bool _stun;
        [SerializeField] private bool _silence;
        [SerializeField] private bool _root;

        public CombatControl Controls =>
            (_stun ? CombatControl.Stun : CombatControl.None)
            | (_silence ? CombatControl.Silence : CombatControl.None)
            | (_root ? CombatControl.Root : CombatControl.None);

        public void OnStart(ICombatSettings combatSettings, ICombatManager manager, StatusEffect effect, CombatUnit wearer)
        {
            if (_stun) wearer.Stun();
            if (_silence) wearer.Silence();
            if (_root) wearer.Root();

            if (wearer.View == null) return;

            manager.PopupManager.Show(
                PopupKinds.StatusNegative,
                wearer.View.HealthBarTransform.position,
                0,
                new TextPopupContext { LeftText = _popupText });
        }

        public void OnTick(ICombatSettings combatSettings, ICombatManager manager, StatusEffect effect, CombatUnit wearer) { }

        public void OnEnd(ICombatSettings combatSettings, ICombatManager manager, StatusEffect effect, CombatUnit wearer)
        {
            if (_stun) wearer.Unstun();
            if (_silence) wearer.Unsilence();
            if (_root) wearer.Unroot();
        }
    }
}
