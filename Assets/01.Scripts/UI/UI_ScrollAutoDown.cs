using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UI_ScrollAutoDown : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ScrollRect _scrollRect;
    [SerializeField] private RectTransform _contentRect;

    private Coroutine _heightWatchRoutine;
    private float _previousContentPreferredHeight;

    private void OnEnable()
    {
        _previousContentPreferredHeight = GetContentPreferredHeight();
        _heightWatchRoutine = StartCoroutine(WatchContentHeightAndScrollDown());
    }

    private void OnDisable()
    {
        StopHeightWatchRoutine();
    }

    private IEnumerator WatchContentHeightAndScrollDown()
    {
        while (true)
        {
            float currentContentPreferredHeight = GetContentPreferredHeight();

            if (HasContentHeightChanged(currentContentPreferredHeight))
            {
                _previousContentPreferredHeight = currentContentPreferredHeight;

                yield return null;

                RebuildContentLayout();
                ScrollToBottom();
            }

            yield return null;
        }
    }


    private float GetContentPreferredHeight()
    {
        return LayoutUtility.GetPreferredHeight(_contentRect);
    }

    private bool HasContentHeightChanged(float currentContentPreferredHeight)
    {
        return Mathf.Approximately(currentContentPreferredHeight, _previousContentPreferredHeight) == false;
    }

    private void RebuildContentLayout()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(_contentRect);
    }

    private void ScrollToBottom()
    {
        _scrollRect.verticalNormalizedPosition = 0f;
    }

    private void StopHeightWatchRoutine()
    {
        if (_heightWatchRoutine == null)
            return;

        StopCoroutine(_heightWatchRoutine);
        _heightWatchRoutine = null;
    }
}