using UnityEngine;

public class PlaceVisual : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlaceObject _placeObject;
    [SerializeField] private SpriteRenderer _outlineSprite;

    [Header("Outline Settings")]
    [SerializeField] private Material _normalMaterial;
    [SerializeField] private Material _selectedMaterial;
    [SerializeField] private Material _placedMaterial;


    private void Awake()
    {
        if (_placeObject == null) _placeObject = GetComponent<PlaceObject>();
        if (_outlineSprite == null) _outlineSprite = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        if (_placeObject != null)
        {
            _placeObject.OnStateChanged += UpdateVisual;
        }
    }

    private void OnDisable()
    {
        if (_placeObject != null)
        {
            _placeObject.OnStateChanged -= UpdateVisual;
        }
    }

    private void Start()
    {
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (_placeObject == null) return;
        if (_outlineSprite == null) return;

        // 1. Already placed: Hide outline
        if (_placeObject.IsPlaced)
        {
            _outlineSprite.sharedMaterial = _placedMaterial;

            return;
        }

        // 2. Not Placed: Show outline
        _outlineSprite.gameObject.SetActive(true);
        _outlineSprite.sharedMaterial = _placeObject.IsSelected ? _selectedMaterial : _normalMaterial;
    }
}
