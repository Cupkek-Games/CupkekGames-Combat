using System;
using UnityEngine;
using CupkekGames.Audio;
using CupkekGames.TextPopup;

namespace CupkekGames.Combat
{
    /// <summary>
    /// A share of the wearer's max health per effect level, every tick. The hit
    /// names the effect's applier and source, so a death by it has a killer.
    /// </summary>
    [Serializable]
    public class DamageOverTimeBehavior : IStatusEffectBehaviorFeature
    {
        [SerializeField] private float _damagePercentagePerSkillLevel = 0.05f;
        [SerializeField] private SFXPlayerSO _sfxPlayer;
        [Tooltip("The popup kind its ticks show as (a bleed's, a poison's).")]
        [SerializeField] private string _popupKind = PopupKinds.Damage;

        public void OnStart(ICombatSettings combatSettings, ICombatManager manager, StatusEffect effect, CombatUnit wearer) { }

        public void OnTick(ICombatSettings combatSettings, ICombatManager manager, StatusEffect effect, CombatUnit wearer)
        {
            if (_sfxPlayer != null)
                _sfxPlayer.Play(wearer.View.Center.transform);

            Vector3 targetPos = wearer.View.HealthBarTransform.position;

            float maxHP = wearer.GetAttributeValue(wearer.Attributes.HP);
            int damage = (int)((maxHP * _damagePercentagePerSkillLevel * effect.Level) + 0.5f);

            wearer.Health.TakeDamage(new CombatHit(effect.Applier, effect.Source, damage));

            manager.PopupManager.Show(_popupKind, targetPos, damage, CombatDamageCalculator.PopupContext(combatSettings, wearer, null));
        }

        public void OnEnd(ICombatSettings combatSettings, ICombatManager manager, StatusEffect effect, CombatUnit wearer) { }
    }
}
