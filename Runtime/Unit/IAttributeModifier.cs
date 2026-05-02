using CupkekGames.RPGStats;

namespace CupkekGames.Combat
{
    /// <summary>
    /// Implemented by <see cref="CupkekGames.Units.IUnitFeature"/> instances that modify
    /// combat attribute values in the <see cref="CombatUnit.GetAttributeValue"/> pipeline.
    /// </summary>
    public interface IAttributeModifier
    {
        float ModifyAttribute(CombatUnit unit, AttributeDefinitionSO attribute, float currentValue);
    }
}
