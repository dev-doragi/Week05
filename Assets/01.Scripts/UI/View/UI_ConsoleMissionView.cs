using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_ConsoleMissionView : MonoBehaviour
{
    [SerializeField] private TMP_Text _missionTitle;
    [SerializeField] private Image _successImage;

    private int _index;
    public int Index => _index; 


    public void Init(int missionIndex, string missionTitle)
    {
        _missionTitle.text = missionTitle;
        _index = missionIndex;
    }

    public void IsSuccess()
    {
        if (_successImage != null)
        {
            _successImage.gameObject.SetActive(true);
        }
    }
}
