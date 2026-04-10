using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class MinimapRawImageInteractor3D : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Camera minimapCamera;
    [SerializeField] private LayerMask minimapHitMask = ~0;
    [SerializeField] private float maxDistance = 1000f;
    [SerializeField] private bool debugLog = true;

    private RawImage _rawImage;
    private RectTransform _rect;

    private void Awake()
    {
        _rawImage = GetComponent<RawImage>();
        _rect = _rawImage.rectTransform;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (minimapCamera == null) return;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _rect, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
            return;

        Rect r = _rect.rect;
        if (!r.Contains(localPoint)) return;

        float nx = Mathf.InverseLerp(r.xMin, r.xMax, localPoint.x);
        float ny = Mathf.InverseLerp(r.yMin, r.yMax, localPoint.y);

        Rect uv = _rawImage.uvRect;
        float u = uv.x + nx * uv.width;
        float v = uv.y + ny * uv.height;

        Ray ray = minimapCamera.ViewportPointToRay(new Vector3(u, v, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, minimapHitMask, QueryTriggerInteraction.Collide))
        {

            RoomTile room = hit.collider.GetComponentInParent<RoomTile>();
            if (room != null)
            {
                room.OnMinimapClicked();
                return;
            }

            PassageTile passage = hit.collider.GetComponentInParent<PassageTile>();
            if (passage != null)
            {
                passage.OnMinimapClicked();
                return;
            }

        }
        
    }
}
