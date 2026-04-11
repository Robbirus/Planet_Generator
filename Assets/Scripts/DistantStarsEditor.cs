using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DistantStars))]
public class DistantStarsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        DistantStars script = (DistantStars)target;

        if (GUILayout.Button("Generate Stars"))
        {
            script.GenerateStars();
        }
    }
}