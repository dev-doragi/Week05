using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    public GameObject IngamePlayButton;
    public GameObject IngameSkipButton;

    public GameObject DebugGamePannel;

    public GameObject CCTVPannel;
    public GameObject EditorPannel;
    public GameObject SettingPannel;

    public GameObject CCTVIconImage;
    public GameObject UnityIconImage;

    [Header("Unity Icon States")]
    public GameObject BGBase_Unity;
    public GameObject BarBase_Unity;
    public GameObject BGAlert_Unity;
    public GameObject BarAlert_Unity;

    [Header("CCTV Icon States")]
    public GameObject BGBase_CCTV;
    public GameObject BarBase_CCTV;
    public GameObject BGAlert_CCTV;
    public GameObject BarAlert_CCTV;

    private bool unityHover;
    private bool cctvHover;
    private bool unityAlert;
    private bool cctvAlert;
    private bool unitySelected = true;
    private bool cctvSelected = false;

    protected override void Init()
    {
        IngamePlayButton.SetActive(false);
        IngameSkipButton.SetActive(false);
        DebugGamePannel.SetActive(false);
        EditorPannelPopDown();

        unityHover = false;
        cctvHover = false;
        unityAlert = false;
        cctvAlert = false;
        unitySelected = true;
        cctvSelected = false;
        RefreshIconUI();
    }

    public void InGameChoiceButtonActive(bool choice)
    {
        IngamePlayButton.SetActive(choice);
        IngameSkipButton.SetActive(choice);
    }

    public void DebugGamePannelActive(bool choice)
    {
        DebugGamePannel.SetActive(choice);
    }

    public void EditorPannelPopup()
    {
        EditorPannel.transform.SetAsLastSibling();
        SettingPannel.transform.SetAsLastSibling();        
    }

    public void EditorPannelPopDown()
    {
        EditorPannel.transform.SetAsFirstSibling();        
    }

    public void HoverUnity(bool enter)
    {
        unityHover = enter;
        RefreshIconUI();
    }

    public void HoverCCTV(bool enter)
    {
        cctvHover = enter;
        RefreshIconUI();
    }

    public void ClickUnity()
    {
        unitySelected = true;
        cctvSelected = false;
        unityAlert = false;

        if (EditorPannel != null) EditorPannel.transform.SetAsLastSibling();
        if (SettingPannel != null) SettingPannel.transform.SetAsLastSibling();

        RefreshIconUI();
    }

    public void ClickCCTV()
    {
        cctvSelected = true;
        unitySelected = false;
        cctvAlert = false;

        if (CCTVPannel != null) CCTVPannel.transform.SetAsLastSibling();
        if (SettingPannel != null) SettingPannel.transform.SetAsLastSibling();

        RefreshIconUI();
    }

    // 알림 판정시
    public void SetUnityAlert(bool on)
    {
        unityAlert = on;
        RefreshIconUI();
    }

    public void SetCCTVAlert(bool on)
    {
        cctvAlert = on;
        RefreshIconUI();
    }

    private void RefreshIconUI()
    {
        bool unityBaseBgOn = (unityHover || unitySelected) && !unityAlert;
        bool unityBaseBarOn = unitySelected && !unityAlert;

        bool cctvBaseBgOn = (cctvHover || cctvSelected) && !cctvAlert;
        bool cctvBaseBarOn = cctvSelected && !cctvAlert;

        SetActiveSafe(BGBase_Unity, unityBaseBgOn);
        SetActiveSafe(BarBase_Unity, unityBaseBarOn);
        SetActiveSafe(BGAlert_Unity, unityAlert);
        SetActiveSafe(BarAlert_Unity, unityAlert);

        SetActiveSafe(BGBase_CCTV, cctvBaseBgOn);
        SetActiveSafe(BarBase_CCTV, cctvBaseBarOn);
        SetActiveSafe(BGAlert_CCTV, cctvAlert);
        SetActiveSafe(BarAlert_CCTV, cctvAlert);
    }

    private void SetActiveSafe(GameObject go, bool active)
    {
        if (go != null && go.activeSelf != active)
            go.SetActive(active);
    }
}
