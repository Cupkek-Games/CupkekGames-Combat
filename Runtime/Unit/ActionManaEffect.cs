using System;

namespace CupkekGames.Combat
{
    public enum ManaEffectType
    {
        None,
        GainAttribute,
        GainAmount,
        DrainAll,
    }

    [Serializable]
    public class ActionManaEffect
    {
        public int ActionTypeId;
        public ManaEffectType Effect;
        public float Value;
    }
}
