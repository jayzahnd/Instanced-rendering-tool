using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class EnvironmentPropsInstancer
{
    // A prop instancer object revolves around a single shared material share by all the meshes it affects
    [SerializeField] private Material m_SharedMaterial = null;
    private Camera m_Camera;

    [SerializeField] private UnityEngine.Rendering.ShadowCastingMode doCastShadows = UnityEngine.Rendering.ShadowCastingMode.On;
    [SerializeField] private bool doReceiveShadows = true;
    [SerializeField] private UnityEngine.Rendering.LightProbeUsage lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.BlendProbes;
    private int m_RenderingLayerNumber = 0;
    [Space]
    private Transform m_ControllerTransform;
    private Mesh[] m_MeshesUsingThisMaterial = null;
    private string[] m_MeshNames; // Should switch to id number stored as a short.
    private int[][] m_SubMeshIndices; // Used to know on which submesh the shared materials should be rendered
    private InstantiableProp[] m_AllPropsForThisMaterial = null;

    private Dictionary<string, Matrix4x4[]> m_AllPropGroups;

    private RenderParams persistentRenderParams;

    public EnvironmentPropsInstancer(Material sharedMaterial, Mesh[] meshes, string[] meshNames, int[][] subMeshIndices, InstantiableProp[] props, Transform controllerTransform, int renderLayer)
    {
        m_ControllerTransform = controllerTransform;
        m_SharedMaterial = sharedMaterial;
        m_MeshesUsingThisMaterial = meshes;
        m_MeshNames = meshNames;
        m_SubMeshIndices = subMeshIndices;
        m_AllPropsForThisMaterial = props;
        m_RenderingLayerNumber = renderLayer;
    }
    
    public void InstancerStart(Camera cam)
    {
        m_Camera = cam;

        persistentRenderParams = new RenderParams(m_SharedMaterial);
        // Currently not going for a compute shader, but still needed for DrawMeshInstanced
        // MaterialPropertyBlock so you can access the custom StructuredBuffer in the shader
        persistentRenderParams.matProps = new MaterialPropertyBlock();

        //persistentRenderParams.matProps.
        if (null == m_MeshesUsingThisMaterial)
        {
            Debug.LogError("<color=red>No meshes found to build the matrices, aborting...</color>");
            return;
        }

        int meshCount = m_MeshesUsingThisMaterial.Length;
        m_AllPropGroups = new Dictionary<string, Matrix4x4[]>(meshCount);
        Dictionary<string, List<Matrix4x4>> tempDict = new Dictionary<string, List<Matrix4x4>>(meshCount);

        int propCount = m_AllPropsForThisMaterial.Length;

        for (int i = 0; i < propCount; i++)
        {
            InstantiableProp childInstancedProp = m_AllPropsForThisMaterial[i];
            string meshID = childInstancedProp.MeshIdentifier.name;

            // Some props have varying scale, so the best is to simply get the object world matrix obtained from the Renderer.localToWorldMatrix function
            Matrix4x4 tempMatrix4X4 = childInstancedProp.PropMatrix;

            if (tempDict.ContainsKey(meshID))
            {
                tempDict[meshID].Add(tempMatrix4X4);
            }
            else
            {
                tempDict.Add(meshID, new List<Matrix4x4>() { tempMatrix4X4 });
            }
        }

        foreach (KeyValuePair<string, List<Matrix4x4>> entry in tempDict)
        {
            m_AllPropGroups.Add(entry.Key, entry.Value.ToArray());
        }
    }

    public void InstancerUpdate()
    {
        // Don't update without a material. We may not really need this check, but at least we'll immediately  see what doesn't get rendered
        if (null == m_SharedMaterial) { return; }

        Camera mainCam = m_Camera;
        // RenderMeshInstanced is Bugged in version 2021.3.19f1 of Unity :( generating tons of garbage. We're using the old DrawMeshInstanced instead
        int meshLength = m_MeshesUsingThisMaterial.Length;
        int idx = 0;
        do
        {
            //string meshName = m_MeshesUsingThisMaterial[idx].name;
            // Iterate over every submesh of each mesh
            for(int i = 0; i < m_SubMeshIndices[idx].Length; i++) {
                Graphics.DrawMeshInstanced(
                m_MeshesUsingThisMaterial[idx]
                , m_SubMeshIndices[idx][i]
                , m_SharedMaterial
                , m_AllPropGroups[m_MeshNames[idx]]
                , m_AllPropGroups[m_MeshNames[idx]].Length
                , persistentRenderParams.matProps
                , doCastShadows
                , doReceiveShadows
                , m_RenderingLayerNumber
                , mainCam
                , lightProbeUsage);
            }
            idx++;
        }
        while (idx < meshLength);
    }

    public bool IsDataComplete()
    {
        return !(null == m_MeshesUsingThisMaterial || null == m_SharedMaterial || null == m_AllPropsForThisMaterial );
    }

}

