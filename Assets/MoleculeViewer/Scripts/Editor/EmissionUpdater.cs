using UnityEngine;
using UnityEditor;

public class MatchEmissionToBaseColor
{
    // Adds a clickable option under the "Assets" menu and right-click context menu
    [MenuItem("Assets/Match Emission to Base Color", false, 100)]
    private static void MatchEmission()
    {
        // Get all currently selected objects in the Project window
        Object[] selectedObjects = Selection.objects;
        int modificationCount = 0;

        foreach (Object obj in selectedObjects)
        {
            // Check if the selected asset is actually a Material
            if (obj is Material material)
            {
                // Record the object for Undo functionality in the editor
                Undo.RecordObject(material, "Match Emission to Base Color");

                // Standard Unity Shaders look for "_BaseColor" (URP/HDRP) or "_Color" (Built-in)
                Color baseColor = Color.white;
                bool colorFound = false;

                if (material.HasProperty("_BaseColor"))
                {
                    baseColor = material.GetColor("_BaseColor");
                    colorFound = true;
                }
                else if (material.HasProperty("_Color"))
                {
                    baseColor = material.GetColor("_Color");
                    colorFound = true;
                }

                if (colorFound)
                {
                    // Enable the Emission keyword on the material
                    material.EnableKeyword("_EMISSION");
                    
                    // Set the emission color property
                    material.SetColor("_EmissionColor", baseColor);
                    
                    // Tell Unity the material properties have changed so it updates visually
                    EditorUtility.SetDirty(material);
                    modificationCount++;
                }
            }
        }

        // Display a summary message in the Unity Console
        Debug.Log($"Successfully synchronized emission color on {modificationCount} material(s).");
    }

    // Validation method: Disables the menu item if you haven't selected anything
    [MenuItem("Assets/Match Emission to Base Color", true)]
    private static bool ValidateMatchEmission()
    {
        return Selection.objects.Length > 0;
    }
}