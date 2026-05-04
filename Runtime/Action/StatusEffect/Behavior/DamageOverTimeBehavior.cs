using System;
using UnityEngine;
using CupkekGames.Audio;
using CupkekGames.TextPopup;

namespace CupkekGames.Combat
{
    [Serializable]
    public class DamageOverTimeBehavior : IStatusEffectBehaviorFeature
    {
        [SerializeField] private float _damagePercentagePerSkillLevel = 0.05f;
        [SerializeField] private SFXPlayerSO _sfxPlayer;

        public void OnStart(ICombatSettings combatSettings, ICombatManager manager,
            CombatUnit caster, CombatUnit target, int skillLevel) { }

        public void OnTick(ICombatSettings combatSettings, ICombatManager manager,
            CombatUnit caster, CombatUnit target, int skillLevel)
        {
            if (_sfxPlayer != null)
                _sfxPlayer.Play(target.CombatUnitGameObject.Center.transform);

            Vector3 targetPos = target.CombatUnitGameObject.HealthBarTransform.position;

            float maxHP = target.GetAttributeValue(target.Attributes.HP);
            int damage = (int)((maxHP * _damagePercentagePerSkillLevel * skillLevel) + 0.5f);

            target.Health.TakeDamage(damage, null);

            manager.PopupManager.Show(PopupKinds.Damage, targetPos, damage);
        }

        public void OnEnd(ICombatSettings combatSettings, ICombatManager manager,
            CombatUnit caster, CombatUnit target, int skillLevel) { }
    }
}
