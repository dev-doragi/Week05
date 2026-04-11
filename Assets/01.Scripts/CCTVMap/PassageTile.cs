using UnityEngine;
using System.Collections;

public class PassageTile : MonoBehaviour, IMinimapHoverTarget
{
    [Header("Path")]
    [SerializeField] private RoomID fromRoom = RoomID.None;
    [SerializeField] private RoomID toRoom = RoomID.None;

    [Header("Visual")]
    [SerializeField] private Renderer[] renderers;
    [SerializeField] private Material openMaterial;
    [SerializeField] private Material blockedMaterial;

    [SerializeField] private Material hoverMaterial;
    
    [Header("Blink")]
    [SerializeField] private Material coachBlinkMaterial;
    [SerializeField] private float coachBlinkInterval = 0.2f;
    private bool _isHovered;

    private bool _isBlockedVisual;

    private bool _isCoachBlinking;
    private bool _blinkOn;
    private Coroutine _blinkRoutine;

    public RoomID FromRoom => fromRoom;
    public RoomID ToRoom => toRoom;

    private void Awake()
    {

        SetBlockedVisual(false);
    }

    private void Update()
    {
        RefreshVisualFromManager();
    }

    private void OnDisable()
    {
        SetCoachBlink(false);
    }
    public void OnMinimapClicked()
    {
        bool success = GimmickManager.Instance.TryBlockPath(fromRoom, toRoom);

        if (success)
        {
            Debug.Log($"[PassageTile] BLOCKED: {fromRoom} <-> {toRoom}");
        }
        else
        {
            Debug.Log($"[PassageTile] 실패(쿨다운/이동중). 남은쿨: {GimmickManager.Instance.BlockPathCooldownRemaining:0.00}s");
        }

        RefreshVisualFromManager();
    }

    private void RefreshVisualFromManager()
    {

        bool blockedNow = GimmickManager.Instance.IsPathBlocked(fromRoom, toRoom);
        if (blockedNow == _isBlockedVisual) return;

        SetBlockedVisual(blockedNow);
    }

    private void SetBlockedVisual(bool blocked)
    {
        _isBlockedVisual = blocked;

        if (_isBlockedVisual && _isCoachBlinking)
        {
            SetCoachBlink(false);
            return;
        }

        RefreshVisual();
    }


    private void ApplyMaterial(Material targetMat)
    {
        if (renderers == null || targetMat == null) return;

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer r = renderers[i];
            if (r == null) continue;
            r.sharedMaterial = targetMat;
        }
    }
    public void SetHovered(bool hovered)
    {
        if (_isHovered == hovered) return;
        _isHovered = hovered;
        RefreshVisual();
    }

    private void RefreshVisual()
    {
        Material target =
            _isBlockedVisual ? blockedMaterial :
            (_isCoachBlinking && _blinkOn && coachBlinkMaterial != null) ? coachBlinkMaterial :
            (_isHovered && hoverMaterial != null) ? hoverMaterial :
            openMaterial;

        ApplyMaterial(target);
    }


    public bool Connects(RoomID a, RoomID b)
    {
        return (fromRoom == a && toRoom == b) || (fromRoom == b && toRoom == a);
    }

    public void SetCoachBlink(bool active)
    {
        if (_isBlockedVisual) active = false; // 막힌 통로는 Blink 안 함
        if (_isCoachBlinking == active) return;

        _isCoachBlinking = active;

        if (_blinkRoutine != null)
        {
            StopCoroutine(_blinkRoutine);
            _blinkRoutine = null;
        }

        _blinkOn = false;

        if (_isCoachBlinking)
            _blinkRoutine = StartCoroutine(Co_Blink());

        RefreshVisual();
    }

    private IEnumerator Co_Blink()
    {
        while (true)
        {
            _blinkOn = !_blinkOn;
            RefreshVisual();
            yield return new WaitForSeconds(coachBlinkInterval);
        }
    }


}
