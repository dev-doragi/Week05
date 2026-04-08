using UnityEngine;

public class FlyPillar : MonoBehaviour
{
    [Header("Pillar")]
    public Transform top;
    public Transform bottom;

    public void Init(float gap)
    {
        top.localPosition = new Vector2(0, gap / 2);
        bottom.localPosition = new Vector2(0, -gap / 2);
    }
}
