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

    public struct ClearedStageInfo
    {
        public string StageId;
        public string StageTitle;

        public ClearedStageInfo(string id, string title)
        {
            StageId = id;
            StageTitle = title;
        }
    }

    private static readonly List<FailedStageInfo> _failedStages = new List<FailedStageInfo>();
    private static readonly List<ClearedStageInfo> _clearedStages = new List<ClearedStageInfo>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnPlayStart()
    {
        _failedStages.Clear();
        _clearedStages.Clear();
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

    public static void SetClearedStages(IReadOnlyList<StageDefinition> defs)
    {
        _clearedStages.Clear();
        if (defs == null) return;

        for (int i = 0; i < defs.Count; i++)
        {
            StageDefinition d = defs[i];
            if (d == null) continue;
            _clearedStages.Add(new ClearedStageInfo(d.StageId, d.StageTitle));
        }
    }

    public static List<FailedStageInfo> ConsumeFailedStages()
    {
        List<FailedStageInfo> copy = new List<FailedStageInfo>(_failedStages);
        _failedStages.Clear();
        return copy;
    }

    public static List<ClearedStageInfo> ConsumeClearedStages()
    {
        List<ClearedStageInfo> copy = new List<ClearedStageInfo>(_clearedStages);
        _clearedStages.Clear();
        return copy;
    }

    public static void Clear()
    {
        _failedStages.Clear();
        _clearedStages.Clear();
    }
}
