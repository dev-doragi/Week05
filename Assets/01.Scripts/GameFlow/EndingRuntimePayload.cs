using System.Collections.Generic;
using UnityEngine;

public static class EndingRuntimePayload
{
    public struct FailedStageInfo
    {
        public string StageId;
        public string StageTitle;

        public FailedStageInfo(string id, string title)
        {
            StageId = id;
            StageTitle = title;
        }
    }

    private static readonly List<FailedStageInfo> _failedStages = new List<FailedStageInfo>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnPlayStart()
    {
        _failedStages.Clear();
    }

    public static void SetFailedStages(IReadOnlyList<StageDefinition> defs)
    {
        _failedStages.Clear();
        if (defs == null) return;

        for (int i = 0; i < defs.Count; i++)
        {
            StageDefinition d = defs[i];
            if (d == null) continue;
            _failedStages.Add(new FailedStageInfo(d.StageId, d.StageTitle));
        }
    }

    public static List<FailedStageInfo> ConsumeFailedStages()
    {
        List<FailedStageInfo> copy = new List<FailedStageInfo>(_failedStages);
        _failedStages.Clear(); // 1회성 소비
        return copy;
    }

    public static void Clear()
    {
        _failedStages.Clear();
    }
}
