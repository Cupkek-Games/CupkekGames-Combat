namespace CupkekGames.Combat
{
    public class CombatUnitPowerLevel : IPowerLevelCalculator
    {
        private readonly CombatUnit _owner;
        private readonly ICombatSettings _combatSettings;

        public CombatUnitPowerLevel(CombatUnit owner, ICombatSettings combatSettings)
        {
            _owner = owner;
            _combatSettings = combatSettings;
        }

        public bool TryCritical()
        {
            if (_owner.Attributes.CritChance == null) return false;
            float critChance = _owner.GetAttributeValue(_owner.Attributes.CritChance);
            return UnityEngine.Random.Range(0f, 1f) <= critChance;
        }

        public float ApplyCritical(float damage)
        {
            if (_owner.Attributes.CritDmg == null) return damage;
            float critDamage = _owner.GetAttributeValue(_owner.Attributes.CritDmg);
            return damage * critDamage;
        }

        public int GetATK(int bonusLevel = 0)
        {
            var attrs = _owner.Attributes;
            float ATK = attrs.ATK != null ? _owner.GetAttributeValue(attrs.ATK, bonusLevel) : 0f;
            float MATK = attrs.MATK != null ? _owner.GetAttributeValue(attrs.MATK, bonusLevel) : 0f;
            float SPEED = _owner.GetAttributeValue(attrs.SPEED, bonusLevel);

            float baseAttack = ATK + MATK;
            if (baseAttack == 0f) return 0;

            float speedMultiplier = 1f / CombatUnit.GetActionCooldownSeconds(_combatSettings, SPEED);

            float manaMultiplier = 1f;
            if (attrs.MP != null)
            {
                float MP = _owner.GetAttributeValue(attrs.MP, bonusLevel);
                manaMultiplier = 1 + (MP / _combatSettings.MaxMP);
            }

            float critMultiplier = 1f;
            if (attrs.CritChance != null && attrs.CritDmg != null)
            {
                float CRIT_CHANCE = _owner.GetAttributeValue(attrs.CritChance, bonusLevel);
                float CRIT_DMG = _owner.GetAttributeValue(attrs.CritDmg, bonusLevel);
                critMultiplier = CRIT_CHANCE * CRIT_DMG;
            }

            return (int)(4 * baseAttack * speedMultiplier * manaMultiplier * critMultiplier);
        }

        public int GetDEF(int bonusLevel = 0)
        {
            var attrs = _owner.Attributes;
            float HP = _owner.GetAttributeValue(attrs.HP, bonusLevel);

            float defTotal = 0f;
            if (attrs.DEF != null)
            {
                float DEF = _owner.GetAttributeValue(attrs.DEF, bonusLevel);
                float defMultiplier = 1 - _combatSettings.GetDefenseReduction(DEF);
                defTotal += HP * defMultiplier;
            }
            if (attrs.MDEF != null)
            {
                float MDEF = _owner.GetAttributeValue(attrs.MDEF, bonusLevel);
                float mdefMultiplier = 1 - _combatSettings.GetDefenseReduction(MDEF);
                defTotal += HP * mdefMultiplier;
            }

            if (defTotal == 0f) defTotal = HP;

            return (int)(defTotal / 4f);
        }
    }
}
