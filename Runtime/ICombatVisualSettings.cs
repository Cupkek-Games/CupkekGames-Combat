using UnityEngine;
using CupkekGames.RPGStats;

namespace CupkekGames.Combat
{
    public interface ICombatVisualSettings
    {
        // Hit Effects
        Color HitColor { get; }
        Color HitColorEmission { get; }
        int HitColorWeight { get; }
        int HitColorDurationMS { get; }
        float HitSquashAndStretchBumpAmount { get; }
        float CritCameraShakeIntensity { get; }

        /// <summary>A hit's popup colour for its <paramref name="element"/> (null: a hit with no element). Colour says the element and nothing else.</summary>
        Color ElementColor(ElementTypeDefinitionSO element);

        // Outline
        float HoverOutlineWidth { get; }
        float HoverOutlineFadeInDuration { get; }
        float HoverOutlineFadeOutDuration { get; }

        // UI
        int BossBarMinTier { get; }

        // World Space UI Scaling
        float WorldSpaceMinDistance { get; }
        float WorldSpaceMaxDistance { get; }
        float WorldSpaceMinScale { get; }
        float WorldSpaceMaxScale { get; }
        float HealthBarGapPerHealth { get; }
    }
}
