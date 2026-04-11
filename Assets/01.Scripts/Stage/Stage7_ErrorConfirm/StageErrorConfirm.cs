using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageErrorConfirm : Stage
{
    [Header("UI")]
    public RectTransform canvasRect;
    public float padding = 100f;

    #region Error Window
    private enum SpawnPattern
    {
        Random,
        Regular
    }
    private SpawnPattern _spawnPattern;
    
    [Header("Error Window Settings")]
    public GameObject errorWindowPrefab;
    public int minErrors = 15;
    public int maxErrors = 20;
    [Space(5)]
    [Min(0f)] public float _firstSpawnInterval;
    [Min(0f)] public float _nextSpawnInterval;
    public Vector2 spawnStep;
    #endregion

    private int _spawnTargetCount;  // 스폰할 에러창 갯수 (생성 중 에러창 확인으로 인한 버그 방지)
    private int _remainningErrors;  // 남은 에러창 갯수
    private int _spawnedCount;      // 스폰된 에러창 갯수 (스폰 패턴에 따른 스폰 위치 계산용)
    private Coroutine _spawnRoutine;
    private List<GameObject> _spawnedWindows = new List<GameObject>();

    protected override void OnStart()
    {
        // Init
        _spawnedWindows.Clear();
        if (canvasRect == null) canvasRect = FindFirstObjectByType<Canvas>().GetComponent<RectTransform>();

        // Error Window Spawn Count Set
        _spawnTargetCount = Random.Range(minErrors, maxErrors + 1);
        _remainningErrors = _spawnTargetCount;

        // Error Window Spawn Pattern Set
        _spawnPattern = Random.value < 0.5f ? SpawnPattern.Random : SpawnPattern.Regular;

        // Error Window Spawn
        _spawnRoutine = StartCoroutine(Co_SpawnErrors());
        Debug.Log("[Stage Error Confirm] Error Confirm Start, Required: " + _spawnTargetCount);
    }

    #region Error Window Spawn
    private IEnumerator Co_SpawnErrors()    // Error window spawn coroutine
    {
        for (int i = 0; i < _spawnTargetCount; i++)
        {
            switch (_spawnPattern)
            {
                case SpawnPattern.Random:
                    RandomSpawn();
                    if (i == 0)
                        yield return new WaitForSeconds(_firstSpawnInterval);
                    else
                        yield return new WaitForSeconds(_nextSpawnInterval);
                    break;
                case SpawnPattern.Regular:
                    RegularSpawn();
                    if (i == 0)
                        yield return new WaitForSeconds(_firstSpawnInterval);
                    else
                        yield return new WaitForSeconds(_nextSpawnInterval);
                    break;
            }
        }
    }

    private void RandomSpawn()              // Random pattern spawn method
    {
        // Spawn
        GameObject errorWindow = SpawnErrorWindow();
        RectTransform windowRect = errorWindow.GetComponent<RectTransform>();

        // Position
        Vector2 pos = SetPosRandomWindow(windowRect);
        windowRect.anchoredPosition = pos;
    }

    private  void RegularSpawn()            // Rugular pattern spawn method
    {
        // Spawn
        GameObject errorWindow = SpawnErrorWindow();
        RectTransform windowRect = errorWindow.GetComponent<RectTransform>();

        // Position
        Vector2 pos = (Vector2)transform.position + (spawnStep * _spawnedCount);
        pos = SetPosRegularWindow(windowRect, pos);

        windowRect.anchoredPosition = pos;
        _spawnedCount++;
    }

    private GameObject SpawnErrorWindow()   // Actually spawn the error window method
    {
        // Spawn
        GameObject errorWindow = Instantiate(errorWindowPrefab, canvasRect);
        _spawnedWindows.Add(errorWindow);

        // Event Subscribe
        ConfirmErrorWindow errorWindowScript = errorWindow.GetComponent<ConfirmErrorWindow>();
        if (errorWindowScript != null)
            errorWindowScript.OnConfirm += HandleErrorConfirmed;

        return errorWindow;
    }
    #endregion

    #region Error window position setting
    private Vector2 SetPosRandomWindow(RectTransform windowRect)
    {
        // Canvas size calc
        float canvasHalfW = canvasRect.rect.width * 0.5f;
        float canvasHalfH = canvasRect.rect.height * 0.5f;

        // Error window size calc
        float windowHalfW = windowRect.rect.width * 0.5f;
        float windowHalfH = windowRect.rect.height * 0.5f;

        // Position Clamp
        float minX = -canvasHalfW + windowHalfW + padding;
        float maxX = canvasHalfW - windowHalfW - padding;
        float minY = -canvasHalfH + windowHalfH + padding;
        float maxY = canvasHalfH - windowHalfH - padding;

        // Position Set
        float rx = Random.Range(minX, maxX);
        float ry = Random.Range(minY, maxY);

        return new Vector2(rx, ry);
    }

    private Vector2 SetPosRegularWindow(RectTransform windowRect, Vector2 pos)
    {
        // Canvas size calc
        float canvasHalfW = canvasRect.rect.width * 0.5f;
        float canvasHalfH = canvasRect.rect.height * 0.5f;

        // Error window size calc
        float windowHalfW = windowRect.rect.width * 0.5f;
        float windowHalfH = windowRect.rect.height * 0.5f;

        // Position Clamp
        float minX = -canvasHalfW + windowHalfW + padding;
        float maxX = canvasHalfW - windowHalfW - padding;
        float minY = -canvasHalfH + windowHalfH + padding;
        float maxY = canvasHalfH - windowHalfH - padding;

        // Position Set
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        return pos;
    }
    #endregion

    private void HandleErrorConfirmed(GameObject window)
    {
        Debug.Log($"[Confirm Debugging] Error Confirm! (Remain: {_remainningErrors})");

        ConfirmErrorWindow errorWindowScript = window.GetComponent<ConfirmErrorWindow>();
        if (errorWindowScript != null)
            errorWindowScript.OnConfirm -= HandleErrorConfirmed;

        _remainningErrors--;
        _spawnedWindows.Remove(window);
        Destroy(window);

        if (_remainningErrors <= 0) Clear();
    }

    private void CleanWindow()
    {
        foreach (var window in _spawnedWindows)
        {
            if (window == null) continue;

            ConfirmErrorWindow errorWindowScript = window.GetComponent<ConfirmErrorWindow>();
            if (errorWindowScript != null)
                errorWindowScript.OnConfirm -= HandleErrorConfirmed;

            Destroy(window);
        }

        _spawnedWindows.Clear();
    }

    private void OnDisable()
    {
        if (_spawnRoutine != null)
        {
            StopCoroutine(_spawnRoutine);
            _spawnRoutine = null;
        }

        CleanWindow();
    }
}
