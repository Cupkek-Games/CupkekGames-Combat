using UnityEngine;

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

        /// <summary>A killing blow's popup reads as an overkill when its damage is at least this many times what the target had left.</summary>
        float OverkillShare { get; }

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
