using System.Collections.Generic;
using UnityEngine;
using CupkekGames.BehaviourTrees;

namespace CupkekGames.Combat
{
  public abstract class CombatActionNodeWithTarget : BTNodeAction
  {
    [SerializeField] private CombatActionNodeTargetType _targetType;
    public CombatActionNodeTargetType TargetType
    {
      get => _targetType;
      set => _targetType = value;
    }

    public List<CombatUnit> GetTargetList(CombatUnit caster, List<CombatUnit> targetList)
    {
      List<CombatUnit> list = new List<CombatUnit>();
      if (_targetType == CombatActionNodeTargetType.Caster)
      {
        list.Add(caster);
      }
      else if (_targetType == CombatActionNodeTargetType.Targets)
      {
        list.AddRange(targetList);
      }

      // remove null and null CombatUnitGameObject
      list.RemoveAll(unit => unit == null || unit.CombatUnitGameObject == null);

      return list;
    }
  }
}