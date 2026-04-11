using System;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmErrorWindow : MonoBehaviour
{
    public event Action<GameObject> OnConfirm;

    [Header("UI")]
    public Button confirmButton;

    void Start() => confirmButton.onClick.AddListener(ConfirmAction);

    private void ConfirmAction()
    {
        OnConfirm?.Invoke(gameObject);
    }
}