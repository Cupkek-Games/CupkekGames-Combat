using System.Collections.Generic;
using UnityEngine;

namespace CupkekGames.Combat
{
  public class CombatWave
  {
    public Dictionary<Vector2Int, CombatUnitReference> Wave = new Dictionary<Vector2Int, CombatUnitReference>();

    public CombatWave() { }

    public CombatWave(CombatWave other)
    {
      if (other?.Wave == null)
        return;
      foreach (KeyValuePair<Vector2Int, CombatUnitReference> pair in other.Wave)
        Wave[pair.Key] = pair.Value != null ? new CombatUnitReference(pair.Value) : null;
    }
  }
}
