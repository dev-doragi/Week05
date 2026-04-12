using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlaceableSelectManager : MonoBehaviour
{
    private PlaceObject _currentSelected;
    private Camera _camera;

    private void Awake() => _camera = Camera.main;

    private void OnEnable() => PlaceObject.OnSelected += HandleSelected;
    private void OnDisable() => PlaceObject.OnSelected -= HandleSelected;

    private void Update()
    {
        HandleDelete();
        HandleSelectCancel();
    }

    #region Select
    private void HandleSelected(PlaceObject selected)
    {
        if (_currentSelected != null && _currentSelected != selected)
        {
            _currentSelected.SetSelect(false);
        }

        _currentSelected = selected;
    }
    #endregion

    #region Select Cancel
    private void HandleSelectCancel()
    {
        if (Pointer.current != null &&
            Pointer.current.press.wasPressedThisFrame)
        {
            // If ui selected cancel
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

            Vector2 screenPos = Pointer.current.position.ReadValue();
            Vector2 worldPos = _camera.ScreenToWorldPoint(screenPos);

            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

            if (hit.collider == null) SelectCancel();
            else
            {
                PlaceObject hitObj = hit.collider.GetComponent<PlaceObject>();
                if (hitObj == null) SelectCancel();
            }
        }
    }

    private void SelectCancel()
    {
        if (_currentSelected == null) return;

        _currentSelected.SetSelect(false);
        _currentSelected = null;
    }
    #endregion

    #region Delete
    private void HandleDelete()
    {
        if (Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (_currentSelected != null && !_currentSelected.IsPlaced)
            {
                _currentSelected.DeleteObject();
                _currentSelected = null;

                Debug.Log("[Placeable Select Manager] Selected Object Deleted");
            }
        }
    }
    #endregion
}
