using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnvironmentInstancerController : MonoBehaviour
{
    private Camera m_Camera;
    // Building is done via editor controls, using EnvironmentPropsInstancingPreparator.BuildMeshGroups()
    [SerializeField] private int renderingLayerNumberGlobal = 0;
    [SerializeField] private EnvironmentPropsInstancer[] m_PropMeshGroups = null;

    private RenderParams persistentRenderParams;

    #region MonoBehaviour methods    
    private void Start()
    {
        m_Camera = Camera.main;
        if (null == m_Camera) { Debug.Log("<color=#F5470F>NULL Result for Camera.main</color>"); }
        if (null == m_PropMeshGroups)
        {
            //Debug.Log("<color=#F5470F>NO PROP MESH GROUP FOUND, MAKE SURE TO USE THE EDITOR TOOL TO BUILD THEM</color>");
            return;
        }
        int propMeshCount = m_PropMeshGroups.Length;
        if (0 == propMeshCount)
        {
            //Debug.Log("<color=#F5470F>NO PROP MESH GROUP FOUND, Rebuilding</color>");
            BuildMeshGroupsAtRuntime();
            propMeshCount = m_PropMeshGroups.Length;
        }
        for (int i = 0; i < propMeshCount; i++)
        {
            if (false == m_PropMeshGroups[i].IsDataComplete())
            {
                //Debug.Log("<color=#F5470F>PROP MESH DATA INCOMPLETE, Rebuilding</color>");
                BuildMeshGroupsAtRuntime();
                break;
            }
        }

        for (int i = 0; i < propMeshCount; i++)
        {
            m_PropMeshGroups[i].InstancerStart(m_Camera);
        }
    }
    private void Update()
    {
        // NOTE: This should be independent from GameManager run state. Pausing this will stop all prop rendering

        // We assume the mesh groups will be built no need to check at this stage.
        for (int i = 0; i < m_PropMeshGroups.Length; i++)
        {
            m_PropMeshGroups[i].InstancerUpdate();
        }
    }
    #endregion

    //--------------------------------------------------------------------------------------------------------------------
    private void BuildMeshGroupsAtRuntime()
    {
        try
        {            
            BuildMeshGroups();;
        }
        catch (System.Exception ex)
        {
            Debug.Log($"Failure to rebuild mesh groups: {ex}");
            return;
        }
    }

    //----------------------------------------------------------------------------------------------------------------------
    //--------------------------------------SNIPPET ALSO USED BY EDITOR----------------------------------------------------------
    //----------------------------------------------------------------------------------------------------------------------

    public void BuildMeshGroups()
    {
        Dictionary<string, InstancedDataPreparator> materialCollection = new Dictionary<string, InstancedDataPreparator>();

        InstantiableProp[] allPropsInScene = FindObjectsByType<InstantiableProp>(FindObjectsSortMode.None);
        int propCount = allPropsInScene.Length;
        for (int i = 0; i < propCount; i++) {
            InstantiableProp prop = allPropsInScene[i];
            prop.GetMeshReferenceFromObject();

            if (null == prop.MeshIdentifier) 
                { Debug.Log($"No mesh found for {prop.name}, skipping."); continue; }

            for (int j = 0; j < prop.SharedMaterials.Length; j++) 
            {
                string sharedMatName = prop.SharedMaterials[j].name;

                if (materialCollection.ContainsKey(sharedMatName)) 
                {
                    materialCollection[sharedMatName].UpdateDataWithNewProp(prop, j);
                }
                else 
                {
                    InstancedDataPreparator idp = new InstancedDataPreparator(prop, j);
                    materialCollection.Add(sharedMatName, idp);
                }
            }
        }

        m_PropMeshGroups = new EnvironmentPropsInstancer[materialCollection.Count];
        int itr = 0;
        foreach (KeyValuePair<string, InstancedDataPreparator> kvp in materialCollection)
        {
            InstancedDataPreparator data = kvp.Value;
            m_PropMeshGroups[itr] = new EnvironmentPropsInstancer(
                data.UniqueMaterial,
                data.MeshesForThisUniqueMaterial.ToArray(),
                data.UniqueMeshNamesForThatMat.ToArray(),
                data.ConvertSubmeshIndicesToArrays(),
                data.AssociatedProps.ToArray(),
                transform,
                renderingLayerNumberGlobal
                );

            itr++;
        }
    }
    

    //----------------------------- Inner class for preparing data -------------------------
    public sealed class InstancedDataPreparator
    {
        public Material UniqueMaterial { get; private set; }
        public List<Mesh> MeshesForThisUniqueMaterial { get; private set; }
        public List<InstantiableProp> AssociatedProps { get; private set; }
        public List<string> UniqueMeshNamesForThatMat { get; private set; }
        public Dictionary<string, List<int>> SubmeshIndicesUsingThisMat { get; private set; }   // name of unique mesh name | submesh indices

        public InstancedDataPreparator(InstantiableProp prop, int submeshIndex) 
        {
            UniqueMaterial = prop.SharedMaterials[submeshIndex];
            MeshesForThisUniqueMaterial = new List<Mesh> { prop.MeshIdentifier };
            AssociatedProps = new List<InstantiableProp> { prop };
            UniqueMeshNamesForThatMat = new List<string> { prop.MeshName };
            SubmeshIndicesUsingThisMat = new Dictionary<string, List<int>>();
            SubmeshIndicesUsingThisMat.Add( prop.MeshName, new List<int> { submeshIndex });
        }

        public void UpdateDataWithNewProp(InstantiableProp prop, int submeshIndex) 
        {
            AssociatedProps.Add(prop);
            string meshName = prop.MeshName;
            if (false == UniqueMeshNamesForThatMat.Contains(meshName)) 
            {
                UniqueMeshNamesForThatMat.Add(meshName);
                MeshesForThisUniqueMaterial.Add(prop.MeshIdentifier);
                SubmeshIndicesUsingThisMat.Add(meshName, new List<int> { submeshIndex });
            }
            else if (false == SubmeshIndicesUsingThisMat[meshName].Contains(submeshIndex))
            {
                // For the special case where the same material is used on different submeshes.
                SubmeshIndicesUsingThisMat[meshName].Add(submeshIndex);
            }
        }

        public int[][] ConvertSubmeshIndicesToArrays() 
        {
            // UniqueMeshNamesForThatMat and SubmeshIndicesUsingThisMat should have the same amount of elements
            int[][] tempIndicesArrays = new int[UniqueMeshNamesForThatMat.Count][];
            int count = UniqueMeshNamesForThatMat.Count;
            for(int i = 0; i < count; i++)
            {
                tempIndicesArrays[i] = (SubmeshIndicesUsingThisMat[UniqueMeshNamesForThatMat[i]]).ToArray();
            }
            return tempIndicesArrays;

        }
    }
}
