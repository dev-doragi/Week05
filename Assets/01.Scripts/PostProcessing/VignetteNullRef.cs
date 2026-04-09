using UnityEngine;

public class PPNullRef : MonoBehaviour
{
    [SerializeField] private VignetteService _vignetteService;
    [SerializeField] private ChromaticAberrationService _chromaticAberractionService;
    [SerializeField] private VignetteEffectSettings _vignetteSetting;
    [SerializeField] private ChromaticAberrationEffectSettings _chromaticAberrationSetting;

    private UI_InGameEditorRuntimeState _runtimeState;

    private void Start()
    {

        _runtimeState = InGameEditorController.EditorRuntimeState;

        if (_runtimeState != null)
            _runtimeState.ReferenceBroken += HandleReferenceBroken;
    }

    private void OnDisable()
    {
        if (_runtimeState != null)
            _runtimeState.ReferenceBroken -= HandleReferenceBroken;
    }

    private void HandleReferenceBroken(UI_RuntimeReferenceKey _)
    {
        OnPlayerDamaged();
    }

    public void OnPlayerDamaged()
    {
        _vignetteService.Play(_vignetteSetting);
        _chromaticAberractionService.Play(_chromaticAberrationSetting);
    }
}
