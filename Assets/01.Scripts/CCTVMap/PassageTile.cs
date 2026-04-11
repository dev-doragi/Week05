using UnityEngine;
using System.Collections;

public class PassageTile : MonoBehaviour, IMinimapHoverTarget
{
    [Header("Path")]
    [SerializeField] private RoomTile fromRoomTile;
    [SerializeField] private RoomTile toRoomTile;

    private RoomID FromRoom => fromRoomTile != null ? fromRoomTile.RoomId : RoomID.None;
    private RoomID ToRoom => toRoomTile != null ? toRoomTile.RoomId : RoomID.None;
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
       RoomID from = FromRoom;
        RoomID to = ToRoom;

        if (from == RoomID.None || to == RoomID.None)
        {
            Debug.LogWarning($"[PassageTile] from/to RoomTile 연결 필요: {name}");
            return;
        }

        bool success = GimmickManager.Instance.TryBlockPath(from, to);

        RefreshVisualFromManager();
    }

    private void RefreshVisualFromManager()
    {

        RoomID from = FromRoom;
        RoomID to = ToRoom;
        if (from == RoomID.None || to == RoomID.None) return;

        bool blockedNow = GimmickManager.Instance.IsPathBlocked(from, to);
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
        RoomID from = FromRoom;
        RoomID to = ToRoom;
        return (from == a && to == b) || (from == b && to == a);
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
