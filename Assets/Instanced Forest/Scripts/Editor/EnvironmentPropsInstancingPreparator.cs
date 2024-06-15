using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EnvironmentPropsInstancingPreparator : Editor
{
    [MenuItem("Level Editor/Mesh Instancing/Build prop mesh groups from convertible props in the scene")]
    private static void BuildMeshGroupsInEditor()
    {
        EnvironmentInstancerController[] environmentInstancerControllers = FindObjectsByType<EnvironmentInstancerController>(FindObjectsSortMode.None);
        if (1 < environmentInstancerControllers.Length)
        {
            EditorUtility.DisplayDialog("Mesh groups generator", $"Failure\nMore than one EnvironmentInstancerController in scene", "OK", "");
            return;
        }
        EnvironmentInstancerController environmentInstancerController = environmentInstancerControllers[0];

        try
        {            
            environmentInstancerController.BuildMeshGroups();// tempUniqueMaterials, tempMeshDict, tempAssociatedProps);
        }
        catch (System.Exception ex)
        {
            EditorUtility.DisplayDialog("Mesh groups generator", $"Failure\n{ex}", "OK", "");
            return;
        }

    }

    [MenuItem("Level Editor/Mesh Instancing/Convert meshed objects to points (DisableRenderer)")]
    private static void MeshReplacement()
    {
        InstantiableProp[] instancedPropsInScene = FindObjectsByType<InstantiableProp>(FindObjectsSortMode.None);
        if (null == instancedPropsInScene)
        {
            EditorUtility.DisplayDialog("Mesh conversion to points", $"Failure\nNo valid object with found with an InstancedProp component", "OK", "");
            return;
        }
        int propCount = instancedPropsInScene.Length;
        if (0 == propCount)
        {
            EditorUtility.DisplayDialog("Mesh conversion to points", $"Failure\nNo valid object with found with an InstancedProp component", "OK", "");
            return;
        }
        int idx = 0;
        do
        {
            // If we have a meshrenderer, fetch the mesh reference from convertibleProp, then disable the renderer
            InstantiableProp prop = instancedPropsInScene[idx];
            instancedPropsInScene[idx].GetMeshReferenceFromObject();

            if (null == prop.MeshIdentifier) { Debug.Log($"No mesh found for {prop.name}, skipping."); continue; }

            prop.gameObject.GetComponent<MeshRenderer>().enabled = false;

            idx++;
        }
        while (idx < propCount);

        EditorUtility.DisplayDialog("Mesh conversion to points.", $"Completed!!\nMeshes should now be replaced", "OK", "");
    }

    [MenuItem("Level Editor/Mesh Instancing/Convert points back to meshed objects (EnableRenderer)")]
    public static void ReenableMesh()
    {
        InstantiableProp[] instancedPropsInScene = FindObjectsByType<InstantiableProp>(FindObjectsSortMode.None);
        if (null == instancedPropsInScene)
        {
            EditorUtility.DisplayDialog("\"Mesh restoration from points.", $"Failure\nNo valid object with found with an InstancedProp component", "OK", "");
            return;
        }
        int propCount = instancedPropsInScene.Length;
        if (0 == propCount)
        {
            EditorUtility.DisplayDialog("\"Mesh restoration from points.", $"Failure\nNo valid object with found with an InstancedProp component", "OK", "");
            return;
        }
        int idx = 0;
        do
        {
            // If we have a meshrenderer, fetch the mesh reference from convertibleProp, then enable the renderer
            InstantiableProp prop = instancedPropsInScene[idx];
            instancedPropsInScene[idx].GetMeshReferenceFromObject();

            if (null == prop.MeshIdentifier) { Debug.Log($"No mesh found for {prop.name}, skipping."); continue; }

            prop.gameObject.GetComponent<MeshRenderer>().enabled = true;

            idx++;
        }
        while (idx < propCount);

        EditorUtility.DisplayDialog("Mesh restoration from points.", $"Completed!!\nMeshes should now be reenabled", "OK", "");
    }
}
