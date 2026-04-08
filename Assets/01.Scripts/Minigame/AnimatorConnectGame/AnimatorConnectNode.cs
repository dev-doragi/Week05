using UnityEngine;
using TMPro;

public class AnimatorConnectNode : MonoBehaviour
{
    public TMP_Text label;
    public AnimatorConnectPoint point;

    public void Init(AnimatorConnectGame game, string text, Color color, PointSide side)
    {
        label.text = text;
        point.Init(game, color, side);
    }
}