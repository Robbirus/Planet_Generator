// Place this file in an Editor/ folder.
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Custom Inspector for SkillNodeSO.
/// Adds:
///   - A "Generate ID from name" button
///   - A prerequisites chain summary
///   - A warning if the node has no effects
/// </summary>
[CustomEditor(typeof(SkillNodeSO))]
public class SkillNodeSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SkillNodeSO node = (SkillNodeSO)target;

        // ── ID helpers ────────────────────────────────────────────────────────
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("ID Tools");
        if (GUILayout.Button("Generate from name"))
        {
            Undo.RecordObject(node, "Generate Skill ID");
            node.id = node.name.ToLower().Replace(" ", "_");
            EditorUtility.SetDirty(node);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(4);

        // ── Default Inspector ─────────────────────────────────────────────────
        DrawDefaultInspector();

        EditorGUILayout.Space(8);

        // ── Warnings ──────────────────────────────────────────────────────────
        if (node.effects.Count == 0)
            EditorGUILayout.HelpBox("This node has no effects — it won't do anything when unlocked.", MessageType.Warning);

        if (string.IsNullOrEmpty(node.id))
            EditorGUILayout.HelpBox("ID is empty. Click 'Generate from name' or set it manually.", MessageType.Error);

        if (node.costs.Count == 0)
            EditorGUILayout.HelpBox("This node is free (no cost). Is that intentional?", MessageType.Info);

        // ── Prerequisite chain display ────────────────────────────────────────
        if (node.prerequisites.Count > 0)
        {
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Prerequisite chain", EditorStyles.boldLabel);

            foreach (SkillNodeSO prereq in node.prerequisites)
            {
                if (prereq == null)
                {
                    EditorGUILayout.HelpBox("A prerequisite slot is null!", MessageType.Error);
                    continue;
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"  ← {prereq.displayName}  (ID: {prereq.id})");
                if (GUILayout.Button("Select", GUILayout.Width(55)))
                    Selection.activeObject = prereq;
                EditorGUILayout.EndHorizontal();
            }
        }

        // ── Effect summary ────────────────────────────────────────────────────
        if (node.effects.Count > 0)
        {
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Effect summary", EditorStyles.boldLabel);

            foreach (SkillEffect effect in node.effects)
            {
                string sign = effect.value >= 0 ? "+" : "";
                string unit = effect.mode == SkillModifierMode.Ratio ? "%" : "";
                float display = effect.mode == SkillModifierMode.Ratio ? effect.value * 100f : effect.value;
                EditorGUILayout.LabelField($"  {effect.type}  {sign}{display:0.##}{unit}  [{effect.mode}]");
            }
        }
    }
}
#endif