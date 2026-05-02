using System.Collections.Generic;
using UnityEngine;

namespace CupkekGames.Combat
{
    public interface IAutobattlerSettings
    {
        // Roster
        int MaxRosterSize { get; }

        // Mana
        int MaxMP { get; }
        int DefaultActionTypeId { get; }
        int FullManaActionTypeId { get; }
        IReadOnlyList<ActionManaEffect> ActionManaEffects { get; }
        int TakeDamageManaInterval { get; }

        // Threat
        float ThreatTableCheckInterval { get; }
        int DistanceThreat { get; }

        // AI Collider
        float AIColliderRadius { get; }
        float AIColliderHeight { get; }
        Vector3 AIColliderCenter { get; }
    }
}
