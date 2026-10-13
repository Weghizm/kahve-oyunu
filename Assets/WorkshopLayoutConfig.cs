using System;
using UnityEngine;

[Serializable]
public sealed class WorkshopLayoutRect
{
    public string id;
    public float x;
    public float y;
    public float width;
    public float height;

    public Vector2 Position => new Vector2(x, y);
    public Vector2 Size => new Vector2(width, height);

    public WorkshopLayoutRect() { }

    public WorkshopLayoutRect(string id, float x, float y, float width, float height)
    {
        this.id = id;
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = height;
    }
}

[Serializable]
public sealed class WorkshopLayoutConfig
{
    public const string ResourcePath = "Data/workshop_layout";
    public const float ReferenceWidth = 1672f;
    public const float ReferenceHeight = 941f;

    public float panelWidth = ReferenceWidth;
    public float panelHeight = ReferenceHeight;
    public float panelY = 14f;
    public float referenceOverlayAlpha;

    public WorkshopLayoutRect title;
    public WorkshopLayoutRect titleText;
    public WorkshopLayoutRect closeButton;
    public WorkshopLayoutRect[] categories;
    public WorkshopLayoutRect[] categoryIcons;
    public WorkshopLayoutRect[] categoryTexts;
    public WorkshopLayoutRect[] materials;
    public WorkshopLayoutRect[] materialIcons;
    public WorkshopLayoutRect[] materialNames;
    public WorkshopLayoutRect[] materialCounts;
    public WorkshopLayoutRect preview;
    public WorkshopLayoutRect previewFrame;
    public WorkshopLayoutRect[] productCards;
    public WorkshopLayoutRect[] productImages;
    public WorkshopLayoutRect[] productNames;
    public WorkshopLayoutRect[] productRarities;
    public WorkshopLayoutRect[] productStatuses;
    public WorkshopLayoutRect detailPanel;
    public WorkshopLayoutRect detailTitle;
    public WorkshopLayoutRect detailCategory;
    public WorkshopLayoutRect detailRarity;
    public WorkshopLayoutRect selectedProductFrame;
    public WorkshopLayoutRect selectedProduct;
    public WorkshopLayoutRect effectHeading;
    public WorkshopLayoutRect[] effectRows;
    public WorkshopLayoutRect[] effectTexts;
    public WorkshopLayoutRect requirementHeading;
    public WorkshopLayoutRect[] requirementCards;
    public WorkshopLayoutRect[] requirementIcons;
    public WorkshopLayoutRect[] requirementNames;
    public WorkshopLayoutRect[] requirementCounts;
    public WorkshopLayoutRect actionButton;
    public WorkshopLayoutRect actionText;

    public static WorkshopLayoutConfig Load()
    {
        TextAsset json = Resources.Load<TextAsset>(ResourcePath);
        if (json == null || string.IsNullOrWhiteSpace(json.text)) return CreateReferencePreset();

        try
        {
            WorkshopLayoutConfig result = JsonUtility.FromJson<WorkshopLayoutConfig>(json.text);
            return result != null && result.IsUsable() ? result : CreateReferencePreset();
        }
        catch (Exception)
        {
            return CreateReferencePreset();
        }
    }

    public bool IsUsable()
    {
        return panelWidth > 0f && panelHeight > 0f &&
               categories != null && categories.Length == 9 &&
               categoryIcons != null && categoryIcons.Length == 9 &&
               categoryTexts != null && categoryTexts.Length == 9 &&
               materials != null && materials.Length == 7 &&
               materialIcons != null && materialIcons.Length == 7 &&
               materialNames != null && materialNames.Length == 7 &&
               materialCounts != null && materialCounts.Length == 7 &&
               productCards != null && productCards.Length == 6 &&
               productImages != null && productImages.Length == 6 &&
               productNames != null && productNames.Length == 6 &&
               productRarities != null && productRarities.Length == 6 &&
               productStatuses != null && productStatuses.Length == 6 &&
               selectedProductFrame != null && selectedProduct != null &&
               effectHeading != null && requirementHeading != null &&
               effectRows != null && effectRows.Length == 2 &&
               effectTexts != null && effectTexts.Length == 2 &&
               requirementCards != null && requirementCards.Length == 2 &&
               requirementIcons != null && requirementIcons.Length == 2 &&
               requirementNames != null && requirementNames.Length == 2 &&
               requirementCounts != null && requirementCounts.Length == 2;
    }

    public static WorkshopLayoutConfig CreateReferencePreset()
    {
        WorkshopLayoutConfig config = new WorkshopLayoutConfig
        {
            title = FromPixels("title", 586, 0, 514, 106),
            titleText = FromPixels("titleText", 636, 20, 414, 66),
            closeButton = FromPixels("close", 1584, 18, 68, 68),
            categories = new WorkshopLayoutRect[9],
            categoryIcons = new WorkshopLayoutRect[9],
            categoryTexts = new WorkshopLayoutRect[9],
            materials = new WorkshopLayoutRect[7],
            materialIcons = new WorkshopLayoutRect[7],
            materialNames = new WorkshopLayoutRect[7],
            materialCounts = new WorkshopLayoutRect[7],
            preview = FromPixels("preview", 252, 220, 850, 458),
            previewFrame = FromPixels("previewFrame", 244, 212, 866, 474),
            productCards = new WorkshopLayoutRect[6],
            productImages = new WorkshopLayoutRect[6],
            productNames = new WorkshopLayoutRect[6],
            productRarities = new WorkshopLayoutRect[6],
            productStatuses = new WorkshopLayoutRect[6],
            detailPanel = FromPixels("detailPanel", 1110, 218, 544, 704),
            detailTitle = FromPixels("detailTitle", 1140, 245, 484, 58),
            detailCategory = FromPixels("detailCategory", 1150, 305, 190, 36),
            detailRarity = FromPixels("detailRarity", 1434, 305, 170, 36),
            selectedProductFrame = FromPixels("selectedProductFrame", 1127, 339, 250, 250),
            selectedProduct = FromPixels("selectedProduct", 1160, 372, 184, 184),
            effectHeading = FromPixels("effectHeading", 1378, 350, 232, 36),
            effectRows = new[]
            {
                FromPixels("effect0", 1378, 397, 232, 64),
                FromPixels("effect1", 1378, 477, 232, 64)
            },
            effectTexts = new[]
            {
                FromPixels("effectText0", 1390, 412, 208, 34),
                FromPixels("effectText1", 1390, 492, 208, 34)
            },
            requirementHeading = FromPixels("requirementHeading", 1180, 600, 404, 38),
            requirementCards = new[]
            {
                FromPixels("requirement0", 1140, 650, 220, 150),
                FromPixels("requirement1", 1382, 650, 220, 150)
            },
            requirementIcons = new WorkshopLayoutRect[2],
            requirementNames = new WorkshopLayoutRect[2],
            requirementCounts = new WorkshopLayoutRect[2],
            actionButton = FromPixels("action", 1220, 816, 320, 78),
            actionText = new WorkshopLayoutRect("actionText", 0f, 0f, 240f, 46f)
        };

        for (int i = 0; i < config.categories.Length; i++)
        {
            config.categories[i] = FromPixels("category" + i, 14, 45 + (i * 90), 215, 84);
            config.categoryIcons[i] = new WorkshopLayoutRect("categoryIcon" + i, -68f, 1f, 54f, 54f);
            config.categoryTexts[i] = new WorkshopLayoutRect("categoryText" + i, 27f, 2f, 142f, 58f);
        }

        for (int i = 0; i < config.materials.Length; i++)
        {
            config.materials[i] = FromPixels("material" + i, 254 + (i * 199), 111, 190, 96);
            Vector2 center = config.materials[i].Position;
            config.materialIcons[i] = Absolute("materialIcon" + i, center + new Vector2(-58f, 0f), new Vector2(70f, 70f));
            config.materialNames[i] = Absolute("materialName" + i, center + new Vector2(32f, 18f), new Vector2(112f, 26f));
            config.materialCounts[i] = Absolute("materialCount" + i, center + new Vector2(32f, -19f), new Vector2(112f, 38f));
        }

        for (int i = 0; i < config.productCards.Length; i++)
        {
            config.productCards[i] = FromPixels("product" + i, 252 + (i * 146), 690, 134, 226);
            config.productImages[i] = new WorkshopLayoutRect("productImage" + i, 0f, 28f, 116f, 104f);
            config.productNames[i] = new WorkshopLayoutRect("productName" + i, 0f, -44f, 124f, 42f);
            config.productRarities[i] = new WorkshopLayoutRect("productRarity" + i, 0f, 91f, 120f, 22f);
            config.productStatuses[i] = new WorkshopLayoutRect("productStatus" + i, 0f, -87f, 120f, 24f);
        }

        for (int i = 0; i < config.requirementCards.Length; i++)
        {
            Vector2 center = config.requirementCards[i].Position;
            config.requirementIcons[i] = Absolute("requirementIcon" + i, center + new Vector2(-55f, 8f), new Vector2(78f, 78f));
            config.requirementNames[i] = Absolute("requirementName" + i, center + new Vector2(42f, 24f), new Vector2(112f, 34f));
            config.requirementCounts[i] = Absolute("requirementCount" + i, center + new Vector2(42f, -24f), new Vector2(112f, 40f));
        }

        return config;
    }

    private static WorkshopLayoutRect FromPixels(string id, float left, float top, float width, float height)
    {
        float x = left + (width * 0.5f) - (ReferenceWidth * 0.5f);
        float y = (ReferenceHeight * 0.5f) - top - (height * 0.5f);
        return new WorkshopLayoutRect(id, x, y, width, height);
    }

    private static WorkshopLayoutRect Absolute(string id, Vector2 position, Vector2 size)
    {
        return new WorkshopLayoutRect(id, position.x, position.y, size.x, size.y);
    }
}
