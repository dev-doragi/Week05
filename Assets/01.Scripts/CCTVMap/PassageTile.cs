using UnityEngine;

public class PassageTile : MonoBehaviour
{
    [SerializeField] private Renderer[] renderers;
    [SerializeField] private Material openMaterial;
    [SerializeField] private Material blockedMaterial;
    [SerializeField] private bool blockedOnStart = false;

    public bool IsBlocked { get; private set; }

    private void Awake()
    {
        SetBlocked(blockedOnStart);
    }

    public void OnMinimapClicked()
    {
        SetBlocked(!IsBlocked);
    }

    private void SetBlocked(bool blocked)
    {
        IsBlocked = blocked;
        ApplyMaterial(blocked ? blockedMaterial : openMaterial);

    }

    private void ApplyMaterial(Material targetMat)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer r = renderers[i];
            if (r == null) continue;
            r.sharedMaterial = targetMat;
        }
    }
}
