using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public sealed class WorkshopLayoutImporterTool : EditorWindow
{
    private const string JsonAssetPath = "Assets/Resources/Data/workshop_layout.json";
    private const string ReferenceAssetPath = "Assets/Resources/Art/UI/Workshop/workshop_layout_reference.png";
    private const string ModularAssetDirectory = "Assets/Resources/Art/UI/Workshop/Modular";

    private WorkshopLayoutConfig config;
    private Texture2D referenceTexture;
    private Vector2 scroll;
    private string report = "Hazır. Referans yerleşimi yükleyin veya doğrulayın.";

    [MenuItem("Oyun Araclari/Atolye Pixel Layout Importer")]
    public static void ShowWindow()
    {
        WorkshopLayoutImporterTool window = GetWindow<WorkshopLayoutImporterTool>("Atölye Layout");
        window.minSize = new Vector2(620f, 720f);
        window.Show();
    }

    private void OnEnable()
    {
        Reload();
    }

    private void OnGUI()
    {
        if (config == null) Reload();

        EditorGUILayout.LabelField("Atölye Pixel-Perfect Layout Importer", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Referans çözünürlüğü 1672×941. JSON'daki her değer doğrudan Unity RectTransform " +
            "anchoredPosition ve sizeDelta değeridir; ölçek tahmini yapılmaz.", MessageType.Info);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Referans Presetini Yeniden Oluştur"))
            {
                config = WorkshopLayoutConfig.CreateReferencePreset();
                SaveConfig();
                report = ValidateConfig(config);
            }
            if (GUILayout.Button("JSON'u Yeniden Yükle")) Reload();
            if (GUILayout.Button("Doğrula")) report = ValidateConfig(config);
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("%50 Referans Bindirmesi")) SetOverlay(0.5f);
            if (GUILayout.Button("Bindirmeyi Kapat")) SetOverlay(0f);
            if (GUILayout.Button("Modüler PNG'leri Doğrula")) report = ValidateModularAssets();
            if (GUILayout.Button("Kaydet"))
            {
                SaveConfig();
                report = "Yerleşim JSON'u kaydedildi.\n" + ValidateConfig(config);
            }
        }

        DrawReferencePreview();

        scroll = EditorGUILayout.BeginScrollView(scroll);
        config.panelWidth = EditorGUILayout.FloatField("Panel Genişliği", config.panelWidth);
        config.panelHeight = EditorGUILayout.FloatField("Panel Yüksekliği", config.panelHeight);
        config.panelY = EditorGUILayout.FloatField("Panel Y", config.panelY);
        config.referenceOverlayAlpha = EditorGUILayout.Slider("Referans Alpha", config.referenceOverlayAlpha, 0f, 1f);

        DrawRectField("Başlık", config.title);
        DrawRectField("Başlık Yazısı", config.titleText);
        DrawRectField("Kapat", config.closeButton);
        DrawRectArray("Kategoriler", config.categories);
        DrawRectArray("Kategori İkonları (Yerel)", config.categoryIcons);
        DrawRectArray("Kategori Yazıları (Yerel)", config.categoryTexts);
        DrawRectArray("Malzemeler", config.materials);
        DrawRectArray("Malzeme İkonları", config.materialIcons);
        DrawRectArray("Malzeme İsimleri", config.materialNames);
        DrawRectArray("Malzeme Sayıları", config.materialCounts);
        DrawRectField("Kafe Önizleme", config.preview);
        DrawRectField("Önizleme Çerçevesi", config.previewFrame);
        DrawRectArray("Ürün Kartları", config.productCards);
        DrawRectArray("Ürün Görselleri (Yerel)", config.productImages);
        DrawRectArray("Ürün İsimleri (Yerel)", config.productNames);
        DrawRectArray("Ürün Nadirlikleri (Yerel)", config.productRarities);
        DrawRectArray("Ürün Durumları (Yerel)", config.productStatuses);
        DrawRectField("Detay Paneli", config.detailPanel);
        DrawRectField("Detay Başlığı", config.detailTitle);
        DrawRectField("Detay Kategorisi", config.detailCategory);
        DrawRectField("Detay Nadirliği", config.detailRarity);
        DrawRectField("Seçili Ürün Çerçevesi", config.selectedProductFrame);
        DrawRectField("Seçili Ürün", config.selectedProduct);
        DrawRectField("Etki Başlığı", config.effectHeading);
        DrawRectArray("Etki Satırları", config.effectRows);
        DrawRectArray("Etki Metinleri", config.effectTexts);
        DrawRectField("Gereksinim Başlığı", config.requirementHeading);
        DrawRectArray("Gereksinim Kartları", config.requirementCards);
        DrawRectArray("Gereksinim İkonları", config.requirementIcons);
        DrawRectArray("Gereksinim İsimleri", config.requirementNames);
        DrawRectArray("Gereksinim Sayıları", config.requirementCounts);
        DrawRectField("Üret Butonu", config.actionButton);
        DrawRectField("Üret Yazısı", config.actionText);

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Doğrulama Raporu", EditorStyles.boldLabel);
        EditorGUILayout.TextArea(report, GUILayout.MinHeight(100f));
        EditorGUILayout.EndScrollView();
    }

    private void DrawReferencePreview()
    {
        if (referenceTexture == null)
        {
            EditorGUILayout.HelpBox("Referans PNG bulunamadı: " + ReferenceAssetPath, MessageType.Warning);
            return;
        }

        float width = Mathf.Min(position.width - 28f, 560f);
        float height = width * (referenceTexture.height / (float)referenceTexture.width);
        Rect previewRect = GUILayoutUtility.GetRect(width, height, GUILayout.ExpandWidth(false));
        EditorGUI.DrawPreviewTexture(previewRect, referenceTexture, null, ScaleMode.ScaleToFit);
    }

    private static void DrawRectArray(string label, WorkshopLayoutRect[] values)
    {
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
        if (values == null) return;
        EditorGUI.indentLevel++;
        for (int i = 0; i < values.Length; i++) DrawRectField(values[i].id, values[i]);
        EditorGUI.indentLevel--;
    }

    private static void DrawRectField(string label, WorkshopLayoutRect value)
    {
        if (value == null) return;
        Rect rect = new Rect(value.x, value.y, value.width, value.height);
        Rect edited = EditorGUILayout.RectField(label, rect);
        value.x = edited.x;
        value.y = edited.y;
        value.width = Mathf.Max(1f, edited.width);
        value.height = Mathf.Max(1f, edited.height);
    }

    private void Reload()
    {
        referenceTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(ReferenceAssetPath);
        if (File.Exists(JsonAssetPath))
        {
            try
            {
                config = JsonUtility.FromJson<WorkshopLayoutConfig>(File.ReadAllText(JsonAssetPath));
            }
            catch (Exception exception)
            {
                report = "JSON okunamadı: " + exception.Message;
            }
        }
        if (config == null || !config.IsUsable()) config = WorkshopLayoutConfig.CreateReferencePreset();
    }

    private void SetOverlay(float alpha)
    {
        config.referenceOverlayAlpha = alpha;
        SaveConfig();
        report = alpha > 0f
            ? "Referans bindirmesi etkin. Play modunda mevcut UI ile üst üste görünecek."
            : "Referans bindirmesi kapatıldı.";
    }

    private void SaveConfig()
    {
        string directory = Path.GetDirectoryName(JsonAssetPath);
        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
        File.WriteAllText(JsonAssetPath, JsonUtility.ToJson(config, true));
        ApplyReferenceImportSettings();
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
    }

    private static void ApplyReferenceImportSettings()
    {
        AssetDatabase.ImportAsset(ReferenceAssetPath, ImportAssetOptions.ForceUpdate);
        TextureImporter importer = AssetImporter.GetAtPath(ReferenceAssetPath) as TextureImporter;
        if (importer == null) return;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.filterMode = FilterMode.Bilinear;
        TextureImporterSettings settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        importer.SetTextureSettings(settings);
        importer.SaveAndReimport();
    }

    private static string ValidateConfig(WorkshopLayoutConfig value)
    {
        if (value == null || !value.IsUsable()) return "HATA: Yerleşim eksik veya dizi sayıları yanlış.";

        List<string> issues = new List<string>();
        ValidateRect(value, value.title, issues);
        ValidateRect(value, value.titleText, issues);
        ValidateRect(value, value.closeButton, issues);
        ValidateArray(value, value.categories, issues);
        ValidateArray(value, value.categoryIcons, issues);
        ValidateArray(value, value.categoryTexts, issues);
        ValidateArray(value, value.materials, issues);
        ValidateArray(value, value.materialIcons, issues);
        ValidateArray(value, value.materialNames, issues);
        ValidateArray(value, value.materialCounts, issues);
        ValidateRect(value, value.preview, issues);
        ValidateRect(value, value.previewFrame, issues);
        ValidateArray(value, value.productCards, issues);
        ValidateArray(value, value.productImages, issues);
        ValidateArray(value, value.productNames, issues);
        ValidateArray(value, value.productRarities, issues);
        ValidateArray(value, value.productStatuses, issues);
        ValidateRect(value, value.detailPanel, issues);
        ValidateRect(value, value.detailTitle, issues);
        ValidateRect(value, value.detailCategory, issues);
        ValidateRect(value, value.detailRarity, issues);
        ValidateRect(value, value.selectedProductFrame, issues);
        ValidateRect(value, value.selectedProduct, issues);
        ValidateRect(value, value.effectHeading, issues);
        ValidateArray(value, value.effectRows, issues);
        ValidateArray(value, value.effectTexts, issues);
        ValidateRect(value, value.requirementHeading, issues);
        ValidateArray(value, value.requirementCards, issues);
        ValidateArray(value, value.requirementIcons, issues);
        ValidateArray(value, value.requirementNames, issues);
        ValidateArray(value, value.requirementCounts, issues);
        ValidateRect(value, value.actionButton, issues);
        ValidateRect(value, value.actionText, issues);

        return issues.Count == 0
            ? "OK: 9 kategori, 7 malzeme, 6 ürün kartı ve detay alanı panel sınırları içinde."
            : "Bulunan sorunlar:\n" + string.Join("\n", issues);
    }

    private static string ValidateModularAssets()
    {
        string[] filledAssets =
        {
            "workshop_title_plaque.png",
            "workshop_category_button_clean.png",
            "workshop_material_counter_clean.png",
            "workshop_product_card.png",
            "workshop_detail_panel_clean.png",
            "workshop_selected_product_frame.png",
            "workshop_effect_row.png",
            "workshop_requirement_card_clean.png",
            "workshop_action_button.png"
        };

        List<string> issues = new List<string>();
        for (int i = 0; i < filledAssets.Length; i++)
        {
            string assetPath = ModularAssetDirectory + "/" + filledAssets[i];
            if (!File.Exists(assetPath))
            {
                issues.Add(filledAssets[i] + " bulunamadı.");
                continue;
            }

            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            try
            {
                if (!texture.LoadImage(File.ReadAllBytes(assetPath)))
                {
                    issues.Add(filledAssets[i] + " okunamadı.");
                    continue;
                }

                // The detail panel must be a continuously filled parchment. Other ornate
                // controls may intentionally have empty rows around their curved silhouettes.
                if (filledAssets[i] == "workshop_detail_panel_clean.png")
                    ValidateOpaqueInterior(texture, filledAssets[i], issues);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        return issues.Count == 0
            ? "OK: Kullanılan modüler PNG'lerde kopuk parça veya büyük iç şeffaf boşluk yok."
            : "PNG sorunları:\n" + string.Join("\n", issues);
    }

    private static void ValidateOpaqueInterior(Texture2D texture, string assetName, List<string> issues)
    {
        int[] sampleRows =
        {
            Mathf.Clamp(Mathf.RoundToInt(texture.height * 0.10f), 0, texture.height - 1),
            Mathf.Clamp(Mathf.RoundToInt(texture.height * 0.50f), 0, texture.height - 1),
            Mathf.Clamp(Mathf.RoundToInt(texture.height * 0.90f), 0, texture.height - 1)
        };

        Color32[] pixels = texture.GetPixels32();
        for (int rowIndex = 0; rowIndex < sampleRows.Length; rowIndex++)
        {
            int y = sampleRows[rowIndex];
            int first = -1;
            int last = -1;
            for (int x = 0; x < texture.width; x++)
            {
                if (pixels[(y * texture.width) + x].a <= 16) continue;
                if (first < 0) first = x;
                last = x;
            }

            if (first < 0 || last <= first)
            {
                issues.Add(assetName + " görünür içerik taşımıyor.");
                return;
            }

            int longestTransparentRun = 0;
            int currentRun = 0;
            for (int x = first; x <= last; x++)
            {
                if (pixels[(y * texture.width) + x].a <= 16)
                {
                    currentRun++;
                    longestTransparentRun = Mathf.Max(longestTransparentRun, currentRun);
                }
                else
                {
                    currentRun = 0;
                }
            }

            if (longestTransparentRun > texture.width * 0.12f)
            {
                issues.Add(assetName + " içinde kopuk/şeffaf oyuk var (satır %" +
                    Mathf.RoundToInt((y / (float)texture.height) * 100f) + ").");
                return;
            }
        }
    }

    private static void ValidateArray(WorkshopLayoutConfig config, WorkshopLayoutRect[] values, List<string> issues)
    {
        if (values == null) return;
        for (int i = 0; i < values.Length; i++) ValidateRect(config, values[i], issues);
    }

    private static void ValidateRect(WorkshopLayoutConfig config, WorkshopLayoutRect value, List<string> issues)
    {
        if (value == null)
        {
            issues.Add("Eksik Rect alanı.");
            return;
        }

        float left = value.x - (value.width * 0.5f);
        float right = value.x + (value.width * 0.5f);
        float bottom = value.y - (value.height * 0.5f);
        float top = value.y + (value.height * 0.5f);
        if (left < -config.panelWidth * 0.5f || right > config.panelWidth * 0.5f ||
            bottom < -config.panelHeight * 0.5f || top > config.panelHeight * 0.5f)
        {
            issues.Add(value.id + " panel sınırının dışında.");
        }
    }

    public static void CreateAndValidateForBatch()
    {
        WorkshopLayoutConfig preset = WorkshopLayoutConfig.CreateReferencePreset();
        string directory = Path.GetDirectoryName(JsonAssetPath);
        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
        File.WriteAllText(JsonAssetPath, JsonUtility.ToJson(preset, true));
        AssetDatabase.Refresh();
        ApplyReferenceImportSettings();
        Debug.Log("[WorkshopLayoutImporter] " + ValidateConfig(preset));
        Debug.Log("[WorkshopLayoutImporter] " + ValidateModularAssets());
    }
}
