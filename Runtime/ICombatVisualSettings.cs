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

        // A hit's finer feel. Each defaults to how hits looked before, so a game opts in.
        /// <summary>A flash of this colour laid on first, for <see cref="HitPopDurationMS"/> (a bright frame).</summary>
        Color HitPopColor => Color.white;
        /// <summary>The flash's colour on the emission.</summary>
        Color HitPopColorEmission => Color.white;
        /// <summary>The flash's weight against the hit colour's; 0: no flash.</summary>
        int HitPopWeight => 0;
        int HitPopDurationMS => 0;
        /// <summary>After <see cref="HitColorDurationMS"/>, the hit colour eases out over this long; 0: it drops at once.</summary>
        int HitColorFadeMS => 0;
        /// <summary>The squash's whole length in seconds: squash, stretch and settle.</summary>
        float HitSquashSeconds => 0.3f;
        /// <summary>How far a hit pushes the body away from its attacker, in world units; 0: no nudge.</summary>
        float HitNudgeDistance => 0f;
        /// <summary>The nudge's whole length in seconds: out and back.</summary>
        float HitNudgeSeconds => 0.2f;

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
