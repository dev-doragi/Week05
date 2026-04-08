using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum PointSide { Left, Right }

public class AnimatorConnectPoint : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public PointSide side;
    public Color matchColor;
    public AnimatorConnectPoint linkedPoint;

    private AnimatorConnectGame _game;
    private ConnectionLine _draggingLine;

    public void Init(AnimatorConnectGame game, Color color, PointSide pointSide)
    {
        _game = game;
        matchColor = color;
        side = pointSide;
        GetComponent<Image>().color = color;
    }

    public void OnPointerDown(PointerEventData e)
    {
        if (side != PointSide.Left) return;
        if (linkedPoint != null) return;

        Vector3 worldPos = ScreenToWorld(e.position);
        _draggingLine = _game.CreateDraggingLine(this, worldPos);
    }

    public void OnDrag(PointerEventData e)
    {
        if (_draggingLine == null) return;
        _draggingLine.UpdateEnd(ScreenToWorld(e.position));
    }

    public void OnPointerUp(PointerEventData e)
    {
        if (_draggingLine == null) return;

        AnimatorConnectPoint target = _game.FindPointAt(e.position, this);

        if (target != null
            && target.side == PointSide.Right
            && target.matchColor == matchColor
            && target.linkedPoint == null)
        {
            _game.Connect(this, target, _draggingLine);
        }
        else
        {
            _game.DestroyLine(_draggingLine);
        }
        _draggingLine = null;
    }

    Vector3 ScreenToWorld(Vector2 screen)
    {
        Vector3 p = Camera.main.ScreenToWorldPoint(new Vector3(screen.x, screen.y, 10f));
        return p;
    }
}