using UnityEngine;
using System.Collections;
using TMPro;

public class DentureCollectGame : MiniGame
{
    [Header("References")]
    public DentureCollectPlayer player;
    public GameObject denturePrefab;
    public Transform spawnLine;
    public Transform leftBoundary;
    public Transform rightBoundary;
    public TextMeshPro countText;

    [Header("Settings")]
    public int targetCount = 10;
    public float spawnInterval = 1f;

    int _caught;

    void OnEnable() => player.OnCaught += HandleCaught;
    void OnDisable()
    {
        player.OnCaught -= HandleCaught;
        StopAllCoroutines();
        DestroyAllDentures();
    }

    protected override void OnStart()
    {
        _caught = 0;
        UpdateUI();
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            float spawnX = Random.Range(leftBoundary.position.x, rightBoundary.position.x);
            Vector3 pos = new Vector3(spawnX, spawnLine.position.y, 0f);
            Instantiate(denturePrefab, pos, Quaternion.identity, transform);
        }
    }

    void HandleCaught(DentureCollectDenture denture)
    {
        _caught++;
        UpdateUI();

        Debug.Log($"[DentureCollectGame] {_caught} / {targetCount}");

        if (_caught >= targetCount)
        {
            StopAllCoroutines();
            Clear();
        }
    }

    void UpdateUI()
    {
        if (countText != null)
            countText.text = $"{_caught} / {targetCount}";
    }

    void DestroyAllDentures()
    {
        foreach (DentureCollectDenture denture in GetComponentsInChildren<DentureCollectDenture>())
            Destroy(denture.gameObject);
    }
}