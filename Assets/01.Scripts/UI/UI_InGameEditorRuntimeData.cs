using System.Collections.Generic;
using UnityEngine;

public class UI_InGameEditorRuntimeData
{
    public string Id;
    public string DisplayName;
    public SO_ComponentData SourceData;

    public UI_ProjectRuntimeData ProjectData;
    public UI_HierarchyRuntimeData HierarchyData;

    public List<UI_RuntimeInspectorSectionData> Sections = new();
}

public class UI_ProjectRuntimeData
{
    public ProejctFileStruct FileStruct;
}

public class UI_HierarchyRuntimeData
{
    public UIEditorScene Scene;
}

public class UI_RuntimeInspectorSectionData
{
    public InspectorComponent InspectorComponent;
    public List<UI_RuntimeReferenceData> References = new();
}

public class UI_RuntimeReferenceData
{
    public string SlotId;
    public string Label;

    public string ExpectedTargetId;
    public string CurrentTargetId;

    public bool CanSpawnError;
    public bool IsRequired;
}
