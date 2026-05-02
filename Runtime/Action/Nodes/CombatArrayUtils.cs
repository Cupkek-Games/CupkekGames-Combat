namespace CupkekGames.Combat
{
    internal static class CombatArrayUtils
    {
        public static T GetArrayElementOrLast<T>(T[] array, int index)
        {
            if (array == null || array.Length == 0)
                return default;
            if (index < 0)
                return default;

            int length = array.Length;
            return index < length ? array[index] : array[length - 1];
        }
    }
}
