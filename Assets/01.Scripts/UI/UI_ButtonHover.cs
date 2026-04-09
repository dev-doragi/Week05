using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 마우스 호버 시 지정된 텍스트 라벨을 활성화/비활성화하는 UI 피드백 기능을 담당합니다.
/// </summary>
public class UI_ButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI Components")]
    [SerializeField] private TMP_Text _hoverLabel;

    private bool _isInitialized;

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (_isInitialized) return;

        // 시작 시 호버 라벨은 비활성화 상태로 초기화
        if (_hoverLabel != null)
            _hoverLabel.gameObject.SetActive(false);

        _isInitialized = true;
    }

    private void OnDisable()
    {
        // 오브젝트가 비활성화될 때 라벨도 함께 정리
        if (_hoverLabel != null)
            _hoverLabel.gameObject.SetActive(false);
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
}