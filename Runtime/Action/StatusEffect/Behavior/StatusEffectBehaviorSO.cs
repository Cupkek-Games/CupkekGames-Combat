using UnityEngine;

namespace CupkekGames.Combat
{
  public abstract class StatusEffectBehaviorSO : ScriptableObject
  {
    public virtual void OnStart(ICombatSettings combatSettings, ICombatManager manager,
      CombatUnit caster, CombatUnit target, int skillLevel) { }

    public virtual void OnTick(ICombatSettings combatSettings, ICombatManager manager,
      CombatUnit caster, CombatUnit target, int skillLevel) { }

    public virtual void OnEnd(ICombatSettings combatSettings, ICombatManager manager,
      CombatUnit caster, CombatUnit target, int skillLevel) { }
  }
}
