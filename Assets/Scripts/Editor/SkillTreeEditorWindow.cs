// Place this file in an Editor/ folder — it will not be included in builds.
#if UNITY_EDITOR
using System.Collections.Generic;
using TreeEditor;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor window that displays a SkillTreeSO as a node graph.
/// Open via: Window → Skill Tree Editor
///
/// Features:
///   - Visualises all nodes and connections
///   - Highlights duplicate IDs and missing prerequisites in red
///   - Lets you click a node to ping the ScriptableObject in the Project window
/// </summary>
public class SkillTreeEditorWindow : EditorWindow
{
    private SkillTreeSO tree;
    private Vector2 scrollOffset;
    private Vector2 scrollPos;
    private float zoom = 1f;

    private SerializedObject serializedObject;
    private SerializedProperty treeProp;

    private const float NODE_W = 160f;
    private const float NODE_H = 60f;

    [MenuItem("Window/Skill Tree Editor")]
    public static void Open()
    {
        GetWindow<SkillTreeEditorWindow>("Skill Tree Editor");
    }

    private void OnGUI()
    {
        DrawToolbar();

        if (tree == null)
        {
            EditorGUILayout.HelpBox("Assign a SkillTreeSO to visualise the tree.", MessageType.Info);
            return;
        }

        // Validate before drawing
        List<string> duplicates = tree.FindDuplicateIDs();

        if (duplicates.Count > 0)
        {
            EditorGUILayout.HelpBox(
                $"Duplicate IDs found: {string.Join(", ", duplicates)}\n" +
                "Each node must have a unique ID.", MessageType.Error);
        }

        DrawCanvas();
    }

    private void OnEnable()
    {
        serializedObject = new SerializedObject(this);
        treeProp = serializedObject.FindProperty("tree");
    }

    // ── Toolbar ───────────────────────────────────────────────────────────────

    private void DrawToolbar()
    {
        serializedObject.Update();

        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        EditorGUILayout.PropertyField(treeProp, new GUIContent("Skill Tree"), GUILayout.Width(300f));

        zoom = EditorGUILayout.Slider("Zoom", zoom, 0.4f, 2f, GUILayout.Width(200f));

        if (GUILayout.Button("Reset View", EditorStyles.toolbarButton))
        {
            scrollOffset = Vector2.zero;
            zoom = 1f;
        }

        if (GUILayout.Button("Refresh", EditorStyles.toolbarButton))
            Repaint();

        EditorGUILayout.EndHorizontal();

        serializedObject.ApplyModifiedProperties();
    }

    // ── Canvas ────────────────────────────────────────────────────────────────

    private void DrawCanvas()
    {
        Rect canvasRect = new Rect(0, EditorGUIUtility.singleLineHeight + 4, position.width, position.height - EditorGUIUtility.singleLineHeight - 4);

        // Scroll / pan
        scrollPos = GUI.BeginScrollView(canvasRect, scrollPos,
            new Rect(0, 0, 3000f * zoom, 3000f * zoom));

        // Draw connections first (so nodes appear on top)
        DrawConnections();

        // Draw nodes
        foreach (SkillNodeSO node in tree.nodes)
        {
            if (node != null)
                DrawNode(node);
        }

        GUI.EndScrollView();

        // Handle pan with middle mouse
        HandlePan(canvasRect);
    }

    private void DrawConnections()
    {
        foreach (SkillNodeSO node in tree.nodes)
        {
            if (node == null) continue;

            foreach (SkillNodeSO prereq in node.prerequisites)
            {
                if (prereq == null) continue;

                Vector2 from = NodeCenter(prereq) * zoom;
                Vector2 to = NodeCenter(node) * zoom;

                Handles.color = Color.gray;
                Handles.DrawLine(from, to);

                // Arrow head
                Vector2 dir = (to - from).normalized;
                Vector2 tip = to - dir * (NODE_H * 0.5f * zoom);
                Handles.DrawSolidDisc(tip, Vector3.forward, 4f * zoom);
            }
        }
    }

    private void DrawNode(SkillNodeSO node)
    {
        Rect rect = NodeRect(node);
        Rect scaled = new Rect(rect.x * zoom, rect.y * zoom, rect.width * zoom, rect.height * zoom);

        bool hasDuplicateId = tree.FindDuplicateIDs().Contains(node.id);

        // Background
        Color bg = hasDuplicateId ? new Color(0.8f, 0.2f, 0.2f) : new Color(0.25f, 0.25f, 0.25f);
        EditorGUI.DrawRect(scaled, bg);

        // Border
        GUI.color = hasDuplicateId ? Color.red : Color.gray;
        GUI.Box(scaled, GUIContent.none);
        GUI.color = Color.white;

        // Content
        GUILayout.BeginArea(scaled);
        GUILayout.Label(node.displayName, EditorStyles.boldLabel);
        GUILayout.Label($"ID: {node.id}", EditorStyles.miniLabel);
        GUILayout.Label($"Effects: {node.effects.Count}  Cost entries: {node.costs.Count}", EditorStyles.miniLabel);
        GUILayout.EndArea();

        // Click to ping the asset
        if (GUI.Button(scaled, GUIContent.none, GUIStyle.none))
        {
            EditorGUIUtility.PingObject(node);
            Selection.activeObject = node;
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private Rect NodeRect(SkillNodeSO node)
    {
        return new Rect(
            node.canvasPosition.x - NODE_W * 0.5f + 100f,
            -node.canvasPosition.y - NODE_H * 0.5f + 100f,
            NODE_W, NODE_H);
    }

    private Vector2 NodeCenter(SkillNodeSO node)
    {
        Rect r = NodeRect(node);
        return new Vector2(r.center.x, r.center.y);
    }

    private void HandlePan(Rect area)
    {
        Event e = Event.current;
        if (e.type == EventType.MouseDrag && e.button == 2 && area.Contains(e.mousePosition))
        {
            scrollPos -= e.delta;
            e.Use();
            Repaint();
        }
    }
}
#endif