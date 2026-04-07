using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class ReflectGame : MiniGame
{
    [Header("References")]
    public Transform monster;
    public Transform player;
    public BulletMover bullet;
    public SpriteRenderer monsterSprite;
    public SpriteRenderer playerSprite;

    [Header("Settings")]
    public int monsterHp = 3;
    public float reflectRange = 2f;
    public float bulletResetDelay = 1f;
    public float flashInterval = 0.1f;
    public int flashCount = 4;

    int _currentHp;

    void OnEnable() => CollisionReporter.OnEnter2D += HandleCollision;
    void OnDisable() => CollisionReporter.OnEnter2D -= HandleCollision;

    protected override void OnStart()
    {
        _currentHp = monsterHp;
        FireFromMonster();
    }

    void Update()
    {
        if (!bullet.gameObject.activeSelf) return;
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;

        float dist = Vector2.Distance(bullet.transform.position, player.position);
        if (dist > reflectRange) return;

        Vector2 dir = (monster.position - bullet.transform.position).normalized;
        bullet.Launch(dir);
    }

    void HandleCollision(Collider2D other)
    {
        if (!bullet.gameObject.activeSelf) return;

        if (other.transform == monster)
        {
            StartCoroutine(Flash(monsterSprite));
            _currentHp--;
            Debug.Log($"[ReflectGame] 몬스터 HP: {_currentHp}");

            if (_currentHp <= 0)
            {
                bullet.gameObject.SetActive(false);
                Clear();
                return;
            }
        }
        else if (other.transform == player)
        {
            StartCoroutine(Flash(playerSprite));
        }

        bullet.gameObject.SetActive(false);
        Invoke(nameof(FireFromMonster), bulletResetDelay);
    }

    void FireFromMonster()
    {
        bullet.transform.position = monster.position;
        Vector2 dir = (player.position - monster.position).normalized;
        bullet.Launch(dir);
    }

    IEnumerator Flash(SpriteRenderer sr)
    {
        for (int i = 0; i < flashCount; i++)
        {
            sr.enabled = false;
            yield return new WaitForSeconds(flashInterval);
            sr.enabled = true;
            yield return new WaitForSeconds(flashInterval);
        }
    }
}