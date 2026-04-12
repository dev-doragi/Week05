using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlaceObject : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    // Event
    public event Action OnStateChanged;
    public static event Action<PlaceObject> OnSelected;

    [Header("Match Settings")]
    public string objectKey;

    [Header("Match State")]
    [SerializeField] private bool _isPlaced = false;
    [SerializeField] private bool _isSelected = false;
    public bool IsPlaced => _isPlaced;
    public bool IsSelected => _isSelected;


    private Vector3 _offset;
    private Camera _camera;
    private Collider2D _collider;


    private void Awake()
    {
        _camera = Camera.main;
        _collider = GetComponent<Collider2D>();
    }


    private void Start()
    {
        Init();
    }

    public void Init()
    {
        _isPlaced = false;
        _isSelected = false;

        if (_collider != null) _collider.isTrigger = true;
        OnStateChanged?.Invoke();
    }

    public void SetSelect(bool select)
    {
        if (_isPlaced) return;
        _isSelected = select;

        if (select) OnSelected?.Invoke(this);
        OnStateChanged?.Invoke();
    }

    public void DeleteObject()
    {
        Destroy(gameObject);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_isPlaced) return;
        SetSelect(true);

        // Moment of click, Mouse Position
        _offset = transform.position - GetMouseWorldPos();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_isPlaced) return;

        // While dragging, Update position to mouse
        transform.position = GetMouseWorldPos() + _offset;
    }

    // Change Match standard to Collider
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
        _isSelected = false;
        transform.position = pos;

        if (_collider != null) _collider.isTrigger = false;
        OnStateChanged?.Invoke();
    }
}
