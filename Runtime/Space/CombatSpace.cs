using System;
using System.Collections.Generic;
using CupkekGames.TimeSystem;
using UnityEngine;

namespace CupkekGames.Combat
{
    /// <summary>
    /// An <see cref="ICombatSpace"/> that lives on a fight (a component beside its unit
    /// manager), so a fight picks its space by which one it carries. Implementations live
    /// in their own packages (a hex grid).
    /// </summary>
    public abstract class CombatSpace : MonoBehaviour, ICombatSpace
    {
        public abstract float Distance(CombatUnit a, CombatUnit b);
        public abstract bool InRange(CombatUnit caster, CombatUnit target, float range);
        public abstract int StepsToReach(CombatUnit caster, CombatUnit target, float range);
        public abstract void Collect(in CombatArea area, List<CombatUnit> results);
        public abstract float ToWorld(float units);
        public abstract ICombatMover CreateMover(CombatUnitView view);
        public abstract IDisposable Drive(TimeContext time, Action<float> step);
    }
}
