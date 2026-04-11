using UnityEngine;

public class PassageTile : MonoBehaviour
{
    [Header("Path")]
    [SerializeField] private RoomID fromRoom = RoomID.None;
    [SerializeField] private RoomID toRoom = RoomID.None;

    [Header("Visual")]
    [SerializeField] private Renderer[] renderers;
    [SerializeField] private Material openMaterial;
    [SerializeField] private Material blockedMaterial;

    private bool _isBlockedVisual;

    private void Awake()
    {

        SetBlockedVisual(false);
    }

    private void Update()
    {
        RefreshVisualFromManager();
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
        ApplyMaterial(blocked ? blockedMaterial : openMaterial);
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
}
