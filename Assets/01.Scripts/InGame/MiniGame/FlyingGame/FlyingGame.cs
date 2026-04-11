using System;
using System.Collections;
using UnityEngine;
using TMPro;

public class FlyingGame : MiniGame
{
    public static event Action<Vector2, float>  OnFlyingGameStart;

    [Header("Game Settings")]
    [SerializeField] private TextMeshPro _countDownText;
    [SerializeField] private int _countDownCount;
    [SerializeField] private float _secondPerCount = 0.6f;

    [Header("Player Settings")]
    public Vector2 startPos;


    [Header("Pillar Settings")]
    [SerializeField] private FlyPillar[] pillars;
    public float pillarMinGap = 3f;
    public float pillarMaxGap = 5f;
    public float pillarMinY;
    public float pillarMaxY;


    #region Event
    void OnEnable() => CollisionReporter.OnEnter2D += HandleCollision;
    
    void OnDisable() => CollisionReporter.OnEnter2D -= HandleCollision;
    #endregion

    protected override void OnStart()
    {
        PillarSetting();

        _countDownText.gameObject.SetActive(false);
        Vector3 worldStartPos = transform.TransformPoint(startPos);

        float totalWaitTime = _countDownCount * _secondPerCount;
        OnFlyingGameStart?.Invoke(worldStartPos, totalWaitTime);

        StartCoroutine(Co_CountdownRoutine());
    }

    private IEnumerator Co_CountdownRoutine()
    {
        _countDownText.gameObject.SetActive(true);

        for (int i = _countDownCount; i > 0; i--)
        {
            _countDownText.text = i.ToString();
            _countDownText.transform.localScale = Vector3.one;

            float timer = 0f;
            while (timer < _secondPerCount) 
            {
                timer += Time.deltaTime;
                float progress = timer / _secondPerCount;
                _countDownText.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.3f, progress);

                yield return null;
            }
        }

        _countDownText.gameObject.SetActive(false);
        StartFlyingGame();
    }

    private void StartFlyingGame()
    {
        Debug.Log("[Flying Game] Game Start");
    }

    private void PillarSetting()
    {
        foreach (var p in  pillars)
        {
            float gap = UnityEngine.Random.Range(pillarMinGap, pillarMaxGap);
            float y = UnityEngine.Random.Range(pillarMinY, pillarMaxY);

            p.transform.localPosition = new Vector2(p.transform.localPosition.x, y);
            p.Init(gap);
        }

        Debug.Log("[Flying Game] Pillar Setting");
    }

    private void HandleCollision(Collider2D collision)
    {
        if (collision.GetComponent<ClearTrigger>())
        {
            Clear();
        }
    }

    private void OnDrawGizmos()
    {
        // 플레이어 시작 위치 표시
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(startPos, 0.3f);
    }
}
