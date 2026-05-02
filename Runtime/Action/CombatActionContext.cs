using System.Collections.Generic;
using System.Threading;

namespace CupkekGames.Combat
{
    /// <summary>
    /// Provides strongly-typed access to the combat action behaviour tree blackboard.
    /// Created once per action in CombatActionRunner.Setup() and stored under the "Context" key.
    /// </summary>
    public class CombatActionContext
    {
        private readonly Dictionary<string, object> _blackboard;
        private CancellationTokenSource _linkedCts;

        public CombatActionContext(Dictionary<string, object> blackboard)
        {
            _blackboard = blackboard;
        }

        /// <summary>
        /// Retrieves the CombatActionContext from the blackboard.
        /// </summary>
        public static CombatActionContext From(Dictionary<string, object> blackboard)
        {
            return (CombatActionContext)blackboard["Context"];
        }

        // ─── Core combat data ────────────────────────────────────────────

        public CombatActionSO ActionSO => Get<CombatActionSO>("CombatActionSO");
        public ICombatSettings CombatSettings => Get<ICombatSettings>("CombatSettings");
        public ICombatManager CombatManager => Get<ICombatManager>("CombatManager");
        public CombatUnit Caster => Get<CombatUnit>("Caster");
        public CombatUnit PrimaryTarget => Get<CombatUnit>("PrimaryTarget");
        public int SkillLevel => Get<int>("SkillLevel");

        public List<CombatUnit> TargetList
        {
            get => Get<List<CombatUnit>>("TargetList");
            set => _blackboard["TargetList"] = value;
        }

        // ─── Cancellation tokens ────────────────────────────────────────

        public CancellationToken CombatCancelToken => GetToken("CancellationToken");
        public CancellationToken CasterDeathToken => GetToken("CancellationTokenCasterDeath");
        public CancellationToken CasterInterruptToken => GetToken("CancellationTokenCasterInterrupt");

        /// <summary>
        /// Returns true if any caster-level cancellation token has been triggered.
        /// </summary>
        public bool IsCancelled =>
            CombatCancelToken.IsCancellationRequested ||
            CasterDeathToken.IsCancellationRequested ||
            CasterInterruptToken.IsCancellationRequested;

        /// <summary>
        /// A linked CancellationToken that triggers when any caster-level token is cancelled.
        /// Cached after first access per context instance.
        /// </summary>
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

        /// <summary>
        /// Creates a linked token combining all caster tokens with a target death token.
        /// </summary>
        public CancellationToken CreateTargetLinkedToken(CancellationToken targetDeathToken)
        {
            return CancellationTokenSource.CreateLinkedTokenSource(LinkedCancelToken, targetDeathToken).Token;
        }

        /// <summary>
        /// Creates a linked token combining all caster tokens with target death and interrupt tokens.
        /// </summary>
        public CancellationToken CreateTargetLinkedToken(CancellationToken targetDeathToken,
            CancellationToken targetInterruptToken)
        {
            return CancellationTokenSource.CreateLinkedTokenSource(LinkedCancelToken, targetDeathToken,
                targetInterruptToken).Token;
        }

        /// <summary>
        /// Disposes the cached linked CancellationTokenSource. Called on action completion.
        /// </summary>
        public void Dispose()
        {
            _linkedCts?.Dispose();
            _linkedCts = null;
        }

        // ─── Private helpers ─────────────────────────────────────────────

        private T Get<T>(string key)
        {
            return _blackboard.TryGetValue(key, out var val) ? (T)val : default;
        }

        private CancellationToken GetToken(string key)
        {
            return _blackboard.TryGetValue(key, out var val) ? (CancellationToken)val : default;
        }
    }
}
