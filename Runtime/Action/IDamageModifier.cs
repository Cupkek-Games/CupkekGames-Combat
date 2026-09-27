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
        /// <summary>The action the hit comes from (the source's action); null for a hit from no action.</summary>
        public CombatActionSO ActionSO;
        /// <summary>Where the hit comes from.</summary>
        public CombatSource Source;
    }
}
