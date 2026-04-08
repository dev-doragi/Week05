using System;
using UnityEngine;
using TMPro;

public class LogManager : Singleton<LogManager>
{
    protected override void Init()
    {
        
    }
    [SerializeField] private TextMeshProUGUI LogTitle;
    [SerializeField] private TextMeshProUGUI LogMessage;

    public void UpdateIssueLog(IssueDefinition issue)
    {
        if (LogTitle == null || LogMessage == null) return;

        if (issue == null)
        {
            LogTitle.text = string.Empty;
            LogMessage.text = string.Empty;
            return;
        }

        LogTitle.text = issue.IssueTitle;
        LogMessage.text = issue.ConsoleWarning;
    }
    
}
