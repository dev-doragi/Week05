using UnityEngine;
using System;

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

    [Header("Guide Arrows")]
    [SerializeField] private GameObject arrowFromTo;
    [SerializeField] private GameObject arrowToFrom;

    private bool _isHovered;
    private bool _isBlockedVisual;

    public event Action<PassageTile, bool> BlockVisualChanged;

    private void Awake()
    {
        SetBlockedVisual(false);
        HideGuide();
    }

    private void Update()
    {
        RefreshVisualFromManager();
    }

    private void OnDisable()
    {
        HideGuide();
    }

    public void OnMinimapClicked()
    {
        RoomID from = FromRoom;
        RoomID to = ToRoom;
        
        bool success = GimmickManager.Instance.TryBlockPath(from, to);
        Debug.Log($"[PassageTile] Block {from} <-> {to} : {success}");

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
        bool changed = (_isBlockedVisual != blocked);
        _isBlockedVisual = blocked;

        if (_isBlockedVisual)
            HideGuide();

        RefreshVisual();

        if (changed)
            BlockVisualChanged?.Invoke(this, _isBlockedVisual);
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
            (_isHovered && hoverMaterial != null) ? hoverMaterial :
            openMaterial;

        ApplyMaterial(target);
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

    public bool Connects(RoomID a, RoomID b)
    {
        RoomID from = FromRoom;
        RoomID to = ToRoom;
        return (from == a && to == b) || (from == b && to == a);
    }

    // 호환용 (기존 호출 유지)
    public void SetCoachBlink(bool active)
    {
        if (!active || _isBlockedVisual)
        {
            HideGuide();
            return;
        }

        SetGuideObjects(false, false);
    }

    public void ShowGuideFrom(RoomID startRoom)
    {
        if (_isBlockedVisual)
        {
            HideGuide();
            return;
        }

        bool isFrom = (startRoom == FromRoom);
        bool isTo = (startRoom == ToRoom);

        if (!isFrom && !isTo)
        {
            HideGuide();
            return;
        }

        SetGuideObjects(isFrom, isTo);
    }



    public void HideGuide()
    {
        SetGuideObjects(false, false);
    }

    private void SetGuideObjects(bool showFromTo, bool showToFrom)
    {
        if (arrowFromTo != null) arrowFromTo.SetActive(showFromTo);
        if (arrowToFrom != null) arrowToFrom.SetActive(showToFrom);

    }
}
