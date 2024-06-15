using System.Collections.Generic;
using UnityEngine;

// previously ConvertibleProp_v2
public class InstantiableProp : MonoBehaviour
{
    public Mesh MeshIdentifier { get { return meshRef; } }
    public string MeshName { get { return meshName; } }
    public Material[] SharedMaterials { get { return sharedMaterials; } }
    public Matrix4x4 PropMatrix { get; private set; }

    [SerializeField] private Mesh meshRef;
    private string meshName;
    private Material[] sharedMaterials;

    private void Awake()
    {
        if(GetMeshReferenceFromObject())
        {
            PropMatrix = GetComponent<MeshRenderer>().localToWorldMatrix;
        }
    }

    public bool GetMeshReferenceFromObject()
    {
        bool isSuccess = false;
        if (gameObject.TryGetComponent(out MeshFilter meshFilter) && gameObject.TryGetComponent(out MeshRenderer meshRenderer))
        {
            meshRef = meshFilter.sharedMesh; // shared mesh => reference. mesh => concrete instance
            meshName = meshFilter.sharedMesh.name;
            sharedMaterials = meshRenderer.sharedMaterials;
            isSuccess = true;
        }
        return isSuccess;
    }
}

