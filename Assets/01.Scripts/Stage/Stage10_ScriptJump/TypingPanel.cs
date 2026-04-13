using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class TypingPanel : MonoBehaviour
{
    [Header("설정")]
    [TextArea] public string targetText = "Hello World";

    [Header("UI 연결")]
    public GameObject panel;
    public TextMeshProUGUI displayText;
    public Button saveButton;

    public event Action<bool> OnTypingComplete;

    private int currentIndex = 0;
    private bool _isComplete = false;

    public void OpenPanel()
    {
        currentIndex = 0;
        _isComplete = false;
        panel.SetActive(true);
        saveButton.interactable = false;

        SkipNonAlpha();
        UpdateDisplay();

        Keyboard.current.onTextInput += OnTextInput;
        InputSystem.onAfterUpdate += CheckEnter;
    }

    void CloseInput()
    {
        Keyboard.current.onTextInput -= OnTextInput;
        InputSystem.onAfterUpdate -= CheckEnter;
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
            {
                _isComplete = true;
                saveButton.interactable = true;
            }
        }
    }

    void CheckEnter()
    {
        if (_isComplete && Keyboard.current.enterKey.wasPressedThisFrame)
            TrySave();
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

    // Save 버튼 OnClick에 연결
    public void OnSaveClicked()
    {
        TrySave();
    }

    void TrySave()
    {
        if (!_isComplete) return;

        CloseInput();
        panel.SetActive(false);
        OnTypingComplete?.Invoke(true);
    }

    void OnDisable()
    {
        if (Keyboard.current != null)
            Keyboard.current.onTextInput -= OnTextInput;
        InputSystem.onAfterUpdate -= CheckEnter;
    }
}