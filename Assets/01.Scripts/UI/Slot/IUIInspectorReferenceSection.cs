using System;

public interface IUIInspectorReferenceSection : IUIInspectorSection
{
    event Action<string, string, string> ReferenceDropped;
}
