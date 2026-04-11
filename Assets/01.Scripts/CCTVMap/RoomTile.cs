using UnityEngine;

public class RoomTile : MonoBehaviour
{
    [SerializeField] private Renderer[] renderers;
    [SerializeField] private Material normalMaterial;
    [SerializeField] private Material selectedMaterial;

    [Header("CCTV Link")]
    [SerializeField] private RoomID roomId = RoomID.None;
    [Header("Rotate Group")]
    [SerializeField, Range(0, 3)] private int rotateStepIndex = 0;
    public int RotateStepIndex => rotateStepIndex;


    [SerializeField] private RoomSelectionGroup selectionGroup;

    
    public bool IsSelected { get; private set; }

    private void Awake()
    {
        if (selectionGroup == null)
            selectionGroup = GetComponentInParent<RoomSelectionGroup>();

    }

    public void OnMinimapClicked()
    { 
        if (CameraManager.Instance != null)
        CameraManager.Instance.SelectCameraByRoomId(roomId);
        if (selectionGroup != null)
        {
            selectionGroup.SelectRoom(this);
            return;
        }

        SetSelectedVisual(!IsSelected);
    }

    public void SetSelectedVisual(bool selected)
    {
        IsSelected = selected;
        ApplyMaterial(selected ? selectedMaterial : normalMaterial);
    }


    private void ApplyMaterial(Material mat)
    {
        if (renderers == null || mat == null) return;

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer r = renderers[i];
            if (r == null) continue;
            r.sharedMaterial = mat;
        }
    }
}
