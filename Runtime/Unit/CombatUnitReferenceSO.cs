using CupkekGames.Luna;
using UnityEngine;

namespace CupkekGames.Combat
{
  public class CombatUnitReferenceSO : ScriptableObject
  {
    [SerializeField] private CombatUnitReference _combatUnitReference;
    public CombatUnitReference CombatUnitReference => _combatUnitReference;
  }
}