using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UI_ScrollAutoDown : MonoBehaviour
{
    [SerializeField] private ScrollRect _scrollRect;
    [SerializeField] private RectTransform _content;

    private Coroutine _watchCoroutine;
    private int _lastChildCount;

    private void OnEnable()
    {
        if (_scrollRect == null || _content == null)
            return;

        _lastChildCount = _content.childCount;
        _watchCoroutine = StartCoroutine(CoWatchContent());
    }

    private void OnDisable()
    {
        if (_watchCoroutine != null)
        {
            StopCoroutine(_watchCoroutine);
            _watchCoroutine = null;
        }
    }

    private IEnumerator CoWatchContent()
    {
        while (true)
        {
            if (_content.childCount != _lastChildCount)
            {
                _lastChildCount = _content.childCount;

                yield return null;

                LayoutRebuilder.ForceRebuildLayoutImmediate(_content);
                _scrollRect.verticalNormalizedPosition = 0f;
            }

            yield return null;
        }
    }
}