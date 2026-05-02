using System;
using CupkekGames.Data;
using CupkekGames.Luna;
using UnityEngine;
using CupkekGames.RPGStats;
using CupkekGames.Data.Primitives;

namespace CupkekGames.Combat
{
    /// <summary>
    /// Visual wrapper for AttributeEffect with display metadata.
    /// Used for gatherables, skills, and other sources that need to show buffs/debuffs.
    /// </summary>
    [Serializable]
    public class CombatAttributeDataEffectVisual
    {
        public SerializedGuid ID = new SerializedGuid(Guid.NewGuid());

        [Tooltip("Display name for this effect")]
        public string DisplayName;

        [Tooltip("Combat attribute effects")] public AttributeEffect AttributeEffect = new AttributeEffect();

        [Tooltip("Duration in seconds (0 = permanent for scout duration, -1 = infinite)")]
        public float Duration = 0f;

        [Tooltip("Icon for this effect (optional)")]
        public Sprite Icon;

        [Tooltip("Color for UI display")] public UIColor Color = new UIColor("amber", UIColorValue.V_400);

        [Tooltip("Description for tooltip")] [TextArea(2, 3)]
        public string Description;

        /// <summary>
        /// Check if this effect is empty (has no effects)
        /// </summary>
        public bool IsEmpty()
        {
            return AttributeEffect == null || AttributeEffect.IsEmpty();
        }

        /// <summary>
        /// Create a runtime instance of this effect.
        /// Uses the ID for deduplication unless overridden.
        /// </summary>
        public CombatAttributeDataEffectRuntime CreateRuntime(AttributeDisplayConfigSO displayConfig = null)
        {
            if (IsEmpty())
            {
                return null;
            }

            // Use provided ID, or fall back to this effect's ID for deduplication
            Guid effectId = ID.Value();

            UIColor color = displayConfig != null
                ? AttributeEffectColorHelper.GetColor(AttributeEffect, displayConfig)
                : AttributeEffectColorHelper.GetColor(AttributeEffect);

            return new CombatAttributeDataEffectRuntime(
                effectId,
                AttributeEffect,
                Duration,
                1, // level
                Icon,
                DisplayName,
                Description,
                color,
                false, // imageTint
                null, // extraLinesLayerOne
                null, // extraLinesLayerTwo
                true // show
            );
        }
    }
}

