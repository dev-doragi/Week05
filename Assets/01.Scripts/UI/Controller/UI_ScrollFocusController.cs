using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UI_ScrollFocusController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ScrollRect _scrollRect;
    [SerializeField] private RectTransform _contentRect;

    private Coroutine _focusRoutine;

    public void Focus(RectTransform targetRect)
    {
        if (CanFocus(targetRect) == false)
            return;

        StopFocusRoutine();
        _focusRoutine = StartCoroutine(FocusRoutine(targetRect));
    }

    private IEnumerator FocusRoutine(RectTransform targetRect)
    {
        yield return null;
        yield return null;

        if (CanFocus(targetRect) == false)
            yield break;

        RebuildLayout();
        MoveContentToShowTarget(targetRect);
    }

    private bool CanFocus(RectTransform targetRect)
    {
        if (_scrollRect == null)
            return false;

        if (_contentRect == null)
            return false;

        if (targetRect == null)
            return false;

        if (targetRect.gameObject.activeInHierarchy == false)
            return false;

        return targetRect.IsChildOf(_contentRect);
    }

    private void MoveContentToShowTarget(RectTransform targetRect)
    {
        RectTransform viewportRect = GetViewportRect();

        float viewportTop = viewportRect.rect.yMax;
        float viewportBottom = viewportRect.rect.yMin;

        float targetTop = GetTopInViewportSpace(targetRect, viewportRect);
        float targetBottom = GetBottomInViewportSpace(targetRect, viewportRect);

        Vector2 nextAnchoredPosition = _contentRect.anchoredPosition;

        if (targetTop > viewportTop)
        {
            nextAnchoredPosition.y -= targetTop - viewportTop;
        }
        else if (targetBottom < viewportBottom)
        {
            nextAnchoredPosition.y += viewportBottom - targetBottom;
        }

        nextAnchoredPosition.y = ClampVerticalPosition(nextAnchoredPosition.y, viewportRect);

        _scrollRect.StopMovement();
        _contentRect.anchoredPosition = nextAnchoredPosition;
    }

    private float GetTopInViewportSpace(RectTransform targetRect, RectTransform viewportRect)
    {
        Vector3[] corners = new Vector3[4];
        targetRect.GetWorldCorners(corners);

        Vector3 topLeft = viewportRect.InverseTransformPoint(corners[1]);
        Vector3 topRight = viewportRect.InverseTransformPoint(corners[2]);

        return Mathf.Max(topLeft.y, topRight.y);
    }

    private float GetBottomInViewportSpace(RectTransform targetRect, RectTransform viewportRect)
    {
        Vector3[] corners = new Vector3[4];
        targetRect.GetWorldCorners(corners);

        Vector3 bottomLeft = viewportRect.InverseTransformPoint(corners[0]);
        Vector3 bottomRight = viewportRect.InverseTransformPoint(corners[3]);

        return Mathf.Min(bottomLeft.y, bottomRight.y);
    }

    private float ClampVerticalPosition(float positionY, RectTransform viewportRect)
    {
        float maxScrollY = Mathf.Max(0f, _contentRect.rect.height - viewportRect.rect.height);
        return Mathf.Clamp(positionY, 0f, maxScrollY);
    }

    private void RebuildLayout()
    {
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(_contentRect);
    }

    private RectTransform GetViewportRect()
    {
        if (_scrollRect.viewport != null)
            return _scrollRect.viewport;

        return _scrollRect.transform as RectTransform;
    }

    private void OnDisable()
    {
        StopFocusRoutine();
    }

    private void StopFocusRoutine()
    {
        if (_focusRoutine == null)
            return;

        StopCoroutine(_focusRoutine);
        _focusRoutine = null;
    }

#if UNITY_EDITOR
    private void Reset()
    {
        _scrollRect = GetComponent<ScrollRect>();

        if (_scrollRect != null)
            _contentRect = _scrollRect.content;
    }
#endif
}
