using CupkekGames.RPGStats;

namespace CupkekGames.Combat
{
    public interface IDamageModifier
    {
        int Order { get; }
        float ModifyRawDamage(float damage, CombatUnit attacker, CombatUnit target, DamageContext ctx);
        int ModifyFinalDamage(int damage, CombatUnit attacker, CombatUnit target, DamageContext ctx);
    }

    public struct DamageContext
    {
        public bool IsCrit;
        public float ElementMultiplier;
        /// <summary>Fraction of damage that gets through the target's defense (final damage = attack * this).</summary>
        public float DamageTakenMultiplier;
        public DamageTypeDefinitionSO DamageType;
        public CombatActionSO ActionSO;
    }
}
