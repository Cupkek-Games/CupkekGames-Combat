using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Text.RegularExpressions;
using CupkekGames.BehaviourTrees;
using CupkekGames.Graphs;
using CupkekGames.ShapeDrawing;
using CupkekGames.TimeSystem;
using CupkekGames.RPGStats;

using CupkekGames.VFX;

namespace CupkekGames.Combat
{
  [CreateAssetMenu(fileName = "CombatActionSO", menuName = "CupkekGames/Combat/CombatActionSO")]
  public class CombatActionSO : CupkekGames.BehaviourTrees.BehaviourTree
  {
    private static readonly Regex NodePlaceholderRegex = new(@"\{node(\d+)\}", RegexOptions.Compiled);
    private static readonly Regex NodeDurPlaceholderRegex = new(@"\{node(\d+)_dur\}", RegexOptions.Compiled);
    private static readonly Regex SkillLevelPlaceholderRegex = new(@"\{skillLevel\}", RegexOptions.Compiled);
    private static readonly Regex RangePlaceholderRegex = new(@"\{range\}", RegexOptions.Compiled);

    public string Name;
    [TextAreaAttribute] public string Description;
    public Sprite Icon;

    [SerializeReference]
    public CombatTargetSelection TargetSelection = new CombatTargetSelectionPrimaryTarget();

    public static void AttackTarget(ICombatSettings combatSettings, ICombatManager manager, CombatUnit attacker, float attack,
      CombatUnit target, DamageTypeDefinitionSO damageType)
    {
      DamageResult result = CombatDamageCalculator.CalculateAttackDamage(combatSettings, attacker, target, attack, damageType);
      CombatDamageCalculator.ApplyDamageAndVisuals(combatSettings, manager, attacker, target, result);
    }

    public List<CombatUnit> GetTargets(ICombatUnitManager combatUnitManager, CombatUnit caster, CombatUnit primaryTarget,
      bool debug)
    {
      return TargetSelection.GetTargets(combatUnitManager, caster, primaryTarget, debug);
    }

    public Indicator ShowIndicator(
      IIndicatorPool indicatorPool,
      Vector3 position,
      Quaternion rotation,
      float duration,
      CancellationToken? ct,
      TimeBundle timeBundle,
      Color? color = null)
    {
      return TargetSelection.ShowIndicator(indicatorPool, position, rotation, duration, ct, timeBundle, color);
    }

    /// <summary>
    /// Calculates and returns the utility percentage for AI decision-making.
    /// </summary>
    /// <returns>Value between 0-1</returns>
    public virtual float GetUtilityAIPercentage(ICombatUnitManager combatUnitManager, CombatUnit caster,
      CombatUnit primaryTarget, bool debug)
    {
      List<CombatUnit> targetList = GetTargets(combatUnitManager, caster, primaryTarget, debug);

      if (targetList.Count == 0)
      {
        return 0;
      }

      return 0.1f;
    }

    public string GetName(int skillLevel)
    {
      return Name + " T" + skillLevel;
    }

    /// <summary>
    /// The authored description with every placeholder resolved: {nodeN} and
    /// {nodeN_dur} ask the node at that index for its fragment, {skillLevel}
    /// and {range} are literal. Fragments are coloured and iconed through
    /// <see cref="ICombatRules.DescriptionStyle"/>; with a caster the numbers
    /// include that unit's attribute scaling.
    /// </summary>
    public string GetDescription(int skillLevel, ICombatRules rules, CombatUnit caster = null)
    {
      return ReplacePlaceholders(Description, Nodes, skillLevel, caster, rules, TargetSelection);
    }

    public static string ReplacePlaceholders(string input, IReadOnlyList<GraphNodeSO> nodes, int skillLevel,
      CombatUnit caster, ICombatRules rules, CombatTargetSelection TargetSelection)
    {
      string result = NodePlaceholderRegex.Replace(input, match =>
      {
        if (int.TryParse(match.Groups[1].Value, out int index) && index < nodes.Count)
        {
          var node = nodes[index];
          if (node is ICombatActionNodeDescription describable)
          {
            return describable.GetDescription(skillLevel, caster, rules);
          }
        }

        return match.Value;
      });

      result = NodeDurPlaceholderRegex.Replace(result, match =>
      {
        if (int.TryParse(match.Groups[1].Value, out int index) && index < nodes.Count)
        {
          var node = nodes[index];
          if (node is ICombatActionNodeDescription describable)
          {
            return describable.GetDescriptionDuration(skillLevel, caster, rules);
          }
        }

        return match.Value;
      });

      result = SkillLevelPlaceholderRegex.Replace(result, skillLevel.ToString());
      result = RangePlaceholderRegex.Replace(result, TargetSelection.Range.ToString());

      return result;
    }
  }
}