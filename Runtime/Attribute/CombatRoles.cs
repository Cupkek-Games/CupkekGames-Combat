namespace CupkekGames.Combat
{
    /// <summary>
    /// Reserved combat-role string constants. Used as keys in <see cref="CombatAttributeRegistrySO"/>
    /// to map roles → game-specific <see cref="CupkekGames.RPGStats.AttributeDefinitionSO"/>.
    /// Domain games can add their own roles (e.g. "Stamina") by registering new entries on the
    /// registry; framework code uses these constants for the standard combat slots.
    /// </summary>
    public static class CombatRoles
    {
        /// <summary>Health pool. Required.</summary>
        public const string HP = "HP";
        /// <summary>Mana / resource pool. Optional.</summary>
        public const string MP = "MP";
        /// <summary>Physical attack power. Optional.</summary>
        public const string ATK = "ATK";
        /// <summary>Magical attack power. Optional.</summary>
        public const string MATK = "MATK";
        /// <summary>Physical defense. Optional.</summary>
        public const string DEF = "DEF";
        /// <summary>Magical defense. Optional.</summary>
        public const string MDEF = "MDEF";
        /// <summary>Turn / action speed. Required.</summary>
        public const string SPEED = "SPEED";
        /// <summary>Critical-hit roll chance (0–1). Optional.</summary>
        public const string CritChance = "CritChance";
        /// <summary>Critical-hit damage multiplier. Optional.</summary>
        public const string CritDmg = "CritDmg";
    }
}
