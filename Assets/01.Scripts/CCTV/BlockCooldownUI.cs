using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BlockCooldownUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image fillImage;

    [Header("Mode")]
    [SerializeField] private bool showReadyAsFull = true;
    [SerializeField] private bool hideWhenReady = false;

    private void Update()
    {
        if (GimmickManager.Instance == null) return;

        float duration = GimmickManager.Instance.BlockPathCooldownDuration;
        float remain = GimmickManager.Instance.BlockPathCooldownRemaining;

        float remain01 = (duration <= 0f) ? 0f : Mathf.Clamp01(remain / duration);
        float value01 = showReadyAsFull ? (1f - remain01) : remain01;

        if (fillImage != null) fillImage.fillAmount = value01;

        if (hideWhenReady)
            gameObject.SetActive(remain > 0f);
    }
}
