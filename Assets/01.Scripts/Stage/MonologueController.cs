using UnityEngine;
using TMPro;
using System.Collections;

public class MonologueController : MonoBehaviour
{
    public TextMeshProUGUI messageText;

    [Header("Animation")]
    public float slideOutDuration = 0.3f;
    public float slideInDuration = 0.5f;

    private RectTransform rect;
    private float shownY;
    private float hiddenY;
    private bool hasReceived = false;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        shownY = rect.anchoredPosition.y;
        hiddenY = shownY + rect.rect.height + 5f;

        // 처음엔 숨김
        rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, hiddenY);
    }

    void OnEnable() => Stage.OnSendMessage += HandleMessage;
    void OnDisable() => Stage.OnSendMessage -= HandleMessage;

    void HandleMessage(string msg)
    {
        StopAllCoroutines();
        StartCoroutine(hasReceived ? SwapRoutine(msg) : FirstRoutine(msg));
        hasReceived = true;
    }

    // 첫 메시지 — 그냥 바로 내려오기
    IEnumerator FirstRoutine(string msg)
    {
        messageText.text = msg;
        yield return Move(hiddenY, shownY, slideInDuration, EaseOutQuart);
    }

    // 이후 메시지 — 올라갔다가 텍스트 바꾸고 내려오기
    IEnumerator SwapRoutine(string msg)
    {
        yield return Move(shownY, hiddenY, slideOutDuration, EaseInQuart);
        messageText.text = msg;
        yield return Move(hiddenY, shownY, slideInDuration, EaseOutQuart);
    }

    IEnumerator Move(float fromY, float toY, float duration, System.Func<float, float> ease)
    {
        float elapsed = 0f;
        float x = rect.anchoredPosition.x;

        while (elapsed < duration)
        {
            float t = ease(elapsed / duration);
            rect.anchoredPosition = new Vector2(x, Mathf.Lerp(fromY, toY, t));
            elapsed += Time.deltaTime;
            yield return null;
        }

        rect.anchoredPosition = new Vector2(x, toY);
    }

    float EaseOutQuart(float t) => 1f - Mathf.Pow(1f - t, 4f);
    float EaseInQuart(float t) => t * t * t * t;
}