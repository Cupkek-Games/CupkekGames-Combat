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

            // Expected-value crit factor: 1 + chance * (dmg - 1). The old
            // chance * dmg product was not an expectation — it shrank the score
            // for any unit with contract-scale data (0.15 * 1.5 = 0.225) and
            // rated crit-less units above identical units with crit attributes.
            float critMultiplier = 1f;
            if (attrs.CritChance != null && attrs.CritDmg != null)
            {
                float critChance = UnityEngine.Mathf.Clamp01(_owner.GetAttributeValue(attrs.CritChance, bonusLevel));
                float critDmg = _owner.GetAttributeValue(attrs.CritDmg, bonusLevel);
                if (critDmg > 1f)
                {
                    critMultiplier = 1f + critChance * (critDmg - 1f);
                }
            }

            return (int)(4 * baseAttack * speedMultiplier * manaMultiplier * critMultiplier);
        }

        // Effective HP: how much raw damage it takes to down the unit. Each
        // damage school multiplies incoming damage by GetDamageTakenMultiplier,
        // so the unit effectively has HP / thatMultiplier; schools average.
        // The old HP * (1 - multiplier) capped at raw HP and read 0 for an
        // undefended unit instead of its actual HP.
        public int GetDEF(int bonusLevel = 0)
        {
            var attrs = _owner.Attributes;
            float HP = _owner.GetAttributeValue(attrs.HP, bonusLevel);
            if (HP <= 0f) return 0;

            float effectiveHpSum = 0f;
            int schools = 0;
            if (attrs.DEF != null)
            {
                float DEF = _owner.GetAttributeValue(attrs.DEF, bonusLevel);
                float taken = _combatSettings.GetDamageTakenMultiplier(DEF);
                effectiveHpSum += HP / UnityEngine.Mathf.Max(0.05f, taken);
                schools++;
            }
            if (attrs.MDEF != null)
            {
                float MDEF = _owner.GetAttributeValue(attrs.MDEF, bonusLevel);
                float taken = _combatSettings.GetDamageTakenMultiplier(MDEF);
                effectiveHpSum += HP / UnityEngine.Mathf.Max(0.05f, taken);
                schools++;
            }

            float effectiveHp = schools > 0 ? effectiveHpSum / schools : HP;
            return (int)(effectiveHp / 4f);
        }
    }
}
