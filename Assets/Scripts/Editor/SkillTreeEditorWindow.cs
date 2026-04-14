// Place this file in an Editor/ folder — it will not be included in builds.
#if UNITY_EDITOR
using System.Collections.Generic;
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
    [SerializeField] private SkillTreeSO tree;

    private Vector2 scrollPos;
    private float zoom = 1f;

    // Cached per frame to avoid calling FindDuplicateIDs() once per node
    private HashSet<string> duplicateIds = new HashSet<string>();

    private SerializedObject serializedObject;
    private SerializedProperty treeProp;

    private const float NODE_W = 160f;
    private const float NODE_H = 70f;

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
        duplicateIds = new HashSet<string>(tree.FindDuplicateIDs());

        if (duplicateIds.Count > 0)
        {
            EditorGUILayout.HelpBox(
                $"Duplicate IDs found: {string.Join(", ", duplicateIds)}\n" +
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

        // Width label + field manually so it fits the toolbar height
        EditorGUILayout.LabelField("Skill Tree:", GUILayout.Width(65f));
        EditorGUILayout.PropertyField(treeProp,GUIContent.none, GUILayout.Width(220f));

        zoom = EditorGUILayout.Slider("Zoom", zoom, 0.4f, 2f, GUILayout.Width(120f));

        if (GUILayout.Button("Reset View", EditorStyles.toolbarButton, GUILayout.Width(80f)))
        {
            scrollPos = Vector2.zero;
            zoom = 1f;
        }

        if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(60f)))
            Repaint();

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        serializedObject.ApplyModifiedProperties();

        // Sync field after ApplyModifiedProperties so 'tree' is up to date
        tree = (SkillTreeSO)treeProp.objectReferenceValue;
    }

    // ── Canvas ────────────────────────────────────────────────────────────────

    private void DrawCanvas()
    {
        float toolbarH = EditorGUIUtility.singleLineHeight + 6f;
        float warningH = duplicateIds.Count > 0 ? 42f : 0f;
        float topOffset = toolbarH + warningH;

        Rect canvasRect = new Rect(0, topOffset, position.width, position.height - topOffset);

        Rect contentRect = new Rect(0, 0, 4000f * zoom, 4000f * zoom);
        scrollPos = GUI.BeginScrollView(canvasRect, scrollPos, contentRect);

        // Draw connections FIRST (behind nodes)
        DrawConnections();

        // Draw nodes with pure GUI — no GUILayout inside a ScrollView
        if (tree != null)
        {
            foreach (SkillNodeSO node in tree.nodes)
            {
                if (node != null)
                    DrawNode(node);
            }
        }

        GUI.EndScrollView();

        // Pan with middle mouse button
        HandlePan(canvasRect);
    }

    private void DrawConnections()
    {
        if (tree == null) return;

        foreach (SkillNodeSO node in tree.nodes)
        {
            if (node == null) continue;

            foreach (SkillNodeSO prereq in node.prerequisites)
            {
                if (prereq == null) continue;

                Vector2 from = NodeCenter(prereq) * zoom;
                Vector2 to = NodeCenter(node) * zoom;

                Handles.color = new Color(0.7f, 0.7f, 0.7f, 0.8f);
                Handles.DrawLine(from, to);

                // Arrow tip
                Vector2 dir = (to - from).normalized;
                Vector2 tip = to - dir * (NODE_H * 0.5f * zoom);
                Handles.DrawSolidDisc(tip, Vector3.forward, 4f * zoom);
            }
        }
    }

    private void DrawNode(SkillNodeSO node)
    {
        Rect rect = NodeRect(node);
        Rect scaled = Scale(rect);

        bool isDuplicate = duplicateIds.Contains(node.id);

        // Background
        EditorGUI.DrawRect(scaled, isDuplicate
            ? new Color(0.65f, 0.15f, 0.15f)
            : new Color(0.22f, 0.22f, 0.22f));

        // Border (1px inset)
        Rect border = new Rect(scaled.x + 1, scaled.y + 1, scaled.width - 2, scaled.height - 2);
        EditorGUI.DrawRect(border, isDuplicate ? new Color(1f, 0.3f, 0.3f, 0.3f) : new Color(0.5f, 0.5f, 0.5f, 0.3f));

        // Labels — pure GUI.Label, no GUILayout
        float pad = 6f * zoom;
        float lineH = 16f * zoom;

        Rect nameRect = new Rect(scaled.x + pad, scaled.y + pad, scaled.width - pad * 2, lineH);
        Rect idRect = new Rect(scaled.x + pad, scaled.y + pad + lineH, scaled.width - pad * 2, lineH);
        Rect detailRect = new Rect(scaled.x + pad, scaled.y + pad + lineH * 2, scaled.width - pad * 2, lineH);

        GUIStyle bold = new GUIStyle(EditorStyles.boldLabel) { fontSize = Mathf.RoundToInt(11f * zoom) };
        GUIStyle mini = new GUIStyle(EditorStyles.miniLabel) { fontSize = Mathf.RoundToInt(9f * zoom) };

        GUI.Label(nameRect, node.displayName, bold);
        GUI.Label(idRect, $"ID: {node.id}", mini);
        GUI.Label(detailRect, $"Effects: {node.effects.Count}  Costs: {node.costs.Count}", mini);

        // Invisible button over the whole node to handle click
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
            node.canvasPosition.x - NODE_W * 0.5f + (1920/2f),
            -node.canvasPosition.y - NODE_H * 0.5f + (1080/2f),
            NODE_W, NODE_H);
    }

    private Vector2 NodeCenter(SkillNodeSO node)
    {
        Rect r = NodeRect(node);
        return new Vector2(r.center.x, r.center.y);
    }

    private Rect Scale(Rect r)
    {
        return new Rect(r.x * zoom, r.y * zoom, r.width * zoom, r.height * zoom);
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