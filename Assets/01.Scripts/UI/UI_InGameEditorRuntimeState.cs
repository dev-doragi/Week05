using System.Collections.Generic;
using UnityEngine;

// 게임 내 에디터(가짜 유니티 에디터)의 런타임(실행 중) 상태를 중앙에서 관리하는 클래스입니다.
// 아이템 목록, 현재 선택된 객체, 씬 정보, 그리고 인스펙터 참조(Reference) 오류 상태를 추적합니다.
public class UI_InGameEditorRuntimeState
{
    // 모든 에디터 컴포넌트 데이터를 ID로 빠르게 찾기 위해 캐싱해두는 딕셔너리
    private readonly Dictionary<string, UI_InGameEditorRuntimeData> _componentById = new();

    // 프로젝트 창과 하이라키 창에 각각 표시될 아이템들의 ID 목록
    private readonly List<string> _projectItemIds = new();
    private readonly List<string> _hierarchyItemIds = new();

    // 현재 선택된 객체들의 ID 및 현재 활성화된 씬 정보
    public string SelectedRuntimeComponentId { get; private set; }
    public string SelectedProjectComponentId { get; private set; }
    public string SelectedHierarchyComponentId { get; private set; }

    public UIEditorScene CurrentScene { get; private set; }

    // 에디터 초기화: ScriptableObject 데이터를 읽어와 런타임 데이터를 생성하고 목록에 등록합니다.
    public void Init(SO_InGameEditorInitialPlacementData initialPlacementData)
    {
        _componentById.Clear();
        _projectItemIds.Clear();
        _hierarchyItemIds.Clear();

        SelectedRuntimeComponentId = null;
        SelectedProjectComponentId = null;
        SelectedHierarchyComponentId = null;
        CurrentScene = UIEditorScene.None;

        if (initialPlacementData == null)
            return;

        bool hasScene = false;

        foreach (var objectEntry in initialPlacementData.Objects)
        {
            if (objectEntry == null || string.IsNullOrEmpty(objectEntry.ObjectId) || objectEntry.ComponentData == null)
                continue;

            // 원본 데이터(SO)를 바탕으로 런타임에서 조작할 수 있는 데이터 객체를 생성
            var runtimeData = CreateRuntimeData(objectEntry);

            _componentById[runtimeData.Id] = runtimeData;
            RegisterWindow(runtimeData);

            // 첫 번째로 발견된 하이라키 객체의 씬을 현재 씬으로 설정합니다.
            if (hasScene == false && runtimeData.HierarchyData != null)
            {
                CurrentScene = runtimeData.HierarchyData.Scene;
                hasScene = true;
            }
        }
    }

    // 현재 활성화된 씬을 변경합니다.
    // 현재 사용되지 않음
    public void SetCurrentScene(UIEditorScene scene)
    {
        CurrentScene = scene;
    }

    // 프로젝트 창에서 아이템을 선택했을 때 호출됩니다.
    public void SelectFromProject(string componentId)
    {
        SelectedRuntimeComponentId = componentId;
        SelectedProjectComponentId = componentId;
        SelectedHierarchyComponentId = null; // 하이라키 선택 상태는 해제
    }

    // 하이라키 창에서 아이템을 선택했을 때 호출됩니다.
    public void SelectFromHierarchy(string componentId)
    {
        SelectedRuntimeComponentId = componentId;
        SelectedProjectComponentId = null; // 프로젝트 선택 상태는 해제
        SelectedHierarchyComponentId = componentId;
    }

    // 현재 선택된 객체의 런타임 데이터를 반환합니다.
    public UI_InGameEditorRuntimeData GetSelectedRuntimeData()
    {
        if (string.IsNullOrEmpty(SelectedRuntimeComponentId))
            return null;

        _componentById.TryGetValue(SelectedRuntimeComponentId, out var data);
        return data;
    }

    // 프로젝트 창에 표시할 아이템 목록을 반환합니다.
    public IReadOnlyList<UI_InGameEditorRuntimeData> GetProjectItems()
    {
        var items = new List<UI_InGameEditorRuntimeData>();

        foreach (var id in _projectItemIds)
        {
            if (_componentById.TryGetValue(id, out var data))
                items.Add(data);
        }

        return items;
    }

    // 하이라키 창에 표시할 아이템 목록을 반환합니다. (현재 씬에 속한 아이템만 필터링)
    public IReadOnlyList<UI_InGameEditorRuntimeData> GetHierarchyItems()
    {
        var items = new List<UI_InGameEditorRuntimeData>();

        foreach (var id in _hierarchyItemIds)
        {
            if (_componentById.TryGetValue(id, out var data) == false)
                continue;

            if (data.HierarchyData == null)
                continue;

            // 다른 씬의 객체는 하이라키에 표시하지 않습니다.
            if (data.HierarchyData.Scene != CurrentScene)
                continue;

            items.Add(data);
        }

        return items;
    }

    // 인위적으로 참조 오류를 발생시킬 수 있는(정답이 연결된) 참조 슬롯 목록
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
                    if (reference.CanSpawnError == false)
                        continue;

                    if (reference.IsCorrect() == false)
                        continue;

                    keys.Add(new UI_RuntimeReferenceKey
                    {
                        OwnerId = runtimeData.Id,
                        SlotId = reference.SlotId
                    });
                }
            }
        }

        return keys;
    }

    // 문제 출제용. 특정 참조 연결 끊기
    public bool TryBreakReference(string ownerId, string slotId)
    {
        var reference = FindReference(ownerId, slotId);

        if (reference == null || reference.CanSpawnError == false)
            return false;

        reference.CurrentTargetId = null;
        return true;
    }

    // 랜덤문제 출제용. 아무거나 연결 참조 하나 끊기
    public bool TryBreakRandomReference(out UI_RuntimeReferenceKey brokenKey)
    {
        brokenKey = null;

        var candidates = GetBreakableReferenceKeys();
        if (candidates.Count == 0)
            return false;

        int randomIndex = Random.Range(0, candidates.Count);
        var candidate = candidates[randomIndex];

        if (TryBreakReference(candidate.OwnerId, candidate.SlotId) == false)
            return false;

        brokenKey = candidate;
        return true;
    }

    // 끊어졌던 참조 슬롯을 원래의 올바른 상태(Expected Target)로 복구
    public bool TryRestoreReference(string ownerId, string slotId)
    {
        var reference = FindReference(ownerId, slotId);

        if (reference == null)
            return false;

        reference.CurrentTargetId = reference.ExpectedTargetId;
        return true;
    }

    // 플레이어가 특정 슬롯에 새로운 타겟 객체를 할당할 때 호출
    public bool TryAssignReference(string ownerId, string slotId, string targetId)
    {
        var reference = FindReference(ownerId, slotId);

        if (reference == null)
            return false;

        reference.CurrentTargetId = targetId;
        return true;
    }

    // 에디터 내의 모든 컴포넌트를 검사하여 잘못된 참조(Missing 또는 Wrong)가 있는지 요약 리포트를 반환
    // 정답 체크
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
                    if (errorType == UI_ReferenceValidationErrorType.None)
                        continue;

                    // 오류가 발견되면 리포트에 기록
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

    // 해당 참조 데이터가 어떤 종류의 오류(Missing, Wrong)를 가지고 있는지 판별
    private UI_ReferenceValidationErrorType GetErrorType(UI_RuntimeReferenceData reference)
    {
        if (reference == null)
            return UI_ReferenceValidationErrorType.None;

        if (reference.IsMissing())
            return UI_ReferenceValidationErrorType.MissingReference;

        if (reference.IsWrongReference())
            return UI_ReferenceValidationErrorType.WrongReference;

        return UI_ReferenceValidationErrorType.None;
    }

    // ==========================================
    // 아래는 데이터 초기화를 위한 내부 헬퍼 메서드
    // ==========================================

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

    // 객체가 프로젝트 창용인지, 하이라키 창용인지 판별하여 해당 데이터를 세팅
    private void BuildWindowData(UI_InGameEditorRuntimeData runtimeData, SO_InGameEditorInitialPlacementData.ObjectEntry objectEntry)
    {
        switch (runtimeData.SourceData.ComponentWindow)
        {
            case ComponentWindow.Project:
                runtimeData.ProjectData = new UI_ProjectRuntimeData { FileStruct = runtimeData.SourceData.ProjectFileStruct };
                break;

            case ComponentWindow.Hierarchy:
                runtimeData.HierarchyData = new UI_HierarchyRuntimeData { Scene = objectEntry.Scene };
                break;

            case ComponentWindow.Both:
                runtimeData.ProjectData = new UI_ProjectRuntimeData { FileStruct = runtimeData.SourceData.ProjectFileStruct };
                runtimeData.HierarchyData = new UI_HierarchyRuntimeData { Scene = objectEntry.Scene };
                break;
        }
    }

    // 인스펙터 창에 표시될 섹션들(컴포넌트들)과 각 슬롯(Reference)들의 런타임 데이터를 생성
    private void BuildSections(UI_InGameEditorRuntimeData runtimeData, SO_InGameEditorInitialPlacementData.ObjectEntry objectEntry)
    {
        foreach (var inspectorComponent in objectEntry.ComponentData.InspectorComponents)
        {
            var section = new UI_RuntimeInspectorSectionData
            {
                InspectorComponent = inspectorComponent
            };

            foreach (var referenceEntry in objectEntry.References)
            {
                if (referenceEntry == null || referenceEntry.InspectorComponent != inspectorComponent)
                    continue;

                section.References.Add(new UI_RuntimeReferenceData
                {
                    SlotId = referenceEntry.SlotId,
                    Label = referenceEntry.Label,
                    ExpectedTargetId = referenceEntry.NormalTargetId,
                    CurrentTargetId = referenceEntry.NormalTargetId, // 초기값은 정상 타겟으로 설정
                    CanSpawnError = referenceEntry.CanSpawnError,
                    IsRequired = referenceEntry.IsRequired
                });
            }

            runtimeData.Sections.Add(section);
        }
    }

    // 생성된 객체를 프로젝트/하이라키 목록(List)에 등록합니다. 중복 등록을 방지
    private void RegisterWindow(UI_InGameEditorRuntimeData runtimeData)
    {
        if (runtimeData.ProjectData != null && _projectItemIds.Contains(runtimeData.Id) == false)
            _projectItemIds.Add(runtimeData.Id);

        if (runtimeData.HierarchyData != null && _hierarchyItemIds.Contains(runtimeData.Id) == false)
            _hierarchyItemIds.Add(runtimeData.Id);
    }

    // 특정 객체(ownerId)가 가진 인스펙터 컴포넌트들 중에서 특정 슬롯(slotId)을 찾아 반환
    private UI_RuntimeReferenceData FindReference(string ownerId, string slotId)
    {
        if (_componentById.TryGetValue(ownerId, out var data) == false)
            return null;

        foreach (var section in data.Sections)
        {
            foreach (var reference in section.References)
            {
                if (reference.SlotId == slotId)
                    return reference;
            }
        }

        return null;
    }
}