using UnityEngine;

namespace CupkekGames.Combat
{
    /// <summary>The shape an area covers.</summary>
    public enum CombatAreaShape
    {
        /// <summary>Everything within <see cref="CombatArea.Radius"/> of the origin.</summary>
        Circle,
        /// <summary>A circle's slice of <see cref="CombatArea.Angle"/> degrees, centred on <see cref="CombatArea.Forward"/>.</summary>
        Arc,
        /// <summary>A strip <see cref="CombatArea.Width"/> wide and <see cref="CombatArea.Length"/> long, from the origin along <see cref="CombatArea.Forward"/>.</summary>
        Line,
    }

    /// <summary>
    /// A region an action hits, handed to <see cref="ICombatSpace.Collect"/>. The origin and
    /// the forward direction are world values (where the caster stands, where a shot
    /// landed); the sizes are combat units, which the space turns into its own measure.
    /// </summary>
    public readonly struct CombatArea
    {
        public readonly CombatAreaShape Shape;
        public readonly Vector3 Origin;
        public readonly Vector3 Forward;
        public readonly float Radius;
        public readonly float Angle;
        public readonly float Length;
        public readonly float Width;

        private CombatArea(CombatAreaShape shape, Vector3 origin, Vector3 forward, float radius, float angle, float length, float width)
        {
            Shape = shape;
            Origin = origin;
            Forward = forward;
            Radius = radius;
            Angle = angle;
            Length = length;
            Width = width;
        }

        public static CombatArea Circle(Vector3 origin, float radius)
            => new CombatArea(CombatAreaShape.Circle, origin, Vector3.forward, radius, 360f, 0f, 0f);

        public static CombatArea Arc(Vector3 origin, Vector3 forward, float radius, float angle)
            => new CombatArea(CombatAreaShape.Arc, origin, forward, radius, angle, 0f, 0f);

        public static CombatArea Line(Vector3 origin, Vector3 forward, float length, float width)
            => new CombatArea(CombatAreaShape.Line, origin, forward, 0f, 0f, length, width);
    }
}
