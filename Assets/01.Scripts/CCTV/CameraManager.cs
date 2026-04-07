using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CameraManager : MonoBehaviour
{
    public RawImage background;
    private Material backgroundMaterial;
    private CameraAreaController lastSelected;

    public void Start()
    {
        backgroundMaterial = background.material;
    }

    public void SelectCamera(CameraAreaController selected)
    {
        backgroundMaterial.SetTexture("_background", selected.roomBackgroundSprite);

        if (lastSelected != null && lastSelected != selected)
            lastSelected.StopBlinking();

        selected.StartBlinking();
        lastSelected = selected;
    }
}