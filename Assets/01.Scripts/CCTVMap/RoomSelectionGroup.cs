using UnityEngine;
using DG.Tweening;

public class RoomSelectionGroup : MonoBehaviour
{
    [System.Serializable]
    public struct ViewPose
    {
        public Vector3 position;
        public Vector3 rotationEuler;
    }

    [SerializeField] private RoomTile[] rooms;

    [Header("Target")]
    [SerializeField] private Transform rotateTarget; // Archi

    [Header("Tween")]
    [SerializeField] private float moveDuration = 0.35f;
    [SerializeField] private Ease moveEase = Ease.OutCubic;
    [SerializeField] private bool useLocalSpace = true;

    [Header("4 Preset Poses (index 0~3)")]
    [SerializeField] private ViewPose[] poses = new ViewPose[4];

    [Header("Rotate Toggle")]
    [SerializeField] private bool mapRotationEnabled = true;
    public bool MapRotationEnabled => mapRotationEnabled;
    private Tween _moveTween;
    private Tween _rotTween;

    private void Awake()
    {
        if (rotateTarget == null) rotateTarget = transform;
    }

    private void Start()
    {
        if (rooms == null || rooms.Length == 0)
            rooms = GetComponentsInChildren<RoomTile>(true);
    }

    private void OnDisable()
    {
        if (_moveTween != null && _moveTween.IsActive()) _moveTween.Kill();
        if (_rotTween != null && _rotTween.IsActive()) _rotTween.Kill();
    }

    public void SelectRoom(RoomTile target)
    {
        if (target == null) return;

        for (int i = 0; i < rooms.Length; i++)
        {
            RoomTile room = rooms[i];
            if (room == null) continue;
            room.SetSelectedVisual(room == target);
        }

        if (mapRotationEnabled)
            MoveToPose(target.RotateStepIndex);
    }
    public void SetMapRotationEnabled(bool enabled)
    {
        mapRotationEnabled = enabled;

        if (!mapRotationEnabled)
        {
            if (_moveTween != null && _moveTween.IsActive()) _moveTween.Kill();
            if (_rotTween != null && _rotTween.IsActive()) _rotTween.Kill();
        }
    }

    public void ClearSelection()
    {
        for (int i = 0; i < rooms.Length; i++)
        {
            if (rooms[i] == null) continue;
            rooms[i].SetSelectedVisual(false);
        }
    }

    private void MoveToPose(int index)
    {
        if (poses == null || poses.Length == 0) return;

        index = Mathf.Clamp(index, 0, poses.Length - 1);
        ViewPose p = poses[index];

        if (_moveTween != null && _moveTween.IsActive()) _moveTween.Kill();
        if (_rotTween != null && _rotTween.IsActive()) _rotTween.Kill();

        if (useLocalSpace)
        {
            _moveTween = rotateTarget.DOLocalMove(p.position, moveDuration).SetEase(moveEase);
            _rotTween = rotateTarget.DOLocalRotate(p.rotationEuler, moveDuration, RotateMode.Fast).SetEase(moveEase);
        }
        else
        {
            _moveTween = rotateTarget.DOMove(p.position, moveDuration).SetEase(moveEase);
            _rotTween = rotateTarget.DORotate(p.rotationEuler, moveDuration, RotateMode.Fast).SetEase(moveEase);
        }
    }
}
