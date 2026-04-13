using UnityEngine;

public class PlaceVisual : MonoBehaviour
{
    public enum VisualType
    {
        Object,
        Target
    }

    [Header("Type Settings")]
    [SerializeField] private VisualType _type;

    [Header("References")]
    [SerializeField] private PlaceObject _placeObject;
    [SerializeField] private PlaceTarget _placeTarget;
    [SerializeField] private SpriteRenderer _sprite;

    [Header("Object Settings")]
    [SerializeField] private Material _normalMaterial;
    [SerializeField] private Material _selectedMaterial;
    [SerializeField] private Material _placedMaterial;

    [Header("Target Settings")]
    [SerializeField] private float _pulseSpeed;
    [SerializeField] private float _minAlpha;
    [SerializeField] private float _maxAlpha;


    private void Awake()
    {
        if (_sprite == null) _sprite = GetComponent<SpriteRenderer>();

        if (_type == VisualType.Object) if (_placeObject == null) _placeObject = GetComponent<PlaceObject>();
        else if (_type == VisualType.Target) if (_placeTarget == null) _placeTarget = GetComponent<PlaceTarget>();
    }

    private void OnEnable()
    {
        if (_placeObject != null && _type == VisualType.Object)
        {
            _placeObject.OnStateChanged += UpdateObjectVisual;
        }
    }

    private void OnDisable()
    {
        if (_placeObject != null && _type == VisualType.Object)
        {
            _placeObject.OnStateChanged -= UpdateObjectVisual;
        }
    }

    private void Start()
    {
        if (_type == VisualType.Object) UpdateObjectVisual();
    }

    private void Update()
    {
        if (_type == VisualType.Target) UpdateTargetVisual();
    }

    private void UpdateObjectVisual()
    {
        if (_placeObject == null) return;
        if (_sprite == null) return;

        // 1. Already placed: Hide outline
        if (_placeObject.IsPlaced)
        {
            _sprite.sharedMaterial = _placedMaterial;

            return;
        }

        // 2. Not Placed: Show outline
        _sprite.gameObject.SetActive(true);
        _sprite.sharedMaterial = _placeObject.IsSelected ? _selectedMaterial : _normalMaterial;
    }

    private void UpdateTargetVisual()
    {
        if (_placeTarget == null || _sprite == null) return;

        if (_placeTarget.IsMatch)   // 1. Already matched: Hide target
        {
            Color c = _sprite.color;
            c.a = 0;
            _sprite.color = c;

            return;
        }
        else                        // 2. Not matched: Pulse alpha
        {
            float alpha = Mathf.PingPong(Time.time * _pulseSpeed, _maxAlpha - _minAlpha) + _minAlpha;
            
            Color c = _sprite.color;
            c.a = alpha;
            _sprite.color = c;
        }
    }
}
