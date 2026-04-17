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
            }
        }

        if (GUILayout.Button("Generate Planet"))
        {
            planet.GeneratePlanet();
        }

        DrawSettingsEditor(planet.shapeSettings, planet.OnShapeSettingsUpdated, ref planet.shapeSettingsFoldout, ref shapeEditor);
        DrawSettingsEditor(planet.colourSettings, planet.OnColourSettingsUpdated, ref planet.colourSettingsFoldout, ref colourEditor);
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

    private void OnEnable()
    {
        planet = (Planet)target;
    }
}
