using System.Collections.Generic;
using UnityEngine;
using CupkekGames.Luna;
using CupkekGames.Data.Primitives;
using CupkekGames.VFX;

namespace CupkekGames.Combat
{
  /// <summary>What putting a status on a unit that already has it does.</summary>
  public enum StatusStacking
  {
    /// <summary>The higher level wins (a lower one is refused); either way the longer time stays.</summary>
    Level,
    /// <summary>The levels add up to <see cref="StatusEffectSO.MaxStacks"/> (stacks of a burn); the longer time stays.</summary>
    Additive,
  }

  [CreateAssetMenu(fileName = "StatusEffect", menuName = "CupkekGames/Combat/StatusEffect/Status Effect")]
  public class StatusEffectSO : ScriptableObject
  {
    public SerializedGuid ID = new SerializedGuid(System.Guid.NewGuid());
    public string Name;
    [TextAreaAttribute] public string Description;
    public Sprite Icon;
    public int Priority = 0;
    public bool HasTier;
    public UIColor Color;
    [SerializeField] public VFXBundle VFXBundle;

    [Tooltip("Put on a unit that has it already: the higher level wins, or the levels add up as stacks.")]
    [SerializeField] private StatusStacking _stacking = StatusStacking.Level;
    [Tooltip("The most stacks (levels) it adds up to when additive.")]
    [Min(1)] [SerializeField] private int _maxStacks = 1;

    [SerializeReference] private List<IStatusEffectBehaviorFeature> _behaviors = new List<IStatusEffectBehaviorFeature>();

    public StatusStacking Stacking => _stacking;
    public int MaxStacks => _maxStacks;
    public IReadOnlyList<IStatusEffectBehaviorFeature> Behaviors => _behaviors;

    /// <summary>The controls it takes from its wearer (its <see cref="DisableBehavior"/>s).</summary>
    public CombatControl Controls
    {
      get
      {
        CombatControl controls = CombatControl.None;
        foreach (IStatusEffectBehaviorFeature b in _behaviors)
          if (b is DisableBehavior disable) controls |= disable.Controls;
        return controls;
      }
    }

    public void OnStart(ICombatSettings combatSettings, ICombatManager manager, StatusEffect effect, CombatUnit wearer)
    {
      foreach (IStatusEffectBehaviorFeature b in _behaviors)
        b?.OnStart(combatSettings, manager, effect, wearer);
    }

    public void OnTick(ICombatSettings combatSettings, ICombatManager manager, StatusEffect effect, CombatUnit wearer)
    {
      foreach (IStatusEffectBehaviorFeature b in _behaviors)
        b?.OnTick(combatSettings, manager, effect, wearer);
    }

    public void OnEnd(ICombatSettings combatSettings, ICombatManager manager, StatusEffect effect, CombatUnit wearer)
    {
      foreach (IStatusEffectBehaviorFeature b in _behaviors)
        b?.OnEnd(combatSettings, manager, effect, wearer);
    }

    public string GetName(int skillLevel)
    {
      return Name + " T" + skillLevel;
    }

    /// <summary>
    /// The authored description with {skillLevel} resolved. Status effects
    /// have no node graph, so no {nodeN} placeholders apply.
    /// </summary>
    public string GetDescription(int skillLevel)
    {
      return Description.Replace("{skillLevel}", skillLevel.ToString());
    }

    public void Prewarm(GameObject parent)
    {
      if (VFXBundle != null)
      {
        VFXBundle.Prewarm(parent);
      }
    }
  }
}