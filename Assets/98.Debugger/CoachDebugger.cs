using UnityEngine;

public class CoachDebugger : MonoBehaviour
{
    [SerializeField] private CoachMovementController _coach;

    private void OnEnable()
    {
        if (_coach != null)
        {
            _coach.OnCoachPreparingToMove += (from, to) => Debug.Log($"[준비] 코치가 {from}에서 {to}(으)로 갈 준비 중...");
            _coach.OnCoachMoved += (from, to) => Debug.Log($"[이동] 코치가 {from}에서 {to}(으)로 이동 완료. (현재 어그로: {_coach.CurrentAggro})");
            _coach.OnCoachReachedOffice += () => Debug.Log("GameOver");
        }
    }
}