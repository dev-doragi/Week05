using UnityEngine;

public class PlaceDebugging : MiniGame
{
    [Header("Placement Targets")]
    public PlaceObject[] objects;
    public PlaceTarget[] targets;

    [Header("Match Settings")]
    public int minMatchCount = 3;
    public int maxMatchCount = 5;

    private int _requireMatchCount;
    [SerializeField] private int _currentMatchCount = 0;

    [Header("Place Area")]
    public Rect placeArea;


    protected override void OnStart()
    {
        _currentMatchCount = 0;

        // 매칭 갯수 결정
        int maxPossible = Mathf.Min(objects.Length, targets.Length, maxMatchCount);
        _requireMatchCount = Random.Range(minMatchCount, maxPossible + 1);

        // 초기화
        foreach (var obj in objects) obj.gameObject.SetActive(false);
        foreach (var t in targets) t.gameObject.SetActive(false);

        for (int i = 0; i < _requireMatchCount; i++)
        {
            // 활성화
            objects[i].gameObject.SetActive(true);
            targets[i].gameObject.SetActive(true);

            // 갱신
            objects[i].Init();
            targets[i].Init();
            targets[i].OnMatch += HandleMatch;

            // 배치
            objects[i].transform.position = GetRandomPosition();
            targets[i].transform.position = GetRandomPosition();
        }

        Debug.Log("[Place Debugging] Place Debug Start Required: " + _requireMatchCount);
    }

    private Vector3 GetRandomPosition()
    {
        float rx = Random.Range(placeArea.xMin, placeArea.xMax);
        float ry = Random.Range(placeArea.yMin, placeArea.yMax);

        return new Vector3(rx, ry, 0);
    }

    private void HandleMatch()
    {
        _currentMatchCount++;
        Debug.Log($"[Place Debugging] Match Success (Total: {_currentMatchCount} / {_requireMatchCount})");

        if (_currentMatchCount >= _requireMatchCount)
        {
            Debug.Log("[Place Debugging] Match All Clear!");

            Clear();
        }
    }

    private void OnDisable()
    {
        foreach(var t in targets) if (t != null) t.OnMatch -= HandleMatch;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        Vector3 center = new Vector3(placeArea.x + placeArea.width / 2, placeArea.y + placeArea.height / 2, 0);
        Vector3 size = new Vector3(placeArea.width, placeArea.height);

        Gizmos.DrawWireCube(center, size);
    }
}
