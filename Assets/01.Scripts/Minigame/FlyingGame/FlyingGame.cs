using System;
using System.Collections.Generic;
using UnityEngine;

public class FlyingGame : MiniGame
{
    public static event Action<Vector2>  OnFlyingGameStart;
    
    [Header("Player Settings")]
    public Vector2 startPos;
    //public Vector2 gameSize = new Vector2(12, 6);
    //public float offset = 1f;


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
        Debug.Log("[Flying Game] Game Start");

        OnFlyingGameStart?.Invoke(startPos);
        PillarSetting();
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
        /*
        // 1. 전체 게임 영역 (검은색 외곽선)
        Gizmos.color = Color.black;
        Gizmos.DrawWireCube(transform.position, new Vector3(gameSize.x, gameSize.y, 0));

        // 내부 영역 계산
        float innerWidth = gameSize.x - (offset * 2);
        float innerHeight = gameSize.y - (offset * 2);
        Vector3 center = transform.position;

        // 2. 여백(Offset) 라인 (회색)
        Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        Gizmos.DrawWireCube(center, new Vector3(innerWidth, innerHeight, 0));

        // 3. 구역 분할 계산
        float genZoneWidth = innerWidth * pillarZoneRatio;
        float goalZoneWidth = innerWidth * (1 - pillarZoneRatio);

        // 기둥 생성 존 위치 (초록색)
        Vector3 genZonePos = center + new Vector3(-innerWidth / 2 + genZoneWidth / 2, 0, 0);
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(genZonePos, new Vector3(genZoneWidth, innerHeight, 0));

        // 골인 존 위치 (노란색)
        Vector3 goalZonePos = center + new Vector3(innerWidth / 2 - goalZoneWidth / 2, 0, 0);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(goalZonePos, new Vector3(goalZoneWidth, innerHeight, 0));
        */

        // 4. 플레이어 시작 위치 표시 (하늘색 구체)
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(startPos, 0.3f);
    }
}
