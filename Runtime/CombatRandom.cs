namespace CupkekGames.Combat
{
    /// <summary>
    /// A fight's own random numbers (crits, ties between equally good actions), seeded per
    /// fight, so the same fight from the same inputs rolls the same way. Never
    /// <c>UnityEngine.Random</c> inside a fight.
    /// </summary>
    public sealed class CombatRandom
    {
        private readonly System.Random _random;

        public CombatRandom(int seed)
        {
            Seed = seed;
            _random = new System.Random(seed);
        }

        public int Seed { get; }

        /// <summary>A number from 0 (inclusive) to 1 (exclusive).</summary>
        public float Value() => (float)_random.NextDouble();

        /// <summary>A whole number from <paramref name="minInclusive"/> to <paramref name="maxExclusive"/>.</summary>
        public int Range(int minInclusive, int maxExclusive) => _random.Next(minInclusive, maxExclusive);
    }
}
