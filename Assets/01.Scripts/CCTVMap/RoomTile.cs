using UnityEngine;
using TMPro;

public class RoomTile : MonoBehaviour, IMinimapHoverTarget
{
    [SerializeField] private Renderer[] renderers;
    [SerializeField] private Material normalMaterial;
    [SerializeField] private Material selectedMaterial;
    [SerializeField] private Material hoverMaterial;
    private bool _isHovered;

    [SerializeField] private SO_SFX _cameraClickSfx;
    [SerializeField] private SFXPlayer _sfxPlayer;
    [SerializeField] private float _clickVolume = 1f;

    [Header("CCTV Link")]
    [SerializeField] private RoomID roomId = RoomID.None;
    public RoomID RoomId => roomId;
        
    [Header("Rotate Group")]
    [SerializeField, Range(0, 3)] private int rotateStepIndex = 0;
    public int RotateStepIndex => rotateStepIndex;


    [SerializeField] private RoomSelectionGroup selectionGroup;

    [Header("Label")]
    [SerializeField] private TMP_Text roomIdText;  

    
    public bool IsSelected { get; private set; }

    private void Awake()
    {
        if (selectionGroup == null)
            selectionGroup = GetComponentInParent<RoomSelectionGroup>();
        RefreshRoomIdText();
        RefreshVisual();

    }

    public void OnMinimapClicked()
    { 
        if (CameraManager.Instance != null)
        CameraManager.Instance.SelectCameraByRoomId(roomId);

        if (_sfxPlayer != null && _cameraClickSfx != null)
            _sfxPlayer.Play(_cameraClickSfx, _clickVolume);

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
        RefreshVisual();
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
    #if UNITY_EDITOR
    private void OnValidate()
    {
        RefreshRoomIdText();
    }
    #endif

    private void RefreshRoomIdText()
    {
        if (roomIdText == null) return;

        string value = roomId.ToString();

        if (value.StartsWith("B1F_")) value = value.Substring("B1F_".Length);
        else if (value.StartsWith("F1_")) value = value.Substring("F1_".Length);
        else if (value.StartsWith("F3_")) value = value.Substring("F3_".Length);

        value = value.Replace("Upper", "");
        value = value.Replace("Lower", "");
        value = value.Replace('_', ' ');
        value = value.Trim();

        roomIdText.text = value;
    }

    public void SetHovered(bool hovered)
    {
        if (_isHovered == hovered) return;
        _isHovered = hovered;
        RefreshVisual();
    }

    private void RefreshVisual()
    {
        Material mat = IsSelected? selectedMaterial : ((_isHovered && hoverMaterial != null) ? hoverMaterial : normalMaterial);

        ApplyMaterial(mat);
    }




}
