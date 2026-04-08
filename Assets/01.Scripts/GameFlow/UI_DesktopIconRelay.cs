using UnityEngine;
using UnityEngine.EventSystems;

public enum DesktopIconType
{
    Unity,
    CCTV
}

public class UI_DesktopIconRelay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private DesktopIconType iconType;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (UIManager.Instance == null) return;
        if (iconType == DesktopIconType.Unity) UIManager.Instance.HoverUnity(true);
        else UIManager.Instance.HoverCCTV(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (UIManager.Instance == null) return;
        if (iconType == DesktopIconType.Unity) UIManager.Instance.HoverUnity(false);
        else UIManager.Instance.HoverCCTV(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (UIManager.Instance == null) return;

        if (iconType == DesktopIconType.Unity) UIManager.Instance.ClickUnity();
        else UIManager.Instance.ClickCCTV();
    }
}
