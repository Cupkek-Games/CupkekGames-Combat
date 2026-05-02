using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using CupkekGames.RPGStats;

namespace CupkekGames.Combat
{
  public abstract class CombatActionSOWithDamage : CombatActionSO
  {
    [SerializeField] protected int _baseAttack;
    [SerializeField] protected float _attackAttrMultiplier;
    [SerializeField] protected DamageTypeDefinitionSO _damageType;
    public float GetAttackDamage(CombatUnit caster)
    {
      return (caster.GetAttributeValue(_damageType.AttackAttribute) * _attackAttrMultiplier) + _baseAttack;
    }
  }
}