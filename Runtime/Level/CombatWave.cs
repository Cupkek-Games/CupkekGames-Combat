using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;
using Newtonsoft.Json.Linq;

namespace CupkekGames.Combat
{
  public class CombatWave
  {
    public Dictionary<Vector2Int, CombatUnitReference> Wave = new Dictionary<Vector2Int, CombatUnitReference>();
  }
}