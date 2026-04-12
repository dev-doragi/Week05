using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using System;

public class TypingPanel : MonoBehaviour
{
    public Action<bool> OnTypingComplete;

    [Header("설정")]
    [TextArea] public string targetText = "Hello World";

    [Header("UI 연결")]
    public GameObject panel;
    public TextMeshProUGUI displayText;
    public Button saveButton;

    private int currentIndex = 0;

    private void Start()
    {
        OpenPanel();

    }

    public void OpenPanel()
    {
        currentIndex = 0;
        panel.SetActive(true);
        saveButton.interactable = false;

        SkipNonAlpha();
        UpdateDisplay();

        Keyboard.current.onTextInput += OnTextInput;
    }

    void CloseInput()
    {
        Keyboard.current.onTextInput -= OnTextInput;
    }

    void OnTextInput(char c)
    {
        if (currentIndex >= targetText.Length) return;

        if (char.ToLower(c) == char.ToLower(targetText[currentIndex]))
        {
            currentIndex++;
            SkipNonAlpha();
            UpdateDisplay();

            if (currentIndex >= targetText.Length)
                saveButton.interactable = true;
        }
    }

    void SkipNonAlpha()
    {
        while (currentIndex < targetText.Length && !char.IsLetter(targetText[currentIndex]))
            currentIndex++;
    }

    void UpdateDisplay()
    {
        string done = targetText.Substring(0, currentIndex);
        string remaining = targetText.Substring(currentIndex);
        displayText.text = $"<color=white>{done}</color><color=grey>{remaining}</color>";
    }

    public void OnSaveClicked()
    {
        CloseInput();
        OnTypingComplete?.Invoke(true);
        panel.SetActive(false);
    }

    void OnDisable()
    {
        // 패널 비활성화될 때 구독 누수 방지
        if (Keyboard.current != null)
            Keyboard.current.onTextInput -= OnTextInput;
    }
}