using UnityEngine;
using UnityEngine.InputSystem;

public class JellyShotgunGame : MiniGame
{
    [Header("References")]
    public Transform gun;
    public JellyShotgunBullet bullet;
    public GameObject[] enemies;

    [Header("Laser")]
    public LineRenderer laserLine;
    public float laserLength = 20f;

    int _remainingEnemies;

    void OnEnable() => JellyShotgunBullet.OnHitEnemy += HandleHitEnemy;
    void OnDisable() => JellyShotgunBullet.OnHitEnemy -= HandleHitEnemy;

    protected override void OnStart()
    {
        foreach (GameObject enemy in enemies)
            enemy.SetActive(true);

        _remainingEnemies = enemies.Length;
        bullet.gameObject.SetActive(false);
    }

    void Update()
    {
        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 dir = (mouseWorld - (Vector2)gun.position).normalized;

        // 총 회전
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        gun.rotation = Quaternion.Euler(0f, 0f, angle);

        // 레이저
        RaycastHit2D hit = Physics2D.Raycast(gun.position, dir, laserLength);
        Vector2 laserEnd = hit.collider != null
            ? hit.point
            : (Vector2)gun.position + dir * laserLength;

        laserLine.SetPosition(0, gun.position);
        laserLine.SetPosition(1, laserEnd);

        // 발사
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;
        if (bullet.gameObject.activeSelf) return;

        bullet.transform.position = gun.position;
        bullet.Fire(dir);
    }

    void HandleHitEnemy(GameObject enemy)
    {
        enemy.SetActive(false);
        _remainingEnemies--;

        Debug.Log($"[JellyShotgunGame] 남은 적: {_remainingEnemies}");

        if (_remainingEnemies <= 0) Clear();
    }
}