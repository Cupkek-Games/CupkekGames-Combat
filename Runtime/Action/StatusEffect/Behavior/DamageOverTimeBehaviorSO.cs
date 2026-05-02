using UnityEngine;
using CupkekGames.Audio;

namespace CupkekGames.Combat
{
    [CreateAssetMenu(fileName = "DamageOverTimeBehavior", menuName = "CupkekGames/Combat/StatusEffect/Behavior/DamageOverTime")]
    public class DamageOverTimeBehaviorSO : StatusEffectBehaviorSO
    {
        [SerializeField] private float _damagePercentagePerSkillLevel = 0.05f;
        [SerializeField] private ScriptableObject _sfxPlayerAsset;

        public override void OnTick(ICombatSettings combatSettings, ICombatManager manager,
          CombatUnit caster, CombatUnit target, int skillLevel)
        {
            var sfxPlayer = _sfxPlayerAsset as ISFXPlayer;
            sfxPlayer?.Play(target.CombatUnitGameObject.Center.transform);

            Vector3 targetPos = target.CombatUnitGameObject.HealthBarTransform.position;

            float maxHP = target.GetAttributeValue(target.Attributes.HP);
            int damage = (int)((maxHP * _damagePercentagePerSkillLevel * skillLevel) + 0.5f);

            target.Health.TakeDamage(damage, null);

            manager.DamagePopup.ShowDamage(targetPos, damage, 1f, false);
        }
    }
}
