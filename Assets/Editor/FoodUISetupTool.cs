using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public sealed class FoodUISetupTool : EditorWindow
{
    private const string DefaultFolder = "Assets/Resources/Art/UI/Food";

    private static readonly Dictionary<string, Vector4> SliceBorders = new Dictionary<string, Vector4>(StringComparer.OrdinalIgnoreCase)
    {
        { "food_upgrade_button.png", new Vector4(42f, 26f, 42f, 26f) },
        { "food_category_tab_selected.png", new Vector4(44f, 32f, 44f, 32f) },
        { "food_category_tab_normal.png", new Vector4(44f, 32f, 44f, 32f) }
    };

    private string folderPath = DefaultFolder;
    private Vector2 reportScroll;
    private string report = "Run Validate to inspect Food UI assets without changing them.";

    [MenuItem("Oyun Araclari/Food UI Setup & Validate")]
    public static void ShowWindow()
    {
        GetWindow<FoodUISetupTool>("Food UI Tool");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Food UI Import & Safe Setup", EditorStyles.boldLabel);
        folderPath = EditorGUILayout.TextField("Asset Folder", folderPath);
        EditorGUILayout.HelpBox("Validate is read-only. Apply changes import settings only and never edits scene, hierarchy, GridLayoutGroup, or RectTransform values.", MessageType.Info);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Validate Only")) report = ValidateFolder(folderPath, false);
            if (GUILayout.Button("Report, Confirm & Apply")) RunInteractiveApply();
        }

        reportScroll = EditorGUILayout.BeginScrollView(reportScroll);
        EditorGUILayout.TextArea(report, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();
    }

    public static void ValidateForBatch()
    {
        Debug.Log(ValidateFolder(DefaultFolder, false));
    }

    public static void ApplyForBatch()
    {
        Debug.Log(ValidateFolder(DefaultFolder, true));
    }

    private void RunInteractiveApply()
    {
        string dryRun = ValidateFolder(folderPath, false);
        report = dryRun;
        if (dryRun.Contains("No issues found."))
        {
            EditorUtility.DisplayDialog("Food UI Report", "All Food UI assets already match the safe standards.", "OK");
            return;
        }

        int choice = EditorUtility.DisplayDialogComplex(
            "Food UI Change Report",
            dryRun,
            "Apply Listed Changes",
            "Cancel",
            "Keep Report Only");
        if (choice == 0)
        {
            report = ValidateFolder(folderPath, true);
            EditorUtility.DisplayDialog("Food UI Report", report, "OK");
        }
    }

    private static string ValidateFolder(string folder, bool apply)
    {
        if (!AssetDatabase.IsValidFolder(folder))
        {
            return "[FoodUITool] Folder not found: " + folder;
        }

        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folder });
        List<string> lines = new List<string>();
        int changedCount = 0;

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (!assetPath.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) continue;

            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null) continue;

            List<string> issues = GetIssues(assetPath, importer);
            if (issues.Count == 0) continue;

            lines.Add(Path.GetFileName(assetPath) + ": " + string.Join(", ", issues));
            if (!apply) continue;

            ApplyStandards(assetPath, importer);
            importer.SaveAndReimport();
            changedCount++;
        }

        if (apply)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        string header = apply
            ? "[FoodUITool] Applied safe standards to " + changedCount + " asset(s)."
            : "[FoodUITool] Validation found " + lines.Count + " asset(s) with differences.";
        return lines.Count == 0 ? header + " No issues found." : header + "\n" + string.Join("\n", lines);
    }

    private static List<string> GetIssues(string assetPath, TextureImporter importer)
    {
        List<string> issues = new List<string>();
        if (importer.textureType != TextureImporterType.Sprite) issues.Add("TextureType");
        if (importer.mipmapEnabled) issues.Add("MipMap");
        if (importer.spriteMeshType != SpriteMeshType.FullRect) issues.Add("MeshType");
        if (!importer.alphaIsTransparency) issues.Add("Alpha");
        if (importer.wrapMode != TextureWrapMode.Clamp) issues.Add("WrapMode");

        string fileName = Path.GetFileName(assetPath);
        if (SliceBorders.TryGetValue(fileName, out Vector4 expectedBorder))
        {
            if (importer.spriteImportMode != SpriteImportMode.Single) issues.Add("SpriteModeFor9Slice");
            if (importer.spriteBorder != expectedBorder) issues.Add("9SliceBorder");
        }

        TextureImporterPlatformSettings android = importer.GetPlatformTextureSettings("Android");
        TextureImporterFormat expectedFormat = IsLargeBackground(fileName)
            ? TextureImporterFormat.ASTC_6x6
            : TextureImporterFormat.ASTC_4x4;
        if (!android.overridden || android.format != expectedFormat || android.maxTextureSize != 2048)
        {
            issues.Add("AndroidCompression");
        }

        return issues;
    }

    private static void ApplyStandards(string assetPath, TextureImporter importer)
    {
        string fileName = Path.GetFileName(assetPath);
        importer.textureType = TextureImporterType.Sprite;
        importer.mipmapEnabled = false;
        importer.spriteMeshType = SpriteMeshType.FullRect;
        importer.alphaIsTransparency = true;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.filterMode = FilterMode.Bilinear;
        importer.spritePixelsPerUnit = 100f;
        if (SliceBorders.TryGetValue(fileName, out Vector4 border))
        {
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spriteBorder = border;
        }

        TextureImporterPlatformSettings android = importer.GetPlatformTextureSettings("Android");
        android.name = "Android";
        android.overridden = true;
        android.maxTextureSize = 2048;
        android.resizeAlgorithm = TextureResizeAlgorithm.Mitchell;
        android.format = IsLargeBackground(fileName)
            ? TextureImporterFormat.ASTC_6x6
            : TextureImporterFormat.ASTC_4x4;
        android.textureCompression = TextureImporterCompression.CompressedHQ;
        importer.SetPlatformTextureSettings(android);
    }

    private static bool IsLargeBackground(string fileName)
    {
        return fileName.IndexOf("background", StringComparison.OrdinalIgnoreCase) >= 0 ||
               fileName.IndexOf("panel", StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
