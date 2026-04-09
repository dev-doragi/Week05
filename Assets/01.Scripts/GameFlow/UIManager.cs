using UnityEngine;

using DG.Tweening;
public class UIManager : Singleton<UIManager>
{
    [SerializeField] private Canvas rootCanvas;
    public GameObject IngamePlayButton;
    public GameObject IngameSkipButton;

    



    public GameObject DebugGamePannel;

    public GameObject CCTVPannel;
    public GameObject EditorPannel;
    public GameObject SettingPannel;

    public GameObject CCTVIconImage;
    public GameObject UnityIconImage;

    [Header("Unity Icon")]
    public GameObject BGBase_Unity;
    public GameObject BarBase_Unity;
    public GameObject BGAlert_Unity;
    public GameObject BarAlert_Unity;

    [Header("CCTV Icon")]
    public GameObject BGBase_CCTV;
    public GameObject BarBase_CCTV;
    public GameObject BGAlert_CCTV;
    public GameObject BarAlert_CCTV;

    [Header("LogMessage")]

    public GameObject LogMessage;

    [Header("Message States")]

    public GameObject AlertMessage;
    public Transform FirstMessageTransform;
    public Transform ChangeMessageTransform;


    private bool unityHover;
    private bool cctvHover;
    private bool unityAlert;
    private bool cctvAlert;
    private bool unitySelected = true;
    private bool cctvSelected = false;

    public GameObject NoiseScreen;

    private Sequence _alertMessageSeq;



    protected override void Init()
    {
        IngamePlayButton.SetActive(false);
        IngameSkipButton.SetActive(false);
        DebugGamePannel.SetActive(false);
        LogMessageActive(false);
        //EditorPannelPopDown();
        EditorPannelPopup();
        unityHover = false;
        cctvHover = false;
        unityAlert = false;
        cctvAlert = false;
        unitySelected = true;
        cctvSelected = false;
        AlertMessage.transform.position = FirstMessageTransform.position;
        NoiseScreen.SetActive(true);

        RefreshIconUI();
    }

    public void InGameChoiceButtonActive(bool choice)
    {
        IngamePlayButton.SetActive(choice);
        IngameSkipButton.SetActive(choice);
    }
    public void LogMessageActive(bool choice)
    {
        LogMessage.SetActive(choice);
    }
    public void DebugGamePannelActive(bool choice)
    {
        DebugGamePannel.SetActive(choice);
    }

    public void EditorPannelPopup()
    {
        rootCanvas.GetComponent<Canvas>().sortingOrder = 0;
        EditorPannel.transform.SetAsLastSibling();
        SettingPannel.transform.SetAsLastSibling();        
    }

    public void EditorPannelPopDown()
    {
        rootCanvas.GetComponent<Canvas>().sortingOrder = 10;
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

        EditorPannelPopup();
        


        RefreshIconUI();
    }

    public void ClickCCTV()
    {
        cctvSelected = true;
        unitySelected = false;
        cctvAlert = false;

        EditorPannelPopDown();
        
        RefreshIconUI();
    }

    // 알림 판정시
    public void SetUnityAlert(bool on)
    {
        PlayAlertMessageTween();
        unityAlert = on;
        RefreshIconUI();
    }

    public void SetCCTVAlert(bool on)
    {
        cctvAlert = on;
        RefreshIconUI();
    }

    public void NoiseActive(bool on)
    {
        NoiseScreen.SetActive(on);
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
    public void PlayAlertMessageTween()
    {

        if (_alertMessageSeq != null && _alertMessageSeq.IsActive())
            _alertMessageSeq.Kill();

        AlertMessage.SetActive(true);

        Transform msg = AlertMessage.transform;
        msg.localPosition = FirstMessageTransform.localPosition;

        _alertMessageSeq = DOTween.Sequence();
        _alertMessageSeq.Append(msg.DOLocalMove(ChangeMessageTransform.localPosition, 0.4f).SetEase(Ease.OutCubic));
        _alertMessageSeq.AppendInterval(1f);
        _alertMessageSeq.Append(msg.DOLocalMove(FirstMessageTransform.localPosition, 0.4f).SetEase(Ease.InCubic));
        _alertMessageSeq.OnComplete(() => { AlertMessage.SetActive(false); });
    }
    private void OnDisable()
    {
        if (_alertMessageSeq != null && _alertMessageSeq.IsActive())
            _alertMessageSeq.Kill();
    }

    




}
