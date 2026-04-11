using UnityEngine;

[CreateAssetMenu(fileName = "SO_ToastData", menuName = "Scriptable Objects/SO_ToastData")]
public class SO_ToastData : ScriptableObject
{
    [Header("발신 정보")]
    public Sprite profileIcon;         // 프로필 이미지
    public string workspaceName;       // 워크스페이스 명 (예: krafton-aliens)
    public string senderName;          // 발신자 이름 (예: 게임랩5기_김경근)

    [Header("메시지 내용")]
    [TextArea(3, 10)]
    public string messageContext;      // 알림 본문 내용

    [Header("설정")]
    public Color outlineColor = Color.white; // 알림창 외곽선 색상
    public float displayDuration = 3.0f;     // 알림 유지 시간
}