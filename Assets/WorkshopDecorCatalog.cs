using System;
using System.Collections.Generic;

public enum WorkshopDecorCategory
{
    Wall,
    Floor,
    Table,
    Chair,
    Rug,
    Shelf,
    Counter,
    Plant,
    WallArt
}

public enum WorkshopDecorRarity
{
    Common,
    Rare,
    Luxury
}

public enum WorkshopCafeEffectType
{
    CustomerRate,
    ServiceSpeed,
    CoinIncome,
    WaitTolerance,
    VipChance,
    PassiveIncome,
    PreparationSpeed,
    Tips,
    XpIncome
}

public enum WorkshopCraftMaterialType
{
    Paint,
    Wood,
    Fabric,
    Metal,
    Stone,
    Glass,
    Ceramic
}

[Serializable]
public sealed class WorkshopCraftRequirement
{
    public WorkshopCraftMaterialType Material;
    public int Amount;

    public WorkshopCraftRequirement(WorkshopCraftMaterialType material, int amount)
    {
        Material = material;
        Amount = amount;
    }
}

[Serializable]
public sealed class WorkshopDecorProduct
{
    public string Id;
    public string DisplayName;
    public WorkshopDecorCategory Category;
    public WorkshopDecorRarity Rarity;
    public string VariantKey;

    // Temiz urun goruntusu; ileride sahne onizlemesi ve yerlestirme icin kullanilir.
    public string AtlasResourcePath;
    public int AtlasCellIndex;
    public int AtlasColumns = 3;
    public int AtlasRows = 2;

    // Nadirlik efektli Atolye karti.
    public string CardAtlasResourcePath;
    public int CardAtlasCellIndex;
    public int CardAtlasColumns = 6;
    public int CardAtlasRows = 3;

    public WorkshopCafeEffectType PrimaryEffect;
    public int PrimaryEffectValue;
    public bool HasSecondaryEffect;
    public WorkshopCafeEffectType SecondaryEffect;
    public int SecondaryEffectValue;
    public bool AvailableInWorkshop;
    public WorkshopCraftRequirement[] Recipe;

    public WorkshopDecorProduct(
        string id,
        string displayName,
        WorkshopDecorCategory category,
        WorkshopDecorRarity rarity,
        string variantKey,
        string atlasResourcePath,
        int atlasCellIndex,
        string cardAtlasResourcePath,
        int cardAtlasCellIndex,
        WorkshopCafeEffectType primaryEffect,
        WorkshopCafeEffectType secondaryEffect,
        WorkshopCraftRequirement[] recipe)
    {
        Id = id;
        DisplayName = displayName;
        Category = category;
        Rarity = rarity;
        VariantKey = variantKey;
        AtlasResourcePath = atlasResourcePath;
        AtlasCellIndex = atlasCellIndex;
        CardAtlasResourcePath = cardAtlasResourcePath;
        CardAtlasCellIndex = cardAtlasCellIndex;
        PrimaryEffect = primaryEffect;
        PrimaryEffectValue = WorkshopDecorCatalog.GetPrimaryEffectValue(rarity);
        HasSecondaryEffect = rarity == WorkshopDecorRarity.Luxury;
        SecondaryEffect = secondaryEffect;
        SecondaryEffectValue = HasSecondaryEffect ? 2 : 0;
        AvailableInWorkshop = true;
        Recipe = recipe;
    }
}

[Serializable]
public sealed class WorkshopCraftMaterialDefinition
{
    public WorkshopCraftMaterialType Type;
    public string Id;
    public string DisplayName;
    public string AtlasResourcePath;
    public int AtlasCellIndex;
    public int AtlasColumns = 4;
    public int AtlasRows = 2;

    public WorkshopCraftMaterialDefinition(
        WorkshopCraftMaterialType type,
        string id,
        string displayName,
        int atlasCellIndex)
    {
        Type = type;
        Id = id;
        DisplayName = displayName;
        AtlasResourcePath = WorkshopDecorCatalog.CraftMaterialAtlasPath;
        AtlasCellIndex = atlasCellIndex;
    }
}

public static class WorkshopDecorCatalog
{
    public const string AtlasRoot = "Art/WorkshopCatalog/Atlases/";
    public const string CardAtlasRoot = "Art/WorkshopCatalog/Cards/";
    public const string CraftMaterialAtlasPath = AtlasRoot + "workshop_material_icons_v2";
    public const string PanelReferencePath = "Art/UI/Workshop/workshop_panel_effects_reference";

    // Atölye panelinin kullandığı üretilebilir dekor ve malzeme kataloğu.
    public static readonly IReadOnlyList<WorkshopDecorProduct> Products = BuildProducts();
    public static readonly IReadOnlyList<WorkshopCraftMaterialDefinition> CraftMaterials = BuildCraftMaterials();

    public static int GetPrimaryEffectValue(WorkshopDecorRarity rarity)
    {
        if (rarity == WorkshopDecorRarity.Rare) return 5;
        if (rarity == WorkshopDecorRarity.Luxury) return 8;
        return 2;
    }

    public static string GetRarityDisplayName(WorkshopDecorRarity rarity)
    {
        if (rarity == WorkshopDecorRarity.Rare) return "NADİR";
        if (rarity == WorkshopDecorRarity.Luxury) return "LÜKS";
        return "SIRADAN";
    }

    public static string GetEffectDisplayName(WorkshopCafeEffectType effect)
    {
        switch (effect)
        {
            case WorkshopCafeEffectType.CustomerRate: return "MÜŞTERİ HIZI";
            case WorkshopCafeEffectType.ServiceSpeed: return "SERVİS HIZI";
            case WorkshopCafeEffectType.CoinIncome: return "COIN GELİRİ";
            case WorkshopCafeEffectType.WaitTolerance: return "BEKLEME SÜRESİ";
            case WorkshopCafeEffectType.VipChance: return "VIP ŞANSI";
            case WorkshopCafeEffectType.PassiveIncome: return "PASİF GELİR";
            case WorkshopCafeEffectType.PreparationSpeed: return "HAZIRLAMA HIZI";
            case WorkshopCafeEffectType.Tips: return "BAHŞİŞ";
            case WorkshopCafeEffectType.XpIncome: return "XP GELİRİ";
            default: return "KAFE ETKİSİ";
        }
    }

    public static string GetEffectValueText(WorkshopCafeEffectType effect, int value)
    {
        return effect == WorkshopCafeEffectType.WaitTolerance ? "+" + value + " sn" : "+" + value + "%";
    }

    private static IReadOnlyList<WorkshopDecorProduct> BuildProducts()
    {
        List<WorkshopDecorProduct> products = new List<WorkshopDecorProduct>(54);

        AddVariants(products, WorkshopDecorCategory.Wall, "wall", "Duvar", "decor_wall_variants", "workshop_collection_1", 0,
            WorkshopCafeEffectType.CustomerRate, WorkshopCafeEffectType.Tips,
            new[] { "Krem Şampanya", "Ceviz", "Gül Kurusu", "Mint", "Bordo", "Lacivert" },
            new[] { "cream", "walnut", "rose", "mint", "burgundy", "navy" },
            new[] { 2, 3, 0, 1, 4, 5 });

        AddVariants(products, WorkshopDecorCategory.Floor, "floor", "Zemin", "decor_floor_variants", "workshop_collection_1", 1,
            WorkshopCafeEffectType.ServiceSpeed, WorkshopCafeEffectType.Tips,
            new[] { "Bal Meşesi", "Gül Terrazzo", "Mint Geometrik", "Bordo Damalı", "Krem Mermer", "Antrasit Taş" },
            new[] { "honey_oak", "rose_terrazzo", "mint_geometric", "burgundy_checker", "cream_marble", "charcoal_stone" },
            new[] { 0, 2, 3, 4, 1, 5 });

        AddVariants(products, WorkshopDecorCategory.Table, "table", "Masa", "decor_table_variants", "workshop_collection_1", 2,
            WorkshopCafeEffectType.CoinIncome, WorkshopCafeEffectType.XpIncome,
            new[] { "Bal Meşesi", "Gül Kurusu", "Mint", "Bordo", "Krem Altın", "Siyah Mermer" },
            new[] { "honey_oak", "rose", "mint", "burgundy", "cream_gold", "black_marble" },
            new[] { 0, 2, 3, 4, 1, 5 });

        AddVariants(products, WorkshopDecorCategory.Chair, "chair", "Sandalye", "decor_chair_variants", "workshop_collection_2", 0,
            WorkshopCafeEffectType.WaitTolerance, WorkshopCafeEffectType.Tips,
            new[] { "Zeytin", "Krem", "Gül Kurusu", "Mint", "Bordo", "Antrasit" },
            new[] { "olive", "cream", "rose", "mint", "burgundy", "charcoal" },
            new[] { 0, 1, 2, 3, 4, 5 });

        AddVariants(products, WorkshopDecorCategory.Rug, "rug", "Halı", "decor_rug_variants", "workshop_collection_2", 1,
            WorkshopCafeEffectType.VipChance, WorkshopCafeEffectType.CustomerRate,
            new[] { "Ceviz Kum", "Gül Kurusu", "Mint", "Bordo", "Krem Altın", "Lacivert" },
            new[] { "walnut_sand", "rose", "mint", "burgundy", "cream_gold", "navy" },
            new[] { 0, 2, 3, 4, 1, 5 });

        AddVariants(products, WorkshopDecorCategory.Shelf, "shelf", "Raf", "decor_shelf_variants", "workshop_collection_2", 2,
            WorkshopCafeEffectType.PassiveIncome, WorkshopCafeEffectType.CoinIncome,
            new[] { "Bal Meşesi", "Krem", "Gül Kurusu", "Mint", "Koyu Ceviz", "Antrasit" },
            new[] { "honey_oak", "cream", "rose", "mint", "dark_walnut", "charcoal" },
            new[] { 0, 1, 2, 3, 4, 5 });

        AddVariants(products, WorkshopDecorCategory.Counter, "counter", "Tezgâh", "decor_counter_variants", "workshop_collection_3", 0,
            WorkshopCafeEffectType.PreparationSpeed, WorkshopCafeEffectType.CoinIncome,
            new[] { "Bal Meşesi", "Gül Kurusu", "Mint", "Koyu Bordo", "Krem Altın", "Siyah Mermer" },
            new[] { "honey_oak", "rose", "mint", "dark_burgundy", "cream_gold", "black_marble" },
            new[] { 0, 2, 3, 4, 1, 5 });

        AddVariants(products, WorkshopDecorCategory.Plant, "plant", "Saksı Bitkisi", "decor_plant_variants", "workshop_collection_3", 1,
            WorkshopCafeEffectType.Tips, WorkshopCafeEffectType.CustomerRate,
            new[] { "Terrakota", "Krem", "Gül Kurusu", "Mint", "Bordo", "Antrasit Altın" },
            new[] { "terracotta", "cream", "rose", "mint", "burgundy", "charcoal_gold" },
            new[] { 0, 1, 2, 3, 4, 5 });

        AddVariants(products, WorkshopDecorCategory.WallArt, "wall_art", "Duvar Tablosu", "decor_wall_art_variants", "workshop_collection_3", 2,
            WorkshopCafeEffectType.XpIncome, WorkshopCafeEffectType.VipChance,
            new[] { "Ceviz", "Gül Kurusu", "Mint", "Bordo", "Altın Krem", "Antrasit Altın" },
            new[] { "walnut", "rose", "mint", "burgundy", "gold_cream", "charcoal_gold" },
            new[] { 0, 2, 3, 4, 1, 5 });

        return products;
    }

    private static IReadOnlyList<WorkshopCraftMaterialDefinition> BuildCraftMaterials()
    {
        return new List<WorkshopCraftMaterialDefinition>
        {
            new WorkshopCraftMaterialDefinition(WorkshopCraftMaterialType.Paint, "paint", "Boya", 0),
            new WorkshopCraftMaterialDefinition(WorkshopCraftMaterialType.Wood, "wood", "Tahta", 1),
            new WorkshopCraftMaterialDefinition(WorkshopCraftMaterialType.Fabric, "fabric", "Kumaş", 2),
            new WorkshopCraftMaterialDefinition(WorkshopCraftMaterialType.Metal, "metal", "Metal", 3),
            new WorkshopCraftMaterialDefinition(WorkshopCraftMaterialType.Stone, "stone", "Taş / Mermer", 4),
            new WorkshopCraftMaterialDefinition(WorkshopCraftMaterialType.Glass, "glass", "Cam", 5),
            new WorkshopCraftMaterialDefinition(WorkshopCraftMaterialType.Ceramic, "ceramic", "Seramik", 6)
        };
    }

    private static void AddVariants(
        List<WorkshopDecorProduct> products,
        WorkshopDecorCategory category,
        string idPrefix,
        string categoryName,
        string atlasName,
        string cardAtlasName,
        int cardRow,
        WorkshopCafeEffectType primaryEffect,
        WorkshopCafeEffectType secondaryEffect,
        string[] variantNames,
        string[] variantKeys,
        int[] sourceCellIndices)
    {
        for (int i = 0; i < variantNames.Length; i++)
        {
            WorkshopDecorRarity rarity = i < 2
                ? WorkshopDecorRarity.Common
                : (i < 4 ? WorkshopDecorRarity.Rare : WorkshopDecorRarity.Luxury);

            products.Add(new WorkshopDecorProduct(
                idPrefix + "_" + variantKeys[i] + "_001",
                variantNames[i] + " " + categoryName,
                category,
                rarity,
                variantKeys[i],
                AtlasRoot + atlasName,
                sourceCellIndices[i],
                CardAtlasRoot + cardAtlasName,
                cardRow * 6 + i,
                primaryEffect,
                secondaryEffect,
                CreateRecipe(category, rarity, variantKeys[i])));
        }
    }

    private static WorkshopCraftRequirement[] CreateRecipe(
        WorkshopDecorCategory category,
        WorkshopDecorRarity rarity,
        string variantKey)
    {
        switch (category)
        {
            case WorkshopDecorCategory.Wall:
                if (variantKey == "cream") return Recipe(WorkshopCraftMaterialType.Paint, 2, WorkshopCraftMaterialType.Wood, 2);
                if (variantKey == "walnut") return Recipe(WorkshopCraftMaterialType.Wood, 3, WorkshopCraftMaterialType.Paint, 1);
                if (variantKey == "rose" || variantKey == "mint") return Recipe(WorkshopCraftMaterialType.Paint, 3, WorkshopCraftMaterialType.Wood, 3);
                return Recipe(WorkshopCraftMaterialType.Paint, 5, WorkshopCraftMaterialType.Wood, 4);

            case WorkshopDecorCategory.Floor:
                if (variantKey == "honey_oak") return Recipe(WorkshopCraftMaterialType.Wood, 4, WorkshopCraftMaterialType.Paint, 1);
                if (variantKey == "rose_terrazzo") return Recipe(WorkshopCraftMaterialType.Stone, 3, WorkshopCraftMaterialType.Ceramic, 2);
                if (variantKey == "mint_geometric") return Recipe(WorkshopCraftMaterialType.Stone, 5, WorkshopCraftMaterialType.Ceramic, 2);
                if (variantKey == "burgundy_checker") return Recipe(WorkshopCraftMaterialType.Stone, 5, WorkshopCraftMaterialType.Paint, 2);
                if (variantKey == "cream_marble") return Recipe(WorkshopCraftMaterialType.Stone, 7, WorkshopCraftMaterialType.Metal, 2);
                return Recipe(WorkshopCraftMaterialType.Stone, 8, WorkshopCraftMaterialType.Metal, 2);

            case WorkshopDecorCategory.Table:
                if (variantKey == "honey_oak") return Recipe(WorkshopCraftMaterialType.Wood, 4, WorkshopCraftMaterialType.Metal, 1);
                if (variantKey == "rose") return Recipe(WorkshopCraftMaterialType.Wood, 3, WorkshopCraftMaterialType.Paint, 2);
                if (variantKey == "mint") return Recipe(WorkshopCraftMaterialType.Wood, 5, WorkshopCraftMaterialType.Paint, 2);
                if (variantKey == "burgundy") return Recipe(WorkshopCraftMaterialType.Wood, 5, WorkshopCraftMaterialType.Paint, 3);
                if (variantKey == "cream_gold") return Recipe(WorkshopCraftMaterialType.Wood, 6, WorkshopCraftMaterialType.Metal, 4);
                return Recipe(WorkshopCraftMaterialType.Stone, 6, WorkshopCraftMaterialType.Metal, 4);

            case WorkshopDecorCategory.Chair:
                if (rarity == WorkshopDecorRarity.Common) return Recipe(WorkshopCraftMaterialType.Wood, 3, WorkshopCraftMaterialType.Fabric, 2);
                if (rarity == WorkshopDecorRarity.Rare) return Recipe(WorkshopCraftMaterialType.Wood, 4, WorkshopCraftMaterialType.Fabric, 4);
                return Recipe(WorkshopCraftMaterialType.Wood, 5, WorkshopCraftMaterialType.Fabric, 5);

            case WorkshopDecorCategory.Rug:
                if (variantKey == "walnut_sand") return Recipe(WorkshopCraftMaterialType.Fabric, 4, WorkshopCraftMaterialType.Paint, 1);
                if (variantKey == "rose") return Recipe(WorkshopCraftMaterialType.Fabric, 4, WorkshopCraftMaterialType.Paint, 2);
                if (variantKey == "mint") return Recipe(WorkshopCraftMaterialType.Fabric, 6, WorkshopCraftMaterialType.Paint, 2);
                if (variantKey == "burgundy") return Recipe(WorkshopCraftMaterialType.Fabric, 6, WorkshopCraftMaterialType.Paint, 3);
                if (variantKey == "cream_gold") return Recipe(WorkshopCraftMaterialType.Fabric, 7, WorkshopCraftMaterialType.Metal, 2);
                return Recipe(WorkshopCraftMaterialType.Fabric, 8, WorkshopCraftMaterialType.Paint, 3);

            case WorkshopDecorCategory.Shelf:
                if (variantKey == "honey_oak") return Recipe(WorkshopCraftMaterialType.Wood, 4, WorkshopCraftMaterialType.Metal, 1);
                if (variantKey == "cream") return Recipe(WorkshopCraftMaterialType.Wood, 3, WorkshopCraftMaterialType.Paint, 2);
                if (variantKey == "rose" || variantKey == "mint") return Recipe(WorkshopCraftMaterialType.Wood, 5, WorkshopCraftMaterialType.Paint, 2);
                if (variantKey == "dark_walnut") return Recipe(WorkshopCraftMaterialType.Wood, 7, WorkshopCraftMaterialType.Metal, 2);
                return Recipe(WorkshopCraftMaterialType.Wood, 6, WorkshopCraftMaterialType.Metal, 3);

            case WorkshopDecorCategory.Counter:
                if (variantKey == "honey_oak") return Recipe(WorkshopCraftMaterialType.Wood, 4, WorkshopCraftMaterialType.Stone, 2);
                if (variantKey == "rose") return Recipe(WorkshopCraftMaterialType.Wood, 4, WorkshopCraftMaterialType.Paint, 2);
                if (variantKey == "mint" || variantKey == "dark_burgundy") return Recipe(WorkshopCraftMaterialType.Wood, 6, WorkshopCraftMaterialType.Paint, 3);
                if (variantKey == "cream_gold") return Recipe(WorkshopCraftMaterialType.Wood, 7, WorkshopCraftMaterialType.Metal, 3);
                return Recipe(WorkshopCraftMaterialType.Stone, 7, WorkshopCraftMaterialType.Metal, 4);

            case WorkshopDecorCategory.Plant:
                if (rarity == WorkshopDecorRarity.Common) return Recipe(WorkshopCraftMaterialType.Ceramic, 3, WorkshopCraftMaterialType.Paint, 1);
                if (rarity == WorkshopDecorRarity.Rare) return Recipe(WorkshopCraftMaterialType.Ceramic, 4, WorkshopCraftMaterialType.Paint, 2);
                if (variantKey == "charcoal_gold") return Recipe(WorkshopCraftMaterialType.Ceramic, 5, WorkshopCraftMaterialType.Metal, 3);
                return Recipe(WorkshopCraftMaterialType.Ceramic, 6, WorkshopCraftMaterialType.Paint, 3);

            case WorkshopDecorCategory.WallArt:
                if (rarity == WorkshopDecorRarity.Common) return Recipe(WorkshopCraftMaterialType.Wood, 3, WorkshopCraftMaterialType.Glass, 1);
                if (rarity == WorkshopDecorRarity.Rare) return Recipe(WorkshopCraftMaterialType.Wood, 4, WorkshopCraftMaterialType.Glass, 2);
                return Recipe(WorkshopCraftMaterialType.Wood, 5, WorkshopCraftMaterialType.Glass, 3);

            default:
                return Recipe(WorkshopCraftMaterialType.Paint, 1, WorkshopCraftMaterialType.Wood, 1);
        }
    }

    private static WorkshopCraftRequirement[] Recipe(
        WorkshopCraftMaterialType firstMaterial,
        int firstAmount,
        WorkshopCraftMaterialType secondMaterial,
        int secondAmount)
    {
        return new[]
        {
            new WorkshopCraftRequirement(firstMaterial, firstAmount),
            new WorkshopCraftRequirement(secondMaterial, secondAmount)
        };
    }
}
