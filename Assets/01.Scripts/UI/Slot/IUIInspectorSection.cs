using System;

public interface IUIInspectorSection
{
    event Action<string, string, string> ReferenceDropped;

    void Bind(string ownerId, UI_RuntimeInspectorSectionData sectionData);
}
