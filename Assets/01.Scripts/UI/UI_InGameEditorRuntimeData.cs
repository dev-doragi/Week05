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

    public bool IsMissing()
    {
        return IsRequired && string.IsNullOrEmpty(CurrentTargetId);
    }

    public bool IsWrongReference()
    {
        if (string.IsNullOrEmpty(CurrentTargetId))
            return false;

        return CurrentTargetId != ExpectedTargetId;
    }

    public bool IsCorrect()
    {
        if (IsMissing())
            return false;

        if (IsWrongReference())
            return false;

        if (string.IsNullOrEmpty(ExpectedTargetId))
            return false;

        return CurrentTargetId == ExpectedTargetId;
    }

    public bool HasError()
    {
        return IsMissing() || IsWrongReference();
    }
}

public enum UI_ReferenceValidationErrorType
{
    None,
    MissingReference,
    WrongReference,
}

public class UI_ReferenceValidationResult
{
    public string OwnerId;
    public string OwnerDisplayName;
    public InspectorComponent InspectorComponent;
    public string SlotId;
    public string SlotLabel;
    public string ExpectedTargetId;
    public string CurrentTargetId;
    public UI_ReferenceValidationErrorType ErrorType;
}

public class UI_ValidationSummary
{
    public List<UI_ReferenceValidationResult> Errors = new();

    public int ErrorCount => Errors.Count;
    public bool HasError => ErrorCount > 0;
    public bool IsSolved => ErrorCount == 0;
}

public class UI_RuntimeReferenceKey
{
    public string OwnerId;
    public InspectorComponent InspectorComponent;
    public string SlotId;
}
