// AnimPanel.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class AnimPanel : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Canvas _canvas;
    [SerializeField] private RectTransform _panelRoot;
    [SerializeField] private RectTransform _boardRoot;

    [Header("Prefabs")]
    [SerializeField] private AnimNode _leftNodePrefab;
    [SerializeField] private AnimNode _rightNodePrefab;
    [SerializeField] private RectTransform _linePrefab;

    [Header("Layout")]
    [SerializeField] private float _leftX = -260f;
    [SerializeField] private float _rightX = 260f;
    [SerializeField] private float _startY = 0f;
    [SerializeField] private float _gapY = 90f;

    [Header("Hit")]
    [SerializeField] private float _hitRadius = 40f;
    [SerializeField] private float _lineThickness = 8f;

    private static readonly Color[] Colors =
    {
        Color.red,
        new Color(1f, 0.85f, 0f),
        new Color(0.1f, 0.75f, 0.1f),
        new Color(0.3f, 0.6f, 1f),
        new Color(0.9f, 0.4f, 1f),
        new Color(1f, 0.5f, 0f),
    };

    private readonly List<GameObject> _spawned = new();
    private readonly List<AnimNode> _rightNodes = new();

    private AnimTarget _currentTarget;
    private AnimNode _dragStartNode;
    private RectTransform _dragLine;
    private int _pairCount;
    private int _connectedCount;

    public void Open(AnimTarget target, AnimPairData[] pairs, Vector2 panelPos)
    {
        _currentTarget = target;

        _panelRoot.anchoredPosition = panelPos;
        _panelRoot.gameObject.SetActive(true);

        Build(pairs);
    }

    public void Close()
    {
        ClearBoard();
        _panelRoot.gameObject.SetActive(false);
        _currentTarget = null;
    }

    public void OnClickCloseButton()
    {
        Close();
    }

    private void Update()
    {
        if (_dragStartNode == null) return;
        if (_dragLine == null) return;
        if (Mouse.current == null) return;

        Vector2 mousePos = GetMouseBoardPos();
        SetLine(_dragLine, _dragStartNode.GetPointPos(), mousePos);

        if (Mouse.current.leftButton.wasReleasedThisFrame)
            EndDrag(mousePos);
    }

    public void BeginDrag(AnimNode node)
    {
        if (node == null) return;
        if (node.Side != AnimNodeSide.Left) return;
        if (node.IsLinked) return;

        _dragStartNode = node;
        _dragLine = CreateLine(node.NodeColor);

        SetLine(_dragLine, node.GetPointPos(), node.GetPointPos());
    }

    public Vector2 WorldToBoard(Vector3 worldPos)
    {
        return _boardRoot.InverseTransformPoint(worldPos);
    }

    private void Build(AnimPairData[] pairs)
    {
        ClearBoard();

        if (pairs == null || pairs.Length == 0)
            return;

        _pairCount = pairs.Length;

        int[] leftOrder = Shuffle(pairs.Length);
        int[] rightOrder = Shuffle(pairs.Length);

        float totalHeight = (pairs.Length - 1) * _gapY;
        float topY = _startY + totalHeight * 0.5f;

        for (int i = 0; i < pairs.Length; i++)
        {
            Color color = Colors[i % Colors.Length];

            float leftY = topY - leftOrder[i] * _gapY;
            float rightY = topY - rightOrder[i] * _gapY;

            SpawnNode(_leftNodePrefab, pairs[i].leftText, color, AnimNodeSide.Left, i, new Vector2(_leftX, leftY));
            SpawnNode(_rightNodePrefab, pairs[i].rightText, color, AnimNodeSide.Right, i, new Vector2(_rightX, rightY));
        }
    }

    private void SpawnNode(
        AnimNode prefab,
        string text,
        Color color,
        AnimNodeSide side,
        int pairId,
        Vector2 pos)
    {
        AnimNode node = Instantiate(prefab, _boardRoot);
        node.Setup(this, text, color, side, pairId, pos);

        _spawned.Add(node.gameObject);

        if (side == AnimNodeSide.Right)
            _rightNodes.Add(node);
    }

    private void EndDrag(Vector2 mousePos)
    {
        AnimNode rightNode = FindRightNode(mousePos);

        if (CanConnect(_dragStartNode, rightNode))
            Connect(_dragStartNode, rightNode);
        else
            RemoveLine(_dragLine);

        _dragStartNode = null;
        _dragLine = null;
    }

    private bool CanConnect(AnimNode leftNode, AnimNode rightNode)
    {
        if (leftNode == null) return false;
        if (rightNode == null) return false;
        if (rightNode.IsLinked) return false;

        return leftNode.PairId == rightNode.PairId;
    }

    private void Connect(AnimNode leftNode, AnimNode rightNode)
    {
        leftNode.Link(rightNode);
        rightNode.Link(leftNode);

        SetLine(_dragLine, leftNode.GetPointPos(), rightNode.GetPointPos());

        _connectedCount++;

        if (_connectedCount >= _pairCount)
            Solve();
    }

    private void Solve()
    {
        AnimTarget target = _currentTarget;

        Close();

        if (target != null)
            target.Solve();
    }

    private AnimNode FindRightNode(Vector2 mousePos)
    {
        AnimNode best = null;
        float bestDistance = float.MaxValue;

        foreach (AnimNode node in _rightNodes)
        {
            if (node.IsLinked) continue;

            float distance = Vector2.Distance(node.GetPointPos(), mousePos);

            if (distance > _hitRadius) continue;
            if (distance >= bestDistance) continue;

            best = node;
            bestDistance = distance;
        }

        return best;
    }

    private RectTransform CreateLine(Color color)
    {
        RectTransform line = Instantiate(_linePrefab, _boardRoot);

        Image image = line.GetComponent<Image>();
        if (image != null)
            image.color = color;

        _spawned.Add(line.gameObject);
        return line;
    }

    private void RemoveLine(RectTransform line)
    {
        if (line == null) return;

        _spawned.Remove(line.gameObject);
        Destroy(line.gameObject);
    }

    private void SetLine(RectTransform line, Vector2 start, Vector2 end)
    {
        Vector2 dir = end - start;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        float length = dir.magnitude;

        line.anchoredPosition = (start + end) * 0.5f;
        line.sizeDelta = new Vector2(length, _lineThickness);
        line.localRotation = Quaternion.Euler(0f, 0f, angle);
    }

    private Vector2 GetMouseBoardPos()
    {
        Camera eventCamera = GetEventCamera();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _boardRoot,
            Mouse.current.position.ReadValue(),
            eventCamera,
            out Vector2 localPoint
        );

        return localPoint;
    }

    private Camera GetEventCamera()
    {
        if (_canvas == null) return null;
        if (_canvas.renderMode == RenderMode.ScreenSpaceOverlay) return null;

        return _canvas.worldCamera;
    }

    private void ClearBoard()
    {
        foreach (GameObject obj in _spawned)
        {
            if (obj != null)
                Destroy(obj);
        }

        _spawned.Clear();
        _rightNodes.Clear();

        _dragStartNode = null;
        _dragLine = null;
        _pairCount = 0;
        _connectedCount = 0;
    }

    private static int[] Shuffle(int count)
    {
        int[] values = new int[count];

        for (int i = 0; i < count; i++)
            values[i] = i;

        for (int i = count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            (values[i], values[randomIndex]) = (values[randomIndex], values[i]);
        }

        return values;
    }
}
