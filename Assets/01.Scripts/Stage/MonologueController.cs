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

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        shownY = rect.anchoredPosition.y;
        hiddenY = shownY + rect.rect.height;
    }

    void OnEnable() => Stage.OnSendMessage += HandleMessage;
    void OnDisable() => Stage.OnSendMessage -= HandleMessage;

    void HandleMessage(string msg)
    {
        StopAllCoroutines();
        StartCoroutine(SwapRoutine(msg));
    }

    IEnumerator SwapRoutine(string msg)
    {
        // 위로 슥 올라가기
        yield return Move(shownY, hiddenY, slideOutDuration, EaseInQuart);

        // 텍스트 교체
        messageText.text = msg;

        // 아래로 슥 내려오기
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