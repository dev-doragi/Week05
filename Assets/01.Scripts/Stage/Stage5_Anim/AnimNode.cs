// AnimNode.cs
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum AnimNodeSide
{
    Left,
    Right
}

public class AnimNode : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private TMP_Text _label;
    [SerializeField] private RectTransform _root;
    [SerializeField] private RectTransform _point;
    [SerializeField] private Image _pointImage;

    private AnimPanel _panel;
    private AnimNode _linkedNode;

    public AnimNodeSide Side { get; private set; }
    public int PairId { get; private set; }
    public Color NodeColor { get; private set; }
    public bool IsLinked => _linkedNode != null;

    public void Setup(
        AnimPanel panel,
        string text,
        Color color,
        AnimNodeSide side,
        int pairId,
        Vector2 pos)
    {
        _panel = panel;
        _linkedNode = null;

        Side = side;
        PairId = pairId;
        NodeColor = color;

        _label.text = text;
        _root.anchoredPosition = pos;
        _pointImage.color = color;
    }

    public void Link(AnimNode other)
    {
        _linkedNode = other;
    }

    public Vector2 GetPointPos()
    {
        if (_panel == null) return Vector2.zero;
        return _panel.WorldToBoard(_point.position);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (_panel == null) return;

        _panel.BeginDrag(this);
    }

#if UNITY_EDITOR
    private void Reset()
    {
        _root = GetComponent<RectTransform>();
    }
#endif
}


[Serializable]
public class AnimPairData
{
    public string leftText;
    public string rightText;
}