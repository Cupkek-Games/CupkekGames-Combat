using System;
using System.Collections.Generic;
using CupkekGames.TimeSystem;
using UnityEngine;

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

        /// <summary>
        /// How many steps <paramref name="caster"/> still has to walk, around whoever is in the
        /// way, before <paramref name="target"/> is within <paramref name="range"/>: 0 when it
        /// already is, <see cref="int.MaxValue"/> when no way gets there now. A step is the
        /// space's own (one tile on a grid).
        /// </summary>
        int StepsToReach(CombatUnit caster, CombatUnit target, float range);

        /// <summary>Adds every unit inside <paramref name="area"/> to <paramref name="results"/> (cleared first), both sides, in the space's own order.</summary>
        void Collect(in CombatArea area, List<CombatUnit> results);

        /// <summary>
        /// Adds every unit within <paramref name="rings"/> combat units of <paramref name="center"/>
        /// to <paramref name="results"/> (cleared first): its own place and the rings around it,
        /// both sides, <paramref name="center"/> included, in the space's own order. Nothing when
        /// <paramref name="center"/> is not on the field.
        /// </summary>
        void CollectAround(CombatUnit center, int rings, List<CombatUnit> results);

        /// <summary>
        /// Adds every unit on the line from <paramref name="from"/> through
        /// <paramref name="through"/> and on past it, <paramref name="length"/> combat units long,
        /// to <paramref name="results"/> (cleared first): both sides, <paramref name="from"/> left
        /// out, in the space's own order. Nothing when either is not on the field.
        /// </summary>
        void CollectLine(CombatUnit from, CombatUnit through, int length, List<CombatUnit> results);

        /// <summary>
        /// Pushes <paramref name="unit"/> straight away from <paramref name="from"/> by up to
        /// <paramref name="tiles"/> combat units, stopping before the first place it cannot take
        /// (the edge, a wall, another unit); its body follows over <paramref name="duration"/>
        /// seconds. Returns how far it moved: 0 when either is not on the field. Checks no
        /// immunity: <see cref="CombatPush"/> does.
        /// </summary>
        int Push(CombatUnit unit, CombatUnit from, int tiles, float duration);

        /// <summary>
        /// Draws the warning for <paramref name="area"/> in <paramref name="color"/>: exactly the
        /// cells <see cref="Collect"/> takes for it, outlined, its fill empty until
        /// <see cref="CombatAreaMark.Fill"/>.
        /// </summary>
        CombatAreaMark ShowArea(in CombatArea area, Color color);

        /// <summary>A length in combat units as world metres (effects).</summary>
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
