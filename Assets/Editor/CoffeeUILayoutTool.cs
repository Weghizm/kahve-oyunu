using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

public sealed class CoffeeUILayoutTool : EditorWindow
{
    private const string DefaultSpecPath = "Assets/Resources/Data/coffee_ui_layout_daily_login_example.json";
    private const string DefaultPythonToolPath = "Tools/CoffeeUILayoutTool/coffee_ui_layout_tool.py";
    private const string DefaultOutputFolder = "Assets/Resources/Art/UI/CoffeeUILayoutTool/DailyLogin";
    private const string DefaultPythonPath = "C:/Users/akkus/.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe";

    private string specPath = DefaultSpecPath;
    private string pythonToolPath = DefaultPythonToolPath;
    private string pythonPath = DefaultPythonPath;
    private CoffeeUILayoutSpec spec;
    private Texture2D referenceTexture;
    private Texture2D outputPreviewTexture;
    private Vector2 scroll;
    private string report = "Hazır. Önce starter spec oluşturabilir, sonra PNG text + önizleme üretebilirsin.";

    [MenuItem("Oyun Araclari/Coffee UI Layout Tool")]
    public static void ShowWindow()
    {
        CoffeeUILayoutTool window = GetWindow<CoffeeUILayoutTool>("Coffee UI Layout");
        window.minSize = new Vector2(720f, 760f);
        window.Show();
    }

    private void OnEnable()
    {
        ReloadSpec();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Coffee UI Layout Tool", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Bu araç panel tasarımını katmanlara ayırır: ana PNG, slotlar, sabit PNG yazılar ve dinamik TMP alanları. " +
            "Amaç elle piksel düzeltmeyi azaltmak ve yeni panelleri aynı sistemle üretmek.", MessageType.Info);

        specPath = EditorGUILayout.TextField("Spec JSON", specPath);
        pythonToolPath = EditorGUILayout.TextField("Python Tool", pythonToolPath);
        pythonPath = EditorGUILayout.TextField("Python", pythonPath);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Starter Daily Spec Oluştur")) CreateStarterDailySpec();
            if (GUILayout.Button("Spec Yenile")) ReloadSpec();
            if (GUILayout.Button("Doğrula")) report = ValidateSpec(spec);
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("PNG Text + Önizleme Üret")) RunPythonGenerator();
            if (GUILayout.Button("Çıktıları UI Sprite Yap")) report = ApplyOutputImportSettings();
            if (GUILayout.Button("Önizlemeyi Yenile")) LoadPreviewTexture();
        }

        DrawPreview(referenceTexture, "Referans / Ana Panel");
        DrawPreview(outputPreviewTexture, "Son Üretilen Önizleme");

        scroll = EditorGUILayout.BeginScrollView(scroll);
        if (spec != null)
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Panel", EditorStyles.boldLabel);
            spec.id = EditorGUILayout.TextField("Id", spec.id);
            spec.panelName = EditorGUILayout.TextField("Panel Adı", spec.panelName);
            spec.canvasWidth = EditorGUILayout.IntField("Canvas Width", spec.canvasWidth);
            spec.canvasHeight = EditorGUILayout.IntField("Canvas Height", spec.canvasHeight);
            spec.referenceImagePath = EditorGUILayout.TextField("Ana PNG", spec.referenceImagePath);
            spec.outputFolder = EditorGUILayout.TextField("Çıktı Klasörü", spec.outputFolder);
            spec.defaultFontPath = EditorGUILayout.TextField("Varsayılan Font", spec.defaultFontPath);
            spec.drawSlotOverlay = EditorGUILayout.Toggle("Önizlemede Slotları Göster", spec.drawSlotOverlay);

            EditorGUILayout.Space(8f);
            DrawTextSprites(spec.textSprites);
            EditorGUILayout.Space(8f);
            DrawSlots(spec.slots);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Spec'i Kaydet"))
                {
                    SaveSpec();
                    report = "Spec kaydedildi.\n" + ValidateSpec(spec);
                }
            }
        }

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Rapor", EditorStyles.boldLabel);
        EditorGUILayout.TextArea(report, GUILayout.MinHeight(120f));
        EditorGUILayout.EndScrollView();
    }

    private void DrawPreview(Texture2D texture, string title)
    {
        if (texture == null) return;
        EditorGUILayout.Space(6f);
        EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
        float width = Mathf.Min(position.width - 32f, 650f);
        float height = width * texture.height / Mathf.Max(1f, texture.width);
        Rect rect = GUILayoutUtility.GetRect(width, height, GUILayout.ExpandWidth(false));
        EditorGUI.DrawPreviewTexture(rect, texture, null, ScaleMode.ScaleToFit);
    }

    private static void DrawTextSprites(CoffeeUITextSprite[] values)
    {
        EditorGUILayout.LabelField("PNG Yazılar", EditorStyles.boldLabel);
        if (values == null) return;
        EditorGUI.indentLevel++;
        for (int i = 0; i < values.Length; i++)
        {
            CoffeeUITextSprite value = values[i];
            EditorGUILayout.LabelField(value.id, EditorStyles.miniBoldLabel);
            EditorGUI.indentLevel++;
            value.text = EditorGUILayout.TextField("Text", value.text);
            value.fileName = EditorGUILayout.TextField("Dosya", value.fileName);
            value.fontPath = EditorGUILayout.TextField("Font Path", value.fontPath);
            value.width = EditorGUILayout.IntField("Width", value.width);
            value.height = EditorGUILayout.IntField("Height", value.height);
            value.fontSize = EditorGUILayout.IntField("Font Size", value.fontSize);
            value.mode = EditorGUILayout.TextField("Mode", value.mode);
            value.arcHeight = EditorGUILayout.FloatField("Arc Height", value.arcHeight);
            value.fill = EditorGUILayout.TextField("Fill", value.fill);
            value.stroke = EditorGUILayout.TextField("Stroke", value.stroke);
            value.strokeWidth = EditorGUILayout.IntField("Stroke Width", value.strokeWidth);
            value.shadow = EditorGUILayout.Toggle("Shadow", value.shadow);
            EditorGUI.indentLevel--;
        }
        EditorGUI.indentLevel--;
    }

    private static void DrawSlots(CoffeeUILayoutSlot[] values)
    {
        EditorGUILayout.LabelField("Slotlar / RectTransform Alanları", EditorStyles.boldLabel);
        if (values == null) return;
        EditorGUI.indentLevel++;
        for (int i = 0; i < values.Length; i++)
        {
            CoffeeUILayoutSlot value = values[i];
            EditorGUILayout.LabelField(value.id, EditorStyles.miniBoldLabel);
            EditorGUI.indentLevel++;
            value.role = EditorGUILayout.TextField("Role", value.role);
            value.textSpriteId = EditorGUILayout.TextField("Text Sprite", value.textSpriteId);
            Rect rect = new Rect(value.x, value.y, Mathf.Max(1f, value.width), Mathf.Max(1f, value.height));
            Rect edited = EditorGUILayout.RectField("Rect", rect);
            value.x = edited.x;
            value.y = edited.y;
            value.width = Mathf.Max(1f, edited.width);
            value.height = Mathf.Max(1f, edited.height);
            value.rotation = EditorGUILayout.FloatField("Rotation", value.rotation);
            EditorGUI.indentLevel--;
        }
        EditorGUI.indentLevel--;
    }

    private void ReloadSpec()
    {
        string fullPath = ProjectPath(specPath);
        if (!File.Exists(fullPath))
        {
            spec = null;
            referenceTexture = null;
            outputPreviewTexture = null;
            report = "Spec bulunamadı. Starter Daily Spec oluşturabilirsin: " + specPath;
            return;
        }

        try
        {
            spec = JsonUtility.FromJson<CoffeeUILayoutSpec>(File.ReadAllText(fullPath));
            report = ValidateSpec(spec);
            LoadReferenceTexture();
            LoadPreviewTexture();
        }
        catch (Exception exception)
        {
            spec = null;
            report = "Spec okunamadı: " + exception.Message;
        }
    }

    private void LoadReferenceTexture()
    {
        referenceTexture = null;
        if (spec == null || string.IsNullOrEmpty(spec.referenceImagePath)) return;
        referenceTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(spec.referenceImagePath);
    }

    private void LoadPreviewTexture()
    {
        outputPreviewTexture = null;
        if (spec == null || string.IsNullOrEmpty(spec.outputFolder)) return;
        string previewPath = spec.outputFolder.TrimEnd('/', '\\') + "/" + spec.previewFileName;
        outputPreviewTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(previewPath);
    }

    private void SaveSpec()
    {
        string fullPath = ProjectPath(specPath);
        string directory = Path.GetDirectoryName(fullPath);
        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
        File.WriteAllText(fullPath, JsonUtility.ToJson(spec, true));
        AssetDatabase.Refresh();
    }

    private void CreateStarterDailySpec()
    {
        spec = CoffeeUILayoutSpec.CreateDailyLoginStarter();
        SaveSpec();
        LoadReferenceTexture();
        report = "Starter günlük giriş spec'i oluşturuldu.\n" + ValidateSpec(spec);
    }

    private void RunPythonGenerator()
    {
        if (spec != null) SaveSpec();

        string fullPython = ProjectPath(pythonPath);
        if (!File.Exists(fullPython)) fullPython = pythonPath;
        if (!File.Exists(fullPython))
        {
            report = "Python bulunamadı: " + pythonPath;
            return;
        }

        string fullTool = ProjectPath(pythonToolPath);
        if (!File.Exists(fullTool))
        {
            report = "Python tool bulunamadı: " + pythonToolPath;
            return;
        }

        string fullSpec = ProjectPath(specPath);
        ProcessStartInfo info = new ProcessStartInfo
        {
            FileName = fullPython,
            Arguments = "\"" + fullTool + "\" --spec \"" + fullSpec + "\" --project-root \"" + ProjectRoot + "\"",
            WorkingDirectory = ProjectRoot,
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        try
        {
            using (Process process = Process.Start(info))
            {
                string stdout = process.StandardOutput.ReadToEnd();
                string stderr = process.StandardError.ReadToEnd();
                process.WaitForExit();
                AssetDatabase.Refresh();
                ApplyOutputImportSettings();
                LoadPreviewTexture();
                report = process.ExitCode == 0
                    ? "PNG text + önizleme üretildi.\n" + stdout
                    : "Python tool hata verdi.\n" + stdout + "\n" + stderr;
            }
        }
        catch (Exception exception)
        {
            report = "Python tool çalıştırılamadı: " + exception.Message;
        }
    }

    private string ApplyOutputImportSettings()
    {
        if (spec == null || string.IsNullOrEmpty(spec.outputFolder)) return "Output folder eksik.";
        if (!AssetDatabase.IsValidFolder(spec.outputFolder)) return "Output folder Unity asset klasörü değil: " + spec.outputFolder;

        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { spec.outputFolder });
        int changed = 0;
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (!assetPath.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) continue;
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null) continue;
            ApplySpriteImportSettings(importer, assetPath);
            importer.SaveAndReimport();
            changed++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return "UI sprite import ayarları uygulandı: " + changed + " PNG.";
    }

    private static void ApplySpriteImportSettings(TextureImporter importer, string assetPath)
    {
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.filterMode = FilterMode.Bilinear;
        importer.spritePixelsPerUnit = 100f;

        TextureImporterSettings settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        importer.SetTextureSettings(settings);

        TextureImporterPlatformSettings android = importer.GetPlatformTextureSettings("Android");
        android.name = "Android";
        android.overridden = true;
        android.maxTextureSize = assetPath.IndexOf("preview", StringComparison.OrdinalIgnoreCase) >= 0 ? 2048 : 1024;
        android.resizeAlgorithm = TextureResizeAlgorithm.Mitchell;
        android.format = TextureImporterFormat.ASTC_4x4;
        android.textureCompression = TextureImporterCompression.CompressedHQ;
        importer.SetPlatformTextureSettings(android);
    }

    private static string ValidateSpec(CoffeeUILayoutSpec value)
    {
        if (value == null) return "Spec yok.";
        List<string> issues = new List<string>();
        if (value.canvasWidth <= 0 || value.canvasHeight <= 0) issues.Add("Canvas boyutu eksik.");
        if (string.IsNullOrEmpty(value.referenceImagePath)) issues.Add("Ana PNG yolu eksik.");
        if (string.IsNullOrEmpty(value.outputFolder)) issues.Add("Çıktı klasörü eksik.");

        if (value.slots != null)
        {
            for (int i = 0; i < value.slots.Length; i++)
            {
                CoffeeUILayoutSlot slot = value.slots[i];
                if (slot == null)
                {
                    issues.Add("Boş slot: " + i);
                    continue;
                }
                if (slot.width <= 0f || slot.height <= 0f) issues.Add(slot.id + " boyutu geçersiz.");
                float left = slot.x - slot.width * 0.5f;
                float right = slot.x + slot.width * 0.5f;
                float bottom = slot.y - slot.height * 0.5f;
                float top = slot.y + slot.height * 0.5f;
                if (left < -value.canvasWidth * 0.5f || right > value.canvasWidth * 0.5f ||
                    bottom < -value.canvasHeight * 0.5f || top > value.canvasHeight * 0.5f)
                {
                    issues.Add(slot.id + " panel sınırı dışında.");
                }
            }
        }

        return issues.Count == 0
            ? "OK: Spec geçerli. PNG text üretimine hazır."
            : "Sorunlar:\n" + string.Join("\n", issues);
    }

    private static string ProjectRoot
    {
        get { return Directory.GetParent(Application.dataPath).FullName.Replace("\\", "/"); }
    }

    private static string ProjectPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return ProjectRoot;
        if (Path.IsPathRooted(path)) return path.Replace("\\", "/");
        return Path.Combine(ProjectRoot, path).Replace("\\", "/");
    }
}

[Serializable]
public sealed class CoffeeUILayoutSpec
{
    public string id;
    public string panelName;
    public int canvasWidth;
    public int canvasHeight;
    public string referenceImagePath;
    public string outputFolder;
    public string previewFileName;
    public string defaultFontPath;
    public bool drawSlotOverlay;
    public CoffeeUITextSprite[] textSprites;
    public CoffeeUILayoutSlot[] slots;

    public static CoffeeUILayoutSpec CreateDailyLoginStarter()
    {
        return new CoffeeUILayoutSpec
        {
            id = "daily_login_professional_png_text",
            panelName = "Günlük Giriş - PNG Text Starter",
            canvasWidth = 1728,
            canvasHeight = 960,
            referenceImagePath = "Assets/Resources/Art/UI/DailyReward/Professional/daily_login_professional_panel.png",
            outputFolder = DefaultOutputFolder,
            previewFileName = "coffee_ui_layout_preview.png",
            defaultFontPath = "Assets/Resources/Fonts/PaytoneOne-Regular.ttf",
            drawSlotOverlay = true,
            textSprites = CreateDailyTextSprites(),
            slots = CreateDailySlots()
        };
    }

    private const string DefaultOutputFolder = "Assets/Resources/Art/UI/CoffeeUILayoutTool/DailyLogin";

    private static CoffeeUITextSprite[] CreateDailyTextSprites()
    {
        return new[]
        {
            new CoffeeUITextSprite("title", "GÜNLÜK GİRİŞ", "daily_text_title.png", 520, 86, 50, "#FFE99AFF", "#4A2309FF", 2, true, "straight", 0f),
            new CoffeeUITextSprite("today", "BUGÜN", "daily_text_today.png", 180, 50, 28, "#FFF0B6FF", "#4F260CFF", 2, true, "straight", 0f),
            new CoffeeUITextSprite("today_reward", "1 ELMAS\n1 SANDIK", "daily_text_today_reward.png", 200, 64, 24, "#FFE39AFF", "#4C280EFF", 2, true, "straight", 0f),
            new CoffeeUITextSprite("claim", "ÖDÜLÜ AL", "daily_text_claim.png", 350, 82, 45, "#FFE78EFF", "#4A280CFF", 2, true, "straight", 0f),
            new CoffeeUITextSprite("claimed", "ALINDI", "daily_text_status_claimed.png", 116, 36, 20, "#DDFECCFF", "#185729FF", 1, true, "straight", 0f),
            new CoffeeUITextSprite("available", "BUGÜN", "daily_text_status_available.png", 116, 36, 20, "#FFECAAFF", "#4F2A0DFF", 1, true, "straight", 0f),
            new CoffeeUITextSprite("locked", "KİLİT", "daily_text_status_locked.png", 116, 36, 20, "#FFD990FF", "#4F2A0DFF", 1, true, "straight", 0f),
            new CoffeeUITextSprite("locked_long", "KİLİTLİ", "daily_text_status_locked_long.png", 150, 40, 22, "#FFD990FF", "#4F2A0DFF", 1, true, "straight", 0f),
            new CoffeeUITextSprite("arc_sample", "BUGÜN", "daily_text_arc_sample.png", 150, 44, 24, "#FFF0B6FF", "#4F260CFF", 2, true, "arc", 10f)
        };
    }

    private static CoffeeUILayoutSlot[] CreateDailySlots()
    {
        return new[]
        {
            new CoffeeUILayoutSlot("title_text", "text_png", "title", 16f, 382f, 520f, 86f),
            new CoffeeUILayoutSlot("today_text", "text_png", "today", -646f, 270f, 180f, 50f),
            new CoffeeUILayoutSlot("today_reward_text", "text_png", "today_reward", -642f, -300f, 200f, 64f),
            new CoffeeUILayoutSlot("claim_text", "text_png", "claim", 504f, -344f, 350f, 82f),
            new CoffeeUILayoutSlot("day1_status", "text_png", "claimed", -346f, -116f, 116f, 36f),
            new CoffeeUILayoutSlot("day2_status", "text_png", "available", -196f, -116f, 116f, 36f),
            new CoffeeUILayoutSlot("day3_status", "text_png", "locked", -46f, -116f, 116f, 36f),
            new CoffeeUILayoutSlot("day4_status", "text_png", "locked", 104f, -116f, 116f, 36f),
            new CoffeeUILayoutSlot("day5_status", "text_png", "locked", 254f, -116f, 116f, 36f),
            new CoffeeUILayoutSlot("day6_status", "text_png", "locked", 404f, -116f, 116f, 36f),
            new CoffeeUILayoutSlot("day7_status", "text_png", "locked_long", 632f, -144f, 150f, 40f),
            new CoffeeUILayoutSlot("close_button", "button", "", 744f, 350f, 112f, 112f),
            new CoffeeUILayoutSlot("claim_button", "button", "", 504f, -344f, 390f, 118f)
        };
    }
}

[Serializable]
public sealed class CoffeeUITextSprite
{
    public string id;
    public string text;
    public string fileName;
    public string fontPath;
    public int width;
    public int height;
    public int fontSize;
    public string fill;
    public string stroke;
    public int strokeWidth;
    public bool shadow;
    public string mode;
    public float arcHeight;

    public CoffeeUITextSprite(string id, string text, string fileName, int width, int height, int fontSize, string fill, string stroke, int strokeWidth, bool shadow, string mode, float arcHeight)
    {
        this.id = id;
        this.text = text;
        this.fileName = fileName;
        fontPath = string.Empty;
        this.width = width;
        this.height = height;
        this.fontSize = fontSize;
        this.fill = fill;
        this.stroke = stroke;
        this.strokeWidth = strokeWidth;
        this.shadow = shadow;
        this.mode = mode;
        this.arcHeight = arcHeight;
    }
}

[Serializable]
public sealed class CoffeeUILayoutSlot
{
    public string id;
    public string role;
    public string textSpriteId;
    public float x;
    public float y;
    public float width;
    public float height;
    public float rotation;

    public CoffeeUILayoutSlot(string id, string role, string textSpriteId, float x, float y, float width, float height)
    {
        this.id = id;
        this.role = role;
        this.textSpriteId = textSpriteId;
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = height;
        rotation = 0f;
    }
}
