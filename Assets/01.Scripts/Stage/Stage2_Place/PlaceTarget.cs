using System;
using UnityEngine;

public class PlaceTarget : MonoBehaviour
{
    [Header("Match Settings")]
    public string targetKey;
    [Tooltip("배치 타겟에 오브젝트가 매칭됐을 때 이벤트")]
    public event Action OnMatch;

    //[Tooltip("근처에 놓으면 자동으로 붙을 거리")]
    //[SerializeField] public float snapDistance = 1f;

    [Header("Match State")]
    [SerializeField] private bool _isMatch = false;
    public bool IsMatch => _isMatch;

    /*
    private void OnEnable()
    {
        PlaceObject.OnDrop += HandleDrop;
    }

    private void OnDisable()
    {
        PlaceObject.OnDrop -= HandleDrop;
    }
    */

    public void Init()
    {
        _isMatch = false;
    }

    private void HandleDrop()
    {
        if (_isMatch) return;

        /*
        float dist = Vector2.Distance(transform.position, droppedObj.transform.position);
        if (dist < snapDistance)
        {
            MatchSuccess(droppedObj);
        }
        */
    }

    private void CheckMatch(Collider2D collision)
    {
        if (_isMatch) return;

        PlaceObject obj = collision.GetComponent<PlaceObject>();
        if (obj != null
            && !obj.IsPlaced
            && obj.objectKey == this.targetKey)
        {
            Debug.Log("[Place Target] Enter: " + collision.gameObject);
            MatchSuccess(obj);
        }
    }

    private void MatchSuccess(PlaceObject obj)
    {
        _isMatch = true;
        obj.SnapTo(transform.position);

        OnMatch?.Invoke();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        CheckMatch(collision);
    }

    /*
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(transform.position, snapDistance);
    }
    */
}
