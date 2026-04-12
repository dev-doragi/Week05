using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_ConsoleMissionView : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text _missionTitle;
    [SerializeField] private Image _successImage;
    [SerializeField] private Image _hoverImage;

    [Header("Animation")]
    [SerializeField] private float _successAnimDuration = 0.4f;

    private int _index;
    public int Index => _index;

    private Sequence _successSequence;

    public void Init(int missionIndex, string missionTitle)
    {
        _missionTitle.text = missionTitle;
        _index = missionIndex;

        if (_successImage != null)
            _successImage.gameObject.SetActive(false);

        if (_hoverImage != null)
            _hoverImage.gameObject.SetActive(false);
    }

    public void IsSuccess()
    {
        PlaySuccessAnimation();
        ApplyStrikeThrough();
        ShowSuccessIcon();
    }

    private void PlaySuccessAnimation()
    {
        if (_hoverImage == null)
            return;

        _successSequence?.Kill();

        _hoverImage.gameObject.SetActive(true);

        _hoverImage.type = Image.Type.Filled;
        _hoverImage.fillMethod = Image.FillMethod.Horizontal;
        _hoverImage.fillOrigin = 0;
        _hoverImage.fillAmount = 0f;

        Color hoverColor = _hoverImage.color;
        hoverColor.a = 0f;
        _hoverImage.color = hoverColor;

        _successSequence = DOTween.Sequence();

        _successSequence
            .Append(_hoverImage.DOFade(1f, _successAnimDuration * 0.25f))
            .Join(_hoverImage.DOFillAmount(1f, _successAnimDuration))
            .Append(_hoverImage.DOFade(0f, _successAnimDuration * 0.35f))
            .OnComplete(() =>
            {
                _hoverImage.gameObject.SetActive(false);
            });
    }

    private void ApplyStrikeThrough()
    {
        if (_missionTitle == null)
            return;

        _missionTitle.fontStyle |= FontStyles.Strikethrough;
        _missionTitle.color = Color.gray;
    }

    private void ShowSuccessIcon()
    {
        if (_successImage == null)
            return;

        _successImage.gameObject.SetActive(true);
    }

    private void OnDestroy()
    {
        _successSequence?.Kill();
    }
}