// AnimObject.cs
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class AnimObject : MonoBehaviour
{
    [System.Serializable]
    public class PairData
    {
        public string leftText;
        public string rightText;
    }

    private enum NodeSide
    {
        Left,
        Right
    }

    private class RuntimeNode
    {
        public RectTransform root;
        public RectTransform point;
        public TMP_Text label;
        public Image pointImage;

        public NodeSide side;
        public int pairId;
        public Color color;
        public RuntimeNode linkedNode;

        public bool IsLinked => linkedNode != null;
    }
    [SerializeField] private MissionClearer _missionClearer;

    [Header("Panel")]
    [SerializeField] private AnimPanel _panel;
    [SerializeField] private Canvas _canvas;
    [SerializeField] private RectTransform _boardRoot;
    [SerializeField] private RectTransform _leftRoot;
    [SerializeField] private RectTransform _rightRoot;
    [SerializeField] private RectTransform _lineRoot;

    [Header("Node Prefabs")]
    [SerializeField] private RectTransform _leftNodePrefab;
    [SerializeField] private RectTransform _rightNodePrefab;
    [SerializeField] private RectTransform _linePrefab;

    [Header("Data")]
    [SerializeField] private PairData[] _pairs;

    [Header("Hit")]
    [SerializeField] private float _hitRadius = 40f;
    [SerializeField] private float _lineThickness = 8f;

    [Header("View")]
    [SerializeField] private SpriteRenderer _targetRenderer;
    [SerializeField] private Color _normalColor = Color.white;
    [SerializeField] private Color _clearColor = Color.green;

    private static readonly Color[] ColorPool =
    {
        Color.red,
        new Color(1f, 0.85f, 0f),
        new Color(0.1f, 0.75f, 0.1f),
        new Color(0.3f, 0.6f, 1f),
        new Color(0.9f, 0.4f, 1f),
        new Color(1f, 0.5f, 0f),
    };

    private readonly List<GameObject> _spawned = new();
    private readonly List<RuntimeNode> _leftNodes = new();
    private readonly List<RuntimeNode> _rightNodes = new();

    private RuntimeNode _dragStartNode;
    private RectTransform _dragLine;
    private int _connectedCount;

    public event System.Action Cleared;

    public bool IsCleared { get; private set; }

    public void Init()
    {
        IsCleared = false;

        ClosePanel();
        UpdateView();
    }

    public void OpenPanel(PointerEventData eventData)
    {
        if (IsCleared) return;
        if (_panel == null) return;
        if (_pairs == null || _pairs.Length == 0) return;

        ClosePanel();

        _panel.Show(eventData);
        BuildNodes();
    }

    public void ClosePanel()
    {
        ClearNodes();

        if (_panel != null)
            _panel.Close();
    }

    public void OnClickCloseButton()
    {
        ClosePanel();
    }

    private void Update()
    {
        if (_panel == null) return;
        if (_panel.IsOpen == false) return;
        if (Mouse.current == null) return;

        Vector2 mouseScreen = Mouse.current.position.ReadValue();

        if (Mouse.current.leftButton.wasPressedThisFrame)
            BeginDrag(mouseScreen);

        if (Mouse.current.leftButton.isPressed)
            UpdateDrag(mouseScreen);

        if (Mouse.current.leftButton.wasReleasedThisFrame)
            EndDrag(mouseScreen);
    }

    private void BeginDrag(Vector2 mouseScreen)
    {
        RuntimeNode node = FindNodeAt(_leftNodes, mouseScreen, false);
        if (node == null) return;

        _dragStartNode = node;
        _dragLine = CreateLine(node.color);

        Vector2 start = GetBoardLocalPoint(node.point);
        SetLine(_dragLine, start, start);
    }

    private void UpdateDrag(Vector2 mouseScreen)
    {
        if (_dragStartNode == null) return;
        if (_dragLine == null) return;

        Vector2 start = GetBoardLocalPoint(_dragStartNode.point);
        Vector2 end = GetMouseBoardLocalPoint(mouseScreen);

        SetLine(_dragLine, start, end);
    }

    private void EndDrag(Vector2 mouseScreen)
    {
        if (_dragStartNode == null || _dragLine == null)
        {
            _dragStartNode = null;
            _dragLine = null;
            return;
        }

        RuntimeNode target = FindNodeAt(_rightNodes, mouseScreen, true);

        if (CanConnect(_dragStartNode, target))
            Connect(_dragStartNode, target);
        else
            RemoveLine(_dragLine);

        _dragStartNode = null;
        _dragLine = null;
    }

    private void BuildNodes()
    {
        ClearNodes();

        int count = _pairs.Length;
        int[] leftOrder = Shuffle(count);
        int[] rightOrder = Shuffle(count);

        for (int i = 0; i < count; i++)
        {
            Color color = ColorPool[i % ColorPool.Length];

            SpawnNode(
                _leftNodePrefab,
                _leftRoot,
                _pairs[i].leftText,
                color,
                NodeSide.Left,
                i,
                _leftNodes,
                leftOrder[i]
            );

            SpawnNode(
                _rightNodePrefab,
                _rightRoot,
                _pairs[i].rightText,
                color,
                NodeSide.Right,
                i,
                _rightNodes,
                rightOrder[i]
            );
        }
    }

    private void SpawnNode(
        RectTransform prefab,
        RectTransform parent,
        string text,
        Color color,
        NodeSide side,
        int pairId,
        List<RuntimeNode> list,
        int siblingIndex)
    {
        if (prefab == null) return;
        if (parent == null) return;

        RectTransform root = Instantiate(prefab, parent);
        root.SetSiblingIndex(siblingIndex);

        TMP_Text label = root.GetComponentInChildren<TMP_Text>(true);

        Transform pointTransform = root.Find("Point");
        RectTransform point = pointTransform as RectTransform;
        Image pointImage = point != null ? point.GetComponent<Image>() : null;

        if (label != null)
            label.text = text;

        if (pointImage != null)
            pointImage.color = color;

        RuntimeNode node = new RuntimeNode
        {
            root = root,
            point = point,
            label = label,
            pointImage = pointImage,
            side = side,
            pairId = pairId,
            color = color,
            linkedNode = null
        };

        _spawned.Add(root.gameObject);
        list.Add(node);
    }

    private RuntimeNode FindNodeAt(List<RuntimeNode> nodes, Vector2 mouseScreen, bool skipLinked)
    {
        RuntimeNode best = null;
        float bestDistance = float.MaxValue;

        foreach (RuntimeNode node in nodes)
        {
            if (node == null) continue;
            if (node.point == null) continue;
            if (skipLinked && node.IsLinked) continue;

            Vector2 pointScreen = RectTransformUtility.WorldToScreenPoint(
                GetEventCamera(),
                node.point.position
            );

            float distance = Vector2.Distance(pointScreen, mouseScreen);

            if (distance > _hitRadius) continue;
            if (distance >= bestDistance) continue;

            best = node;
            bestDistance = distance;
        }

        return best;
    }

    private bool CanConnect(RuntimeNode leftNode, RuntimeNode rightNode)
    {
        if (leftNode == null) return false;
        if (rightNode == null) return false;
        if (rightNode.IsLinked) return false;

        return leftNode.pairId == rightNode.pairId;
    }

    private void Connect(RuntimeNode leftNode, RuntimeNode rightNode)
    {
        leftNode.linkedNode = rightNode;
        rightNode.linkedNode = leftNode;

        Vector2 start = GetBoardLocalPoint(leftNode.point);
        Vector2 end = GetBoardLocalPoint(rightNode.point);

        SetLine(_dragLine, start, end);

        _connectedCount++;

        if (_connectedCount >= _pairs.Length)
            CompletePuzzle();
    }

    private void CompletePuzzle()
    {
        IsCleared = true;
        Cleared?.Invoke();

        _missionClearer.ClearMission();
        ClosePanel();
        UpdateView();
    }

    private RectTransform CreateLine(Color color)
    {
        if (_linePrefab == null) return null;
        if (_lineRoot == null) return null;

        RectTransform line = Instantiate(_linePrefab, _lineRoot);

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
        if (line == null) return;

        Vector2 dir = end - start;
        float length = dir.magnitude;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        line.anchoredPosition = (start + end) * 0.5f;
        line.sizeDelta = new Vector2(length, _lineThickness);
        line.localRotation = Quaternion.Euler(0f, 0f, angle);
    }

    private Vector2 GetBoardLocalPoint(RectTransform target)
    {
        if (_boardRoot == null || target == null)
            return Vector2.zero;

        return _boardRoot.InverseTransformPoint(target.position);
    }

    private Vector2 GetMouseBoardLocalPoint(Vector2 mouseScreen)
    {
        if (_boardRoot == null)
            return Vector2.zero;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _boardRoot,
            mouseScreen,
            GetEventCamera(),
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

    private void ClearNodes()
    {
        foreach (GameObject obj in _spawned)
        {
            if (obj != null)
                Destroy(obj);
        }

        _spawned.Clear();
        _leftNodes.Clear();
        _rightNodes.Clear();

        _dragStartNode = null;
        _dragLine = null;
        _connectedCount = 0;
    }

    private void UpdateView()
    {
        if (_targetRenderer == null) return;
        _targetRenderer.color = IsCleared ? _clearColor : _normalColor;
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
