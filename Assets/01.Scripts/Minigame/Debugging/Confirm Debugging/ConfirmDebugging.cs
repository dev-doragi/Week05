using System.Collections.Generic;
using UnityEngine;

public class ConfirmDebugging : MiniGame
{
    [Header("UI")]
    public GameObject errorWindowPrefab;
    public RectTransform canvasRect;
    public float padding = 100f;


    [Header("Settings")]
    public int minErrors = 15;
    public int maxErrors = 20;


    private int _remainningErrors;
    private List<GameObject> _spawnedWindows = new List<GameObject>();

    protected override void OnStart()
    {
        _spawnedWindows.Clear();
        _remainningErrors = Random.Range(minErrors, maxErrors + 1);

        for (int i = 0; i < _remainningErrors; i++) SpawnErrorWindow();

        Debug.Log("[Confirm Debugging] Confirm Debug Start, Required: " + _remainningErrors);
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

        _remainningErrors--;
        _spawnedWindows.Remove(window);
        Destroy(window);

        if (_remainningErrors <= 0) Clear();
    }

    private void OnDisable()
    {
        foreach (var window in _spawnedWindows) if (window != null) Destroy(window);
        
        _spawnedWindows.Clear();
    }
}
