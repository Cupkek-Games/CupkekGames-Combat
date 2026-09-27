using System;

namespace CupkekGames.Combat
{
    /// <summary>What a <see cref="CombatSource"/> is.</summary>
    public enum CombatSourceKind
    {
        /// <summary>A unit's action: its attack, its skill, its ultimate.</summary>
        Action,
        /// <summary>A status effect working on its wearer (damage over time).</summary>
        Status,
        /// <summary>A game rule answering a hit (an upgrade's proc).</summary>
        Proc,
        /// <summary>An item used in the fight (a thrown potion).</summary>
        Item,
        /// <summary>The field itself: nobody's doing.</summary>
        Environment,
    }

    /// <summary>
    /// Where a hit came from. An action's source is a fresh instance on every
    /// run of the action (<see cref="CombatActionRunner.Setup"/>), so "once per
    /// action" means "once per instance"; a projectile carries the source of
    /// the run that launched it to wherever it lands.
    /// </summary>
    public sealed class CombatSource
    {
        public CombatSourceKind Kind { get; }

        /// <summary>A free tag: the action's or the status's asset name, a proc's key, an item's key.</summary>
        public string Tag { get; }

        /// <summary>The unit behind it, when there is one: the caster, the applier, an item's stand-in.</summary>
        public CombatUnit Owner { get; }

        /// <summary>The action that ran, for Action and Item sources; null otherwise.</summary>
        public CombatActionSO Action { get; }

        public CombatSource(CombatSourceKind kind, string tag, CombatUnit owner = null, CombatActionSO action = null)
        {
            Kind = kind;
            Tag = tag ?? string.Empty;
            Owner = owner;
            Action = action;
        }

        /// <summary>A fresh source for one run of <paramref name="action"/> by <paramref name="caster"/>.</summary>
        public static CombatSource ForAction(CombatActionSO action, CombatUnit caster)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            return new CombatSource(CombatSourceKind.Action, action.name, caster, action);
        }

        /// <summary>A status effect's source for its whole life: its definition, put on by <paramref name="applier"/>.</summary>
        public static CombatSource ForStatus(StatusEffectSO status, CombatUnit applier)
            => new CombatSource(CombatSourceKind.Status, status != null ? status.name : string.Empty, applier);

        public override string ToString() => $"{Kind}:{Tag}";
    }
}
