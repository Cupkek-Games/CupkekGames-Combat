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
        public float DefenseReduction;
        public DamageTypeDefinitionSO DamageType;
        public CombatActionSO ActionSO;
    }
}
