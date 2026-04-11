using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlaceObject : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [Tooltip("배치 오브젝트를 놔둘때 이벤트")]
    public static event Action OnDrop;

    [Header("Match Settings")]
    public string objectKey;

    [Header("Match State")]
    [SerializeField] private bool _isPlaced = false;
    private Vector3 _offset;
    private Camera _camera;
    private Collider2D _collider;

    public bool IsPlaced => _isPlaced;

    private void Start()
    {
        Init();
    }

    public void Init()
    {
        _isPlaced = false;
        _camera = Camera.main;
        _collider = GetComponent<Collider2D>();
        if (_collider != null) _collider.isTrigger = true;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_isPlaced) return;

        // 클릭한 순간의 거리
        _offset = transform.position - GetMouseWorldPos();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_isPlaced) return;

        // 드래그 하는 동안 마우스 따라 이동
        transform.position = GetMouseWorldPos() + _offset;
    }

    /*
    public void OnPointerUp(PointerEventData eventData)
    {
        if (_isPlaced) return;

        OnDrop?.Invoke();
    }
    */

    private Vector3 GetMouseWorldPos()
    {
        Vector3 screenPos = Pointer.current.position.ReadValue();

        float depth = Mathf.Abs(_camera.transform.position.z);
        Vector3 mousePos = new Vector3(screenPos.x, screenPos.y, depth);

        return _camera.ScreenToWorldPoint(mousePos);
    }

    public void SnapTo(Vector2 pos)
    {
        _isPlaced = true;

        transform.position = pos;
        _collider.isTrigger = false;
    }
}
