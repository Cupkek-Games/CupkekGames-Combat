using System.Threading;
using Cysharp.Threading.Tasks;
using CupkekGames.TimeSystem;
using UnityEngine;

namespace CupkekGames.Combat
{
    /// <summary>
    /// A warning on the field: the cells an area covers, drawn by the fight's space
    /// (<see cref="ICombatSpace.ShowArea"/>). It shows outlined; <see cref="Fill"/> brings its
    /// fill up over a wind-up; <see cref="Hide"/> takes it down.
    /// </summary>
    public abstract class CombatAreaMark : MonoBehaviour
    {
        /// <summary>Counts the mark's showings, so a late hide never takes down a later warning on the same (pooled) mark.</summary>
        protected int Showing { get; private set; }

        /// <summary>Subclasses call this each time they show a new warning.</summary>
        protected void BeginShowing() => Showing++;

        /// <summary>Raises the fill from empty to full over <paramref name="seconds"/> of <paramref name="time"/>; 0 fills it at once.</summary>
        public abstract void Fill(float seconds, TimeBundle time, CancellationToken cancellationToken);

        /// <summary>Takes the warning down at once.</summary>
        public abstract void Hide();

        /// <summary>Takes this warning down after <paramref name="seconds"/> of <paramref name="time"/>.</summary>
        public async UniTask HideAfter(float seconds, TimeBundle time)
        {
            int showing = Showing;
            if (seconds > 0f) await time.TimeContext.DelayAsync(seconds);
            if (showing == Showing) Hide();
        }
    }
}
