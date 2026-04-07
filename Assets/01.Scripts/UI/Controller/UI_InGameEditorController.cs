using System;
using UnityEngine;

public class InGameEditorController : MonoBehaviour
{
    [Header("컴포넌트 생성")]
    [SerializeField] private Transform _projectComponentContainer;
    [SerializeField] private SO_UIProjectComponentCatalog _projectComponentCatalog;

    [SerializeField] private Transform _hierarchyComponentContainer;
    [SerializeField] private SO_UIHierarchyComponentCatalog _hierarchyComponentCatalog;


    [Header("선택된 컴포넌트 관리")]
    [SerializeField] private GameObject _curSelectedComponent;
    [SerializeField] private GameObject _prevSelectedComponent;

    public Action SelectedActionEvent;
    // 선택 자체는 버튼
    // 드래그 만 따로 빼기!

    public void Init()
    {

    }


    private void ComponentGenerate()
    {

    }
}
