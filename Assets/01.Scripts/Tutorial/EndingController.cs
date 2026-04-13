using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class EndingController : MonoBehaviour
{
    [System.Serializable]
    private struct RectPose
    {
        public float left;
        public float top;
        public float right;
        public float bottom;
        public Vector3 scale;
    }

    [Header("Refs")]
    [SerializeField] private RectTransform targetRect;
    [SerializeField] private GameObject dialogueRoot;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private GameObject buildProfileWindow;

    [Header("Flow")]
    [SerializeField] private bool autoStartOnEnable = true;
    [SerializeField] private float typeCharInterval = 0.03f;
    [SerializeField] private Ease tweenEase = Ease.InOutSine;

    [Header("Poses")]
    [SerializeField] private RectPose poseA = new RectPose
    {
        left = -781f,
        top = 267f,
        right = 781f,
        bottom = -267f,
        scale = new Vector3(2.6402f, 2.6402f, 2.6402f)
    };

    [SerializeField] private RectPose poseB = new RectPose
    {
        left = -247f,
        top = 175f,
        right = 247f,
        bottom = -175f,
        scale = new Vector3(1.297615f, 1.297615f, 1.297615f)
    };

    [SerializeField] private RectPose poseC = new RectPose
    {
        left = 287f,
        top = 175f,
        right = -287f,
        bottom = -175f,
        scale = new Vector3(1.297615f, 1.297615f, 1.297615f)
    };

    [Header("Durations")]
    [SerializeField] private float toPoseBDuration = 2f;
    [SerializeField] private float toPoseCDuration = 1.2f;
    [SerializeField] private float returnToPoseADuration = 2f;

    [Header("Lines")]
    [TextArea(2, 4)]
    [SerializeField] private string[] lines =
    {
        "잘 돼가요? 빌드는 다 됐죠?",
        "리뷰 시작할게요, 빌드 눌러보세요."
    };

    private bool _running;
    private bool _dialogueActive;
    private bool _isTyping;
    private int _lineIndex = -1;

    private Coroutine _typingRoutine;
    private Sequence _activeTween;

    private void OnEnable()
    {
        if (autoStartOnEnable)
            StartEnding();
    }

    public void StartEnding()
    {
        if (_running) return;
        _running = true;

        if (buildProfileWindow != null) buildProfileWindow.SetActive(false);
        if (dialogueRoot != null) dialogueRoot.SetActive(false);
        if (dialogueText != null) dialogueText.text = string.Empty;

        ApplyPoseInstant(poseA);
        StartCoroutine(CoStartFlow());
    }

    private IEnumerator CoStartFlow()
    {
        yield return PlayPoseTween(poseB, toPoseBDuration);
        yield return PlayPoseTween(poseC, toPoseCDuration);

        if (dialogueRoot != null) dialogueRoot.SetActive(true);

        _dialogueActive = true;
        _lineIndex = 0;
        StartTypingCurrentLine();
    }

    private void Update()
    {
        if (!_dialogueActive) return;
        if (Mouse.current == null) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
            Click();
    }

    public void Click()
    {
        if (!_dialogueActive) return;

        if (_isTyping)
        {
            CompleteTypingImmediately();
            return;
        }

        _lineIndex++;

        if (_lineIndex < lines.Length)
        {
            StartTypingCurrentLine();
            return;
        }

        _dialogueActive = false;
        StartCoroutine(CoFinishFlow());
    }

    private IEnumerator CoFinishFlow()
    {
        if (dialogueRoot != null) dialogueRoot.SetActive(false);

        yield return PlayPoseTween(poseA, returnToPoseADuration);

        if (buildProfileWindow != null)
            buildProfileWindow.SetActive(true);
    }

    private void StartTypingCurrentLine()
    {
        if (lines == null || _lineIndex < 0 || _lineIndex >= lines.Length)
            return;

        if (_typingRoutine != null)
            StopCoroutine(_typingRoutine);

        _typingRoutine = StartCoroutine(CoType(lines[_lineIndex]));
    }

    private IEnumerator CoType(string line)
    {
        _isTyping = true;
        if (dialogueText != null) dialogueText.text = string.Empty;

        for (int i = 0; i < line.Length; i++)
        {
            if (dialogueText != null) dialogueText.text += line[i];
            yield return new WaitForSeconds(typeCharInterval);
        }

        _isTyping = false;
        _typingRoutine = null;
    }

    private void CompleteTypingImmediately()
    {
        if (!_isTyping) return;

        if (_typingRoutine != null)
        {
            StopCoroutine(_typingRoutine);
            _typingRoutine = null;
        }

        if (dialogueText != null && lines != null && _lineIndex >= 0 && _lineIndex < lines.Length)
            dialogueText.text = lines[_lineIndex];

        _isTyping = false;
    }

    private IEnumerator PlayPoseTween(RectPose to, float duration)
    {
        if (targetRect == null || duration <= 0f)
        {
            ApplyPoseInstant(to);
            yield break;
        }

        RectPose from = CaptureCurrentPose();
        float t = 0f;

        if (_activeTween != null && _activeTween.IsActive())
            _activeTween.Kill();

        _activeTween = DOTween.Sequence();
        _activeTween.Join(DOTween.To(() => t, x =>
        {
            t = x;
            ApplyPoseLerp(from, to, t);
        }, 1f, duration).SetEase(tweenEase));

        yield return _activeTween.WaitForCompletion();
    }

    private RectPose CaptureCurrentPose()
    {
        return new RectPose
        {
            left = targetRect.offsetMin.x,
            bottom = targetRect.offsetMin.y,
            right = -targetRect.offsetMax.x,
            top = -targetRect.offsetMax.y,
            scale = targetRect.localScale
        };
    }

    private void ApplyPoseInstant(RectPose pose)
    {
        if (targetRect == null) return;

        Vector2 min = targetRect.offsetMin;
        Vector2 max = targetRect.offsetMax;

        min.x = pose.left;
        min.y = pose.bottom;
        max.x = -pose.right;
        max.y = -pose.top;

        targetRect.offsetMin = min;
        targetRect.offsetMax = max;
        targetRect.localScale = pose.scale;
    }

    private void ApplyPoseLerp(RectPose from, RectPose to, float t)
    {
        if (targetRect == null) return;

        Vector2 min = targetRect.offsetMin;
        Vector2 max = targetRect.offsetMax;

        min.x = Mathf.Lerp(from.left, to.left, t);
        min.y = Mathf.Lerp(from.bottom, to.bottom, t);

        float right = Mathf.Lerp(from.right, to.right, t);
        float top = Mathf.Lerp(from.top, to.top, t);

        max.x = -right;
        max.y = -top;

        targetRect.offsetMin = min;
        targetRect.offsetMax = max;
        targetRect.localScale = Vector3.Lerp(from.scale, to.scale, t);
    }

    private void OnDisable()
    {
        if (_typingRoutine != null) StopCoroutine(_typingRoutine);
        if (_activeTween != null && _activeTween.IsActive()) _activeTween.Kill();

        _typingRoutine = null;
        _activeTween = null;
    }
}
