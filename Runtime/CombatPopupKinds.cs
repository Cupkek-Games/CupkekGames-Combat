namespace CupkekGames.Combat
{
    /// <summary>
    /// Popup kinds combat shows beyond <see cref="CupkekGames.TextPopup.PopupKinds"/>; a game
    /// registers each with its popup manager.
    /// </summary>
    public static class CombatPopupKinds
    {
        /// <summary>A dodged hit (<see cref="DamageResult.IsMiss"/>).</summary>
        public const string Miss = "Miss";
    }
}
