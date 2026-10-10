using System;

namespace CupkekGames.Combat
{
    /// <summary>What can take a unit's control away.</summary>
    [Flags]
    public enum CombatControl
    {
        None = 0,
        Stun = 1,
        Silence = 2,
        Root = 4,
        Push = 8,
    }

    /// <summary>
    /// Implemented by <see cref="CupkekGames.Units.IUnitFeature"/> instances that make their
    /// unit shrug off controls for now: a status that stuns, silences or roots
    /// (<see cref="StatusEffectSO.Controls"/>) is refused whole when any of its controls is
    /// shrugged off, and a push (<see cref="CombatPush"/>) does not move it.
    /// </summary>
    public interface IControlImmunity
    {
        CombatControl GetImmuneControls(CombatUnit unit);
    }
}
