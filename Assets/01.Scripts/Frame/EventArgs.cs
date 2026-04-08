using System;

[Serializable]
public class IssueDrawnEventArgs : EventArgs
{
    public IssueDefinition Issue;
    public IssueDrawnEventArgs(IssueDefinition issue)
    {
        Issue = issue;
    }
}

[Serializable]
public class IssueReturnedEventArgs : EventArgs
{
    public IssueDefinition Issue;
    public bool Returned;

    public IssueReturnedEventArgs(IssueDefinition issue, bool returned)
    {
        Issue = issue;
        Returned = returned;
    }
}
