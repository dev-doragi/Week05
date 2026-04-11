using System;
using System.Collections.Generic;
using UnityEngine;

public class StageRGB : Stage
{
    [Serializable]
    public class TargetData
    {
        public RGBObject target;
    }

    [Header("RGB Targets")]
    [SerializeField] private List<TargetData> _targetListData = new List<TargetData>();
    [SerializeField] private Transform _spawnPos;

    private void Start()
    {
        OnStart();

    }
    protected override void OnStart()
    {
        for (int i = 0; i < _targetListData.Count; i++)
        {
            var data = _targetListData[i];

            if (data.target != null)
                data.target.Init(i, _spawnPos);
        }
    }
}
