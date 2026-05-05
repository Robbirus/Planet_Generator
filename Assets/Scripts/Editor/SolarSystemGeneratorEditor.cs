#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Custom Inspector for SolarSystemGenerator.
/// Displays:
///   - Probability breakdown for moon-less planets (ring / binary / nothing)
///   - Range previews for planets, moons and comets
///   - Warnings for common misconfigurations
/// </summary>
[CustomEditor(typeof(SolarSystemGenerator))]
public class SolarSystemGeneratorEditor : Editor
{
    // Foldout states
    private bool showPlanetRanges = true;
    private bool showMoonRanges = true;
    private bool showCometRanges = true;
    private bool showProbs = true;

    public override void OnInspectorGUI()
    {
        SolarSystemGenerator gen = (SolarSystemGenerator)target;

        DrawDefaultInspector();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Generator Analysis", EditorStyles.boldLabel);
        EditorGUILayout.Space(4);

        DrawProbabilitySection(gen);
        EditorGUILayout.Space(6);

        DrawRangePreview("Planet Ranges", gen.GetPlanetData(), ref showPlanetRanges);
        DrawRangePreview("Moon Ranges", gen.GetMoonData(), ref showMoonRanges);
        DrawRangePreview("Comet Ranges", gen.GetCometData(), ref showCometRanges);

        DrawWarnings(gen);
    }

    // Probability section

    private void DrawProbabilitySection(SolarSystemGenerator gen)
    {
        showProbs = EditorGUILayout.Foldout(showProbs, "Moon-less Planet Outcomes", true,
                                             EditorStyles.foldoutHeader);
        if (!showProbs) return;

        EditorGUI.indentLevel++;

        var planetData = gen.GetPlanetData();
        float binaryChance = planetData != null ? planetData.binaryChance : 0f;
        float ringChance = gen.GetRingChance();

        // Clamp so the bar makes sense even with bad values
        float totalSpecial = Mathf.Clamp01(binaryChance + ringChance);
        float nothingChance = Mathf.Max(0f, 1f - totalSpecial);

        EditorGUILayout.Space(4);

        // Stacked bar 
        Rect barRect = EditorGUILayout.GetControlRect(false, 24f);

        float binaryWidth = barRect.width * binaryChance;
        float ringWidth = barRect.width * ringChance;
        float nothingWidth = barRect.width * nothingChance;

        // Binary segment
        Rect binaryRect = new Rect(barRect.x, barRect.y, binaryWidth, barRect.height);
        EditorGUI.DrawRect(binaryRect, new Color(0.3f, 0.6f, 1f));

        // Ring segment
        Rect ringRect = new Rect(barRect.x + binaryWidth, barRect.y, ringWidth, barRect.height);
        EditorGUI.DrawRect(ringRect, new Color(1f, 0.7f, 0.2f));

        // Nothing segment
        Rect nothingRect = new Rect(barRect.x + binaryWidth + ringWidth, barRect.y,
                                     nothingWidth, barRect.height);
        EditorGUI.DrawRect(nothingRect, new Color(0.3f, 0.3f, 0.3f));

        // Overflow warning (red overlay)
        if (totalSpecial > 1f)
        {
            Rect overRect = new Rect(barRect.x + barRect.width - 4f, barRect.y, 4f, barRect.height);
            EditorGUI.DrawRect(overRect, Color.red);
        }

        // Labels inside the bar
        GUIStyle centeredWhite = new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white }
        };

        if (binaryWidth > 30f)
            GUI.Label(binaryRect, $"Binary\n{binaryChance * 100f:0.#}%", centeredWhite);
        if (ringWidth > 30f)
            GUI.Label(ringRect, $"Ring\n{ringChance * 100f:0.#}%", centeredWhite);
        if (nothingWidth > 30f)
            GUI.Label(nothingRect, $"None\n{nothingChance * 100f:0.#}%", centeredWhite);

        EditorGUILayout.Space(4);

        // Legend 
        DrawLegendRow(new Color(0.3f, 0.6f, 1f), $"Binary planet  {binaryChance * 100f:0.#}%");
        DrawLegendRow(new Color(1f, 0.7f, 0.2f), $"Ring           {ringChance * 100f:0.#}%");
        DrawLegendRow(new Color(0.3f, 0.3f, 0.3f), $"Neither        {nothingChance * 100f:0.#}%");

        if (totalSpecial > 1f)
            EditorGUILayout.HelpBox(
                $"Binary + Ring chances exceed 100% ({totalSpecial * 100f:0.#}%). " +
                "Reduce one of them - the 'Nothing' outcome will become impossible.",
                MessageType.Error);

        // Comet count preview
        EditorGUILayout.Space(6);
        var cometRange = gen.GetCometCountRange();
        EditorGUILayout.LabelField("Comet count",
            $"{cometRange.x} - {cometRange.y}", EditorStyles.miniLabel);

        EditorGUI.indentLevel--;
    }

    // Range preview 
    private void DrawRangePreview(string title, CelestialObjectDataSO data, ref bool foldout)
    {
        if (data == null) return;

        foldout = EditorGUILayout.Foldout(foldout, title, true, EditorStyles.foldoutHeader);
        if (!foldout) return;

        EditorGUI.indentLevel++;

        DrawMinMaxBar("Count", data.numberRange, 0f, 20f, new Color(0.4f, 0.8f, 0.4f));
        DrawMinMaxBar("Distance", data.distanceRange, 0f, 300f, new Color(0.4f, 0.6f, 1f));
        DrawMinMaxBar("Orbital speed", data.orbitalSpeedRange, 0f, 200f, new Color(1f, 0.8f, 0.2f));
        DrawMinMaxBar("Rotation speed", data.rotationSpeedRange, 0f, 200f, new Color(1f, 0.5f, 0.2f));
        DrawMinMaxBar("Mass", data.massRange, 0f, 100f, new Color(0.8f, 0.4f, 0.8f));
        DrawMinMaxBar("Density", data.densityRange, 0f, 5f, new Color(0.6f, 0.8f, 1f));

        if (data.eccentricityRange != Vector2.zero)
            DrawMinMaxBar("Eccentricity", data.eccentricityRange, 0f, 1f, new Color(1f, 0.4f, 0.4f));

        EditorGUI.indentLevel--;
        EditorGUILayout.Space(2);
    }

    // Warnings

    private void DrawWarnings(SolarSystemGenerator gen)
    {
        EditorGUILayout.Space(6);

        var pd = gen.GetPlanetData();
        var md = gen.GetMoonData();
        var cd = gen.GetCometData();

        if (pd == null)
            EditorGUILayout.HelpBox("Planet Data (SO) is not assigned.", MessageType.Error);

        if (md == null)
            EditorGUILayout.HelpBox("Moon Data is not assigned - no moons will be generated.",
                                     MessageType.Warning);

        if (cd == null)
            EditorGUILayout.HelpBox("Comet Data is not assigned - no comets will be generated.",
                                     MessageType.Info);

        if (pd != null && pd.distanceRange.x <= 0f)
            EditorGUILayout.HelpBox("Planet min distance is 0 - planets may spawn inside the Sun.",
                                     MessageType.Warning);

        if (pd != null && md != null && pd.distanceRange.y < md.distanceRange.y * 2f)
            EditorGUILayout.HelpBox(
                "Max planet distance may be too small relative to max moon distance. " +
                "Planets could run out of space quickly.", MessageType.Warning);

        if (pd != null && pd.binaryChance > 0f && gen.GetCometData() == null)
            EditorGUILayout.HelpBox(
                "Binary chance > 0 but no comet data - that's fine, just informational.",
                MessageType.None);
    }

    // Drawing helpers 

    private void DrawMinMaxBar(string label, Vector2 range, float absMin, float absMax, Color color)
    {
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField(label, GUILayout.Width(110f));

        Rect barRect = EditorGUILayout.GetControlRect(false, 16f);

        // Background
        EditorGUI.DrawRect(barRect, new Color(0.18f, 0.18f, 0.18f));

        float span = Mathf.Max(absMax - absMin, 0.001f);
        float left = Mathf.Clamp01((range.x - absMin) / span);
        float right = Mathf.Clamp01((range.y - absMin) / span);

        Rect fillRect = new Rect(
            barRect.x + barRect.width * left,
            barRect.y,
            barRect.width * (right - left),
            barRect.height);
        EditorGUI.DrawRect(fillRect, color);

        // Value text
        GUIStyle mini = new GUIStyle(EditorStyles.miniLabel)
        { alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.white } };
        GUI.Label(fillRect, $"{range.x:0.#} - {range.y:0.#}", mini);

        EditorGUILayout.EndHorizontal();
    }

    private void DrawLegendRow(Color color, string text)
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(16f);

        Rect colorRect = EditorGUILayout.GetControlRect(false, 14f, GUILayout.Width(14f));
        EditorGUI.DrawRect(colorRect, color);

        EditorGUILayout.LabelField(text, EditorStyles.miniLabel);
        EditorGUILayout.EndHorizontal();
    }
}
#endif