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

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        DrawSettingsEditor(planet.shapeSettings, planet.OnShapeSettingsUpdated);
        DrawSettingsEditor(planet.colourSettings, planet.OnColourSettingsUpdated);
    }

    // With this we can check if the planet's settings have been changed in the editor
    // and update the planet accordingly
    void DrawSettingsEditor(Object settings, System.Action onSettingsUpdated)
    {
        using (var check = new EditorGUI.ChangeCheckScope()) 
        {
            Editor editor = CreateEditor(settings);
            editor.OnInspectorGUI();

            if (check.changed) 
            {
                if (onSettingsUpdated != null)
                { 
                    onSettingsUpdated();
                }
                
            }
        }
    }

    private void OnEnable()
    {
        planet = (Planet)target;
    }
}
