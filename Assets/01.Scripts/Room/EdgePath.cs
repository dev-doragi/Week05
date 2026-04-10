using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EdgePath : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private RoomID _from;
    [SerializeField] private RoomID _to;
    [SerializeField] private GimmickManager _gimmickManager;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_gimmickManager == null)
            return;

        bool success = _gimmickManager.TryBlockPath(_from, _to);
        Debug.Log($"BlockPath {_from} <-> {_to} : {success}");
    }
}