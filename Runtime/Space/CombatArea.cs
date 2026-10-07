using UnityEngine;

namespace CupkekGames.Combat
{
    /// <summary>The shape an area covers.</summary>
    public enum CombatAreaShape
    {
        /// <summary>Everything within <see cref="CombatArea.Radius"/> of the origin.</summary>
        Circle,
        /// <summary>A circle's slice of <see cref="CombatArea.Angle"/> degrees, centred on the way to <see cref="CombatArea.Towards"/>.</summary>
        Arc,
        /// <summary>A strip <see cref="CombatArea.Width"/> wide and <see cref="CombatArea.Length"/> long, from the origin the way to <see cref="CombatArea.Towards"/>.</summary>
        Line,
    }

    /// <summary>
    /// A region an action hits, handed to <see cref="ICombatSpace.Collect"/>. The origin and
    /// the point it aims at are world values (where the caster stands, where its target
    /// stands, where a shot landed); the sizes are combat units, which the space turns into
    /// its own measure. Locked when a cast starts, an area hits where it was aimed however
    /// anyone moves after.
    /// </summary>
    public readonly struct CombatArea
    {
        public readonly CombatAreaShape Shape;
        public readonly Vector3 Origin;
        /// <summary>The point an arc or a line aims at (the origin for a circle).</summary>
        public readonly Vector3 Towards;
        public readonly float Radius;
        public readonly float Angle;
        public readonly float Length;
        public readonly float Width;

        private CombatArea(CombatAreaShape shape, Vector3 origin, Vector3 towards, float radius, float angle, float length, float width)
        {
            Shape = shape;
            Origin = origin;
            Towards = towards;
            Radius = radius;
            Angle = angle;
            Length = length;
            Width = width;
        }

        /// <summary>The flat way from the origin to <see cref="Towards"/>; zero for a circle.</summary>
        public Vector3 Forward
        {
            get
            {
                Vector3 way = Towards - Origin;
                way.y = 0f;
                return way.normalized;
            }
        }

        public static CombatArea Circle(Vector3 origin, float radius)
            => new CombatArea(CombatAreaShape.Circle, origin, origin, radius, 360f, 0f, 0f);

        public static CombatArea Arc(Vector3 origin, Vector3 towards, float radius, float angle)
            => new CombatArea(CombatAreaShape.Arc, origin, towards, radius, angle, 0f, 0f);

        public static CombatArea Line(Vector3 origin, Vector3 towards, float length, float width)
            => new CombatArea(CombatAreaShape.Line, origin, towards, 0f, 0f, length, width);
    }
}
