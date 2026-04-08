using UnityEngine;

[CreateAssetMenu(fileName = "Issue_", menuName = "DelayTheInevitable/Issue Definition")]
public class IssueDefinition : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string issueId = "ISSUE_001";
    [SerializeField] private string issueTitle = "Missing Reference";
    [TextArea] [SerializeField] private string consoleWarning = "Console Warning";

    [Header("1on1 Mapping")]
    [SerializeField] private string debugMiniGameKey = "Debug_001";
    [SerializeField] private string ingameMiniGameKey = "Ingame_001";

    public string IssueId => issueId;
    public string IssueTitle => issueTitle;
    public string ConsoleWarning => consoleWarning;
    public string DebugMiniGameKey => debugMiniGameKey;
    public string IngameMiniGameKey => ingameMiniGameKey;
}
