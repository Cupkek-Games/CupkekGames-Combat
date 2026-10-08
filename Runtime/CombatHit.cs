using System;

namespace CupkekGames.Combat
{
    /// <summary>
    /// One hit on a unit: who dealt it, from what, and how it landed. Every
    /// damage goes through <see cref="CombatUnitHealth.TakeDamage(in CombatHit)"/>,
    /// which records the landed hit as <see cref="CombatUnit.LastHit"/> and
    /// raises <see cref="CombatUnitHealth.OnHit"/> before the death it causes,
    /// so a death's handlers find the killer in the last hit.
    /// </summary>
    public readonly struct CombatHit
    {
        /// <summary>The unit that dealt it; null when nobody did (the environment, an applier that is gone).</summary>
        public CombatUnit Attacker { get; }

        public CombatSource Source { get; }

        /// <summary>The damage the hit carried, before shields.</summary>
        public int Damage { get; }

        public bool IsCrit { get; }

        /// <summary>The element's multiplier the damage was dealt with (1 when neutral).</summary>
        public float ElementMultiplier { get; }

        /// <summary>What the target lost, shields and health together. Set when the hit lands.</summary>
        public int Dealt { get; }

        /// <summary>This hit felled the target. Set when the hit lands.</summary>
        public bool Killed { get; }

        public CombatHit(CombatUnit attacker, CombatSource source, int damage, bool isCrit = false, float elementMultiplier = 1f)
        {
            Attacker = attacker;
            Source = source ?? throw new ArgumentNullException(nameof(source), "A hit needs its source.");
            Damage = Math.Max(0, damage);
            IsCrit = isCrit;
            ElementMultiplier = elementMultiplier;
            Dealt = 0;
            Killed = false;
        }

        private CombatHit(in CombatHit hit, int dealt, bool killed)
        {
            Attacker = hit.Attacker;
            Source = hit.Source;
            Damage = hit.Damage;
            IsCrit = hit.IsCrit;
            ElementMultiplier = hit.ElementMultiplier;
            Dealt = dealt;
            Killed = killed;
        }

        /// <summary>This hit as it landed.</summary>
        internal CombatHit Landed(int dealt, bool killed) => new CombatHit(this, dealt, killed);

        /// <summary>The hit felled its target with at least <paramref name="share"/> times what it had left.</summary>
        public bool IsOverkill(float share) => Killed && Damage >= share * Math.Max(1, Dealt);
    }
}
