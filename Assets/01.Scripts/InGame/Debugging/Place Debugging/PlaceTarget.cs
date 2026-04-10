using System;
using UnityEngine;

public class PlaceTarget : MonoBehaviour
{
    [SerializeField] private PlaceDebugging manager;
    public float snapDistance = 1f;

    public event Action OnMatch;

    private bool _isMatch = false;
    public bool IsMatch => _isMatch;


    private void OnEnable()
    {
        if (manager == null) return;

        foreach (var obj in manager.objects) obj.OnDrop += HandleDrop;
    }

    public void Init()
    {
        _isMatch = false;
    }

    private void HandleDrop(PlaceObject droppedObj)
    {
        if (_isMatch || droppedObj.IsPlaced) return;

        float dist = Vector2.Distance(transform.position, droppedObj.transform.position);
        if (dist < snapDistance)
        {
            _isMatch = true;
            droppedObj.SnapTo(transform.position);

            OnMatch?.Invoke();
        }
    }

    private void OnDisable()
    {
        if (manager != null)
        {
            foreach (var obj in manager.objects)
            {
                if (obj != null) obj.OnDrop -= HandleDrop;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(transform.position, snapDistance);
    }
}
