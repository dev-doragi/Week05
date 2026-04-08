using UnityEngine;
using UnityEngine.UI;

public class CameraManager : MonoBehaviour
{
    private static readonly int BackgroundPropertyId = Shader.PropertyToID("_background");

    [SerializeField] private RawImage _background;
    [SerializeField] private CameraNoiseOverlay _cameraNoiseOverlay;

    private Material _backgroundMaterial;
    private CameraAreaController _lastSelected;

    private void Awake()
    {
        if (_background == null)
            return;

        if (_background.material != null)
        {
            _backgroundMaterial = Instantiate(_background.material);
            _background.material = _backgroundMaterial;
        }
    }

    private void Start()
    {
        if (_lastSelected != null)
            ApplyCameraTexture(_lastSelected);
    }

    public void SelectCamera(CameraAreaController selectedArea)
    {
        if (selectedArea == null)
            return;

        ApplyCameraTexture(selectedArea);

        if (_lastSelected != null && _lastSelected != selectedArea)
            _lastSelected.StopBlinking();

        selectedArea.StartBlinking();
        _lastSelected = selectedArea;

        if (_cameraNoiseOverlay != null)
            _cameraNoiseOverlay.PlaySwitchNoiseOnce();
    }

    public void RefreshSelectedCamera()
    {
        if (_lastSelected == null)
            return;

        ApplyCameraTexture(_lastSelected);
    }

    private void ApplyCameraTexture(CameraAreaController selectedArea)
    {
        Texture targetTexture = selectedArea.CurrentBackgroundTexture;

        if (_backgroundMaterial != null)
        {
            _backgroundMaterial.SetTexture(BackgroundPropertyId, targetTexture);
            return;
        }

        if (_background != null)
            _background.texture = targetTexture;
    }
}