using UnityEngine;

public enum PointSide { Left, Right }

public class AnimatorConnectPoint : MonoBehaviour
{
    public PointSide side;
    public Color matchColor;
    public AnimatorConnectPoint linkedPoint;

    private AnimatorConnectGame _game;

    public void Init(AnimatorConnectGame game, Color color, PointSide pointSide)
    {
        _game = game;
        matchColor = color;
        side = pointSide;
        GetComponent<SpriteRenderer>().color = color;
    }
}