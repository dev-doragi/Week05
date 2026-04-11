using System.Collections.Generic;
using UnityEngine;

public class StageErrorConfirm : Stage
{
    [Header("UI")]
    public RectTransform canvasRect;
    public float padding = 100f;

    [Header("Error Window Settings")]
    public GameObject errorWindowPrefab;
    public int minErrors = 15;
    public int maxErrors = 20;
    [Space(5)]
    [Min(0f)] public float spawnInterval = 0.1f;
    
    private int _remainningErrors;
    private Coroutine _spawnRoutine;
    private List<GameObject> _spawnedWindows = new List<GameObject>();

    protected override void OnStart()
    {
        _spawnedWindows.Clear();
        _remainningErrors = Random.Range(minErrors, maxErrors + 1);

        if (canvasRect == null) canvasRect = FindFirstObjectByType<Canvas>().GetComponent<RectTransform>();
        for (int i = 0; i < _remainningErrors; i++) SpawnErrorWindow();

        Debug.Log("[Stage Error Confirm] Error Confirm Start, Required: " + _remainningErrors);
    }

    private void SpawnErrorWindow()
    {
        GameObject errorWindow = Instantiate(errorWindowPrefab, canvasRect);
        RectTransform windowRect = errorWindow.GetComponent<RectTransform>();

        #region Position
        float canvasW = canvasRect.rect.width;
        float canvasH = canvasRect.rect.height;

        float halfW = (canvasW / 2f) - padding;
        float halfH = (canvasH / 2f) - padding;

        float rx = Random.Range(-halfW, halfW);
        float ry = Random.Range(-halfH, halfH);

        windowRect.anchoredPosition = new Vector2(rx, ry);
        #endregion

        // Event
        ConfirmErrorWindow errorWindowScript = errorWindow.GetComponent<ConfirmErrorWindow>();
        if (errorWindow != null) errorWindowScript.OnConfirm += HandleErrorConfirmed;
    }

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
            if (window != null) continue;

            ConfirmErrorWindow errorWindowScript = window.GetComponent<ConfirmErrorWindow>();
            if (errorWindowScript != null)
                errorWindowScript.OnConfirm -= HandleErrorConfirmed;

            Destroy(window);
        }

        _spawnedWindows.Clear();
    }

    private void OnDisable()
    {


        CleanWindow();
    }
}
