#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Custom Inspector for HealthComponent.
/// Adds:
///    A visual HP bar
///    A durability bar
///    Color-coded armor class badge
///    MainFrame and Team banners
///    Runtime damage test buttons (Play Mode only)
///    Warnings for common misconfigurations
/// </summary>
[CustomEditor(typeof(HealthComponent))]
public class HealthComponentEditor : Editor
{
    // Foldout states
    private bool showStats = true;
    private bool showArmor = true;
    private bool showDebug = false;

    public override void OnInspectorGUI()
    {
        HealthComponent hc = (HealthComponent)target;

        DrawBanners(hc);
        EditorGUILayout.Space(4);

        DrawStats(hc);
        EditorGUILayout.Space(4);

        DrawArmorSection(hc);
        EditorGUILayout.Space(4);

        DrawDestructionSection();
        EditorGUILayout.Space(4);

        DrawDebugSection(hc);
        EditorGUILayout.Space(4);

        DrawWarnings(hc);

        if (Application.isPlaying)
        {
            EditorGUILayout.Space(4);
            DrawRuntimeTools(hc);
        }
    }

    // Banners

    private void DrawBanners(HealthComponent hc)
    {
        // MainFrame banner
        if (hc.IsMainFrame())
        {
            EditorGUILayout.HelpBox("MAIN FRAME  —  Destroying this part kills the enemy.",
                MessageType.Warning);
        }

        // Team badge
        Color teamColor = hc.GetTeam() == Team.Player
            ? new Color(0.2f, 0.6f, 1f)
            : new Color(1f, 0.3f, 0.3f);

        Rect teamRect = EditorGUILayout.GetControlRect(false, 20f);
        EditorGUI.DrawRect(teamRect, teamColor);
        GUIStyle teamStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white }
        };
        GUI.Label(teamRect, $"Team : {hc.GetTeam()}", teamStyle);
    }

    // Stats

    private void DrawStats(HealthComponent hc)
    {
        showStats = EditorGUILayout.Foldout(showStats, "Health", true, EditorStyles.foldoutHeader);
        if (!showStats) return;

        EditorGUI.indentLevel++;

        // HP bar
        float hpRatio = hc.GetMaxHealth() > 0f ? hc.GetCurrentHealth() / hc.GetMaxHealth() : 0f;
        DrawProgressBar(
            label: $"HP  {hc.GetCurrentHealth():0.#} / {hc.GetMaxHealth():0.#}",
            ratio: hpRatio,
            healthy: new Color(0.2f, 0.8f, 0.2f),
            danger: new Color(0.9f, 0.2f, 0.2f));

        // Draw the actual serialized properties
        DrawProperty("team");
        DrawProperty("maxHealth");
        if (Application.isPlaying)
        {
            GUI.enabled = false;
            EditorGUILayout.FloatField("Current Health", hc.GetCurrentHealth());
            GUI.enabled = true;
        }

        EditorGUI.indentLevel--;
    }

    // Armor

    private void DrawArmorSection(HealthComponent hc)
    {
        showArmor = EditorGUILayout.Foldout(showArmor, "Armor", true, EditorStyles.foldoutHeader);
        if (!showArmor) return;

        EditorGUI.indentLevel++;

        // Armor class badge
        Rect badgeRect = EditorGUILayout.GetControlRect(false, 18f);
        Color armorColor = GetArmorColor(hc.GetArmorType());
        EditorGUI.DrawRect(badgeRect, armorColor);
        GUIStyle badgeStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white },
            fontStyle = FontStyle.Bold
        };
        GUI.Label(badgeRect, hc.GetArmorType().ToString(), badgeStyle);

        DrawProperty("armorType");

        // Durability bar
        float durRatio = hc.GetDurability() / 100f;
        DrawProgressBar(
            label: $"Durability  {hc.GetDurability():0.#}%",
            ratio: durRatio,
            healthy: new Color(0.3f, 0.6f, 1f),
            danger: new Color(0.6f, 0.3f, 1f));

        DrawProperty("durability");

        EditorGUI.indentLevel--;
    }

    // Destruction

    private void DrawDestructionSection()
    {
        EditorGUILayout.LabelField("Destruction", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        DrawProperty("isDestructible");
        DrawProperty("isMainFrame");
        EditorGUI.indentLevel--;
    }

    // Debug

    private void DrawDebugSection(HealthComponent hc)
    {
        showDebug = EditorGUILayout.Foldout(showDebug, "Debug", true, EditorStyles.foldoutHeader);
        if (!showDebug) return;

        EditorGUI.indentLevel++;
        DrawProperty("logDamage");

        if (Application.isPlaying)
        {
            GUI.enabled = false;
            EditorGUILayout.Toggle("Is Dead", hc.IsDead());
            GUI.enabled = true;
        }

        EditorGUI.indentLevel--;
    }

    // Warnings

    private void DrawWarnings(HealthComponent hc)
    {
        if (hc.GetMaxHealth() <= 0f)
            EditorGUILayout.HelpBox("Max Health is 0 or negative.", MessageType.Error);

        if (hc.GetDurability() <= 0f)
            EditorGUILayout.HelpBox("Durability is 0 — this part will absorb no damage.", MessageType.Warning);

        if (hc.GetArmorType() == ArmorType.INDESTRUCTIBLE && !hc.IsMainFrame())
            EditorGUILayout.HelpBox("INDESTRUCTIBLE armor on a non-MainFrame part — it can never be destroyed.", MessageType.Info);

        if (hc.IsMainFrame() && !hc.gameObject.GetComponentInParent<EnemyHealth>())
            EditorGUILayout.HelpBox("MainFrame is set but no EnemyHealth found in the parent hierarchy.", MessageType.Warning);
    }

    // Runtime tools

    private float testDamage = 10f;
    private float testHeal = 10f;

    private void DrawRuntimeTools(HealthComponent hc)
    {
        EditorGUILayout.LabelField("Runtime Test Tools", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        testDamage = EditorGUILayout.FloatField("Damage", testDamage, GUILayout.Width(200f));
        if (GUILayout.Button("Apply Damage"))
        {
            hc.TakeDamage(testDamage);
            EditorUtility.SetDirty(hc);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        testHeal = EditorGUILayout.FloatField("Heal", testHeal, GUILayout.Width(200f));
        if (GUILayout.Button("Apply Heal"))
        {
            hc.Heal(testHeal);
            EditorUtility.SetDirty(hc);
        }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Kill (set HP to 0)"))
        {
            hc.TakeDamage(hc.GetCurrentHealth() + 1f);
            EditorUtility.SetDirty(hc);
        }

        if (GUILayout.Button("Full Heal"))
        {
            hc.Heal(hc.GetMaxHealth());
            EditorUtility.SetDirty(hc);
        }

        // Auto-repaint in play mode so bars stay live
        if (Application.isPlaying)
            Repaint();
    }

    // Helpers

    /// <summary>Draws a labelled gradient progress bar.</summary>
    private void DrawProgressBar(string label, float ratio,
                                  Color healthy, Color danger)
    {
        Rect barRect = EditorGUILayout.GetControlRect(false, 18f);

        // Background
        EditorGUI.DrawRect(barRect, new Color(0.15f, 0.15f, 0.15f));

        // Fill
        float clampedRatio = Mathf.Clamp01(ratio);
        Rect fillRect = new Rect(barRect.x, barRect.y,
                                  barRect.width * clampedRatio, barRect.height);
        EditorGUI.DrawRect(fillRect, Color.Lerp(danger, healthy, clampedRatio));

        // Label
        GUIStyle style = new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white }
        };
        GUI.Label(barRect, label, style);
    }

    /// <summary>Draws a single serialized property by name.</summary>
    private void DrawProperty(string propertyName)
    {
        SerializedProperty prop = serializedObject.FindProperty(propertyName);
        if (prop != null)
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(prop, true);
            serializedObject.ApplyModifiedProperties();
        }
    }

    /// <summary>Maps armor tier to a color for the badge.</summary>
    private Color GetArmorColor(ArmorType armor)
    {
        return armor switch
        {
            ArmorType.UNAMORED_I => new Color(0.6f, 0.6f, 0.6f),
            ArmorType.UNAMORED_II => new Color(0.55f, 0.55f, 0.55f),
            ArmorType.LIGHT => new Color(0.2f, 0.7f, 0.3f),
            ArmorType.MEDIUM => new Color(0.2f, 0.5f, 0.8f),
            ArmorType.HEAVY => new Color(0.7f, 0.4f, 0.1f),
            ArmorType.TANK_I => new Color(0.6f, 0.1f, 0.1f),
            ArmorType.TANK_II => new Color(0.65f, 0.08f, 0.08f),
            ArmorType.TANK_III => new Color(0.7f, 0.06f, 0.06f),
            ArmorType.TANK_IV => new Color(0.75f, 0.04f, 0.04f),
            ArmorType.TANK_V => new Color(0.8f, 0.02f, 0.02f),
            ArmorType.TANK_VI => new Color(0.85f, 0f, 0f),
            ArmorType.INDESTRUCTIBLE => new Color(0.15f, 0.15f, 0.15f),
            _ => Color.gray
        };
    }
}
#endif