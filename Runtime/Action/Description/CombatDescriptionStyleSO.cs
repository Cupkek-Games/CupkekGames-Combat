using System;
using System.Collections.Generic;
using UnityEngine;
using CupkekGames.RPGStats;

namespace CupkekGames.Combat
{
  /// <summary>
  /// The one place skill-description colours, inline icons and number
  /// formatting are authored. Nodes that implement
  /// <see cref="ICombatActionNodeDescription"/> build their fragments through
  /// this asset, so authored descriptions carry no rich-text tags of their
  /// own. Rich text cannot read USS variables; copy the theme palette hexes
  /// here so text and UI agree. Icons are sprite names in the panel's text
  /// settings sprite asset (rendered as <c>&lt;sprite name="..." tint=1&gt;</c>,
  /// tinted with the surrounding text colour).
  /// <para>
  /// One asset per surface the text is read on. <see cref="ICombatRules.DescriptionStyle"/>
  /// is the default surface; a screen that shows descriptions on another one
  /// (a light panel next to dark tooltips) authors a second asset with that
  /// surface's shades and passes it to <see cref="CombatActionSO.GetDescription"/>.
  /// </para>
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

    /// <summary>A damage type's colour on this style's surface.</summary>
    [Serializable]
    public class DamageTypeStyle
    {
      public DamageTypeDefinitionSO DamageType;
      public Color Color = Color.white;
    }

    [SerializeField] private RoleStyle _heal = new();
    [SerializeField] private RoleStyle _shield = new();
    [SerializeField] private RoleStyle _status = new();
    [SerializeField] private RoleStyle _duration = new();

    [Header("Damage and attribute changes")]
    [Tooltip("Damage colours on this surface. A damage type without an entry keeps its own rich-text colour (DamageTypeDefinitionSO). The first entry for a type wins.")]
    [SerializeField] private List<DamageTypeStyle> _damageTypes = new();
    [Tooltip("The config whose positive and negative rich-text colours buff and debuff fragments use on this surface. Empty = the rules' AttributeDisplayConfig.")]
    [SerializeField] private AttributeDisplayConfigSO _attributeDisplayConfig;

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

    /// <summary>
    /// The opening colour tag of <paramref name="damageType"/>'s fragments on
    /// this surface: this style's entry for the type when it has one, else the
    /// type's own <see cref="DamageTypeDefinitionSO.RichTextColor"/>.
    /// </summary>
    public string OpenTag(DamageTypeDefinitionSO damageType)
    {
      foreach (DamageTypeStyle entry in _damageTypes)
      {
        if (entry.DamageType == damageType)
        {
          return OpenTag(entry.Color);
        }
      }

      return damageType.RichTextColor;
    }

    /// <summary>Wraps <paramref name="text"/> in the role's colour.</summary>
    public string Colorize(CombatDescriptionRole role, string text)
    {
      return OpenTag(role) + text + CloseTag;
    }

    /// <summary>Wraps <paramref name="text"/> in the damage type's colour on this surface.</summary>
    public string Colorize(DamageTypeDefinitionSO damageType, string text)
    {
      return OpenTag(damageType) + text + CloseTag;
    }

    /// <summary>
    /// The config whose rich-text colours buff and debuff fragments use on
    /// this surface: this style's own when authored, else
    /// <paramref name="rules"/>' <see cref="ICombatRules.AttributeDisplayConfig"/>.
    /// </summary>
    public AttributeDisplayConfigSO GetAttributeDisplayConfig(ICombatRules rules)
    {
      return _attributeDisplayConfig != null ? _attributeDisplayConfig : rules.AttributeDisplayConfig;
    }

    /// <summary>The role's inline icon tag, or empty when none is authored.</summary>
    public string Icon(CombatDescriptionRole role)
    {
      return SpriteTag(Get(role).SpriteName);
    }

    public static string SpriteTag(string spriteName)
    {
      return string.IsNullOrEmpty(spriteName) ? string.Empty : $"<sprite name=\"{spriteName}\" tint=1>";
    }

    /// <summary>
    /// A flat number (no caster to scale it): the fragment's icon, then the
    /// value in bold. <paramref name="icon"/> is an inline sprite tag (may be
    /// empty).
    /// </summary>
    public string Number(string icon, int value)
    {
      return $"{icon}<b>{value}</b>";
    }

    /// <summary>
    /// A scaled number: icon, total in bold, then the base + attribute
    /// breakdown at reduced size when enabled.
    /// </summary>
    public string Number(string icon, int total, int baseValue, int addition)
    {
      if (!_showBreakdown)
      {
        return Number(icon, total);
      }

      return $"{icon}<b>{total}</b> <size={_breakdownSizePercent}%>({baseValue} + {addition})</size>";
    }
  }
}
