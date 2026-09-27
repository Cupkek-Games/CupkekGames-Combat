using UnityEngine;
using CupkekGames.RPGStats;

namespace CupkekGames.Combat.Tests
{
    /// <summary>Holds an effect so the tests can author it through the serializer.</summary>
    internal sealed class EffectHost : ScriptableObject
    {
        public AttributeEffect Effect = new();
    }
}
