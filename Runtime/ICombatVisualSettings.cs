using CupkekGames.ShapeDrawing;
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

        // Outline
        float HoverOutlineWidth { get; }
        float HoverOutlineFadeInDuration { get; }
        float HoverOutlineFadeOutDuration { get; }

        // UI
        int BossBarMinTier { get; }
        IndicatorSettings IndicatorSettings { get; }

        // World Space UI Scaling
        float WorldSpaceMinDistance { get; }
        float WorldSpaceMaxDistance { get; }
        float WorldSpaceMinScale { get; }
        float WorldSpaceMaxScale { get; }
        float HealthBarGapPerHealth { get; }
    }
}
