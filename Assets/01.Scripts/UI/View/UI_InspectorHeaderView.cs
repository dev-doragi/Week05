using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_InspectorHeaderView : MonoBehaviour
{
    [SerializeField] private List<GameObject> _panelRoots = new();
    [SerializeField] private TMP_Text _titleText;

    public void Show(string displayName)
    {
        SetRootsActive(true);

        if (_titleText != null)
            _titleText.text = displayName;
    }

    public void Hide()
    {
        SetRootsActive(false);

        if (_titleText != null)
            _titleText.text = string.Empty;
    }

    private void SetRootsActive(bool isActive)
    {
        for (int i = 0; i < _panelRoots.Count; i++)
        {
            if (_panelRoots[i] != null)
                _panelRoots[i].SetActive(isActive);
        }
    }
}
