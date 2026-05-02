using System;
using System.Collections.Generic;
using CupkekGames.AddressableAssets;
using CupkekGames.SceneManagement;
using CupkekGames.Sequencer;
using CupkekGames.Services;
using CupkekGames.Settings;
using CupkekGames.GameSave;
using UnityEngine;

using CupkekGames.VFX;

namespace CupkekGames.Combat
{
    /// <summary>
    /// Centralizes highlight logic for a single combat unit GameObject (or any object with renderers).
    /// - Outlines: source-tracked, with optional fade via <see cref="FadeableOutline"/> (same as gatherables).
    /// - Shader color/emission: source-tracked via ids on the shader controllers.
    /// </summary>
    public class CombatUnitHighlightController : MonoBehaviour
    {
        // We intentionally keep only ONE fadeable-outline index, matching the "enabled" outline preset.
        // Other outline indices remain instant add/remove via WideOutlineController.
        private const int FadeableOutlineIndex = 1;

        [Header("Fadeable outline settings (index 1)")]
        [SerializeField]
        private float _fadeableOutlineWidth = 2f;

        [SerializeField] private float _fadeInDuration = 0.12f;
        [SerializeField] private float _fadeOutDuration = 0.12f;

        private WideOutlineManager _wideOutlineManager;
        private FadeableOutline _fadeableOutline;
        private ShaderColorController _shaderColorController;
        private ShaderEmissionController _shaderEmissionController;

        private readonly Dictionary<object, Guid> _colorBySource = new();
        private readonly Dictionary<object, Guid> _emissionBySource = new();
        private readonly Dictionary<int, HashSet<object>> _outlineSourcesByIndex = new();

        private void Awake()
        {
            // Global combat settings (preferred source for hover outline feel)
            ICombatSettings combatSettings = ServiceLocator.Get<ICombatSettings>(true);
            if (combatSettings != null)
            {
                _fadeableOutlineWidth = combatSettings.HoverOutlineWidth;
                _fadeInDuration = combatSettings.HoverOutlineFadeInDuration;
                _fadeOutDuration = combatSettings.HoverOutlineFadeOutDuration;
            }

            // Wide outline is a shared service in the scene
            _wideOutlineManager = ServiceLocator.Get<WideOutlineManager>(true);

            // These exist on CombatUnitGameObject prefabs; allow null for non-unit objects (slot markers)
            _shaderColorController = GetComponentInChildren<ShaderColorController>();
            _shaderEmissionController = GetComponentInChildren<ShaderEmissionController>();

            EnsureFadeableOutline();
        }

        private void OnDisable()
        {
            // Safety: release all highlights owned by this controller when disabled.
            ReleaseAll();
        }

        public void Acquire(object sourceKey, Color color, int colorWeight, int outlineIndex)
        {
            if (sourceKey == null) throw new ArgumentNullException(nameof(sourceKey));

            AcquireOutline(sourceKey, outlineIndex);
            AcquireColor(sourceKey, color, colorWeight);
            AcquireEmission(sourceKey, color, colorWeight);
        }

        public void AcquireOutline(object sourceKey, int outlineIndex)
        {
            if (sourceKey == null) throw new ArgumentNullException(nameof(sourceKey));

            if (!_outlineSourcesByIndex.TryGetValue(outlineIndex, out HashSet<object> sources))
            {
                sources = new HashSet<object>();
                _outlineSourcesByIndex.Add(outlineIndex, sources);
            }

            if (!sources.Add(sourceKey) || sources.Count != 1) return;

            if (outlineIndex == FadeableOutlineIndex)
            {
                EnsureFadeableOutline();
                _fadeableOutline?.Fadeable.FadeIn();
                return;
            }

            _wideOutlineManager?.AddOutline(gameObject, outlineIndex);
        }

        public void AcquireColor(object sourceKey, Color color, int weight)
        {
            if (sourceKey == null) throw new ArgumentNullException(nameof(sourceKey));
            if (_shaderColorController == null) return;

            // Replace existing source contribution
            if (_colorBySource.Remove(sourceKey, out Guid existing))
            {
                _shaderColorController.RemoveColor(existing);
            }

            Guid id = _shaderColorController.AddColor(color, weight);
            _colorBySource[sourceKey] = id;
        }

        public void AcquireEmission(object sourceKey, Color color, int weight)
        {
            if (sourceKey == null) throw new ArgumentNullException(nameof(sourceKey));
            if (_shaderEmissionController == null) return;

            // Replace existing source contribution
            if (_emissionBySource.Remove(sourceKey, out Guid existing))
            {
                _shaderEmissionController.RemoveColor(existing);
            }

            Guid id = _shaderEmissionController.AddColor(color, weight);
            _emissionBySource[sourceKey] = id;
        }

        public void Release(object sourceKey)
        {
            if (sourceKey == null) return;

            // Outline: remove source from all outline indices
            foreach (var kvp in _outlineSourcesByIndex)
            {
                int outlineIndex = kvp.Key;
                HashSet<object> sources = kvp.Value;
                if (!sources.Remove(sourceKey)) continue;

                if (sources.Count != 0) continue;

                if (outlineIndex == FadeableOutlineIndex)
                {
                    EnsureFadeableOutline();
                    _fadeableOutline?.Fadeable.FadeOut();
                }
                else
                {
                    _wideOutlineManager?.RemoveOutline(gameObject, outlineIndex);
                }
            }

            // Color overlays
            if (_shaderColorController != null && _colorBySource.Remove(sourceKey, out Guid colorId))
            {
                _shaderColorController.RemoveColor(colorId);
            }

            if (_shaderEmissionController != null && _emissionBySource.Remove(sourceKey, out Guid emissionId))
            {
                _shaderEmissionController.RemoveColor(emissionId);
            }
        }

        public void ReleaseAll()
        {
            // Copy keys to avoid modifying during iteration
            List<object> sources = new List<object>();
            foreach (var key in _colorBySource.Keys) sources.Add(key);
            foreach (var key in _emissionBySource.Keys)
                if (!sources.Contains(key))
                    sources.Add(key);
            foreach (var kvp in _outlineSourcesByIndex)
            {
                foreach (var key in kvp.Value)
                {
                    if (!sources.Contains(key)) sources.Add(key);
                }
            }

            foreach (var source in sources)
            {
                Release(source);
            }

            _outlineSourcesByIndex.Clear();
        }

        private void EnsureFadeableOutline()
        {
            if (_fadeableOutline != null) return;

            bool created = false;
            _fadeableOutline = GetComponent<FadeableOutline>();
            if (_fadeableOutline == null)
            {
                created = true;
                _fadeableOutline = gameObject.AddComponent<FadeableOutline>();
            }

            // If it was just added, Awake() already ran with default settings and may have added an outline.
            // Remove it so this controller is the only driver.
            if (created)
            {
                _fadeableOutline.RemoveOutline();
            }

            _fadeableOutline._outlineIndex = FadeableOutlineIndex;
            _fadeableOutline._removeOnOther = true; // Ensure per-object outline control, not shared width

            // Configure hover feel (FadeableOutlineSize copies these into the underlying controller fade).
            _fadeableOutline.Fadeable._in = _fadeableOutlineWidth;
            _fadeableOutline.Fadeable._out = 0f;
            _fadeableOutline.Fadeable._fadeInDuration = _fadeInDuration;
            _fadeableOutline.Fadeable._fadeOutDuration = _fadeOutDuration;
            _fadeableOutline.Fadeable._fadeInDelay = 0f;
            _fadeableOutline.Fadeable._fadeOutDelay = 0f;

            if (created)
            {
                _fadeableOutline.Fadeable.SetFadedOut();
            }
        }
    }
}

