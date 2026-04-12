using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIManager_New : Singleton<UIManager_New>
{
    [Header("Refs")]
    [SerializeField] private Canvas cctvCanvas;
    [SerializeField] private RectTransform cctvPanelJaein;
    [SerializeField] private RectTransform editorPanel;

    [Header("Canvas Order")]
    [SerializeField] private int openOrder = 101;
    [SerializeField] private int closedOrder = 99;

    [Header("CCTV Panel (Left/Right)")]
    [SerializeField] private float cctvClosedLeft = -2000f;
    [SerializeField] private float cctvClosedRight = 2000f;
    [SerializeField] private float cctvOpenLeft = 0f;
    [SerializeField] private float cctvOpenRight = 0f;

    [Header("Editor Panel (PosX)")]
    [SerializeField] private float editorShownX = 0.6f;
    [SerializeField] private float editorHiddenX = 2000.6f;

    [Header("Tween")]
    [SerializeField] private float duration = 0.35f;
    [SerializeField] private Ease ease = Ease.OutCubic;

    [Header("Initial")]
    [SerializeField] private bool startCctvOpened = false;

    [Header("GameOver")]
    [SerializeField] private GameObject GameOverImage;
    [SerializeField] private GameObject GameClearImage;


    private bool _isOpen;
    private Sequence _seq;

    protected override void Init()
    {
        _isOpen = startCctvOpened;
        ApplyImmediate(_isOpen);
        GameOverImage.SetActive(false);
        GameClearImage.SetActive(false);
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.tabKey.wasPressedThisFrame)
            ToggleByTab();
    }

    public void OpenCctvPanel()
    {
        if (_isOpen)
            return;

        _isOpen = true;
        PlayToggle(true);
    }

    public void ToggleByTab()
    {
        _isOpen = !_isOpen;
        PlayToggle(_isOpen);
    }

    private void PlayToggle(bool open)
    {
        if (_seq != null && _seq.IsActive())
            _seq.Kill();

        if (open && cctvCanvas != null)
            cctvCanvas.sortingOrder = openOrder;

        float startLeft = cctvPanelJaein != null ? cctvPanelJaein.offsetMin.x : 0f;
        float startRight = cctvPanelJaein != null ? -cctvPanelJaein.offsetMax.x : 0f;
        float endLeft = open ? cctvOpenLeft : cctvClosedLeft;
        float endRight = open ? cctvOpenRight : cctvClosedRight;
        float endEditorX = open ? editorHiddenX : editorShownX;

        _seq = DOTween.Sequence();

        float t = 0f;
        _seq.Join(
            DOTween.To(() => t, x =>
            {
                t = x;
                float left = Mathf.Lerp(startLeft, endLeft, t);
                float right = Mathf.Lerp(startRight, endRight, t);
                SetCctvLeftRight(left, right);
            }, 1f, duration).SetEase(ease)
        );

        if (editorPanel != null)
            _seq.Join(editorPanel.DOAnchorPosX(endEditorX, duration).SetEase(ease));

        _seq.OnComplete(() =>
        {
            if (!open && cctvCanvas != null)
                cctvCanvas.sortingOrder = closedOrder;
        });
    }

    private void ApplyImmediate(bool open)
    {
        if (cctvCanvas != null)
            cctvCanvas.sortingOrder = open ? openOrder : closedOrder;

        SetCctvLeftRight(open ? cctvOpenLeft : cctvClosedLeft, open ? cctvOpenRight : cctvClosedRight);

        if (editorPanel != null)
        {
            Vector2 p = editorPanel.anchoredPosition;
            p.x = open ? editorHiddenX : editorShownX;
            editorPanel.anchoredPosition = p;
        }
    }

    private void SetCctvLeftRight(float left, float right)
    {
        if (cctvPanelJaein == null) return;

        Vector2 min = cctvPanelJaein.offsetMin;
        Vector2 max = cctvPanelJaein.offsetMax;

        min.x = left;
        max.x = -right;
        cctvPanelJaein.offsetMin = min;
        cctvPanelJaein.offsetMax = max;
    }

    private void OnDisable()
    {
        if (_seq != null && _seq.IsActive())
            _seq.Kill();
    }

    public void GameOverImagePopUP()
    {
        GameOverImage.SetActive(true);

    }
    public void GameClearImagePopUP()
    {
        GameClearImage.SetActive(true);
    }
}
