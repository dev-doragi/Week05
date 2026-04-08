using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    public GameObject IngamePlayButton;
    public GameObject IngameSkipButton;

    public GameObject DebugGamePannel;
    protected override void Init()
    {
        IngamePlayButton.SetActive(false);
        IngameSkipButton.SetActive(false);
        DebugGamePannel.SetActive(false);
    }

    public void InGameChoiceButtonActive(bool choice)
    {
        IngamePlayButton.SetActive(choice);
        IngameSkipButton.SetActive(choice);
    }

    public void DebugGamePannelActive(bool choice)
    {
        DebugGamePannel.SetActive(choice);
    }

    
    
}
