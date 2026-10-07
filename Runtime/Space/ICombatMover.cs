using System.Threading;
using CupkekGames.TimeSystem;
using PrimeTween;
using UnityEngine;

namespace CupkekGames.Combat
{
    /// <summary>
    /// How one unit moves in its fight's <see cref="ICombatSpace"/>: it walks after its
    /// target until it is within reach, holds still while it acts, and can be rooted or
    /// pushed. The mover also plays the unit's move animation.
    /// </summary>
    public interface ICombatMover
    {
        /// <summary>Starts moving on the unit's clock (its AI starts).</summary>
        void Start(TimeBundle timeBundle);

        /// <summary>Stops for good and lets go of the clock (its AI stops).</summary>
        void Stop();

        /// <summary>How close to its target it walks, in combat units (the chosen action's range).</summary>
        void SetReach(float range);

        /// <summary>Walks after <paramref name="target"/> until it is within reach.</summary>
        void Follow(CombatUnit target);

        /// <summary>Stands still while acting; the next <see cref="Follow"/> walks on.</summary>
        void Hold();

        /// <summary>Stops every movement at once (the unit fell); the clock stays bound.</summary>
        void Halt();

        /// <summary>A rooted unit cannot move.</summary>
        void SetRooted(bool rooted);

        /// <summary>
        /// Pushes the unit by <paramref name="offset"/> combat units (already turned the way
        /// the unit faces) over <paramref name="duration"/> seconds.
        /// </summary>
        void Dash(Vector3 offset, float duration, Ease ease, int avoidancePriority, CancellationToken cancellationToken);

        /// <summary>Standing still where it belongs: not walking, not pushed. A unit starts an action only when settled.</summary>
        bool IsSettled { get; }

        /// <summary>Picks the unit up off the field (a drag in the formation phase); it holds no place until <see cref="SetDown"/>.</summary>
        void Lift();

        /// <summary>Sets a lifted unit down at <paramref name="world"/>; the space throws if it cannot stand exactly there.</summary>
        void SetDown(Vector3 world);
    }
}
