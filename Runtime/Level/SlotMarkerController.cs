using System.Collections;
using UnityEngine;
using CupkekGames.Luna;
using CupkekGames.AddressableAssets;
using CupkekGames.SceneManagement;
using CupkekGames.Sequencer;
using CupkekGames.Services;
using CupkekGames.Settings;
using CupkekGames.GameSave;

using CupkekGames.VFX;
using CupkekGames.Fadeables;

namespace CupkekGames.Combat
{
  /// <summary>
  /// Controls an optional slot marker GameObject: show/hide and highlight (scale + shader fade).
  /// Used by combat formation and base training. Add FadeableScale and ShaderPropertyFade on this GameObject or on _slotMarker for highlight/fade-in.
  /// </summary>
  public class SlotMarkerController : MonoBehaviour
  {
    [Header("Optional slot marker (no runtime spawning)")]
    [SerializeField] private GameObject _slotMarker;
    public GameObject SlotMarker => _slotMarker;

    private FadeableScale _slotMarkerScaleAnim;
    private ShaderPropertyFade _slotMarkerShaderFade;
    private Coroutine _showWithFadeRoutine;

    private void Awake()
    {
      // Prefer fade on the marker child so only the marker fades; fallback to this (e.g. combat highlight)
      if (_slotMarker != null)
      {
        _slotMarkerScaleAnim = _slotMarker.GetComponent<FadeableScale>();
        _slotMarkerShaderFade = _slotMarker.GetComponent<ShaderPropertyFade>();
      }
      if (_slotMarkerScaleAnim == null)
        _slotMarkerScaleAnim = GetComponent<FadeableScale>();
      if (_slotMarkerShaderFade == null)
        _slotMarkerShaderFade = GetComponent<ShaderPropertyFade>();

      if (_slotMarkerScaleAnim == null)
        Debug.LogError($"[SlotMarker] {gameObject.name}: Missing FadeableScale (on marker or this). Add it in Unity.", this);
      if (_slotMarkerShaderFade == null)
        Debug.LogError($"[SlotMarker] {gameObject.name}: Missing ShaderPropertyFade (on marker or this). Add it in Unity.", this);
    }

    /// <summary>
    /// Highlights the slot marker using ShaderPropertyFade (no outline).
    /// Components must be manually added in Unity.
    /// </summary>
    public void SetHighlighted(object sourceKey, bool highlighted, int outlineIndex = 2)
    {
      if (_slotMarkerScaleAnim == null || _slotMarkerShaderFade == null)
      {
        Debug.LogWarning($"[SlotMarker] {gameObject.name}: Cannot highlight - missing components (Scale: {_slotMarkerScaleAnim != null}, ShaderFade: {_slotMarkerShaderFade != null})", this);
        return;
      }

      if (highlighted)
      {
        _slotMarkerShaderFade.Fadeable.FadeIn();
        _slotMarkerScaleAnim.Fadeable.FadeIn();
      }
      else
      {
        _slotMarkerShaderFade.Fadeable.FadeOut();
        _slotMarkerScaleAnim.Fadeable.FadeOut();
      }
    }

    /// <summary>
    /// Enables or disables the slot marker GameObject (instant).
    /// </summary>
    public void SetSlotMarkerActive(bool active)
    {
      if (_showWithFadeRoutine != null)
      {
        StopCoroutine(_showWithFadeRoutine);
        _showWithFadeRoutine = null;
      }
      if (_slotMarker != null)
        _slotMarker.SetActive(active);
    }

    /// <summary>
    /// After delay seconds, shows the slot marker and fades it in. Use for training area (matches UI delay + fade).
    /// </summary>
    public void ShowWithDelayAndFadeIn(float delaySeconds)
    {
      if (_slotMarker == null) return;
      if (_showWithFadeRoutine != null)
        StopCoroutine(_showWithFadeRoutine);
      _showWithFadeRoutine = StartCoroutine(ShowWithDelayAndFadeInRoutine(delaySeconds));
    }

    private IEnumerator ShowWithDelayAndFadeInRoutine(float delaySeconds)
    {
      if (delaySeconds > 0f)
        yield return new WaitForSeconds(delaySeconds);
      _showWithFadeRoutine = null;
      _slotMarker.SetActive(true);
      if (_slotMarkerScaleAnim != null && _slotMarkerShaderFade != null)
      {
        _slotMarkerShaderFade.Fadeable.FadeIn();
        _slotMarkerScaleAnim.Fadeable.FadeIn();
      }
    }

    // public void SpawnCombatUnit(CombatUnit combatUnit)
    // {
    //   combatUnit.Initialize();

    //   Quaternion rotation = combatUnit.IsAlly ? Quaternion.Euler(0f, -90f, 0f) : Quaternion.Euler(0f, 90f, 0f);

    //   combatUnit.SpawnCombatUnitGameObject(transform.position, rotation);
    // }

    // public async UniTask SpawnCombatUnitAsync(CombatUnit combatUnit)
    // {
    //   combatUnit.Initialize();

    //   Quaternion rotation = combatUnit.IsAlly ? Quaternion.Euler(0f, -90f, 0f) : Quaternion.Euler(0f, 90f, 0f);

    //   await combatUnit.SpawnCombatUnitGameObjectAsync(transform.position, rotation);
    // }
  }
}
