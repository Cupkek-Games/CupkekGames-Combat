using System.Collections.Generic;
using System.Threading;
using CupkekGames.Graphs;

namespace CupkekGames.Combat
{
    /// <summary>
    /// Strongly-typed accessors over the action's <see cref="GraphFrame"/>.
    /// One instance per scope: the runner creates the root context during
    /// Setup; decorators that want to scope a value to their child branch
    /// (target selection / target update) push a child frame and create a
    /// new context bound to it, then store that nested context under the
    /// "Context" key on the local frame — descendants of the decorator
    /// resolve <see cref="From"/> to the scoped instance instead of the
    /// root one.
    /// </summary>
    public class CombatActionContext
    {
        readonly GraphFrame _frame;
        CancellationTokenSource _linkedCts;

        /// <summary>The frame this context is bound to.</summary>
        public GraphFrame Frame => _frame;

        public CombatActionContext(GraphFrame frame)
        {
            _frame = frame;
        }

        /// <summary>Resolve the most-scoped Context visible from <paramref name="frame"/>.</summary>
        public static CombatActionContext From(GraphFrame frame)
        {
            return frame != null && frame.TryGet<CombatActionContext>("Context", out var v) ? v : null;
        }

        // ─── Core combat data ────────────────────────────────────────────

        public CombatActionSO ActionSO => Get<CombatActionSO>("CombatActionSO");
        public ICombatSettings CombatSettings => Get<ICombatSettings>("CombatSettings");
        public ICombatManager CombatManager => Get<ICombatManager>("CombatManager");
        public CombatUnit Caster => Get<CombatUnit>("Caster");
        public CombatUnit PrimaryTarget => Get<CombatUnit>("PrimaryTarget");
        public int SkillLevel => Get<int>("SkillLevel");

        /// <summary>
        /// Reads walk the frame chain so a scoped decorator's local
        /// <c>TargetList</c> is visible to its descendants. Writes go
        /// to the bound frame's locals — so to scope a list to a branch,
        /// push a frame, create a new context, then set the list via that
        /// nested context.
        /// </summary>
        public List<CombatUnit> TargetList
        {
            get => Get<List<CombatUnit>>("TargetList");
            set => _frame.SetLocal("TargetList", value);
        }

        // ─── Cancellation tokens ────────────────────────────────────────

        public CancellationToken CombatCancelToken => GetToken("CancellationToken");
        public CancellationToken CasterDeathToken => GetToken("CancellationTokenCasterDeath");
        public CancellationToken CasterInterruptToken => GetToken("CancellationTokenCasterInterrupt");

        public bool IsCancelled =>
            CombatCancelToken.IsCancellationRequested ||
            CasterDeathToken.IsCancellationRequested ||
            CasterInterruptToken.IsCancellationRequested;

        public CancellationToken LinkedCancelToken
        {
            get
            {
                if (_linkedCts == null)
                {
                    _linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                        CombatCancelToken, CasterDeathToken, CasterInterruptToken);
                }
                return _linkedCts.Token;
            }
        }

        public CancellationToken CreateTargetLinkedToken(CancellationToken targetDeathToken)
        {
            return CancellationTokenSource.CreateLinkedTokenSource(LinkedCancelToken, targetDeathToken).Token;
        }

        public CancellationToken CreateTargetLinkedToken(CancellationToken targetDeathToken,
            CancellationToken targetInterruptToken)
        {
            return CancellationTokenSource.CreateLinkedTokenSource(LinkedCancelToken, targetDeathToken,
                targetInterruptToken).Token;
        }

        public void Dispose()
        {
            _linkedCts?.Dispose();
            _linkedCts = null;
        }

        // ─── Private helpers ─────────────────────────────────────────────

        T Get<T>(string key) => _frame.TryGet<T>(key, out var v) ? v : default;
        CancellationToken GetToken(string key) => _frame.TryGet<CancellationToken>(key, out var v) ? v : default;
    }
}
