using System.Collections.Generic;
using UnityEngine;

// 인게임 에디터의 핵심 데이터를 관리하는 State(상태) 클래스입니다.
// 프로젝트 창, 하이라키 창의 목록과 인스펙터의 참조 상태를 동기화합니다.
public class UI_InGameEditorRuntimeState
{
    public event System.Action<UI_RuntimeReferenceKey> ReferenceSolved;
    // ID를 키로 하여 모든 런타임 데이터를 저장하는 캐시 저장소
    private readonly Dictionary<string, UI_InGameEditorRuntimeData> _componentById = new();

    // 각 창(Window)에 노출될 아이템 ID 리스트
    private readonly List<string> _projectItemIds = new();
    private readonly List<string> _hierarchyItemIds = new();

    // 현재 선택된 객체 정보 및 활성화된 씬
    public string SelectedRuntimeComponentId { get; private set; }
    public string SelectedProjectComponentId { get; private set; }
    public string SelectedHierarchyComponentId { get; private set; }
    public UIEditorScene CurrentScene { get; private set; }

    // 초기 데이터(SO)를 받아 런타임 환경을 구축합니다.
    public void Init(SO_InGameEditorInitialPlacementData initialPlacementData)
    {
        _componentById.Clear();
        _projectItemIds.Clear();
        _hierarchyItemIds.Clear();

        SelectedRuntimeComponentId = null;
        SelectedProjectComponentId = null;
        SelectedHierarchyComponentId = null;
        CurrentScene = UIEditorScene.None;

        if (initialPlacementData == null) return;

        bool hasScene = false;

        foreach (var objectEntry in initialPlacementData.Objects)
        {
            if (objectEntry == null || string.IsNullOrEmpty(objectEntry.ObjectId) || objectEntry.ComponentData == null)
                continue;

            // 1. 데이터 생성 및 등록
            var runtimeData = CreateRuntimeData(objectEntry);
            _componentById[runtimeData.Id] = runtimeData;
            RegisterWindow(runtimeData);

            // 2. 하이라키 데이터가 있다면 현재 씬 정보 설정
            if (hasScene == false && runtimeData.HierarchyData != null)
            {
                CurrentScene = runtimeData.HierarchyData.Scene;
                hasScene = true;
            }
        }

        // 초기화 마지막에 모든 참조의 표시 이름(Display Name)을 갱신합니다.
        RefreshAllReferenceDisplayNames();
    }

    public void SetCurrentScene(UIEditorScene scene)
    {
        CurrentScene = scene;
    }

    // 프로젝트 창에서 아이템 선택 시 상태 갱신
    public void SelectFromProject(string componentId)
    {
        SelectedRuntimeComponentId = componentId;
        SelectedProjectComponentId = componentId;
        SelectedHierarchyComponentId = null;
    }

    // 하이라키 창에서 아이템 선택 시 상태 갱신
    public void SelectFromHierarchy(string componentId)
    {
        SelectedRuntimeComponentId = componentId;
        SelectedProjectComponentId = null;
        SelectedHierarchyComponentId = componentId;
    }

    // 현재 선택된 객체의 데이터를 반환
    public UI_InGameEditorRuntimeData GetSelectedRuntimeData()
    {
        if (string.IsNullOrEmpty(SelectedRuntimeComponentId)) return null;
        _componentById.TryGetValue(SelectedRuntimeComponentId, out var data);
        return data;
    }

    // 프로젝트 창용 아이템 목록 추출
    public IReadOnlyList<UI_InGameEditorRuntimeData> GetProjectItems()
    {
        var items = new List<UI_InGameEditorRuntimeData>();
        foreach (var id in _projectItemIds)
        {
            if (_componentById.TryGetValue(id, out var data)) items.Add(data);
        }
        return items;
    }

    // 현재 씬에 맞는 하이라키 아이템 목록 추출
    public IReadOnlyList<UI_InGameEditorRuntimeData> GetHierarchyItems()
    {
        var items = new List<UI_InGameEditorRuntimeData>();
        foreach (var id in _hierarchyItemIds)
        {
            if (!_componentById.TryGetValue(id, out var data)) continue;
            if (data.HierarchyData == null || data.HierarchyData.Scene != CurrentScene) continue;
            items.Add(data);
        }
        return items;
    }

    // 오류(버그)를 발생시킬 수 있는 정상적인 참조들의 키 목록을 반환합니다.
    public List<UI_RuntimeReferenceKey> GetBreakableReferenceKeys()
    {
        var keys = new List<UI_RuntimeReferenceKey>();
        foreach (var pair in _componentById)
        {
            var runtimeData = pair.Value;
            foreach (var section in runtimeData.Sections)
            {
                foreach (var reference in section.References)
                {
                    // 오류 생성이 가능하고 현재 값이 정상인 것만 후보군에 포함
                    if (!reference.CanSpawnError || !reference.IsCorrect()) continue;

                    keys.Add(new UI_RuntimeReferenceKey
                    {
                        OwnerId = runtimeData.Id,
                        InspectorComponent = section.InspectorComponent,
                        SlotId = reference.SlotId
                    });
                }
            }
        }
        return keys;
    }

    // 특정 참조 슬롯을 해제(Missing) 상태로 만듭니다.
    public bool TryBreakReference(string ownerId, InspectorComponent inspectorComponent, string slotId)
    {
        var reference = FindReference(ownerId, inspectorComponent, slotId);
        if (reference == null || !reference.CanSpawnError) return false;

        reference.CurrentTargetId = null;
        reference.CurrentTargetDisplayName = null;
        return true;
    }

    // 무작위로 참조 하나를 끊어 오류 상태를 유도합니다.
    public bool TryBreakRandomReference(out UI_RuntimeReferenceKey brokenKey)
    {
        brokenKey = null;
        var candidates = GetBreakableReferenceKeys();
        if (candidates.Count == 0) return false;

        var candidate = candidates[Random.Range(0, candidates.Count)];
        if (!TryBreakReference(candidate.OwnerId, candidate.InspectorComponent, candidate.SlotId))
            return false;

        brokenKey = candidate;
        return true;
    }

    // 끊어진 참조를 원래 정답 데이터로 복구합니다.
    public bool TryRestoreReference(string ownerId, InspectorComponent inspectorComponent, string slotId)
    {
        var reference = FindReference(ownerId, inspectorComponent, slotId);
        if (reference == null) return false;

        reference.CurrentTargetId = reference.ExpectedTargetId;
        reference.CurrentTargetDisplayName = reference.ExpectedTargetDisplayName;
        return true;
    }

    // 플레이어가 특정 슬롯에 새로운 타겟(객체)을 직접 할당합니다.
    public bool TryAssignReference(string ownerId, InspectorComponent inspectorComponent, string slotId, string targetId)
    {
        var reference = FindReference(ownerId, inspectorComponent, slotId);
        if (reference == null) return false;

        bool hadError = reference.HasError();
        reference.CurrentTargetId = targetId;
        reference.CurrentTargetDisplayName = ResolveDisplayName(targetId);

        if (hadError && reference.IsCorrect())
        {
            ReferenceSolved?.Invoke(new UI_RuntimeReferenceKey
            {
                OwnerId = ownerId,
                InspectorComponent = inspectorComponent,
                SlotId = slotId
            });
        }

        return true;
    }

    // 에디터 내 모든 참조의 유효성을 검사하여 리포트(Summary)를 생성합니다.
    public UI_ValidationSummary ValidateAll()
    {
        var summary = new UI_ValidationSummary();
        foreach (var pair in _componentById)
        {
            var runtimeData = pair.Value;
            foreach (var section in runtimeData.Sections)
            {
                foreach (var reference in section.References)
                {
                    var errorType = GetErrorType(reference);
                    if (errorType == UI_ReferenceValidationErrorType.None) continue;

                    summary.Errors.Add(new UI_ReferenceValidationResult
                    {
                        OwnerId = runtimeData.Id,
                        OwnerDisplayName = runtimeData.DisplayName,
                        InspectorComponent = section.InspectorComponent,
                        SlotId = reference.SlotId,
                        SlotLabel = reference.Label,
                        ExpectedTargetId = reference.ExpectedTargetId,
                        CurrentTargetId = reference.CurrentTargetId,
                        ErrorType = errorType
                    });
                }
            }
        }
        return summary;
    }

    // 참조 데이터의 현재 오류 상태(Missing/Wrong)를 판별합니다.
    private UI_ReferenceValidationErrorType GetErrorType(UI_RuntimeReferenceData reference)
    {
        if (reference == null) return UI_ReferenceValidationErrorType.None;
        if (reference.IsMissing()) return UI_ReferenceValidationErrorType.MissingReference;
        if (reference.IsWrongReference()) return UI_ReferenceValidationErrorType.WrongReference;
        return UI_ReferenceValidationErrorType.None;
    }

    // SO 데이터를 기반으로 런타임용 객체를 생성합니다.
    private UI_InGameEditorRuntimeData CreateRuntimeData(SO_InGameEditorInitialPlacementData.ObjectEntry objectEntry)
    {
        var runtimeData = new UI_InGameEditorRuntimeData
        {
            Id = objectEntry.ObjectId,
            DisplayName = objectEntry.ComponentData.DisplayName,
            SourceData = objectEntry.ComponentData,
        };

        BuildWindowData(runtimeData, objectEntry);
        BuildSections(runtimeData, objectEntry);

        return runtimeData;
    }

    // 객체가 어느 창(Project/Hierarchy)에 표시될지 결정합니다.
    private void BuildWindowData(UI_InGameEditorRuntimeData runtimeData, SO_InGameEditorInitialPlacementData.ObjectEntry objectEntry)
    {
        var data = runtimeData.SourceData;
        switch (data.ComponentWindow)
        {
            case ComponentWindow.Project:
                runtimeData.ProjectData = new UI_ProjectRuntimeData { FileStruct = data.ProjectFileStruct };
                break;
            case ComponentWindow.Hierarchy:
                runtimeData.HierarchyData = new UI_HierarchyRuntimeData { Scene = objectEntry.Scene };
                break;
            case ComponentWindow.Both:
                runtimeData.ProjectData = new UI_ProjectRuntimeData { FileStruct = data.ProjectFileStruct };
                runtimeData.HierarchyData = new UI_HierarchyRuntimeData { Scene = objectEntry.Scene };
                break;
        }
    }

    // 인스펙터 섹션과 각 슬롯(Reference) 정보를 구성합니다.
    private void BuildSections(UI_InGameEditorRuntimeData runtimeData, SO_InGameEditorInitialPlacementData.ObjectEntry objectEntry)
    {
        foreach (var inspectorComponent in objectEntry.ComponentData.InspectorComponents)
        {
            var section = new UI_RuntimeInspectorSectionData { InspectorComponent = inspectorComponent };
            foreach (var referenceEntry in objectEntry.References)
            {
                if (referenceEntry == null || referenceEntry.InspectorComponent != inspectorComponent) continue;

                section.References.Add(new UI_RuntimeReferenceData
                {
                    SlotId = referenceEntry.SlotId,
                    Label = referenceEntry.Label,
                    ExpectedTargetId = referenceEntry.NormalTargetId,
                    CurrentTargetId = referenceEntry.NormalTargetId,
                    CanSpawnError = referenceEntry.CanSpawnError,
                    IsRequired = referenceEntry.IsRequired
                });
            }
            runtimeData.Sections.Add(section);
        }
    }

    // 유효한 ID인 경우 각 리스트에 등록
    private void RegisterWindow(UI_InGameEditorRuntimeData runtimeData)
    {
        if (runtimeData.ProjectData != null && !_projectItemIds.Contains(runtimeData.Id))
            _projectItemIds.Add(runtimeData.Id);

        if (runtimeData.HierarchyData != null && !_hierarchyItemIds.Contains(runtimeData.Id))
            _hierarchyItemIds.Add(runtimeData.Id);
    }

    // 특정 소유자 ID, 컴포넌트, 슬롯 ID를 조합해 참조 데이터를 정밀하게 찾습니다.
    private UI_RuntimeReferenceData FindReference(string ownerId, InspectorComponent inspectorComponent, string slotId)
    {
        if (!_componentById.TryGetValue(ownerId, out var data)) return null;

        foreach (var section in data.Sections)
        {
            if (section.InspectorComponent != inspectorComponent) continue;
            foreach (var reference in section.References)
            {
                if (reference.SlotId == slotId) return reference;
            }
        }
        return null;
    }

    // 모든 참조 슬롯의 타겟 이름을 실제 표시 이름(DisplayName)으로 갱신합니다.
    private void RefreshAllReferenceDisplayNames()
    {
        foreach (var pair in _componentById)
        {
            foreach (var section in pair.Value.Sections)
            {
                foreach (var reference in section.References)
                {
                    reference.ExpectedTargetDisplayName = ResolveDisplayName(reference.ExpectedTargetId);
                    reference.CurrentTargetDisplayName = ResolveDisplayName(reference.CurrentTargetId);
                }
            }
        }
    }

    // ID 값으로부터 해당 객체의 DisplayName을 찾아 반환합니다.
    private string ResolveDisplayName(string targetId)
    {
        if (string.IsNullOrEmpty(targetId)) return null;
        return _componentById.TryGetValue(targetId, out var runtimeData) ? runtimeData.DisplayName : targetId;
    }
}
