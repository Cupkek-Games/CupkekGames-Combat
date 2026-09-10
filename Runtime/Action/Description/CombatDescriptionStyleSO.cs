using System;
using UnityEngine;

namespace CupkekGames.Combat
{
  /// <summary>
  /// The one place skill-description colours, inline icons and number
  /// formatting are authored. Nodes that implement
  /// <see cref="ICombatActionNodeDescription"/> build their fragments through
  /// this asset, so authored descriptions carry no rich-text tags of their
  /// own. Rich text cannot read USS variables; copy the theme palette hexes
  /// here so text and UI agree. Icons are sprite names in the panel's text
  /// settings sprite asset (rendered as <c>&lt;sprite name="..."&gt;</c>).
  /// </summary>
  [CreateAssetMenu(fileName = "CombatDescriptionStyle", menuName = "CupkekGames/Combat/Combat Description Style")]
  public class CombatDescriptionStyleSO : ScriptableObject
  {
    [Serializable]
    public class RoleStyle
    {
      public Color Color = Color.white;
      [Tooltip("Sprite name in the panel's sprite asset. Empty = no inline icon.")]
      public string SpriteName;
    }

    [SerializeField] private RoleStyle _heal = new();
    [SerializeField] private RoleStyle _shield = new();
    [SerializeField] private RoleStyle _status = new();
    [SerializeField] private RoleStyle _duration = new();

    [Header("Numbers")]
    [Tooltip("Show the base + attribute breakdown after a scaled total when a caster is known.")]
    [SerializeField] private bool _showBreakdown = true;
    [Tooltip("Font size of the breakdown, percent of the surrounding text.")]
    [Range(50, 100)][SerializeField] private int _breakdownSizePercent = 80;

    public RoleStyle Get(CombatDescriptionRole role)
    {
      switch (role)
      {
        case CombatDescriptionRole.Heal: return _heal;
        case CombatDescriptionRole.Shield: return _shield;
        case CombatDescriptionRole.Status: return _status;
        case CombatDescriptionRole.Duration: return _duration;
        default: throw new ArgumentOutOfRangeException(nameof(role), role, null);
      }
    }

    public string OpenTag(CombatDescriptionRole role)
    {
      return OpenTag(Get(role).Color);
    }

    public static string OpenTag(Color color)
    {
      return $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>";
    }

    public const string CloseTag = "</color>";

    /// <summary>Wraps <paramref name="text"/> in the role's colour.</summary>
    public string Colorize(CombatDescriptionRole role, string text)
    {
      return OpenTag(role) + text + CloseTag;
    }

    /// <summary>The role's inline icon tag, or empty when none is authored.</summary>
    public string Icon(CombatDescriptionRole role)
    {
      return SpriteTag(Get(role).SpriteName);
    }

    public static string SpriteTag(string spriteName)
    {
      return string.IsNullOrEmpty(spriteName) ? string.Empty : $"<sprite name=\"{spriteName}\">";
    }

    /// <summary>A flat number (no caster to scale it).</summary>
    public string Number(int value)
    {
      return $"<b>{value}</b>";
    }

    /// <summary>
    /// A scaled number: the total in bold, then the base + attribute
    /// breakdown at reduced size when enabled. <paramref name="scalingIcon"/>
    /// is the attribute's inline icon tag (may be empty).
    /// </summary>
    public string Number(int total, int baseValue, int addition, string scalingIcon)
    {
      if (!_showBreakdown)
      {
        return Number(total);
      }

      return $"<b>{total}</b> <size={_breakdownSizePercent}%>({baseValue} + {addition}{scalingIcon})</size>";
    }
  }
}
