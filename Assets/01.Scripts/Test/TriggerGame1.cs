using UnityEngine;

public class TriggerGame1 : MonoBehaviour
{
    [SerializeField] private MiniGame ownerMiniGame;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool oneShot = true;
    private bool used;

    private void Awake()
    {
        if (ownerMiniGame == null)
            ownerMiniGame = GetComponentInParent<MiniGame>();
    }

    private void OnEnable()
    {
        used = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (oneShot && used) return;
        if (!other.CompareTag(playerTag)) return;
        if (ownerMiniGame == null) return;

        used = true;
        ownerMiniGame.CompleteFromChild();
    }
}