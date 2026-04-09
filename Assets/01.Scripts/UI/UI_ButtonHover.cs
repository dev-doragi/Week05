using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// 마우스 호버 시 텍스트 표시 기능과 외부 호출을 통한 버튼 컬러 깜빡임 연출을 제어합니다.
/// </summary>
public class UI_ButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI Components")]
    [SerializeField] private Button _button;
    [SerializeField] private TMP_Text _hoverLabel;
    [SerializeField] private Image _blinkGraphic;

    [Header("Blink Settings")]
    [SerializeField] private Color _blinkColor = Color.red;
    [SerializeField] private float _blinkInterval = 0.2f;

    private Coroutine _blinkRoutine;
    private Color _originalColor;
    private bool _isInitialized;

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (_isInitialized) return;

        // 필수 컴포넌트 자동 연결
        if (_button == null) _button = GetComponent<Button>();

        // 깜빡일 그래픽이 지정되지 않았다면 버튼의 타겟 그래픽 사용
        if (_blinkGraphic == null && _button != null)
            _blinkGraphic = _button.targetGraphic as Image;

        if (_blinkGraphic != null)
            _originalColor = _blinkGraphic.color;

        // 호버 라벨은 시작 시 비활성화
        if (_hoverLabel != null)
            _hoverLabel.gameObject.SetActive(false);

        // 버튼 클릭 시 깜빡임이 멈추도록 설정
        if (_button != null)
            _button.onClick.AddListener(StopBlink);

        _isInitialized = true;
    }

    private void OnDisable()
    {
        StopBlink();
    }

    #region Mouse Hover Interface

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_hoverLabel != null)
            _hoverLabel.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_hoverLabel != null)
            _hoverLabel.gameObject.SetActive(false);
    }

    #endregion

    #region Public Control Methods

    /// <summary>
    /// 버튼 깜빡임 연출을 시작합니다.
    /// </summary>
    public void StartBlink()
    {
        Initialize();
        if (_blinkRoutine != null) return;
        _blinkRoutine = StartCoroutine(Co_BlinkLoop());
    }

    /// <summary>
    /// 깜빡임을 멈추고 버튼을 원래 색상으로 복구합니다.
    /// </summary>
    public void StopBlink()
    {
        if (_blinkRoutine != null)
        {
            StopCoroutine(_blinkRoutine);
            _blinkRoutine = null;
        }

        if (_blinkGraphic != null)
            _blinkGraphic.color = _originalColor;
    }

    #endregion

    private IEnumerator Co_BlinkLoop()
    {
        if (_blinkGraphic == null) yield break;

        while (true)
        {
            _blinkGraphic.color = _blinkColor;
            yield return new WaitForSecondsRealtime(_blinkInterval);
            _blinkGraphic.color = _originalColor;
            yield return new WaitForSecondsRealtime(_blinkInterval);
        }
    }
}