using UnityEngine;

public class BlockPathIndicator : MonoBehaviour
{
    [SerializeField] private GameObject _indicator;

    private GimmickManager _gimmickManager;
    private bool _lastState;

    private void Awake()
    {
        Refresh(true);
    }

    private void Update()
    {
        Refresh(false);
    }

    private void Refresh(bool force)
    {
        if (_gimmickManager == null)
            _gimmickManager = GimmickManager.Instance;

        if (_indicator == null)
            return;

        bool isReady = _gimmickManager.CanUseBlockPath;

        if (!force && _lastState == isReady)
            return;

        _lastState = isReady;
        _indicator.SetActive(isReady);
    }
}