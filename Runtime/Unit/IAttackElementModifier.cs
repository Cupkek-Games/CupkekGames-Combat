using CupkekGames.RPGStats;

namespace CupkekGames.Combat
{
    /// <summary>
    /// Implemented by <see cref="CupkekGames.Units.IUnitFeature"/> instances that
    /// change the element their unit attacks in (an infusion), in feature order
    /// (<see cref="CombatUnit.GetAttackElement"/>). Only the attacker's side: the
    /// element the unit is hit in stays its own.
    /// </summary>
    public interface IAttackElementModifier
    {
        ElementTypeDefinitionSO ModifyAttackElement(CombatUnit unit, ElementTypeDefinitionSO current);
    }
}
