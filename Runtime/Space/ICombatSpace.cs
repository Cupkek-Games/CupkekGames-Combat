using System;
using System.Collections.Generic;
using CupkekGames.TimeSystem;

namespace CupkekGames.Combat
{
    /// <summary>
    /// Where a fight happens: how far apart units are, who an area covers, and how units
    /// move. Combat never measures or moves anything itself; it asks the fight's space.
    /// Distances and sizes are combat units (one body, or one tile), which each space
    /// turns into its own measure: metres on a navmesh, tiles on a grid. A game provides
    /// one per fight through <see cref="ICombatUnitManager.Space"/>.
    /// </summary>
    public interface ICombatSpace
    {
        /// <summary>The distance between two units, in combat units.</summary>
        float Distance(CombatUnit a, CombatUnit b);

        /// <summary>Whether <paramref name="target"/> is within <paramref name="range"/> combat units of <paramref name="caster"/>, with the space's own tolerance.</summary>
        bool InRange(CombatUnit caster, CombatUnit target, float range);

        /// <summary>Adds every unit inside <paramref name="area"/> to <paramref name="results"/> (cleared first), both sides, in the space's own order.</summary>
        void Collect(in CombatArea area, List<CombatUnit> results);

        /// <summary>A length in combat units as world metres (indicators, effects).</summary>
        float ToWorld(float units);

        /// <summary>The mover for one unit, made once when its AI is set up.</summary>
        ICombatMover CreateMover(CombatUnitView view);

        /// <summary>
        /// Steps a unit on its time: calls <paramref name="step"/> with the seconds each step
        /// covers, every frame on a navmesh, every fixed tick on a grid (units in turn, so a
        /// fight plays out the same at any frame rate). A paused <paramref name="time"/> takes
        /// no steps. Dispose the result to stop.
        /// </summary>
        IDisposable Drive(TimeContext time, Action<float> step);
    }
}
