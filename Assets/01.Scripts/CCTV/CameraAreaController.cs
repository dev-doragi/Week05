using UnityEngine;

public class CameraAreaController : MonoBehaviour
{
    [Header("Room Data")]
    [SerializeField] private RoomID _roomId = RoomID.None;
    [SerializeField] private Texture _roomBackgroundTexture;
    [SerializeField] private Texture _roomBackgroundWithCoachTexture;

    public RoomID RoomId => _roomId;

    public Texture GetBackgroundTexture(bool hasCoach)
    {
        if (hasCoach && _roomBackgroundWithCoachTexture != null)
            return _roomBackgroundWithCoachTexture;

        return _roomBackgroundTexture;
    }

    // 현재 미니맵 클릭 구조에서는 안 쓰지만 CameraManager 호환용으로 유지
    // public void SetBlinkState(BlinkState blinkState)
    // {
    // }
}
