using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;

// Create a custom editor to change a planet's characteristics
[CustomEditor(typeof(Planet))]
public class PlanetEditor : Editor
{
    Planet planet;
    Editor shapeEditor;
    Editor colourEditor;

    public override void OnInspectorGUI()
    {
        using (var check = new EditorGUI.ChangeCheckScope())
        {
            base.OnInspectorGUI();
            if (check.changed)
            {
                planet.GeneratePlanet();
                PersistGeneratedMeshes();
            }
        }

        if (GUILayout.Button("Generate Planet"))
        {
            planet.GeneratePlanet();
            PersistGeneratedMeshes();
        }

        DrawSettingsEditor(planet.shapeSettings, () =>
        {
            planet.OnShapeSettingsUpdated();
            PersistGeneratedMeshes();
        }, ref planet.shapeSettingsFoldout, ref shapeEditor);

        DrawSettingsEditor(planet.colourSettings, () =>
        {
            planet.OnColourSettingsUpdated();
            PersistGeneratedMeshes();
        }, ref planet.colourSettingsFoldout, ref colourEditor);
    }

    // With this we can check if the planet's settings have been changed in the editor
    // and update the planet accordingly
    void DrawSettingsEditor(Object settings, System.Action onSettingsUpdated, ref bool foldout, ref Editor editor)
    {
        if (settings != null)
        {
            foldout = EditorGUILayout.InspectorTitlebar(foldout, settings); // Create titlebar
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                if (foldout)
                {
                    CreateCachedEditor(settings, null, ref editor);
                    editor.OnInspectorGUI();
                }

                if (check.changed)
                {
                    if (onSettingsUpdated != null)
                    {
                        onSettingsUpdated();
                    }
                }
            }
        }
    }

    /// <summary>
    /// The meshes created by Planet.Initialize() (new Mesh()) only exist in
    /// memory: they are never written to the file. prefab as long as they
    /// are not explicitly recorded as sub-assets via
    /// AssetDatabase. Without that, "Generate Planet" works visually in
    /// session, but after saving + reloading (closing Unity, exit
    /// from the prefab edition mode...), MeshFilter.sharedMesh becomes "None" again.
    ///
    /// This method only has an effect when editing a Prefab Asset
    /// directly (and not an instance in a scene, where this problem does not occur)
    /// does not pose since nothing needs to survive a reload of
    /// domain while the game is running).
    /// </summary>
    private void PersistGeneratedMeshes()
    {
        string path = AssetDatabase.GetAssetPath(planet.gameObject);

        // Not a prefab asset (e.g., instance in a scene) -> nothing to persist.
        if (string.IsNullOrEmpty(path) || !path.EndsWith(".prefab"))
            return;

        bool dirty = false;

        foreach (Mesh mesh in planet.GetGeneratedMeshes())
        {
            if (!AssetDatabase.Contains(mesh))
            {
                AssetDatabase.AddObjectToAsset(mesh, path);
                dirty = true;
            }
        }

        if (dirty)
        {
            AssetDatabase.ImportAsset(path);
            AssetDatabase.SaveAssets();
        }
    }

    private void OnEnable()
    {
        planet = (Planet)target;
    }
}