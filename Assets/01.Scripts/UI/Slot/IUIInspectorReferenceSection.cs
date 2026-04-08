using System;

public interface IUIInspectorReferenceSection : IUIInspectorSection
{
    event Action<string, InspectorComponent, string, string> ReferenceDropped;
}
