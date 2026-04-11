using UnityEngine;
using System;
using System.Collections.Generic;

public class StageRGB : Stage
{
    [Serializable]
    public class TargetData
    {
        public bool isCleared;
        public RGBObject target;
    }

    [Header("RGB Targets")]
    [SerializeField] private List<TargetData> _targetListData = new List<TargetData>();

    private Dictionary<int, TargetData> _targets = new Dictionary<int, TargetData>();

    private void Start()
    {
        _targets.Clear();

        for (int i = 0; i < _targetListData.Count; i++)
        {
            var data = _targetListData[i];

            if (data.target != null)
            {
                _targets.Add(i, data);
                data.target.Init(i);
            }
        }

        SubscribeEvents();
    }
    protected override void OnStart()
    {
        _targets.Clear();

        for (int i = 0; i < _targetListData.Count; i++)
        {
            var data = _targetListData[i];

            if (data.target != null)
            {
                _targets.Add(i, data);
                data.target.Init(i);
            }
        }

        SubscribeEvents();
    }

    private void SubscribeEvents()
    {
        foreach (var data in _targets.Values)
        {
            if (data.target != null)
            {
                data.target.Success += TargetSuccess;
            }
        }
    }

    private void OnDisable()
    {
        foreach (var data in _targets.Values)
        {
            if (data.target != null)
            {
                data.target.Success -= TargetSuccess;
            }
        }
    }

    private void TargetSuccess(int index)
    {
        if (_targets.TryGetValue(index, out TargetData solvedData))
        {
            // 중복 실행 방지를 위해 성공한 타겟 구독 해제
            if (solvedData.target != null)
            {
                solvedData.target.Success -= TargetSuccess;
            }

            // 클리어 상태 업데이트
            solvedData.isCleared = true;

            if (AllTargetsCleared())
            {
                StageClear();
            }
        }
    }

    private bool AllTargetsCleared()
    {
        foreach (var data in _targets.Values)
        {
            if (!data.isCleared) return false;
        }
        return true;
    }

    public void StageClear()
    {
        Debug.Log("[Stage RGB] Stage Clear!");
        Clear();
    }
}