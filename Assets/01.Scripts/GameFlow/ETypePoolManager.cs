using System.Collections.Generic;
using UnityEngine;

public class ETypePoolManager : MonoBehaviour
{
    [SerializeField] private List<IssueDefinition> seedIssues = new List<IssueDefinition>();

    [SerializeField] private int remainingCountInspector;
    [SerializeField] private List<IssueDefinition> currentPoolInspector = new List<IssueDefinition>();


    private readonly List<IssueDefinition> debugPool = new List<IssueDefinition>();
    private readonly HashSet<string> issueKeys = new HashSet<string>();

    public int RemainingCount => debugPool.Count;

    public void ResetPool()
    {
        debugPool.Clear();
        issueKeys.Clear();

        for (int i = 0; i < seedIssues.Count; i++)
            AddInternal(seedIssues[i]);

        Debug.Log($"Reset: {debugPool.Count}");
        SyncInspectorView();

    }

    public bool TryDrawRandomIssue(out IssueDefinition issue)
    {
        if (debugPool.Count == 0)
        {
            issue = null;
            return false;
        }

        int index = Random.Range(0, debugPool.Count);
        issue = debugPool[index];

        debugPool.RemoveAt(index);
        SyncInspectorView();

        issueKeys.Remove(GetKey(issue));

        EventManager.Instance.PostNotification( MEventType.IssueDrawn, this, new IssueDrawnEventArgs(issue));


        return true;
    }

    public bool ReturnIssueWithChance(IssueDefinition issue, float chance01)
    {
        if (issue == null) return false;

        bool returned = Random.value <= Mathf.Clamp01(chance01);
        if (!returned) return false;

        AddInternal(issue);

        EventManager.Instance.PostNotification(
            MEventType.IssueReturned,
            this,
            new IssueReturnedEventArgs(issue, returned));
        
        SyncInspectorView();

        return returned;

    }

    private void AddInternal(IssueDefinition issue)
    {
        if (issue == null) return;

        string key = GetKey(issue);
        if (issueKeys.Contains(key)) return;

        issueKeys.Add(key);
        debugPool.Add(issue);
    }

    private string GetKey(IssueDefinition issue)
    {
        if (issue == null) return string.Empty;
        if (!string.IsNullOrWhiteSpace(issue.IssueId)) return issue.IssueId;
        return issue.GetInstanceID().ToString();
    }

    private void SyncInspectorView()
    {
        remainingCountInspector = debugPool.Count;
        currentPoolInspector.Clear();
        currentPoolInspector.AddRange(debugPool);
    }
}
