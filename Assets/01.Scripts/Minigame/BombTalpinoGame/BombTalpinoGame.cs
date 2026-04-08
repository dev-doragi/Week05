using UnityEngine;
using UnityEngine.InputSystem;

public class BombTalpinoGame : MiniGame
{
    [Header("References")]
    public Transform player;
    public BombTalpinoBomb bomb;
    public BombTalpinoTarget target;

    [Header("Settings")]
    public float throwForce = 10f;

    BombTalpinoWall[] _walls;

    void Awake()
    {
        _walls = GetComponentsInChildren<BombTalpinoWall>(true);
    }

    void OnEnable() => BombTalpinoTarget.OnDestroyed += HandleTargetDestroyed;
    void OnDisable()
    {
        BombTalpinoTarget.OnDestroyed -= HandleTargetDestroyed;
        bomb.gameObject.SetActive(false);
    }

    protected override void OnStart()
    {
        bomb.gameObject.SetActive(false);
        target.gameObject.SetActive(true);

        foreach (BombTalpinoWall wall in _walls)
            wall.gameObject.SetActive(true);
    }

    void Update()
    {
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;
        if (bomb.gameObject.activeSelf) return;

        Vector3 screenPos = Mouse.current.position.ReadValue();
        screenPos.z = -Camera.main.transform.position.z;
        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(screenPos);
        Vector2 dir = (mouseWorld - (Vector2)player.position).normalized;

        bomb.transform.position = player.position;
        bomb.Fire(dir, throwForce);
    }

    void HandleTargetDestroyed()
    {
        Debug.Log("[BombTalpinoGame] CLEAR");
        Clear();
    }
}