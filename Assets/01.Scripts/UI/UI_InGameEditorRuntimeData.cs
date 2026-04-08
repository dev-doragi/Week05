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

    public bool HasReferenceError()
    {
        foreach (var section in Sections)
        {
            if (section == null)
                continue;

            foreach (var reference in section.References)
            {
                if (reference == null)
                    continue;

                if (reference.HasError())
                    return true;
            }
        }

        return false;
    }

    public int GetReferenceErrorCount()
    {
        int count = 0;

        foreach (var section in Sections)
        {
            if (section == null)
                continue;

            foreach (var reference in section.References)
            {
                if (reference == null)
                    continue;

                if (reference.HasError())
                    count++;
            }
        }

        return count;
    }
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
    public string ExpectedTargetDisplayName;
    public string CurrentTargetId;
    public string CurrentTargetDisplayName;

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

    public UI_ReferenceValidationErrorType GetErrorType()
    {
        if (IsMissing())
            return UI_ReferenceValidationErrorType.MissingReference;

        if (IsWrongReference())
            return UI_ReferenceValidationErrorType.WrongReference;

        return UI_ReferenceValidationErrorType.None;
    }

    public bool IsCorrect()
    {
        if (GetErrorType() != UI_ReferenceValidationErrorType.None)
            return false;

        if (string.IsNullOrEmpty(ExpectedTargetId))
            return false;

        return CurrentTargetId == ExpectedTargetId;
    }

    public bool HasError()
    {
        return GetErrorType() != UI_ReferenceValidationErrorType.None;
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
