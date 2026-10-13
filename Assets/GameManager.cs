using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameManager : MonoBehaviour
{
    private enum ItemRarity
    {
        Common,
        Rare,
        Luxury
    }

    private enum ItemCategory
    {
        Material,
        Decor
    }

    private enum ChestTier
    {
        Common,
        Rare,
        Luxury
    }

    private enum DecorSlot
    {
        Wall,
        Floor,
        Table
    }

    private class ItemData
    {
        public string Id;
        public string DisplayName;
        public ItemCategory Category;
        public ItemRarity Rarity;
        public Color PaintColor;

        public ItemData(string id, string displayName, ItemCategory category, ItemRarity rarity, Color paintColor)
        {
            Id = id;
            DisplayName = displayName;
            Category = category;
            Rarity = rarity;
            PaintColor = paintColor;
        }
    }

    private class DecorItemData
    {
        public int PreviewId;
        public string ItemId;
        public string DisplayName;
        public DecorSlot Slot;
        public int StyleId;
        public string CostResource;
        public int CostAmount;

        public DecorItemData(int previewId, string itemId, string displayName, DecorSlot slot, int styleId, string costResource, int costAmount)
        {
            PreviewId = previewId;
            ItemId = itemId;
            DisplayName = displayName;
            Slot = slot;
            StyleId = styleId;
            CostResource = costResource;
            CostAmount = costAmount;
        }
    }

    private sealed class WorkshopProductCardUi
    {
        public GameObject Root;
        public Image Background;
        public Image ProductImage;
        public TMP_Text NameText;
        public TMP_Text RarityText;
        public TMP_Text StatusText;
        public Button Button;
        public WorkshopDecorProduct Product;
    }

    private enum FoodUpgradeCategory
    {
        Coffee,
        Sweet,
        Snack,
        Cold
    }

    private class FoodUpgradeData
    {
        public string Id;
        public string DisplayName;
        public FoodUpgradeCategory Category;
        public string IconResourceName;
        public int BaseCost;
        public int UnlockPlayerLevel;
        public int Level;
        public int SalesBonusPercent;
        public int PassiveBonusPercent;

        public FoodUpgradeData(string id, string displayName, FoodUpgradeCategory category, string iconResourceName, int baseCost, int unlockPlayerLevel, int salesBonusPercent, int passiveBonusPercent)
        {
            Id = id;
            DisplayName = displayName;
            Category = category;
            IconResourceName = iconResourceName;
            BaseCost = baseCost;
            UnlockPlayerLevel = unlockPlayerLevel;
            SalesBonusPercent = salesBonusPercent;
            PassiveBonusPercent = passiveBonusPercent;
            Level = 0;
        }
    }

    private class FoodUpgradeCardUi
    {
        public FoodUpgradeData Data;
        public GameObject RootObject;
        public Image CardImage;
        public Image IconImage;
        public Image LevelBadgeImage;
        public Image LevelRowImage;
        public Image OrderStripImage;
        public Image PassiveStripImage;
        public Image OrderIconImage;
        public Image PassiveIconImage;
        public Image PriceCoinImage;
        public Image BeanLeftImage;
        public Image BeanCenterImage;
        public Image BeanRightImage;
        public Image LockedBadgeImage;
        public Image LockedIconImage;
        public Image UnlockStripImage;
        public TMP_Text NameText;
        public TMP_Text LevelText;
        public TMP_Text LevelLabelText;
        public TMP_Text UnlockText;
        public TMP_Text OrderText;
        public TMP_Text PassiveText;
        public TMP_Text PriceText;
        public TMP_Text ButtonText;
        public Button Button;
    }

    private const float OfferCooldownSeconds = 600f;
    private const float OfferVisibleSeconds = 60f;
    private const float RewardBoostSeconds = 300f;
    private const float MaxBoostSeconds = 1800f;
    private const float OfflineIncomeRate = 0.33333334f;
    private const float MaxOfflineEarningsSeconds = 60f * 60f * 4f;
    private const float CustomerSpawnSeconds = 8f;
    private const float CustomerVisibleSeconds = 12f;
    private const float BaseBaristaAutoServeSeconds = 4.2f;
    private const float FastBaristaAutoServeSeconds = 2.6f;
    private const int UpgradeMaxLevel = 30;
    private const int NextTierRequiredLevel = 20;
    private const float UpgradeCostGrowth = 1.25f;
    private const int PremiumTierUnlockLevel = 5;
    private const int SignatureTierUnlockLevel = 10;
    private const int SeniorBaristaUnlockLevel = 6;
    private const int MasterBaristaUnlockLevel = 12;

    private const int BasicCoffeeTapGain = 1;
    private const int PremiumCoffeeTapGain = 5;
    private const int SignatureCoffeeTapGain = 20;
    private const float BasicMachineIncomeGain = 1.5f;
    private const float EspressoMachineIncomeGain = 8f;
    private const float PremiumMachineIncomeGain = 35f;
    private const float JuniorBaristaIncomeGain = 5f;
    private const float SeniorBaristaIncomeGain = 20f;
    private const float MasterBaristaIncomeGain = 80f;

    private const int NormalCustomerCoinMultiplier = 2;
    private const int RushCustomerCoinMultiplier = 4;
    private const int VipCustomerCoinMultiplier = 10;
    private const int NormalCustomerXpReward = 1;
    private const int RushCustomerXpReward = 1;
    private const int VipCustomerXpReward = 3;
    private const double DailyRewardCooldownHours = 24;
    private const int DailyRewardCycleDays = 7;
    private const int DecorUnlockCost = 3;
    private const double DailyTasksResetHours = 24;
    private const int RoomCount = 3;
    private const int ProductionRoomUnlockCost = 5000;
    private const int VipRoomUnlockCost = 25000;
    private const int MaxVipLevel = 5;

    private readonly struct VipBonusSlotLayout
    {
        public readonly Vector2 PillPosition;
        public readonly Vector2 TextPosition;
        public readonly Vector2 TextSize;
        public readonly Color AccentColor;

        public VipBonusSlotLayout(Vector2 pillPosition, Vector2 textPosition, Vector2 textSize, Color accentColor)
        {
            PillPosition = pillPosition;
            TextPosition = textPosition;
            TextSize = textSize;
            AccentColor = accentColor;
        }
    }

    private static readonly VipBonusSlotLayout VipCoinBonusLayout = new VipBonusSlotLayout(
        new Vector2(-420f, -212f),
        new Vector2(-11.651f, -27f),
        new Vector2(90.661f, 48f),
        new Color(1f, 0.72f, 0.14f, 1f));

    private static readonly VipBonusSlotLayout VipXpBonusLayout = new VipBonusSlotLayout(
        new Vector2(-200f, -212f),
        new Vector2(3.4f, -25f),
        new Vector2(90.661f, 48f),
        new Color(0.35f, 0.55f, 1f, 1f));

    private static readonly VipBonusSlotLayout VipCustomerBonusLayout = new VipBonusSlotLayout(
        new Vector2(22f, -212f),
        new Vector2(33.4f, -25f),
        new Vector2(121.3214f, 48f),
        new Color(0.28f, 0.70f, 0.42f, 1f));

    private static readonly VipBonusSlotLayout VipPassiveBonusLayout = new VipBonusSlotLayout(
        new Vector2(244f, -212f),
        new Vector2(36.1f, -24f),
        new Vector2(121.3214f, 48f),
        new Color(0.92f, 0.24f, 0.32f, 1f));

    private static readonly VipBonusSlotLayout VipDiscountBonusLayout = new VipBonusSlotLayout(
        Vector2.zero,
        new Vector2(551.1f, -237f),
        new Vector2(168f, 72f),
        new Color(0.86f, 0.36f, 0.18f, 1f));

    private static readonly Vector2 VipUpgradeButtonPosition = new Vector2(0.6f, -302.4f);
    private static readonly Vector2 VipUpgradeButtonSize = new Vector2(146.8112f, 62f);
    private static readonly Vector2 VipUpgradeButtonTextPosition = new Vector2(-28f, -9f);
    private static readonly Vector2 VipUpgradeButtonTextSize = new Vector2(106f, 62f);
    private static readonly Vector2 VipUpgradeGemIconPosition = new Vector2(49f, -9f);
    private static readonly Vector2 VipUpgradeGemIconSize = new Vector2(42f, 42f);
    // Atlas ring bounds map to a shared local center: left/right are symmetric around X 0,
    // while top/bottom map to Y 97.5 at the card's rendered aspect ratio.
    private static readonly Vector2 VipCardMedallionCenter = new Vector2(0f, 97.5f);
    private static readonly Vector2 VipLeftBadgePosition = new Vector2(-507.21f, 80.079f);
    private static readonly Vector2 VipLeftBadgeSize = new Vector2(92f, 223.4507f);
    private static readonly Vector2 VipLeftProgressBarPosition = new Vector2(-453.8874f, -113.0993f);
    private static readonly Vector2 VipLeftProgressBarSize = new Vector2(322f, 44f);
    private static readonly Vector2 VipLeftProgressCrownPosition = new Vector2(-436.8f, -111.6f);
    private static readonly Vector2 VipLeftProgressCrownSize = new Vector2(322f, 44f);
    private static readonly Vector2 VipLeftGiftPosition = new Vector2(-253.0928f, -54.2053f);
    private static readonly Vector2 VipLeftGiftSize = new Vector2(61f, 65f);
    private static readonly Vector2 VipLeftClaimButtonPosition = new Vector2(-253.5431f, -109.2119f);
    private static readonly Vector2 VipLeftClaimButtonSize = new Vector2(124f, 44f);
    private static readonly Vector2 VipLeftCoinPillPosition = new Vector2(-499.4f, -20f);
    private static readonly Vector2 VipLeftXpPillPosition = new Vector2(-376.54f, -20f);
    private static readonly Vector2 VipLeftCustomerPillPosition = new Vector2(-499.4f, -67f);
    private static readonly Vector2 VipLeftPassivePillPosition = new Vector2(-377.4f, -67f);
    private static readonly Vector2 VipLeftBonusPillSize = new Vector2(122f, 36f);
    private static readonly Vector2 VipLeftDailyRewardLabelPosition = new Vector2(-441.4f, -122.5f);
    private static readonly Vector2 VipLeftDailyRewardLabelSize = new Vector2(250f, 75f);
    private static readonly Vector2 VipInfoActivePackageBadgePosition = new Vector2(-323f, 130f);
    private static readonly Vector2 VipInfoActivePackageBadgeSize = new Vector2(132f, 28f);
    private static readonly Vector2 VipInfoCurrentTitlePosition = new Vector2(-326f, 91f);
    private static readonly Vector2 VipInfoCurrentTitleSize = new Vector2(182f, 33f);
    private static readonly Vector2 VipInfoNextTitlePosition = new Vector2(-326f, 54f);
    private static readonly Vector2 VipInfoNextTitleSize = new Vector2(170f, 18f);
    private static readonly Vector2 VipInfoPriceLinePosition = new Vector2(-326f, 26f);
    private static readonly Vector2 VipInfoPriceLineSize = new Vector2(160f, 22f);
    private static readonly Vector2 VipLeftClaimButtonTextPosition = new Vector2(0f, 0f);
    private static readonly Vector2 VipLeftClaimButtonTextSize = new Vector2(96f, 34f);
    private static readonly Vector2 VipProfessionalInfoPosition = new Vector2(-384.8642f, 29.97939f);
    private static readonly Vector2 VipProfessionalInfoSize = new Vector2(628.9684f, 427.0721f);
    private const float DailyRewardLayoutScale = 1260f / 1728f;
    private const string DailyRewardTextSpriteRoot = "Art/UI/CoffeeUILayoutTool/DailyLoginV2/";

    private TMP_Text coinText;
    private readonly TMP_Text[] coinInfoPopupRows = new TMP_Text[6];
    private TMP_Text activeBoostText;
    private TMP_Text customerStatsText;
    private TMP_Text gemText;
    private TMP_Text chestText;
    private TMP_Text xpText;
    private TMP_Text levelBadgeText;
    private TMP_Text roomNameText;
    private TMP_Text roomUnlockButtonText;
    private TMP_Text taskPanelText;
    private TMP_Text taskRewardButtonText;
    private TMP_Text dailyTasksPanelText;
    private TMP_Text dailyTaskCoffeeButtonText;
    private TMP_Text dailyTaskCustomerButtonText;
    private TMP_Text dailyTaskChestButtonText;
    private TMP_Text dailyRewardPanelText;
    private TMP_Text dailyRewardButtonText;
    private GameObject dailyRewardDimmer;
    private TMP_Text dailyRewardTodayLabelText;
    private TMP_Text dailyRewardTodayAmountText;
    private TMP_Text dailyRewardClaimStateText;
    private TMP_Text dailyRewardTimerText;
    private readonly List<TMP_Text> dailyRewardDayNumberTexts = new List<TMP_Text>();
    private readonly List<Image> dailyRewardDayHighlightImages = new List<Image>();
    private readonly List<Image> dailyRewardStepBadgeImages = new List<Image>();
    private readonly List<TMP_Text> dailyRewardStepStatusTexts = new List<TMP_Text>();
    private readonly List<Image> dailyRewardDayNumberImages = new List<Image>();
    private readonly List<Image> dailyRewardStepStatusImages = new List<Image>();
    private readonly List<Vector2> dailyRewardStepStatusReferencePositions = new List<Vector2>();
    private Image dailyRewardClaimTextImage;
    private Sprite dailyRewardStatusClaimedSprite;
    private Sprite dailyRewardStatusAvailableSprite;
    private Sprite dailyRewardStatusLockedSprite;
    private Sprite dailyRewardStatusLockedLongSprite;
    private Sprite dailyRewardClaimSprite;
    private TMP_Text chestPanelText;
    private TMP_Text chestRewardTitleText;
    private TMP_Text commonChestCountText;
    private TMP_Text rareChestCountText;
    private TMP_Text luxuryChestCountText;
    private TMP_Text chestOpenButtonText;
    private TMP_Text rareChestOpenButtonText;
    private TMP_Text luxuryChestOpenButtonText;
    private TMP_Text decorPanelText;
    private TMP_Text decorDetailTitleText;
    private TMP_Text decorDetailTypeText;
    private TMP_Text decorDetailStatusText;
    private Image decorDetailIconImage;
    private Graphic decorPreviewBackWallImage;
    private Graphic decorPreviewLeftWallImage;
    private Graphic decorPreviewRightWallImage;
    private Graphic decorPreviewFloorImage;
    private Graphic decorPreviewTableTopImage;
    private Image decorPreviewBeforeSwatchImage;
    private Image decorPreviewAfterSwatchImage;
    private Image decorCostIconImage;
    private TMP_Text decorCostText;
    private Image decorStoragePaintIconImage;
    private TMP_Text decorStoragePaintText;
    private Image decorStorageWoodIconImage;
    private TMP_Text decorStorageWoodText;
    private readonly List<TMP_Text> decorTabTexts = new List<TMP_Text>();
    private TMP_Text pinkWallButtonText;
    private TMP_Text mintWallButtonText;
    private TMP_Text creamFloorButtonText;
    private TMP_Text roseTableButtonText;
    private TMP_Text decorActionButtonText;
    private TMP_Text workshopDetailEffectText;
    private readonly List<TMP_Text> workshopEffectTexts = new List<TMP_Text>();
    private readonly List<Image> workshopEffectFrameImages = new List<Image>();
    private TMP_Text workshopRecipeText;
    private readonly List<Image> workshopRequirementFrameImages = new List<Image>();
    private readonly List<Image> workshopRequirementIconImages = new List<Image>();
    private readonly List<TMP_Text> workshopRequirementTexts = new List<TMP_Text>();
    private readonly List<TMP_Text> workshopRequirementCountTexts = new List<TMP_Text>();
    private TMP_Text workshopMaterialSummaryText;
    private TMP_Text workshopCategoryTitleText;
    private Image workshopSelectedProductImage;
    private Image workshopCafePreviewImage;
    private TextureQuadGraphic workshopCafePreviewFloorGraphic;
    private readonly Dictionary<WorkshopDecorCategory, List<Image>> workshopCafePreviewObjectImages = new Dictionary<WorkshopDecorCategory, List<Image>>();
    private readonly List<Image> workshopMaterialIconImages = new List<Image>();
    private readonly List<TMP_Text> workshopMaterialCountTexts = new List<TMP_Text>();
    private readonly List<TMP_Text> workshopCategoryTabTexts = new List<TMP_Text>();
    private readonly List<Image> workshopCategoryTabImages = new List<Image>();
    private readonly List<Image> workshopCategoryIconImages = new List<Image>();
    private readonly List<WorkshopProductCardUi> workshopProductCards = new List<WorkshopProductCardUi>();
    private readonly HashSet<string> unlockedWorkshopProducts = new HashSet<string>(StringComparer.Ordinal);
    private readonly Dictionary<WorkshopDecorCategory, string> selectedWorkshopProducts = new Dictionary<WorkshopDecorCategory, string>();
    private WorkshopDecorCategory selectedWorkshopCategory = WorkshopDecorCategory.Wall;
    private int workshopCategoryPage = 0;
    private string selectedWorkshopPreviewId = "wall_cream_001";
    private TMP_Text basicCoffeeButtonText;
    private TMP_Text premiumCoffeeButtonText;
    private TMP_Text signatureCoffeeButtonText;
    private TMP_Text basicMachineButtonText;
    private TMP_Text espressoMachineButtonText;
    private TMP_Text premiumMachineButtonText;
    private TMP_Text baristaStatusText;
    private TMP_Text juniorBaristaButtonText;
    private TMP_Text seniorBaristaButtonText;
    private TMP_Text masterBaristaButtonText;
    private Image taskNotifyDot;
    private Image dailyTasksNotifyDot;
    private Image vipNotifyDot;
    private TMP_Text offlineText;
    private TMP_Text offlineAmountText;
    private TMP_Text adBoostButtonText;
    private TMP_Text offerButtonText;
    private TMP_Text offerPanelText;
    private TMP_Text customerButtonText;
    private TMP_Text vipBadgeText;
    private TMP_Text vipHudText;
    private TMP_Text vipHudLevelText;
    private Image vipHudIconImage;
    private TMP_Text vipPanelText;
    private TMP_Text vipLevelSummaryText;
    private TMP_Text vipUpgradeButtonText;
    private TMP_Text vipCoinBonusText;
    private TMP_Text vipXpBonusText;
    private TMP_Text vipCoffeeBonusText;
    private TMP_Text vipPassiveBonusText;
    private TMP_Text vipDecorBonusText;
    private TMP_Text vipInfoPanelText;
    private TMP_Text vipLeftCoinPillText;
    private TMP_Text vipLeftXpPillText;
    private TMP_Text vipLeftCustomerPillText;
    private TMP_Text vipLeftPassivePillText;
    private TMP_Text vipLeftClaimButtonText;
    private TMP_Text vipProfessionalTierTitleText;
    private TMP_Text vipProfessionalStatusText;
    private TMP_Text vipProfessionalUpgradeText;
    private TMP_Text vipProfessionalUpgradeCostText;
    private Button vipProfessionalUpgradeButton;
    private Button vipProfessionalClaimButton;
    private TMP_Text foodUpgradeButtonText;
    private TMP_Text foodCategoryProgressText;
    private TMP_Text foodCategorySalesText;
    private TMP_Text foodCategoryPassiveText;
    private Image vipUpgradeGemIconImage;
    private Image vipProfessionalUpgradeGemImage;
    private TMP_FontAsset vipPaytoneFontAsset;
    private readonly List<TMP_Text> vipLevelCardTexts = new List<TMP_Text>();
    private readonly List<TMP_Text> vipLevelCardTitleTexts = new List<TMP_Text>();
    private readonly List<TMP_Text> vipLevelCardBonusTexts = new List<TMP_Text>();
    private readonly List<Image> vipLevelCardImages = new List<Image>();
    private readonly List<Image> vipLevelLockImages = new List<Image>();
    private readonly List<Image> vipLevelSelectImages = new List<Image>();
    private readonly List<Image> vipLevelBottomStarImages = new List<Image>();
    private readonly List<RectTransform> vipLevelBottomStarRects = new List<RectTransform>();
    private readonly List<RectTransform> vipLevelButtonRects = new List<RectTransform>();
    private readonly Dictionary<int, Sprite> vipCardSprites = new Dictionary<int, Sprite>();
    private readonly Dictionary<int, Sprite> vipCardBottomStarSprites = new Dictionary<int, Sprite>();
    private Texture2D vipCardAtlasTexture;
    private Texture2D vipProfessionalIconAtlasTexture;
    private Sprite vipProfessionalPanelSprite;
    private readonly Dictionary<int, Sprite> vipProfessionalIconSprites = new Dictionary<int, Sprite>();
    private int vipUnlockFlashLevel = 0;
    private float vipUnlockFlashEndTime = 0f;
    private bool vipInfoSeen = false;
    private TMP_Text passiveIncomePanelText;
    private GameObject coinInfoPopup;
    private GameObject offlinePanel;
    private GameObject dailyRewardPanel;
    private GameObject chestPanel;
    private GameObject chestRewardPanel;
    private GameObject decorPanel;
    private GameObject testPanel;
    private GameObject dailyTasksPanel;
    private GameObject taskPanel;
    private GameObject coffeePanel;
    private GameObject machinePanel;
    private GameObject baristaPanel;
    private GameObject passiveIncomePanel;
    private GameObject offerButtonObject;
    private GameObject offerPanel;
    private GameObject adBoostConfirmPanel;
    private GameObject customerButtonObject;
    private GameObject vipBadgeObject;
    private GameObject vipPanel;
    private GameObject vipInfoPanel;
    private GameObject foodPanel;
    private RectTransform foodCardsContent;
    private ScrollRect foodCardsScrollRect;
    private TMP_Text coffeeUpgradeButtonText;
    private TMP_Text machineUpgradeButtonText;
    private TMP_Text baristaUpgradeButtonText;
    private TMP_Text chestRewardText;
    private Button coffeeUpgradeButton;
    private Button machineUpgradeButton;
    private Button baristaUpgradeButton;
    private Button foodUpgradeButton;
    private Image cafeSceneImage;
    private Image backgroundTapImage;
    private Image leftWallImage;
    private Image rightWallImage;
    private Image floorPreviewImage;
    private Image tablePreviewImage;
    private Image backWallTopTrim;
    private Image leftCornerLine;
    private Image rightCornerLine;
    private Image floorBackEdge;
    private Image floorCenterLine;
    private Image floorLeftPerspectiveLine;
    private Image floorRightPerspectiveLine;
    private Image roomAccentStrip;
    private IsoQuadGraphic isoFloorGraphic;
    private IsoQuadGraphic isoBackWallGraphic;
    private IsoQuadGraphic isoLeftWallGraphic;
    private IsoQuadGraphic isoRightWallGraphic;
    private IsoQuadGraphic isoFloorShadowGraphic;
    private Image productionMachineVisual;
    private Image productionConveyorVisual;
    private Image baristaSceneImage;
    private Image baristaLockedSceneImage;
    private Image baristaCounterForegroundImage;
    private Image coffeeMachineSceneImage;
    private Image coffeeMachineLockedPlaceholderImage;
    private Image vipCounterVisual;
    private Image vipRugVisual;
    private Image vipPlantVisual;
    private Image roomLockOverlay;
    private Image chestRewardCardFrameImage;
    private Image chestRewardIconImage;
    private Image chestOpeningImage;
    private Image chestOpeningTransitionImage;
    private RectTransform chestLayeredRootRect;
    private Image chestLayeredBodyImage;
    private Image chestLayeredLidImage;
    private Image chestOpeningGlowImage;
    private Image chestOpeningFlashImage;
    private Image chestOpeningPlatformImage;
    private Image chestOpeningOrbitImage;
    private Image chestOpeningShockwaveImage;
    private Image chestOpeningKeyImage;
    private Image chestOpeningScreenFlashImage;
    private TMP_Text chestOpeningTapText;
    private CanvasGroup chestRewardIconCanvasGroup;
    private CanvasGroup chestRewardTextCanvasGroup;
    private readonly List<Image> chestRewardTrailImages = new List<Image>();
    private readonly List<Image> chestOpeningRayImages = new List<Image>();
    private readonly List<Image> chestOpeningSparkleImages = new List<Image>();
    private Sprite chestGlowSprite;
    private Sprite chestRingSprite;
    private Sprite chestBeamSprite;
    private Sprite chestStarSprite;
    private Sprite chestRewardCardFrameSprite;
    private GameObject chestRewardCloseButtonObject;
    private readonly Dictionary<ChestTier, Sprite[]> chestAnimationFrames = new Dictionary<ChestTier, Sprite[]>();
    private readonly Dictionary<ChestTier, Sprite[]> chestSilhouetteFrames = new Dictionary<ChestTier, Sprite[]>();
    private readonly Dictionary<string, Sprite> chestLayerSprites = new Dictionary<string, Sprite>();
    private bool shouldAnimateChestReward;
    private bool chestOpeningInProgress;
    private ChestTier openingChestTier;
    private Coroutine chestOpeningCoroutine;
    private AudioSource chestOpeningAudioSource;
    private AudioClip chestKeyImpactClip;
    private AudioClip chestOpenCommonClip;
    private AudioClip chestOpenRareClip;
    private AudioClip chestOpenLuxuryClip;
    private AudioClip chestRewardLaunchClip;
    private AudioClip chestRewardRevealClip;
    private AudioSource saleCoinAudioSource;
    private AudioClip saleCoinClip;
    private float lastSaleCoinTime = -1f;
    private RectTransform saleFeedbackLayer;
    private Sprite saleFeedbackCoffeeSprite;
    private RectTransform saleCornerFeedbackRoot;
    private CanvasGroup saleCornerFeedbackGroup;
    private Image saleCornerFeedbackIcon;
    private TMP_Text saleCornerFeedbackText;
    private Coroutine saleCornerFeedbackCoroutine;
    private Image xpFillImage;
    private Sprite commonPaintCanSprite;
    private Sprite rarePaintCanSprite;
    private Sprite specialPaintCanSprite;
    private readonly Dictionary<string, Sprite> paintCanSprites = new Dictionary<string, Sprite>();
    private readonly Dictionary<string, Sprite> decorIconSprites = new Dictionary<string, Sprite>();
    private readonly Dictionary<WorkshopCraftMaterialType, Sprite> workshopMaterialSprites = new Dictionary<WorkshopCraftMaterialType, Sprite>();
    private readonly Dictionary<string, Texture2D> workshopProductAtlasTextures = new Dictionary<string, Texture2D>();
    private readonly Dictionary<string, Sprite> workshopProductSprites = new Dictionary<string, Sprite>();
    private readonly Dictionary<string, Sprite> workshopUiSprites = new Dictionary<string, Sprite>();
    private Texture2D workshopMaterialAtlasTexture;
    private Sprite craftPaintIconSprite;
    private Sprite chestButtonSprite;
    private Sprite coinIconSprite;
    private Sprite gemIconSprite;
    private Sprite woodIconSprite;
    private Sprite levelBadgeSprite;
    private Sprite vipIconSprite;
    private Sprite tasksMenuIconSprite;
    private Sprite dailyTasksMenuIconSprite;
    private Sprite dailyRewardMenuIconSprite;
    private Sprite decorMenuIconSprite;
    private Sprite notifyExclamationSprite;
    private Sprite adBoostIconSprite;
    private Sprite adBoostTimerBadgeSprite;
    private Sprite vipPanelBackgroundSprite;
    private Sprite vipLeftBadgeSprite;
    private Sprite vipLeftProgressBarSprite;
    private Sprite vipLeftProgressCrownSprite;
    private Sprite vipLeftGiftSprite;
    private Sprite vipLeftClaimButtonSprite;
    private Sprite vipLeftBonusPillSprite;
    private Sprite vipLeftDailyRewardLabelSprite;
    private Sprite vipInfoActivePackageBadgeSprite;
    private Sprite vipInfoCurrentTitleSprite;
    private Sprite vipInfoNextTitleSprite;
    private Sprite vipInfoPriceLineSprite;
    private Sprite vipLockIconSprite;
    private Sprite foodPanelBackgroundSprite;
    private Sprite foodCardFrameSprite;
    private Sprite foodCardLockedFrameSprite;
    private Sprite foodUpgradeButtonSprite;
    private Sprite foodLockedButtonSprite;
    private Sprite foodCategoryTabSelectedSprite;
    private Sprite foodCategoryTabNormalSprite;
    private Sprite foodCategoryInfoRibbonSprite;
    private Sprite foodParchmentWatermarkSprite;
    private Sprite foodLevelBadgeSprite;
    private Sprite foodCoinChipSprite;
    private Sprite foodPassiveChipSprite;
    private Sprite foodCloseButtonSprite;
    private Sprite warmWoodCafeSceneSprite;
    private Sprite warmWoodCafeSceneNoMachineSprite;
    private readonly Dictionary<string, Sprite> workshopWallSceneSprites = new Dictionary<string, Sprite>();
    private readonly Dictionary<string, Sprite> workshopEmptySceneSprites = new Dictionary<string, Sprite>();
    private Sprite coffeeMachineSceneSprite;
    private Sprite coffeeMachineLockedPlaceholderSprite;
    private Sprite baristaSceneSprite;
    private Sprite baristaLockedPlaceholderSprite;
    private Sprite baristaCounterForegroundSprite;
    private Sprite coinInfoPanelSprite;
    private readonly List<GameObject> startingRoomVisuals = new List<GameObject>();
    private readonly List<GameObject> cafeObjectHotspots = new List<GameObject>();
    private readonly List<GameObject> cafePrototypeVisuals = new List<GameObject>();
    private readonly List<FoodUpgradeData> foodUpgradeDatabase = new List<FoodUpgradeData>();
    private readonly List<FoodUpgradeCardUi> foodUpgradeCardUis = new List<FoodUpgradeCardUi>();
    private readonly Dictionary<string, Sprite> foodIconSprites = new Dictionary<string, Sprite>();
    private readonly Dictionary<FoodUpgradeCategory, TMP_Text> foodCategoryTabTexts = new Dictionary<FoodUpgradeCategory, TMP_Text>();
    private FoodUpgradeCategory selectedFoodCategory = FoodUpgradeCategory.Coffee;

    private float coins = 0;
    private int coinsPerClick = 1;
    private float coinsPerSecond = 0;

    private int coffeeUpgradeLevel = 0;
    private int coffeeUpgradeCost = 25;
    private int premiumCoffeeLevel = 0;
    private int premiumCoffeeCost = 750;
    private int signatureCoffeeLevel = 0;
    private int signatureCoffeeCost = 5000;

    private int machineUpgradeLevel = 0;
    private int machineUpgradeCost = 100;
    private int espressoMachineLevel = 0;
    private int espressoMachineCost = 1000;
    private int premiumMachineLevel = 0;
    private int premiumMachineCost = 10000;

    private int baristaUpgradeLevel = 0;
    private int baristaUpgradeCost = 250;
    private int seniorBaristaLevel = 0;
    private int seniorBaristaCost = 2500;
    private int masterBaristaLevel = 0;
    private int masterBaristaCost = 20000;

    private float saveTimer = 0;
    private float bonusTimer = 0;
    private float incomeMultiplier = 1;
    private float offerCooldownTimer = 10f;
    private float offerVisibleTimer = 0;
    private float customerSpawnTimer = 5f;
    private float customerVisibleTimer = 0;
    private float baristaAutoServeTimer = 0;
    private string currentCustomerName = "Musteri";
    private int currentCustomerCoinMultiplier = 2;
    private int currentCustomerXpReward = 1;
    private Color currentCustomerColor = new Color(0.18f, 0.58f, 0.32f);
    private int lastOfflineCoins = 0;

    private int gems = 0;
    private int chests = 0;
    private int rareChests = 0;
    private int luxuryChests = 0;
    private int paintPieces = 0;
    private int woodPieces = 0;
    private string lastChestRewardText = "Henuz sandik acilmadi.";
    private readonly Dictionary<string, int> itemInventory = new Dictionary<string, int>();
    private readonly List<ItemData> itemDatabase = new List<ItemData>();
    private readonly List<DecorItemData> decorItemDatabase = new List<DecorItemData>();
    private bool pinkWallUnlocked = false;
    private bool mintWallUnlocked = false;
    private bool creamFloorUnlocked = false;
    private bool roseTableUnlocked = false;
    private int selectedWallStyle = 0;
    private int selectedFloorStyle = 0;
    private int selectedTableStyle = 0;
    private int previewDecorItem = 0;
    private int xp = 0;
    private int playerLevel = 1;
    private int dailyRewardDay = 0;
    private string lastDailyRewardUtc = "";
    private int totalCoffeeSold = 0;
    private int customerSales = 0;
    private int dailyCoffeeSold = 0;
    private int dailyCustomerSales = 0;
    private int dailyChestsOpened = 0;
    private bool dailyCoffeeRewardClaimed = false;
    private bool dailyCustomerRewardClaimed = false;
    private bool dailyChestRewardClaimed = false;
    private string lastDailyTasksUtc = "";
    private int vipCustomerSales = 0;
    private int missedCustomers = 0;
    private bool firstTaskRewardClaimed = false;
    private bool secondTaskRewardClaimed = false;
    private bool thirdTaskRewardClaimed = false;
    private bool fourthTaskRewardClaimed = false;
    private bool fifthTaskRewardClaimed = false;
    private int seenTaskId = 0;
    private int currentRoomIndex = 0;
    private bool productionRoomUnlocked = false;
    private bool vipRoomUnlocked = false;
    private int vipLevel = 0;
    private int selectedVipPreviewLevel = 0;
    private string lastVipDailyRewardUtc = "";

    public IReadOnlyList<WorkshopDecorProduct> GetWorkshopDecorProducts()
    {
        return WorkshopDecorCatalog.Products;
    }

    public IReadOnlyList<WorkshopCraftMaterialDefinition> GetWorkshopCraftMaterials()
    {
        return WorkshopDecorCatalog.CraftMaterials;
    }

    public int GetWorkshopCraftMaterialCount(WorkshopCraftMaterialType material)
    {
        if (material == WorkshopCraftMaterialType.Paint) return GetAvailablePaintCanCount();
        if (material == WorkshopCraftMaterialType.Wood) return woodPieces;
        return GetInventoryItemCount(GetWorkshopMaterialInventoryId(material));
    }

    public void GrantWorkshopCraftMaterial(WorkshopCraftMaterialType material, int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        if (material == WorkshopCraftMaterialType.Paint)
        {
            paintPieces += amount;
        }
        else if (material == WorkshopCraftMaterialType.Wood)
        {
            woodPieces += amount;
        }
        else
        {
            AddInventoryItem(GetWorkshopMaterialInventoryId(material), amount);
        }

        UpdateUI();
        SaveGame();
    }

    private void Start()
    {
        ForceLandscapeMode();
        InitializeItemDatabase();
        InitializeFoodUpgradeDatabase();
        LoadGame();
        RefreshDailyTasksIfNeeded();
        ApplyOfflineEarnings();
        BuildUI();
        UpdateUI();
        ShowDailyRewardIfAvailable();
    }

    private void ForceLandscapeMode()
    {
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        Screen.fullScreen = true;
    }

    private void Update()
    {
        UpdateBoostTimer();
        UpdateOfferTimers();
        UpdateCustomerTimers();
        UpdateBaristaAutoService();
        RefreshDailyTasksIfNeeded();

        if (coinsPerSecond > 0)
        {
            coins += GetCurrentPassiveIncome() * Time.deltaTime;
        }

        UpdateUI();

        saveTimer += Time.deltaTime;
        if (saveTimer >= 5f)
        {
            saveTimer = 0;
            SaveGame();
        }
    }

    private void UpdateBoostTimer()
    {
        if (bonusTimer <= 0)
        {
            bonusTimer = 0;
            incomeMultiplier = 1;
            return;
        }

        bonusTimer -= Time.deltaTime;
        if (bonusTimer <= 0)
        {
            bonusTimer = 0;
            incomeMultiplier = 1;
        }
    }


    private void UpdateOfferTimers()
    {
        if (offerVisibleTimer > 0)
        {
            offerVisibleTimer -= Time.deltaTime;
            if (offerVisibleTimer <= 0)
            {
                offerVisibleTimer = 0;
                offerCooldownTimer = OfferCooldownSeconds;
                if (offerPanel != null)
                {
                    offerPanel.SetActive(false);
                }
            }
            return;
        }

        offerCooldownTimer -= Time.deltaTime;
        if (offerCooldownTimer <= 0)
        {
            offerVisibleTimer = OfferVisibleSeconds;
            offerCooldownTimer = 0;
        }
    }

    private void UpdateCustomerTimers()
    {
        if (customerVisibleTimer > 0)
        {
            customerVisibleTimer -= Time.deltaTime;
            if (customerVisibleTimer <= 0)
            {
                customerVisibleTimer = 0;
                customerSpawnTimer = GetCustomerSpawnSeconds();
                missedCustomers++;
                SaveGame();
            }
            return;
        }

        customerSpawnTimer -= Time.deltaTime;
        if (customerSpawnTimer <= 0)
        {
            SpawnCustomer();
        }
    }

    private void UpdateBaristaAutoService()
    {
        if (!HasBaristaUnlocked() || customerVisibleTimer <= 0)
        {
            baristaAutoServeTimer = 0;
            return;
        }

        baristaAutoServeTimer += Time.deltaTime;
        if (baristaAutoServeTimer < GetBaristaAutoServeSeconds())
        {
            return;
        }

        baristaAutoServeTimer = 0;
        SellCoffeeToCustomer(false);
    }

    private bool HasBaristaUnlocked()
    {
        return GetTotalBaristaLevel() > 0;
    }

    private float GetBaristaAutoServeSeconds()
    {
        int totalBaristaLevel = Mathf.Clamp(GetTotalBaristaLevel(), 1, UpgradeMaxLevel * 3);
        float speedRatio = Mathf.Clamp01((totalBaristaLevel - 1) / 18f);
        float seconds = Mathf.Lerp(BaseBaristaAutoServeSeconds, FastBaristaAutoServeSeconds, speedRatio);
        int workshopSpeed = GetWorkshopEffectTotal(WorkshopCafeEffectType.ServiceSpeed)
            + GetWorkshopEffectTotal(WorkshopCafeEffectType.PreparationSpeed);
        return seconds / (1f + (workshopSpeed / 100f));
    }

    private float GetCustomerSpawnSeconds()
    {
        int rateBonus = GetWorkshopEffectTotal(WorkshopCafeEffectType.CustomerRate);
        return CustomerSpawnSeconds / (1f + (rateBonus / 100f));
    }

    private string GetBaristaStatusText()
    {
        if (!HasBaristaUnlocked())
        {
            return "Barista alininca gelen musterilere otomatik servis yapar ve offline kazanc baslar.";
        }

        return "Otomatik servis aktif: yaklasik " + GetBaristaAutoServeSeconds().ToString("0.#") + " sn | Offline kazanc aktif";
    }

    private void SpawnCustomer()
    {
        int roll = UnityEngine.Random.Range(0, 100);

        int vipThreshold = Mathf.Clamp(10 + GetWorkshopEffectTotal(WorkshopCafeEffectType.VipChance), 10, 60);
        if (roll < vipThreshold)
        {
            currentCustomerName = "VIP Müşteri";
            currentCustomerCoinMultiplier = VipCustomerCoinMultiplier;
            currentCustomerXpReward = VipCustomerXpReward;
            currentCustomerColor = new Color(0.78f, 0.50f, 0.08f);
            customerVisibleTimer = (8f + GetWorkshopEffectTotal(WorkshopCafeEffectType.WaitTolerance)) * GetVipCustomerPatienceMultiplier();
        }
        else if (roll < vipThreshold + 20)
        {
            currentCustomerName = "Aceleci Musteri";
            currentCustomerCoinMultiplier = RushCustomerCoinMultiplier;
            currentCustomerXpReward = RushCustomerXpReward;
            currentCustomerColor = new Color(0.82f, 0.28f, 0.18f);
            customerVisibleTimer = (6f + GetWorkshopEffectTotal(WorkshopCafeEffectType.WaitTolerance)) * GetVipCustomerPatienceMultiplier();
        }
        else
        {
            currentCustomerName = "Musteri";
            currentCustomerCoinMultiplier = NormalCustomerCoinMultiplier;
            currentCustomerXpReward = NormalCustomerXpReward;
            currentCustomerColor = new Color(0.18f, 0.58f, 0.32f);
            customerVisibleTimer = (CustomerVisibleSeconds + GetWorkshopEffectTotal(WorkshopCafeEffectType.WaitTolerance)) * GetVipCustomerPatienceMultiplier();
        }

        customerSpawnTimer = 0;
    }

    public void SellCoffee()
    {
        SellCoffeeFromScene();
    }

    public void SellCoffeeFromEmptySpace()
    {
        SellCoffeeFromScene();
    }

    private void SellCoffeeFromScene()
    {
        coins += GetCurrentTapPower();
        totalCoffeeSold++;
        dailyCoffeeSold++;
        UpdateUI();
        PlaySaleCoinSound();
        SaveGame();
    }

    public void SellCoffeeToCustomer()
    {
        SellCoffeeToCustomer(true);
    }

    private void SellCoffeeToCustomer(bool showTapFeedback)
    {
        bool soldToVip = currentCustomerName == "VIP Müşteri";
        coins += GetCurrentCustomerCoinReward();
        totalCoffeeSold++;
        customerSales++;
        dailyCoffeeSold++;
        dailyCustomerSales++;
        if (soldToVip)
        {
            vipCustomerSales++;
        }
        AddXp(currentCustomerXpReward);
        customerVisibleTimer = 0;
        customerSpawnTimer = GetCustomerSpawnSeconds();
        if (customerButtonObject != null)
        {
            customerButtonObject.SetActive(false);
        }
        UpdateUI();
        PlaySaleCoinSound();
        SaveGame();
    }

    public void AddTestCoins()
    {
        coins += 10000;
        UpdateUI();
        SaveGame();
    }

    public void AddHugeTestCoins()
    {
        coins += 10000000;
        UpdateUI();
        SaveGame();
    }

    public void AddTestGems()
    {
        gems += 1000;
        UpdateUI();
        SaveGame();
    }

    public void AddTestXp()
    {
        AddXp(100);
        UpdateUI();
        SaveGame();
    }

    public void AddTestChest()
    {
        chests++;
        lastChestRewardText = "Test sandigi eklendi.";
        UpdateUI();
        SaveGame();
    }

    public void AddTestRareChest()
    {
        rareChests++;
        lastChestRewardText = "Test nadir sandigi eklendi.";
        UpdateUI();
        SaveGame();
    }

    public void AddTestLuxuryChest()
    {
        luxuryChests++;
        lastChestRewardText = "Test ozel sandigi eklendi.";
        UpdateUI();
        SaveGame();
    }

    public void AddTestVipLevel()
    {
        if (vipLevel >= MaxVipLevel) return;

        vipLevel++;
        selectedVipPreviewLevel = 0;
        UpdateUI();
        SaveGame();
    }

    public void OpenVipPanel()
    {
        if (vipPanel == null)
        {
            return;
        }

        selectedVipPreviewLevel = 0;
        vipPanel.SetActive(true);
        vipPanel.transform.SetAsLastSibling();
        if (!vipInfoSeen)
        {
            vipInfoSeen = true;
            ShowVipInfoPanel();
            SaveGame();
        }
        UpdateUI();
    }

    public void CloseVipInfoPanel()
    {
        if (vipInfoPanel != null)
        {
            vipInfoPanel.SetActive(false);
        }
    }

    private void ShowVipInfoPanel()
    {
        if (vipInfoPanel == null)
        {
            return;
        }

        vipInfoPanel.SetActive(true);
        vipInfoPanel.transform.SetAsLastSibling();
    }

    public void SelectVipPreviewLevel(int level)
    {
        selectedVipPreviewLevel = Mathf.Clamp(level, 1, MaxVipLevel);
        UpdateUI();
    }

    public void ClearVipPreviewSelection()
    {
        selectedVipPreviewLevel = 0;
        UpdateUI();
    }

    public void CloseVipPanel()
    {
        if (vipPanel != null)
        {
            vipPanel.SetActive(false);
        }
    }

    public void ClickVipLeftClaimButton()
    {
        if (!CanClaimVipDailyReward())
        {
            return;
        }

        int gemReward = vipLevel;
        gems += gemReward;

        string chestReward;
        if (vipLevel >= 5)
        {
            luxuryChests += 1;
            chestReward = "1 Özel Sandık";
        }
        else if (vipLevel >= 3)
        {
            rareChests += 1;
            chestReward = "1 Nadir Sandık";
        }
        else
        {
            chests += 1;
            chestReward = "1 Sıradan Sandık";
        }

        lastVipDailyRewardUtc = DateTime.UtcNow.Ticks.ToString();
        string rewardText = "Günlük VIP hediyesi: " + gemReward + " Elmas + " + chestReward;
        ShowChestReward(rewardText, GetVipLeftGiftSprite());
        UpdateUI();
        SaveGame();
    }

    public void UpgradeVipLevel()
    {
        if (vipLevel >= MaxVipLevel)
        {
            return;
        }

        int targetLevel = GetVipPurchaseTargetLevel();
        int cost = GetVipPackageCost(targetLevel);
        if (gems < cost)
        {
            return;
        }

        gems -= cost;
        vipLevel = targetLevel;
        selectedVipPreviewLevel = 0;
        vipUnlockFlashLevel = vipLevel;
        vipUnlockFlashEndTime = Time.time + 1.25f;
        UpdateUI();
        SaveGame();
    }

    public void ResetSave()
    {
        PlayerPrefs.DeleteAll();
        lastOfflineCoins = 0;
        coins = 0;
        coinsPerClick = 1;
        coinsPerSecond = 0;
        coffeeUpgradeLevel = 0;
        coffeeUpgradeCost = 25;
        premiumCoffeeLevel = 0;
        premiumCoffeeCost = 750;
        signatureCoffeeLevel = 0;
        signatureCoffeeCost = 5000;
        machineUpgradeLevel = 0;
        machineUpgradeCost = 100;
        espressoMachineLevel = 0;
        espressoMachineCost = 1000;
        premiumMachineLevel = 0;
        premiumMachineCost = 10000;
        baristaUpgradeLevel = 0;
        baristaUpgradeCost = 250;
        seniorBaristaLevel = 0;
        seniorBaristaCost = 2500;
        masterBaristaLevel = 0;
        masterBaristaCost = 20000;
        for (int i = 0; i < foodUpgradeDatabase.Count; i++)
        {
            foodUpgradeDatabase[i].Level = 0;
        }
        gems = 0;
        chests = 0;
        rareChests = 0;
        luxuryChests = 0;
        paintPieces = 0;
        woodPieces = 0;
        itemInventory.Clear();
        unlockedWorkshopProducts.Clear();
        selectedWorkshopProducts.Clear();
        selectedWorkshopCategory = WorkshopDecorCategory.Wall;
        selectedWorkshopPreviewId = "wall_cream_001";
        lastChestRewardText = "Henuz sandik acilmadi.";
        pinkWallUnlocked = false;
        mintWallUnlocked = false;
        creamFloorUnlocked = false;
        roseTableUnlocked = false;
        selectedWallStyle = 0;
        selectedFloorStyle = 0;
        selectedTableStyle = 0;
        currentRoomIndex = 0;
        productionRoomUnlocked = false;
        vipRoomUnlocked = false;
        vipLevel = 0;
        lastVipDailyRewardUtc = "";
        vipInfoSeen = false;
        xp = 0;
        playerLevel = 1;
        dailyRewardDay = 0;
        lastDailyRewardUtc = "";
        totalCoffeeSold = 0;
        customerSales = 0;
        dailyCoffeeSold = 0;
        dailyCustomerSales = 0;
        dailyChestsOpened = 0;
        dailyCoffeeRewardClaimed = false;
        dailyCustomerRewardClaimed = false;
        dailyChestRewardClaimed = false;
        lastDailyTasksUtc = DateTime.UtcNow.Ticks.ToString();
        vipCustomerSales = 0;
        missedCustomers = 0;
        firstTaskRewardClaimed = false;
        secondTaskRewardClaimed = false;
        thirdTaskRewardClaimed = false;
        fourthTaskRewardClaimed = false;
        fifthTaskRewardClaimed = false;
        seenTaskId = 0;
        bonusTimer = 0;
        incomeMultiplier = 1;
        offerCooldownTimer = 10f;
        offerVisibleTimer = 0;
        customerSpawnTimer = 5f;
        customerVisibleTimer = 0;
        baristaAutoServeTimer = 0;
        if (offlinePanel != null) offlinePanel.SetActive(false);
        if (dailyRewardPanel != null) dailyRewardPanel.SetActive(false);
        if (chestPanel != null) chestPanel.SetActive(false);
        if (decorPanel != null) decorPanel.SetActive(false);
        if (testPanel != null) testPanel.SetActive(false);
        if (dailyTasksPanel != null) dailyTasksPanel.SetActive(false);
        if (offerPanel != null) offerPanel.SetActive(false);
        if (adBoostConfirmPanel != null) adBoostConfirmPanel.SetActive(false);
        if (coffeePanel != null) coffeePanel.SetActive(false);
        if (machinePanel != null) machinePanel.SetActive(false);
        if (baristaPanel != null) baristaPanel.SetActive(false);
        if (passiveIncomePanel != null) passiveIncomePanel.SetActive(false);
        if (coinInfoPopup != null) coinInfoPopup.SetActive(false);
        if (vipPanel != null) vipPanel.SetActive(false);
        if (vipInfoPanel != null) vipInfoPanel.SetActive(false);
        if (customerButtonObject != null) customerButtonObject.SetActive(false);
        if (vipBadgeObject != null) vipBadgeObject.SetActive(false);
        UpdateUI();
        SaveGame();
    }

    public void WatchAdForBoost()
    {
        if (bonusTimer >= MaxBoostSeconds)
        {
            return;
        }

        if (adBoostConfirmPanel == null)
        {
            GrantAdBoostReward();
            return;
        }

        adBoostConfirmPanel.SetActive(true);
        adBoostConfirmPanel.transform.SetAsLastSibling();
        UpdateUI();
    }

    public void CloseAdBoostConfirmPanel()
    {
        if (adBoostConfirmPanel != null)
        {
            adBoostConfirmPanel.SetActive(false);
        }
    }

    public void ConfirmAdBoostReward()
    {
        CloseAdBoostConfirmPanel();
        GrantAdBoostReward();
    }

    private void GrantAdBoostReward()
    {
        bonusTimer = Mathf.Min(MaxBoostSeconds, bonusTimer + RewardBoostSeconds);
        incomeMultiplier = bonusTimer > 0 ? 2f : 1f;
        UpdateUI();
        SaveGame();
    }

    public void OpenTimedOfferPanel()
    {
        if (offerVisibleTimer <= 0 || offerPanel == null)
        {
            return;
        }

        offerPanel.SetActive(true);
        offerPanel.transform.SetAsLastSibling();
        UpdateUI();
    }

    public void WatchTimedOfferAd()
    {
        GrantAdBoostReward();
        offerVisibleTimer = 0;
        offerCooldownTimer = OfferCooldownSeconds;

        if (offerPanel != null)
        {
            offerPanel.SetActive(false);
        }

        UpdateUI();
        SaveGame();
    }
    public void OpenTaskPanel()
    {
        if (taskPanel == null)
        {
            return;
        }

        seenTaskId = GetCurrentTaskId();
        taskPanel.SetActive(true);
        taskPanel.transform.SetAsLastSibling();
        UpdateUI();
        SaveGame();
    }

    public void CloseTaskPanel()
    {
        if (taskPanel != null)
        {
            taskPanel.SetActive(false);
        }
    }

    public void OpenCoffeePanel()
    {
        if (coffeePanel == null)
        {
            return;
        }

        coffeePanel.SetActive(true);
        coffeePanel.transform.SetAsLastSibling();
        UpdateUI();
    }

    public void CloseCoffeePanel()
    {
        if (coffeePanel != null)
        {
            coffeePanel.SetActive(false);
        }
    }

    public void OpenMachinePanel()
    {
        if (machinePanel == null)
        {
            return;
        }

        machinePanel.SetActive(true);
        machinePanel.transform.SetAsLastSibling();
        UpdateUI();
    }

    public void CloseMachinePanel()
    {
        if (machinePanel != null)
        {
            machinePanel.SetActive(false);
        }
    }

    public void OpenBaristaPanel()
    {
        if (baristaPanel == null)
        {
            return;
        }

        baristaPanel.SetActive(true);
        baristaPanel.transform.SetAsLastSibling();
        UpdateUI();
    }

    public void CloseBaristaPanel()
    {
        if (baristaPanel != null)
        {
            baristaPanel.SetActive(false);
        }
    }

    public void OpenPassiveIncomePanel()
    {
        if (passiveIncomePanel == null)
        {
            return;
        }

        passiveIncomePanel.SetActive(true);
        passiveIncomePanel.transform.SetAsLastSibling();
        UpdateUI();
    }

    public void ClosePassiveIncomePanel()
    {
        if (passiveIncomePanel != null)
        {
            passiveIncomePanel.SetActive(false);
        }
    }

    public void OpenFoodUpgradePanel()
    {
        if (foodPanel == null)
        {
            return;
        }

        foodPanel.SetActive(true);
        foodPanel.transform.SetAsLastSibling();
        UpdateUI();
    }

    public void CloseFoodUpgradePanel()
    {
        if (foodPanel != null)
        {
            foodPanel.SetActive(false);
        }
    }

    public void BuyFoodUpgradeByIndex(int index)
    {
        if (index < 0 || index >= foodUpgradeDatabase.Count)
        {
            return;
        }

        FoodUpgradeData data = foodUpgradeDatabase[index];
        if (!IsFoodUpgradeUnlocked(data) || data.Level > 0)
        {
            return;
        }

        int cost = GetDiscountedUpgradeCost(GetFoodUpgradeCost(data));
        if (coins < cost)
        {
            return;
        }

        coins -= cost;
        data.Level = 1;
        UpdateUI();
        SaveGame();
    }

    public void AddTestWorkshopMaterials()
    {
        paintPieces += 20;
        woodPieces += 20;
        AddInventoryItem(GetWorkshopMaterialInventoryId(WorkshopCraftMaterialType.Fabric), 20);
        AddInventoryItem(GetWorkshopMaterialInventoryId(WorkshopCraftMaterialType.Metal), 20);
        AddInventoryItem(GetWorkshopMaterialInventoryId(WorkshopCraftMaterialType.Stone), 20);
        AddInventoryItem(GetWorkshopMaterialInventoryId(WorkshopCraftMaterialType.Glass), 20);
        AddInventoryItem(GetWorkshopMaterialInventoryId(WorkshopCraftMaterialType.Ceramic), 20);
        lastChestRewardText = "Atölye test malzemeleri eklendi.";
        UpdateUI();
        SaveGame();
    }

    public void ToggleCoinInfoPopup()
    {
        if (coinInfoPopup == null)
        {
            return;
        }

        bool shouldOpen = !coinInfoPopup.activeSelf;
        coinInfoPopup.SetActive(shouldOpen);
        if (shouldOpen)
        {
            coinInfoPopup.transform.SetAsLastSibling();
            UpdateUI();
        }
    }

    public void CloseCoinInfoPopup()
    {
        if (coinInfoPopup != null)
        {
            coinInfoPopup.SetActive(false);
        }
    }

    public void PreviousRoom()
    {
        currentRoomIndex--;
        if (currentRoomIndex < 0)
        {
            currentRoomIndex = RoomCount - 1;
        }

        UpdateUI();
        SaveGame();
    }

    public void NextRoom()
    {
        currentRoomIndex++;
        if (currentRoomIndex >= RoomCount)
        {
            currentRoomIndex = 0;
        }

        UpdateUI();
        SaveGame();
    }

    public void UnlockCurrentRoom()
    {
        if (IsCurrentRoomUnlocked())
        {
            return;
        }

        int cost = GetCurrentRoomUnlockCost();
        if (coins < cost)
        {
            return;
        }

        coins -= cost;
        if (currentRoomIndex == 1)
        {
            productionRoomUnlocked = true;
        }
        else if (currentRoomIndex == 2)
        {
            vipRoomUnlocked = true;
        }

        UpdateUI();
        SaveGame();
    }

    public void ClaimTaskReward()
    {
        if (!firstTaskRewardClaimed && totalCoffeeSold >= 10)
        {
            firstTaskRewardClaimed = true;
            gems += 1;
            AddXp(25);
            seenTaskId = GetCurrentTaskId();
            UpdateUI();
            SaveGame();
            return;
        }

        if (playerLevel >= 2 && !secondTaskRewardClaimed && GetTotalMachineLevel() >= 1)
        {
            secondTaskRewardClaimed = true;
            gems += 2;
            AddXp(50);
            seenTaskId = GetCurrentTaskId();
            UpdateUI();
            SaveGame();
            return;
        }

        if (playerLevel >= 3 && !thirdTaskRewardClaimed && customerSales >= 5)
        {
            thirdTaskRewardClaimed = true;
            gems += 2;
            AddXp(75);
            seenTaskId = GetCurrentTaskId();
            UpdateUI();
            SaveGame();
            return;
        }

        if (playerLevel >= 4 && !fourthTaskRewardClaimed && GetTotalBaristaLevel() >= 1)
        {
            fourthTaskRewardClaimed = true;
            gems += 3;
            AddXp(100);
            seenTaskId = GetCurrentTaskId();
            UpdateUI();
            SaveGame();
            return;
        }

        if (playerLevel >= 5 && !fifthTaskRewardClaimed && vipCustomerSales >= 3)
        {
            fifthTaskRewardClaimed = true;
            gems += 5;
            AddXp(150);
            seenTaskId = GetCurrentTaskId();
            UpdateUI();
            SaveGame();
        }
    }

    private void AddXp(int amount)
    {
        amount = Mathf.CeilToInt(amount * GetVipXpMultiplier() * GetWorkshopMultiplier(WorkshopCafeEffectType.XpIncome));
        xp += amount;
        while (xp >= GetXpNeededForNextLevel())
        {
            xp -= GetXpNeededForNextLevel();
            playerLevel++;
        }
    }

    private int GetXpNeededForNextLevel()
    {
        return 100 + ((playerLevel - 1) * 50);
    }

    private int GetCurrentCustomerCoinReward()
    {
        return Mathf.CeilToInt(coinsPerClick * GetFoodSalesMultiplier() * incomeMultiplier * currentCustomerCoinMultiplier * GetVipCoinMultiplier()
            * GetWorkshopMultiplier(WorkshopCafeEffectType.CoinIncome)
            * GetWorkshopMultiplier(WorkshopCafeEffectType.Tips));
    }

    private int GetCurrentTapPower()
    {
        return Mathf.CeilToInt(coinsPerClick * GetFoodSalesMultiplier() * incomeMultiplier * GetVipCoinMultiplier()
            * GetWorkshopMultiplier(WorkshopCafeEffectType.CoinIncome));
    }

    private float GetCurrentPassiveIncome()
    {
        return coinsPerSecond * GetFoodPassiveMultiplier() * incomeMultiplier * GetVipPassiveIncomeMultiplier()
            * GetWorkshopMultiplier(WorkshopCafeEffectType.PassiveIncome);
    }

    private float GetWorkshopMultiplier(WorkshopCafeEffectType effect)
    {
        return 1f + (GetWorkshopEffectTotal(effect) / 100f);
    }

    private int GetWorkshopEffectTotal(WorkshopCafeEffectType effect)
    {
        int total = 0;
        foreach (KeyValuePair<WorkshopDecorCategory, string> pair in selectedWorkshopProducts)
        {
            WorkshopDecorProduct product = GetWorkshopProduct(pair.Value);
            if (product == null || !unlockedWorkshopProducts.Contains(product.Id)) continue;
            if (product.PrimaryEffect == effect) total += product.PrimaryEffectValue;
            if (product.HasSecondaryEffect && product.SecondaryEffect == effect) total += product.SecondaryEffectValue;
        }
        return total;
    }

    private int GetFoodSalesBonusPercent()
    {
        int total = 0;
        for (int i = 0; i < foodUpgradeDatabase.Count; i++)
        {
            FoodUpgradeData data = foodUpgradeDatabase[i];
            if (data.Level > 0)
            {
                total += data.SalesBonusPercent;
            }
        }

        return total;
    }

    private int GetFoodPassiveBonusPercent()
    {
        int total = 0;
        for (int i = 0; i < foodUpgradeDatabase.Count; i++)
        {
            FoodUpgradeData data = foodUpgradeDatabase[i];
            if (data.Level > 0)
            {
                total += data.PassiveBonusPercent;
            }
        }

        return total;
    }

    private float GetFoodSalesMultiplier()
    {
        return 1f + (GetFoodSalesBonusPercent() / 100f);
    }

    private float GetFoodPassiveMultiplier()
    {
        return 1f + (GetFoodPassiveBonusPercent() / 100f);
    }

    private int GetFoodUpgradeCost(FoodUpgradeData data)
    {
        if (data == null)
        {
            return 0;
        }

        return data.BaseCost;
    }

    private bool IsFoodUpgradeUnlocked(FoodUpgradeData data)
    {
        return data != null && playerLevel >= data.UnlockPlayerLevel;
    }

    private int GetTotalMachineLevel()
    {
        return machineUpgradeLevel + espressoMachineLevel + premiumMachineLevel;
    }

    private int GetTotalBaristaLevel()
    {
        return baristaUpgradeLevel + seniorBaristaLevel + masterBaristaLevel;
    }

    private string GetPassiveIncomeBreakdownText()
    {
        float basicMachineIncome = machineUpgradeLevel * BasicMachineIncomeGain;
        float espressoMachineIncome = espressoMachineLevel * EspressoMachineIncomeGain;
        float premiumMachineIncome = premiumMachineLevel * PremiumMachineIncomeGain;
        float juniorBaristaIncome = baristaUpgradeLevel * JuniorBaristaIncomeGain;
        float seniorBaristaIncome = seniorBaristaLevel * SeniorBaristaIncomeGain;
        float masterBaristaIncome = masterBaristaLevel * MasterBaristaIncomeGain;
        float machineTotal = basicMachineIncome + espressoMachineIncome + premiumMachineIncome;
        float baristaTotal = juniorBaristaIncome + seniorBaristaIncome + masterBaristaIncome;
        int foodBonusPercent = GetFoodPassiveBonusPercent();
        float baseTotal = machineTotal + baristaTotal;
        float foodBoostedTotal = baseTotal * GetFoodPassiveMultiplier();
        float boostedTotal = foodBoostedTotal * incomeMultiplier * GetVipPassiveIncomeMultiplier();

        string text = "Kahve Makineleri: " + machineTotal.ToString("0.#") + "/sn"
            + "\n- Basit Makine: " + basicMachineIncome.ToString("0.#") + "/sn"
            + "\n- Espresso Makinesi: " + espressoMachineIncome.ToString("0.#") + "/sn"
            + "\n- Premium Makine: " + premiumMachineIncome.ToString("0.#") + "/sn"
            + "\n\nBaristalar: " + baristaTotal.ToString("0.#") + "/sn"
            + "\n- Barista: " + juniorBaristaIncome.ToString("0.#") + "/sn"
            + "\n- Usta Barista: " + seniorBaristaIncome.ToString("0.#") + "/sn"
            + "\n- Sef Barista: " + masterBaristaIncome.ToString("0.#") + "/sn"
            + "\n\nGida Urunleri: +%" + foodBonusPercent
            + "\n\nToplam: " + foodBoostedTotal.ToString("0.#") + "/sn";

        if (vipLevel > 0)
        {
            text += "\nVIP bonuslu toplam: " + (foodBoostedTotal * GetVipPassiveIncomeMultiplier()).ToString("0.#") + "/sn";
        }

        if (incomeMultiplier > 1f)
        {
            text += "\n2x aktif toplam: " + boostedTotal.ToString("0.#") + "/sn";
        }

        return text;
    }

    private string[] GetCoinInfoPopupRows()
    {
        string boostText = bonusTimer > 0 ? "Aktif (" + FormatCompactTime(bonusTimer) + ")" : "Kapalı";
        string vipText = vipLevel > 0
            ? "+" + GetVipCoinBonusPercent(vipLevel) + "% coin, +" + GetVipPassiveBonusPercent(vipLevel) + "% pasif"
            : "Yok";

        int foodSalesBonusPercent = GetFoodSalesBonusPercent();
        int foodPassiveBonusPercent = GetFoodPassiveBonusPercent();
        return new[]
        {
            "Kahve Ücreti   +" + GetCurrentTapPower() + " coin",
            "Pasif Gelir   " + GetCurrentPassiveIncome().ToString("0.#") + " / sn",
            "Gıda Etkisi   +%" + foodSalesBonusPercent + " satış, +%" + foodPassiveBonusPercent + " pasif",
            "VIP Etkisi   " + vipText,
            "2x Reklam   " + boostText,
            "Offline Gelir   " + GetOfflinePassiveIncomePerSecond().ToString("0.#") + " / sn  (1/3, maks. 4 sa)"
        };
    }

    private float GetOfflinePassiveIncomePerSecond()
    {
        return coinsPerSecond * GetFoodPassiveMultiplier() * GetVipPassiveIncomeMultiplier() * OfflineIncomeRate;
    }

    private float GetVipCoinMultiplier()
    {
        return GetVipCoinMultiplier(vipLevel);
    }

    private float GetVipCoinMultiplier(int level)
    {
        return 1f + (Mathf.Clamp(level, 0, MaxVipLevel) * 0.04f);
    }

    private float GetVipXpMultiplier()
    {
        return GetVipXpMultiplier(vipLevel);
    }

    private float GetVipXpMultiplier(int level)
    {
        return 1f + (Mathf.Clamp(level, 0, MaxVipLevel) * 0.03f);
    }

    private float GetVipPassiveIncomeMultiplier()
    {
        return GetVipPassiveIncomeMultiplier(vipLevel);
    }

    private float GetVipPassiveIncomeMultiplier(int level)
    {
        return 1f + (Mathf.Clamp(level, 0, MaxVipLevel) * 0.05f);
    }

    private float GetVipCustomerPatienceMultiplier()
    {
        return GetVipCustomerPatienceMultiplier(vipLevel);
    }

    private float GetVipCustomerPatienceMultiplier(int level)
    {
        return 1f + (Mathf.Clamp(level, 0, MaxVipLevel) * 0.04f);
    }

    private int GetVipCoinBonusPercent(int level)
    {
        return Mathf.RoundToInt((GetVipCoinMultiplier(level) - 1f) * 100f);
    }

    private int GetVipXpBonusPercent(int level)
    {
        return Mathf.RoundToInt((GetVipXpMultiplier(level) - 1f) * 100f);
    }

    private int GetVipPassiveBonusPercent(int level)
    {
        return Mathf.RoundToInt((GetVipPassiveIncomeMultiplier(level) - 1f) * 100f);
    }

    private int GetVipPatienceBonusPercent(int level)
    {
        return Mathf.RoundToInt((GetVipCustomerPatienceMultiplier(level) - 1f) * 100f);
    }

    private int GetVipUpgradeDiscountPercent(int level)
    {
        int safeLevel = Mathf.Clamp(level, 0, MaxVipLevel);
        if (safeLevel < 3)
        {
            return 0;
        }

        return (safeLevel - 2) * 5;
    }

    private int GetDiscountedUpgradeCost(int baseCost)
    {
        int discountPercent = GetVipUpgradeDiscountPercent(vipLevel);
        if (discountPercent <= 0)
        {
            return baseCost;
        }

        return Mathf.Max(1, Mathf.CeilToInt(baseCost * (1f - (discountPercent / 100f))));
    }

    private string GetVipDiscountBonusText(int level)
    {
        int discountPercent = GetVipUpgradeDiscountPercent(level);
        return discountPercent > 0 ? "-" + discountPercent + "%" : "Kilitli";
    }

    private bool CanUpgradeVipLevel()
    {
        return vipLevel < MaxVipLevel && gems >= GetVipUpgradeCost();
    }

    private string GetVipBonusValueText(int percent)
    {
        return percent > 0 ? "+" + percent + "%" : "VIP al";
    }

    private int GetVipUpgradeCost()
    {
        switch (vipLevel)
        {
            case 0: return 50;
            case 1: return 125;
            case 2: return 300;
            case 3: return 650;
            case 4: return 1200;
            default: return 0;
        }
    }

    private int GetVipPackageCost(int level)
    {
        switch (level)
        {
            case 1: return 50;
            case 2: return 125;
            case 3: return 300;
            case 4: return 650;
            case 5: return 1200;
            default: return 0;
        }
    }

    private int GetVipPurchaseTargetLevel()
    {
        if (vipLevel >= MaxVipLevel)
        {
            return MaxVipLevel;
        }

        int selectedLevel = selectedVipPreviewLevel > 0
            ? Mathf.Clamp(selectedVipPreviewLevel, 1, MaxVipLevel)
            : 0;

        return selectedLevel > vipLevel ? selectedLevel : vipLevel + 1;
    }

    private int GetVipPurchaseCost()
    {
        return vipLevel >= MaxVipLevel ? 0 : GetVipPackageCost(GetVipPurchaseTargetLevel());
    }

    private string GetVipKitName(int level)
    {
        switch (level)
        {
            case 1: return "Bronz VIP";
            case 2: return "Gümüş VIP";
            case 3: return "Altın VIP";
            case 4: return "Platin VIP";
            case 5: return "Elmas VIP";
            default: return "VIP Yok";
        }
    }

    private string GetVipTierName(int level)
    {
        switch (level)
        {
            case 1: return "Bronz VIP";
            case 2: return "Gümüş VIP";
            case 3: return "Altın VIP";
            case 4: return "Platin VIP";
            case 5: return "Elmas VIP";
            default: return "BASLANGIC";
        }
    }

    private Color GetVipTierColor(int level)
    {
        switch (level)
        {
            case 1: return new Color(0.72f, 0.42f, 0.24f, 0.96f);
            case 2: return new Color(0.72f, 0.76f, 0.82f, 0.96f);
            case 3: return new Color(0.95f, 0.67f, 0.18f, 0.96f);
            case 4: return new Color(0.48f, 0.24f, 0.72f, 0.96f);
            case 5: return new Color(0.28f, 0.74f, 0.92f, 0.96f);
            default: return new Color(0.22f, 0.18f, 0.26f, 0.96f);
        }
    }

    private Color GetVipTierTextColor(int level)
    {
        return level == 2 || level == 3 || level == 5 ? new Color(0.08f, 0.07f, 0.09f, 1f) : Color.white;
    }

    private string GetVipTitleColorHex(int level)
    {
        switch (level)
        {
            case 1: return "#B87333";
            case 2: return "#BFC7D2";
            case 3: return "#F5B52E";
            case 4: return "#D7E5F0";
            case 5: return "#5EDAF2";
            default: return "#3A1C07";
        }
    }

    private string FormatVipPanelTitle(string title, int level)
    {
        return "<size=20><color=" + GetVipTitleColorHex(level) + ">" + title + "</color></size>";
    }

    private string GetVipLevelBonusLine(int level)
    {
        if (level <= 0)
        {
            return "Bonus yok";
        }

        return "+" + GetVipCoinBonusPercent(level) + "% coin"
            + "\n+" + GetVipXpBonusPercent(level) + "% XP"
            + "\n+" + GetVipPassiveBonusPercent(level) + "% pasif"
            + "\n" + (GetVipUpgradeDiscountPercent(level) > 0 ? "-" + GetVipUpgradeDiscountPercent(level) + "% indirim" : "İndirim Kilitli");
    }

    private string GetVipPanelText()
    {
        int previewLevel = selectedVipPreviewLevel > 0 ? Mathf.Clamp(selectedVipPreviewLevel, 1, MaxVipLevel) : 0;
        if (previewLevel > 0)
        {
            return FormatVipPanelTitle(GetVipKitName(previewLevel), previewLevel)
                + (previewLevel > vipLevel ? "\nÜcret: " + GetVipPackageCost(previewLevel) + " Elmas" : "\nAktif Paket");
        }

        if (vipLevel <= 0)
        {
            return FormatVipPanelTitle("VIP YOK", 0)
                + "\nSonraki: " + FormatVipPanelTitle(GetVipKitName(1), 1)
                + "\nÜcret: " + GetVipUpgradeCost() + " Elmas";
        }

        if (vipLevel >= MaxVipLevel)
        {
            return FormatVipPanelTitle(GetVipKitName(vipLevel), vipLevel)
                + "\nVIP MAX";
        }

        return FormatVipPanelTitle(GetVipKitName(vipLevel), vipLevel)
            + "\nSonraki: " + FormatVipPanelTitle(GetVipKitName(vipLevel + 1), vipLevel + 1)
            + "\nÜcret: " + GetVipUpgradeCost() + " Elmas";
    }

    private string GetVipCardText(int level)
    {
        string state = vipLevel >= level ? "ACIK" : "KILIT";
        if (vipLevel == level)
        {
            state = "AKTIF";
        }

        return GetVipTierName(level)
            + "\n" + state;
    }

    public void BuyCoffeeUpgrade()
    {
        int cost = GetDiscountedUpgradeCost(coffeeUpgradeCost);
        if (coffeeUpgradeLevel >= UpgradeMaxLevel || coins < cost) return;

        coins -= cost;
        coffeeUpgradeLevel++;
        coinsPerClick += BasicCoffeeTapGain;
        coffeeUpgradeCost = Mathf.CeilToInt(coffeeUpgradeCost * UpgradeCostGrowth);
        UpdateUI();
        SaveGame();
    }

    public void BuyPremiumCoffee()
    {
        int cost = GetDiscountedUpgradeCost(premiumCoffeeCost);
        if (premiumCoffeeLevel >= UpgradeMaxLevel || playerLevel < PremiumTierUnlockLevel || coffeeUpgradeLevel < NextTierRequiredLevel || coins < cost) return;

        coins -= cost;
        premiumCoffeeLevel++;
        coinsPerClick += PremiumCoffeeTapGain;
        premiumCoffeeCost = Mathf.CeilToInt(premiumCoffeeCost * UpgradeCostGrowth);
        UpdateUI();
        SaveGame();
    }

    public void BuySignatureCoffee()
    {
        int cost = GetDiscountedUpgradeCost(signatureCoffeeCost);
        if (signatureCoffeeLevel >= UpgradeMaxLevel || playerLevel < SignatureTierUnlockLevel || premiumCoffeeLevel < NextTierRequiredLevel || coins < cost) return;

        coins -= cost;
        signatureCoffeeLevel++;
        coinsPerClick += SignatureCoffeeTapGain;
        signatureCoffeeCost = Mathf.CeilToInt(signatureCoffeeCost * UpgradeCostGrowth);
        UpdateUI();
        SaveGame();
    }

    public void BuyMachineUpgrade()
    {
        int cost = GetDiscountedUpgradeCost(machineUpgradeCost);
        if (machineUpgradeLevel >= UpgradeMaxLevel || coins < cost) return;

        coins -= cost;
        machineUpgradeLevel++;
        coinsPerSecond += BasicMachineIncomeGain;
        machineUpgradeCost = Mathf.CeilToInt(machineUpgradeCost * UpgradeCostGrowth);
        UpdateUI();
        SaveGame();
    }

    public void BuyEspressoMachine()
    {
        int cost = GetDiscountedUpgradeCost(espressoMachineCost);
        if (espressoMachineLevel >= UpgradeMaxLevel || playerLevel < PremiumTierUnlockLevel || machineUpgradeLevel < NextTierRequiredLevel || coins < cost) return;

        coins -= cost;
        espressoMachineLevel++;
        coinsPerSecond += EspressoMachineIncomeGain;
        espressoMachineCost = Mathf.CeilToInt(espressoMachineCost * UpgradeCostGrowth);
        UpdateUI();
        SaveGame();
    }

    public void BuyPremiumMachine()
    {
        int cost = GetDiscountedUpgradeCost(premiumMachineCost);
        if (premiumMachineLevel >= UpgradeMaxLevel || playerLevel < SignatureTierUnlockLevel || espressoMachineLevel < NextTierRequiredLevel || coins < cost) return;

        coins -= cost;
        premiumMachineLevel++;
        coinsPerSecond += PremiumMachineIncomeGain;
        premiumMachineCost = Mathf.CeilToInt(premiumMachineCost * UpgradeCostGrowth);
        UpdateUI();
        SaveGame();
    }

    public void BuyBaristaUpgrade()
    {
        int cost = GetDiscountedUpgradeCost(baristaUpgradeCost);
        if (baristaUpgradeLevel >= UpgradeMaxLevel || coins < cost) return;

        coins -= cost;
        baristaUpgradeLevel++;
        coinsPerSecond += JuniorBaristaIncomeGain;
        baristaUpgradeCost = Mathf.CeilToInt(baristaUpgradeCost * UpgradeCostGrowth);
        UpdateUI();
        SaveGame();
    }

    public void BuySeniorBarista()
    {
        int cost = GetDiscountedUpgradeCost(seniorBaristaCost);
        if (seniorBaristaLevel >= UpgradeMaxLevel || playerLevel < SeniorBaristaUnlockLevel || baristaUpgradeLevel < NextTierRequiredLevel || coins < cost) return;

        coins -= cost;
        seniorBaristaLevel++;
        coinsPerSecond += SeniorBaristaIncomeGain;
        seniorBaristaCost = Mathf.CeilToInt(seniorBaristaCost * UpgradeCostGrowth);
        UpdateUI();
        SaveGame();
    }

    public void BuyMasterBarista()
    {
        int cost = GetDiscountedUpgradeCost(masterBaristaCost);
        if (masterBaristaLevel >= UpgradeMaxLevel || playerLevel < MasterBaristaUnlockLevel || seniorBaristaLevel < NextTierRequiredLevel || coins < cost) return;

        coins -= cost;
        masterBaristaLevel++;
        coinsPerSecond += MasterBaristaIncomeGain;
        masterBaristaCost = Mathf.CeilToInt(masterBaristaCost * UpgradeCostGrowth);
        UpdateUI();
        SaveGame();
    }

    private void BuildUI()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }
        else
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        ConfigureCanvasForLandscape(canvas);
        ClearCanvasChildren(canvas.transform);

        if (FindObjectOfType<EventSystem>() == null)
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        ConfigureSaleCoinAudio();

        RectTransform gameFrame = CreateSafeGameFrame(canvas.transform);

        CreateCafeScene(gameFrame);
        CreateBackgroundTapArea(gameFrame);
        CreateCafeObjectHotspots(gameFrame);

        roomNameText = CreateText(gameFrame, "RoomNameText", "", new Vector2(0, -330), new Vector2(520, 42), 24);
        roomNameText.gameObject.SetActive(false);
        CreateButton(gameFrame, "PreviousRoomButton", "<", new Vector2(-790, -95), new Vector2(66, 88), PreviousRoom, new Color(0.25f, 0.32f, 0.38f), 34);
        CreateButton(gameFrame, "NextRoomButton", ">", new Vector2(790, -95), new Vector2(66, 88), NextRoom, new Color(0.25f, 0.32f, 0.38f), 34);
        roomUnlockButtonText = CreateButton(gameFrame, "RoomUnlockButton", "", new Vector2(0, -382), new Vector2(380, 46), UnlockCurrentRoom, new Color(0.22f, 0.48f, 0.40f), 19);

        coinText = CreateResourceBar(gameFrame, "CoinBar", GetCoinIconSprite(), new Vector2(-245, 500), new Vector2(220, 52), new Color(0.28f, 0.15f, 0.05f, 0.93f), new Color(0.96f, 0.69f, 0.19f, 1f), 29, 1f);
        MakeResourceBarClickable(coinText, ToggleCoinInfoPopup);
        CreateCoinInfoPopup(gameFrame, new Vector2(-245, 315));
        activeBoostText = CreateText(gameFrame, "ActiveBoostText", "", new Vector2(0, 375), new Vector2(520, 38), 24);
        activeBoostText.gameObject.SetActive(false);
        customerStatsText = CreateText(gameFrame, "CustomerStatsText", "Satis: 0 | Musteri: 0 | Kacan: 0", new Vector2(0, -470), new Vector2(620, 34), 18);
        customerStatsText.gameObject.SetActive(false);
        gemText = CreateResourceBar(gameFrame, "GemBar", GetGemIconSprite(), new Vector2(0, 500), new Vector2(220, 52), new Color(0.04f, 0.15f, 0.22f, 0.93f), new Color(0.22f, 0.81f, 1f, 1f), 29, 1.18f);
        chestText = CreateText(gameFrame, "ChestText", "", new Vector2(-760, 456), new Vector2(260, 38), 20);
        chestText.gameObject.SetActive(false);
        xpText = CreateXpBar(gameFrame, new Vector2(245, 500), new Vector2(220, 52));
        vipHudText = CreateButton(gameFrame, "VipHudButton", "", new Vector2(-890, -315), new Vector2(132, 132), OpenVipPanel, new Color(1f, 1f, 1f, 0f), 18);
        ConfigureVipHudButton(vipHudText);

        CreateOfflinePanel(canvas.transform);
        CreateDailyRewardPanel(canvas.transform);
        CreateChestPanel(canvas.transform);
        CreateChestRewardPanel(canvas.transform);
        CreateDecorPanel(canvas.transform);
        CreateTestPanel(canvas.transform);
        CreateDailyTasksPanel(canvas.transform);
        CreateTaskPanel(canvas.transform);
        CreateCoffeePanel(canvas.transform);
        CreateMachinePanel(canvas.transform);
        CreateBaristaPanel(canvas.transform);
        CreatePassiveIncomePanel(canvas.transform);
        CreateFoodUpgradePanel(canvas.transform);
        CreateVipPanel(canvas.transform);
        CreateVipInfoPanel(canvas.transform);
        CreateTimedOfferButton(gameFrame);
        CreateTimedOfferPanel(canvas.transform);
        CreateAdBoostConfirmPanel(canvas.transform);
        CreateCustomerButton(gameFrame);

        CreateButton(gameFrame, "SellCoffeeButton", "SAT", new Vector2(0, -475), new Vector2(150, 68), SellCoffee, new Color(0.43f, 0.24f, 0.14f), 28);
        adBoostButtonText = CreateButton(gameFrame, "AdBoostButton", "", new Vector2(850, 250), new Vector2(128, 148), WatchAdForBoost, new Color(1f, 1f, 1f, 0.01f), 20);
        ConfigureAdBoostButton(adBoostButtonText);
        CreateButton(gameFrame, "OpenTestPanelButton", "T", new Vector2(850, -415), new Vector2(82, 74), OpenTestPanel, new Color(0.20f, 0.20f, 0.20f), 26);
        TMP_Text tasksButtonText = CreateButton(gameFrame, "TasksButton", "", new Vector2(-890, 310), new Vector2(122, 116), OpenTaskPanel, Color.white, 24);
        ApplyButtonSprite(tasksButtonText, GetTasksMenuIconSprite());
        taskNotifyDot = CreateNotifyDot(tasksButtonText.transform.parent, new Vector2(51, 47));
        TMP_Text dailyTasksButtonText = CreateButton(gameFrame, "DailyTasksButton", "", new Vector2(-890, 185), new Vector2(122, 116), OpenDailyTasksPanel, Color.white, 24);
        ApplyButtonSprite(dailyTasksButtonText, GetDailyTasksMenuIconSprite());
        dailyTasksNotifyDot = CreateNotifyDot(dailyTasksButtonText.transform.parent, new Vector2(51, 47));
        TMP_Text dailyRewardMenuButtonText = CreateButton(gameFrame, "DailyRewardButton", "", new Vector2(-890, 60), new Vector2(122, 116), OpenDailyRewardPanel, Color.white, 24);
        ApplyButtonSprite(dailyRewardMenuButtonText, GetDailyRewardMenuIconSprite());
        TMP_Text chestsButtonText = CreateButton(gameFrame, "ChestsButton", "", new Vector2(-890, -65), new Vector2(122, 116), OpenChestPanel, Color.white, 24);
        ApplyButtonSprite(chestsButtonText, GetChestButtonSprite());
        TMP_Text decorButtonText = CreateButton(gameFrame, "DecorButton", "", new Vector2(-890, -190), new Vector2(122, 116), OpenDecorPanel, Color.white, 19);
        ApplyButtonSprite(decorButtonText, GetDecorMenuIconSprite());

        coffeeUpgradeButtonText = CreateButton(gameFrame, "CoffeeUpgradeButton", "Kah", new Vector2(-165, -475), new Vector2(126, 68), OpenCoffeePanel, new Color(0.86f, 0.56f, 0.20f), 23);
        coffeeUpgradeButton = coffeeUpgradeButtonText.GetComponentInParent<Button>();
        HideLegacyUpgradeButton(coffeeUpgradeButtonText);

        machineUpgradeButtonText = CreateButton(gameFrame, "MachineUpgradeButton", "Mak", new Vector2(-315, -475), new Vector2(126, 68), OpenMachinePanel, new Color(0.20f, 0.55f, 0.48f), 23);
        machineUpgradeButton = machineUpgradeButtonText.GetComponentInParent<Button>();
        HideLegacyUpgradeButton(machineUpgradeButtonText);

        baristaUpgradeButtonText = CreateButton(gameFrame, "BaristaUpgradeButton", "Bar", new Vector2(165, -475), new Vector2(126, 68), OpenBaristaPanel, new Color(0.54f, 0.32f, 0.72f), 23);
        baristaUpgradeButton = baristaUpgradeButtonText.GetComponentInParent<Button>();
        HideLegacyUpgradeButton(baristaUpgradeButtonText);

        foodUpgradeButtonText = CreateButton(gameFrame, "FoodUpgradeButton", "Gıda", new Vector2(315, -475), new Vector2(126, 68), OpenFoodUpgradePanel, new Color(0.55f, 0.32f, 0.13f), 23);
        foodUpgradeButton = foodUpgradeButtonText.GetComponentInParent<Button>();
        CreateSaleCornerFeedback(gameFrame);
    }

    private void CreateSaleCornerFeedback(RectTransform parent)
    {
        GameObject rootObject = new GameObject("SaleCornerFeedback", typeof(RectTransform), typeof(CanvasGroup), typeof(CanvasRenderer), typeof(Image));
        rootObject.transform.SetParent(parent, false);

        saleCornerFeedbackRoot = rootObject.GetComponent<RectTransform>();
        saleCornerFeedbackRoot.anchorMin = new Vector2(0.5f, 0.5f);
        saleCornerFeedbackRoot.anchorMax = new Vector2(0.5f, 0.5f);
        saleCornerFeedbackRoot.pivot = new Vector2(0.5f, 0.5f);
        saleCornerFeedbackRoot.anchoredPosition = new Vector2(720f, 360f);
        saleCornerFeedbackRoot.sizeDelta = new Vector2(176f, 46f);

        Image background = rootObject.GetComponent<Image>();
        background.sprite = CreateRoundedRectSprite(
            saleCornerFeedbackRoot.sizeDelta,
            new Color(0.18f, 0.075f, 0.015f, 0.32f),
            new Color(1f, 0.72f, 0.22f, 0.42f),
            23,
            1);
        background.color = Color.white;
        background.raycastTarget = false;

        saleCornerFeedbackGroup = rootObject.GetComponent<CanvasGroup>();
        saleCornerFeedbackGroup.alpha = 0f;
        saleCornerFeedbackGroup.blocksRaycasts = false;
        saleCornerFeedbackGroup.interactable = false;

        GameObject iconObject = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        iconObject.transform.SetParent(rootObject.transform, false);
        RectTransform iconRect = iconObject.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.5f, 0.5f);
        iconRect.anchorMax = new Vector2(0.5f, 0.5f);
        iconRect.pivot = new Vector2(0.5f, 0.5f);
        iconRect.anchoredPosition = new Vector2(-54f, 0f);
        iconRect.sizeDelta = new Vector2(38f, 38f);
        saleCornerFeedbackIcon = iconObject.GetComponent<Image>();
        saleCornerFeedbackIcon.sprite = GetSaleFeedbackCoffeeSprite();
        saleCornerFeedbackIcon.preserveAspect = true;
        saleCornerFeedbackIcon.raycastTarget = false;

        saleCornerFeedbackText = CreateText(rootObject.transform, "Text", "SELL", new Vector2(30f, 0f), new Vector2(112f, 40f), 26f);
        saleCornerFeedbackText.color = new Color(1f, 0.86f, 0.42f, 1f);
        saleCornerFeedbackText.fontStyle = FontStyles.Bold;
        saleCornerFeedbackText.raycastTarget = false;

        Outline outline = saleCornerFeedbackText.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0.20f, 0.075f, 0.005f, 0.88f);
        outline.effectDistance = new Vector2(1.3f, -1.3f);

        rootObject.SetActive(false);
    }

    private void CreateSaleFeedbackLayer(Transform parent)
    {
        GameObject previousLayer = GameObject.Find("SaleFeedbackOverlayCanvas");
        if (previousLayer != null)
        {
            Destroy(previousLayer);
        }

        GameObject layerObject = new GameObject("SaleFeedbackOverlayCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));

        saleFeedbackLayer = layerObject.GetComponent<RectTransform>();
        saleFeedbackLayer.anchorMin = Vector2.zero;
        saleFeedbackLayer.anchorMax = Vector2.one;
        saleFeedbackLayer.pivot = new Vector2(0.5f, 0.5f);
        saleFeedbackLayer.anchoredPosition = Vector2.zero;
        saleFeedbackLayer.offsetMin = Vector2.zero;
        saleFeedbackLayer.offsetMax = Vector2.zero;
        saleFeedbackLayer.sizeDelta = new Vector2(1920f, 1080f);
        saleFeedbackLayer.SetAsLastSibling();

        Canvas layerCanvas = layerObject.GetComponent<Canvas>();
        layerCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        layerCanvas.overrideSorting = true;
        layerCanvas.sortingOrder = 30000;

        CanvasScaler scaler = layerObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 1f;
    }

    private void ShowSaleFeedbackAtPointer()
    {
        if (saleFeedbackLayer == null)
        {
            CreateSaleFeedbackLayer(null);
        }
        if (saleFeedbackLayer == null) return;

        saleFeedbackLayer.SetAsLastSibling();
        while (saleFeedbackLayer.childCount > 7)
        {
            Destroy(saleFeedbackLayer.GetChild(0).gameObject);
        }

        Vector2 pointer = Input.mousePosition;
        float screenWidth = Mathf.Max(1f, Screen.width);
        float screenHeight = Mathf.Max(1f, Screen.height);
        Vector2 localPosition = new Vector2(
            ((pointer.x / screenWidth) - 0.5f) * 1920f,
            ((pointer.y / screenHeight) - 0.5f) * 1080f
        );

        StartCoroutine(PlaySaleFeedback(Vector2.zero));
    }

    private IEnumerator PlaySaleFeedback(Vector2 startPosition)
    {
        GameObject rootObject = new GameObject("SaleCoffeeFeedback", typeof(RectTransform), typeof(CanvasGroup));
        rootObject.transform.SetParent(saleFeedbackLayer, false);

        RectTransform rootRect = rootObject.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.5f, 0.5f);
        rootRect.anchorMax = new Vector2(0.5f, 0.5f);
        rootRect.pivot = new Vector2(0.5f, 0.5f);
        rootRect.anchoredPosition = startPosition;
        rootRect.sizeDelta = new Vector2(260f, 110f);

        CanvasGroup group = rootObject.GetComponent<CanvasGroup>();
        group.alpha = 1f;
        group.blocksRaycasts = false;
        group.interactable = false;

        GameObject pillObject = new GameObject("FeedbackPill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        pillObject.transform.SetParent(rootObject.transform, false);
        RectTransform pillRect = pillObject.GetComponent<RectTransform>();
        pillRect.anchorMin = new Vector2(0.5f, 0.5f);
        pillRect.anchorMax = new Vector2(0.5f, 0.5f);
        pillRect.pivot = new Vector2(0.5f, 0.5f);
        pillRect.anchoredPosition = Vector2.zero;
        pillRect.sizeDelta = new Vector2(250f, 92f);
        Image pillImage = pillObject.GetComponent<Image>();
        pillImage.color = new Color(0.55f, 0.25f, 0.04f, 0.86f);
        pillImage.raycastTarget = false;

        GameObject iconObject = new GameObject("CoffeeIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        iconObject.transform.SetParent(rootObject.transform, false);
        RectTransform iconRect = iconObject.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.5f, 0.5f);
        iconRect.anchorMax = new Vector2(0.5f, 0.5f);
        iconRect.pivot = new Vector2(0.5f, 0.5f);
        iconRect.anchoredPosition = new Vector2(-62f, 0f);
        iconRect.sizeDelta = new Vector2(88f, 88f);
        Image iconImage = iconObject.GetComponent<Image>();
        iconImage.sprite = GetSaleFeedbackCoffeeSprite();
        iconImage.preserveAspect = true;
        iconImage.raycastTarget = false;

        GameObject textObject = new GameObject("SellText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(rootObject.transform, false);
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = new Vector2(50f, 2f);
        textRect.sizeDelta = new Vector2(166f, 70f);

        TMP_Text text = textObject.GetComponent<TMP_Text>();
        text.text = "SELL";
        text.fontSize = 46f;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.MidlineLeft;
        text.color = new Color(1f, 0.91f, 0.58f, 1f);
        text.raycastTarget = false;

        Outline textOutline = textObject.AddComponent<Outline>();
        textOutline.effectColor = new Color(0.28f, 0.11f, 0.02f, 0.92f);
        textOutline.effectDistance = new Vector2(2f, -2f);
        Shadow textShadow = textObject.AddComponent<Shadow>();
        textShadow.effectColor = new Color(0f, 0f, 0f, 0.34f);
        textShadow.effectDistance = new Vector2(0f, -3f);

        const float duration = 1.15f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (rootRect == null)
            {
                yield break;
            }

            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float pop = Mathf.Sin(Mathf.Clamp01(t / 0.2f) * Mathf.PI) * 0.18f;
            float drift = EaseOutCubic(t) * 86f;
            float fade = t < 0.56f ? 1f : 1f - Mathf.Clamp01((t - 0.56f) / 0.44f);

            rootRect.anchoredPosition = startPosition + new Vector2(8f * Mathf.Sin(t * Mathf.PI), drift);
            rootRect.localScale = Vector3.one * (0.82f + (0.18f * EaseOutBack(Mathf.Clamp01(t / 0.24f))) + pop);
            group.alpha = fade;

            yield return null;
        }

        if (rootObject != null)
        {
            Destroy(rootObject);
        }
    }

    private Sprite GetSaleFeedbackCoffeeSprite()
    {
        if (saleFeedbackCoffeeSprite == null)
        {
            saleFeedbackCoffeeSprite = LoadSpriteFromResources("Art/UI/Food/food_icon_coffee");
            if (saleFeedbackCoffeeSprite == null)
            {
                saleFeedbackCoffeeSprite = GetCoinIconSprite();
            }
        }

        return saleFeedbackCoffeeSprite;
    }

    private RectTransform CreateSafeGameFrame(Transform canvasTransform)
    {
        GameObject safeAreaObject = new GameObject("SafeGameArea", typeof(RectTransform));
        safeAreaObject.transform.SetParent(canvasTransform, false);

        RectTransform safeAreaRect = safeAreaObject.GetComponent<RectTransform>();
        ApplySafeAreaAnchors(safeAreaRect);

        GameObject frameObject = new GameObject("GameFrameResponsive", typeof(RectTransform));
        frameObject.transform.SetParent(safeAreaObject.transform, false);

        RectTransform frameRect = frameObject.GetComponent<RectTransform>();
        frameRect.anchorMin = Vector2.zero;
        frameRect.anchorMax = Vector2.one;
        frameRect.pivot = new Vector2(0.5f, 0.5f);
        frameRect.anchoredPosition = Vector2.zero;
        frameRect.offsetMin = Vector2.zero;
        frameRect.offsetMax = Vector2.zero;

        return frameRect;
    }

    private void ApplySafeAreaAnchors(RectTransform rect)
    {
        if (Screen.width <= 0 || Screen.height <= 0)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return;
        }

        Rect safeArea = Screen.safeArea;
        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;
        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private void ConfigureCanvasForLandscape(Canvas canvas)
    {
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = canvas.gameObject.AddComponent<CanvasScaler>();
        }

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 1f;
    }

    private void ClearCanvasChildren(Transform canvasTransform)
    {
        for (int i = canvasTransform.childCount - 1; i >= 0; i--)
        {
            Transform child = canvasTransform.GetChild(i);
            if (child.GetComponent<GameManager>() != null)
            {
                continue;
            }

            Destroy(child.gameObject);
        }
    }

    public void CloseOfflinePanel()
    {
        CollectOfflineCoins(1);
    }

    public void CollectOfflineDouble()
    {
        CollectOfflineCoins(2);
    }

    private void CollectOfflineCoins(int multiplier)
    {
        if (lastOfflineCoins > 0)
        {
            coins += lastOfflineCoins * multiplier;
            lastOfflineCoins = 0;
            PlayerPrefs.SetInt("lastOfflineCoins", 0);
            UpdateUI();
        }
        if (offlinePanel != null)
        {
            offlinePanel.SetActive(false);
        }

        SaveGame();
    }

    private void CreateOfflinePanel(Transform parent)
    {
        offlinePanel = new GameObject("OfflinePanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        offlinePanel.transform.SetParent(parent, false);

        RectTransform panelRect = offlinePanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = new Vector2(0, 35);
        panelRect.sizeDelta = new Vector2(900, 450);

        Image panelImage = offlinePanel.GetComponent<Image>();
        panelImage.sprite = GetCoinInfoPanelSprite();
        panelImage.color = Color.white;
        panelImage.preserveAspect = true;

        TMP_Text title = CreateText(offlinePanel.transform, "OfflineTitle", "BARİSTA ÇALIŞTI", new Vector2(-10, 130), new Vector2(700, 54), 34);
        title.color = new Color(1f, 0.79f, 0.28f, 1f);
        title.fontStyle = FontStyles.Bold;
        title.alignment = TextAlignmentOptions.Center;
        title.outlineColor = new Color(0.12f, 0.04f, 0.01f, 1f);
        title.outlineWidth = 0.14f;
        ApplyVipFont(title);

        offlineAmountText = CreateText(offlinePanel.transform, "OfflineAmountText", "", new Vector2(0, 70), new Vector2(700, 62), 42);
        offlineAmountText.color = new Color(1f, 0.86f, 0.42f, 1f);
        offlineAmountText.fontStyle = FontStyles.Bold;
        offlineAmountText.alignment = TextAlignmentOptions.Center;
        offlineAmountText.outlineColor = new Color(0.12f, 0.04f, 0.01f, 1f);
        offlineAmountText.outlineWidth = 0.14f;
        ApplyVipFont(offlineAmountText);

        offlineText = CreateText(offlinePanel.transform, "OfflineText", "", new Vector2(0, 5), new Vector2(700, 72), 24);
        offlineText.color = new Color(1f, 0.91f, 0.68f, 1f);
        offlineText.fontStyle = FontStyles.Bold;
        offlineText.alignment = TextAlignmentOptions.Center;
        offlineText.enableAutoSizing = true;
        offlineText.fontSizeMin = 18;
        offlineText.fontSizeMax = 24;
        ApplyVipFont(offlineText);

        StyleCoffeePopupButton(CreateButton(offlinePanel.transform, "OfflineCollectButton", "TOPLA", new Vector2(-183, -96), new Vector2(300, 72), CloseOfflinePanel, Color.white, 29), new Color(0.27f, 0.10f, 0.025f, 0.98f), false);
        StyleCoffeePopupButton(CreateButton(offlinePanel.transform, "OfflineDoubleButton", "2x TOPLA", new Vector2(141, -96), new Vector2(300, 72), CollectOfflineDouble, Color.white, 29), new Color(0.72f, 0.38f, 0.07f, 0.98f), true);
        offlinePanel.transform.SetAsLastSibling();
        offlinePanel.SetActive(false);
    }

    private TMP_Text StyleCoffeePopupButton(TMP_Text labelText, Color fillColor, bool darkText)
    {
        Image image = labelText.GetComponentInParent<Image>();
        RectTransform rect = labelText.GetComponentInParent<RectTransform>();
        if (image != null && rect != null)
        {
            image.sprite = CreateRoundedRectSprite(rect.sizeDelta, fillColor, new Color(0.96f, 0.68f, 0.20f, 1f), 18, 3);
            image.color = Color.white;
        }

        labelText.color = darkText ? new Color(0.16f, 0.055f, 0.01f, 1f) : new Color(1f, 0.91f, 0.67f, 1f);
        labelText.fontStyle = FontStyles.Bold;
        labelText.enableAutoSizing = true;
        labelText.fontSizeMin = 18;
        labelText.fontSizeMax = 29;
        labelText.outlineColor = darkText ? new Color(1f, 0.78f, 0.32f, 0.42f) : new Color(0f, 0f, 0f, 0.72f);
        labelText.outlineWidth = 0.10f;
        ApplyVipFont(labelText);
        return labelText;
    }

    public void OpenDailyRewardPanel()
    {
        if (dailyRewardPanel == null)
        {
            return;
        }

        if (dailyRewardDimmer != null)
        {
            dailyRewardDimmer.SetActive(true);
            dailyRewardDimmer.transform.SetAsLastSibling();
        }
        dailyRewardPanel.SetActive(true);
        dailyRewardPanel.transform.SetAsLastSibling();
        UpdateUI();
    }

    public void CloseDailyRewardPanel()
    {
        if (dailyRewardPanel != null)
        {
            dailyRewardPanel.SetActive(false);
        }
        if (dailyRewardDimmer != null)
        {
            dailyRewardDimmer.SetActive(false);
        }
    }

    public void ClaimDailyReward()
    {
        if (!CanClaimDailyReward())
        {
            return;
        }

        int gemReward = GetDailyGemReward(dailyRewardDay);
        int commonChestReward = GetDailyCommonChestReward(dailyRewardDay);
        int rareChestReward = GetDailyRareChestReward(dailyRewardDay);
        int luxuryChestReward = GetDailyLuxuryChestReward(dailyRewardDay);

        gems += gemReward;
        chests += commonChestReward;
        rareChests += rareChestReward;
        luxuryChests += luxuryChestReward;
        string rewardText = "Günlük giriş ödülü: " + GetDailyRewardLine(dailyRewardDay);
        lastChestRewardText = rewardText;
        lastDailyRewardUtc = DateTime.UtcNow.Ticks.ToString();
        dailyRewardDay = (dailyRewardDay + 1) % DailyRewardCycleDays;

        if (dailyRewardPanel != null)
        {
            dailyRewardPanel.SetActive(false);
        }
        if (dailyRewardDimmer != null)
        {
            dailyRewardDimmer.SetActive(false);
        }

        ShowChestReward(rewardText, GetGemIconSprite());
        UpdateUI();
        SaveGame();
    }

    private void ShowDailyRewardIfAvailable()
    {
        if (CanClaimDailyReward())
        {
            OpenDailyRewardPanel();
        }
    }

    private void CreateDailyRewardPanel(Transform parent)
    {
        dailyRewardDimmer = new GameObject("DailyRewardDimmer", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        dailyRewardDimmer.transform.SetParent(parent, false);
        RectTransform dimmerRect = dailyRewardDimmer.GetComponent<RectTransform>();
        dimmerRect.anchorMin = Vector2.zero;
        dimmerRect.anchorMax = Vector2.one;
        dimmerRect.offsetMin = Vector2.zero;
        dimmerRect.offsetMax = Vector2.zero;
        Image dimmerImage = dailyRewardDimmer.GetComponent<Image>();
        dimmerImage.color = new Color(0.03f, 0.02f, 0.015f, 0.42f);
        dimmerImage.raycastTarget = true;
        Button dimmerButton = dailyRewardDimmer.AddComponent<Button>();
        dimmerButton.transition = Selectable.Transition.None;
        dailyRewardDimmer.SetActive(false);

        dailyRewardPanel = new GameObject("DailyRewardPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        dailyRewardPanel.transform.SetParent(parent, false);

        RectTransform panelRect = dailyRewardPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = new Vector2(0f, -12f);
        panelRect.sizeDelta = new Vector2(1260f, 691f);

        Image dailyRewardBackground = dailyRewardPanel.GetComponent<Image>();
        dailyRewardBackground.sprite = LoadSpriteFromResources("Art/UI/DailyReward/ScratchDesign/daily_login_scratch_icon_panel_v1_alpha");
        dailyRewardBackground.color = Color.white;
        dailyRewardBackground.preserveAspect = true;

        TMP_Text title = CreateText(dailyRewardPanel.transform, "DailyRewardTitle", "GÜNLÜK GİRİŞ", new Vector2(0f, 258f), new Vector2(500f, 56f), 36);
        StylePopupTitle(title, 36);
        title.color = new Color(1f, 0.86f, 0.48f, 1f);
        title.outlineColor = new Color(0.16f, 0.06f, 0.01f, 0.92f);
        title.outlineWidth = 0.20f;
        title.gameObject.SetActive(false);

        dailyRewardPanelText = CreateText(dailyRewardPanel.transform, "DailyRewardText", "", new Vector2(0, 65), new Vector2(790, 330), 28);
        StylePopupBodyText(dailyRewardPanelText, 19, 27, TextAlignmentOptions.TopLeft);
        dailyRewardPanelText.gameObject.SetActive(false);
        CreateDailyRewardLiveOverlay();

        dailyRewardButtonText = StylePopupButton(CreateButton(dailyRewardPanel.transform, "DailyRewardClaimButton", "", DailyRewardLayoutPosition(new Vector2(504f, -344f)), DailyRewardLayoutSize(new Vector2(390f, 118f)), ClaimDailyReward, new Color(0.84f, 0.38f, 0.22f), 30), new Color(0.84f, 0.38f, 0.22f), 30);
        StylePopupButton(CreateButton(dailyRewardPanel.transform, "DailyRewardCloseButton", "", DailyRewardLayoutPosition(new Vector2(744f, 350f)), DailyRewardLayoutSize(new Vector2(112f, 112f)), CloseDailyRewardPanel, new Color(0.20f, 0.20f, 0.20f), 28), new Color(0.20f, 0.20f, 0.20f), 28);

        ConfigureDailyRewardTransparentButton(dailyRewardButtonText, DailyRewardLayoutPosition(new Vector2(504f, -344f)), DailyRewardLayoutSize(new Vector2(390f, 118f)));
        Transform closeButtonTransform = dailyRewardPanel.transform.Find("DailyRewardCloseButton");
        if (closeButtonTransform != null)
        {
            TMP_Text closeButtonText = closeButtonTransform.GetComponentInChildren<TMP_Text>();
            ConfigureDailyRewardTransparentButton(closeButtonText, DailyRewardLayoutPosition(new Vector2(744f, 350f)), DailyRewardLayoutSize(new Vector2(112f, 112f)));
        }

        dailyRewardPanel.transform.SetAsLastSibling();
        dailyRewardPanel.SetActive(false);
    }

    private void CreateDailyRewardLiveOverlay()
    {
        dailyRewardDayNumberTexts.Clear();
        dailyRewardDayHighlightImages.Clear();
        dailyRewardStepBadgeImages.Clear();
        dailyRewardStepStatusTexts.Clear();
        dailyRewardDayNumberImages.Clear();
        dailyRewardStepStatusImages.Clear();
        dailyRewardStepStatusReferencePositions.Clear();

        dailyRewardStatusClaimedSprite = LoadDailyRewardTextSprite("daily_v2_text_status_claimed");
        dailyRewardStatusAvailableSprite = LoadDailyRewardTextSprite("daily_v2_text_status_available");
        dailyRewardStatusLockedSprite = LoadDailyRewardTextSprite("daily_v2_text_status_locked");
        dailyRewardStatusLockedLongSprite = LoadDailyRewardTextSprite("daily_v2_text_status_locked_long");
        dailyRewardClaimSprite = LoadDailyRewardTextSprite("daily_v2_text_claim");

        CreateDailyRewardSpriteOverlay("DailyRewardTitleTextPng", "daily_v2_text_title", new Vector2(16f, 360f), new Vector2(540f, 82f));
        CreateDailyRewardSpriteOverlay("DailyRewardTodayLabelPng", "daily_v2_text_today", new Vector2(-642f, 252f), new Vector2(178f, 46f));
        dailyRewardClaimTextImage = CreateDailyRewardSpriteOverlay("DailyRewardClaimTextPng", "daily_v2_text_claim", new Vector2(504f, -346f), new Vector2(350f, 80f));

        Vector2[] pngDayNumberPositions =
        {
            new Vector2(-346f, 128f),
            new Vector2(-196f, 128f),
            new Vector2(-46f, 128f),
            new Vector2(104f, 128f),
            new Vector2(254f, 128f),
            new Vector2(404f, 128f),
            new Vector2(632f, 136f)
        };

        for (int i = 0; i < DailyRewardCycleDays; i++)
        {
            Image numberImage = CreateDailyRewardSpriteOverlay("DailyRewardDayNumberPng_" + (i + 1), "daily_v2_text_day_" + (i + 1), pngDayNumberPositions[i], new Vector2(58f, 58f));
            dailyRewardDayNumberImages.Add(numberImage);
        }

        Vector2[] pngStatusPositions =
        {
            new Vector2(-342.9f, -157.7f),
            new Vector2(-198.1f, -157.7f),
            new Vector2(-54.1f, -157.7f),
            new Vector2(89.9f, -157.7f),
            new Vector2(232.9f, -157.7f),
            new Vector2(377.9f, -157.7f),
            new Vector2(585.7f, -184.1f)
        };

        for (int i = 0; i < DailyRewardCycleDays; i++)
        {
            Vector2 statusSize = i == DailyRewardCycleDays - 1 ? new Vector2(150f, 40f) : new Vector2(112f, 34f);
            Image statusImage = CreateDailyRewardSpriteOverlay("DailyRewardStatusTextPng_" + (i + 1), i == DailyRewardCycleDays - 1 ? "daily_v2_text_status_locked_long" : "daily_v2_text_status_locked", pngStatusPositions[i], statusSize);
            dailyRewardStepStatusImages.Add(statusImage);
            dailyRewardStepStatusReferencePositions.Add(pngStatusPositions[i]);
        }

        dailyRewardTodayAmountText = null;
        dailyRewardClaimStateText = null;
        dailyRewardTimerText = null;
        if (dailyRewardPanel != null) return;

        dailyRewardTodayLabelText = CreateText(dailyRewardPanel.transform, "DailyRewardTodayLabel", "BUGÜN", new Vector2(-468f, 153f), new Vector2(160f, 38f), 25);
        dailyRewardTodayLabelText.color = new Color(1f, 0.92f, 0.66f, 1f);
        dailyRewardTodayLabelText.fontStyle = FontStyles.Bold;
        dailyRewardTodayLabelText.enableAutoSizing = true;
        dailyRewardTodayLabelText.fontSizeMin = 18;
        dailyRewardTodayLabelText.fontSizeMax = 25;
        dailyRewardTodayLabelText.margin = Vector4.zero;
        dailyRewardTodayLabelText.outlineColor = new Color(0.10f, 0.04f, 0.01f, 0.90f);
        dailyRewardTodayLabelText.outlineWidth = 0.18f;
        ApplyVipFont(dailyRewardTodayLabelText);
        dailyRewardTodayLabelText.raycastTarget = false;

        Vector2[] dayNumberPositions =
        {
            new Vector2(-258f, 98f),
            new Vector2(-145f, 98f),
            new Vector2(-32f, 98f),
            new Vector2(81f, 98f),
            new Vector2(195f, 98f),
            new Vector2(306f, 98f),
            new Vector2(435f, 117f)
        };

        for (int i = 0; i < DailyRewardCycleDays; i++)
        {
            TMP_Text number = CreateText(dailyRewardPanel.transform, "DailyRewardDayNumber_" + i, (i + 1).ToString(), dayNumberPositions[i], new Vector2(48f, 46f), 27);
            number.color = new Color(1f, 0.87f, 0.48f, 1f);
            number.fontStyle = FontStyles.Bold;
            number.enableAutoSizing = true;
            number.fontSizeMin = 18;
            number.fontSizeMax = i == 6 ? 30 : 27;
            number.margin = Vector4.zero;
            number.outlineColor = new Color(0.10f, 0.04f, 0.01f, 0.92f);
            number.outlineWidth = 0.18f;
            ApplyVipFont(number);
            number.raycastTarget = false;
            dailyRewardDayNumberTexts.Add(number);
        }

        Vector2[] statusPositions =
        {
            new Vector2(-258f, -76f),
            new Vector2(-145f, -76f),
            new Vector2(-32f, -76f),
            new Vector2(81f, -76f),
            new Vector2(195f, -76f),
            new Vector2(306f, -76f),
            new Vector2(435f, -109f)
        };

        for (int i = 0; i < DailyRewardCycleDays; i++)
        {
            Vector2 badgeSize = i == 6 ? new Vector2(166f, 38f) : new Vector2(92f, 31f);
            Image badge = CreateDailyRewardBadge("DailyRewardStepBadge_" + i, statusPositions[i], badgeSize, new Color(0.13f, 0.38f, 0.25f, 0.96f));
            badge.gameObject.SetActive(false);
            TMP_Text status = CreateText(dailyRewardPanel.transform, "DailyRewardStepStatus_" + i, "", statusPositions[i], badgeSize + new Vector2(8f, 6f), 18);
            status.gameObject.SetActive(true);
            status.color = new Color(1f, 0.92f, 0.64f, 1f);
            status.fontStyle = FontStyles.Bold;
            status.alignment = TextAlignmentOptions.Center;
            status.enableAutoSizing = true;
            status.fontSizeMin = 13;
            status.fontSizeMax = i == 6 ? 19 : 18;
            status.margin = Vector4.zero;
            status.outlineColor = new Color(0.12f, 0.04f, 0.01f, 0.82f);
            status.outlineWidth = 0.22f;
            ApplyVipFont(status);
            status.raycastTarget = false;

            dailyRewardStepBadgeImages.Add(badge);
            dailyRewardStepStatusTexts.Add(status);
        }

        CreateDailyRewardBadge("DailyRewardTodayAmountBadge", new Vector2(-472f, -183f), new Vector2(196f, 48f), new Color(0.18f, 0.10f, 0.04f, 0.92f));
        dailyRewardTodayAmountText = CreateText(dailyRewardPanel.transform, "DailyRewardTodayAmount", "", new Vector2(-472f, -183f), new Vector2(186f, 42f), 18);
        dailyRewardTodayAmountText.color = new Color(1f, 0.90f, 0.58f, 1f);
        dailyRewardTodayAmountText.fontStyle = FontStyles.Bold;
        dailyRewardTodayAmountText.enableAutoSizing = true;
        dailyRewardTodayAmountText.fontSizeMin = 10;
        dailyRewardTodayAmountText.fontSizeMax = 18;
        dailyRewardTodayAmountText.margin = new Vector4(5f, 0f, 5f, 0f);
        dailyRewardTodayAmountText.outlineColor = new Color(0.12f, 0.04f, 0.01f, 0.82f);
        dailyRewardTodayAmountText.outlineWidth = 0.16f;
        ApplyVipFont(dailyRewardTodayAmountText);
        dailyRewardTodayAmountText.raycastTarget = false;

        dailyRewardClaimStateText = CreateText(dailyRewardPanel.transform, "DailyRewardClaimState", "", new Vector2(390f, -255f), new Vector2(330f, 66f), 32);
        dailyRewardClaimStateText.color = new Color(1f, 0.86f, 0.48f, 1f);
        dailyRewardClaimStateText.fontStyle = FontStyles.Bold;
        dailyRewardClaimStateText.enableAutoSizing = true;
        dailyRewardClaimStateText.fontSizeMin = 20;
        dailyRewardClaimStateText.fontSizeMax = 34;
        dailyRewardClaimStateText.margin = Vector4.zero;
        dailyRewardClaimStateText.outlineColor = new Color(0.08f, 0.03f, 0.01f, 0.90f);
        dailyRewardClaimStateText.outlineWidth = 0.16f;
        ApplyVipFont(dailyRewardClaimStateText);
        dailyRewardClaimStateText.raycastTarget = false;
        dailyRewardClaimStateText.gameObject.SetActive(true);

        dailyRewardTimerText = CreateText(dailyRewardPanel.transform, "DailyRewardTimer", "", new Vector2(92f, -365f), new Vector2(360f, 34f), 18);
        dailyRewardTimerText.color = new Color(0.36f, 0.18f, 0.05f, 1f);
        dailyRewardTimerText.fontStyle = FontStyles.Bold;
        dailyRewardTimerText.enableAutoSizing = true;
        dailyRewardTimerText.fontSizeMin = 12;
        dailyRewardTimerText.fontSizeMax = 18;
        dailyRewardTimerText.margin = Vector4.zero;
        ApplyVipFont(dailyRewardTimerText);
        dailyRewardTimerText.raycastTarget = false;
        dailyRewardTimerText.gameObject.SetActive(false);
    }

    private Vector2 DailyRewardLayoutPosition(Vector2 referencePosition)
    {
        return referencePosition * DailyRewardLayoutScale;
    }

    private Vector2 DailyRewardLayoutSize(Vector2 referenceSize)
    {
        return referenceSize * DailyRewardLayoutScale;
    }

    private Sprite LoadDailyRewardTextSprite(string spriteName)
    {
        return LoadSpriteFromResources(DailyRewardTextSpriteRoot + spriteName);
    }

    private Image CreateDailyRewardSpriteOverlay(string objectName, string spriteName, Vector2 referencePosition, Vector2 referenceSize)
    {
        GameObject overlayObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        overlayObject.transform.SetParent(dailyRewardPanel.transform, false);

        RectTransform rect = overlayObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = DailyRewardLayoutPosition(referencePosition);
        rect.sizeDelta = DailyRewardLayoutSize(referenceSize);

        Image image = overlayObject.GetComponent<Image>();
        image.sprite = LoadDailyRewardTextSprite(spriteName);
        image.color = Color.white;
        image.preserveAspect = true;
        image.raycastTarget = false;
        return image;
    }

    private Image CreateDailyRewardBadge(string objectName, Vector2 position, Vector2 size, Color color)
    {
        GameObject badgeObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        badgeObject.transform.SetParent(dailyRewardPanel.transform, false);

        RectTransform rect = badgeObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image image = badgeObject.GetComponent<Image>();
        image.sprite = CreateRoundedRectSprite(size, color, new Color(0.96f, 0.70f, 0.24f, 1f), 12, 2);
        image.color = Color.white;
        image.raycastTarget = false;
        return image;
    }

    private void ConfigureDailyRewardTransparentButton(TMP_Text buttonText, Vector2 position, Vector2 size)
    {
        if (buttonText == null) return;
        Button button = buttonText.GetComponentInParent<Button>();
        if (button == null) return;
        RectTransform buttonRect = button.GetComponent<RectTransform>();
        if (buttonRect != null)
        {
            buttonRect.anchoredPosition = position;
            buttonRect.sizeDelta = size;
        }
        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = new Color(1f, 1f, 1f, 0.01f);
        }
        buttonText.text = "";
        buttonText.color = new Color(1f, 1f, 1f, 0f);
    }

    public void OpenChestPanel()
    {
        if (chestPanel == null)
        {
            return;
        }

        chestPanel.SetActive(true);
        chestPanel.transform.SetAsLastSibling();
        UpdateUI();
    }

    public void CloseChestPanel()
    {
        if (chestPanel != null)
        {
            chestPanel.SetActive(false);
        }
    }

    private void InitializeItemDatabase()
    {
        itemDatabase.Clear();
        decorItemDatabase.Clear();

        AddItemData("paint_can_white_001", "Beyaz Boya Kutusu", ItemCategory.Material, ItemRarity.Common, new Color(0.96f, 0.94f, 0.88f));
        AddItemData("paint_can_cream_001", "Krem Boya Kutusu", ItemCategory.Material, ItemRarity.Common, new Color(0.94f, 0.82f, 0.56f));
        AddItemData("paint_can_brown_001", "Kahverengi Boya Kutusu", ItemCategory.Material, ItemRarity.Common, new Color(0.45f, 0.25f, 0.13f));
        AddItemData("paint_can_light_blue_001", "Acik Mavi Boya Kutusu", ItemCategory.Material, ItemRarity.Common, new Color(0.45f, 0.78f, 0.96f));
        AddItemData("paint_can_light_green_001", "Acik Yesil Boya Kutusu", ItemCategory.Material, ItemRarity.Common, new Color(0.54f, 0.86f, 0.56f));

        AddItemData("paint_can_pink_001", "Pembe Boya Kutusu", ItemCategory.Material, ItemRarity.Rare, new Color(0.95f, 0.36f, 0.67f));
        AddItemData("paint_can_red_001", "Kirmizi Boya Kutusu", ItemCategory.Material, ItemRarity.Rare, new Color(0.88f, 0.12f, 0.12f));
        AddItemData("paint_can_purple_001", "Mor Boya Kutusu", ItemCategory.Material, ItemRarity.Rare, new Color(0.52f, 0.24f, 0.82f));
        AddItemData("paint_can_black_001", "Siyah Boya Kutusu", ItemCategory.Material, ItemRarity.Rare, new Color(0.08f, 0.08f, 0.10f));
        AddItemData("paint_can_turquoise_001", "Turkuaz Boya Kutusu", ItemCategory.Material, ItemRarity.Rare, new Color(0.10f, 0.78f, 0.76f));

        AddItemData("paint_can_gold_001", "Altın Boya Kutusu", ItemCategory.Material, ItemRarity.Luxury, new Color(1.00f, 0.72f, 0.12f));
        AddItemData("paint_can_silver_001", "Gümüş Boya Kutusu", ItemCategory.Material, ItemRarity.Luxury, new Color(0.78f, 0.82f, 0.86f));
        AddItemData("paint_can_neon_blue_001", "Neon Mavi Boya Kutusu", ItemCategory.Material, ItemRarity.Luxury, new Color(0.08f, 0.38f, 1.00f));
        AddItemData("paint_can_pearl_white_001", "Inci Beyazi Boya Kutusu", ItemCategory.Material, ItemRarity.Luxury, new Color(0.95f, 0.92f, 0.98f));
        AddItemData("paint_can_lavender_001", "Lavanta Boya Kutusu", ItemCategory.Material, ItemRarity.Luxury, new Color(0.84f, 0.68f, 1.00f));

        AddItemData("craft_fabric_001", "Kumaş", ItemCategory.Material, ItemRarity.Common, new Color(0.58f, 0.18f, 0.20f));
        AddItemData("craft_metal_001", "Metal", ItemCategory.Material, ItemRarity.Rare, new Color(0.42f, 0.43f, 0.46f));
        AddItemData("craft_stone_001", "Taş / Mermer", ItemCategory.Material, ItemRarity.Rare, new Color(0.78f, 0.77f, 0.74f));
        AddItemData("craft_glass_001", "Cam", ItemCategory.Material, ItemRarity.Luxury, new Color(0.35f, 0.68f, 0.78f));
        AddItemData("craft_ceramic_001", "Seramik", ItemCategory.Material, ItemRarity.Luxury, new Color(0.88f, 0.78f, 0.58f));

        AddDecorItemData(1, "wall_pink_soft_001", "Pembe Duvar", DecorSlot.Wall, 1, "paint", DecorUnlockCost);
        AddDecorItemData(2, "wall_mint_soft_001", "Mint Duvar", DecorSlot.Wall, 2, "paint", DecorUnlockCost);
        AddDecorItemData(3, "floor_cream_tile_001", "Krem Zemin", DecorSlot.Floor, 1, "paint", 2);
        AddDecorItemData(4, "table_rose_round_001", "Rose Masa", DecorSlot.Table, 1, "wood", 5);
    }

    private void InitializeFoodUpgradeDatabase()
    {
        foodUpgradeDatabase.Clear();
        foodUpgradeDatabase.Add(new FoodUpgradeData("coffee", "Kahve", FoodUpgradeCategory.Coffee, "food_icon_coffee", 250000, 1, 5, 5));
        foodUpgradeDatabase.Add(new FoodUpgradeData("cookie", "Kurabiye", FoodUpgradeCategory.Sweet, "food_icon_cookie", 750000, 2, 5, 5));
        foodUpgradeDatabase.Add(new FoodUpgradeData("donut", "Donut", FoodUpgradeCategory.Sweet, "food_icon_donut", 2000000, 3, 5, 5));
        foodUpgradeDatabase.Add(new FoodUpgradeData("cake", "Kek", FoodUpgradeCategory.Sweet, "food_icon_cake", 5000000, 5, 5, 5));
        foodUpgradeDatabase.Add(new FoodUpgradeData("croissant", "Kruvasan", FoodUpgradeCategory.Snack, "food_icon_croissant", 12000000, 7, 5, 5));
        foodUpgradeDatabase.Add(new FoodUpgradeData("cupcake", "Cupcake", FoodUpgradeCategory.Sweet, "food_icon_cupcake", 25000000, 9, 5, 5));
        foodUpgradeDatabase.Add(new FoodUpgradeData("cheesecake", "Cheesecake", FoodUpgradeCategory.Sweet, "food_icon_cheesecake", 50000000, 12, 5, 5));
        foodUpgradeDatabase.Add(new FoodUpgradeData("sandwich", "Sandviç", FoodUpgradeCategory.Snack, "food_icon_sandwich", 100000000, 15, 5, 5));
        foodUpgradeDatabase.Add(new FoodUpgradeData("cold_coffee", "Soğuk Kahve", FoodUpgradeCategory.Cold, "food_icon_cold_coffee", 200000000, 18, 5, 5));
    }

    private void AddItemData(string id, string displayName, ItemCategory category, ItemRarity rarity, Color paintColor)
    {
        itemDatabase.Add(new ItemData(id, displayName, category, rarity, paintColor));
    }

    private void AddDecorItemData(int previewId, string itemId, string displayName, DecorSlot slot, int styleId, string costResource, int costAmount)
    {
        decorItemDatabase.Add(new DecorItemData(previewId, itemId, displayName, slot, styleId, costResource, costAmount));
    }

    private ItemData GetItemData(string itemId)
    {
        for (int i = 0; i < itemDatabase.Count; i++)
        {
            if (itemDatabase[i].Id == itemId)
            {
                return itemDatabase[i];
            }
        }

        return null;
    }

    private void AddInventoryItem(string itemId, int amount)
    {
        if (string.IsNullOrEmpty(itemId) || amount <= 0)
        {
            return;
        }

        if (!itemInventory.ContainsKey(itemId))
        {
            itemInventory[itemId] = 0;
        }

        itemInventory[itemId] += amount;
    }

    private int GetInventoryItemCount(string itemId)
    {
        if (string.IsNullOrEmpty(itemId) || !itemInventory.ContainsKey(itemId))
        {
            return 0;
        }

        return itemInventory[itemId];
    }

    private string GetWorkshopMaterialInventoryId(WorkshopCraftMaterialType material)
    {
        if (material == WorkshopCraftMaterialType.Fabric) return "craft_fabric_001";
        if (material == WorkshopCraftMaterialType.Metal) return "craft_metal_001";
        if (material == WorkshopCraftMaterialType.Stone) return "craft_stone_001";
        if (material == WorkshopCraftMaterialType.Glass) return "craft_glass_001";
        if (material == WorkshopCraftMaterialType.Ceramic) return "craft_ceramic_001";
        return "";
    }

    private string GetRandomPaintCanId(ItemRarity maxRarity)
    {
        List<ItemData> candidates = new List<ItemData>();
        for (int i = 0; i < itemDatabase.Count; i++)
        {
            ItemData item = itemDatabase[i];
            if (item.Category == ItemCategory.Material
                && item.Id.StartsWith("paint_can_", StringComparison.Ordinal)
                && (int)item.Rarity <= (int)maxRarity)
            {
                candidates.Add(item);
            }
        }

        if (candidates.Count == 0)
        {
            return "";
        }

        return candidates[UnityEngine.Random.Range(0, candidates.Count)].Id;
    }

    private string GetPaintInventorySummary()
    {
        return GetPaintInventoryLine(ItemRarity.Common, "Siradan Boya Kutulari")
            + GetPaintInventoryLine(ItemRarity.Rare, "Nadir Boya Kutulari")
            + GetPaintInventoryLine(ItemRarity.Luxury, "Ozel Boya Kutulari");
    }

    private string GetPaintInventoryLine(ItemRarity rarity, string label)
    {
        string line = "";
        int total = 0;

        for (int i = 0; i < itemDatabase.Count; i++)
        {
            ItemData item = itemDatabase[i];
            int count = GetInventoryItemCount(item.Id);
            if (count <= 0 || item.Rarity != rarity || !item.Id.StartsWith("paint_can_", StringComparison.Ordinal))
            {
                continue;
            }

            total += count;

            if (line.Length > 0)
            {
                line += ", ";
            }

            line += GetCompactItemDisplayNameWithRarity(item) + " x" + count;
        }

        if (string.IsNullOrEmpty(line))
        {
            return "\n" + label + ": 0";
        }

        return "\n" + label + ": " + total + " | " + line;
    }

    public void OpenChest()
    {
        OpenChestByTier(ChestTier.Common);
    }

    public void OpenRareChest()
    {
        OpenChestByTier(ChestTier.Rare);
    }

    public void OpenLuxuryChest()
    {
        OpenChestByTier(ChestTier.Luxury);
    }

    private void OpenChestByTier(ChestTier tier)
    {
        if (chestOpeningInProgress)
        {
            return;
        }

        if (GetChestCount(tier) <= 0)
        {
            lastChestRewardText = "Acacak " + GetChestTierName(tier) + " yok.";
            UpdateUI();
            return;
        }

        if (tier == ChestTier.Common)
            chests--;
        else if (tier == ChestTier.Rare)
            rareChests--;
        else
            luxuryChests--;

        chestOpeningInProgress = true;
        shouldAnimateChestReward = true;
        openingChestTier = tier;
        dailyChestsOpened++;
        int roll = UnityEngine.Random.Range(0, 100);

        if (tier == ChestTier.Common)
        {
            OpenCommonChestReward(roll);
        }
        else if (tier == ChestTier.Rare)
        {
            OpenRareChestReward(roll);
        }
        else
        {
            OpenLuxuryChestReward(roll);
        }

        UpdateUI();
        SaveGame();
    }

    private void OpenCommonChestReward(int roll)
    {
        if (roll < 20)
        {
            gems += 1;
            lastChestRewardText = "Sıradan Sandıktan 1 Elmas çıktı.";
            ShowChestReward(lastChestRewardText, GetGemIconSprite());
        }
        else
        {
            AddRandomWorkshopMaterialReward(ChestTier.Common);
        }
    }

    private void OpenRareChestReward(int roll)
    {
        if (roll < 20)
        {
            gems += 2;
            lastChestRewardText = "Nadir Sandıktan 2 Elmas çıktı.";
            ShowChestReward(lastChestRewardText, GetGemIconSprite());
        }
        else
        {
            AddRandomWorkshopMaterialReward(ChestTier.Rare);
        }
    }

    private void OpenLuxuryChestReward(int roll)
    {
        if (roll < 20)
        {
            gems += 5;
            lastChestRewardText = "Özel Sandıktan 5 Elmas çıktı.";
            ShowChestReward(lastChestRewardText, GetGemIconSprite());
        }
        else
        {
            AddRandomWorkshopMaterialReward(ChestTier.Luxury);
        }
    }

    private void AddRandomWorkshopMaterialReward(ChestTier tier)
    {
        WorkshopCraftMaterialType[] pool;
        int amount;
        string chestName;
        if (tier == ChestTier.Luxury)
        {
            pool = new[]
            {
                WorkshopCraftMaterialType.Paint,
                WorkshopCraftMaterialType.Wood,
                WorkshopCraftMaterialType.Fabric, WorkshopCraftMaterialType.Fabric,
                WorkshopCraftMaterialType.Metal, WorkshopCraftMaterialType.Metal, WorkshopCraftMaterialType.Metal,
                WorkshopCraftMaterialType.Stone, WorkshopCraftMaterialType.Stone, WorkshopCraftMaterialType.Stone,
                WorkshopCraftMaterialType.Glass, WorkshopCraftMaterialType.Glass, WorkshopCraftMaterialType.Glass,
                WorkshopCraftMaterialType.Ceramic, WorkshopCraftMaterialType.Ceramic, WorkshopCraftMaterialType.Ceramic
            };
            amount = UnityEngine.Random.Range(2, 4);
            chestName = "Özel Sandık";
        }
        else if (tier == ChestTier.Rare)
        {
            pool = new[]
            {
                WorkshopCraftMaterialType.Paint, WorkshopCraftMaterialType.Paint,
                WorkshopCraftMaterialType.Wood, WorkshopCraftMaterialType.Wood,
                WorkshopCraftMaterialType.Fabric, WorkshopCraftMaterialType.Fabric, WorkshopCraftMaterialType.Fabric,
                WorkshopCraftMaterialType.Metal, WorkshopCraftMaterialType.Metal, WorkshopCraftMaterialType.Metal,
                WorkshopCraftMaterialType.Stone, WorkshopCraftMaterialType.Stone, WorkshopCraftMaterialType.Stone,
                WorkshopCraftMaterialType.Glass, WorkshopCraftMaterialType.Glass,
                WorkshopCraftMaterialType.Ceramic
            };
            amount = UnityEngine.Random.Range(1, 3);
            chestName = "Nadir Sandık";
        }
        else
        {
            pool = new[]
            {
                WorkshopCraftMaterialType.Paint, WorkshopCraftMaterialType.Paint, WorkshopCraftMaterialType.Paint,
                WorkshopCraftMaterialType.Wood, WorkshopCraftMaterialType.Wood, WorkshopCraftMaterialType.Wood,
                WorkshopCraftMaterialType.Fabric, WorkshopCraftMaterialType.Fabric, WorkshopCraftMaterialType.Fabric,
                WorkshopCraftMaterialType.Stone, WorkshopCraftMaterialType.Stone, WorkshopCraftMaterialType.Stone,
                WorkshopCraftMaterialType.Ceramic, WorkshopCraftMaterialType.Ceramic,
                WorkshopCraftMaterialType.Metal,
                WorkshopCraftMaterialType.Glass
            };
            amount = 1;
            chestName = "Sıradan Sandık";
        }

        WorkshopCraftMaterialType material = pool[UnityEngine.Random.Range(0, pool.Length)];
        if (material == WorkshopCraftMaterialType.Paint)
        {
            paintPieces += amount;
        }
        else if (material == WorkshopCraftMaterialType.Wood)
        {
            woodPieces += amount;
        }
        else
        {
            AddInventoryItem(GetWorkshopMaterialInventoryId(material), amount);
        }

        lastChestRewardText = chestName + " ödülü: " + amount + " " + GetWorkshopMaterialName(material) + ".";
        ShowChestReward(lastChestRewardText, GetWorkshopMaterialSprite(material), GetWorkshopChestMaterialColor(tier));
    }

    private static Color GetWorkshopChestMaterialColor(ChestTier tier)
    {
        if (tier == ChestTier.Luxury) return new Color(0.30f, 0.12f, 0.38f, 0.99f);
        if (tier == ChestTier.Rare) return new Color(0.08f, 0.34f, 0.38f, 0.99f);
        return new Color(0.56f, 0.32f, 0.12f, 0.99f);
    }

    private void AddRandomPaintCanReward(ItemRarity maxRarity, string chestName)
    {
        string itemId = GetRandomPaintCanId(maxRarity);
        ItemData item = GetItemData(itemId);
        if (item == null)
        {
            paintPieces += 1;
            lastChestRewardText = chestName + " odulu: 1 Boya Kutusu.";
            ShowChestReward(lastChestRewardText, null);
            return;
        }

        AddInventoryItem(itemId, 1);
        lastChestRewardText = chestName + " odulu: 1 " + GetItemDisplayNameWithRarity(item) + ".";
        ShowChestReward(lastChestRewardText, GetPaintCanSprite(item), GetItemRarityPanelColor(item));
    }

    private string GetItemDisplayNameWithRarity(ItemData item)
    {
        if (item == null)
        {
            return "";
        }

        if (item.Rarity == ItemRarity.Rare)
        {
            return item.DisplayName + " (N)";
        }

        if (item.Rarity == ItemRarity.Luxury)
        {
            return item.DisplayName + " (O)";
        }

        return item.DisplayName;
    }

    private string GetCompactItemDisplayNameWithRarity(ItemData item)
    {
        if (item == null)
        {
            return "";
        }

        string displayName = item.DisplayName
            .Replace(" Boya Kutusu", "")
            .Replace(" Boyasi", "")
            .Replace(" Efekt", "");

        if (item.Rarity == ItemRarity.Rare)
        {
            return displayName + "(N)";
        }

        if (item.Rarity == ItemRarity.Luxury)
        {
            return displayName + "(O)";
        }

        return displayName;
    }

    private string GetChestTierName(ChestTier tier)
    {
        if (tier == ChestTier.Rare)
        {
            return "Nadir Sandik";
        }

        if (tier == ChestTier.Luxury)
        {
            return "Ozel Sandik";
        }

        return "Siradan Sandik";
    }

    private int GetChestCount(ChestTier tier)
    {
        if (tier == ChestTier.Rare)
        {
            return rareChests;
        }

        if (tier == ChestTier.Luxury)
        {
            return luxuryChests;
        }

        return chests;
    }

    private Image CreateChestSelectionImage(Transform parent, string objectName, ChestTier tier, Vector2 position)
    {
        GameObject imageObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        imageObject.transform.SetParent(parent, false);
        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(245f, 245f);
        Image image = imageObject.GetComponent<Image>();
        Sprite[] frames = GetChestAnimationFrames(tier);
        image.sprite = frames != null && frames.Length > 0 ? frames[0] : null;
        image.preserveAspect = true;
        image.raycastTarget = false;
        return image;
    }

    private Sprite[] GetChestAnimationFrames(ChestTier tier)
    {
        if (chestAnimationFrames.TryGetValue(tier, out Sprite[] cached))
        {
            return cached;
        }

        string assetName = tier == ChestTier.Rare
            ? "chest_rare_animation"
            : tier == ChestTier.Luxury ? "chest_luxury_animation" : "chest_common_animation";
        Texture2D atlas = Resources.Load<Texture2D>("Art/UI/Chests/" + assetName);
        if (atlas == null)
        {
            chestAnimationFrames[tier] = null;
            return null;
        }

        float cellWidth = atlas.width * 0.5f;
        float cellHeight = atlas.height * 0.5f;
        Sprite[] frames = new Sprite[4];
        frames[0] = Sprite.Create(atlas, new Rect(0f, cellHeight, cellWidth, cellHeight), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
        frames[1] = Sprite.Create(atlas, new Rect(cellWidth, cellHeight, cellWidth, cellHeight), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
        frames[2] = Sprite.Create(atlas, new Rect(0f, 0f, cellWidth, cellHeight), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
        frames[3] = Sprite.Create(atlas, new Rect(cellWidth, 0f, cellWidth, cellHeight), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
        for (int i = 0; i < frames.Length; i++)
        {
            frames[i].name = assetName + "_frame_" + i;
        }
        chestAnimationFrames[tier] = frames;
        return frames;
    }

    private Sprite[] GetChestSilhouetteFrames(ChestTier tier)
    {
        if (chestSilhouetteFrames.TryGetValue(tier, out Sprite[] cached)) return cached;
        string assetName = tier == ChestTier.Rare
            ? "chest_rare_silhouette"
            : tier == ChestTier.Luxury ? "chest_luxury_silhouette" : "chest_common_silhouette";
        Texture2D atlas = Resources.Load<Texture2D>("Art/UI/Chests/" + assetName);
        if (atlas == null)
        {
            chestSilhouetteFrames[tier] = null;
            return null;
        }

        float cellWidth = atlas.width * 0.5f;
        float cellHeight = atlas.height * 0.5f;
        Sprite[] frames = new Sprite[4];
        frames[0] = Sprite.Create(atlas, new Rect(0f, cellHeight, cellWidth, cellHeight), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
        frames[1] = Sprite.Create(atlas, new Rect(cellWidth, cellHeight, cellWidth, cellHeight), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
        frames[2] = Sprite.Create(atlas, new Rect(0f, 0f, cellWidth, cellHeight), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
        frames[3] = Sprite.Create(atlas, new Rect(cellWidth, 0f, cellWidth, cellHeight), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
        chestSilhouetteFrames[tier] = frames;
        return frames;
    }

    private Sprite GetChestLayerSprite(ChestTier tier, string layer)
    {
        string tierName = tier == ChestTier.Rare ? "rare" : tier == ChestTier.Luxury ? "luxury" : "common";
        string key = tierName + "_" + layer;
        if (chestLayerSprites.TryGetValue(key, out Sprite cached)) return cached;
        Texture2D texture = Resources.Load<Texture2D>("Art/UI/Chests/chest_" + key);
        if (texture == null)
        {
            chestLayerSprites[key] = null;
            return null;
        }
        Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
        sprite.name = "ChestLayer_" + key;
        chestLayerSprites[key] = sprite;
        return sprite;
    }

    private Sprite CreateChestGlowSprite()
    {
        if (chestGlowSprite != null) return chestGlowSprite;
        const int size = 96;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.name = "ChestOpeningRadialGlowTexture";
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;
        Color[] pixels = new Color[size * size];
        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        float radius = size * 0.5f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center) / radius;
                float alpha = Mathf.Pow(1f - Mathf.Clamp01(distance), 2.2f);
                pixels[(y * size) + x] = new Color(1f, 1f, 1f, alpha);
            }
        }
        texture.SetPixels(pixels);
        texture.Apply(false, true);
        chestGlowSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
        chestGlowSprite.name = "ChestOpeningRadialGlow";
        return chestGlowSprite;
    }

    private Sprite CreateChestRingSprite()
    {
        if (chestRingSprite != null) return chestRingSprite;
        const int size = 128;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.name = "ChestOpeningEnergyRingTexture";
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;
        Color[] pixels = new Color[size * size];
        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        float radius = size * 0.5f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 offset = new Vector2(x, y) - center;
                float normalizedDistance = offset.magnitude / radius;
                float ring = 1f - Mathf.Clamp01(Mathf.Abs(normalizedDistance - 0.72f) / 0.055f);
                float angle = Mathf.Atan2(offset.y, offset.x);
                float streak = 0.72f + (0.28f * Mathf.Sin((angle * 7f) + (normalizedDistance * 12f)));
                float alpha = ring * streak;
                pixels[(y * size) + x] = new Color(1f, 1f, 1f, alpha);
            }
        }
        texture.SetPixels(pixels);
        texture.Apply(false, true);
        chestRingSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
        chestRingSprite.name = "ChestOpeningEnergyRing";
        return chestRingSprite;
    }

    private Sprite CreateChestBeamSprite()
    {
        if (chestBeamSprite != null) return chestBeamSprite;
        const int width = 32;
        const int height = 256;
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.name = "ChestOpeningBeamTexture";
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;
        Color[] pixels = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            float vertical = y / (height - 1f);
            float taper = Mathf.Pow(1f - vertical, 1.35f);
            for (int x = 0; x < width; x++)
            {
                float horizontal = Mathf.Abs((x / (width - 1f)) - 0.5f) * 2f;
                float alpha = Mathf.Pow(1f - Mathf.Clamp01(horizontal), 2.8f) * taper;
                pixels[(y * width) + x] = new Color(1f, 1f, 1f, alpha);
            }
        }
        texture.SetPixels(pixels);
        texture.Apply(false, true);
        chestBeamSprite = Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0f), 100f);
        chestBeamSprite.name = "ChestOpeningBeam";
        return chestBeamSprite;
    }

    private Sprite CreateChestStarSprite()
    {
        if (chestStarSprite != null) return chestStarSprite;
        const int size = 64;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.name = "ChestOpeningStarTexture";
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;
        Color[] pixels = new Color[size * size];
        float center = (size - 1) * 0.5f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Abs(x - center) / center;
                float dy = Mathf.Abs(y - center) / center;
                float vertical = 1f - Mathf.Clamp01(dx * 5.5f + dy * 0.85f);
                float horizontal = 1f - Mathf.Clamp01(dy * 5.5f + dx * 0.85f);
                float core = 1f - Mathf.Clamp01(Mathf.Sqrt((dx * dx) + (dy * dy)) * 5f);
                float alpha = Mathf.Clamp01(Mathf.Max(vertical, horizontal) + core);
                pixels[(y * size) + x] = new Color(1f, 1f, 1f, alpha);
            }
        }
        texture.SetPixels(pixels);
        texture.Apply(false, true);
        chestStarSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
        chestStarSprite.name = "ChestOpeningStar";
        return chestStarSprite;
    }

    private AudioClip CreateChestImpactSound()
    {
        const int sampleRate = 44100;
        const int channels = 2;
        const float duration = 0.62f;
        int frameCount = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[frameCount * channels];
        System.Random random = new System.Random(4817);
        float filteredNoiseLeft = 0f;
        float filteredNoiseRight = 0f;
        for (int i = 0; i < frameCount; i++)
        {
            float time = i / (float)sampleRate;
            float bassFrequency = Mathf.Lerp(112f, 43f, Mathf.Clamp01(time * 5f));
            float thud = Mathf.Sin(time * bassFrequency * Mathf.PI * 2f) * Mathf.Exp(-time * 10.5f);
            float metal = (Mathf.Sin(time * 1280f * Mathf.PI * 2f) * 0.36f
                + Mathf.Sin(time * 2137f * Mathf.PI * 2f) * 0.24f
                + Mathf.Sin(time * 3181f * Mathf.PI * 2f) * 0.12f) * Mathf.Exp(-time * 16f);
            float secondLatchTime = Mathf.Max(0f, time - 0.105f);
            float secondLatch = time >= 0.105f
                ? (Mathf.Sin(secondLatchTime * 1760f * Mathf.PI * 2f) * Mathf.Exp(-secondLatchTime * 28f) * 0.42f)
                : 0f;
            float noiseLeft = ((float)random.NextDouble() * 2f) - 1f;
            float noiseRight = ((float)random.NextDouble() * 2f) - 1f;
            filteredNoiseLeft = (filteredNoiseLeft * 0.86f) + (noiseLeft * 0.14f);
            filteredNoiseRight = (filteredNoiseRight * 0.86f) + (noiseRight * 0.14f);
            float gritEnvelope = Mathf.Exp(-time * 19f) * 0.24f;
            float left = (thud * 0.72f) + (metal * 0.70f) + secondLatch + (filteredNoiseLeft * gritEnvelope);
            float right = (thud * 0.72f) + (metal * 0.62f) + (secondLatch * 0.82f) + (filteredNoiseRight * gritEnvelope);
            samples[(i * channels)] = left / (1f + Mathf.Abs(left));
            samples[(i * channels) + 1] = right / (1f + Mathf.Abs(right));
        }
        AudioClip clip = AudioClip.Create("ChestKeyImpact", frameCount, channels, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip CreateChestOpenSound(string clipName, float baseFrequency, float intensity, int seed)
    {
        const int sampleRate = 44100;
        const int channels = 2;
        const float duration = 1.36f;
        int frameCount = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[frameCount * channels];
        System.Random random = new System.Random(seed);
        float slowNoiseLeft = 0f;
        float slowNoiseRight = 0f;
        float brightNoiseLeft = 0f;
        float brightNoiseRight = 0f;
        float sweepPhase = 0f;
        for (int i = 0; i < frameCount; i++)
        {
            float time = i / (float)sampleRate;
            float t = time / duration;
            float noiseLeft = ((float)random.NextDouble() * 2f) - 1f;
            float noiseRight = ((float)random.NextDouble() * 2f) - 1f;
            slowNoiseLeft = (slowNoiseLeft * 0.975f) + (noiseLeft * 0.025f);
            slowNoiseRight = (slowNoiseRight * 0.975f) + (noiseRight * 0.025f);
            brightNoiseLeft = (brightNoiseLeft * 0.72f) + (noiseLeft * 0.28f);
            brightNoiseRight = (brightNoiseRight * 0.72f) + (noiseRight * 0.28f);

            float hingeT = Mathf.Clamp01(time / 0.48f);
            float hingeEnvelope = Mathf.Sin(hingeT * Mathf.PI) * (time < 0.52f ? 1f : 0f);
            float hingeFrequency = Mathf.Lerp(118f, 54f, hingeT) + (Mathf.Sin(time * 31f) * 9f);
            float hinge = (Mathf.Sin(time * hingeFrequency * Mathf.PI * 2f)
                + (Mathf.Sin(time * hingeFrequency * 2.47f * Mathf.PI * 2f) * 0.34f)) * hingeEnvelope;

            float impactTime = Mathf.Max(0f, time - 0.29f);
            float bodyImpact = time >= 0.29f
                ? Mathf.Sin(impactTime * Mathf.Lerp(96f, 42f, Mathf.Clamp01(impactTime * 5f)) * Mathf.PI * 2f) * Mathf.Exp(-impactTime * 8.8f)
                : 0f;

            float whooshT = Mathf.Clamp01((time - 0.18f) / 1.08f);
            float whooshEnvelope = Mathf.Sin(whooshT * Mathf.PI) * Mathf.Sqrt(whooshT);
            float sweepFrequency = Mathf.Lerp(baseFrequency * 0.30f, baseFrequency * 1.72f, Mathf.Pow(whooshT, 0.62f));
            sweepPhase += (Mathf.PI * 2f * sweepFrequency) / sampleRate;
            float tonalSweep = Mathf.Sin(sweepPhase) * whooshEnvelope;

            float shimmerTime = Mathf.Max(0f, time - 0.34f);
            float shimmerEnvelope = time >= 0.34f ? Mathf.Exp(-shimmerTime * 2.25f) * Mathf.Clamp01(shimmerTime * 9f) : 0f;
            float shimmerLeft = (Mathf.Sin(shimmerTime * baseFrequency * 1.01f * Mathf.PI * 2f) * 0.32f
                + Mathf.Sin(shimmerTime * baseFrequency * 2.02f * Mathf.PI * 2f) * 0.13f) * shimmerEnvelope;
            float shimmerRight = (Mathf.Sin(shimmerTime * baseFrequency * 1.006f * Mathf.PI * 2f) * 0.32f
                + Mathf.Sin(shimmerTime * baseFrequency * 2.51f * Mathf.PI * 2f) * 0.11f) * shimmerEnvelope;

            float left = ((hinge * 0.25f) + (bodyImpact * 0.58f) + (slowNoiseLeft * hingeEnvelope * 0.34f)
                + (brightNoiseLeft * whooshEnvelope * 0.24f) + (tonalSweep * 0.16f) + shimmerLeft) * intensity;
            float right = ((hinge * 0.23f) + (bodyImpact * 0.58f) + (slowNoiseRight * hingeEnvelope * 0.34f)
                + (brightNoiseRight * whooshEnvelope * 0.24f) + (tonalSweep * 0.16f) + shimmerRight) * intensity;
            samples[(i * channels)] = left / (1f + Mathf.Abs(left));
            samples[(i * channels) + 1] = right / (1f + Mathf.Abs(right));
        }
        AudioClip clip = AudioClip.Create(clipName, frameCount, channels, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip CreateChestRewardLaunchSound()
    {
        const int sampleRate = 44100;
        const int channels = 2;
        const float duration = 0.82f;
        int frameCount = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[frameCount * channels];
        System.Random random = new System.Random(8241);
        float filteredLeft = 0f;
        float filteredRight = 0f;
        float phase = 0f;
        for (int i = 0; i < frameCount; i++)
        {
            float time = i / (float)sampleRate;
            float t = time / duration;
            float noiseLeft = ((float)random.NextDouble() * 2f) - 1f;
            float noiseRight = ((float)random.NextDouble() * 2f) - 1f;
            filteredLeft = (filteredLeft * 0.84f) + (noiseLeft * 0.16f);
            filteredRight = (filteredRight * 0.84f) + (noiseRight * 0.16f);
            float envelope = Mathf.Sin(Mathf.Clamp01(t) * Mathf.PI) * Mathf.Pow(Mathf.Clamp01(t), 0.35f);
            float frequency = Mathf.Lerp(180f, 1320f, Mathf.Pow(t, 0.72f));
            phase += (Mathf.PI * 2f * frequency) / sampleRate;
            float rise = Mathf.Sin(phase) * envelope;
            float sparkleTime = Mathf.Max(0f, time - 0.32f);
            float sparkle = time >= 0.32f
                ? (Mathf.Sin(sparkleTime * 1760f * Mathf.PI * 2f) + (Mathf.Sin(sparkleTime * 2637f * Mathf.PI * 2f) * 0.44f)) * Mathf.Exp(-sparkleTime * 7.5f)
                : 0f;
            float left = (filteredLeft * envelope * 0.34f) + (rise * 0.20f) + (sparkle * 0.18f);
            float right = (filteredRight * envelope * 0.34f) + (rise * 0.20f) + (sparkle * 0.22f);
            samples[(i * channels)] = left / (1f + Mathf.Abs(left));
            samples[(i * channels) + 1] = right / (1f + Mathf.Abs(right));
        }
        AudioClip clip = AudioClip.Create("ChestRewardLaunch", frameCount, channels, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip CreateChestRewardSound()
    {
        const int sampleRate = 44100;
        const int channels = 2;
        const float duration = 1.02f;
        int frameCount = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[frameCount * channels];
        float[] frequencies = { 523.25f, 659.25f, 783.99f, 1046.5f };
        for (int i = 0; i < frameCount; i++)
        {
            float time = i / (float)sampleRate;
            float left = 0f;
            float right = 0f;
            for (int note = 0; note < frequencies.Length; note++)
            {
                float delay = note * 0.058f;
                if (time < delay) continue;
                float noteTime = time - delay;
                float attack = Mathf.Clamp01(noteTime * 34f);
                float envelope = attack * Mathf.Exp(-noteTime * (3.4f + (note * 0.28f)));
                float amplitude = 0.24f - (note * 0.025f);
                float pan = Mathf.Lerp(-0.48f, 0.48f, note / (frequencies.Length - 1f));
                float fundamental = Mathf.Sin(noteTime * frequencies[note] * Mathf.PI * 2f);
                float overtone = Mathf.Sin(noteTime * frequencies[note] * 2.003f * Mathf.PI * 2f) * 0.23f;
                float tone = (fundamental + overtone) * envelope * amplitude;
                left += tone * (1f - Mathf.Max(0f, pan));
                right += tone * (1f + Mathf.Min(0f, pan));
            }
            float lowBody = Mathf.Sin(time * 130.81f * Mathf.PI * 2f) * Mathf.Exp(-time * 5.2f) * 0.12f;
            left += lowBody;
            right += lowBody;
            samples[(i * channels)] = left / (1f + Mathf.Abs(left));
            samples[(i * channels) + 1] = right / (1f + Mathf.Abs(right));
        }
        AudioClip clip = AudioClip.Create("ChestRewardReveal", frameCount, channels, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private void ConfigureSaleCoinAudio()
    {
        if (saleCoinAudioSource == null)
        {
            saleCoinAudioSource = gameObject.AddComponent<AudioSource>();
            saleCoinAudioSource.playOnAwake = false;
            saleCoinAudioSource.loop = false;
            saleCoinAudioSource.spatialBlend = 0f;
            saleCoinAudioSource.ignoreListenerPause = true;
            saleCoinAudioSource.volume = 0.78f;
        }

        if (saleCoinClip == null)
        {
            saleCoinClip = Resources.Load<AudioClip>("Audio/sale_coin_ting");
            if (saleCoinClip == null)
            {
                saleCoinClip = CreateSaleCoinSound();
            }
        }
    }

    private AudioClip CreateSaleCoinSound()
    {
        const int sampleRate = 44100;
        const int channels = 2;
        const float duration = 0.34f;
        int frameCount = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[frameCount * channels];
        System.Random random = new System.Random(7319);
        float filteredLeft = 0f;
        float filteredRight = 0f;
        for (int i = 0; i < frameCount; i++)
        {
            float time = i / (float)sampleRate;
            float attack = Mathf.Clamp01(time * 900f);
            float contactEnvelope = attack * Mathf.Exp(-time * 78f);
            float ringEnvelope = attack * Mathf.Exp(-time * 13.5f);
            float contact = (Mathf.Sin(time * 940f * Mathf.PI * 2f) * 0.12f
                + Mathf.Sin(time * 1835f * Mathf.PI * 2f) * 0.17f) * contactEnvelope;
            float coinRing = (Mathf.Sin(time * 2380f * Mathf.PI * 2f) * 0.25f
                + Mathf.Sin(time * 3565f * Mathf.PI * 2f) * 0.17f
                + Mathf.Sin(time * 5270f * Mathf.PI * 2f) * 0.08f) * ringEnvelope;
            float noiseLeft = ((float)random.NextDouble() * 2f) - 1f;
            float noiseRight = ((float)random.NextDouble() * 2f) - 1f;
            filteredLeft = (filteredLeft * 0.68f) + (noiseLeft * 0.32f);
            filteredRight = (filteredRight * 0.68f) + (noiseRight * 0.32f);
            float textureEnvelope = Mathf.Exp(-time * 125f) * 0.045f;
            float left = contact + coinRing + (filteredLeft * textureEnvelope);
            float right = (contact * 0.96f) + (coinRing * 1.035f) + (filteredRight * textureEnvelope);
            samples[i * channels] = left / (1f + Mathf.Abs(left));
            samples[(i * channels) + 1] = right / (1f + Mathf.Abs(right));
        }

        AudioClip clip = AudioClip.Create("SaleCoinTing", frameCount, channels, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private void PlaySaleCoinSound()
    {
        if (saleCoinAudioSource == null || saleCoinClip == null) return;
        if (Time.unscaledTime - lastSaleCoinTime < 0.018f) return;
        lastSaleCoinTime = Time.unscaledTime;
        saleCoinAudioSource.pitch = 1.08f;
        saleCoinAudioSource.PlayOneShot(saleCoinClip, 0.9f);
        ShowSaleCornerFeedback();
    }

    private void ShowSaleCornerFeedback()
    {
        if (saleCornerFeedbackRoot == null || saleCornerFeedbackGroup == null)
        {
            return;
        }

        saleCornerFeedbackRoot.gameObject.SetActive(true);
        saleCornerFeedbackRoot.SetAsLastSibling();
        if (saleCornerFeedbackCoroutine != null)
        {
            StopCoroutine(saleCornerFeedbackCoroutine);
        }

        saleCornerFeedbackCoroutine = StartCoroutine(PlaySaleCornerFeedback());
    }

    private IEnumerator PlaySaleCornerFeedback()
    {
        Vector2 basePosition = new Vector2(720f, 360f);
        saleCornerFeedbackRoot.anchoredPosition = basePosition;
        saleCornerFeedbackGroup.alpha = 1f;

        const float duration = 0.62f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float pop = Mathf.Sin(Mathf.Clamp01(t / 0.18f) * Mathf.PI) * 0.10f;
            float fade = t < 0.46f ? 1f : 1f - Mathf.Clamp01((t - 0.46f) / 0.54f);
            saleCornerFeedbackRoot.anchoredPosition = basePosition + new Vector2(EaseOutCubic(t) * 10f, EaseOutCubic(t) * 18f);
            saleCornerFeedbackRoot.localScale = Vector3.one * (1f + pop);
            saleCornerFeedbackGroup.alpha = fade;
            yield return null;
        }

        saleCornerFeedbackGroup.alpha = 0f;
        saleCornerFeedbackRoot.localScale = Vector3.one;
        saleCornerFeedbackRoot.gameObject.SetActive(false);
        saleCornerFeedbackCoroutine = null;
    }

    private IEnumerator ShowSaleFeedbackAfterFrame()
    {
        yield return null;
        ShowSaleFeedbackAtPointer();
    }

    private IEnumerator CrossfadeChestFrame(Sprite nextFrame, float duration)
    {
        if (chestOpeningImage == null || chestOpeningTransitionImage == null || nextFrame == null)
        {
            if (chestOpeningImage != null) chestOpeningImage.sprite = nextFrame;
            yield break;
        }

        chestOpeningTransitionImage.sprite = nextFrame;
        chestOpeningTransitionImage.color = new Color(1f, 1f, 1f, 0f);
        chestOpeningTransitionImage.gameObject.SetActive(true);
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            chestOpeningTransitionImage.color = new Color(1f, 1f, 1f, EaseOutCubic(t));
            chestOpeningImage.color = new Color(1f, 1f, 1f, Mathf.Lerp(1f, 0.72f, t));
            yield return null;
        }
        chestOpeningImage.sprite = nextFrame;
        chestOpeningImage.color = Color.white;
        chestOpeningTransitionImage.gameObject.SetActive(false);
    }

    private Image CreateChestAnimationEffectImage(Transform parent, string objectName, Vector2 position, Vector2 size)
    {
        GameObject effectObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        effectObject.transform.SetParent(parent, false);
        RectTransform rect = effectObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        Image image = effectObject.GetComponent<Image>();
        image.sprite = chestGlowSprite != null ? chestGlowSprite : CreateChestGlowSprite();
        image.color = Color.clear;
        image.raycastTarget = false;
        return image;
    }

    private static float EaseOutBack(float t)
    {
        const float overshoot = 1.70158f;
        float shifted = Mathf.Clamp01(t) - 1f;
        return 1f + ((overshoot + 1f) * shifted * shifted * shifted) + (overshoot * shifted * shifted);
    }

    private static float EaseOutCubic(float t)
    {
        float shifted = 1f - Mathf.Clamp01(t);
        return 1f - (shifted * shifted * shifted);
    }

    private void CreateChestPanel(Transform parent)
    {
        chestPanel = new GameObject("ChestPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        chestPanel.transform.SetParent(parent, false);

        RectTransform panelRect = chestPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = new Vector2(0f, 12f);
        panelRect.sizeDelta = new Vector2(958f, 636f);

        Image panelImage = chestPanel.GetComponent<Image>();
        panelImage.sprite = LoadSpriteFromResources("Art/UI/Chests/chest_selection_panel_v1");
        panelImage.color = Color.white;
        panelImage.preserveAspect = true;

        TMP_Text title = CreateText(chestPanel.transform, "ChestPanelTitle", "SANDIKLAR", new Vector2(0f, 240.1f), new Vector2(610f, 58f), 42);
        title.fontStyle = FontStyles.Bold;
        title.color = new Color(0.28f, 0.12f, 0.035f, 1f);
        title.alignment = TextAlignmentOptions.Center;
        title.enableAutoSizing = true;
        title.fontSizeMin = 28f;
        title.fontSizeMax = 42f;

        chestPanelText = CreateText(chestPanel.transform, "ChestPanelText", "", new Vector2(0, 45), new Vector2(850, 330), 20);
        StylePopupBodyText(chestPanelText, 14, 20, TextAlignmentOptions.TopLeft);
        chestPanelText.gameObject.SetActive(false);

        commonChestCountText = CreateText(chestPanel.transform, "CommonChestCount", "", new Vector2(-294.7f, -156.8f), new Vector2(205f, 46f), 25);
        rareChestCountText = CreateText(chestPanel.transform, "RareChestCount", "", new Vector2(0f, -156.8f), new Vector2(205f, 46f), 25);
        luxuryChestCountText = CreateText(chestPanel.transform, "LuxuryChestCount", "", new Vector2(294.7f, -156.8f), new Vector2(205f, 46f), 25);
        ConfigureChestPanelCountText(commonChestCountText, new Color(1f, 0.84f, 0.42f, 1f));
        ConfigureChestPanelCountText(rareChestCountText, new Color(0.72f, 1f, 0.96f, 1f));
        ConfigureChestPanelCountText(luxuryChestCountText, new Color(1f, 0.76f, 0.42f, 1f));
        commonChestCountText.enableAutoSizing = false;
        commonChestCountText.fontSize = 19f;
        rareChestCountText.enableAutoSizing = false;
        rareChestCountText.fontSize = 19f;
        luxuryChestCountText.enableAutoSizing = false;
        luxuryChestCountText.fontSize = 19f;

        chestOpenButtonText = CreateChestPanelButton("ChestOpenButton", "SIRADAN AÇ", new Vector2(-297.2f, -233.9f), OpenChest, new Color(1f, 0.90f, 0.63f, 1f));
        rareChestOpenButtonText = CreateChestPanelButton("RareChestOpenButton", "NADIR AÇ", new Vector2(0f, -233.9f), OpenRareChest, new Color(0.80f, 1f, 0.97f, 1f));
        luxuryChestOpenButtonText = CreateChestPanelButton("LuxuryChestOpenButton", "ÖZEL AÇ", new Vector2(297.2f, -233.9f), OpenLuxuryChest, new Color(1f, 0.87f, 0.56f, 1f));

        TMP_Text closeButtonText = CreateButton(chestPanel.transform, "ChestCloseButton", "", new Vector2(444f, 266f), new Vector2(68f, 68f), CloseChestPanel, Color.clear, 1f);
        Image closeButtonImage = closeButtonText.GetComponentInParent<Image>();
        if (closeButtonImage != null) closeButtonImage.color = Color.clear;

        chestPanel.transform.SetAsLastSibling();
        chestPanel.SetActive(false);
    }

    private static void ConfigureChestPanelCountText(TMP_Text text, Color color)
    {
        if (text == null) return;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.color = color;
        text.enableAutoSizing = true;
        text.enableWordWrapping = false;
        text.margin = Vector4.zero;
        text.lineSpacing = 0f;
        text.fontSizeMin = 18f;
        text.fontSizeMax = 25f;
        text.outlineColor = new Color32(40, 18, 4, 235);
        text.outlineWidth = 0.18f;
    }

    private TMP_Text CreateChestPanelButton(string objectName, string label, Vector2 position, UnityEngine.Events.UnityAction action, Color textColor)
    {
        TMP_Text text = CreateButton(chestPanel.transform, objectName, label, position, new Vector2(252f, 72f), action, Color.clear, 22f);
        Image image = text.GetComponentInParent<Image>();
        if (image != null) image.color = Color.clear;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.color = textColor;
        text.enableAutoSizing = true;
        text.enableWordWrapping = false;
        text.overflowMode = TextOverflowModes.Ellipsis;
        text.fontSizeMin = 16f;
        text.fontSizeMax = 22f;
        text.margin = new Vector4(18f, 5f, 18f, 5f);
        text.outlineColor = new Color32(43, 16, 3, 245);
        text.outlineWidth = 0.20f;
        return text;
    }

    private void CreateChestRewardPanel(Transform parent)
    {
        chestRewardPanel = new GameObject("ChestRewardPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        chestRewardPanel.transform.SetParent(parent, false);

        RectTransform panelRect = chestRewardPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = new Vector2(0, 15);
        panelRect.sizeDelta = new Vector2(940, 620);

        StylePopupPanel(chestRewardPanel, panelRect.sizeDelta, new Color(0.98f, 0.92f, 0.78f, 0.99f), new Color(0.62f, 0.42f, 0.16f, 1f));

        Transform rewardHeader = chestRewardPanel.transform.Find("PopupHeader");
        if (rewardHeader != null) rewardHeader.gameObject.SetActive(false);
        Transform rewardHeaderLine = chestRewardPanel.transform.Find("PopupHeaderLine");
        if (rewardHeaderLine != null) rewardHeaderLine.gameObject.SetActive(false);

        chestRewardTitleText = CreateText(chestRewardPanel.transform, "ChestRewardTitle", "Sandık Ödülü", new Vector2(0, 258), new Vector2(820, 60), 38);
        StylePopupTitle(chestRewardTitleText, 30);
        chestRewardTitleText.gameObject.SetActive(false);

        GameObject rewardCardObject = new GameObject("ChestRewardCardFrame", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        rewardCardObject.transform.SetParent(chestRewardPanel.transform, false);
        RectTransform rewardCardRect = rewardCardObject.GetComponent<RectTransform>();
        rewardCardRect.anchorMin = new Vector2(0.5f, 0.5f);
        rewardCardRect.anchorMax = new Vector2(0.5f, 0.5f);
        rewardCardRect.anchoredPosition = new Vector2(0f, 55f);
        rewardCardRect.sizeDelta = new Vector2(230f, 230f);
        chestRewardCardFrameImage = rewardCardObject.GetComponent<Image>();
        chestRewardCardFrameImage.sprite = GetChestRewardCardFrameSprite();
        chestRewardCardFrameImage.preserveAspect = true;
        chestRewardCardFrameImage.raycastTarget = false;
        chestRewardCardFrameImage.color = Color.white;
        rewardCardObject.SetActive(false);

        GameObject iconObject = new GameObject("ChestRewardIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        iconObject.transform.SetParent(chestRewardPanel.transform, false);
        RectTransform iconRect = iconObject.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.5f, 0.5f);
        iconRect.anchorMax = new Vector2(0.5f, 0.5f);
        iconRect.anchoredPosition = new Vector2(0, 55);
        iconRect.sizeDelta = new Vector2(210, 210);
        chestRewardIconImage = iconObject.GetComponent<Image>();
        chestRewardIconImage.preserveAspect = true;
        chestRewardIconImage.color = Color.white;
        chestRewardIconCanvasGroup = iconObject.AddComponent<CanvasGroup>();

        chestRewardTrailImages.Clear();
        for (int i = 0; i < 4; i++)
        {
            GameObject trailObject = new GameObject("ChestRewardTrail_" + i, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            trailObject.transform.SetParent(chestRewardPanel.transform, false);
            trailObject.transform.SetSiblingIndex(iconObject.transform.GetSiblingIndex());
            RectTransform trailRect = trailObject.GetComponent<RectTransform>();
            trailRect.anchorMin = new Vector2(0.5f, 0.5f);
            trailRect.anchorMax = new Vector2(0.5f, 0.5f);
            trailRect.anchoredPosition = new Vector2(0f, 55f);
            trailRect.sizeDelta = new Vector2(190f, 190f);
            Image trailImage = trailObject.GetComponent<Image>();
            trailImage.preserveAspect = true;
            trailImage.raycastTarget = false;
            trailImage.color = Color.clear;
            trailObject.SetActive(false);
            chestRewardTrailImages.Add(trailImage);
        }

        chestGlowSprite = CreateChestGlowSprite();
        chestRingSprite = CreateChestRingSprite();
        chestOpeningPlatformImage = CreateChestAnimationEffectImage(chestRewardPanel.transform, "ChestOpeningPlatform", new Vector2(0f, -105f), new Vector2(650f, 180f));
        chestOpeningPlatformImage.gameObject.SetActive(false);
        chestOpeningOrbitImage = CreateChestAnimationEffectImage(chestRewardPanel.transform, "ChestOpeningOrbit", new Vector2(0f, 25f), new Vector2(520f, 520f));
        chestOpeningOrbitImage.sprite = chestRingSprite;
        chestOpeningOrbitImage.gameObject.SetActive(false);
        chestOpeningShockwaveImage = CreateChestAnimationEffectImage(chestRewardPanel.transform, "ChestOpeningShockwave", new Vector2(0f, 25f), new Vector2(560f, 360f));
        chestOpeningShockwaveImage.sprite = chestGlowSprite;
        chestOpeningShockwaveImage.preserveAspect = true;
        chestOpeningShockwaveImage.gameObject.SetActive(false);

        chestOpeningRayImages.Clear();
        Sprite beamSprite = CreateChestBeamSprite();
        for (int i = 0; i < 11; i++)
        {
            Image ray = CreateChestAnimationEffectImage(chestRewardPanel.transform, "ChestOpeningRay_" + i, new Vector2(0f, 42f), new Vector2(22f + ((i % 3) * 8f), 250f + ((i % 4) * 42f)));
            ray.sprite = beamSprite;
            ray.preserveAspect = false;
            ray.rectTransform.pivot = new Vector2(0.5f, 0f);
            ray.gameObject.SetActive(false);
            chestOpeningRayImages.Add(ray);
        }
        chestOpeningGlowImage = CreateChestAnimationEffectImage(chestRewardPanel.transform, "ChestOpeningGlow", new Vector2(0f, 32f), new Vector2(500f, 500f));
        chestOpeningFlashImage = CreateChestAnimationEffectImage(chestRewardPanel.transform, "ChestOpeningFlash", new Vector2(0f, 32f), new Vector2(390f, 390f));
        chestOpeningFlashImage.sprite = CreateChestStarSprite();
        chestOpeningGlowImage.gameObject.SetActive(false);
        chestOpeningFlashImage.gameObject.SetActive(false);

        GameObject openingObject = new GameObject("ChestOpeningVisual", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        openingObject.transform.SetParent(chestRewardPanel.transform, false);
        RectTransform openingRect = openingObject.GetComponent<RectTransform>();
        openingRect.anchorMin = new Vector2(0.5f, 0.5f);
        openingRect.anchorMax = new Vector2(0.5f, 0.5f);
        openingRect.anchoredPosition = new Vector2(0f, 35f);
        openingRect.sizeDelta = new Vector2(390f, 390f);
        chestOpeningImage = openingObject.GetComponent<Image>();
        chestOpeningImage.preserveAspect = true;
        chestOpeningImage.raycastTarget = false;
        openingObject.SetActive(false);

        GameObject transitionObject = new GameObject("ChestOpeningFrameTransition", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        transitionObject.transform.SetParent(openingObject.transform, false);
        RectTransform transitionRect = transitionObject.GetComponent<RectTransform>();
        transitionRect.anchorMin = Vector2.zero;
        transitionRect.anchorMax = Vector2.one;
        transitionRect.offsetMin = Vector2.zero;
        transitionRect.offsetMax = Vector2.zero;
        chestOpeningTransitionImage = transitionObject.GetComponent<Image>();
        chestOpeningTransitionImage.preserveAspect = true;
        chestOpeningTransitionImage.raycastTarget = false;
        transitionObject.SetActive(false);

        GameObject layeredRootObject = new GameObject("ChestLayeredOpeningRoot", typeof(RectTransform));
        layeredRootObject.transform.SetParent(chestRewardPanel.transform, false);
        chestLayeredRootRect = layeredRootObject.GetComponent<RectTransform>();
        chestLayeredRootRect.anchorMin = new Vector2(0.5f, 0.5f);
        chestLayeredRootRect.anchorMax = new Vector2(0.5f, 0.5f);
        chestLayeredRootRect.anchoredPosition = new Vector2(0f, 35f);
        chestLayeredRootRect.sizeDelta = new Vector2(390f, 390f);

        GameObject bodyLayerObject = new GameObject("ChestLayeredBody", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        bodyLayerObject.transform.SetParent(layeredRootObject.transform, false);
        RectTransform bodyLayerRect = bodyLayerObject.GetComponent<RectTransform>();
        bodyLayerRect.anchorMin = Vector2.zero;
        bodyLayerRect.anchorMax = Vector2.one;
        bodyLayerRect.offsetMin = Vector2.zero;
        bodyLayerRect.offsetMax = Vector2.zero;
        chestLayeredBodyImage = bodyLayerObject.GetComponent<Image>();
        chestLayeredBodyImage.preserveAspect = true;
        chestLayeredBodyImage.raycastTarget = false;

        GameObject lidLayerObject = new GameObject("ChestLayeredLid", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        lidLayerObject.transform.SetParent(layeredRootObject.transform, false);
        RectTransform lidLayerRect = lidLayerObject.GetComponent<RectTransform>();
        lidLayerRect.anchorMin = Vector2.zero;
        lidLayerRect.anchorMax = Vector2.one;
        lidLayerRect.offsetMin = Vector2.zero;
        lidLayerRect.offsetMax = Vector2.zero;
        lidLayerRect.pivot = new Vector2(0.5f, 0.46f);
        chestLayeredLidImage = lidLayerObject.GetComponent<Image>();
        chestLayeredLidImage.preserveAspect = true;
        chestLayeredLidImage.raycastTarget = false;
        layeredRootObject.SetActive(false);

        GameObject keyObject = new GameObject("ChestOpeningKeySeal", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        keyObject.transform.SetParent(chestRewardPanel.transform, false);
        RectTransform keyRect = keyObject.GetComponent<RectTransform>();
        keyRect.anchorMin = new Vector2(0.5f, 0.5f);
        keyRect.anchorMax = new Vector2(0.5f, 0.5f);
        keyRect.anchoredPosition = new Vector2(-360f, 100f);
        keyRect.sizeDelta = new Vector2(92f, 92f);
        chestOpeningKeyImage = keyObject.GetComponent<Image>();
        chestOpeningKeyImage.sprite = CreateChestStarSprite();
        chestOpeningKeyImage.preserveAspect = true;
        chestOpeningKeyImage.raycastTarget = false;
        keyObject.SetActive(false);

        chestOpeningSparkleImages.Clear();
        for (int i = 0; i < 24; i++)
        {
            float sparkleSize = 14f + ((i % 4) * 5f);
            Image sparkle = CreateChestAnimationEffectImage(chestRewardPanel.transform, "ChestOpeningSparkle_" + i, new Vector2(0f, 25f), new Vector2(sparkleSize, sparkleSize));
            sparkle.sprite = CreateChestStarSprite();
            sparkle.gameObject.SetActive(false);
            chestOpeningSparkleImages.Add(sparkle);
        }

        chestOpeningTapText = CreateText(chestRewardPanel.transform, "ChestOpeningTapText", "KİLİT AÇILIYOR", new Vector2(0f, -210f), new Vector2(420f, 44f), 24);
        chestOpeningTapText.alignment = TextAlignmentOptions.Center;
        chestOpeningTapText.color = new Color(1f, 0.88f, 0.48f, 1f);
        chestOpeningTapText.gameObject.SetActive(false);

        chestOpeningScreenFlashImage = CreateChestAnimationEffectImage(chestRewardPanel.transform, "ChestOpeningScreenFlash", Vector2.zero, panelRect.sizeDelta);
        chestOpeningScreenFlashImage.sprite = null;
        chestOpeningScreenFlashImage.gameObject.SetActive(false);

        chestRewardText = CreateText(chestRewardPanel.transform, "ChestRewardText", "", new Vector2(0, -135), new Vector2(760, 110), 28);
        StylePopupBodyText(chestRewardText, 20, 27, TextAlignmentOptions.Center);
        chestRewardTextCanvasGroup = chestRewardText.gameObject.AddComponent<CanvasGroup>();

        TMP_Text closeButtonText = StylePopupButton(CreateButton(chestRewardPanel.transform, "ChestRewardCloseButton", "Tamam", new Vector2(0, -255), new Vector2(260, 70), CloseChestRewardPanel, new Color(0.22f, 0.48f, 0.40f), 27), new Color(0.22f, 0.48f, 0.40f), 27);
        chestRewardCloseButtonObject = closeButtonText.transform.parent.gameObject;

        chestOpeningAudioSource = chestRewardPanel.AddComponent<AudioSource>();
        chestOpeningAudioSource.playOnAwake = false;
        chestOpeningAudioSource.loop = false;
        chestOpeningAudioSource.spatialBlend = 0f;
        chestOpeningAudioSource.volume = 0.72f;
        chestKeyImpactClip = CreateChestImpactSound();
        chestOpenCommonClip = CreateChestOpenSound("ChestOpenCommon", 392f, 0.78f, 1861);
        chestOpenRareClip = CreateChestOpenSound("ChestOpenRare", 523.25f, 0.88f, 2713);
        chestOpenLuxuryClip = CreateChestOpenSound("ChestOpenLuxury", 659.25f, 1f, 3919);
        chestRewardLaunchClip = CreateChestRewardLaunchSound();
        chestRewardRevealClip = CreateChestRewardSound();

        chestRewardPanel.transform.SetAsLastSibling();
        chestRewardPanel.SetActive(false);
    }

    public void CloseChestRewardPanel()
    {
        if (chestOpeningCoroutine != null)
        {
            StopCoroutine(chestOpeningCoroutine);
            chestOpeningCoroutine = null;
        }
        chestOpeningInProgress = false;
        shouldAnimateChestReward = false;
        if (chestOpeningImage != null) chestOpeningImage.gameObject.SetActive(false);
        if (chestOpeningTransitionImage != null) chestOpeningTransitionImage.gameObject.SetActive(false);
        if (chestLayeredRootRect != null) chestLayeredRootRect.gameObject.SetActive(false);
        if (chestRewardCardFrameImage != null) chestRewardCardFrameImage.gameObject.SetActive(false);
        if (chestOpeningGlowImage != null) chestOpeningGlowImage.gameObject.SetActive(false);
        if (chestOpeningFlashImage != null) chestOpeningFlashImage.gameObject.SetActive(false);
        if (chestOpeningPlatformImage != null) chestOpeningPlatformImage.gameObject.SetActive(false);
        if (chestOpeningOrbitImage != null) chestOpeningOrbitImage.gameObject.SetActive(false);
        if (chestOpeningShockwaveImage != null) chestOpeningShockwaveImage.gameObject.SetActive(false);
        if (chestOpeningKeyImage != null) chestOpeningKeyImage.gameObject.SetActive(false);
        if (chestOpeningScreenFlashImage != null) chestOpeningScreenFlashImage.gameObject.SetActive(false);
        if (chestOpeningTapText != null) chestOpeningTapText.gameObject.SetActive(false);
        for (int i = 0; i < chestOpeningRayImages.Count; i++)
        {
            if (chestOpeningRayImages[i] != null) chestOpeningRayImages[i].gameObject.SetActive(false);
        }
        for (int i = 0; i < chestOpeningSparkleImages.Count; i++)
        {
            if (chestOpeningSparkleImages[i] != null) chestOpeningSparkleImages[i].gameObject.SetActive(false);
        }
        if (chestRewardIconCanvasGroup != null) chestRewardIconCanvasGroup.alpha = 1f;
        if (chestRewardTextCanvasGroup != null) chestRewardTextCanvasGroup.alpha = 1f;
        for (int i = 0; i < chestRewardTrailImages.Count; i++)
        {
            if (chestRewardTrailImages[i] != null) chestRewardTrailImages[i].gameObject.SetActive(false);
        }
        if (chestOpeningAudioSource != null) chestOpeningAudioSource.Stop();
        if (chestRewardPanel != null)
        {
            chestRewardPanel.SetActive(false);
        }
    }

    private void ShowChestReward(string rewardText, Sprite rewardSprite)
    {
        ShowChestReward(rewardText, rewardSprite, new Color(0.98f, 0.94f, 0.84f, 0.99f));
    }

    private void ShowChestReward(string rewardText, Sprite rewardSprite, Color panelColor)
    {
        if (chestRewardPanel == null)
        {
            chestOpeningInProgress = false;
            shouldAnimateChestReward = false;
            return;
        }

        if (shouldAnimateChestReward)
        {
            shouldAnimateChestReward = false;
            chestOpeningCoroutine = StartCoroutine(PlayChestOpeningAnimationV3(rewardText, rewardSprite, panelColor, openingChestTier));
            return;
        }

        Image panelImage = chestRewardPanel.GetComponent<Image>();
        if (panelImage != null)
        {
            panelImage.color = panelColor;
        }

        Color textColor = IsDarkColor(panelColor) ? Color.white : Color.black;
        if (chestRewardTitleText != null)
        {
            chestRewardTitleText.color = textColor;
        }

        if (chestRewardText != null)
        {
            chestRewardText.text = rewardText;
            chestRewardText.color = textColor;
        }

        if (chestRewardIconImage != null)
        {
            chestRewardIconImage.sprite = rewardSprite;
            chestRewardIconImage.gameObject.SetActive(rewardSprite != null);
        }

        if (chestRewardCardFrameImage != null)
        {
            bool showCardFrame = rewardSprite != null && rewardSprite == GetGemIconSprite();
            chestRewardCardFrameImage.sprite = GetChestRewardCardFrameSprite();
            chestRewardCardFrameImage.rectTransform.anchoredPosition = new Vector2(0f, 55f);
            chestRewardCardFrameImage.rectTransform.localScale = Vector3.one;
            chestRewardCardFrameImage.rectTransform.localRotation = Quaternion.identity;
            chestRewardCardFrameImage.color = Color.white;
            chestRewardCardFrameImage.gameObject.SetActive(showCardFrame);
            if (showCardFrame && chestRewardIconImage != null)
                chestRewardCardFrameImage.transform.SetSiblingIndex(chestRewardIconImage.transform.GetSiblingIndex());
        }

        chestRewardPanel.SetActive(true);
        chestRewardPanel.transform.SetAsLastSibling();
    }

    private IEnumerator PlayChestOpeningAnimationV3(string rewardText, Sprite rewardSprite, Color rewardPanelColor, ChestTier tier)
    {
        Sprite closedSprite = GetChestLayerSprite(tier, "closed");
        Sprite bodySprite = GetChestLayerSprite(tier, "body");
        Sprite lidSprite = GetChestLayerSprite(tier, "lid");
        Sprite[] silhouettes = GetChestSilhouetteFrames(tier);
        if (closedSprite == null || bodySprite == null || lidSprite == null || chestLayeredRootRect == null)
        {
            chestOpeningInProgress = false;
            chestOpeningCoroutine = null;
            ShowChestReward(rewardText, rewardSprite, rewardPanelColor);
            yield break;
        }

        Image panelImage = chestRewardPanel.GetComponent<Image>();
        RectTransform panelRect = chestRewardPanel.GetComponent<RectTransform>();
        Vector2 panelRest = panelRect != null ? panelRect.anchoredPosition : Vector2.zero;
        RectTransform closedRect = chestOpeningImage.rectTransform;
        RectTransform lidRect = chestLayeredLidImage.rectTransform;
        RectTransform bodyRect = chestLayeredBodyImage.rectTransform;
        Vector2 stageCenter = new Vector2(0f, 4f);
        Vector2 openStagePosition = new Vector2(0f, -72f);
        Color primary = tier == ChestTier.Luxury
            ? new Color(1f, 0.25f, 0.06f, 1f)
            : tier == ChestTier.Rare ? new Color(0.08f, 0.92f, 1f, 1f) : new Color(1f, 0.64f, 0.10f, 1f);
        Color accent = tier == ChestTier.Luxury
            ? new Color(1f, 0.88f, 0.22f, 1f)
            : tier == ChestTier.Rare ? new Color(0.72f, 1f, 0.96f, 1f) : new Color(1f, 0.94f, 0.62f, 1f);
        Color darkStage = tier == ChestTier.Luxury
            ? new Color(0.15f, 0.012f, 0.025f, 0.998f)
            : tier == ChestTier.Rare ? new Color(0.008f, 0.09f, 0.16f, 0.998f) : new Color(0.14f, 0.048f, 0.008f, 0.998f);
        int rayCount = tier == ChestTier.Luxury ? 11 : tier == ChestTier.Rare ? 9 : 7;
        float raySpread = tier == ChestTier.Luxury ? 78f : tier == ChestTier.Rare ? 69f : 58f;
        float strength = tier == ChestTier.Luxury ? 1.18f : tier == ChestTier.Rare ? 1.06f : 0.92f;

        if (panelImage != null) panelImage.color = darkStage;
        if (chestRewardTitleText != null) chestRewardTitleText.gameObject.SetActive(false);
        if (chestRewardText != null) chestRewardText.gameObject.SetActive(false);
        if (chestRewardCloseButtonObject != null) chestRewardCloseButtonObject.SetActive(false);
        if (chestRewardIconImage != null) chestRewardIconImage.gameObject.SetActive(false);
        if (chestRewardCardFrameImage != null) chestRewardCardFrameImage.gameObject.SetActive(false);
        if (chestOpeningTransitionImage != null) chestOpeningTransitionImage.gameObject.SetActive(false);
        if (chestLayeredRootRect != null) chestLayeredRootRect.gameObject.SetActive(false);
        if (chestOpeningKeyImage != null) chestOpeningKeyImage.gameObject.SetActive(false);
        if (chestOpeningOrbitImage != null) chestOpeningOrbitImage.gameObject.SetActive(false);
        if (chestOpeningShockwaveImage != null) chestOpeningShockwaveImage.gameObject.SetActive(false);
        if (chestOpeningTapText != null) chestOpeningTapText.gameObject.SetActive(false);
        for (int i = 0; i < chestOpeningSparkleImages.Count; i++) chestOpeningSparkleImages[i].gameObject.SetActive(false);
        for (int i = 0; i < chestRewardTrailImages.Count; i++) chestRewardTrailImages[i].gameObject.SetActive(false);

        chestOpeningImage.sprite = closedSprite;
        chestOpeningImage.color = Color.white;
        closedRect.anchoredPosition = stageCenter + new Vector2(0f, 22f);
        closedRect.localScale = Vector3.one * 0.72f;
        closedRect.localRotation = Quaternion.identity;
        chestOpeningImage.gameObject.SetActive(true);

        chestLayeredBodyImage.sprite = bodySprite;
        chestLayeredBodyImage.color = Color.white;
        chestLayeredLidImage.sprite = lidSprite;
        chestLayeredLidImage.color = Color.white;
        chestLayeredRootRect.anchoredPosition = stageCenter;
        chestLayeredRootRect.localScale = Vector3.one;
        chestLayeredRootRect.localRotation = Quaternion.identity;
        bodyRect.localScale = Vector3.one;
        bodyRect.localRotation = Quaternion.identity;
        lidRect.localScale = new Vector3(1f, 0.10f, 1f);
        lidRect.localRotation = Quaternion.Euler(0f, 0f, -1.5f);

        chestOpeningGlowImage.rectTransform.anchoredPosition = stageCenter + new Vector2(0f, -12f);
        chestOpeningGlowImage.rectTransform.localScale = Vector3.one * 0.42f;
        chestOpeningGlowImage.color = new Color(primary.r, primary.g, primary.b, 0f);
        chestOpeningGlowImage.gameObject.SetActive(true);
        if (chestOpeningPlatformImage != null)
        {
            chestOpeningPlatformImage.rectTransform.anchoredPosition = new Vector2(0f, -130f);
            chestOpeningPlatformImage.rectTransform.localScale = new Vector3(0.52f, 0.36f, 1f);
            chestOpeningPlatformImage.color = new Color(primary.r, primary.g, primary.b, 0f);
            chestOpeningPlatformImage.gameObject.SetActive(true);
        }
        for (int i = 0; i < chestOpeningRayImages.Count; i++)
        {
            Image ray = chestOpeningRayImages[i];
            bool active = i < rayCount;
            ray.gameObject.SetActive(active);
            if (!active) continue;
            float normalized = rayCount <= 1 ? 0.5f : i / (rayCount - 1f);
            float angle = Mathf.Lerp(-raySpread, raySpread, normalized);
            ray.rectTransform.anchoredPosition = stageCenter + new Vector2(0f, 8f);
            ray.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -angle);
            ray.rectTransform.localScale = new Vector3(0.24f, 0.02f, 1f);
            Color rayColor = i % 2 == 0 ? primary : accent;
            ray.color = new Color(rayColor.r, rayColor.g, rayColor.b, 0f);
        }
        if (chestOpeningScreenFlashImage != null)
        {
            chestOpeningScreenFlashImage.color = Color.clear;
            chestOpeningScreenFlashImage.gameObject.SetActive(false);
        }
        chestRewardPanel.SetActive(true);
        chestRewardPanel.transform.SetAsLastSibling();

        float elapsed = 0f;
        const float entranceDuration = 0.26f;
        while (elapsed < entranceDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / entranceDuration);
            float eased = EaseOutBack(t);
            closedRect.anchoredPosition = Vector2.LerpUnclamped(stageCenter + new Vector2(0f, 22f), stageCenter, eased);
            closedRect.localScale = Vector3.one * Mathf.LerpUnclamped(0.72f, 1f, eased);
            yield return null;
        }

        elapsed = 0f;
        const float anticipationDuration = 0.52f;
        while (elapsed < anticipationDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / anticipationDuration);
            float charge = EaseOutCubic(t);
            float bounce = Mathf.Sin(t * Mathf.PI * 3f) * t;
            closedRect.anchoredPosition = stageCenter + new Vector2(0f, Mathf.Lerp(0f, 20f, charge) + (bounce * 4f));
            closedRect.localScale = new Vector3(1f + (bounce * 0.065f), 1f - (bounce * 0.048f), 1f);
            chestOpeningGlowImage.rectTransform.localScale = Vector3.one * Mathf.Lerp(0.42f, 1.04f, charge);
            chestOpeningGlowImage.color = new Color(primary.r, primary.g, primary.b, Mathf.Lerp(0f, 0.40f * strength, charge));
            if (chestOpeningPlatformImage != null)
            {
                chestOpeningPlatformImage.rectTransform.localScale = new Vector3(Mathf.Lerp(0.52f, 0.98f, charge), Mathf.Lerp(0.36f, 0.68f, charge), 1f);
                chestOpeningPlatformImage.color = new Color(primary.r, primary.g, primary.b, Mathf.Lerp(0f, 0.46f, charge));
            }
            for (int i = 0; i < rayCount; i++)
            {
                float stagger = Mathf.Clamp01((t - ((i % 3) * 0.055f)) / 0.78f);
                Image ray = chestOpeningRayImages[i];
                Color rayColor = i % 2 == 0 ? primary : accent;
                ray.rectTransform.localScale = new Vector3(0.30f + ((i % 3) * 0.13f), Mathf.Lerp(0.02f, 0.82f + ((i % 4) * 0.10f), EaseOutCubic(stagger)), 1f);
                ray.color = new Color(rayColor.r, rayColor.g, rayColor.b, stagger * 0.36f * strength);
            }
            yield return null;
        }

        if (chestOpeningAudioSource != null && chestKeyImpactClip != null)
            chestOpeningAudioSource.PlayOneShot(chestKeyImpactClip, 0.66f);
        if (chestOpeningTransitionImage != null && silhouettes != null && silhouettes.Length > 0)
        {
            chestOpeningTransitionImage.sprite = silhouettes[0];
            chestOpeningTransitionImage.color = new Color(1f, 0.98f, 0.86f, 0f);
            chestOpeningTransitionImage.gameObject.SetActive(true);
        }
        if (chestOpeningScreenFlashImage != null)
        {
            chestOpeningScreenFlashImage.gameObject.SetActive(true);
            chestOpeningScreenFlashImage.color = Color.clear;
        }
        elapsed = 0f;
        const float flashDuration = 0.20f;
        while (elapsed < flashDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / flashDuration);
            float pulse = Mathf.Sin(t * Mathf.PI);
            closedRect.localScale = Vector3.one * (1f + (pulse * 0.17f));
            if (chestOpeningTransitionImage != null)
                chestOpeningTransitionImage.color = new Color(1f, 0.98f, 0.86f, Mathf.Clamp01(t * 3.6f));
            if (chestOpeningScreenFlashImage != null)
            {
                float flashStrength = tier == ChestTier.Luxury ? 0.78f : tier == ChestTier.Rare ? 0.66f : 0.54f;
                chestOpeningScreenFlashImage.color = new Color(1f, 0.97f, 0.82f, pulse * flashStrength);
            }
            if (panelRect != null)
            {
                float shake = (1f - t) * (tier == ChestTier.Luxury ? 8f : 5f);
                panelRect.anchoredPosition = panelRest + new Vector2(Mathf.Sin(elapsed * 102f) * shake, Mathf.Cos(elapsed * 76f) * shake * 0.42f);
            }
            yield return null;
        }
        if (panelRect != null) panelRect.anchoredPosition = panelRest;

        chestOpeningImage.gameObject.SetActive(false);
        chestLayeredRootRect.anchoredPosition = stageCenter + new Vector2(0f, 20f);
        chestLayeredRootRect.localScale = Vector3.one * 1.08f;
        chestLayeredRootRect.gameObject.SetActive(true);
        lidRect.localScale = new Vector3(1f, 0.10f, 1f);
        lidRect.localRotation = Quaternion.Euler(0f, 0f, -1.5f);
        bodyRect.localScale = new Vector3(1.05f, 0.93f, 1f);
        if (chestOpeningTransitionImage != null) chestOpeningTransitionImage.gameObject.SetActive(false);

        AudioClip openClip = tier == ChestTier.Luxury ? chestOpenLuxuryClip : tier == ChestTier.Rare ? chestOpenRareClip : chestOpenCommonClip;
        if (chestOpeningAudioSource != null && openClip != null) chestOpeningAudioSource.PlayOneShot(openClip, 0.86f);
        elapsed = 0f;
        const float lidOpenDuration = 0.43f;
        while (elapsed < lidOpenDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / lidOpenDuration);
            float eased = EaseOutBack(t);
            float lidProgress = Mathf.LerpUnclamped(0.10f, 1f, eased);
            lidRect.localScale = new Vector3(1f, lidProgress, 1f);
            lidRect.localRotation = Quaternion.Euler(0f, 0f, Mathf.LerpUnclamped(-1.5f, 0f, eased));
            float bodyImpact = Mathf.Sin(t * Mathf.PI);
            bodyRect.localScale = new Vector3(1f + (bodyImpact * 0.045f), Mathf.LerpUnclamped(0.93f, 1f, eased) - (bodyImpact * 0.025f), 1f);
            chestLayeredRootRect.anchoredPosition = Vector2.LerpUnclamped(stageCenter + new Vector2(0f, 20f), openStagePosition, eased);
            chestLayeredRootRect.localScale = Vector3.one * Mathf.LerpUnclamped(1.08f, 0.92f, eased);
            for (int i = 0; i < rayCount; i++)
            {
                Image ray = chestOpeningRayImages[i];
                Color rayColor = i % 2 == 0 ? primary : accent;
                ray.rectTransform.localScale = new Vector3(0.42f + ((i % 3) * 0.13f), Mathf.Lerp(0.80f, 1.42f * strength, EaseOutCubic(t)), 1f);
                ray.color = new Color(rayColor.r, rayColor.g, rayColor.b, Mathf.Lerp(0.50f, 0.30f, t));
            }
            if (chestOpeningScreenFlashImage != null)
                chestOpeningScreenFlashImage.color = new Color(1f, 0.97f, 0.82f, Mathf.Lerp(0.32f, 0f, t));
            yield return null;
        }
        lidRect.localScale = Vector3.one;
        lidRect.localRotation = Quaternion.identity;
        bodyRect.localScale = Vector3.one;
        if (chestOpeningScreenFlashImage != null) chestOpeningScreenFlashImage.gameObject.SetActive(false);

        Vector2 rewardStart = openStagePosition + new Vector2(0f, 42f);
        Vector2 rewardPeak = new Vector2(tier == ChestTier.Rare ? 24f : tier == ChestTier.Luxury ? -16f : 0f, 168f);
        Vector2 rewardRest = new Vector2(0f, 55f);
        bool showRewardCard = rewardSprite != null && rewardSprite == GetGemIconSprite() && chestRewardCardFrameImage != null;
        if (rewardSprite != null && chestRewardIconImage != null)
        {
            for (int i = 0; i < chestRewardTrailImages.Count; i++)
            {
                Image trail = chestRewardTrailImages[i];
                trail.sprite = CreateChestBeamSprite();
                trail.preserveAspect = false;
                trail.rectTransform.sizeDelta = new Vector2(14f + (i * 5f), 138f + (i * 24f));
                trail.rectTransform.pivot = new Vector2(0.5f, 1f);
                trail.rectTransform.anchoredPosition = rewardStart;
                trail.rectTransform.localScale = Vector3.one;
                trail.rectTransform.localRotation = Quaternion.identity;
                trail.color = Color.clear;
                trail.transform.SetAsLastSibling();
                trail.gameObject.SetActive(true);
            }
            if (showRewardCard)
            {
                chestRewardCardFrameImage.sprite = GetChestRewardCardFrameSprite();
                chestRewardCardFrameImage.rectTransform.anchoredPosition = rewardStart;
                chestRewardCardFrameImage.rectTransform.localScale = Vector3.one * 0.18f;
                chestRewardCardFrameImage.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -10f);
                chestRewardCardFrameImage.color = new Color(1f, 1f, 1f, 0f);
                chestRewardCardFrameImage.transform.SetAsLastSibling();
                chestRewardCardFrameImage.gameObject.SetActive(true);
            }
            chestRewardIconImage.sprite = rewardSprite;
            chestRewardIconImage.color = Color.white;
            chestRewardIconImage.rectTransform.anchoredPosition = rewardStart;
            chestRewardIconImage.rectTransform.localScale = Vector3.one * 0.18f;
            chestRewardIconImage.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -10f);
            if (chestRewardIconCanvasGroup != null) chestRewardIconCanvasGroup.alpha = 0f;
            chestRewardIconImage.transform.SetAsLastSibling();
            chestRewardIconImage.gameObject.SetActive(true);
        }

        if (chestOpeningAudioSource != null && chestRewardLaunchClip != null)
            chestOpeningAudioSource.PlayOneShot(chestRewardLaunchClip, tier == ChestTier.Luxury ? 0.90f : tier == ChestTier.Rare ? 0.78f : 0.66f);

        elapsed = 0f;
        const float rewardLaunchDuration = 0.62f;
        while (elapsed < rewardLaunchDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / rewardLaunchDuration);
            float eased = EaseOutCubic(t);
            float lateralArc = Mathf.Sin(t * Mathf.PI) * (tier == ChestTier.Rare ? 28f : tier == ChestTier.Luxury ? -20f : 0f);
            Vector2 iconPosition = Vector2.Lerp(rewardStart, rewardPeak, eased) + new Vector2(lateralArc, 0f);
            if (rewardSprite != null && chestRewardIconImage != null)
            {
                chestRewardIconImage.rectTransform.anchoredPosition = iconPosition;
                chestRewardIconImage.rectTransform.localScale = Vector3.one * Mathf.LerpUnclamped(0.18f, 1.08f, EaseOutBack(t));
                chestRewardIconImage.rectTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(-10f, 4f, eased));
                if (chestRewardIconCanvasGroup != null) chestRewardIconCanvasGroup.alpha = Mathf.Clamp01(t * 6f);
                if (showRewardCard)
                {
                    chestRewardCardFrameImage.rectTransform.anchoredPosition = iconPosition;
                    chestRewardCardFrameImage.rectTransform.localScale = chestRewardIconImage.rectTransform.localScale;
                    chestRewardCardFrameImage.rectTransform.localRotation = chestRewardIconImage.rectTransform.localRotation;
                    chestRewardCardFrameImage.color = new Color(1f, 1f, 1f, Mathf.Clamp01(t * 6f));
                }
                for (int i = 0; i < chestRewardTrailImages.Count; i++)
                {
                    Image trail = chestRewardTrailImages[i];
                    float delay = (i + 1) * 0.045f;
                    float delayedT = Mathf.Clamp01((t - delay) / (1f - delay));
                    float delayedEase = EaseOutCubic(delayedT);
                    float trailArc = Mathf.Sin(delayedT * Mathf.PI) * (tier == ChestTier.Rare ? 28f : tier == ChestTier.Luxury ? -20f : 0f);
                    Vector2 trailTop = Vector2.Lerp(rewardStart, rewardPeak, delayedEase) + new Vector2(trailArc, -6f - (i * 8f));
                    trail.rectTransform.anchoredPosition = trailTop;
                    float alpha = Mathf.Sin(delayedT * Mathf.PI) * (0.52f - (i * 0.075f));
                    Color trailColor = i % 2 == 0 ? accent : primary;
                    trail.color = new Color(trailColor.r, trailColor.g, trailColor.b, alpha);
                }
            }
            for (int i = 0; i < rayCount; i++)
            {
                Image ray = chestOpeningRayImages[i];
                Color rayColor = i % 2 == 0 ? primary : accent;
                ray.color = new Color(rayColor.r, rayColor.g, rayColor.b, Mathf.Lerp(0.30f, 0f, t));
            }
            float luxuryPulse = tier == ChestTier.Luxury ? Mathf.Sin(Mathf.Clamp01((t - 0.34f) / 0.44f) * Mathf.PI) * 0.28f : 0f;
            chestOpeningGlowImage.color = new Color(primary.r, primary.g, primary.b, Mathf.Lerp(0.50f, 0.08f, t) + luxuryPulse);
            yield return null;
        }

        elapsed = 0f;
        const float settleDuration = 0.34f;
        while (elapsed < settleDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / settleDuration);
            float eased = EaseOutBack(t);
            if (rewardSprite != null && chestRewardIconImage != null)
            {
                chestRewardIconImage.rectTransform.anchoredPosition = Vector2.LerpUnclamped(rewardPeak, rewardRest, eased);
                chestRewardIconImage.rectTransform.localScale = Vector3.one * Mathf.LerpUnclamped(1.08f, 1f, eased);
                chestRewardIconImage.rectTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.LerpUnclamped(4f, 0f, eased));
                if (showRewardCard)
                {
                    chestRewardCardFrameImage.rectTransform.anchoredPosition = chestRewardIconImage.rectTransform.anchoredPosition;
                    chestRewardCardFrameImage.rectTransform.localScale = chestRewardIconImage.rectTransform.localScale;
                    chestRewardCardFrameImage.rectTransform.localRotation = chestRewardIconImage.rectTransform.localRotation;
                }
            }
            for (int i = 0; i < chestRewardTrailImages.Count; i++)
            {
                Color color = chestRewardTrailImages[i].color;
                color.a = Mathf.Lerp(color.a, 0f, t);
                chestRewardTrailImages[i].color = color;
            }
            chestLayeredRootRect.localScale = Vector3.one * Mathf.Lerp(0.92f, 0.86f, t);
            if (panelImage != null && t > 0.38f)
                panelImage.color = Color.Lerp(darkStage, rewardPanelColor, (t - 0.38f) / 0.62f);
            yield return null;
        }

        for (int i = 0; i < chestRewardTrailImages.Count; i++) chestRewardTrailImages[i].gameObject.SetActive(false);
        for (int i = 0; i < chestOpeningRayImages.Count; i++) chestOpeningRayImages[i].gameObject.SetActive(false);
        if (chestOpeningPlatformImage != null) chestOpeningPlatformImage.gameObject.SetActive(false);
        if (chestOpeningGlowImage != null) chestOpeningGlowImage.gameObject.SetActive(false);
        chestLayeredRootRect.gameObject.SetActive(false);
        if (chestRewardTitleText != null) chestRewardTitleText.gameObject.SetActive(false);
        if (chestRewardText != null) chestRewardText.gameObject.SetActive(true);
        ShowChestReward(rewardText, rewardSprite, rewardPanelColor);
        if (chestRewardIconImage != null)
        {
            chestRewardIconImage.rectTransform.anchoredPosition = rewardRest;
            chestRewardIconImage.rectTransform.localScale = Vector3.one;
            chestRewardIconImage.rectTransform.localRotation = Quaternion.identity;
            if (showRewardCard)
            {
                chestRewardCardFrameImage.rectTransform.anchoredPosition = rewardRest;
                chestRewardCardFrameImage.rectTransform.localScale = Vector3.one;
                chestRewardCardFrameImage.rectTransform.localRotation = Quaternion.identity;
                chestRewardCardFrameImage.transform.SetAsLastSibling();
                chestRewardIconImage.transform.SetAsLastSibling();
            }
        }
        if (chestRewardTextCanvasGroup != null) chestRewardTextCanvasGroup.alpha = 0f;
        if (chestRewardCloseButtonObject != null) chestRewardCloseButtonObject.SetActive(false);
        if (chestOpeningAudioSource != null && chestRewardRevealClip != null)
            chestOpeningAudioSource.PlayOneShot(chestRewardRevealClip, tier == ChestTier.Luxury ? 0.84f : 0.68f);
        elapsed = 0f;
        const float revealDuration = 0.18f;
        while (elapsed < revealDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / revealDuration);
            if (chestRewardTextCanvasGroup != null) chestRewardTextCanvasGroup.alpha = EaseOutCubic(t);
            yield return null;
        }
        if (chestRewardTextCanvasGroup != null) chestRewardTextCanvasGroup.alpha = 1f;
        if (chestRewardCloseButtonObject != null) chestRewardCloseButtonObject.SetActive(true);
        chestOpeningInProgress = false;
        chestOpeningCoroutine = null;
    }

    private IEnumerator PlayChestOpeningAnimationV2(string rewardText, Sprite rewardSprite, Color rewardPanelColor, ChestTier tier)
    {
        Sprite[] frames = GetChestAnimationFrames(tier);
        Sprite[] silhouettes = GetChestSilhouetteFrames(tier);
        if (frames == null || frames.Length < 4 || chestOpeningImage == null || chestOpeningGlowImage == null)
        {
            chestOpeningInProgress = false;
            chestOpeningCoroutine = null;
            ShowChestReward(rewardText, rewardSprite, rewardPanelColor);
            yield break;
        }

        Image panelImage = chestRewardPanel.GetComponent<Image>();
        RectTransform panelRect = chestRewardPanel.GetComponent<RectTransform>();
        Vector2 panelRestPosition = panelRect != null ? panelRect.anchoredPosition : Vector2.zero;
        RectTransform chestRect = chestOpeningImage.rectTransform;
        RectTransform glowRect = chestOpeningGlowImage.rectTransform;
        Vector2 chestCenter = new Vector2(0f, -5f);
        Vector2 openChestPosition = new Vector2(0f, -92f);
        Color primary = tier == ChestTier.Luxury
            ? new Color(1f, 0.28f, 0.08f, 1f)
            : tier == ChestTier.Rare ? new Color(0.10f, 0.90f, 1f, 1f) : new Color(1f, 0.67f, 0.12f, 1f);
        Color accent = tier == ChestTier.Luxury
            ? new Color(1f, 0.86f, 0.22f, 1f)
            : tier == ChestTier.Rare ? new Color(0.72f, 1f, 0.96f, 1f) : new Color(1f, 0.93f, 0.62f, 1f);
        Color openingPanelColor = tier == ChestTier.Luxury
            ? new Color(0.19f, 0.025f, 0.035f, 0.995f)
            : tier == ChestTier.Rare ? new Color(0.015f, 0.13f, 0.20f, 0.995f) : new Color(0.18f, 0.075f, 0.018f, 0.995f);
        int activeRayCount = tier == ChestTier.Luxury ? 11 : tier == ChestTier.Rare ? 9 : 7;
        float raySpread = tier == ChestTier.Luxury ? 76f : tier == ChestTier.Rare ? 68f : 56f;
        float intensity = tier == ChestTier.Luxury ? 1.18f : tier == ChestTier.Rare ? 1.06f : 0.92f;

        if (panelImage != null) panelImage.color = openingPanelColor;
        if (chestRewardTitleText != null) chestRewardTitleText.gameObject.SetActive(false);
        if (chestRewardText != null) chestRewardText.gameObject.SetActive(false);
        if (chestRewardCloseButtonObject != null) chestRewardCloseButtonObject.SetActive(false);
        if (chestRewardIconImage != null) chestRewardIconImage.gameObject.SetActive(false);
        if (chestOpeningKeyImage != null) chestOpeningKeyImage.gameObject.SetActive(false);
        if (chestOpeningOrbitImage != null) chestOpeningOrbitImage.gameObject.SetActive(false);
        if (chestOpeningShockwaveImage != null) chestOpeningShockwaveImage.gameObject.SetActive(false);
        if (chestOpeningFlashImage != null) chestOpeningFlashImage.gameObject.SetActive(false);
        if (chestOpeningTapText != null) chestOpeningTapText.gameObject.SetActive(false);
        for (int i = 0; i < chestOpeningSparkleImages.Count; i++) chestOpeningSparkleImages[i].gameObject.SetActive(false);
        for (int i = 0; i < chestRewardTrailImages.Count; i++) chestRewardTrailImages[i].gameObject.SetActive(false);

        chestOpeningImage.sprite = frames[0];
        chestOpeningImage.color = Color.white;
        chestRect.anchoredPosition = chestCenter + new Vector2(0f, 18f);
        chestRect.localScale = Vector3.one * 0.74f;
        chestRect.localRotation = Quaternion.identity;
        chestOpeningImage.gameObject.SetActive(true);
        if (chestOpeningTransitionImage != null) chestOpeningTransitionImage.gameObject.SetActive(false);

        glowRect.anchoredPosition = chestCenter + new Vector2(0f, -8f);
        glowRect.localScale = Vector3.one * 0.48f;
        chestOpeningGlowImage.color = new Color(primary.r, primary.g, primary.b, 0f);
        chestOpeningGlowImage.gameObject.SetActive(true);
        if (chestOpeningPlatformImage != null)
        {
            chestOpeningPlatformImage.rectTransform.anchoredPosition = new Vector2(0f, -126f);
            chestOpeningPlatformImage.rectTransform.localScale = new Vector3(0.62f, 0.42f, 1f);
            chestOpeningPlatformImage.color = new Color(primary.r, primary.g, primary.b, 0f);
            chestOpeningPlatformImage.gameObject.SetActive(true);
        }
        for (int i = 0; i < chestOpeningRayImages.Count; i++)
        {
            Image ray = chestOpeningRayImages[i];
            bool active = i < activeRayCount;
            ray.gameObject.SetActive(active);
            if (!active) continue;
            float normalized = activeRayCount <= 1 ? 0.5f : i / (activeRayCount - 1f);
            float angle = Mathf.Lerp(-raySpread, raySpread, normalized);
            ray.rectTransform.anchoredPosition = chestCenter + new Vector2(0f, 22f);
            ray.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -angle);
            ray.rectTransform.localScale = new Vector3(0.22f, 0.05f, 1f);
            Color rayColor = i % 2 == 0 ? primary : accent;
            ray.color = new Color(rayColor.r, rayColor.g, rayColor.b, 0f);
        }
        if (chestOpeningScreenFlashImage != null)
        {
            chestOpeningScreenFlashImage.color = Color.clear;
            chestOpeningScreenFlashImage.gameObject.SetActive(false);
        }
        chestRewardPanel.SetActive(true);
        chestRewardPanel.transform.SetAsLastSibling();

        float elapsed = 0f;
        const float entranceDuration = 0.24f;
        while (elapsed < entranceDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / entranceDuration);
            float eased = EaseOutBack(t);
            chestRect.anchoredPosition = Vector2.LerpUnclamped(chestCenter + new Vector2(0f, 18f), chestCenter, eased);
            chestRect.localScale = Vector3.one * Mathf.LerpUnclamped(0.74f, 1f, eased);
            yield return null;
        }
        yield return new WaitForSecondsRealtime(0.10f);

        if (chestOpeningTransitionImage != null)
        {
            chestOpeningTransitionImage.sprite = frames[1];
            chestOpeningTransitionImage.color = new Color(1f, 1f, 1f, 0f);
            chestOpeningTransitionImage.gameObject.SetActive(true);
        }
        elapsed = 0f;
        const float chargeDuration = 0.58f;
        while (elapsed < chargeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / chargeDuration);
            float pulse = Mathf.Sin(t * Mathf.PI * 3f);
            float charge = EaseOutCubic(t);
            chestRect.anchoredPosition = chestCenter + new Vector2(0f, Mathf.Lerp(0f, 18f, charge) + (pulse * 3f * t));
            chestRect.localScale = new Vector3(1f + (pulse * 0.055f * t), 1f - (pulse * 0.040f * t), 1f);
            glowRect.localScale = Vector3.one * Mathf.Lerp(0.48f, 1.08f, charge);
            chestOpeningGlowImage.color = new Color(primary.r, primary.g, primary.b, Mathf.Lerp(0f, 0.48f * intensity, charge));
            if (chestOpeningTransitionImage != null)
                chestOpeningTransitionImage.color = new Color(1f, 1f, 1f, Mathf.Clamp01((t - 0.32f) / 0.30f));
            if (chestOpeningPlatformImage != null)
            {
                chestOpeningPlatformImage.rectTransform.localScale = new Vector3(Mathf.Lerp(0.62f, 1f, charge), Mathf.Lerp(0.42f, 0.72f, charge), 1f);
                chestOpeningPlatformImage.color = new Color(primary.r, primary.g, primary.b, Mathf.Lerp(0f, 0.52f, charge));
            }
            for (int i = 0; i < activeRayCount; i++)
            {
                Image ray = chestOpeningRayImages[i];
                float stagger = Mathf.Clamp01((t - ((i % 3) * 0.06f)) / 0.76f);
                Color rayColor = i % 2 == 0 ? primary : accent;
                ray.rectTransform.localScale = new Vector3(0.32f + ((i % 3) * 0.12f), Mathf.Lerp(0.05f, 0.88f + ((i % 4) * 0.11f), EaseOutCubic(stagger)), 1f);
                ray.color = new Color(rayColor.r, rayColor.g, rayColor.b, stagger * 0.42f * intensity);
            }
            yield return null;
        }
        chestOpeningImage.sprite = frames[1];
        chestOpeningImage.color = Color.white;
        if (chestOpeningTransitionImage != null) chestOpeningTransitionImage.gameObject.SetActive(false);

        if (chestOpeningAudioSource != null && chestKeyImpactClip != null)
            chestOpeningAudioSource.PlayOneShot(chestKeyImpactClip, 0.62f);
        if (chestOpeningTransitionImage != null && silhouettes != null && silhouettes.Length > 1)
        {
            chestOpeningTransitionImage.sprite = silhouettes[1];
            chestOpeningTransitionImage.color = new Color(1f, 1f, 1f, 0f);
            chestOpeningTransitionImage.gameObject.SetActive(true);
        }
        if (chestOpeningScreenFlashImage != null)
        {
            chestOpeningScreenFlashImage.gameObject.SetActive(true);
            chestOpeningScreenFlashImage.color = Color.clear;
        }
        elapsed = 0f;
        const float whiteFlashDuration = 0.22f;
        while (elapsed < whiteFlashDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / whiteFlashDuration);
            float flashPulse = Mathf.Sin(t * Mathf.PI);
            chestRect.localScale = Vector3.one * (1f + (flashPulse * 0.15f));
            if (chestOpeningTransitionImage != null)
                chestOpeningTransitionImage.color = new Color(1f, 0.98f, 0.88f, Mathf.Clamp01(t * 2.8f));
            if (chestOpeningScreenFlashImage != null)
            {
                float maxFlash = tier == ChestTier.Luxury ? 0.78f : tier == ChestTier.Rare ? 0.68f : 0.58f;
                chestOpeningScreenFlashImage.color = new Color(1f, 0.97f, 0.84f, flashPulse * maxFlash);
            }
            if (panelRect != null)
            {
                float shake = (1f - t) * (tier == ChestTier.Luxury ? 8f : 5f);
                panelRect.anchoredPosition = panelRestPosition + new Vector2(Mathf.Sin(elapsed * 96f) * shake, Mathf.Cos(elapsed * 71f) * shake * 0.45f);
            }
            yield return null;
        }
        if (panelRect != null) panelRect.anchoredPosition = panelRestPosition;

        AudioClip tierClip = tier == ChestTier.Luxury ? chestOpenLuxuryClip : tier == ChestTier.Rare ? chestOpenRareClip : chestOpenCommonClip;
        if (chestOpeningAudioSource != null && tierClip != null) chestOpeningAudioSource.PlayOneShot(tierClip, 0.82f);
        chestOpeningImage.sprite = frames[2];
        if (chestOpeningTransitionImage != null && silhouettes != null && silhouettes.Length > 2)
            chestOpeningTransitionImage.sprite = silhouettes[2];
        elapsed = 0f;
        const float openDropDuration = 0.28f;
        while (elapsed < openDropDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / openDropDuration);
            float eased = EaseOutBack(t);
            if (t > 0.42f) chestOpeningImage.sprite = frames[3];
            chestRect.anchoredPosition = Vector2.LerpUnclamped(chestCenter + new Vector2(0f, 18f), openChestPosition, eased);
            chestRect.localScale = Vector3.one * Mathf.LerpUnclamped(1.12f, 0.90f, eased);
            if (chestOpeningTransitionImage != null)
                chestOpeningTransitionImage.color = new Color(1f, 0.98f, 0.88f, Mathf.Lerp(1f, 0f, EaseOutCubic(t)));
            for (int i = 0; i < activeRayCount; i++)
            {
                Image ray = chestOpeningRayImages[i];
                Color rayColor = i % 2 == 0 ? primary : accent;
                ray.rectTransform.localScale = new Vector3(0.42f + ((i % 3) * 0.12f), Mathf.Lerp(0.9f, 1.38f * intensity, EaseOutCubic(t)), 1f);
                ray.color = new Color(rayColor.r, rayColor.g, rayColor.b, Mathf.Lerp(0.62f, 0.34f, t));
            }
            yield return null;
        }
        if (chestOpeningTransitionImage != null) chestOpeningTransitionImage.gameObject.SetActive(false);
        if (chestOpeningScreenFlashImage != null) chestOpeningScreenFlashImage.gameObject.SetActive(false);

        Vector2 rewardStart = openChestPosition + new Vector2(0f, 28f);
        Vector2 rewardPeak = new Vector2(tier == ChestTier.Rare ? 26f : tier == ChestTier.Luxury ? -18f : 0f, 162f);
        Vector2 rewardRest = new Vector2(0f, 55f);
        if (rewardSprite != null && chestRewardIconImage != null)
        {
            chestRewardIconImage.sprite = rewardSprite;
            chestRewardIconImage.color = Color.white;
            chestRewardIconImage.rectTransform.anchoredPosition = rewardStart;
            chestRewardIconImage.rectTransform.localScale = Vector3.one * 0.20f;
            chestRewardIconImage.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -12f);
            if (chestRewardIconCanvasGroup != null) chestRewardIconCanvasGroup.alpha = 0f;
            chestRewardIconImage.gameObject.SetActive(true);
            for (int i = 0; i < chestRewardTrailImages.Count; i++)
            {
                Image trail = chestRewardTrailImages[i];
                trail.sprite = rewardSprite;
                trail.rectTransform.anchoredPosition = rewardStart;
                trail.rectTransform.localScale = Vector3.one * (0.16f + (i * 0.025f));
                trail.color = Color.clear;
                trail.gameObject.SetActive(true);
            }
        }

        elapsed = 0f;
        const float rewardLaunchDuration = 0.66f;
        while (elapsed < rewardLaunchDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / rewardLaunchDuration);
            float eased = EaseOutCubic(t);
            float arc = Mathf.Sin(t * Mathf.PI) * (tier == ChestTier.Rare ? 34f : tier == ChestTier.Luxury ? -24f : 0f);
            Vector2 rewardPosition = Vector2.Lerp(rewardStart, rewardPeak, eased) + new Vector2(arc, 0f);
            if (chestRewardIconImage != null && rewardSprite != null)
            {
                chestRewardIconImage.rectTransform.anchoredPosition = rewardPosition;
                chestRewardIconImage.rectTransform.localScale = Vector3.one * Mathf.LerpUnclamped(0.20f, 1.08f, EaseOutBack(t));
                chestRewardIconImage.rectTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(-12f, 5f, eased));
                if (chestRewardIconCanvasGroup != null) chestRewardIconCanvasGroup.alpha = Mathf.Clamp01(t * 5f);
                for (int i = 0; i < chestRewardTrailImages.Count; i++)
                {
                    float delayedT = Mathf.Clamp01(t - ((i + 1) * 0.055f));
                    float delayedEase = EaseOutCubic(delayedT);
                    float delayedArc = Mathf.Sin(delayedT * Mathf.PI) * (tier == ChestTier.Rare ? 34f : tier == ChestTier.Luxury ? -24f : 0f);
                    Image trail = chestRewardTrailImages[i];
                    trail.rectTransform.anchoredPosition = Vector2.Lerp(rewardStart, rewardPeak, delayedEase) + new Vector2(delayedArc, -8f - (i * 7f));
                    trail.rectTransform.localScale = Vector3.one * Mathf.Lerp(0.18f, 0.82f, delayedEase);
                    float alpha = Mathf.Sin(Mathf.Clamp01(delayedT) * Mathf.PI) * (0.30f - (i * 0.045f));
                    Color trailColor = i % 2 == 0 ? accent : primary;
                    trail.color = new Color(trailColor.r, trailColor.g, trailColor.b, alpha);
                }
            }
            for (int i = 0; i < activeRayCount; i++)
            {
                Image ray = chestOpeningRayImages[i];
                Color rayColor = i % 2 == 0 ? primary : accent;
                ray.color = new Color(rayColor.r, rayColor.g, rayColor.b, Mathf.Lerp(0.38f, 0f, t));
            }
            if (chestOpeningGlowImage != null)
            {
                float secondPulse = tier == ChestTier.Luxury ? Mathf.Sin(Mathf.Clamp01((t - 0.36f) / 0.42f) * Mathf.PI) * 0.34f : 0f;
                chestOpeningGlowImage.color = new Color(primary.r, primary.g, primary.b, Mathf.Lerp(0.56f, 0.10f, t) + secondPulse);
            }
            yield return null;
        }

        elapsed = 0f;
        const float settleDuration = 0.36f;
        while (elapsed < settleDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / settleDuration);
            float eased = EaseOutBack(t);
            if (chestRewardIconImage != null && rewardSprite != null)
            {
                chestRewardIconImage.rectTransform.anchoredPosition = Vector2.LerpUnclamped(rewardPeak, rewardRest, eased);
                chestRewardIconImage.rectTransform.localScale = Vector3.one * Mathf.LerpUnclamped(1.08f, 1f, eased);
                chestRewardIconImage.rectTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.LerpUnclamped(5f, 0f, eased));
            }
            for (int i = 0; i < chestRewardTrailImages.Count; i++)
            {
                Color current = chestRewardTrailImages[i].color;
                current.a = Mathf.Lerp(current.a, 0f, t);
                chestRewardTrailImages[i].color = current;
            }
            if (panelImage != null && t > 0.42f)
                panelImage.color = Color.Lerp(openingPanelColor, rewardPanelColor, (t - 0.42f) / 0.58f);
            yield return null;
        }

        for (int i = 0; i < chestRewardTrailImages.Count; i++) chestRewardTrailImages[i].gameObject.SetActive(false);
        for (int i = 0; i < chestOpeningRayImages.Count; i++) chestOpeningRayImages[i].gameObject.SetActive(false);
        if (chestOpeningPlatformImage != null) chestOpeningPlatformImage.gameObject.SetActive(false);
        if (chestOpeningGlowImage != null) chestOpeningGlowImage.gameObject.SetActive(false);
        chestOpeningImage.gameObject.SetActive(false);
        if (chestRewardTitleText != null) chestRewardTitleText.gameObject.SetActive(false);
        if (chestRewardText != null) chestRewardText.gameObject.SetActive(true);
        ShowChestReward(rewardText, rewardSprite, rewardPanelColor);
        if (chestRewardIconImage != null)
        {
            chestRewardIconImage.rectTransform.anchoredPosition = rewardRest;
            chestRewardIconImage.rectTransform.localScale = Vector3.one;
            chestRewardIconImage.rectTransform.localRotation = Quaternion.identity;
        }
        if (chestRewardTextCanvasGroup != null) chestRewardTextCanvasGroup.alpha = 0f;
        if (chestRewardCloseButtonObject != null) chestRewardCloseButtonObject.SetActive(false);
        if (chestOpeningAudioSource != null && chestRewardRevealClip != null)
            chestOpeningAudioSource.PlayOneShot(chestRewardRevealClip, tier == ChestTier.Luxury ? 0.84f : 0.68f);
        elapsed = 0f;
        const float uiRevealDuration = 0.20f;
        while (elapsed < uiRevealDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / uiRevealDuration);
            if (chestRewardTextCanvasGroup != null) chestRewardTextCanvasGroup.alpha = EaseOutCubic(t);
            yield return null;
        }
        if (chestRewardTextCanvasGroup != null) chestRewardTextCanvasGroup.alpha = 1f;
        if (chestRewardCloseButtonObject != null) chestRewardCloseButtonObject.SetActive(true);
        chestOpeningInProgress = false;
        chestOpeningCoroutine = null;
    }

    private IEnumerator PlayChestOpeningAnimation(string rewardText, Sprite rewardSprite, Color rewardPanelColor, ChestTier tier)
    {
        Sprite[] frames = GetChestAnimationFrames(tier);
        if (frames == null || frames.Length < 4 || chestOpeningImage == null || chestOpeningGlowImage == null)
        {
            chestOpeningInProgress = false;
            chestOpeningCoroutine = null;
            ShowChestReward(rewardText, rewardSprite, rewardPanelColor);
            yield break;
        }

        Image panelImage = chestRewardPanel.GetComponent<Image>();
        RectTransform rewardPanelRect = chestRewardPanel.GetComponent<RectTransform>();
        Vector2 rewardPanelRestPosition = rewardPanelRect != null ? rewardPanelRect.anchoredPosition : Vector2.zero;
        Color openingColor = tier == ChestTier.Luxury
            ? new Color(0.40f, 0.08f, 0.08f, 0.99f)
            : tier == ChestTier.Rare ? new Color(0.05f, 0.28f, 0.31f, 0.99f) : new Color(0.34f, 0.18f, 0.07f, 0.99f);
        if (panelImage != null) panelImage.color = openingColor;
        if (chestRewardTitleText != null)
        {
            chestRewardTitleText.text = tier == ChestTier.Luxury
                ? "ÖZEL SANDIK"
                : tier == ChestTier.Rare ? "NADİR SANDIK" : "SIRADAN SANDIK";
            chestRewardTitleText.color = Color.white;
        }
        if (chestRewardText != null) chestRewardText.gameObject.SetActive(false);
        if (chestRewardIconImage != null) chestRewardIconImage.gameObject.SetActive(false);
        if (chestRewardCloseButtonObject != null) chestRewardCloseButtonObject.SetActive(false);

        RectTransform openingRect = chestOpeningImage.rectTransform;
        Vector2 basePosition = new Vector2(0f, 35f);
        RectTransform glowRect = chestOpeningGlowImage.rectTransform;
        RectTransform flashRect = chestOpeningFlashImage != null ? chestOpeningFlashImage.rectTransform : null;
        Color tierGlowColor = tier == ChestTier.Luxury
            ? new Color(1f, 0.32f, 0.08f, 1f)
            : tier == ChestTier.Rare ? new Color(0.16f, 0.95f, 1f, 1f) : new Color(1f, 0.68f, 0.12f, 1f);
        Color tierAccentColor = tier == ChestTier.Luxury
            ? new Color(1f, 0.82f, 0.18f, 1f)
            : tier == ChestTier.Rare ? new Color(0.76f, 1f, 0.96f, 1f) : new Color(1f, 0.92f, 0.58f, 1f);
        float tierIntensity = tier == ChestTier.Luxury ? 1.28f : tier == ChestTier.Rare ? 1.10f : 0.92f;
        float tierRaySpread = tier == ChestTier.Luxury ? 82f : tier == ChestTier.Rare ? 68f : 58f;
        float tierParticleDistance = tier == ChestTier.Luxury ? 1.28f : tier == ChestTier.Rare ? 1.12f : 0.92f;

        openingRect.anchoredPosition = basePosition + new Vector2(0f, -55f);
        openingRect.localScale = Vector3.one * 0.55f;
        openingRect.localRotation = Quaternion.Euler(0f, 0f, -4f);
        chestOpeningImage.color = Color.white;
        chestOpeningImage.sprite = frames[0];
        chestOpeningImage.gameObject.SetActive(true);
        if (chestOpeningTransitionImage != null) chestOpeningTransitionImage.gameObject.SetActive(false);
        glowRect.anchoredPosition = new Vector2(0f, 32f);
        glowRect.localScale = Vector3.one * 0.55f;
        glowRect.localRotation = Quaternion.identity;
        chestOpeningGlowImage.color = new Color(tierGlowColor.r, tierGlowColor.g, tierGlowColor.b, 0f);
        chestOpeningGlowImage.gameObject.SetActive(true);
        if (chestOpeningPlatformImage != null)
        {
            chestOpeningPlatformImage.rectTransform.anchoredPosition = new Vector2(0f, -105f);
            chestOpeningPlatformImage.rectTransform.localScale = Vector3.one * 0.72f;
            chestOpeningPlatformImage.color = new Color(tierGlowColor.r, tierGlowColor.g, tierGlowColor.b, 0f);
            chestOpeningPlatformImage.gameObject.SetActive(true);
        }
        if (chestOpeningOrbitImage != null)
        {
            chestOpeningOrbitImage.rectTransform.anchoredPosition = new Vector2(0f, 25f);
            chestOpeningOrbitImage.rectTransform.localScale = Vector3.one * 0.58f;
            chestOpeningOrbitImage.rectTransform.localRotation = Quaternion.identity;
            chestOpeningOrbitImage.color = new Color(tierGlowColor.r, tierGlowColor.g, tierGlowColor.b, 0f);
            chestOpeningOrbitImage.gameObject.SetActive(true);
        }
        if (chestOpeningShockwaveImage != null)
        {
            chestOpeningShockwaveImage.rectTransform.anchoredPosition = new Vector2(0f, 25f);
            chestOpeningShockwaveImage.rectTransform.localScale = Vector3.one * 0.35f;
            chestOpeningShockwaveImage.rectTransform.localRotation = Quaternion.identity;
            chestOpeningShockwaveImage.color = new Color(tierGlowColor.r, tierGlowColor.g, tierGlowColor.b, 0f);
            chestOpeningShockwaveImage.gameObject.SetActive(false);
        }
        for (int i = 0; i < chestOpeningRayImages.Count; i++)
        {
            Image ray = chestOpeningRayImages[i];
            float angle = Mathf.Lerp(-tierRaySpread, tierRaySpread, i / Mathf.Max(1f, chestOpeningRayImages.Count - 1f));
            ray.rectTransform.anchoredPosition = new Vector2(0f, 35f);
            ray.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -angle);
            ray.rectTransform.localScale = new Vector3(0.25f, 0.18f, 1f);
            Color rayColor = i % 2 == 0 ? tierGlowColor : tierAccentColor;
            ray.color = new Color(rayColor.r, rayColor.g, rayColor.b, 0f);
            ray.gameObject.SetActive(true);
        }
        if (chestOpeningKeyImage != null)
        {
            chestOpeningKeyImage.rectTransform.anchoredPosition = new Vector2(-360f, 105f);
            chestOpeningKeyImage.rectTransform.localScale = Vector3.one * 0.38f;
            chestOpeningKeyImage.rectTransform.localRotation = Quaternion.identity;
            chestOpeningKeyImage.color = new Color(tierGlowColor.r, tierGlowColor.g, tierGlowColor.b, 0f);
            chestOpeningKeyImage.gameObject.SetActive(true);
        }
        if (chestOpeningTapText != null)
        {
            chestOpeningTapText.gameObject.SetActive(false);
        }
        if (chestOpeningScreenFlashImage != null)
        {
            chestOpeningScreenFlashImage.color = Color.clear;
            chestOpeningScreenFlashImage.gameObject.SetActive(false);
        }
        if (chestOpeningFlashImage != null)
        {
            chestOpeningFlashImage.color = Color.clear;
            chestOpeningFlashImage.gameObject.SetActive(false);
        }
        for (int i = 0; i < chestOpeningSparkleImages.Count; i++)
        {
            chestOpeningSparkleImages[i].gameObject.SetActive(false);
        }
        chestRewardPanel.SetActive(true);
        chestRewardPanel.transform.SetAsLastSibling();

        float elapsed = 0f;
        const float entranceDuration = 0.52f;
        while (elapsed < entranceDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / entranceDuration);
            float eased = EaseOutBack(t);
            openingRect.anchoredPosition = Vector2.LerpUnclamped(basePosition + new Vector2(0f, -55f), basePosition, eased);
            openingRect.localScale = Vector3.one * Mathf.LerpUnclamped(0.55f, 1f, eased);
            openingRect.localRotation = Quaternion.Euler(0f, 0f, Mathf.LerpUnclamped(-4f, 0f, eased));
            glowRect.localScale = Vector3.one * Mathf.Lerp(0.55f, 0.92f, EaseOutCubic(t));
            chestOpeningGlowImage.color = new Color(tierGlowColor.r, tierGlowColor.g, tierGlowColor.b, Mathf.Lerp(0f, 0.28f, t));
            if (chestOpeningPlatformImage != null)
            {
                chestOpeningPlatformImage.rectTransform.localScale = Vector3.one * Mathf.Lerp(0.72f, 1f, EaseOutCubic(t));
                chestOpeningPlatformImage.color = new Color(tierGlowColor.r, tierGlowColor.g, tierGlowColor.b, Mathf.Lerp(0f, 0.62f, t));
            }
            for (int i = 0; i < chestOpeningRayImages.Count; i++)
            {
                Image ray = chestOpeningRayImages[i];
                float stagger = Mathf.Clamp01((t - (i % 4) * 0.055f) / 0.82f);
                float flicker = 0.78f + (Mathf.Sin((elapsed * (16f + i)) + i) * 0.22f);
                ray.rectTransform.localScale = new Vector3(Mathf.Lerp(0.25f, 0.75f + ((i % 3) * 0.12f), stagger), Mathf.Lerp(0.18f, 0.82f, stagger), 1f);
                Color rayColor = i % 2 == 0 ? tierGlowColor : tierAccentColor;
                ray.color = new Color(rayColor.r, rayColor.g, rayColor.b, stagger * 0.28f * flicker * tierIntensity);
            }
            if (chestOpeningKeyImage != null)
            {
                float keyT = Mathf.Clamp01((t - 0.18f) / 0.82f);
                chestOpeningKeyImage.rectTransform.anchoredPosition = Vector2.Lerp(new Vector2(-360f, 105f), new Vector2(-190f, 120f), EaseOutBack(keyT));
                chestOpeningKeyImage.rectTransform.localScale = Vector3.one * Mathf.Lerp(0.38f, 0.92f, EaseOutBack(keyT));
                chestOpeningKeyImage.rectTransform.localRotation = Quaternion.Euler(0f, 0f, keyT * 180f);
                chestOpeningKeyImage.color = new Color(tierGlowColor.r, tierGlowColor.g, tierGlowColor.b, keyT);
            }
            yield return null;
        }

        yield return new WaitForSecondsRealtime(0.22f);

        if (chestOpeningKeyImage != null)
        {
            Vector2 keyStart = chestOpeningKeyImage.rectTransform.anchoredPosition;
            Vector2 keyControl = new Vector2(105f, 188f);
            Vector2 keyEnd = new Vector2(0f, 48f);
            elapsed = 0f;
            const float keyFlightDuration = 0.52f;
            while (elapsed < keyFlightDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / keyFlightDuration);
                float eased = EaseOutCubic(t);
                float inverse = 1f - eased;
                chestOpeningKeyImage.rectTransform.anchoredPosition = (inverse * inverse * keyStart) + (2f * inverse * eased * keyControl) + (eased * eased * keyEnd);
                chestOpeningKeyImage.rectTransform.localScale = Vector3.one * Mathf.Lerp(0.92f, 0.42f, t);
                chestOpeningKeyImage.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 180f + (t * 540f));
                chestOpeningKeyImage.color = new Color(tierGlowColor.r, tierGlowColor.g, tierGlowColor.b, Mathf.Lerp(1f, 0.72f, t));
                yield return null;
            }
            chestOpeningKeyImage.gameObject.SetActive(false);
        }

        if (chestOpeningAudioSource != null && chestKeyImpactClip != null)
            chestOpeningAudioSource.PlayOneShot(chestKeyImpactClip, 0.82f);

        // Anahtarın kilide çarpmasını kısa ama güçlü bir ezilme ve geri sekmeyle hissettir.
        if (chestOpeningFlashImage != null)
        {
            chestOpeningFlashImage.rectTransform.anchoredPosition = new Vector2(0f, 42f);
            chestOpeningFlashImage.rectTransform.localScale = Vector3.one * 0.22f;
            chestOpeningFlashImage.color = new Color(1f, 0.95f, 0.72f, 1f);
            chestOpeningFlashImage.gameObject.SetActive(true);
        }
        elapsed = 0f;
        const float keyImpactDuration = 0.34f;
        while (elapsed < keyImpactDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / keyImpactDuration);
            float impact = Mathf.Sin(t * Mathf.PI);
            openingRect.anchoredPosition = basePosition + new Vector2(Mathf.Sin(t * 38f) * 10f * (1f - t), -12f * impact);
            openingRect.localScale = new Vector3(1f + (0.13f * impact), 1f - (0.16f * impact), 1f);
            openingRect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * 31f) * 3.5f * (1f - t));
            if (chestOpeningFlashImage != null)
            {
                chestOpeningFlashImage.rectTransform.localScale = Vector3.one * Mathf.Lerp(0.22f, 1.05f, EaseOutCubic(t));
                chestOpeningFlashImage.color = new Color(1f, 0.95f, 0.72f, Mathf.Lerp(1f, 0f, t));
            }
            yield return null;
        }
        if (chestOpeningFlashImage != null) chestOpeningFlashImage.gameObject.SetActive(false);
        openingRect.anchoredPosition = basePosition;
        openingRect.localScale = Vector3.one;
        openingRect.localRotation = Quaternion.identity;
        yield return CrossfadeChestFrame(frames[1], 0.14f);
        if (chestOpeningOrbitImage != null)
        {
            float orbitBaseAlpha = tier == ChestTier.Rare ? 0.30f : tier == ChestTier.Luxury ? 0.22f : 0.14f;
            chestOpeningOrbitImage.color = new Color(tierGlowColor.r, tierGlowColor.g, tierGlowColor.b, orbitBaseAlpha);
        }
        elapsed = 0f;
        const float anticipationDuration = 0.82f;
        while (elapsed < anticipationDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / anticipationDuration);
            float strength = 2.5f + (13.5f * t * t);
            Vector2 liftedPosition = basePosition + new Vector2(0f, Mathf.Lerp(0f, 30f, EaseOutCubic(t)));
            openingRect.anchoredPosition = liftedPosition + new Vector2(Mathf.Sin(elapsed * (52f + (40f * t))) * strength, Mathf.Sin(elapsed * 39f) * strength * 0.22f);
            openingRect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(elapsed * 70f) * 2.2f * t);
            openingRect.localScale = Vector3.one * (1f + (t * 0.10f) + (Mathf.Sin(elapsed * 25f) * 0.035f));
            float pulse = 0.46f + (Mathf.Sin(elapsed * 18f) * 0.13f);
            glowRect.localScale = Vector3.one * (0.92f + (t * 0.16f));
            chestOpeningGlowImage.color = new Color(tierGlowColor.r, tierGlowColor.g, tierGlowColor.b, pulse);
            if (chestOpeningOrbitImage != null)
            {
                chestOpeningOrbitImage.rectTransform.localScale = Vector3.one * Mathf.Lerp(0.58f, 0.96f, EaseOutCubic(t));
                chestOpeningOrbitImage.rectTransform.localRotation = Quaternion.Euler(0f, 0f, t * 420f);
                float orbitStartAlpha = tier == ChestTier.Rare ? 0.20f : 0.10f;
                float orbitEndAlpha = tier == ChestTier.Rare ? 0.38f : tier == ChestTier.Luxury ? 0.26f : 0.18f;
                chestOpeningOrbitImage.color = new Color(tierGlowColor.r, tierGlowColor.g, tierGlowColor.b, Mathf.Lerp(orbitStartAlpha, orbitEndAlpha, t));
            }
            if (chestOpeningPlatformImage != null)
            {
                chestOpeningPlatformImage.rectTransform.localScale = new Vector3(1f + (Mathf.Sin(elapsed * 14f) * 0.05f), 1f, 1f);
                chestOpeningPlatformImage.color = new Color(tierGlowColor.r, tierGlowColor.g, tierGlowColor.b, 0.58f + (Mathf.Sin(elapsed * 16f) * 0.12f));
            }
            for (int i = 0; i < chestOpeningRayImages.Count; i++)
            {
                Image ray = chestOpeningRayImages[i];
                float flicker = 0.76f + (Mathf.Sin((elapsed * (14f + i)) + (i * 0.7f)) * 0.24f);
                ray.rectTransform.localScale = new Vector3(0.65f + ((i % 3) * 0.14f), Mathf.Lerp(0.75f, 1.12f, t), 1f);
                Color rayColor = i % 2 == 0 ? tierGlowColor : tierAccentColor;
                ray.color = new Color(rayColor.r, rayColor.g, rayColor.b, Mathf.Lerp(0.20f, 0.58f, t) * flicker * tierIntensity);
            }
            int orbitParticleCount = tier == ChestTier.Luxury ? 12 : tier == ChestTier.Rare ? 8 : 0;
            for (int i = 0; i < orbitParticleCount && i < chestOpeningSparkleImages.Count; i++)
            {
                Image sparkle = chestOpeningSparkleImages[i];
                float directionSign = tier == ChestTier.Luxury && i % 2 == 0 ? -1f : 1f;
                float angle = ((i / (float)orbitParticleCount) * Mathf.PI * 2f) + (t * Mathf.PI * 2.2f * directionSign);
                float radius = Mathf.Lerp(175f + ((i % 3) * 16f), 72f, EaseOutCubic(t));
                sparkle.rectTransform.anchoredPosition = new Vector2(Mathf.Cos(angle) * radius, 35f + (Mathf.Sin(angle) * radius * 0.62f));
                sparkle.rectTransform.localScale = Vector3.one * (0.28f + ((i % 3) * 0.10f));
                Color orbitColor = i % 2 == 0 ? tierAccentColor : tierGlowColor;
                sparkle.color = new Color(orbitColor.r, orbitColor.g, orbitColor.b, Mathf.Sin(t * Mathf.PI) * 0.78f);
                sparkle.gameObject.SetActive(true);
            }
            yield return null;
        }

        openingRect.anchoredPosition = basePosition + new Vector2(0f, 30f);
        openingRect.localRotation = Quaternion.identity;
        openingRect.localScale = Vector3.one * 1.04f;
        yield return CrossfadeChestFrame(frames[2], 0.16f);
        if (chestOpeningFlashImage != null)
        {
            flashRect.anchoredPosition = new Vector2(0f, 28f);
            flashRect.localScale = Vector3.one * 0.52f;
            chestOpeningFlashImage.color = new Color(1f, 0.94f, 0.70f, 0.92f);
            chestOpeningFlashImage.gameObject.SetActive(true);
        }

        elapsed = 0f;
        const float flashDuration = 0.30f;
        while (elapsed < flashDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / flashDuration);
            if (chestOpeningFlashImage != null)
            {
                flashRect.localScale = Vector3.one * Mathf.Lerp(0.52f, 1.30f, EaseOutCubic(t));
                chestOpeningFlashImage.color = new Color(1f, 0.94f, 0.70f, Mathf.Lerp(0.92f, 0.12f, t));
            }
            glowRect.localScale = Vector3.one * Mathf.Lerp(1.05f, 1.28f, t);
            chestOpeningGlowImage.color = new Color(tierGlowColor.r, tierGlowColor.g, tierGlowColor.b, Mathf.Lerp(0.72f, 0.92f, t));
            yield return null;
        }

        AudioClip tierOpenClip = tier == ChestTier.Luxury
            ? chestOpenLuxuryClip
            : tier == ChestTier.Rare ? chestOpenRareClip : chestOpenCommonClip;
        if (chestOpeningAudioSource != null && tierOpenClip != null)
            chestOpeningAudioSource.PlayOneShot(tierOpenClip, 0.92f);
        yield return CrossfadeChestFrame(frames[3], 0.18f);
        openingRect.localScale = Vector3.one * 1.30f;
        if (chestOpeningTapText != null) chestOpeningTapText.gameObject.SetActive(false);
        if (chestOpeningScreenFlashImage != null)
        {
            chestOpeningScreenFlashImage.color = new Color(1f, 0.96f, 0.78f, 0f);
            chestOpeningScreenFlashImage.gameObject.SetActive(true);
        }
        if (chestOpeningShockwaveImage != null)
        {
            chestOpeningShockwaveImage.rectTransform.localScale = Vector3.one * 0.35f;
            chestOpeningShockwaveImage.rectTransform.localRotation = Quaternion.identity;
            chestOpeningShockwaveImage.color = new Color(tierGlowColor.r, tierGlowColor.g, tierGlowColor.b, 0.78f);
            chestOpeningShockwaveImage.gameObject.SetActive(true);
        }

        for (int i = 0; i < chestOpeningSparkleImages.Count; i++)
        {
            Image sparkle = chestOpeningSparkleImages[i];
            sparkle.rectTransform.anchoredPosition = new Vector2(0f, 34f);
            sparkle.rectTransform.localScale = Vector3.one * (0.55f + ((i % 3) * 0.18f));
            sparkle.color = new Color(tierGlowColor.r, tierGlowColor.g, tierGlowColor.b, 0f);
            sparkle.gameObject.SetActive(true);
        }

        elapsed = 0f;
        const float burstDuration = 0.82f;
        while (elapsed < burstDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / burstDuration);
            float eased = EaseOutCubic(t);
            openingRect.anchoredPosition = Vector2.Lerp(basePosition + new Vector2(0f, 30f), basePosition + new Vector2(0f, 6f), EaseOutBack(t));
            openingRect.localScale = Vector3.one * Mathf.Lerp(1.30f, 1f, EaseOutBack(t));
            glowRect.localScale = Vector3.one * (1.22f + (Mathf.Sin(t * Mathf.PI) * 0.16f));
            glowRect.localRotation = Quaternion.Euler(0f, 0f, t * 28f);
            chestOpeningGlowImage.color = new Color(tierGlowColor.r, tierGlowColor.g, tierGlowColor.b, Mathf.Lerp(0.88f, 0.24f, t));
            if (chestOpeningFlashImage != null)
            {
                chestOpeningFlashImage.color = new Color(1f, 0.94f, 0.70f, Mathf.Lerp(0.12f, 0f, t));
            }
            if (chestOpeningOrbitImage != null)
            {
                chestOpeningOrbitImage.rectTransform.localScale = Vector3.one * Mathf.Lerp(0.96f, 1.48f, EaseOutCubic(t));
                chestOpeningOrbitImage.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 420f + (t * 500f));
                chestOpeningOrbitImage.color = new Color(tierGlowColor.r, tierGlowColor.g, tierGlowColor.b, Mathf.Lerp(0.22f, 0f, t));
            }
            if (chestOpeningShockwaveImage != null)
            {
                float burstAlpha = Mathf.Pow(1f - t, 1.7f) * 0.72f * tierIntensity;
                if (tier == ChestTier.Luxury)
                    burstAlpha += Mathf.Sin(Mathf.Clamp01((t - 0.30f) / 0.44f) * Mathf.PI) * 0.32f;
                chestOpeningShockwaveImage.rectTransform.localScale = new Vector3(Mathf.Lerp(0.35f, 1.45f, EaseOutCubic(t)), Mathf.Lerp(0.35f, 0.92f, EaseOutCubic(t)), 1f);
                chestOpeningShockwaveImage.color = new Color(tierGlowColor.r, tierGlowColor.g, tierGlowColor.b, burstAlpha);
            }
            for (int i = 0; i < chestOpeningRayImages.Count; i++)
            {
                Image ray = chestOpeningRayImages[i];
                float rayDelay = (i % 3) * 0.035f;
                float rayT = Mathf.Clamp01((t - rayDelay) / (1f - rayDelay));
                float rayAlpha = Mathf.Pow(1f - rayT, 1.25f) * (0.72f - ((i % 4) * 0.07f));
                ray.rectTransform.localScale = new Vector3(0.55f + ((i % 3) * 0.16f), Mathf.Lerp(0.85f, 1.72f * tierIntensity, EaseOutCubic(rayT)), 1f);
                float baseRayAngle = Mathf.Lerp(-tierRaySpread, tierRaySpread, i / Mathf.Max(1f, chestOpeningRayImages.Count - 1f));
                float raySwirl = tier == ChestTier.Rare ? Mathf.Sin((t * Mathf.PI) + i) * 18f * (1f - t) : 0f;
                ray.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -baseRayAngle + raySwirl);
                Color rayColor = i % 2 == 0 ? tierGlowColor : tierAccentColor;
                ray.color = new Color(rayColor.r, rayColor.g, rayColor.b, rayAlpha * tierIntensity);
            }
            if (chestOpeningPlatformImage != null)
            {
                chestOpeningPlatformImage.rectTransform.localScale = new Vector3(Mathf.Lerp(1f, 1.36f, t), Mathf.Lerp(1f, 0.65f, t), 1f);
                chestOpeningPlatformImage.color = new Color(tierGlowColor.r, tierGlowColor.g, tierGlowColor.b, Mathf.Lerp(0.70f, 0f, t));
            }
            if (chestOpeningScreenFlashImage != null)
            {
                float screenFlashAlpha = t < 0.16f
                    ? Mathf.Lerp(0f, 0.70f, EaseOutCubic(t / 0.16f))
                    : Mathf.Lerp(0.70f, 0f, (t - 0.16f) / 0.84f);
                chestOpeningScreenFlashImage.color = new Color(1f, 0.97f, 0.82f, screenFlashAlpha);
            }
            if (rewardPanelRect != null)
            {
                float shakeFade = Mathf.Clamp01(1f - (t / 0.42f));
                float shakeStrength = tier == ChestTier.Luxury ? 10f : tier == ChestTier.Rare ? 7f : 5f;
                rewardPanelRect.anchoredPosition = rewardPanelRestPosition + new Vector2(Mathf.Sin(elapsed * 93f) * shakeStrength * shakeFade, Mathf.Cos(elapsed * 71f) * shakeStrength * 0.55f * shakeFade);
            }
            for (int i = 0; i < chestOpeningSparkleImages.Count; i++)
            {
                float spiralOffset = tier == ChestTier.Rare ? t * 115f : tier == ChestTier.Luxury ? Mathf.Sin(t * Mathf.PI) * 24f : 0f;
                float angle = ((360f / chestOpeningSparkleImages.Count) * i + ((i % 2) * 8f) + spiralOffset) * Mathf.Deg2Rad;
                float distance = (115f + ((i % 5) * 24f)) * tierParticleDistance;
                Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                Image sparkle = chestOpeningSparkleImages[i];
                sparkle.rectTransform.anchoredPosition = new Vector2(0f, 28f) + (direction * distance * eased);
                float sparkleAlpha = Mathf.Sin(t * Mathf.PI) * (1f - (0.35f * t));
                Color sparkleColor = i % 3 == 0 ? tierAccentColor : tierGlowColor;
                sparkle.color = new Color(sparkleColor.r, sparkleColor.g, sparkleColor.b, sparkleAlpha * tierIntensity);
                sparkle.rectTransform.localScale = Vector3.one * Mathf.Lerp(0.45f + ((i % 3) * 0.15f), 0.12f, t);
            }
            yield return null;
        }

        if (rewardPanelRect != null) rewardPanelRect.anchoredPosition = rewardPanelRestPosition;

        chestOpeningImage.gameObject.SetActive(false);
        chestOpeningGlowImage.gameObject.SetActive(false);
        if (chestOpeningFlashImage != null) chestOpeningFlashImage.gameObject.SetActive(false);
        if (chestOpeningPlatformImage != null) chestOpeningPlatformImage.gameObject.SetActive(false);
        if (chestOpeningOrbitImage != null) chestOpeningOrbitImage.gameObject.SetActive(false);
        if (chestOpeningShockwaveImage != null) chestOpeningShockwaveImage.gameObject.SetActive(false);
        if (chestOpeningKeyImage != null) chestOpeningKeyImage.gameObject.SetActive(false);
        if (chestOpeningScreenFlashImage != null) chestOpeningScreenFlashImage.gameObject.SetActive(false);
        if (chestOpeningTapText != null) chestOpeningTapText.gameObject.SetActive(false);
        for (int i = 0; i < chestOpeningRayImages.Count; i++) chestOpeningRayImages[i].gameObject.SetActive(false);
        for (int i = 0; i < chestOpeningSparkleImages.Count; i++) chestOpeningSparkleImages[i].gameObject.SetActive(false);
        if (chestRewardText != null) chestRewardText.gameObject.SetActive(true);
        if (chestRewardTitleText != null) chestRewardTitleText.text = "SANDIK ÖDÜLÜ";
        ShowChestReward(rewardText, rewardSprite, rewardPanelColor);
        if (chestOpeningAudioSource != null && chestRewardRevealClip != null)
            chestOpeningAudioSource.PlayOneShot(chestRewardRevealClip, tier == ChestTier.Luxury ? 0.88f : 0.72f);

        if (chestRewardIconCanvasGroup != null) chestRewardIconCanvasGroup.alpha = 0f;
        if (chestRewardTextCanvasGroup != null) chestRewardTextCanvasGroup.alpha = 0f;
        Vector2 rewardIconRestPosition = new Vector2(0f, 55f);
        if (chestRewardIconImage != null)
        {
            chestRewardIconImage.rectTransform.anchoredPosition = rewardIconRestPosition + new Vector2(0f, -70f);
            chestRewardIconImage.rectTransform.localScale = Vector3.one * 0.18f;
            chestRewardIconImage.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -18f);
        }
        elapsed = 0f;
        const float rewardRevealDuration = 0.62f;
        while (elapsed < rewardRevealDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / rewardRevealDuration);
            if (chestRewardIconCanvasGroup != null) chestRewardIconCanvasGroup.alpha = Mathf.Clamp01(t * 1.7f);
            if (chestRewardIconImage != null)
            {
                float reveal = EaseOutBack(t);
                chestRewardIconImage.rectTransform.anchoredPosition = Vector2.LerpUnclamped(rewardIconRestPosition + new Vector2(0f, -70f), rewardIconRestPosition, reveal);
                chestRewardIconImage.rectTransform.localScale = Vector3.one * Mathf.LerpUnclamped(0.18f, 1f, reveal);
                chestRewardIconImage.rectTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.LerpUnclamped(-18f, 0f, reveal));
            }
            if (chestRewardTextCanvasGroup != null) chestRewardTextCanvasGroup.alpha = Mathf.Clamp01((t - 0.28f) / 0.55f);
            yield return null;
        }

        if (chestRewardIconCanvasGroup != null) chestRewardIconCanvasGroup.alpha = 1f;
        if (chestRewardTextCanvasGroup != null) chestRewardTextCanvasGroup.alpha = 1f;
        if (chestRewardIconImage != null)
        {
            chestRewardIconImage.rectTransform.anchoredPosition = rewardIconRestPosition;
            chestRewardIconImage.rectTransform.localScale = Vector3.one;
            chestRewardIconImage.rectTransform.localRotation = Quaternion.identity;
        }
        if (chestRewardCloseButtonObject != null) chestRewardCloseButtonObject.SetActive(true);
        chestOpeningInProgress = false;
        chestOpeningCoroutine = null;
    }

    private Color GetItemRarityPanelColor(ItemData item)
    {
        if (item == null)
        {
            return new Color(0.98f, 0.94f, 0.84f, 0.99f);
        }

        return new Color(0.98f, 0.94f, 0.84f, 0.99f);
    }

    private bool IsDarkColor(Color color)
    {
        float brightness = (color.r * 0.2126f) + (color.g * 0.7152f) + (color.b * 0.0722f);
        return brightness < 0.45f;
    }

    private Sprite GetPaintCanSprite(ItemData item)
    {
        if (item == null)
        {
            return null;
        }

        Sprite workshopPaintSprite = GetWorkshopMaterialSprite(WorkshopCraftMaterialType.Paint);
        if (workshopPaintSprite != null)
        {
            return workshopPaintSprite;
        }

        if (!string.IsNullOrEmpty(item.Id))
        {
            if (!paintCanSprites.ContainsKey(item.Id))
            {
                paintCanSprites[item.Id] = LoadSpriteFromResources("Art/Items/" + item.Id);
            }

            if (paintCanSprites[item.Id] != null)
            {
                return paintCanSprites[item.Id];
            }
        }

        if (item.Rarity == ItemRarity.Luxury)
        {
            if (specialPaintCanSprite == null)
            {
                specialPaintCanSprite = LoadSpriteFromResources("Art/Items/paint_can_special");
            }

            return specialPaintCanSprite;
        }

        if (item.Rarity == ItemRarity.Rare)
        {
            if (rarePaintCanSprite == null)
            {
                rarePaintCanSprite = LoadSpriteFromResources("Art/Items/paint_can_rare");
            }

            return rarePaintCanSprite;
        }

        if (commonPaintCanSprite == null)
        {
            commonPaintCanSprite = LoadSpriteFromResources("Art/Items/paint_can_common");
        }

        return commonPaintCanSprite;
    }

    private Sprite GetChestButtonSprite()
    {
        if (chestButtonSprite == null)
        {
            chestButtonSprite = LoadSpriteFromResources("Art/UI/chest_button");
        }

        return chestButtonSprite;
    }

    private Sprite GetCoinIconSprite()
    {
        if (coinIconSprite == null)
        {
            coinIconSprite = LoadSpriteFromResources("Art/UI/coin_icon");
        }

        return coinIconSprite;
    }

    private Sprite GetGemIconSprite()
    {
        if (gemIconSprite == null)
        {
            gemIconSprite = LoadSpriteFromResources("Art/UI/gem_icon");
        }

        return gemIconSprite;
    }

    private Sprite GetChestRewardCardFrameSprite()
    {
        if (chestRewardCardFrameSprite != null)
        {
            return chestRewardCardFrameSprite;
        }

        Texture2D atlas = Resources.Load<Texture2D>("Art/WorkshopCatalog/Atlases/workshop_material_icons_v2");
        if (atlas == null)
        {
            return null;
        }

        float cellWidth = atlas.width / 4f;
        float cellHeight = atlas.height / 2f;
        chestRewardCardFrameSprite = Sprite.Create(
            atlas,
            new Rect(cellWidth * 3f, 0f, cellWidth, cellHeight),
            new Vector2(0.5f, 0.5f),
            100f,
            0,
            SpriteMeshType.FullRect);
        chestRewardCardFrameSprite.name = "ChestRewardCardFrame";
        return chestRewardCardFrameSprite;
    }

    private Sprite GetWoodIconSprite()
    {
        if (woodIconSprite == null)
        {
            woodIconSprite = GetWorkshopMaterialSprite(WorkshopCraftMaterialType.Wood);
            if (woodIconSprite == null)
            {
                woodIconSprite = LoadSpriteFromResources("Art/Items/wood_bundle_001");
            }
        }

        return woodIconSprite;
    }

    private Sprite GetCraftPaintIconSprite()
    {
        if (craftPaintIconSprite == null)
        {
            craftPaintIconSprite = GetWorkshopMaterialSprite(WorkshopCraftMaterialType.Paint);
            if (craftPaintIconSprite == null)
            {
                craftPaintIconSprite = LoadSpriteFromResources("Art/Items/paint_can_white_001");
            }
        }

        return craftPaintIconSprite;
    }

    private Sprite GetWorkshopMaterialSprite(WorkshopCraftMaterialType material)
    {
        if (workshopMaterialSprites.ContainsKey(material))
        {
            return workshopMaterialSprites[material];
        }

        WorkshopCraftMaterialDefinition definition = null;
        IReadOnlyList<WorkshopCraftMaterialDefinition> materials = WorkshopDecorCatalog.CraftMaterials;
        for (int i = 0; i < materials.Count; i++)
        {
            if (materials[i].Type == material)
            {
                definition = materials[i];
                break;
            }
        }

        if (definition == null)
        {
            workshopMaterialSprites[material] = null;
            return null;
        }

        if (workshopMaterialAtlasTexture == null)
        {
            workshopMaterialAtlasTexture = Resources.Load<Texture2D>(definition.AtlasResourcePath);
        }

        if (workshopMaterialAtlasTexture == null)
        {
            workshopMaterialSprites[material] = null;
            return null;
        }

        int cellWidth = workshopMaterialAtlasTexture.width / definition.AtlasColumns;
        int cellHeight = workshopMaterialAtlasTexture.height / definition.AtlasRows;
        int column = definition.AtlasCellIndex % definition.AtlasColumns;
        int rowFromTop = definition.AtlasCellIndex / definition.AtlasColumns;
        float x = column * cellWidth;
        float y = workshopMaterialAtlasTexture.height - ((rowFromTop + 1) * cellHeight);
        Rect cellRect = new Rect(x, y, cellWidth, cellHeight);

        Sprite sprite = Sprite.Create(
            workshopMaterialAtlasTexture,
            cellRect,
            new Vector2(0.5f, 0.5f),
            100f,
            0,
            SpriteMeshType.FullRect);
        sprite.name = "WorkshopMaterial_" + material;
        workshopMaterialSprites[material] = sprite;
        return sprite;
    }

    private Sprite GetDecorIconSprite(DecorItemData item)
    {
        if (item == null || string.IsNullOrEmpty(item.ItemId))
        {
            return null;
        }

        if (!decorIconSprites.ContainsKey(item.ItemId))
        {
            string workshopProductId = "";
            if (item.ItemId == "wall_pink_soft_001") workshopProductId = "wall_rose_001";
            else if (item.ItemId == "wall_mint_soft_001") workshopProductId = "wall_mint_001";
            else if (item.ItemId == "floor_cream_tile_001") workshopProductId = "floor_cream_marble_001";
            else if (item.ItemId == "table_rose_round_001") workshopProductId = "table_rose_001";

            Sprite sprite = GetWorkshopProductSprite(workshopProductId);
            if (sprite == null)
            {
                sprite = LoadSpriteFromResources("Art/DecorIcons/" + item.ItemId);
            }

            decorIconSprites[item.ItemId] = sprite;
        }

        return decorIconSprites[item.ItemId];
    }

    private Sprite GetWorkshopProductSprite(string productId)
    {
        if (string.IsNullOrEmpty(productId))
        {
            return null;
        }

        if (workshopProductSprites.TryGetValue(productId, out Sprite cachedSprite))
        {
            return cachedSprite;
        }

        WorkshopDecorProduct product = null;
        IReadOnlyList<WorkshopDecorProduct> products = WorkshopDecorCatalog.Products;
        for (int i = 0; i < products.Count; i++)
        {
            if (products[i].Id == productId)
            {
                product = products[i];
                break;
            }
        }

        if (product == null)
        {
            return null;
        }

        Sprite legacyVariantSprite = GetWorkshopLegacyVariantSprite(product);
        if (legacyVariantSprite != null)
        {
            workshopProductSprites[product.Id] = legacyVariantSprite;
            return legacyVariantSprite;
        }

        Texture2D circleProductTexture = Resources.Load<Texture2D>(
            "Art/WorkshopCatalog/CircleProducts/" + product.Id);
        if (circleProductTexture != null)
        {
            Sprite circleProductSprite = Sprite.Create(
                circleProductTexture,
                new Rect(0f, 0f, circleProductTexture.width, circleProductTexture.height),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect);
            circleProductSprite.name = "WorkshopCircleProduct_" + product.Id;
            workshopProductSprites[product.Id] = circleProductSprite;
            return circleProductSprite;
        }

        if (!workshopProductAtlasTextures.ContainsKey(product.AtlasResourcePath))
        {
            workshopProductAtlasTextures[product.AtlasResourcePath] = Resources.Load<Texture2D>(product.AtlasResourcePath);
        }

        Texture2D atlas = workshopProductAtlasTextures[product.AtlasResourcePath];
        if (atlas == null)
        {
            return null;
        }

        int column = product.AtlasCellIndex % product.AtlasColumns;
        int rowFromTop = product.AtlasCellIndex / product.AtlasColumns;
        float sourceCellWidth = atlas.width * 0.258f;
        float sourceCellHeight = atlas.height * 0.365f;
        float centerX = atlas.width * (0.195f + (column * 0.305f));
        float centerY = atlas.height * (rowFromTop == 0 ? 0.705f : 0.215f);
        // The source atlases contain their own legacy card border and number badge.
        // Crop only the actual product artwork so it fits the new workshop cards.
        centerX += sourceCellWidth * 0.06f;
        centerY -= atlas.height * 0.035f;
        float cellWidth = sourceCellWidth * 0.72f;
        float cellHeight = sourceCellHeight * 0.72f;
        Rect cellRect = new Rect(
            Mathf.Clamp(centerX - (cellWidth * 0.5f), 0f, atlas.width - cellWidth),
            Mathf.Clamp(centerY - (cellHeight * 0.5f), 0f, atlas.height - cellHeight),
            cellWidth,
            cellHeight);

        Sprite sprite = Sprite.Create(atlas, cellRect, new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
        sprite.name = "WorkshopProduct_" + product.Id;
        workshopProductSprites[product.Id] = sprite;
        return sprite;
    }

    private Sprite GetWorkshopLegacyVariantSprite(WorkshopDecorProduct product)
    {
        if (product == null || product.Category == WorkshopDecorCategory.Wall || product.Category == WorkshopDecorCategory.Floor)
        {
            return null;
        }

        string atlasName;
        switch (product.Category)
        {
            case WorkshopDecorCategory.Table: atlasName = "legacy_table_variants"; break;
            case WorkshopDecorCategory.Chair: atlasName = "legacy_chair_variants"; break;
            case WorkshopDecorCategory.Rug: atlasName = "legacy_rug_variants"; break;
            case WorkshopDecorCategory.Shelf: atlasName = "legacy_shelf_variants"; break;
            case WorkshopDecorCategory.Counter: atlasName = "legacy_counter_variants"; break;
            case WorkshopDecorCategory.Plant: atlasName = "legacy_plant_variants"; break;
            case WorkshopDecorCategory.WallArt: atlasName = "legacy_wall_art_variants"; break;
            default: return null;
        }

        string atlasPath = "Art/WorkshopCatalog/LegacyAtlases/" + atlasName;
        if (!workshopProductAtlasTextures.TryGetValue(atlasPath, out Texture2D atlas))
        {
            atlas = Resources.Load<Texture2D>(atlasPath);
            workshopProductAtlasTextures[atlasPath] = atlas;
        }
        if (atlas == null)
        {
            return null;
        }

        int cellIndex = GetWorkshopLegacyVariantCellIndex(product.Category, product.VariantKey);
        Rect cellRect = GetWorkshopLegacyVariantContentRect(product.Category, cellIndex, atlas);

        Sprite sprite = Sprite.Create(atlas, cellRect, new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
        sprite.name = "WorkshopLegacyProduct_" + product.Id;
        return sprite;
    }

    private int GetWorkshopLegacyVariantCellIndex(WorkshopDecorCategory category, string variantKey)
    {
        if (string.IsNullOrEmpty(variantKey)) return 0;
        if (variantKey == "honey_oak" || variantKey == "olive" || variantKey == "walnut_sand" || variantKey == "terracotta" || variantKey == "walnut") return 0;
        if (variantKey.Contains("cream") || variantKey == "gold_cream") return 1;
        if (variantKey.Contains("rose")) return 2;
        if (variantKey.Contains("mint")) return 3;
        if (variantKey.Contains("burgundy") || variantKey == "dark_walnut") return 4;
        if (variantKey.Contains("black") || variantKey.Contains("charcoal") || variantKey == "navy") return 5;
        return 0;
    }

    private Rect GetWorkshopLegacyVariantContentRect(WorkshopDecorCategory category, int cellIndex, Texture2D atlas)
    {
        int column = Mathf.Clamp(cellIndex % 3, 0, 2);
        int rowFromTop = Mathf.Clamp(cellIndex / 3, 0, 1);
        float[] centersX;
        float centerTopY;
        float centerBottomY;
        Vector2 contentSize;

        switch (category)
        {
            case WorkshopDecorCategory.Table:
                centersX = new[] { 304.5f, 256f, 208f };
                centerTopY = 281.5f; centerBottomY = 217.5f; contentSize = new Vector2(380f, 380f);
                break;
            case WorkshopDecorCategory.Chair:
                centersX = new[] { 331.5f, 267f, 202f };
                centerTopY = 248f; centerBottomY = 242f; contentSize = new Vector2(270f, 460f);
                break;
            case WorkshopDecorCategory.Rug:
                centersX = new[] { 283f, 254.5f, 228.5f };
                centerTopY = 325f; centerBottomY = 186f; contentSize = new Vector2(450f, 240f);
                break;
            case WorkshopDecorCategory.Shelf:
                centersX = new[] { 261.5f, 260f, 254.5f };
                centerTopY = 275f; centerBottomY = 164f; contentSize = new Vector2(460f, 100f);
                break;
            case WorkshopDecorCategory.Counter:
                centersX = new[] { 263f, 258f, 254f };
                centerTopY = 305f; centerBottomY = 178f; contentSize = new Vector2(500f, 190f);
                break;
            case WorkshopDecorCategory.Plant:
                centersX = new[] { 305f, 254f, 209f };
                centerTopY = 259f; centerBottomY = 227.5f; contentSize = new Vector2(340f, 440f);
                break;
            case WorkshopDecorCategory.WallArt:
                centersX = new[] { 323.5f, 256.5f, 188.5f };
                centerTopY = 256f; centerBottomY = 249f; contentSize = new Vector2(310f, 460f);
                break;
            default:
                float defaultCellWidth = atlas.width / 3f;
                float defaultCellHeight = atlas.height / 2f;
                return new Rect(
                    column * defaultCellWidth,
                    atlas.height - ((rowFromTop + 1) * defaultCellHeight),
                    defaultCellWidth,
                    defaultCellHeight);
        }

        float scaleX = atlas.width / 1536f;
        float scaleY = atlas.height / 1024f;
        float cellWidth = atlas.width / 3f;
        float cellHeight = atlas.height / 2f;
        float centerX = (column * cellWidth) + (centersX[column] * scaleX);
        float centerFromTop = (rowFromTop * cellHeight) + ((rowFromTop == 0 ? centerTopY : centerBottomY) * scaleY);
        float width = contentSize.x * scaleX;
        float height = contentSize.y * scaleY;
        return new Rect(centerX - (width * 0.5f), atlas.height - centerFromTop - (height * 0.5f), width, height);
    }

    private Sprite GetLevelBadgeSprite()
    {
        if (levelBadgeSprite == null)
        {
            levelBadgeSprite = LoadSpriteFromResources("Art/UI/level_badge_icon");
        }

        return levelBadgeSprite;
    }

    private Sprite GetVipIconSprite()
    {
        if (vipIconSprite == null)
        {
            vipIconSprite = LoadSpriteFromResources("Art/UI/vip_icon");
        }

        return vipIconSprite;
    }

    private Sprite GetTasksMenuIconSprite()
    {
        if (tasksMenuIconSprite == null)
        {
            tasksMenuIconSprite = LoadSpriteFromResources("Art/UI/tasks_icon");
        }

        return tasksMenuIconSprite;
    }

    private Sprite GetDailyTasksMenuIconSprite()
    {
        if (dailyTasksMenuIconSprite == null)
        {
            dailyTasksMenuIconSprite = LoadSpriteFromResources("Art/UI/daily_tasks_icon");
        }

        return dailyTasksMenuIconSprite;
    }

    private Sprite GetDailyRewardMenuIconSprite()
    {
        if (dailyRewardMenuIconSprite == null)
        {
            dailyRewardMenuIconSprite = LoadSpriteFromResources("Art/UI/daily_reward_icon");
        }

        return dailyRewardMenuIconSprite;
    }

    private Sprite GetDecorMenuIconSprite()
    {
        if (decorMenuIconSprite == null)
        {
            decorMenuIconSprite = LoadSpriteFromResources("Art/UI/decor_menu_icon");
        }

        return decorMenuIconSprite;
    }

    private Sprite GetNotifyExclamationSprite()
    {
        if (notifyExclamationSprite == null)
        {
            notifyExclamationSprite = LoadSpriteFromResources("Art/UI/notify_exclamation");
        }

        return notifyExclamationSprite;
    }

    private Sprite GetAdBoostIconSprite()
    {
        if (adBoostIconSprite == null)
        {
            adBoostIconSprite = LoadSpriteFromResources("Art/UI/ad_boost_2x_icon");
        }

        return adBoostIconSprite;
    }

    private Sprite GetAdBoostTimerBadgeSprite()
    {
        if (adBoostTimerBadgeSprite == null)
        {
            adBoostTimerBadgeSprite = LoadSpriteFromResources("Art/UI/ad_boost_timer_badge");
        }

        return adBoostTimerBadgeSprite;
    }

    private Sprite GetVipPanelBackgroundSprite()
    {
        if (vipPanelBackgroundSprite == null)
        {
            vipPanelBackgroundSprite = LoadSpriteFromResources("Art/UI/vip_panel_background_modular_blank");
        }

        return vipPanelBackgroundSprite;
    }

    private Sprite GetVipLeftBadgeSprite()
    {
        if (vipLeftBadgeSprite == null)
        {
            vipLeftBadgeSprite = LoadSpriteFromResources("Art/UI/vip_left_badge");
        }

        return vipLeftBadgeSprite;
    }

    private Sprite GetVipLeftProgressBarSprite()
    {
        if (vipLeftProgressBarSprite == null)
        {
            vipLeftProgressBarSprite = LoadSpriteFromResources("Art/UI/vip_left_progress_bar");
        }

        return vipLeftProgressBarSprite;
    }

    private Sprite GetVipLeftProgressCrownSprite()
    {
        if (vipLeftProgressCrownSprite == null)
        {
            vipLeftProgressCrownSprite = LoadSpriteFromResources("Art/UI/vip_left_progress_crown");
        }

        return vipLeftProgressCrownSprite;
    }

    private Sprite GetVipLeftGiftSprite()
    {
        if (vipLeftGiftSprite == null)
        {
            vipLeftGiftSprite = LoadSpriteFromResources("Art/UI/vip_left_gift");
        }

        return vipLeftGiftSprite;
    }

    private Sprite GetVipLeftClaimButtonSprite()
    {
        if (vipLeftClaimButtonSprite == null)
        {
            vipLeftClaimButtonSprite = LoadSpriteFromResources("Art/UI/vip_left_claim_button");
        }

        return vipLeftClaimButtonSprite;
    }

    private Sprite GetVipLeftBonusPillSprite()
    {
        if (vipLeftBonusPillSprite == null)
        {
            vipLeftBonusPillSprite = LoadSpriteFromResources("Art/UI/vip_left_bonus_pill");
        }

        return vipLeftBonusPillSprite;
    }

    private Sprite GetVipLeftDailyRewardLabelSprite()
    {
        if (vipLeftDailyRewardLabelSprite == null)
        {
            vipLeftDailyRewardLabelSprite = LoadSpriteFromResources("Art/UI/vip_left_daily_reward_label");
        }

        return vipLeftDailyRewardLabelSprite;
    }

    private Sprite GetVipInfoActivePackageBadgeSprite()
    {
        if (vipInfoActivePackageBadgeSprite == null)
        {
            vipInfoActivePackageBadgeSprite = LoadSpriteFromResources("Art/UI/vip_info_active_package_badge");
        }

        return vipInfoActivePackageBadgeSprite;
    }

    private Sprite GetVipInfoCurrentTitleSprite()
    {
        if (vipInfoCurrentTitleSprite == null)
        {
            vipInfoCurrentTitleSprite = LoadSpriteFromResources("Art/UI/vip_info_current_title");
        }

        return vipInfoCurrentTitleSprite;
    }

    private Sprite GetVipInfoNextTitleSprite()
    {
        if (vipInfoNextTitleSprite == null)
        {
            vipInfoNextTitleSprite = LoadSpriteFromResources("Art/UI/vip_info_next_title");
        }

        return vipInfoNextTitleSprite;
    }

    private Sprite GetVipInfoPriceLineSprite()
    {
        if (vipInfoPriceLineSprite == null)
        {
            vipInfoPriceLineSprite = LoadSpriteFromResources("Art/UI/vip_info_price_line");
        }

        return vipInfoPriceLineSprite;
    }

    private Sprite GetVipProfessionalPanelSprite()
    {
        if (vipProfessionalPanelSprite == null)
        {
            vipProfessionalPanelSprite = LoadSpriteFromResources("Art/UI/vip_info_panel_professional_blank");
        }

        return vipProfessionalPanelSprite;
    }

    private Sprite GetVipProfessionalIconSprite(int iconIndex)
    {
        if (vipProfessionalIconSprites.TryGetValue(iconIndex, out Sprite cachedSprite))
        {
            return cachedSprite;
        }

        if (vipProfessionalIconAtlasTexture == null)
        {
            vipProfessionalIconAtlasTexture = Resources.Load<Texture2D>("Art/UI/vip_info_icons_atlas");
        }

        if (vipProfessionalIconAtlasTexture == null)
        {
            return null;
        }

        Rect[] iconRects =
        {
            new Rect(88f, 439f, 354f, 413f),
            new Rect(534f, 498f, 300f, 269f),
            new Rect(931f, 515f, 344f, 274f),
            new Rect(1338f, 500f, 346f, 306f),
            new Rect(70f, 126f, 349f, 321f),
            new Rect(500f, 100f, 338f, 301f),
            new Rect(938f, 66f, 329f, 352f)
        };

        int safeIndex = Mathf.Clamp(iconIndex, 0, iconRects.Length - 1);
        Sprite sprite = Sprite.Create(
            vipProfessionalIconAtlasTexture,
            iconRects[safeIndex],
            new Vector2(0.5f, 0.5f),
            100f);
        vipProfessionalIconSprites[safeIndex] = sprite;
        return sprite;
    }

    private Sprite GetVipLockIconSprite()
    {
        if (vipLockIconSprite == null)
        {
            vipLockIconSprite = LoadSpriteFromResources("Art/UI/vip_lock_icon");
        }

        return vipLockIconSprite;
    }

    private Sprite GetFoodPanelBackgroundSprite()
    {
        if (foodPanelBackgroundSprite == null)
        {
            foodPanelBackgroundSprite = LoadImportedSpriteFromResources("Art/UI/Food/food_upgrade_panel_background_v3");
        }

        return foodPanelBackgroundSprite;
    }

    private Sprite GetFoodIconSprite(FoodUpgradeData data)
    {
        if (data == null || string.IsNullOrEmpty(data.IconResourceName))
        {
            return null;
        }

        if (!foodIconSprites.ContainsKey(data.IconResourceName))
        {
            foodIconSprites[data.IconResourceName] = LoadImportedSpriteFromResources("Art/UI/Food/" + data.IconResourceName);
        }

        return foodIconSprites[data.IconResourceName];
    }

    private Sprite GetFoodCardFrameSprite()
    {
        if (foodCardFrameSprite == null)
            foodCardFrameSprite = LoadImportedSpriteFromResources("Art/UI/Food/food_card_walnut");
        return foodCardFrameSprite;
    }

    private Sprite GetFoodCardLockedFrameSprite()
    {
        if (foodCardLockedFrameSprite == null)
            foodCardLockedFrameSprite = LoadImportedSpriteFromResources("Art/UI/Food/food_card_locked_frame");
        return foodCardLockedFrameSprite;
    }

    private Sprite GetFoodUpgradeButtonSprite()
    {
        if (foodUpgradeButtonSprite == null)
            foodUpgradeButtonSprite = LoadImportedSpriteFromResources("Art/UI/Food/food_upgrade_button");
        return foodUpgradeButtonSprite;
    }

    private Sprite GetFoodLockedButtonSprite()
    {
        if (foodLockedButtonSprite == null)
            foodLockedButtonSprite = LoadImportedSpriteFromResources("Art/UI/Food/food_locked_button");
        return foodLockedButtonSprite;
    }

    private Sprite GetFoodCategoryTabSelectedSprite()
    {
        if (foodCategoryTabSelectedSprite == null)
            foodCategoryTabSelectedSprite = LoadImportedSpriteFromResources("Art/UI/Food/food_category_tab_selected_v2");
        return foodCategoryTabSelectedSprite;
    }

    private Sprite GetFoodCategoryTabNormalSprite()
    {
        if (foodCategoryTabNormalSprite == null)
            foodCategoryTabNormalSprite = LoadImportedSpriteFromResources("Art/UI/Food/food_category_tab_normal_v2");
        return foodCategoryTabNormalSprite;
    }

    private Sprite GetFoodCategoryInfoRibbonSprite()
    {
        if (foodCategoryInfoRibbonSprite == null)
            foodCategoryInfoRibbonSprite = LoadImportedSpriteFromResources("Art/UI/Food/food_category_info_ribbon");
        return foodCategoryInfoRibbonSprite;
    }

    private Sprite GetFoodParchmentWatermarkSprite()
    {
        if (foodParchmentWatermarkSprite == null)
            foodParchmentWatermarkSprite = LoadImportedSpriteFromResources("Art/UI/Food/food_parchment_watermark");
        return foodParchmentWatermarkSprite;
    }

    private Sprite GetFoodLevelBadgeSprite()
    {
        if (foodLevelBadgeSprite == null)
            foodLevelBadgeSprite = LoadImportedSpriteFromResources("Art/UI/Food/food_level_badge_octagonal");
        return foodLevelBadgeSprite;
    }

    private Sprite GetFoodCoinChipSprite()
    {
        if (foodCoinChipSprite == null)
            foodCoinChipSprite = LoadImportedSpriteFromResources("Art/UI/Food/food_coin_chip");
        return foodCoinChipSprite;
    }

    private Sprite GetFoodPassiveChipSprite()
    {
        if (foodPassiveChipSprite == null)
            foodPassiveChipSprite = LoadImportedSpriteFromResources("Art/UI/Food/food_passive_chip");
        return foodPassiveChipSprite;
    }

    private Sprite GetFoodCloseButtonSprite()
    {
        if (foodCloseButtonSprite == null)
            foodCloseButtonSprite = LoadSpriteFromResources("Art/UI/Food/food_close_button");
        return foodCloseButtonSprite;
    }

    private Sprite GetVipCardSprite(int level)
    {
        int safeLevel = Mathf.Clamp(level, 1, MaxVipLevel);
        if (!vipCardSprites.ContainsKey(safeLevel))
        {
            if (vipCardAtlasTexture == null)
            {
                vipCardAtlasTexture = Resources.Load<Texture2D>("Art/UI/vip_cards_modular_atlas_v3");
            }

            if (vipCardAtlasTexture == null)
            {
                vipCardSprites[safeLevel] = LoadSpriteFromResources("Art/UI/vip_card_" + safeLevel);
            }
            else
            {
                Rect[] atlasRects =
                {
                    new Rect(64f, 94f, 309f, 651f),
                    new Rect(427f, 95f, 311f, 650f),
                    new Rect(788f, 95f, 314f, 650f),
                    new Rect(1151f, 95f, 313f, 650f),
                    new Rect(1513f, 95f, 312f, 650f)
                };
                vipCardSprites[safeLevel] = Sprite.Create(
                    vipCardAtlasTexture,
                    atlasRects[safeLevel - 1],
                    new Vector2(0.5f, 0.5f),
                    100f);
            }
        }

        return vipCardSprites[safeLevel];
    }

    private Sprite GetVipCardBottomStarSprite(int level)
    {
        int safeLevel = Mathf.Clamp(level, 1, MaxVipLevel);
        if (!vipCardBottomStarSprites.ContainsKey(safeLevel))
        {
            vipCardBottomStarSprites[safeLevel] = LoadSpriteFromResources("Art/UI/vip_card_bottom_star_" + safeLevel);
        }

        return vipCardBottomStarSprites[safeLevel];
    }

    private Sprite GetWarmWoodCafeSceneSprite()
    {
        if (warmWoodCafeSceneSprite == null)
        {
            warmWoodCafeSceneSprite = LoadSpriteFromResources("Art/CafeScenes/warm_wood_cafe_scene");
        }

        return warmWoodCafeSceneSprite;
    }

    private Sprite GetWarmWoodCafeSceneNoMachineSprite()
    {
        if (warmWoodCafeSceneNoMachineSprite == null)
        {
            warmWoodCafeSceneNoMachineSprite = LoadSpriteFromResources("Art/CafeScenes/warm_wood_cafe_scene_no_machine");
        }

        return warmWoodCafeSceneNoMachineSprite != null ? warmWoodCafeSceneNoMachineSprite : GetWarmWoodCafeSceneSprite();
    }

    private Sprite GetWorkshopWallSceneSprite(string variantKey)
    {
        if (string.IsNullOrEmpty(variantKey) || variantKey == "cream") return GetWarmWoodCafeSceneNoMachineSprite();
        if (workshopWallSceneSprites.TryGetValue(variantKey, out Sprite cached)) return cached;

        string path = "";
        if (variantKey == "walnut") path = "Art/CafeScenes/warm_wood_cafe_scene_wall_walnut";
        else if (variantKey == "rose") path = "Art/CafeScenes/warm_wood_cafe_scene_wall_rose";
        else if (variantKey == "mint") path = "Art/CafeScenes/warm_wood_cafe_scene_wall_mint";
        else if (variantKey == "burgundy") path = "Art/CafeScenes/warm_wood_cafe_scene_wall_burgundy";
        else if (variantKey == "navy") path = "Art/CafeScenes/warm_wood_cafe_scene_wall_navy";

        Sprite sprite = string.IsNullOrEmpty(path) ? null : LoadSpriteFromResources(path);
        if (sprite == null) sprite = GetWarmWoodCafeSceneNoMachineSprite();
        workshopWallSceneSprites[variantKey] = sprite;
        return sprite;
    }

    private Sprite GetActiveWorkshopWallSceneSprite(bool includeWorkshopPreview)
    {
        WorkshopDecorProduct wall = null;
        if (includeWorkshopPreview && decorPanel != null && decorPanel.activeSelf && selectedWorkshopCategory == WorkshopDecorCategory.Wall)
        {
            wall = GetWorkshopProduct(selectedWorkshopPreviewId);
        }
        if (wall == null && selectedWorkshopProducts.TryGetValue(WorkshopDecorCategory.Wall, out string selectedId))
        {
            wall = GetWorkshopProduct(selectedId);
        }
        return GetWorkshopWallSceneSprite(wall != null ? wall.VariantKey : "cream");
    }

    private Sprite GetActiveWorkshopEmptySceneSprite()
    {
        WorkshopDecorProduct wall = GetDisplayedWorkshopProduct(WorkshopDecorCategory.Wall);
        string variantKey = wall != null && !string.IsNullOrEmpty(wall.VariantKey) ? wall.VariantKey : "cream";
        if (workshopEmptySceneSprites.TryGetValue(variantKey, out Sprite cached))
        {
            return cached;
        }

        string path = "Art/CafeScenes/Modular/modular_empty_cafe_" + variantKey;
        Sprite sprite = LoadSpriteFromResources(path);
        if (sprite == null && variantKey != "cream")
        {
            sprite = LoadSpriteFromResources("Art/CafeScenes/Modular/modular_empty_cafe_cream");
        }
        if (sprite == null)
        {
            sprite = GetActiveWorkshopWallSceneSprite(true);
        }

        workshopEmptySceneSprites[variantKey] = sprite;
        return sprite;
    }

    private Sprite GetCoffeeMachineSceneSprite()
    {
        if (coffeeMachineSceneSprite == null)
        {
            coffeeMachineSceneSprite = LoadSpriteFromResources("Art/SceneObjects/coffee_machine_scene_sprite");
        }

        return coffeeMachineSceneSprite;
    }

    private Sprite GetCoffeeMachineLockedPlaceholderSprite()
    {
        if (coffeeMachineLockedPlaceholderSprite == null)
        {
            coffeeMachineLockedPlaceholderSprite = LoadSpriteFromResources("Art/SceneObjects/coffee_machine_locked_placeholder");
        }

        return coffeeMachineLockedPlaceholderSprite;
    }

    private Sprite GetBaristaSceneSprite()
    {
        if (baristaSceneSprite == null)
        {
            baristaSceneSprite = LoadSpriteFromResources("Art/SceneObjects/barista_unlocked");
        }

        return baristaSceneSprite;
    }

    private Sprite GetBaristaLockedPlaceholderSprite()
    {
        if (baristaLockedPlaceholderSprite == null)
        {
            baristaLockedPlaceholderSprite = LoadSpriteFromResources("Art/SceneObjects/barista_locked_placeholder");
        }

        return baristaLockedPlaceholderSprite;
    }

    private Sprite GetBaristaCounterForegroundSprite()
    {
        if (baristaCounterForegroundSprite == null)
        {
            baristaCounterForegroundSprite = LoadSpriteFromResources("Art/SceneObjects/barista_counter_foreground");
        }

        return baristaCounterForegroundSprite;
    }

    private Sprite GetCoinInfoPanelSprite()
    {
        if (coinInfoPanelSprite == null)
        {
            coinInfoPanelSprite = LoadImportedSpriteFromResources("Art/UI/HUD/coin_info_panel_v2");
        }

        return coinInfoPanelSprite;
    }

    private Sprite LoadSpriteFromResources(string path)
    {
        Texture2D texture = Resources.Load<Texture2D>(path);
        if (texture == null)
        {
            return null;
        }

        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
    }

    private Sprite LoadImportedSpriteFromResources(string path)
    {
        Sprite sprite = Resources.Load<Sprite>(path);
        if (sprite != null)
        {
            return sprite;
        }

        Sprite[] sprites = Resources.LoadAll<Sprite>(path);
        if (sprites != null && sprites.Length > 0)
        {
            return sprites[0];
        }

        return LoadSpriteFromResources(path);
    }

    private TMP_FontAsset GetVipPaytoneFontAsset()
    {
        if (vipPaytoneFontAsset != null)
        {
            return vipPaytoneFontAsset;
        }

        Font font = Resources.Load<Font>("Fonts/PaytoneOne-Regular");
        if (font == null)
        {
            return null;
        }

        vipPaytoneFontAsset = TMP_FontAsset.CreateFontAsset(font);
        if (vipPaytoneFontAsset != null)
        {
            vipPaytoneFontAsset.atlasPopulationMode = AtlasPopulationMode.Dynamic;
        }

        return vipPaytoneFontAsset;
    }

    private void ApplyVipFont(TMP_Text text)
    {
        TMP_FontAsset fontAsset = GetVipPaytoneFontAsset();
        if (text == null || fontAsset == null)
        {
            return;
        }

        text.font = fontAsset;
    }

    private void ApplyButtonSprite(TMP_Text buttonText, Sprite sprite)
    {
        if (buttonText == null || sprite == null)
        {
            return;
        }

        buttonText.text = "";
        Image buttonImage = buttonText.GetComponentInParent<Image>();
        if (buttonImage == null)
        {
            return;
        }

        buttonImage.sprite = sprite;
        buttonImage.color = Color.white;
        buttonImage.preserveAspect = true;
    }

    private void ConfigureAdBoostButton(TMP_Text buttonText)
    {
        if (buttonText == null)
        {
            return;
        }

        Transform buttonTransform = buttonText.transform.parent;
        if (buttonTransform != null)
        {
            Image buttonImage = buttonTransform.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.sprite = GetAdBoostIconSprite();
                buttonImage.color = Color.white;
                buttonImage.preserveAspect = true;
            }

            GameObject badgeObject = new GameObject("AdBoostTimerBadge", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            badgeObject.transform.SetParent(buttonTransform, false);

            RectTransform badgeRect = badgeObject.GetComponent<RectTransform>();
            badgeRect.anchorMin = new Vector2(0.5f, 0.5f);
            badgeRect.anchorMax = new Vector2(0.5f, 0.5f);
            badgeRect.anchoredPosition = new Vector2(0f, -59f);
            badgeRect.sizeDelta = new Vector2(106f, 32f);

            Image badgeImage = badgeObject.GetComponent<Image>();
            badgeImage.sprite = GetAdBoostTimerBadgeSprite();
            badgeImage.color = Color.white;
            badgeImage.preserveAspect = true;
            badgeImage.raycastTarget = false;
        }

        RectTransform textRect = buttonText.rectTransform;
        textRect.anchoredPosition = new Vector2(0f, -59f);
        textRect.sizeDelta = new Vector2(104f, 28f);
        buttonText.color = new Color(1f, 0.93f, 0.66f);
        buttonText.fontStyle = FontStyles.Bold;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.enableAutoSizing = true;
        buttonText.fontSizeMin = 13;
        buttonText.fontSizeMax = 20;
        buttonText.outlineColor = new Color(0.04f, 0.08f, 0.16f, 0.95f);
        buttonText.outlineWidth = 0.22f;
        buttonText.raycastTarget = false;
        buttonText.transform.SetAsLastSibling();
    }

    private void ConfigureVipHudButton(TMP_Text buttonText)
    {
        if (buttonText == null)
        {
            return;
        }

        Transform buttonTransform = buttonText.transform.parent;
        if (buttonTransform == null)
        {
            return;
        }

        Image buttonImage = buttonTransform.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = new Color(1f, 1f, 1f, 0f);
        }

        buttonText.text = "";
        buttonText.raycastTarget = false;

        GameObject iconObject = new GameObject("VipHudIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        iconObject.transform.SetParent(buttonTransform, false);
        RectTransform iconRect = iconObject.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.5f, 0.5f);
        iconRect.anchorMax = new Vector2(0.5f, 0.5f);
        iconRect.anchoredPosition = Vector2.zero;
        iconRect.sizeDelta = new Vector2(124, 124);

        vipHudIconImage = iconObject.GetComponent<Image>();
        vipHudIconImage.sprite = GetVipIconSprite();
        vipHudIconImage.color = Color.white;
        vipHudIconImage.preserveAspect = true;
        vipHudIconImage.raycastTarget = false;

        vipHudLevelText = CreateText(buttonTransform, "VipHudLevelText", "", new Vector2(24, -26), new Vector2(32, 24), 15);
        vipHudLevelText.color = Color.white;
        vipHudLevelText.fontStyle = FontStyles.Bold;
        vipHudLevelText.raycastTarget = false;
        vipHudLevelText.gameObject.SetActive(false);

        vipNotifyDot = CreateNotifyDot(buttonTransform, new Vector2(44, 48));
        RectTransform notifyRect = vipNotifyDot.GetComponent<RectTransform>();
        notifyRect.sizeDelta = new Vector2(34, 44);
    }

    public void OpenDecorPanel()
    {
        if (decorPanel == null)
        {
            return;
        }

        decorPanel.SetActive(true);
        decorPanel.transform.SetAsLastSibling();
        RefreshWorkshopPanel();
    }

    public void CloseDecorPanel()
    {
        if (decorPanel != null)
        {
            decorPanel.SetActive(false);
        }

        UpdateUI();
    }

    public void PreviewPinkWall()
    {
        previewDecorItem = 1;
        UpdateUI();
    }

    public void PreviewMintWall()
    {
        previewDecorItem = 2;
        UpdateUI();
    }

    public void PreviewCreamFloor()
    {
        previewDecorItem = 3;
        UpdateUI();
    }

    public void PreviewRoseTable()
    {
        previewDecorItem = 4;
        UpdateUI();
    }

    public void ConfirmDecorPreview()
    {
        WorkshopDecorProduct product = GetWorkshopProduct(selectedWorkshopPreviewId);
        if (product == null)
        {
            return;
        }

        if (!unlockedWorkshopProducts.Contains(product.Id))
        {
            if (!TrySpendWorkshopRecipe(product))
            {
                return;
            }

            unlockedWorkshopProducts.Add(product.Id);
        }

        selectedWorkshopProducts[product.Category] = product.Id;
        RefreshWorkshopPanel();
        ApplyDecorations();
        UpdateUI();
        SaveGame();
    }

    private void UnlockOrSelectDecorItem(DecorItemData item)
    {
        if (item == null)
        {
            return;
        }

        if (!IsDecorItemUnlocked(item))
        {
            if (!TrySpendDecorCost(item))
            {
                return;
            }

            SetDecorItemUnlocked(item, true);
        }

        if (item.Slot == DecorSlot.Wall)
        {
            selectedWallStyle = item.StyleId;
        }
        else if (item.Slot == DecorSlot.Floor)
        {
            selectedFloorStyle = item.StyleId;
        }
        else if (item.Slot == DecorSlot.Table)
        {
            selectedTableStyle = item.StyleId;
        }

        UpdateUI();
        SaveGame();
    }

    public void UnlockOrSelectPinkWall()
    {
        if (!pinkWallUnlocked)
        {
            if (!TrySpendPaintCans(DecorUnlockCost)) return;
            pinkWallUnlocked = true;
        }

        selectedWallStyle = 1;
        UpdateUI();
        SaveGame();
    }

    public void UnlockOrSelectMintWall()
    {
        if (!mintWallUnlocked)
        {
            if (!TrySpendPaintCans(DecorUnlockCost)) return;
            mintWallUnlocked = true;
        }

        selectedWallStyle = 2;
        UpdateUI();
        SaveGame();
    }

    public void UnlockOrSelectCreamFloor()
    {
        if (!creamFloorUnlocked)
        {
            if (!TrySpendPaintCans(2)) return;
            creamFloorUnlocked = true;
        }

        selectedFloorStyle = 1;
        UpdateUI();
        SaveGame();
    }

    public void UnlockOrSelectRoseTable()
    {
        if (!roseTableUnlocked)
        {
            if (woodPieces < 5) return;
            woodPieces -= 5;

            roseTableUnlocked = true;
        }

        selectedTableStyle = 1;
        UpdateUI();
        SaveGame();
    }

    private void CreateDecorPanel(Transform parent)
    {
        WorkshopLayoutConfig layout = WorkshopLayoutConfig.Load();
        decorPanel = new GameObject("DecorPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        decorPanel.transform.SetParent(parent, false);

        RectTransform panelRect = decorPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = new Vector2(0f, layout.panelY);
        panelRect.sizeDelta = new Vector2(layout.panelWidth, layout.panelHeight);
        panelRect.localScale = Vector3.one;

        Image panelImage = decorPanel.GetComponent<Image>();
        panelImage.sprite = GetWorkshopUiSprite("workshop_outer_frame");
        panelImage.color = Color.white;
        panelImage.preserveAspect = false;

        CreateWorkshopUiImage(decorPanel.transform, "WorkshopTitlePlaque", layout.title.Position, layout.title.Size, "workshop_title_plaque", false);
        TMP_Text title = CreateText(decorPanel.transform, "WorkshopTitle", "ATÖLYE", layout.titleText.Position, layout.titleText.Size, 48);
        title.color = new Color(1f, 0.82f, 0.36f, 1f);
        title.fontStyle = FontStyles.Bold;
        ApplyVipFont(title);

        TMP_Text closeText = CreateButton(decorPanel.transform, "WorkshopClose", "", layout.closeButton.Position, layout.closeButton.Size, CloseDecorPanel, Color.white, 1);
        Image closeImage = closeText.GetComponentInParent<Image>();
        if (closeImage != null)
        {
            closeImage.sprite = GetFoodCloseButtonSprite();
            closeImage.color = Color.white;
            closeImage.preserveAspect = true;
        }
        closeText.gameObject.SetActive(false);

        workshopCategoryTabTexts.Clear();
        workshopCategoryTabImages.Clear();
        workshopCategoryIconImages.Clear();
        for (int i = 0; i < 9; i++)
        {
            int capturedSlot = i;
            WorkshopDecorCategory category = (WorkshopDecorCategory)i;
            WorkshopLayoutRect categoryRect = layout.categories[i];
            TMP_Text tab = CreateButton(decorPanel.transform, "WorkshopTab_" + i, GetWorkshopCategoryName(category), categoryRect.Position, categoryRect.Size, () => SelectWorkshopCategorySlot(capturedSlot), Color.white, 21);
            tab.fontStyle = FontStyles.Bold;
            tab.enableAutoSizing = true;
            tab.fontSizeMin = 13;
            tab.fontSizeMax = 19;
            tab.margin = new Vector4(4f, 0f, 4f, 0f);
            ApplyVipFont(tab);
            RectTransform tabTextRect = tab.GetComponent<RectTransform>();
            tabTextRect.anchoredPosition = layout.categoryTexts[i].Position;
            tabTextRect.sizeDelta = layout.categoryTexts[i].Size;
            Image tabImage = tab.GetComponentInParent<Image>();
            if (tabImage != null)
            {
                tabImage.sprite = GetWorkshopUiSprite("workshop_category_button_clean");
                tabImage.color = Color.white;
                tabImage.preserveAspect = false;
                workshopCategoryTabImages.Add(tabImage);
            }
            Image categoryIcon = CreateWorkshopResourceIcon(tab.transform.parent, "CategoryIcon", layout.categoryIcons[i].Position, GetWorkshopCategoryIconSprite(category));
            categoryIcon.GetComponent<RectTransform>().sizeDelta = layout.categoryIcons[i].Size;
            categoryIcon.raycastTarget = false;
            workshopCategoryIconImages.Add(categoryIcon);
            workshopCategoryTabTexts.Add(tab);
        }

        workshopMaterialIconImages.Clear();
        workshopMaterialCountTexts.Clear();
        for (int i = 0; i < WorkshopDecorCatalog.CraftMaterials.Count; i++)
        {
            WorkshopCraftMaterialDefinition material = WorkshopDecorCatalog.CraftMaterials[i];
            WorkshopLayoutRect materialRect = layout.materials[i];
            Vector2 materialPosition = materialRect.Position;
            CreateWorkshopUiImage(decorPanel.transform, "WorkshopMaterialCard_" + material.Type, materialPosition, materialRect.Size, "workshop_material_counter_clean", false);
            Image icon = CreateWorkshopResourceIcon(decorPanel.transform, "WorkshopMaterialIcon_" + material.Type, layout.materialIcons[i].Position, GetWorkshopMaterialSprite(material.Type));
            icon.GetComponent<RectTransform>().sizeDelta = layout.materialIcons[i].Size;
            icon.raycastTarget = false;
            TMP_Text materialName = CreateText(decorPanel.transform, "WorkshopMaterialName_" + material.Type, material.DisplayName.ToUpperInvariant(), layout.materialNames[i].Position, layout.materialNames[i].Size, 17);
            materialName.color = new Color(1f, 0.82f, 0.38f, 1f);
            materialName.fontStyle = FontStyles.Bold;
            materialName.enableAutoSizing = true;
            materialName.fontSizeMin = 11;
            materialName.fontSizeMax = 16;
            materialName.margin = new Vector4(2f, 0f, 2f, 0f);
            ApplyVipFont(materialName);
            materialName.raycastTarget = false;
            TMP_Text count = CreateText(decorPanel.transform, "WorkshopMaterialCount_" + material.Type, "", layout.materialCounts[i].Position, layout.materialCounts[i].Size, 26);
            count.color = new Color(1f, 0.92f, 0.70f, 1f);
            count.fontStyle = FontStyles.Bold;
            count.enableAutoSizing = true;
            count.fontSizeMin = 18;
            count.fontSizeMax = 25;
            count.margin = Vector4.zero;
            ApplyVipFont(count);
            count.raycastTarget = false;
            workshopMaterialIconImages.Add(icon);
            workshopMaterialCountTexts.Add(count);
        }

        workshopCafePreviewImage = CreateWorkshopResourceIcon(decorPanel.transform, "WorkshopCafePreview", layout.preview.Position, GetActiveWorkshopEmptySceneSprite());
        workshopCafePreviewImage.GetComponent<RectTransform>().sizeDelta = layout.preview.Size;
        workshopCafePreviewImage.preserveAspect = false;
        workshopCafePreviewImage.raycastTarget = false;
        Mask workshopPreviewMask = workshopCafePreviewImage.gameObject.GetComponent<Mask>();
        if (workshopPreviewMask == null)
        {
            workshopPreviewMask = workshopCafePreviewImage.gameObject.AddComponent<Mask>();
        }
        workshopPreviewMask.showMaskGraphic = true;
        CreateWorkshopCafePreviewObjectLayers(workshopCafePreviewImage.transform);
        CreateWorkshopUiImage(decorPanel.transform, "WorkshopCafePreviewFrame", layout.previewFrame.Position, layout.previewFrame.Size, "workshop_preview_frame_wide", false).raycastTarget = false;

        CreateWorkshopUiImage(decorPanel.transform, "WorkshopDetailPanel", layout.detailPanel.Position, layout.detailPanel.Size, "workshop_detail_panel_clean", false);

        workshopCategoryTitleText = CreateText(decorPanel.transform, "WorkshopDetailCategory", "", layout.detailCategory.Position, layout.detailCategory.Size, 20);
        workshopCategoryTitleText.color = new Color(0.34f, 0.13f, 0.035f, 1f);
        workshopCategoryTitleText.fontStyle = FontStyles.Bold;
        workshopCategoryTitleText.margin = Vector4.zero;
        ApplyVipFont(workshopCategoryTitleText);
        workshopCategoryTitleText.gameObject.SetActive(true);

        GameObject selectedProductMaskObject = new GameObject("WorkshopSelectedProductMask", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Mask));
        selectedProductMaskObject.transform.SetParent(decorPanel.transform, false);
        RectTransform selectedProductMaskRect = selectedProductMaskObject.GetComponent<RectTransform>();
        selectedProductMaskRect.anchorMin = new Vector2(0.5f, 0.5f);
        selectedProductMaskRect.anchorMax = new Vector2(0.5f, 0.5f);
        selectedProductMaskRect.anchoredPosition = layout.selectedProduct.Position;
        selectedProductMaskRect.sizeDelta = layout.selectedProduct.Size;
        Image selectedProductMaskImage = selectedProductMaskObject.GetComponent<Image>();
        selectedProductMaskImage.sprite = CreateRoundedRectSprite(
            layout.selectedProduct.Size,
            Color.white,
            Color.white,
            Mathf.RoundToInt(Mathf.Min(layout.selectedProduct.width, layout.selectedProduct.height) * 0.5f),
            0);
        selectedProductMaskImage.color = Color.white;
        selectedProductMaskImage.raycastTarget = false;
        selectedProductMaskObject.GetComponent<Mask>().showMaskGraphic = false;

        workshopSelectedProductImage = CreateWorkshopResourceIcon(selectedProductMaskObject.transform, "WorkshopSelectedProduct", Vector2.zero, null);
        workshopSelectedProductImage.GetComponent<RectTransform>().sizeDelta = layout.selectedProduct.Size;
        workshopSelectedProductImage.preserveAspect = true;
        CreateWorkshopUiImage(decorPanel.transform, "WorkshopSelectedProductFrame", layout.selectedProductFrame.Position, layout.selectedProductFrame.Size, "workshop_selected_product_frame", false).raycastTarget = false;
        decorDetailTitleText = CreateText(decorPanel.transform, "WorkshopDetailTitle", "", layout.detailTitle.Position, layout.detailTitle.Size, 30);
        decorDetailTitleText.color = new Color(0.34f, 0.13f, 0.035f, 1f);
        decorDetailTitleText.fontStyle = FontStyles.Bold;
        decorDetailTitleText.enableAutoSizing = true;
        decorDetailTitleText.fontSizeMin = 18;
        decorDetailTitleText.fontSizeMax = 27;
        decorDetailTitleText.margin = new Vector4(8f, 0f, 8f, 0f);
        ApplyVipFont(decorDetailTitleText);
        decorDetailStatusText = CreateText(decorPanel.transform, "WorkshopRarity", "", layout.detailRarity.Position, layout.detailRarity.Size, 20);
        decorDetailStatusText.fontStyle = FontStyles.Bold;
        decorDetailStatusText.margin = Vector4.zero;
        ApplyVipFont(decorDetailStatusText);
        TMP_Text effectHeading = CreateText(decorPanel.transform, "WorkshopEffectHeading", "KAFE ETKİLERİ", layout.effectHeading.Position, layout.effectHeading.Size, 20);
        effectHeading.color = new Color(0.34f, 0.13f, 0.035f, 1f);
        effectHeading.fontStyle = FontStyles.Bold;
        effectHeading.enableAutoSizing = true;
        effectHeading.fontSizeMin = 14;
        effectHeading.fontSizeMax = 18;
        effectHeading.margin = Vector4.zero;
        ApplyVipFont(effectHeading);
        effectHeading.raycastTarget = false;
        TMP_Text requirementHeading = CreateText(decorPanel.transform, "WorkshopRequirementHeading", "ÜRETİM GEREKSİNİMLERİ", layout.requirementHeading.Position, layout.requirementHeading.Size, 20);
        requirementHeading.color = new Color(0.34f, 0.13f, 0.035f, 1f);
        requirementHeading.fontStyle = FontStyles.Bold;
        requirementHeading.enableAutoSizing = true;
        requirementHeading.fontSizeMin = 14;
        requirementHeading.fontSizeMax = 18;
        requirementHeading.margin = Vector4.zero;
        ApplyVipFont(requirementHeading);
        requirementHeading.raycastTarget = false;
        workshopEffectFrameImages.Clear();
        workshopEffectTexts.Clear();
        for (int i = 0; i < 2; i++)
        {
            WorkshopLayoutRect effectRect = layout.effectRows[i];
            Image effectFrame = CreateWorkshopUiImage(decorPanel.transform, "WorkshopEffectFrame_" + i, effectRect.Position, effectRect.Size, "workshop_effect_row", false);
            effectFrame.raycastTarget = false;
            TMP_Text effectText = CreateText(decorPanel.transform, "WorkshopEffectText_" + i, "", layout.effectTexts[i].Position, layout.effectTexts[i].Size, 18);
            effectText.color = new Color(0.28f, 0.12f, 0.035f, 1f);
            effectText.fontStyle = FontStyles.Bold;
            effectText.enableAutoSizing = true;
            effectText.fontSizeMin = 14;
            effectText.fontSizeMax = 17;
            effectText.margin = new Vector4(8f, 0f, 8f, 0f);
            ApplyVipFont(effectText);
            effectText.raycastTarget = false;
            workshopEffectFrameImages.Add(effectFrame);
            workshopEffectTexts.Add(effectText);
        }
        workshopRequirementFrameImages.Clear();
        workshopRequirementIconImages.Clear();
        workshopRequirementTexts.Clear();
        workshopRequirementCountTexts.Clear();
        for (int i = 0; i < 2; i++)
        {
            WorkshopLayoutRect requirementRect = layout.requirementCards[i];
            Vector2 requirementPosition = requirementRect.Position;
            Image requirementFrame = CreateWorkshopUiImage(decorPanel.transform, "WorkshopRequirementFrame_" + i, requirementPosition, requirementRect.Size, "workshop_requirement_card_clean", false);
            requirementFrame.raycastTarget = false;
            Image requirementIcon = CreateWorkshopResourceIcon(decorPanel.transform, "WorkshopRequirementIcon_" + i, layout.requirementIcons[i].Position, null);
            requirementIcon.GetComponent<RectTransform>().sizeDelta = layout.requirementIcons[i].Size;
            requirementIcon.raycastTarget = false;
            TMP_Text requirementText = CreateText(decorPanel.transform, "WorkshopRequirementText_" + i, "", layout.requirementNames[i].Position, layout.requirementNames[i].Size, 16);
            requirementText.color = new Color(1f, 0.82f, 0.38f, 1f);
            requirementText.fontStyle = FontStyles.Bold;
            requirementText.enableAutoSizing = true;
            requirementText.fontSizeMin = 12;
            requirementText.fontSizeMax = 15;
            requirementText.margin = Vector4.zero;
            ApplyVipFont(requirementText);
            requirementText.raycastTarget = false;
            TMP_Text requirementCountText = CreateText(decorPanel.transform, "WorkshopRequirementCount_" + i, "", layout.requirementCounts[i].Position, layout.requirementCounts[i].Size, 25);
            requirementCountText.color = new Color(0.38f, 0.85f, 0.35f, 1f);
            requirementCountText.fontStyle = FontStyles.Bold;
            requirementCountText.enableAutoSizing = true;
            requirementCountText.fontSizeMin = 18;
            requirementCountText.fontSizeMax = 24;
            requirementCountText.margin = Vector4.zero;
            ApplyVipFont(requirementCountText);
            requirementCountText.raycastTarget = false;
            workshopRequirementFrameImages.Add(requirementFrame);
            workshopRequirementIconImages.Add(requirementIcon);
            workshopRequirementTexts.Add(requirementText);
            workshopRequirementCountTexts.Add(requirementCountText);
        }
        decorActionButtonText = CreateButton(decorPanel.transform, "WorkshopAction", "ÜRET", layout.actionButton.Position, layout.actionButton.Size, ConfirmDecorPreview, Color.white, 32);
        decorActionButtonText.fontStyle = FontStyles.Bold;
        decorActionButtonText.enableAutoSizing = true;
        decorActionButtonText.fontSizeMin = 22;
        decorActionButtonText.fontSizeMax = 30;
        decorActionButtonText.margin = Vector4.zero;
        ApplyVipFont(decorActionButtonText);
        RectTransform actionTextRect = decorActionButtonText.GetComponent<RectTransform>();
        actionTextRect.anchoredPosition = layout.actionText.Position;
        actionTextRect.sizeDelta = layout.actionText.Size;
        Image actionImage = decorActionButtonText.GetComponentInParent<Image>();
        if (actionImage != null)
        {
            actionImage.sprite = GetWorkshopUiSprite("workshop_action_button");
            actionImage.color = Color.white;
            actionImage.preserveAspect = false;
        }

        workshopProductCards.Clear();
        for (int i = 0; i < 6; i++)
        {
            int capturedIndex = i;
            WorkshopLayoutRect productRect = layout.productCards[i];
            TMP_Text buttonText = CreateButton(decorPanel.transform, "WorkshopProductCard_" + i, "", productRect.Position, productRect.Size, () => SelectWorkshopProduct(capturedIndex), Color.white, 1);
            GameObject root = buttonText.transform.parent.gameObject;
            Image rootImage = root.GetComponent<Image>();
            rootImage.sprite = GetWorkshopUiSprite("workshop_product_card");
            rootImage.color = Color.white;
            rootImage.preserveAspect = false;
            Image productImage = CreateWorkshopResourceIcon(root.transform, "ProductImage", layout.productImages[i].Position, null);
            productImage.GetComponent<RectTransform>().sizeDelta = layout.productImages[i].Size;
            TMP_Text nameText = CreateText(root.transform, "Name", "", layout.productNames[i].Position, layout.productNames[i].Size, 15);
            nameText.color = new Color(1f, 0.88f, 0.58f, 1f);
            nameText.fontStyle = FontStyles.Bold;
            nameText.enableAutoSizing = true;
            nameText.fontSizeMin = 11;
            nameText.fontSizeMax = 14;
            nameText.margin = new Vector4(3f, 0f, 3f, 0f);
            ApplyVipFont(nameText);
            TMP_Text rarityText = CreateText(root.transform, "Rarity", "", layout.productRarities[i].Position, layout.productRarities[i].Size, 12);
            rarityText.fontStyle = FontStyles.Bold;
            rarityText.enableAutoSizing = true;
            rarityText.fontSizeMin = 9;
            rarityText.fontSizeMax = 12;
            rarityText.margin = Vector4.zero;
            ApplyVipFont(rarityText);
            TMP_Text statusText = CreateText(root.transform, "Status", "", layout.productStatuses[i].Position, layout.productStatuses[i].Size, 12);
            statusText.fontStyle = FontStyles.Bold;
            statusText.enableAutoSizing = true;
            statusText.fontSizeMin = 9;
            statusText.fontSizeMax = 12;
            statusText.margin = Vector4.zero;
            ApplyVipFont(statusText);
            workshopProductCards.Add(new WorkshopProductCardUi
            {
                Root = root,
                Background = root.GetComponent<Image>(),
                ProductImage = productImage,
                NameText = nameText,
                RarityText = rarityText,
                StatusText = statusText,
                Button = root.GetComponent<Button>()
            });
        }

        if (layout.referenceOverlayAlpha > 0.001f)
        {
            Image referenceOverlay = CreateWorkshopResourceIcon(decorPanel.transform, "WorkshopReferenceOverlay", Vector2.zero, LoadSpriteFromResources("Art/UI/Workshop/workshop_layout_reference"));
            referenceOverlay.GetComponent<RectTransform>().sizeDelta = new Vector2(layout.panelWidth, layout.panelHeight);
            referenceOverlay.preserveAspect = false;
            referenceOverlay.raycastTarget = false;
            referenceOverlay.color = new Color(1f, 1f, 1f, Mathf.Clamp01(layout.referenceOverlayAlpha));
        }

        decorPanel.transform.SetAsLastSibling();
        decorPanel.SetActive(false);
        RefreshWorkshopPanel();
    }

    private void SelectWorkshopCategory(WorkshopDecorCategory category)
    {
        selectedWorkshopCategory = category;
        workshopCategoryPage = 0;
        List<WorkshopDecorProduct> products = GetWorkshopProductsForCategory(category);
        if (products.Count > 0)
        {
            selectedWorkshopPreviewId = products[0].Id;
        }
        RefreshWorkshopPanel();
    }

    private void SelectWorkshopCategorySlot(int slot)
    {
        int categoryIndex = slot;
        if (!Enum.IsDefined(typeof(WorkshopDecorCategory), categoryIndex)) return;
        SelectWorkshopCategory((WorkshopDecorCategory)categoryIndex);
    }

    private void CycleWorkshopCategoryPage()
    {
        workshopCategoryPage = workshopCategoryPage == 0 ? 1 : 0;
        RefreshWorkshopPanel();
    }

    private void SelectWorkshopProduct(int index)
    {
        List<WorkshopDecorProduct> products = GetWorkshopProductsForCategory(selectedWorkshopCategory);
        if (index < 0 || index >= products.Count)
        {
            return;
        }

        selectedWorkshopPreviewId = products[index].Id;
        RefreshWorkshopPanel();
    }

    private void RefreshWorkshopPanel()
    {
        if (decorPanel == null)
        {
            return;
        }

        List<WorkshopDecorProduct> products = GetWorkshopProductsForCategory(selectedWorkshopCategory);
        WorkshopDecorProduct selected = GetWorkshopProduct(selectedWorkshopPreviewId);
        if (selected == null || selected.Category != selectedWorkshopCategory)
        {
            selected = products.Count > 0 ? products[0] : null;
            selectedWorkshopPreviewId = selected != null ? selected.Id : "";
        }

        if (workshopCategoryTitleText != null)
        {
            workshopCategoryTitleText.text = GetWorkshopCategoryName(selectedWorkshopCategory).ToUpperInvariant();
        }

        for (int i = 0; i < workshopCategoryTabTexts.Count; i++)
        {
            TMP_Text tab = workshopCategoryTabTexts[i];
            int categoryIndex = i;
            bool valid = Enum.IsDefined(typeof(WorkshopDecorCategory), categoryIndex);
            tab.transform.parent.gameObject.SetActive(valid);
            if (!valid) continue;
            WorkshopDecorCategory tabCategory = (WorkshopDecorCategory)categoryIndex;
            tab.text = GetWorkshopCategoryName(tabCategory);
            bool active = categoryIndex == (int)selectedWorkshopCategory;
            tab.color = active ? new Color(1f, 0.90f, 0.58f, 1f) : new Color(0.30f, 0.13f, 0.025f, 1f);
            if (i < workshopCategoryTabImages.Count && workshopCategoryTabImages[i] != null)
            {
                workshopCategoryTabImages[i].color = active ? new Color(0.10f, 0.72f, 0.66f, 1f) : Color.white;
            }
        }

        for (int i = 0; i < workshopProductCards.Count; i++)
        {
            WorkshopProductCardUi card = workshopProductCards[i];
            if (i >= products.Count)
            {
                card.Root.SetActive(false);
                continue;
            }

            WorkshopDecorProduct product = products[i];
            card.Root.SetActive(true);
            card.Product = product;
            card.ProductImage.sprite = GetWorkshopProductSprite(product.Id);
            card.ProductImage.gameObject.SetActive(card.ProductImage.sprite != null);
            card.NameText.text = GetWorkshopCardDisplayName(product);
            card.RarityText.text = WorkshopDecorCatalog.GetRarityDisplayName(product.Rarity);
            card.RarityText.color = GetWorkshopRarityTextColor(product.Rarity);
            bool unlocked = unlockedWorkshopProducts.Contains(product.Id);
            bool equipped = IsWorkshopProductSelected(product);
            card.StatusText.text = equipped ? "KULLANILIYOR" : unlocked ? "ÜRETİLDİ" : "KİLİTLİ";
            card.StatusText.color = equipped ? new Color(0.05f, 0.43f, 0.24f, 1f) : unlocked ? new Color(0.48f, 0.25f, 0.05f, 1f) : new Color(0.58f, 0.12f, 0.08f, 1f);
            card.Background.color = product.Id == selectedWorkshopPreviewId ? new Color(0.78f, 1f, 0.94f, 1f) : Color.white;
        }

        if (selected == null)
        {
            return;
        }

        bool selectedUnlocked = unlockedWorkshopProducts.Contains(selected.Id);
        bool selectedEquipped = IsWorkshopProductSelected(selected);
        if (workshopSelectedProductImage != null)
        {
            workshopSelectedProductImage.sprite = GetWorkshopProductSprite(selected.Id);
            workshopSelectedProductImage.gameObject.SetActive(workshopSelectedProductImage.sprite != null);
        }
        if (decorDetailTitleText != null) decorDetailTitleText.text = selected.DisplayName;
        if (decorDetailStatusText != null)
        {
            decorDetailStatusText.text = WorkshopDecorCatalog.GetRarityDisplayName(selected.Rarity);
            decorDetailStatusText.color = GetWorkshopRarityTextColor(selected.Rarity);
        }
        for (int i = 0; i < workshopEffectTexts.Count; i++)
        {
            bool visible = i == 0 || selected.HasSecondaryEffect;
            workshopEffectTexts[i].gameObject.SetActive(visible);
            if (i < workshopEffectFrameImages.Count) workshopEffectFrameImages[i].gameObject.SetActive(visible);
            if (visible)
            {
                WorkshopCafeEffectType effect = i == 0 ? selected.PrimaryEffect : selected.SecondaryEffect;
                int value = i == 0 ? selected.PrimaryEffectValue : selected.SecondaryEffectValue;
                workshopEffectTexts[i].text = WorkshopDecorCatalog.GetEffectDisplayName(effect) + "  " + WorkshopDecorCatalog.GetEffectValueText(effect, value);
            }
        }
        for (int i = 0; i < workshopRequirementTexts.Count; i++)
        {
            bool hasRequirement = selected.Recipe != null && i < selected.Recipe.Length;
            workshopRequirementTexts[i].gameObject.SetActive(hasRequirement);
            if (i < workshopRequirementFrameImages.Count) workshopRequirementFrameImages[i].gameObject.SetActive(hasRequirement);
            workshopRequirementIconImages[i].gameObject.SetActive(hasRequirement);
            workshopRequirementCountTexts[i].gameObject.SetActive(hasRequirement);
            if (hasRequirement)
            {
                WorkshopCraftRequirement requirement = selected.Recipe[i];
                workshopRequirementIconImages[i].sprite = GetWorkshopMaterialSprite(requirement.Material);
                workshopRequirementTexts[i].text = GetWorkshopMaterialName(requirement.Material).ToUpperInvariant();
                workshopRequirementCountTexts[i].text = GetWorkshopCraftMaterialCount(requirement.Material) + " / " + requirement.Amount;
                Color requirementColor = GetWorkshopCraftMaterialCount(requirement.Material) >= requirement.Amount
                    ? new Color(0.08f, 0.40f, 0.22f, 1f)
                    : new Color(0.58f, 0.12f, 0.07f, 1f);
                workshopRequirementTexts[i].color = requirementColor;
                workshopRequirementCountTexts[i].color = requirementColor;
            }
        }
        if (decorActionButtonText != null)
        {
            decorActionButtonText.text = selectedEquipped ? "KULLANILIYOR" : selectedUnlocked ? "KULLAN" : "ÜRET";
            Button action = decorActionButtonText.GetComponentInParent<Button>();
            if (action != null) action.interactable = !selectedEquipped && (selectedUnlocked || CanCraftWorkshopProduct(selected));
        }
        for (int i = 0; i < workshopMaterialCountTexts.Count; i++)
        {
            bool validMaterial = i < WorkshopDecorCatalog.CraftMaterials.Count;
            workshopMaterialCountTexts[i].gameObject.SetActive(validMaterial);
            if (i < workshopMaterialIconImages.Count) workshopMaterialIconImages[i].gameObject.SetActive(validMaterial);
            if (validMaterial)
            {
                WorkshopCraftMaterialDefinition material = WorkshopDecorCatalog.CraftMaterials[i];
                workshopMaterialCountTexts[i].text = GetWorkshopCraftMaterialCount(material.Type).ToString();
                if (i < workshopMaterialIconImages.Count)
                {
                    workshopMaterialIconImages[i].sprite = GetWorkshopMaterialSprite(material.Type);
                }
            }
        }
        if (decorPreviewBackWallImage != null) decorPreviewBackWallImage.color = GetWallColor();
        if (decorPreviewLeftWallImage != null) decorPreviewLeftWallImage.color = ShadeColor(GetWallColor(), 0.84f);
        if (decorPreviewRightWallImage != null) decorPreviewRightWallImage.color = ShadeColor(GetWallColor(), 0.78f);
        if (decorPreviewFloorImage != null) decorPreviewFloorImage.color = GetFloorColor();
        if (decorPreviewTableTopImage != null) decorPreviewTableTopImage.color = GetTableColor();
        if (workshopCafePreviewImage != null) workshopCafePreviewImage.sprite = GetActiveWorkshopEmptySceneSprite();
        UpdateWorkshopCafePreviewObjectLayers();
    }

    private void CreateWorkshopCafePreviewObjectLayers(Transform previewParent)
    {
        workshopCafePreviewObjectImages.Clear();
        CreateWorkshopCafePreviewFloorLayer(previewParent);
        CreateWorkshopCafePreviewObjectLayer(previewParent, WorkshopDecorCategory.Rug, new Vector2(0f, -115f), new Vector2(355f, 175f));
        CreateWorkshopCafePreviewObjectLayer(previewParent, WorkshopDecorCategory.Counter, new Vector2(0f, -18f), new Vector2(335f, 210f));
        CreateWorkshopCafePreviewObjectLayer(previewParent, WorkshopDecorCategory.Shelf, new Vector2(-155f, 88f), new Vector2(165f, 112f));
        CreateWorkshopCafePreviewObjectLayer(previewParent, WorkshopDecorCategory.Shelf, new Vector2(125f, 82f), new Vector2(165f, 112f));
        CreateWorkshopCafePreviewObjectLayer(previewParent, WorkshopDecorCategory.WallArt, new Vector2(-120f, 112f), new Vector2(88f, 112f));
        CreateWorkshopCafePreviewObjectLayer(previewParent, WorkshopDecorCategory.WallArt, new Vector2(190f, 114f), new Vector2(82f, 106f));
        CreateWorkshopCafePreviewObjectLayer(previewParent, WorkshopDecorCategory.Table, new Vector2(-145f, -126f), new Vector2(150f, 125f));
        CreateWorkshopCafePreviewObjectLayer(previewParent, WorkshopDecorCategory.Table, new Vector2(150f, -126f), new Vector2(150f, 125f));
        CreateWorkshopCafePreviewObjectLayer(previewParent, WorkshopDecorCategory.Chair, new Vector2(-265f, -118f), new Vector2(82f, 122f));
        CreateWorkshopCafePreviewObjectLayer(previewParent, WorkshopDecorCategory.Chair, new Vector2(-55f, -120f), new Vector2(82f, 122f));
        CreateWorkshopCafePreviewObjectLayer(previewParent, WorkshopDecorCategory.Chair, new Vector2(60f, -120f), new Vector2(82f, 122f));
        CreateWorkshopCafePreviewObjectLayer(previewParent, WorkshopDecorCategory.Chair, new Vector2(270f, -118f), new Vector2(82f, 122f));
        CreateWorkshopCafePreviewObjectLayer(previewParent, WorkshopDecorCategory.Plant, new Vector2(-350f, -82f), new Vector2(105f, 142f));
        CreateWorkshopCafePreviewObjectLayer(previewParent, WorkshopDecorCategory.Plant, new Vector2(350f, -82f), new Vector2(105f, 142f));
    }

    private void CreateWorkshopCafePreviewFloorLayer(Transform parent)
    {
        GameObject floorObject = new GameObject("WorkshopCafePreview_FloorPerspective", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextureQuadGraphic));
        floorObject.transform.SetParent(parent, false);

        RectTransform rect = floorObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(760f, 360f);

        workshopCafePreviewFloorGraphic = floorObject.GetComponent<TextureQuadGraphic>();
        workshopCafePreviewFloorGraphic.SetCorners(
            new Vector2(-290f, -28f),
            new Vector2(290f, -28f),
            new Vector2(348f, -188f),
            new Vector2(-348f, -188f));
        workshopCafePreviewFloorGraphic.color = Color.white;
        workshopCafePreviewFloorGraphic.raycastTarget = false;
        floorObject.SetActive(false);
    }

    private void CreateWorkshopCafePreviewObjectLayer(Transform parent, WorkshopDecorCategory category, Vector2 position, Vector2 size)
    {
        GameObject layerObject = new GameObject("WorkshopCafePreview_" + category, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        layerObject.transform.SetParent(parent, false);

        RectTransform rect = layerObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image image = layerObject.GetComponent<Image>();
        image.color = Color.white;
        image.preserveAspect = true;
        image.raycastTarget = false;
        layerObject.SetActive(false);
        if (!workshopCafePreviewObjectImages.TryGetValue(category, out List<Image> categoryImages))
        {
            categoryImages = new List<Image>();
            workshopCafePreviewObjectImages[category] = categoryImages;
        }
        categoryImages.Add(image);
    }

    private void UpdateWorkshopCafePreviewObjectLayers()
    {
        if (workshopCafePreviewFloorGraphic != null)
        {
            WorkshopDecorProduct floorProduct = GetDisplayedWorkshopProduct(WorkshopDecorCategory.Floor);
            Sprite floorSprite = floorProduct != null ? GetWorkshopProductSprite(floorProduct.Id) : null;
            workshopCafePreviewFloorGraphic.SetSprite(floorSprite);
            workshopCafePreviewFloorGraphic.gameObject.SetActive(floorSprite != null);
        }

        foreach (KeyValuePair<WorkshopDecorCategory, List<Image>> pair in workshopCafePreviewObjectImages)
        {
            WorkshopDecorProduct product = GetDisplayedWorkshopProduct(pair.Key);
            Sprite sprite = product != null ? GetWorkshopProductSprite(product.Id) : null;
            List<Image> images = pair.Value;
            for (int i = 0; i < images.Count; i++)
            {
                Image image = images[i];
                if (image == null) continue;
                image.sprite = sprite;
                image.gameObject.SetActive(sprite != null);
            }
        }
    }

    private List<WorkshopDecorProduct> GetWorkshopProductsForCategory(WorkshopDecorCategory category)
    {
        List<WorkshopDecorProduct> result = new List<WorkshopDecorProduct>(6);
        for (int i = 0; i < WorkshopDecorCatalog.Products.Count; i++)
        {
            WorkshopDecorProduct product = WorkshopDecorCatalog.Products[i];
            if (product.Category == category) result.Add(product);
        }
        return result;
    }

    private WorkshopDecorProduct GetWorkshopProduct(string id)
    {
        for (int i = 0; i < WorkshopDecorCatalog.Products.Count; i++)
        {
            WorkshopDecorProduct product = WorkshopDecorCatalog.Products[i];
            if (string.Equals(product.Id, id, StringComparison.Ordinal)) return product;
        }
        return null;
    }

    private bool IsWorkshopProductSelected(WorkshopDecorProduct product)
    {
        return product != null && selectedWorkshopProducts.TryGetValue(product.Category, out string selectedId) && selectedId == product.Id;
    }

    private bool CanCraftWorkshopProduct(WorkshopDecorProduct product)
    {
        if (product == null || product.Recipe == null) return false;
        for (int i = 0; i < product.Recipe.Length; i++)
        {
            WorkshopCraftRequirement requirement = product.Recipe[i];
            if (GetWorkshopCraftMaterialCount(requirement.Material) < requirement.Amount) return false;
        }
        return true;
    }

    private bool TrySpendWorkshopRecipe(WorkshopDecorProduct product)
    {
        if (!CanCraftWorkshopProduct(product)) return false;
        for (int i = 0; i < product.Recipe.Length; i++)
        {
            WorkshopCraftRequirement requirement = product.Recipe[i];
            if (requirement.Material == WorkshopCraftMaterialType.Paint)
            {
                TrySpendPaintCans(requirement.Amount);
            }
            else if (requirement.Material == WorkshopCraftMaterialType.Wood)
            {
                woodPieces -= requirement.Amount;
            }
            else
            {
                string itemId = GetWorkshopMaterialInventoryId(requirement.Material);
                itemInventory[itemId] = Mathf.Max(0, GetInventoryItemCount(itemId) - requirement.Amount);
            }
        }
        return true;
    }

    private string BuildWorkshopRecipeText(WorkshopDecorProduct product)
    {
        if (product == null || product.Recipe == null) return "";
        string text = "GEREKEN: ";
        for (int i = 0; i < product.Recipe.Length; i++)
        {
            WorkshopCraftRequirement requirement = product.Recipe[i];
            if (i > 0) text += "  •  ";
            text += GetWorkshopMaterialName(requirement.Material) + " " + GetWorkshopCraftMaterialCount(requirement.Material) + "/" + requirement.Amount;
        }
        return text;
    }

    private string BuildWorkshopMaterialSummary()
    {
        string text = "MALZEMELER   ";
        for (int i = 0; i < WorkshopDecorCatalog.CraftMaterials.Count; i++)
        {
            WorkshopCraftMaterialDefinition material = WorkshopDecorCatalog.CraftMaterials[i];
            if (i > 0) text += "   ";
            text += material.DisplayName + " " + GetWorkshopCraftMaterialCount(material.Type);
        }
        return text;
    }

    private string GetWorkshopMaterialName(WorkshopCraftMaterialType material)
    {
        for (int i = 0; i < WorkshopDecorCatalog.CraftMaterials.Count; i++)
        {
            if (WorkshopDecorCatalog.CraftMaterials[i].Type == material) return WorkshopDecorCatalog.CraftMaterials[i].DisplayName;
        }
        return material.ToString();
    }

    private static string GetWorkshopCategoryName(WorkshopDecorCategory category)
    {
        switch (category)
        {
            case WorkshopDecorCategory.Wall: return "DUVAR";
            case WorkshopDecorCategory.Floor: return "ZEMİN";
            case WorkshopDecorCategory.Table: return "MASA";
            case WorkshopDecorCategory.Chair: return "SANDALYE";
            case WorkshopDecorCategory.Rug: return "HALI";
            case WorkshopDecorCategory.Shelf: return "RAF";
            case WorkshopDecorCategory.Counter: return "TEZGÂH";
            case WorkshopDecorCategory.Plant: return "BİTKİ";
            case WorkshopDecorCategory.WallArt: return "TABLO";
            default: return "DEKOR";
        }
    }

    private static string GetWorkshopCardDisplayName(WorkshopDecorProduct product)
    {
        if (product == null) return "";
        string suffix = "";
        switch (product.Category)
        {
            case WorkshopDecorCategory.Wall: suffix = " Duvar"; break;
            case WorkshopDecorCategory.Floor: suffix = " Zemin"; break;
            case WorkshopDecorCategory.Table: suffix = " Masa"; break;
            case WorkshopDecorCategory.Chair: suffix = " Sandalye"; break;
            case WorkshopDecorCategory.Rug: suffix = " Halı"; break;
            case WorkshopDecorCategory.Shelf: suffix = " Raf"; break;
            case WorkshopDecorCategory.Counter: suffix = " Tezgâh"; break;
            case WorkshopDecorCategory.Plant: suffix = " Saksı Bitkisi"; break;
            case WorkshopDecorCategory.WallArt: suffix = " Duvar Tablosu"; break;
        }
        return !string.IsNullOrEmpty(suffix) && product.DisplayName.EndsWith(suffix, StringComparison.Ordinal)
            ? product.DisplayName.Substring(0, product.DisplayName.Length - suffix.Length)
            : product.DisplayName;
    }

    private static Color GetWorkshopRarityTextColor(WorkshopDecorRarity rarity)
    {
        if (rarity == WorkshopDecorRarity.Rare) return new Color(0.02f, 0.42f, 0.43f, 1f);
        if (rarity == WorkshopDecorRarity.Luxury) return new Color(0.48f, 0.10f, 0.50f, 1f);
        return new Color(0.43f, 0.22f, 0.07f, 1f);
    }

    private static Color GetWorkshopRarityCardColor(WorkshopDecorRarity rarity, bool selected)
    {
        Color color = rarity == WorkshopDecorRarity.Rare
            ? new Color(0.05f, 0.32f, 0.34f, 1f)
            : rarity == WorkshopDecorRarity.Luxury
                ? new Color(0.35f, 0.08f, 0.38f, 1f)
                : new Color(0.38f, 0.20f, 0.08f, 1f);
        return selected ? Color.Lerp(color, new Color(0.95f, 0.58f, 0.08f, 1f), 0.45f) : color;
    }

    private void CreateLegacyDecorPanel(Transform parent)
    {
        decorPanel = new GameObject("DecorPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        decorPanel.transform.SetParent(parent, false);

        RectTransform panelRect = decorPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = new Vector2(0, 0);
        panelRect.sizeDelta = new Vector2(1220, 700);

        Image panelImage = decorPanel.GetComponent<Image>();
        panelImage.color = new Color(0.08f, 0.07f, 0.06f, 1f);

        CreateWorkshopPanelBox(decorPanel.transform, "WorkshopTopBar", new Vector2(0, 302), new Vector2(1170, 62), new Color(0.13f, 0.10f, 0.08f, 1f));
        CreatePanelStrip(decorPanel.transform, "WorkshopTopGoldLine", new Vector2(0, 270), new Vector2(1172, 2), new Color(0.88f, 0.56f, 0.18f, 0.9f));

        TMP_Text title = CreateText(decorPanel.transform, "DecorPanelTitle", "ATOLYE", new Vector2(-505, 304), new Vector2(170, 48), 31);
        title.color = new Color(1f, 0.80f, 0.40f);
        title.fontStyle = FontStyles.Bold;

        decorTabTexts.Clear();
        decorTabTexts.Add(CreateDecorTabButton("DecorWallTab", "Duvar", new Vector2(-255, 304), PreviewPinkWall, true));
        decorTabTexts.Add(CreateDecorTabButton("DecorFloorTab", "Zemin", new Vector2(-155, 304), PreviewCreamFloor, false));
        decorTabTexts.Add(CreateDecorTabButton("DecorTableTab", "Masa", new Vector2(-55, 304), PreviewRoseTable, false));
        decorTabTexts.Add(CreateDecorTabButton("DecorShelfTab", "Raf", new Vector2(45, 304), PreviewPinkWall, false));
        decorTabTexts.Add(CreateDecorTabButton("DecorCounterTab", "Tezgah", new Vector2(145, 304), PreviewPinkWall, false));

        CreateDecorResourcePill("DecorPaintStoragePill", new Vector2(422, 304));
        CreateDecorResourcePill("DecorWoodStoragePill", new Vector2(542, 304));

        CreateWorkshopPanelBox(decorPanel.transform, "WorkshopPreviewPanel", new Vector2(-180, -34), new Vector2(748, 478), new Color(0.11f, 0.09f, 0.08f, 1f));
        CreateWorkshopPanelBox(decorPanel.transform, "WorkshopCraftPanel", new Vector2(395, -34), new Vector2(342, 478), new Color(0.11f, 0.09f, 0.08f, 1f));

        TMP_Text previewTitle = CreateText(decorPanel.transform, "WorkshopPreviewTitle", "Dukkan Onizlemesi", new Vector2(-370, 178), new Vector2(300, 34), 21);
        previewTitle.color = new Color(1f, 0.77f, 0.34f);
        previewTitle.alignment = TextAlignmentOptions.Left;
        previewTitle.fontStyle = FontStyles.Bold;

        CreateWorkshopPanelBox(decorPanel.transform, "WorkshopSceneFrame", new Vector2(-180, 0), new Vector2(682, 304), new Color(0.15f, 0.12f, 0.10f, 1f));
        CreateDecorPreviewScene(decorPanel.transform, new Vector2(-180, 0));

        CreateWorkshopPanelBox(decorPanel.transform, "WorkshopSelectedPlate", new Vector2(-290, -230), new Vector2(420, 76), new Color(0.16f, 0.12f, 0.10f, 1f));
        decorPanelText = CreateText(decorPanel.transform, "DecorPanelText", "", new Vector2(-290, -230), new Vector2(370, 58), 16);
        decorPanelText.color = new Color(0.96f, 0.91f, 0.82f);
        decorPanelText.alignment = TextAlignmentOptions.Left;
        decorPanelText.enableWordWrapping = true;

        TMP_Text beforeLabel = CreateText(decorPanel.transform, "DecorBeforeLabel", "ONCE", new Vector2(0, -206), new Vector2(76, 22), 12);
        beforeLabel.color = new Color(0.70f, 0.62f, 0.50f);
        decorPreviewBeforeSwatchImage = CreateWorkshopPanelBox(decorPanel.transform, "DecorBeforeSwatch", new Vector2(0, -232), new Vector2(60, 38), new Color(0.70f, 0.82f, 0.78f, 1f));
        TMP_Text afterLabel = CreateText(decorPanel.transform, "DecorAfterLabel", "SONRA", new Vector2(92, -206), new Vector2(76, 22), 12);
        afterLabel.color = new Color(1f, 0.77f, 0.34f);
        decorPreviewAfterSwatchImage = CreateWorkshopPanelBox(decorPanel.transform, "DecorAfterSwatch", new Vector2(92, -232), new Vector2(60, 38), new Color(1f, 0.72f, 0.84f, 1f));

        TMP_Text craftTitle = CreateText(decorPanel.transform, "WorkshopCraftTitle", "Uretim Masasi", new Vector2(340, 178), new Vector2(230, 34), 21);
        craftTitle.color = new Color(1f, 0.77f, 0.34f);
        craftTitle.alignment = TextAlignmentOptions.Left;
        craftTitle.fontStyle = FontStyles.Bold;

        CreateWorkshopPanelBox(decorPanel.transform, "DecorDetailIconFrame", new Vector2(315, 54), new Vector2(176, 176), new Color(0.09f, 0.075f, 0.065f, 1f));
        decorDetailIconImage = CreateWorkshopResourceIcon(decorPanel.transform, "DecorDetailIcon", new Vector2(315, 54), null);
        RectTransform detailIconRect = decorDetailIconImage.GetComponent<RectTransform>();
        detailIconRect.sizeDelta = new Vector2(152, 152);

        decorDetailTitleText = CreateText(decorPanel.transform, "DecorDetailTitle", "", new Vector2(490, 100), new Vector2(150, 32), 20);
        decorDetailTitleText.color = new Color(1f, 0.93f, 0.82f);
        decorDetailTitleText.alignment = TextAlignmentOptions.Left;
        decorDetailTitleText.fontStyle = FontStyles.Bold;
        decorDetailTypeText = CreateText(decorPanel.transform, "DecorDetailType", "", new Vector2(490, 68), new Vector2(150, 26), 15);
        decorDetailTypeText.color = new Color(0.78f, 0.70f, 0.58f);
        decorDetailTypeText.alignment = TextAlignmentOptions.Left;
        decorDetailStatusText = CreateText(decorPanel.transform, "DecorDetailStatus", "", new Vector2(490, 38), new Vector2(150, 26), 17);
        decorDetailStatusText.color = new Color(1f, 0.42f, 0.30f);
        decorDetailStatusText.alignment = TextAlignmentOptions.Left;
        decorDetailStatusText.fontStyle = FontStyles.Bold;

        TMP_Text resourceTitle = CreateText(decorPanel.transform, "DecorResourcesTitle", "Kaynaklar", new Vector2(335, -78), new Vector2(220, 32), 20);
        resourceTitle.color = new Color(1f, 0.77f, 0.34f);
        resourceTitle.alignment = TextAlignmentOptions.Left;
        resourceTitle.fontStyle = FontStyles.Bold;

        CreateWorkshopPanelBox(decorPanel.transform, "DecorCostPlate", new Vector2(410, -128), new Vector2(278, 64), new Color(0.17f, 0.13f, 0.10f, 1f));
        decorCostIconImage = CreateWorkshopResourceIcon(decorPanel.transform, "DecorCostIcon", new Vector2(300, -128), GetCraftPaintIconSprite());
        RectTransform costIconRect = decorCostIconImage.GetComponent<RectTransform>();
        if (costIconRect != null)
        {
            costIconRect.sizeDelta = new Vector2(38, 38);
        }
        decorCostText = CreateText(decorPanel.transform, "DecorCostText", "", new Vector2(430, -128), new Vector2(190, 38), 16);
        decorCostText.color = new Color(0.96f, 0.91f, 0.82f);
        decorCostText.alignment = TextAlignmentOptions.Left;
        decorCostText.fontStyle = FontStyles.Bold;

        decorStoragePaintIconImage = CreateWorkshopResourceIcon(decorPanel.transform, "DecorStoragePaintIcon", new Vector2(392, 304), GetCraftPaintIconSprite());
        RectTransform storagePaintIconRect = decorStoragePaintIconImage.GetComponent<RectTransform>();
        if (storagePaintIconRect != null)
        {
            storagePaintIconRect.sizeDelta = new Vector2(32, 32);
        }
        decorStoragePaintText = CreateText(decorPanel.transform, "DecorStoragePaintText", "", new Vector2(450, 304), new Vector2(70, 30), 16);
        decorStoragePaintText.color = new Color(0.96f, 0.91f, 0.82f);
        decorStoragePaintText.alignment = TextAlignmentOptions.Left;
        decorStoragePaintText.fontStyle = FontStyles.Bold;

        decorStorageWoodIconImage = CreateWorkshopResourceIcon(decorPanel.transform, "DecorStorageWoodIcon", new Vector2(512, 304), GetWoodIconSprite());
        RectTransform storageWoodIconRect = decorStorageWoodIconImage.GetComponent<RectTransform>();
        if (storageWoodIconRect != null)
        {
            storageWoodIconRect.sizeDelta = new Vector2(32, 32);
        }
        decorStorageWoodText = CreateText(decorPanel.transform, "DecorStorageWoodText", "", new Vector2(570, 304), new Vector2(70, 30), 16);
        decorStorageWoodText.color = new Color(0.96f, 0.91f, 0.82f);
        decorStorageWoodText.alignment = TextAlignmentOptions.Left;
        decorStorageWoodText.fontStyle = FontStyles.Bold;

        decorActionButtonText = CreateButton(decorPanel.transform, "DecorActionButton", "Uret", new Vector2(482, -230), new Vector2(128, 54), ConfirmDecorPreview, new Color(0.72f, 0.45f, 0.12f), 18);
        CreateButton(decorPanel.transform, "DecorCloseButton", "Kapat", new Vector2(340, -230), new Vector2(128, 54), CloseDecorPanel, new Color(0.18f, 0.16f, 0.14f), 18);

        CreateWorkshopPanelBox(decorPanel.transform, "DecorBottomRail", new Vector2(0, -306), new Vector2(1128, 58), new Color(0.10f, 0.08f, 0.07f, 1f));
        TMP_Text setTitle = CreateText(decorPanel.transform, "DecorSetTitle", "Duvar Seti", new Vector2(-500, -306), new Vector2(150, 36), 20);
        setTitle.color = new Color(1f, 0.77f, 0.34f);
        setTitle.alignment = TextAlignmentOptions.Left;
        setTitle.fontStyle = FontStyles.Bold;
        pinkWallButtonText = CreateButton(decorPanel.transform, "PinkWallButton", "", new Vector2(-360, -306), new Vector2(58, 46), PreviewPinkWall, new Color(0.48f, 0.30f, 0.12f), 1);
        mintWallButtonText = CreateButton(decorPanel.transform, "MintWallButton", "", new Vector2(-276, -306), new Vector2(58, 46), PreviewMintWall, new Color(0.17f, 0.14f, 0.12f), 1);
        creamFloorButtonText = CreateButton(decorPanel.transform, "CreamFloorButton", "", new Vector2(-192, -306), new Vector2(58, 46), PreviewCreamFloor, new Color(0.17f, 0.14f, 0.12f), 1);
        roseTableButtonText = CreateButton(decorPanel.transform, "RoseTableButton", "", new Vector2(-108, -306), new Vector2(58, 46), PreviewRoseTable, new Color(0.17f, 0.14f, 0.12f), 1);
        CreateDecorButtonIcon(pinkWallButtonText, GetDecorItemData(1));
        CreateDecorButtonIcon(mintWallButtonText, GetDecorItemData(2));
        CreateDecorButtonIcon(creamFloorButtonText, GetDecorItemData(3));
        CreateDecorButtonIcon(roseTableButtonText, GetDecorItemData(4));
        TMP_Text allSetText = CreateText(decorPanel.transform, "DecorAllSetText", "Tum seti gor", new Vector2(480, -306), new Vector2(150, 36), 16);
        allSetText.color = new Color(1f, 0.77f, 0.34f);
        allSetText.alignment = TextAlignmentOptions.Right;

        decorPanel.transform.SetAsLastSibling();
        decorPanel.SetActive(false);
    }

    public void OpenTestPanel()
    {
        if (testPanel == null)
        {
            return;
        }

        testPanel.SetActive(true);
        testPanel.transform.SetAsLastSibling();
    }

    public void CloseTestPanel()
    {
        if (testPanel != null)
        {
            testPanel.SetActive(false);
        }
    }

    private void CreateTestPanel(Transform parent)
    {
        testPanel = new GameObject("TestPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        testPanel.transform.SetParent(parent, false);

        RectTransform panelRect = testPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = new Vector2(0, 20);
        panelRect.sizeDelta = new Vector2(620, 540);

        Image panelImage = testPanel.GetComponent<Image>();
        panelImage.color = new Color(0.92f, 0.92f, 0.92f, 0.97f);

        GameObject dragHandleObject = new GameObject("TestPanelDragHandle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(FloatingWindowDrag));
        dragHandleObject.transform.SetParent(testPanel.transform, false);

        RectTransform dragHandleRect = dragHandleObject.GetComponent<RectTransform>();
        dragHandleRect.anchorMin = new Vector2(0.5f, 0.5f);
        dragHandleRect.anchorMax = new Vector2(0.5f, 0.5f);
        dragHandleRect.anchoredPosition = new Vector2(0, 215);
        dragHandleRect.sizeDelta = new Vector2(560, 64);

        Image dragHandleImage = dragHandleObject.GetComponent<Image>();
        dragHandleImage.color = new Color(0.78f, 0.78f, 0.78f, 0.95f);

        FloatingWindowDrag dragHandle = dragHandleObject.GetComponent<FloatingWindowDrag>();
        dragHandle.target = panelRect;

        TMP_Text title = CreateText(dragHandleObject.transform, "TestPanelTitle", "Test Paneli - Tut ve Tasi", Vector2.zero, dragHandleRect.sizeDelta, 30);
        title.color = Color.black;

        GameObject scrollAreaObject = new GameObject("TestScrollArea", typeof(RectTransform), typeof(ScrollRect));
        scrollAreaObject.transform.SetParent(testPanel.transform, false);

        RectTransform scrollAreaRect = scrollAreaObject.GetComponent<RectTransform>();
        scrollAreaRect.anchorMin = new Vector2(0.5f, 0.5f);
        scrollAreaRect.anchorMax = new Vector2(0.5f, 0.5f);
        scrollAreaRect.anchoredPosition = new Vector2(-15, 0);
        scrollAreaRect.sizeDelta = new Vector2(500, 300);

        GameObject viewportObject = new GameObject("TestScrollViewport", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Mask));
        viewportObject.transform.SetParent(scrollAreaObject.transform, false);

        RectTransform viewportRect = viewportObject.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;

        Image viewportImage = viewportObject.GetComponent<Image>();
        viewportImage.color = new Color(1f, 1f, 1f, 0.05f);

        Mask viewportMask = viewportObject.GetComponent<Mask>();
        viewportMask.showMaskGraphic = false;

        GameObject contentObject = new GameObject("TestScrollContent", typeof(RectTransform));
        contentObject.transform.SetParent(viewportObject.transform, false);

        RectTransform contentRect = contentObject.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.5f, 1f);
        contentRect.anchorMax = new Vector2(0.5f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(480, 820);

        CreateButton(contentObject.transform, "TestHugeCoinsButton", "TEST +10.000.000 Coin", new Vector2(0, 300), new Vector2(440, 70), AddHugeTestCoins, new Color(0.92f, 0.30f, 0.12f), 24);
        CreateButton(contentObject.transform, "TestCoinsButton", "TEST +10000 Coin", new Vector2(0, 215), new Vector2(440, 70), AddTestCoins, new Color(0.80f, 0.18f, 0.18f), 25);
        CreateButton(contentObject.transform, "TestGemsButton", "TEST +1000 Elmas", new Vector2(0, 130), new Vector2(440, 70), AddTestGems, new Color(0.10f, 0.58f, 0.78f), 25);
        CreateButton(contentObject.transform, "TestXpButton", "TEST +100 XP", new Vector2(0, 45), new Vector2(440, 70), AddTestXp, new Color(0.18f, 0.42f, 0.72f), 25);
        CreateButton(contentObject.transform, "TestChestButton", "TEST +1 Siradan Sandik", new Vector2(0, -40), new Vector2(440, 70), AddTestChest, new Color(0.64f, 0.36f, 0.16f), 25);
        CreateButton(contentObject.transform, "TestRareChestButton", "TEST +1 Nadir Sandik", new Vector2(0, -125), new Vector2(440, 70), AddTestRareChest, new Color(0.30f, 0.42f, 0.68f), 25);
        CreateButton(contentObject.transform, "TestLuxuryChestButton", "TEST +1 Ozel Sandik", new Vector2(0, -210), new Vector2(440, 70), AddTestLuxuryChest, new Color(0.72f, 0.54f, 0.22f), 25);
        CreateButton(contentObject.transform, "ResetButton", "RESET", new Vector2(0, -295), new Vector2(440, 70), ResetSave, new Color(0.20f, 0.20f, 0.20f), 25);
        CreateButton(contentObject.transform, "TestVipButton", "TEST +1 VIP", new Vector2(0, -380), new Vector2(440, 70), AddTestVipLevel, new Color(0.72f, 0.54f, 0.18f), 25);
        CreateButton(contentObject.transform, "TestWorkshopMaterialsButton", "TEST +20 Atölye Malzemesi", new Vector2(0, -465), new Vector2(440, 70), AddTestWorkshopMaterials, new Color(0.18f, 0.48f, 0.36f), 23);

        GameObject scrollbarObject = new GameObject("TestScrollbar", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Scrollbar));
        scrollbarObject.transform.SetParent(testPanel.transform, false);

        RectTransform scrollbarRect = scrollbarObject.GetComponent<RectTransform>();
        scrollbarRect.anchorMin = new Vector2(0.5f, 0.5f);
        scrollbarRect.anchorMax = new Vector2(0.5f, 0.5f);
        scrollbarRect.anchoredPosition = new Vector2(265, 10);
        scrollbarRect.sizeDelta = new Vector2(28, 330);

        Image scrollbarImage = scrollbarObject.GetComponent<Image>();
        scrollbarImage.color = new Color(0.70f, 0.70f, 0.70f, 0.9f);

        GameObject handleObject = new GameObject("Handle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        handleObject.transform.SetParent(scrollbarObject.transform, false);

        RectTransform handleRect = handleObject.GetComponent<RectTransform>();
        handleRect.anchorMin = Vector2.zero;
        handleRect.anchorMax = Vector2.one;
        handleRect.offsetMin = new Vector2(4, 4);
        handleRect.offsetMax = new Vector2(-4, -4);

        Image handleImage = handleObject.GetComponent<Image>();
        handleImage.color = new Color(0.20f, 0.20f, 0.20f, 0.95f);

        Scrollbar scrollbar = scrollbarObject.GetComponent<Scrollbar>();
        scrollbar.direction = Scrollbar.Direction.BottomToTop;
        scrollbar.targetGraphic = handleImage;
        scrollbar.handleRect = handleRect;

        ScrollRect scrollRect = scrollAreaObject.GetComponent<ScrollRect>();
        scrollRect.viewport = viewportRect;
        scrollRect.content = contentRect;
        scrollRect.verticalScrollbar = scrollbar;
        scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
        scrollRect.verticalScrollbarSpacing = 8f;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.inertia = true;
        scrollRect.scrollSensitivity = 55f;
        scrollRect.verticalNormalizedPosition = 1f;

        CreateButton(testPanel.transform, "TestPanelCloseButton", "Kapat", new Vector2(0, -220), new Vector2(300, 60), CloseTestPanel, new Color(0.30f, 0.30f, 0.30f), 24);
        dragHandleObject.transform.SetAsLastSibling();

        testPanel.transform.SetAsLastSibling();
        testPanel.SetActive(false);
    }

    public void OpenDailyTasksPanel()
    {
        if (dailyTasksPanel == null)
        {
            return;
        }

        dailyTasksPanel.SetActive(true);
        dailyTasksPanel.transform.SetAsLastSibling();
        UpdateUI();
    }

    public void CloseDailyTasksPanel()
    {
        if (dailyTasksPanel != null)
        {
            dailyTasksPanel.SetActive(false);
        }
    }

    public void ClaimDailyCoffeeTask()
    {
        if (dailyCoffeeRewardClaimed || dailyCoffeeSold < 20) return;

        gems += 1;
        dailyCoffeeRewardClaimed = true;
        UpdateUI();
        SaveGame();
    }

    public void ClaimDailyCustomerTask()
    {
        if (dailyCustomerRewardClaimed || dailyCustomerSales < 5) return;

        chests += 1;
        dailyCustomerRewardClaimed = true;
        UpdateUI();
        SaveGame();
    }

    public void ClaimDailyChestTask()
    {
        if (dailyChestRewardClaimed || dailyChestsOpened < 1) return;

            paintPieces += 1;
            dailyChestRewardClaimed = true;
        UpdateUI();
        SaveGame();
    }

    private void CreateDailyTasksPanel(Transform parent)
    {
        dailyTasksPanel = new GameObject("DailyTasksPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        dailyTasksPanel.transform.SetParent(parent, false);

        RectTransform panelRect = dailyTasksPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = new Vector2(0, 25);
        panelRect.sizeDelta = new Vector2(900, 620);

        StylePopupPanel(dailyTasksPanel, panelRect.sizeDelta, new Color(0.90f, 0.94f, 0.84f, 0.98f), new Color(0.24f, 0.44f, 0.30f, 1f));

        TMP_Text title = CreateText(dailyTasksPanel.transform, "DailyTasksTitle", "Günlük Görevler", new Vector2(0, 250), new Vector2(780, 70), 38);
        StylePopupTitle(title, 34);

        dailyTasksPanelText = CreateText(dailyTasksPanel.transform, "DailyTasksText", "", new Vector2(0, 140), new Vector2(780, 130), 27);
        StylePopupBodyText(dailyTasksPanelText, 19, 27, TextAlignmentOptions.Center);

        dailyTaskCoffeeButtonText = StylePopupButton(CreateButton(dailyTasksPanel.transform, "DailyCoffeeTaskButton", "Ödülü Al", new Vector2(0, 35), new Vector2(720, 70), ClaimDailyCoffeeTask, new Color(0.28f, 0.45f, 0.78f), 24), new Color(0.28f, 0.45f, 0.78f), 24);
        dailyTaskCustomerButtonText = StylePopupButton(CreateButton(dailyTasksPanel.transform, "DailyCustomerTaskButton", "Ödülü Al", new Vector2(0, -55), new Vector2(720, 70), ClaimDailyCustomerTask, new Color(0.18f, 0.58f, 0.32f), 24), new Color(0.18f, 0.58f, 0.32f), 24);
        dailyTaskChestButtonText = StylePopupButton(CreateButton(dailyTasksPanel.transform, "DailyChestTaskButton", "Ödülü Al", new Vector2(0, -145), new Vector2(720, 70), ClaimDailyChestTask, new Color(0.64f, 0.36f, 0.16f), 24), new Color(0.64f, 0.36f, 0.16f), 24);

        StylePopupButton(CreateButton(dailyTasksPanel.transform, "DailyTasksCloseButton", "Kapat", new Vector2(0, -255), new Vector2(300, 70), CloseDailyTasksPanel, new Color(0.20f, 0.20f, 0.20f), 27), new Color(0.20f, 0.20f, 0.20f), 27);

        dailyTasksPanel.transform.SetAsLastSibling();
        dailyTasksPanel.SetActive(false);
    }




    private void CreateTaskPanel(Transform parent)
    {
        taskPanel = new GameObject("TaskPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        taskPanel.transform.SetParent(parent, false);

        RectTransform panelRect = taskPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = new Vector2(0, 40);
        panelRect.sizeDelta = new Vector2(880, 460);

        StylePopupPanel(taskPanel, panelRect.sizeDelta, new Color(0.94f, 0.88f, 1f, 0.98f), new Color(0.38f, 0.25f, 0.58f, 1f));

        taskPanelText = CreateText(taskPanel.transform, "TaskPanelText", "", new Vector2(0, 72), new Vector2(780, 230), 31);
        StylePopupBodyText(taskPanelText, 20, 31, TextAlignmentOptions.Center);

        taskRewardButtonText = StylePopupButton(CreateButton(taskPanel.transform, "TaskRewardButton", "Ödülü Al", new Vector2(0, -112), new Vector2(420, 86), ClaimTaskReward, new Color(0.36f, 0.24f, 0.62f), 31), new Color(0.36f, 0.24f, 0.62f), 31);
        StylePopupButton(CreateButton(taskPanel.transform, "TaskCloseButton", "Kapat", new Vector2(0, -205), new Vector2(300, 68), CloseTaskPanel, new Color(0.20f, 0.20f, 0.20f), 26), new Color(0.20f, 0.20f, 0.20f), 26);

        taskPanel.transform.SetAsLastSibling();
        taskPanel.SetActive(false);
    }

    private void CreateCoffeePanel(Transform parent)
    {
        coffeePanel = new GameObject("CoffeePanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        coffeePanel.transform.SetParent(parent, false);

        RectTransform panelRect = coffeePanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = new Vector2(0, 15);
        panelRect.sizeDelta = new Vector2(900, 660);

        StylePopupPanel(coffeePanel, panelRect.sizeDelta, new Color(1f, 0.92f, 0.74f, 0.98f), new Color(0.70f, 0.42f, 0.14f, 1f));

        TMP_Text title = CreateText(coffeePanel.transform, "CoffeePanelTitle", "Kahve Cesitleri", new Vector2(0, 270), new Vector2(780, 70), 38);
        StylePopupTitle(title, 34);

        basicCoffeeButtonText = StylePopupButton(CreateButton(coffeePanel.transform, "BasicCoffeeButton", "Daha Iyi Kahve", new Vector2(0, 150), new Vector2(760, 92), BuyCoffeeUpgrade, new Color(0.86f, 0.56f, 0.20f), 25), new Color(0.86f, 0.56f, 0.20f), 25);
        premiumCoffeeButtonText = StylePopupButton(CreateButton(coffeePanel.transform, "PremiumCoffeeButton", "Premium Cekirdek", new Vector2(0, 35), new Vector2(760, 92), BuyPremiumCoffee, new Color(0.62f, 0.36f, 0.18f), 25), new Color(0.62f, 0.36f, 0.18f), 25);
        signatureCoffeeButtonText = StylePopupButton(CreateButton(coffeePanel.transform, "SignatureCoffeeButton", "Ozel Harman", new Vector2(0, -80), new Vector2(760, 92), BuySignatureCoffee, new Color(0.48f, 0.22f, 0.42f), 25), new Color(0.48f, 0.22f, 0.42f), 25);

        StylePopupButton(CreateButton(coffeePanel.transform, "CoffeeCloseButton", "Kapat", new Vector2(0, -245), new Vector2(300, 70), CloseCoffeePanel, new Color(0.20f, 0.20f, 0.20f), 27), new Color(0.20f, 0.20f, 0.20f), 27);

        coffeePanel.transform.SetAsLastSibling();
        coffeePanel.SetActive(false);
    }

    private void CreateMachinePanel(Transform parent)
    {
        machinePanel = new GameObject("MachinePanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        machinePanel.transform.SetParent(parent, false);

        RectTransform panelRect = machinePanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = new Vector2(0, 15);
        panelRect.sizeDelta = new Vector2(900, 660);

        StylePopupPanel(machinePanel, panelRect.sizeDelta, new Color(0.86f, 0.95f, 0.90f, 0.98f), new Color(0.20f, 0.50f, 0.44f, 1f));

        TMP_Text title = CreateText(machinePanel.transform, "MachinePanelTitle", "Kahve Makineleri", new Vector2(0, 270), new Vector2(780, 70), 38);
        StylePopupTitle(title, 34);

        basicMachineButtonText = StylePopupButton(CreateButton(machinePanel.transform, "BasicMachineButton", "Basit Kahve Makinesi", new Vector2(0, 150), new Vector2(760, 92), BuyMachineUpgrade, new Color(0.20f, 0.55f, 0.48f), 25), new Color(0.20f, 0.55f, 0.48f), 25);
        espressoMachineButtonText = StylePopupButton(CreateButton(machinePanel.transform, "EspressoMachineButton", "Espresso Makinesi", new Vector2(0, 35), new Vector2(760, 92), BuyEspressoMachine, new Color(0.30f, 0.42f, 0.68f), 25), new Color(0.30f, 0.42f, 0.68f), 25);
        premiumMachineButtonText = StylePopupButton(CreateButton(machinePanel.transform, "PremiumMachineButton", "Premium Makine", new Vector2(0, -80), new Vector2(760, 92), BuyPremiumMachine, new Color(0.62f, 0.42f, 0.18f), 25), new Color(0.62f, 0.42f, 0.18f), 25);

        StylePopupButton(CreateButton(machinePanel.transform, "MachineCloseButton", "Kapat", new Vector2(0, -245), new Vector2(300, 70), CloseMachinePanel, new Color(0.20f, 0.20f, 0.20f), 27), new Color(0.20f, 0.20f, 0.20f), 27);

        machinePanel.transform.SetAsLastSibling();
        machinePanel.SetActive(false);
    }

    private void CreateBaristaPanel(Transform parent)
    {
        baristaPanel = new GameObject("BaristaPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        baristaPanel.transform.SetParent(parent, false);

        RectTransform panelRect = baristaPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = new Vector2(0, 15);
        panelRect.sizeDelta = new Vector2(900, 660);

        StylePopupPanel(baristaPanel, panelRect.sizeDelta, new Color(0.94f, 0.88f, 1f, 0.98f), new Color(0.46f, 0.28f, 0.62f, 1f));

        TMP_Text title = CreateText(baristaPanel.transform, "BaristaPanelTitle", "Baristalar", new Vector2(0, 270), new Vector2(780, 70), 38);
        StylePopupTitle(title, 34);

        baristaStatusText = CreateText(baristaPanel.transform, "BaristaStatusText", "", new Vector2(0, 218), new Vector2(760, 42), 21);
        StylePopupBodyText(baristaStatusText, 14, 20, TextAlignmentOptions.Center);

        juniorBaristaButtonText = StylePopupButton(CreateButton(baristaPanel.transform, "JuniorBaristaButton", "Barista", new Vector2(0, 150), new Vector2(760, 92), BuyBaristaUpgrade, new Color(0.54f, 0.32f, 0.72f), 25), new Color(0.54f, 0.32f, 0.72f), 25);
        seniorBaristaButtonText = StylePopupButton(CreateButton(baristaPanel.transform, "SeniorBaristaButton", "Usta Barista", new Vector2(0, 35), new Vector2(760, 92), BuySeniorBarista, new Color(0.34f, 0.42f, 0.78f), 25), new Color(0.34f, 0.42f, 0.78f), 25);
        masterBaristaButtonText = StylePopupButton(CreateButton(baristaPanel.transform, "MasterBaristaButton", "Sef Barista", new Vector2(0, -80), new Vector2(760, 92), BuyMasterBarista, new Color(0.62f, 0.30f, 0.50f), 25), new Color(0.62f, 0.30f, 0.50f), 25);

        StylePopupButton(CreateButton(baristaPanel.transform, "BaristaCloseButton", "Kapat", new Vector2(0, -245), new Vector2(300, 70), CloseBaristaPanel, new Color(0.20f, 0.20f, 0.20f), 27), new Color(0.20f, 0.20f, 0.20f), 27);

        baristaPanel.transform.SetAsLastSibling();
        baristaPanel.SetActive(false);
    }

    private void CreatePassiveIncomePanel(Transform parent)
    {
        passiveIncomePanel = new GameObject("PassiveIncomePanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        passiveIncomePanel.transform.SetParent(parent, false);

        RectTransform panelRect = passiveIncomePanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = new Vector2(0, 35);
        panelRect.sizeDelta = new Vector2(760, 520);

        StylePopupPanel(passiveIncomePanel, panelRect.sizeDelta, new Color(0.90f, 0.95f, 1f, 0.98f), new Color(0.24f, 0.38f, 0.66f, 1f));

        TMP_Text title = CreateText(passiveIncomePanel.transform, "PassiveIncomeTitle", "Pasif Gelir Detayi", new Vector2(0, 205), new Vector2(700, 60), 34);
        StylePopupTitle(title, 30);

        passiveIncomePanelText = CreateText(passiveIncomePanel.transform, "PassiveIncomeText", "", new Vector2(0, 20), new Vector2(680, 300), 27);
        StylePopupBodyText(passiveIncomePanelText, 17, 25, TextAlignmentOptions.TopLeft);

        StylePopupButton(CreateButton(passiveIncomePanel.transform, "PassiveIncomeCloseButton", "Kapat", new Vector2(0, -220), new Vector2(280, 70), ClosePassiveIncomePanel, new Color(0.20f, 0.20f, 0.20f), 27), new Color(0.20f, 0.20f, 0.20f), 27);

        passiveIncomePanel.transform.SetAsLastSibling();
        passiveIncomePanel.SetActive(false);
    }

    private void CreateFoodUpgradePanel(Transform parent)
    {
        foodUpgradeCardUis.Clear();
        foodCategoryTabTexts.Clear();

        foodPanel = new GameObject("FoodUpgradePanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        foodPanel.transform.SetParent(parent, false);

        RectTransform panelRect = foodPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = new Vector2(0f, 14f);
        panelRect.sizeDelta = new Vector2(1674, 943);

        Image panelImage = foodPanel.GetComponent<Image>();
        panelImage.sprite = GetFoodPanelBackgroundSprite();
        panelImage.color = Color.white;
        panelImage.preserveAspect = true;

        TMP_Text title = CreateText(foodPanel.transform, "FoodPanelTitle", "Gıda Yükseltmeleri", new Vector2(3, 421), new Vector2(650, 72), 48);
        StyleFoodTitle(title, 48);

        TMP_Text closeText = CreateButton(foodPanel.transform, "FoodPanelCloseButton", "", new Vector2(699.2f, 402.3f), new Vector2(76, 76), CloseFoodUpgradePanel, Color.white, 1);
        Image closeImage = closeText.GetComponentInParent<Image>();
        if (closeImage != null)
        {
            closeImage.sprite = GetFoodCloseButtonSprite();
            closeImage.color = Color.white;
            closeImage.preserveAspect = true;
        }
        closeText.gameObject.SetActive(false);

        CreateFoodCategoryTab("FoodTabCoffee", "Kahve", FoodUpgradeCategory.Coffee, new Vector2(-665, 272), true);
        CreateFoodCategoryTab("FoodTabSweet", "Tatlı", FoodUpgradeCategory.Sweet, new Vector2(-665, 93), false);
        CreateFoodCategoryTab("FoodTabSnack", "Atıştırmalık", FoodUpgradeCategory.Snack, new Vector2(-665, -87.2f), false);
        CreateFoodCategoryTab("FoodTabCold", "Soğuk", FoodUpgradeCategory.Cold, new Vector2(-665, -268), false);

        CreateFoodCategoryInfo();

        GameObject viewportObject = new GameObject("FoodCardsViewport", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(RectMask2D));
        viewportObject.transform.SetParent(foodPanel.transform, false);
        RectTransform viewportRect = viewportObject.GetComponent<RectTransform>();
        viewportRect.anchorMin = new Vector2(0.5f, 0.5f);
        viewportRect.anchorMax = new Vector2(0.5f, 0.5f);
        viewportRect.anchoredPosition = new Vector2(90f, -30f);
        viewportRect.sizeDelta = new Vector2(1320f, 580f);
        viewportObject.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.001f);

        GameObject contentObject = new GameObject("FoodCardsContent", typeof(RectTransform));
        contentObject.transform.SetParent(viewportObject.transform, false);
        foodCardsContent = contentObject.GetComponent<RectTransform>();
        foodCardsContent.anchorMin = new Vector2(0.5f, 1f);
        foodCardsContent.anchorMax = new Vector2(0.5f, 1f);
        foodCardsContent.pivot = new Vector2(0.5f, 1f);
        foodCardsContent.anchoredPosition = Vector2.zero;
        foodCardsContent.sizeDelta = new Vector2(1280f, 650f);

        GridLayoutGroup cardsGrid = contentObject.AddComponent<GridLayoutGroup>();
        cardsGrid.padding = new RectOffset(75, 75, 20, 20);
        cardsGrid.cellSize = new Vector2(350f, 455f);
        cardsGrid.spacing = new Vector2(40f, 20f);
        cardsGrid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        cardsGrid.startAxis = GridLayoutGroup.Axis.Horizontal;
        cardsGrid.childAlignment = TextAnchor.UpperLeft;
        cardsGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        cardsGrid.constraintCount = 3;

        ContentSizeFitter contentFitter = contentObject.AddComponent<ContentSizeFitter>();
        contentFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        foodCardsScrollRect = viewportObject.AddComponent<ScrollRect>();
        foodCardsScrollRect.viewport = viewportRect;
        foodCardsScrollRect.content = foodCardsContent;
        foodCardsScrollRect.horizontal = false;
        foodCardsScrollRect.vertical = true;
        foodCardsScrollRect.movementType = ScrollRect.MovementType.Clamped;
        foodCardsScrollRect.inertia = true;
        foodCardsScrollRect.decelerationRate = 0.12f;
        foodCardsScrollRect.scrollSensitivity = 38f;

        for (int i = 0; i < foodUpgradeDatabase.Count; i++)
        {
            CreateFoodUpgradeCard(foodCardsContent, foodUpgradeDatabase[i], i, Vector2.zero);
        }

        RefreshFoodCategoryLayout();

        foodPanel.transform.SetAsLastSibling();
        foodPanel.SetActive(false);
    }

    private void CreateFoodCategoryInfo()
    {
        Image watermark = CreateFoodUiImage(
            foodPanel.transform,
            "FoodParchmentWatermark",
            GetFoodParchmentWatermarkSprite(),
            new Vector2(500f, -225f),
            new Vector2(390f, 92f),
            true);
        watermark.color = new Color(0.82f, 0.61f, 0.24f, 0.11f);

        Image ribbon = CreateFoodUiImage(
            foodPanel.transform,
            "FoodCategoryInfoRibbon",
            GetFoodCategoryInfoRibbonSprite(),
            new Vector2(90f, 330f),
            new Vector2(1260f, 88f),
            false);
        ribbon.type = Image.Type.Sliced;

        Image progressPill = CreateFoodCategoryInfoPill(ribbon.transform, "ProgressPill", new Vector2(-260f, 0f), new Vector2(200f, 40f));
        CreateFoodUiImage(progressPill.transform, "ProgressIcon", GetFoodCoinChipSprite(), new Vector2(-65f, -3f), new Vector2(34f, 34f), true);
        foodCategoryProgressText = CreateText(progressPill.transform, "CategoryProgress", "", new Vector2(18f, 0f), new Vector2(200f, 40f), 23);

        Image salesPill = CreateFoodCategoryInfoPill(ribbon.transform, "SalesPill", new Vector2(0f, 0f), new Vector2(200f, 40f));
        CreateFoodUiImage(salesPill.transform, "SalesIcon", GetFoodCoinChipSprite(), new Vector2(-69f, -1f), new Vector2(34f, 34f), true);
        foodCategorySalesText = CreateText(salesPill.transform, "CategorySales", "", new Vector2(17f, 0f), new Vector2(180f, 44f), 23);

        Image passivePill = CreateFoodCategoryInfoPill(ribbon.transform, "PassivePill", new Vector2(270f, 0f), new Vector2(200f, 40f));
        CreateFoodUiImage(passivePill.transform, "PassiveIcon", GetFoodPassiveChipSprite(), new Vector2(-69.4f, -2.2f), new Vector2(34f, 34f), true);
        foodCategoryPassiveText = CreateText(passivePill.transform, "CategoryPassive", "", new Vector2(17f, 0f), new Vector2(180f, 44f), 23);

        StyleFoodCategoryInfoText(foodCategoryProgressText, 18f, 23f);
        StyleFoodCategoryInfoText(foodCategorySalesText, 18f, 23f);
        StyleFoodCategoryInfoText(foodCategoryPassiveText, 18f, 23f);
        UpdateFoodCategoryInfo();
    }

    private Image CreateFoodCategoryInfoPill(Transform parent, string objectName, Vector2 position, Vector2 size)
    {
        Image pill = CreateFoodUiImage(parent, objectName, null, position, size, false);
        pill.sprite = CreateRoundedRectSprite(
            size,
            new Color(0.20f, 0.075f, 0.018f, 0.78f),
            new Color(0.92f, 0.61f, 0.20f, 0.90f),
            18,
            2);
        pill.color = Color.white;
        return pill;
    }

    private void StyleFoodCategoryInfoText(TMP_Text text, float minSize, float maxSize)
    {
        text.color = new Color(1f, 0.88f, 0.62f, 1f);
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.enableAutoSizing = true;
        text.fontSizeMin = minSize;
        text.fontSizeMax = maxSize;
        text.enableWordWrapping = false;
        text.overflowMode = TextOverflowModes.Ellipsis;
        text.outlineColor = new Color(0.13f, 0.05f, 0.01f, 1f);
        text.outlineWidth = 0.14f;
        ApplyVipFont(text);
    }

    private void UpdateFoodCategoryInfo()
    {
        int total = 0;
        int purchased = 0;
        int salesBonus = 0;
        int passiveBonus = 0;

        for (int i = 0; i < foodUpgradeDatabase.Count; i++)
        {
            FoodUpgradeData data = foodUpgradeDatabase[i];
            if (data.Category != selectedFoodCategory) continue;

            total++;
            if (data.Level <= 0) continue;

            purchased++;
            salesBonus += data.SalesBonusPercent;
            passiveBonus += data.PassiveBonusPercent;
        }

        if (foodCategoryProgressText != null)
            foodCategoryProgressText.text = purchased + "/" + total + " Al\u0131nd\u0131";
        if (foodCategorySalesText != null)
            foodCategorySalesText.text = "Sat\u0131\u015F +%" + salesBonus;
        if (foodCategoryPassiveText != null)
            foodCategoryPassiveText.text = "Pasif +%" + passiveBonus;
    }

    private void CreateFoodCategoryTab(string objectName, string label, FoodUpgradeCategory category, Vector2 position, bool selected)
    {
        TMP_Text tabText = CreateButton(foodPanel.transform, objectName, label, position, new Vector2(220, 165), delegate { SelectFoodCategory(category); }, Color.clear, 26);
        foodCategoryTabTexts[category] = tabText;
        Image tabImage = tabText.GetComponentInParent<Image>();
        RectTransform tabRect = tabText.GetComponentInParent<RectTransform>();
        if (tabImage != null && tabRect != null)
        {
            tabImage.sprite = selected ? GetFoodCategoryTabSelectedSprite() : GetFoodCategoryTabNormalSprite();
            tabImage.color = Color.white;
            tabImage.preserveAspect = false;
            tabImage.type = Image.Type.Simple;
        }

        tabText.color = selected ? new Color(0.18f, 0.09f, 0.03f) : new Color(1f, 0.88f, 0.62f);
        tabText.fontStyle = FontStyles.Bold;
        tabText.enableAutoSizing = true;
        tabText.fontSizeMin = 16;
        tabText.fontSizeMax = 26;
        tabText.raycastTarget = false;
        ApplyVipFont(tabText);
    }

    private void SelectFoodCategory(FoodUpgradeCategory category)
    {
        selectedFoodCategory = category;
        RefreshFoodCategoryLayout();
    }

    private void RefreshFoodCategoryLayout()
    {
        for (int i = 0; i < foodUpgradeCardUis.Count; i++)
        {
            FoodUpgradeCardUi ui = foodUpgradeCardUis[i];
            bool visible = ui.Data.Category == selectedFoodCategory;
            ui.RootObject.SetActive(visible);
            if (!visible) continue;
        }

        if (foodCardsContent != null)
        {
            foodCardsContent.anchoredPosition = Vector2.zero;
            LayoutRebuilder.ForceRebuildLayoutImmediate(foodCardsContent);
        }
        if (foodCardsScrollRect != null)
        {
            foodCardsScrollRect.verticalNormalizedPosition = 1f;
            foodCardsScrollRect.velocity = Vector2.zero;
        }

        foreach (KeyValuePair<FoodUpgradeCategory, TMP_Text> pair in foodCategoryTabTexts)
        {
            bool selected = pair.Key == selectedFoodCategory;
            Image tabImage = pair.Value.GetComponentInParent<Image>();
            if (tabImage != null)
            {
                tabImage.sprite = selected ? GetFoodCategoryTabSelectedSprite() : GetFoodCategoryTabNormalSprite();
                tabImage.color = Color.white;
                tabImage.type = Image.Type.Simple;
            }

            pair.Value.color = selected ? new Color(0.20f, 0.10f, 0.03f, 1f) : new Color(1f, 0.90f, 0.68f, 1f);
            pair.Value.fontSizeMax = selected ? 28f : 24f;
            pair.Value.outlineColor = selected ? new Color(1f, 0.76f, 0.24f, 0.75f) : new Color(0.10f, 0.04f, 0.01f, 0.95f);
            pair.Value.outlineWidth = selected ? 0.08f : 0.15f;
        }

        UpdateFoodCategoryInfo();
    }

    private void CreateFoodUpgradeCard(Transform parent, FoodUpgradeData data, int index, Vector2 position)
    {
        GameObject cellObject = new GameObject("FoodCardCell_" + data.Id, typeof(RectTransform));
        cellObject.transform.SetParent(parent, false);

        GameObject cardObject = new GameObject("FoodCard_" + data.Id, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        cardObject.transform.SetParent(cellObject.transform, false);

        RectTransform cardRect = cardObject.GetComponent<RectTransform>();
        cardRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.anchoredPosition = Vector2.zero;
        cardRect.sizeDelta = new Vector2(205, 330);
        cardRect.localScale = Vector3.one * 1.35f;

        Image cardImage = cardObject.GetComponent<Image>();
        cardImage.sprite = GetFoodCardFrameSprite();
        cardImage.color = Color.white;
        cardImage.preserveAspect = false;

        FoodUpgradeCardUi ui = new FoodUpgradeCardUi();
        ui.Data = data;
        ui.RootObject = cellObject;
        ui.CardImage = cardImage;

        ui.NameText = CreateText(cardObject.transform, "NameText", data.DisplayName, new Vector2(0.59998f, 136.4f), new Vector2(172, 32), 20);
        StyleFoodCardText(ui.NameText, 14, 20, TextAlignmentOptions.Center);

        ui.IconImage = CreateFoodCardIcon(cardObject.transform, data, new Vector2(-0.000035286f, 53f), new Vector2(112.0013f, 94.2971f));

        if (data.Id == "coffee")
        {
            ui.BeanLeftImage = CreateFoodUiImage(cardObject.transform, "BeanDecorLeft", GetFoodPassiveChipSprite(), new Vector2(-24.89997f, -4.900003f), new Vector2(17, 17), true);
            ui.BeanLeftImage.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -28f);
            ui.BeanCenterImage = CreateFoodUiImage(cardObject.transform, "BeanDecorCenter", GetFoodPassiveChipSprite(), new Vector2(-2.9f, -8.9f), new Vector2(18, 18), true);
            ui.BeanCenterImage.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 12f);
            ui.BeanRightImage = CreateFoodUiImage(cardObject.transform, "BeanDecorRight", GetFoodPassiveChipSprite(), new Vector2(19.10003f, -3.900003f), new Vector2(16, 16), true);
            ui.BeanRightImage.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 34f);
        }

        Image levelRow = CreateFoodUiImage(cardObject.transform, "LevelRowBackground", null, new Vector2(18, -35), new Vector2(120, 30), false);
        levelRow.sprite = CreateRoundedRectSprite(levelRow.rectTransform.sizeDelta, new Color(1f, 0.91f, 0.70f, 0.42f), new Color(0.69f, 0.43f, 0.13f, 0.42f), 12, 1);
        levelRow.color = Color.white;
        ui.LevelRowImage = levelRow;

        ui.LevelBadgeImage = CreateFoodUiImage(cardObject.transform, "LevelBadge", GetFoodLevelBadgeSprite(), new Vector2(-52f, -26.4f), new Vector2(42, 42), true);

        ui.LevelText = CreateText(cardObject.transform, "LevelText", "", new Vector2(-52f, -26.024f), new Vector2(38, 30.543f), 11);
        ui.LevelText.color = Color.white;
        ui.LevelText.fontStyle = FontStyles.Bold;
        ui.LevelText.alignment = TextAlignmentOptions.Center;
        ui.LevelText.enableAutoSizing = true;
        ui.LevelText.fontSizeMin = 14;
        ui.LevelText.fontSizeMax = 22;
        ui.LevelText.outlineColor = new Color(0.14f, 0.07f, 0.03f, 0.9f);
        ui.LevelText.outlineWidth = 0.12f;
        ApplyVipFont(ui.LevelText);

        TMP_Text levelLabel = CreateText(cardObject.transform, "LevelLabel", "Gerekli Seviye", new Vector2(17.99998f, -30.04402f), new Vector2(85.427f, 22.5021f), 13);
        StyleFoodCardText(levelLabel, 9, 13, TextAlignmentOptions.Center);
        ui.LevelLabelText = levelLabel;

        ui.UnlockStripImage = CreateFoodUiImage(cardObject.transform, "UnlockStrip", null, new Vector2(14, -74.7f), new Vector2(132, 27), false);
        ui.UnlockStripImage.sprite = CreateRoundedRectSprite(ui.UnlockStripImage.rectTransform.sizeDelta, new Color(1f, 0.89f, 0.64f, 0.94f), new Color(0.63f, 0.36f, 0.09f, 0.95f), 11, 2);
        ui.UnlockStripImage.color = Color.white;
        ui.LockedBadgeImage = CreateFoodUiImage(cardObject.transform, "LockedBadge", GetFoodLevelBadgeSprite(), new Vector2(-56, -74.7f), new Vector2(34, 34), true);
        ui.LockedIconImage = CreateFoodUiImage(cardObject.transform, "LockedIcon", GetVipLockIconSprite(), new Vector2(-56, -74.7f), new Vector2(19, 19), true);
        ui.LockedIconImage.color = new Color(1f, 0.72f, 0.17f, 1f);
        ui.UnlockText = CreateText(cardObject.transform, "UnlockText", "", new Vector2(18, -74.7f), new Vector2(112, 23), 15);
        StyleFoodCardText(ui.UnlockText, 11, 15, TextAlignmentOptions.Center);

        Image orderStrip = CreateFoodUiImage(cardObject.transform, "OrderStrip", null, new Vector2(0.49998f, -69.911f), new Vector2(147, 18), false);
        orderStrip.sprite = CreateRoundedRectSprite(orderStrip.rectTransform.sizeDelta, new Color(1f, 0.91f, 0.72f, 0.34f), new Color(0.71f, 0.48f, 0.18f, 0.34f), 9, 1);
        orderStrip.color = Color.white;
        ui.OrderStripImage = orderStrip;
        Image passiveStrip = CreateFoodUiImage(cardObject.transform, "PassiveStrip", null, new Vector2(-0.00002718f, -95.2f), new Vector2(147, 18), false);
        passiveStrip.sprite = CreateRoundedRectSprite(passiveStrip.rectTransform.sizeDelta, new Color(1f, 0.91f, 0.72f, 0.34f), new Color(0.71f, 0.48f, 0.18f, 0.34f), 9, 1);
        passiveStrip.color = Color.white;
        ui.PassiveStripImage = passiveStrip;

        ui.OrderIconImage = CreateFoodUiImage(cardObject.transform, "OrderIcon", GetFoodCoinChipSprite(), new Vector2(-59.99997f, -72.481f), new Vector2(20, 20), true);
        ui.PassiveIconImage = CreateFoodUiImage(cardObject.transform, "PassiveIcon", GetFoodPassiveChipSprite(), new Vector2(-60f, -96.19999f), new Vector2(20, 20), true);

        ui.OrderText = CreateText(cardObject.transform, "OrderText", "", new Vector2(10.7209f, -69.911f), new Vector2(114.4419f, 19), 12);
        StyleFoodCardText(ui.OrderText, 9, 12, TextAlignmentOptions.Left);
        ui.PassiveText = CreateText(cardObject.transform, "PassiveText", "", new Vector2(9.564996f, -95.7f), new Vector2(112.13f, 19), 12);
        StyleFoodCardText(ui.PassiveText, 9, 12, TextAlignmentOptions.Left);

        ui.PriceText = CreateText(cardObject.transform, "PriceText", "", new Vector2(24.10001f, -127f), new Vector2(72, 28), 14);
        StyleFoodCardText(ui.PriceText, 9, 13, TextAlignmentOptions.Center);
        ui.PriceText.color = Color.white;

        Image priceCoin = CreateFoodUiImage(cardObject.transform, "PriceCoin", GetFoodCoinChipSprite(), new Vector2(50.7f, -127f), new Vector2(15, 15), true);
        ui.PriceCoinImage = priceCoin;

        ui.ButtonText = CreateButton(cardObject.transform, "UpgradeButton", "Satın Al", new Vector2(0, -127f), new Vector2(140, 30), delegate { BuyFoodUpgradeByIndex(index); }, Color.white, 14);
        ui.Button = ui.ButtonText.GetComponentInParent<Button>();
        StyleFoodButton(ui.ButtonText, new Color(0.33f, 0.63f, 0.12f), 17);
        ui.ButtonText.rectTransform.anchoredPosition = new Vector2(-29, 0);
        ui.ButtonText.rectTransform.sizeDelta = new Vector2(60, 25);
        Image upgradeImage = ui.Button != null ? ui.Button.GetComponent<Image>() : null;
        if (upgradeImage != null)
        {
            upgradeImage.sprite = GetFoodUpgradeButtonSprite();
            upgradeImage.color = Color.white;
            upgradeImage.preserveAspect = false;
            upgradeImage.type = Image.Type.Simple;
        }
        ui.PriceText.transform.SetAsLastSibling();
        priceCoin.transform.SetAsLastSibling();

        foodUpgradeCardUis.Add(ui);
    }

    private Image CreateFoodCardIcon(Transform parent, FoodUpgradeData data, Vector2 position, Vector2 size)
    {
        GameObject iconObject = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        iconObject.transform.SetParent(parent, false);
        RectTransform rect = iconObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image image = iconObject.GetComponent<Image>();
        image.sprite = GetFoodIconSprite(data);
        image.color = Color.white;
        image.preserveAspect = true;
        image.raycastTarget = false;
        return image;
    }

    private Image CreateFoodUiImage(Transform parent, string objectName, Sprite sprite, Vector2 position, Vector2 size, bool preserveAspect)
    {
        GameObject imageObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        imageObject.transform.SetParent(parent, false);

        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image image = imageObject.GetComponent<Image>();
        image.sprite = sprite;
        image.color = Color.white;
        image.preserveAspect = preserveAspect;
        image.raycastTarget = false;
        return image;
    }

    private void StyleFoodTitle(TMP_Text text, float maxSize)
    {
        text.color = new Color(1f, 0.89f, 0.58f);
        text.fontStyle = FontStyles.Bold;
        text.enableAutoSizing = true;
        text.fontSizeMin = 28;
        text.fontSizeMax = maxSize;
        text.outlineColor = new Color(0.18f, 0.08f, 0.02f, 0.95f);
        text.outlineWidth = 0.16f;
        ApplyVipFont(text);
    }

    private void StyleFoodCardText(TMP_Text text, float minSize, float maxSize, TextAlignmentOptions alignment)
    {
        text.color = new Color(0.18f, 0.10f, 0.04f);
        text.fontStyle = FontStyles.Bold;
        text.alignment = alignment;
        text.enableAutoSizing = true;
        text.fontSizeMin = minSize;
        text.fontSizeMax = maxSize;
        text.enableWordWrapping = false;
        text.overflowMode = TextOverflowModes.Ellipsis;
        ApplyVipFont(text);
    }

    private void StyleFoodButton(TMP_Text labelText, Color fillColor, float maxSize)
    {
        StylePopupButton(labelText, fillColor, maxSize);
        ApplyVipFont(labelText);
    }

    private void CreateVipPanel(Transform parent)
    {
        vipPanel = new GameObject("VipPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        vipPanel.transform.SetParent(parent, false);

        RectTransform panelRect = vipPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = new Vector2(0, 0);
        panelRect.sizeDelta = new Vector2(1320, 724);

        Image panelImage = vipPanel.GetComponent<Image>();
        panelImage.sprite = GetVipPanelBackgroundSprite();
        panelImage.color = Color.white;
        panelImage.preserveAspect = true;

        TMP_Text title = CreateText(vipPanel.transform, "VipPanelTitle", "", new Vector2(-415, 166), new Vector2(320, 44), 32);
        ApplyVipFont(title);
        title.color = new Color(0.40f, 0.20f, 0.05f, 1f);
        title.fontStyle = FontStyles.Bold;
        title.alignment = TextAlignmentOptions.Left;

        vipLevelSummaryText = CreateText(vipPanel.transform, "VipLevelSummary", "", new Vector2(392, 214), new Vector2(190, 32), 18);
        ApplyVipFont(vipLevelSummaryText);
        vipLevelSummaryText.color = new Color(1f, 0.88f, 0.47f, 1f);
        vipLevelSummaryText.fontStyle = FontStyles.Bold;
        vipLevelSummaryText.alignment = TextAlignmentOptions.Right;
        vipLevelSummaryText.gameObject.SetActive(false);
        vipPanelText = CreateText(vipPanel.transform, "VipPanelText", "", new Vector2(-325.5474f, 80.0787f), new Vector2(174.3353f, 103.7557f), 17);
        ApplyVipFont(vipPanelText);
        vipPanelText.color = new Color(0.32f, 0.17f, 0.06f, 1f);
        vipPanelText.alignment = TextAlignmentOptions.Center;
        vipPanelText.enableWordWrapping = true;
        vipPanelText.richText = true;
        vipPanelText.fontStyle = FontStyles.Bold;
        vipPanelText.enableAutoSizing = true;
        vipPanelText.fontSizeMin = 12f;
        vipPanelText.fontSizeMax = 18f;
        vipPanelText.gameObject.SetActive(false);
        // The professional information card uses live text instead of tier-specific baked labels.

        CreateVipPanelClearSelectionArea(vipPanel.transform);
        CreateVipProfessionalInfoPanel(vipPanel.transform);

        vipLevelCardTexts.Clear();
        vipLevelCardTitleTexts.Clear();
        vipLevelCardBonusTexts.Clear();
        vipLevelCardImages.Clear();
        vipLevelLockImages.Clear();
        vipLevelSelectImages.Clear();
        vipLevelBottomStarImages.Clear();
        vipLevelBottomStarRects.Clear();
        vipLevelButtonRects.Clear();
        for (int i = 1; i <= MaxVipLevel; i++)
        {
            CreateVipCardButton(vipPanel.transform, i, new Vector2(-92f + ((i - 1) * 151f), 18f));
        }

        vipCoinBonusText = CreateVipBonusPill(vipPanel.transform, "Coin", "+0%", VipCoinBonusLayout);
        vipXpBonusText = CreateVipBonusPill(vipPanel.transform, "XP", "+0%", VipXpBonusLayout);
        vipCoffeeBonusText = CreateVipBonusPill(vipPanel.transform, "Müşteri", "+0%", VipCustomerBonusLayout);
        vipPassiveBonusText = CreateVipBonusPill(vipPanel.transform, "Pasif Gelir", "+0%", VipPassiveBonusLayout);
        vipDecorBonusText = CreateVipBonusPill(vipPanel.transform, "İndirim", "Kilitli", VipDiscountBonusLayout);

        vipUpgradeButtonText = CreateTransparentButton(vipPanel.transform, "VipUpgradeButton", "VIP AL", VipUpgradeButtonPosition, VipUpgradeButtonSize, UpgradeVipLevel, 28, new Color(0.35f, 0.16f, 0.03f, 1f));
        ApplyVipFont(vipUpgradeButtonText);
        vipUpgradeButtonText.rectTransform.anchoredPosition = VipUpgradeButtonTextPosition;
        vipUpgradeButtonText.rectTransform.sizeDelta = VipUpgradeButtonTextSize;
        vipUpgradeButtonText.fontStyle = FontStyles.Normal;
        vipUpgradeButtonText.fontSize = 28f;
        vipUpgradeButtonText.fontSizeMax = 28f;
        vipUpgradeButtonText.fontSizeMin = 22f;
        vipUpgradeButtonText.characterSpacing = 0.5f;
        vipUpgradeGemIconImage = CreateVipUpgradeGemIcon(vipUpgradeButtonText.transform.parent);
        AttachPressAnimator(vipUpgradeButtonText.transform.parent as RectTransform, new Vector2(0f, -6f), 0.96f);
        CreateInvisibleButton(vipPanel.transform, "VipCloseButton", new Vector2(592, 309), new Vector2(92, 92), CloseVipPanel);

        vipPanel.transform.SetAsLastSibling();
        vipPanel.SetActive(false);
    }

    private void CreateVipInfoPngElements(Transform parent)
    {
        CreateVipInfoImage(parent, "VipInfoActivePackageBadge", GetVipInfoActivePackageBadgeSprite(), VipInfoActivePackageBadgePosition, VipInfoActivePackageBadgeSize);
        CreateVipInfoImage(parent, "VipInfoCurrentTitle", GetVipInfoCurrentTitleSprite(), VipInfoCurrentTitlePosition, VipInfoCurrentTitleSize);
        CreateVipInfoImage(parent, "VipInfoNextTitle", GetVipInfoNextTitleSprite(), VipInfoNextTitlePosition, VipInfoNextTitleSize);
        CreateVipInfoImage(parent, "VipInfoPriceLine", GetVipInfoPriceLineSprite(), VipInfoPriceLinePosition, VipInfoPriceLineSize);
    }

    private Image CreateVipInfoImage(Transform parent, string objectName, Sprite sprite, Vector2 position, Vector2 size)
    {
        GameObject imageObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        imageObject.transform.SetParent(parent, false);

        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image image = imageObject.GetComponent<Image>();
        image.sprite = sprite;
        image.color = Color.white;
        image.preserveAspect = true;
        image.raycastTarget = false;
        return image;
    }

    private void CreateVipLeftModularElements(Transform parent)
    {
        CreateVipLeftSprite(parent, "VipLeftBadge", GetVipLeftBadgeSprite(), VipLeftBadgePosition, VipLeftBadgeSize);
        vipLeftCoinPillText = CreateVipLeftBonusPill(parent, "VipLeftCoinPill", "Coin +0%", VipLeftCoinPillPosition);
        vipLeftXpPillText = CreateVipLeftBonusPill(parent, "VipLeftXpPill", "XP +0%", VipLeftXpPillPosition);
        vipLeftCustomerPillText = CreateVipLeftBonusPill(parent, "VipLeftCustomerPill", "Müşteri +0%", VipLeftCustomerPillPosition);
        vipLeftPassivePillText = CreateVipLeftBonusPill(parent, "VipLeftPassivePill", "Pasif +0%", VipLeftPassivePillPosition);
        CreateVipLeftDailyRewardLabel(parent);
        CreateVipLeftSprite(parent, "VipLeftGift", GetVipLeftGiftSprite(), VipLeftGiftPosition, VipLeftGiftSize);
        CreateVipLeftClaimButton(parent);
    }

    private void CreateVipProfessionalInfoPanel(Transform parent)
    {
        GameObject rootObject = new GameObject("VipProfessionalInfo", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        rootObject.transform.SetParent(parent, false);

        RectTransform rootRect = rootObject.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.5f, 0.5f);
        rootRect.anchorMax = new Vector2(0.5f, 0.5f);
        rootRect.anchoredPosition = VipProfessionalInfoPosition;
        rootRect.sizeDelta = VipProfessionalInfoSize;

        Image background = rootObject.GetComponent<Image>();
        background.sprite = GetVipProfessionalPanelSprite();
        background.color = Color.white;
        background.preserveAspect = true;
        background.raycastTarget = false;

        GameObject contentObject = new GameObject("Content", typeof(RectTransform));
        contentObject.transform.SetParent(rootObject.transform, false);
        RectTransform contentRect = contentObject.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.5f, 0.5f);
        contentRect.anchorMax = new Vector2(0.5f, 0.5f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(438f, 438f);
        contentRect.localScale = Vector3.one * (VipProfessionalInfoSize.y / 438f);
        Transform content = contentObject.transform;

        CreateVipProfessionalIcon(content, "TierCrest", 0, new Vector2(-134.52f, 112.25f), new Vector2(150f, 150f));
        vipProfessionalUpgradeGemImage = CreateVipProfessionalIcon(content, "UpgradeGem", 1, new Vector2(158.7f, 53.3f), new Vector2(20.4431f, 20.4431f));
        CreateVipProfessionalIcon(content, "CoinIcon", 2, new Vector2(-150.9f, -5.7f), new Vector2(60f, 60f));
        CreateVipProfessionalIcon(content, "XpIcon", 3, new Vector2(38f, -5.7f), new Vector2(60f, 60f));
        CreateVipProfessionalIcon(content, "CustomerIcon", 4, new Vector2(-150.9f, -65.69997f), new Vector2(60f, 60f));
        CreateVipProfessionalIcon(content, "PassiveIcon", 5, new Vector2(38f, -73.79998f), new Vector2(60f, 60f));
        CreateVipProfessionalIcon(content, "GiftIcon", 6, new Vector2(-154.9f, -147f), new Vector2(68f, 68f));

        vipProfessionalTierTitleText = CreateVipProfessionalText(content, "TierTitle", "VIP YOK", new Vector2(59.922f, 146.7f), new Vector2(218f, 48f), 30f);
        vipProfessionalTierTitleText.outlineColor = new Color(0.20f, 0.10f, 0.04f, 0.96f);
        vipProfessionalTierTitleText.outlineWidth = 0.14f;

        vipProfessionalStatusText = CreateVipProfessionalText(content, "Status", "KİLİTLİ", new Vector2(68.4f, 91.6f), new Vector2(104f, 28f), 18f);
        vipProfessionalStatusText.color = Color.white;

        vipProfessionalUpgradeText = CreateVipProfessionalText(content, "UpgradeTarget", "BRONZ VIP'E GEÇ", new Vector2(20.48f, 52.25f), new Vector2(160f, 30f), 16f);
        vipProfessionalUpgradeText.color = new Color(1f, 0.91f, 0.62f, 1f);
        vipProfessionalUpgradeText.alignment = TextAlignmentOptions.Left;

        vipProfessionalUpgradeCostText = CreateVipProfessionalText(content, "UpgradeCost", "0", new Vector2(120.5f, 52.24999f), new Vector2(44f, 30f), 19f);
        vipProfessionalUpgradeCostText.color = new Color(0.30f, 0.13f, 0.02f, 1f);

        vipProfessionalUpgradeButton = CreateInvisibleButton(
            content,
            "VipProfessionalUpgradeButton",
            new Vector2(54f, 52.25f),
            new Vector2(236f, 48f),
            UpgradeVipLevel);

        vipLeftCoinPillText = CreateVipProfessionalText(content, "CoinBonus", "COIN +%0", new Vector2(-75.7052f, -11.75f), new Vector2(121.1997f, 38.5f), 18f);
        vipLeftXpPillText = CreateVipProfessionalText(content, "XpBonus", "XP +%0", new Vector2(119f, -11.75f), new Vector2(94f, 32f), 18f);
        vipLeftCustomerPillText = CreateVipProfessionalText(content, "CustomerBonus", "MÜŞTERİ +%0", new Vector2(-75.705f, -73.8f), new Vector2(102f, 32f), 17f);
        vipLeftPassivePillText = CreateVipProfessionalText(content, "PassiveBonus", "PASİF +%0", new Vector2(127f, -73.80001f), new Vector2(94f, 32f), 17f);

        TMP_Text dailyTitle = CreateVipProfessionalText(content, "DailyGiftTitle", "GÜNLÜK HEDİYE", new Vector2(-37.89999f, -132f), new Vector2(166f, 38f), 18f);
        dailyTitle.color = new Color(1f, 0.89f, 0.55f, 1f);

        vipLeftClaimButtonText = CreateTransparentButton(
            content,
            "VipLeftClaimButton",
            "AL",
            new Vector2(112.5f, -140f),
            new Vector2(126f, 54f),
            ClickVipLeftClaimButton,
            27f,
            new Color(0.32f, 0.13f, 0.01f, 1f));
        ApplyVipFont(vipLeftClaimButtonText);
        vipLeftClaimButtonText.fontStyle = FontStyles.Bold;
        vipLeftClaimButtonText.fontSizeMin = 20f;
        vipLeftClaimButtonText.fontSizeMax = 27f;
        vipProfessionalClaimButton = vipLeftClaimButtonText.GetComponentInParent<Button>();
    }

    private Image CreateVipProfessionalIcon(Transform parent, string objectName, int iconIndex, Vector2 position, Vector2 size)
    {
        GameObject iconObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        iconObject.transform.SetParent(parent, false);

        RectTransform rect = iconObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image image = iconObject.GetComponent<Image>();
        image.sprite = GetVipProfessionalIconSprite(iconIndex);
        image.color = Color.white;
        image.preserveAspect = true;
        image.raycastTarget = false;
        return image;
    }

    private TMP_Text CreateVipProfessionalText(Transform parent, string objectName, string value, Vector2 position, Vector2 size, float maxFontSize)
    {
        TMP_Text text = CreateText(parent, objectName, value, position, size, maxFontSize);
        ApplyVipFont(text);
        text.color = new Color(0.25f, 0.11f, 0.03f, 1f);
        text.alignment = TextAlignmentOptions.Center;
        text.fontStyle = FontStyles.Bold;
        text.enableAutoSizing = true;
        text.fontSizeMin = Mathf.Max(10f, maxFontSize - 7f);
        text.fontSizeMax = maxFontSize;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.overflowMode = TextOverflowModes.Ellipsis;
        text.raycastTarget = false;
        return text;
    }

    private Image CreateVipLeftSprite(Transform parent, string objectName, Sprite sprite, Vector2 position, Vector2 size)
    {
        GameObject imageObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        imageObject.transform.SetParent(parent, false);

        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image image = imageObject.GetComponent<Image>();
        image.sprite = sprite;
        image.color = Color.white;
        image.preserveAspect = true;
        image.raycastTarget = true;

        return image;
    }

    private TMP_Text CreateVipLeftBonusPill(Transform parent, string objectName, string label, Vector2 position)
    {
        GameObject pillObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        pillObject.transform.SetParent(parent, false);

        RectTransform rect = pillObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = VipLeftBonusPillSize;

        Image image = pillObject.GetComponent<Image>();
        image.sprite = GetVipLeftBonusPillSprite();
        image.color = Color.white;
        image.preserveAspect = false;
        image.raycastTarget = true;

        TMP_Text text = CreateText(pillObject.transform, "Text", label, Vector2.zero, new Vector2(VipLeftBonusPillSize.x - 14f, VipLeftBonusPillSize.y - 4f), 15);
        ApplyVipFont(text);
        text.color = new Color(0.20f, 0.10f, 0.03f, 1f);
        text.alignment = TextAlignmentOptions.Center;
        text.fontStyle = FontStyles.Bold;
        text.enableAutoSizing = true;
        text.fontSizeMin = 9f;
        text.fontSizeMax = 15f;
        text.raycastTarget = false;

        return text;
    }

    private Image CreateVipLeftDailyRewardLabel(Transform parent)
    {
        GameObject labelObject = new GameObject("VipLeftDailyRewardLabel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        labelObject.transform.SetParent(parent, false);

        RectTransform rect = labelObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = VipLeftDailyRewardLabelPosition;
        rect.sizeDelta = VipLeftDailyRewardLabelSize;

        Image image = labelObject.GetComponent<Image>();
        image.sprite = GetVipLeftDailyRewardLabelSprite();
        image.color = Color.white;
        image.preserveAspect = false;
        image.raycastTarget = false;

        return image;
    }

    private void CreateVipLeftClaimButton(Transform parent)
    {
        GameObject buttonObject = new GameObject("VipLeftClaimButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = VipLeftClaimButtonPosition;
        rect.sizeDelta = VipLeftClaimButtonSize;

        Image image = buttonObject.GetComponent<Image>();
        image.sprite = GetVipLeftClaimButtonSprite();
        image.color = Color.white;
        image.preserveAspect = true;
        image.raycastTarget = true;

        Button button = buttonObject.GetComponent<Button>();
        button.transition = Selectable.Transition.None;
        button.targetGraphic = image;
        button.onClick.AddListener(ClickVipLeftClaimButton);

        vipLeftClaimButtonText = CreateText(buttonObject.transform, "Text", "AL", VipLeftClaimButtonTextPosition, VipLeftClaimButtonTextSize, 20);
        ApplyVipFont(vipLeftClaimButtonText);
        vipLeftClaimButtonText.color = new Color(0.34f, 0.16f, 0.02f, 1f);
        vipLeftClaimButtonText.alignment = TextAlignmentOptions.Center;
        vipLeftClaimButtonText.fontStyle = FontStyles.Bold;
        vipLeftClaimButtonText.enableAutoSizing = true;
        vipLeftClaimButtonText.fontSizeMin = 12f;
        vipLeftClaimButtonText.fontSizeMax = 20f;
        vipLeftClaimButtonText.raycastTarget = false;

        AttachPressAnimator(rect, new Vector2(0f, -4f), 0.96f);
    }

    private void CreateVipInfoPanel(Transform parent)
    {
        vipInfoPanel = new GameObject("VipInfoPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        vipInfoPanel.transform.SetParent(parent, false);

        RectTransform panelRect = vipInfoPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = new Vector2(0, 30);
        panelRect.sizeDelta = new Vector2(820, 500);

        StylePopupPanel(vipInfoPanel, panelRect.sizeDelta, new Color(1f, 0.91f, 0.76f, 0.98f), new Color(0.40f, 0.20f, 0.54f, 1f));

        TMP_Text title = CreateText(vipInfoPanel.transform, "VipInfoTitle", "VIP Bonuslari", new Vector2(0, 190), new Vector2(720, 60), 36);
        ApplyVipFont(title);
        StylePopupTitle(title, 32);

        vipInfoPanelText = CreateText(vipInfoPanel.transform, "VipInfoText", "", new Vector2(0, 30), new Vector2(710, 250), 24);
        ApplyVipFont(vipInfoPanelText);
        vipInfoPanelText.text = "Vip paketleri kalıcı bonuslar verir."
            + "\nCoin, XP, Müşterilerin sabrı ve pasif gelir artar."
            + "\nGold Vip ile birlikte kalıcı yüzdelik indirimler kazanırsınız"
            + "\nBu indirim yalnızca oyun içi geliştirmelerde geçerlidir.";
        StylePopupBodyText(vipInfoPanelText, 18, 24, TextAlignmentOptions.Center);

        StylePopupButton(CreateButton(vipInfoPanel.transform, "VipInfoCloseButton", "Tamam", new Vector2(0, -170), new Vector2(300, 72), CloseVipInfoPanel, new Color(0.40f, 0.20f, 0.54f), 28), new Color(0.40f, 0.20f, 0.54f), 28);

        vipInfoPanel.transform.SetAsLastSibling();
        vipInfoPanel.SetActive(false);
    }

    private void CreateVipPanelClearSelectionArea(Transform parent)
    {
        GameObject clearObject = new GameObject("VipClearSelectionArea", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        clearObject.transform.SetParent(parent, false);

        RectTransform rect = clearObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(160f, 8f);
        rect.sizeDelta = new Vector2(900f, 430f);

        Image image = clearObject.GetComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.001f);

        Button button = clearObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.None;
        button.onClick.AddListener(ClearVipPreviewSelection);
    }

    private Image CreateVipUpgradeGemIcon(Transform parent)
    {
        GameObject iconObject = new GameObject("VipUpgradeGemIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        iconObject.transform.SetParent(parent, false);

        RectTransform rect = iconObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = VipUpgradeGemIconPosition;
        rect.sizeDelta = VipUpgradeGemIconSize;

        Image image = iconObject.GetComponent<Image>();
        image.sprite = GetGemIconSprite();
        image.color = Color.white;
        image.preserveAspect = true;
        image.raycastTarget = false;
        return image;
    }

    private void AttachPressAnimator(RectTransform target, Vector2 pressedOffset, float pressedScale)
    {
        if (target == null)
        {
            return;
        }

        PressAnimator animator = target.gameObject.AddComponent<PressAnimator>();
        animator.Bind(target, pressedOffset, pressedScale);
    }

#if UNITY_EDITOR
    private void AttachEditorDragProbe(Image image, string label)
    {
        if (image == null)
        {
            return;
        }

        RectTransform rect = image.GetComponent<RectTransform>();
        AttachEditorDragProbe(rect, label);
    }

    private void AttachEditorDragProbe(RectTransform rect, string label)
    {
        if (rect == null)
        {
            return;
        }

        Image image = rect.GetComponent<Image>();
        if (image != null)
        {
            image.raycastTarget = true;
        }

        Graphic graphic = rect.GetComponent<Graphic>();
        if (graphic != null)
        {
            graphic.raycastTarget = true;
        }

        EditorDragProbe probe = rect.gameObject.AddComponent<EditorDragProbe>();
        probe.Bind(rect, label);
    }
#endif

    private TMP_Text CreateVipLevelCard(Transform parent, int level, Vector2 position)
    {
        return CreateVipLevelCard(parent, level, position, false);
    }

    private Vector2 GetVipLockPosition(int level)
    {
        switch (level)
        {
            case 1:
            case 2:
                return new Vector2(-151.9f, 99.8f);
            case 3:
                return new Vector2(-147.7f, 101.4f);
            case 4:
                return new Vector2(-149.7f, 98.3f);
            case 5:
                return new Vector2(-159.4f, 96.7f);
            default:
                return new Vector2(0f, 92f);
        }
    }

    private Vector2 GetVipCardButtonLockPosition(int level)
    {
        return VipCardMedallionCenter;
    }

    private void CreateVipCardHitArea(Transform parent, int level, Vector2 position)
    {
        GameObject hitObject = new GameObject("VipCardHitArea" + level, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        hitObject.transform.SetParent(parent, false);

        RectTransform rect = hitObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(132, 270);

        Image image = hitObject.GetComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0f);
        image.raycastTarget = true;

        Button button = hitObject.GetComponent<Button>();
        int selectedLevel = level;
        button.onClick.AddListener(() => SelectVipPreviewLevel(selectedLevel));
        button.transition = Selectable.Transition.None;

        Image selectGlow = CreatePanelStrip(hitObject.transform, "SelectedGlow", new Vector2(0, -133), new Vector2(112, 10), new Color(1f, 0.78f, 0.18f, 0f));
        selectGlow.raycastTarget = false;
        vipLevelSelectImages.Add(selectGlow);
    }

    private void CreateVipCardButton(Transform parent, int level, Vector2 position)
    {
        GameObject buttonObject = new GameObject("VipCardButton" + level, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(144, 308);
        vipLevelButtonRects.Add(rect);

        Image image = buttonObject.GetComponent<Image>();
        image.sprite = GetVipCardSprite(level);
        image.color = Color.white;
        image.preserveAspect = true;
        image.raycastTarget = true;
        vipLevelCardImages.Add(image);

        Button button = buttonObject.GetComponent<Button>();
        int selectedLevel = level;
        button.onClick.AddListener(() => SelectVipPreviewLevel(selectedLevel));
        button.transition = Selectable.Transition.None;

        GameObject lockObject = new GameObject("LockIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        lockObject.transform.SetParent(buttonObject.transform, false);
        RectTransform lockRect = lockObject.GetComponent<RectTransform>();
        lockRect.anchorMin = new Vector2(0.5f, 0.5f);
        lockRect.anchorMax = new Vector2(0.5f, 0.5f);
        lockRect.anchoredPosition = GetVipCardButtonLockPosition(level);
        lockRect.sizeDelta = new Vector2(42, 42);

        Image lockImage = lockObject.GetComponent<Image>();
        lockImage.sprite = GetVipLockIconSprite();
        lockImage.color = Color.white;
        lockImage.preserveAspect = true;
        lockImage.raycastTarget = false;
        vipLevelLockImages.Add(lockImage);

        TMP_Text titleText = CreateText(buttonObject.transform, "VipCardTitle", "", new Vector2(0f, 49f), new Vector2(130f, 30f), 17);
        ApplyVipFont(titleText);
        titleText.color = new Color(1f, 0.92f, 0.70f, 1f);
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontStyle = FontStyles.Bold;
        titleText.enableAutoSizing = true;
        titleText.fontSizeMin = 12f;
        titleText.fontSizeMax = 17f;
        titleText.raycastTarget = false;
        vipLevelCardTitleTexts.Add(titleText);

        TMP_Text bonusText = CreateText(buttonObject.transform, "VipCardBonus", "", new Vector2(0f, -25f), new Vector2(128f, 104f), 14);
        ApplyVipFont(bonusText);
        bonusText.color = new Color(1f, 0.94f, 0.78f, 1f);
        bonusText.alignment = TextAlignmentOptions.Center;
        bonusText.fontStyle = FontStyles.Bold;
        bonusText.textWrappingMode = TextWrappingModes.Normal;
        bonusText.enableAutoSizing = true;
        bonusText.fontSizeMin = 9f;
        bonusText.fontSizeMax = 14f;
        bonusText.raycastTarget = false;
        vipLevelCardBonusTexts.Add(bonusText);

        TMP_Text statusText = CreateText(buttonObject.transform, "VipCardStatus", "", new Vector2(0f, -126f), new Vector2(112f, 26f), 16);
        ApplyVipFont(statusText);
        statusText.color = new Color(1f, 0.87f, 0.42f, 1f);
        statusText.alignment = TextAlignmentOptions.Center;
        statusText.fontStyle = FontStyles.Bold;
        statusText.enableAutoSizing = true;
        statusText.fontSizeMin = 11f;
        statusText.fontSizeMax = 16f;
        statusText.raycastTarget = false;
        vipLevelCardTexts.Add(statusText);

        Image selectGlow = CreatePanelStrip(buttonObject.transform, "SelectedGlow", new Vector2(0, -151), new Vector2(112, 8), new Color(1f, 0.78f, 0.18f, 0f));
        selectGlow.raycastTarget = false;
        vipLevelSelectImages.Add(selectGlow);
    }

    private void CreateVipCardBottomStarOverlay(Transform parent, int level, Vector2 position)
    {
        GameObject bottomStarObject = new GameObject("VipCardBottomStar" + level, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        bottomStarObject.transform.SetParent(parent, false);
        RectTransform bottomStarRect = bottomStarObject.GetComponent<RectTransform>();
        bottomStarRect.anchorMin = new Vector2(0.5f, 0.5f);
        bottomStarRect.anchorMax = new Vector2(0.5f, 0.5f);
        bottomStarRect.anchoredPosition = position;
        bottomStarRect.sizeDelta = new Vector2(78f, 34f);

        Image bottomStarImage = bottomStarObject.GetComponent<Image>();
        bottomStarImage.sprite = GetVipCardBottomStarSprite(level);
        bottomStarImage.color = Color.white;
        bottomStarImage.preserveAspect = true;
        bottomStarImage.raycastTarget = false;
        bottomStarObject.transform.SetAsLastSibling();
        vipLevelBottomStarImages.Add(bottomStarImage);
        vipLevelBottomStarRects.Add(bottomStarRect);
    }

    private TMP_Text CreateVipLevelCard(Transform parent, int level, Vector2 position, bool textOnly)
    {
        GameObject cardObject = new GameObject("VipLevelCard" + level, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        cardObject.transform.SetParent(parent, false);

        RectTransform rect = cardObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = textOnly ? new Vector2(118, 250) : new Vector2(178, 330);

        Image image = cardObject.GetComponent<Image>();
        image.color = textOnly ? new Color(1f, 1f, 1f, 0f) : GetVipTierColor(level);
        image.raycastTarget = false;
        if (!textOnly)
        {
            vipLevelCardImages.Add(image);
        }

        if (!textOnly)
        {
            CreatePanelStrip(cardObject.transform, "TopShine", new Vector2(0, 144), new Vector2(146, 8), new Color(1f, 1f, 1f, 0.18f));
        }

        TMP_Text text = CreateText(cardObject.transform, "Text", "", textOnly ? new Vector2(0, -8) : new Vector2(0, -8), textOnly ? new Vector2(106, 210) : new Vector2(148, 286), textOnly ? 15 : 17);
        ApplyVipFont(text);
        text.color = textOnly ? Color.white : GetVipTierTextColor(level);
        text.alignment = TextAlignmentOptions.Center;
        text.enableWordWrapping = true;
        text.fontStyle = FontStyles.Bold;
        text.enableAutoSizing = true;
        text.fontSizeMin = textOnly ? 10 : 12;
        text.fontSizeMax = textOnly ? 15 : 17;

        if (textOnly)
        {
            GameObject lockObject = new GameObject("LockIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            lockObject.transform.SetParent(cardObject.transform, false);
            RectTransform lockRect = lockObject.GetComponent<RectTransform>();
            lockRect.anchorMin = new Vector2(0.5f, 0.5f);
            lockRect.anchorMax = new Vector2(0.5f, 0.5f);
            lockRect.anchoredPosition = GetVipLockPosition(level);
            lockRect.sizeDelta = new Vector2(42, 42);

            Image lockImage = lockObject.GetComponent<Image>();
            lockImage.sprite = GetVipLockIconSprite();
            lockImage.color = Color.white;
            lockImage.preserveAspect = true;
            lockImage.raycastTarget = false;
            vipLevelLockImages.Add(lockImage);
        }

        return text;
    }

    private TMP_Text CreateVipBonusPill(Transform parent, string title, string value, VipBonusSlotLayout layout)
    {
        GameObject pill = new GameObject("VipBonus" + title, typeof(RectTransform));
        pill.transform.SetParent(parent, false);

        RectTransform rect = pill.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = layout.PillPosition;
        rect.sizeDelta = new Vector2(168, 72);

        Image dot = CreatePanelStrip(pill.transform, "Icon", new Vector2(-58, 2), new Vector2(44, 44), new Color(layout.AccentColor.r, layout.AccentColor.g, layout.AccentColor.b, 0f));
        dot.raycastTarget = false;

        TMP_Text text = CreateText(pill.transform, "Text", title + "\n" + value, layout.TextPosition, layout.TextSize, 16);
        ApplyVipFont(text);
        text.color = new Color(0.42f, 0.22f, 0.08f, 1f);
        text.alignment = TextAlignmentOptions.Left;
        text.fontStyle = FontStyles.Bold;
        text.enableAutoSizing = true;
        text.fontSizeMin = 11;
        text.fontSizeMax = 16;
        return text;
    }

    private Image CreatePanelStrip(Transform parent, string name, Vector2 position, Vector2 size, Color color)
    {
        GameObject stripObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        stripObject.transform.SetParent(parent, false);

        RectTransform rect = stripObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image image = stripObject.GetComponent<Image>();
        image.color = color;
        return image;
    }

    private Sprite GetWorkshopUiSprite(string assetName)
    {
        if (string.IsNullOrEmpty(assetName)) return null;
        if (workshopUiSprites.TryGetValue(assetName, out Sprite cached)) return cached;

        string resourcePath = "Art/UI/Workshop/Modular/" + assetName;
        Texture2D texture = Resources.Load<Texture2D>(resourcePath);
        if (texture == null)
        {
            workshopUiSprites[assetName] = null;
            return null;
        }

        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
        sprite.name = assetName;
        workshopUiSprites[assetName] = sprite;
        return sprite;
    }

    private Sprite GetWorkshopCategoryIconSprite(WorkshopDecorCategory category)
    {
        string suffix;
        switch (category)
        {
            case WorkshopDecorCategory.Wall: suffix = "wall"; break;
            case WorkshopDecorCategory.Floor: suffix = "floor"; break;
            case WorkshopDecorCategory.Table: suffix = "table"; break;
            case WorkshopDecorCategory.Chair: suffix = "chair"; break;
            case WorkshopDecorCategory.Rug: suffix = "rug"; break;
            case WorkshopDecorCategory.Shelf: suffix = "shelf"; break;
            case WorkshopDecorCategory.Counter: suffix = "counter"; break;
            case WorkshopDecorCategory.Plant: suffix = "plant"; break;
            case WorkshopDecorCategory.WallArt: suffix = "wall_art"; break;
            default: return null;
        }
        return GetWorkshopUiSprite("workshop_category_" + suffix);
    }

    private Image CreateWorkshopUiImage(Transform parent, string name, Vector2 position, Vector2 size, string assetName, bool preserveAspect)
    {
        Image image = CreateWorkshopResourceIcon(parent, name, position, GetWorkshopUiSprite(assetName));
        image.GetComponent<RectTransform>().sizeDelta = size;
        image.preserveAspect = preserveAspect;
        image.raycastTarget = false;
        return image;
    }

    private Image CreateWorkshopPanelBox(Transform parent, string name, Vector2 position, Vector2 size, Color color)
    {
        GameObject boxObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        boxObject.transform.SetParent(parent, false);

        RectTransform rect = boxObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image image = boxObject.GetComponent<Image>();
        image.color = color;
        return image;
    }

    private Image CreateWorkshopResourceIcon(Transform parent, string name, Vector2 position, Sprite sprite)
    {
        GameObject iconObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        iconObject.transform.SetParent(parent, false);

        RectTransform rect = iconObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(46, 46);

        Image image = iconObject.GetComponent<Image>();
        image.sprite = sprite;
        image.color = Color.white;
        image.preserveAspect = true;
        return image;
    }

    private TMP_Text CreateDecorTabButton(string objectName, string label, Vector2 position, UnityEngine.Events.UnityAction action, bool active)
    {
        TMP_Text text = CreateButton(decorPanel.transform, objectName, label, position, new Vector2(96, 36), action, active ? new Color(0.56f, 0.34f, 0.10f, 1f) : new Color(0.12f, 0.10f, 0.08f, 1f), 18);
        text.color = active ? new Color(1f, 0.95f, 0.84f) : new Color(0.70f, 0.62f, 0.50f);
        text.fontStyle = FontStyles.Bold;
        return text;
    }

    private void UpdateDecorTabs(DecorItemData item)
    {
        for (int i = 0; i < decorTabTexts.Count; i++)
        {
            TMP_Text tabText = decorTabTexts[i];
            if (tabText == null)
            {
                continue;
            }

            bool active = false;
            if (item != null)
            {
                active = (i == 0 && item.Slot == DecorSlot.Wall)
                    || (i == 1 && item.Slot == DecorSlot.Floor)
                    || (i == 2 && item.Slot == DecorSlot.Table);
            }

            tabText.color = active ? new Color(1f, 0.95f, 0.84f) : new Color(0.70f, 0.62f, 0.50f);

            Image tabImage = tabText.GetComponentInParent<Image>();
            if (tabImage != null)
            {
                tabImage.color = active ? new Color(0.56f, 0.34f, 0.10f, 1f) : new Color(0.12f, 0.10f, 0.08f, 1f);
            }
        }
    }

    private void CreateDecorResourcePill(string objectName, Vector2 position)
    {
        CreateWorkshopPanelBox(decorPanel.transform, objectName, position, new Vector2(104, 40), new Color(0.12f, 0.10f, 0.08f, 1f));
    }

    private void CreateDecorPreviewScene(Transform parent, Vector2 center)
    {
        Vector2 offset = center;
        decorPreviewBackWallImage = CreateSceneQuad(parent, "DecorPreviewBackWall",
            offset + new Vector2(-255, 118),
            offset + new Vector2(255, 118),
            offset + new Vector2(214, -34),
            offset + new Vector2(-214, -34),
            GetWallColor());
        decorPreviewLeftWallImage = CreateSceneQuad(parent, "DecorPreviewLeftWall",
            offset + new Vector2(-255, 118),
            offset + new Vector2(-214, -34),
            offset + new Vector2(-214, -118),
            offset + new Vector2(-286, -78),
            ShadeColor(GetWallColor(), 0.84f));
        decorPreviewRightWallImage = CreateSceneQuad(parent, "DecorPreviewRightWall",
            offset + new Vector2(255, 118),
            offset + new Vector2(214, -34),
            offset + new Vector2(214, -118),
            offset + new Vector2(286, -78),
            ShadeColor(GetWallColor(), 0.78f));
        decorPreviewFloorImage = CreateSceneQuad(parent, "DecorPreviewFloor",
            offset + new Vector2(-214, -34),
            offset + new Vector2(214, -34),
            offset + new Vector2(286, -78),
            offset + new Vector2(-286, -78),
            GetFloorColor());

        CreatePanelStrip(parent, "DecorPreviewWallTrim", offset + new Vector2(0, 86), new Vector2(400, 22), new Color(0.70f, 0.43f, 0.20f, 0.95f));
        Image panelLine = CreateWorkshopPanelBox(parent, "DecorPreviewWallPanel", offset + new Vector2(0, 22), new Vector2(330, 46), new Color(1f, 0.78f, 0.88f, 0.22f));
        panelLine.raycastTarget = false;
        CreatePanelStrip(parent, "DecorPreviewPanelInner", offset + new Vector2(0, 22), new Vector2(270, 3), new Color(0.45f, 0.22f, 0.26f, 0.55f));

        Image tableTop = CreateWorkshopPanelBox(parent, "DecorPreviewTableTop", offset + new Vector2(0, -88), new Vector2(152, 34), GetTableColor());
        tableTop.raycastTarget = false;
        tableTop.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        decorPreviewTableTopImage = tableTop;
        Image tableLeg = CreateWorkshopPanelBox(parent, "DecorPreviewTableLeg", offset + new Vector2(0, -126), new Vector2(16, 48), new Color(0.20f, 0.14f, 0.11f, 1f));
        tableLeg.raycastTarget = false;
        Image tableShadow = CreateWorkshopPanelBox(parent, "DecorPreviewTableShadow", offset + new Vector2(0, -152), new Vector2(188, 24), new Color(0f, 0f, 0f, 0.18f));
        tableShadow.raycastTarget = false;
    }

    private Image CreateDecorButtonIcon(TMP_Text buttonText, DecorItemData item)
    {
        if (buttonText == null || item == null)
        {
            return null;
        }

        Transform buttonTransform = buttonText.transform.parent;
        if (buttonTransform == null)
        {
            return null;
        }

        RectTransform textRect = buttonText.GetComponent<RectTransform>();
        RectTransform buttonRect = buttonTransform.GetComponent<RectTransform>();
        bool compactButton = buttonRect != null && buttonRect.sizeDelta.x <= 100f;
        if (textRect != null)
        {
            textRect.anchoredPosition = compactButton ? Vector2.zero : new Vector2(46, 0);
            textRect.sizeDelta = compactButton ? new Vector2(1, 1) : new Vector2(240, 76);
        }

        buttonText.fontSize = compactButton ? 1 : 18;
        buttonText.enableWordWrapping = true;
        buttonText.raycastTarget = false;

        GameObject iconObject = new GameObject("DecorIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        iconObject.transform.SetParent(buttonTransform, false);

        RectTransform rect = iconObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = compactButton ? Vector2.zero : new Vector2(-130, 0);
        rect.sizeDelta = compactButton ? new Vector2(52, 44) : new Vector2(74, 74);

        Image image = iconObject.GetComponent<Image>();
        image.sprite = GetDecorIconSprite(item);
        image.color = Color.white;
        image.preserveAspect = true;
        image.raycastTarget = false;
        iconObject.transform.SetAsFirstSibling();
        return image;
    }

    private void StylePopupPanel(GameObject panelObject, Vector2 size, Color fillColor, Color accentColor)
    {
        Image panelImage = panelObject.GetComponent<Image>();
        panelImage.sprite = CreateRoundedRectSprite(size, fillColor, new Color(0.18f, 0.10f, 0.05f, 1f), 18, 6);
        panelImage.color = Color.white;

        CreatePanelStrip(panelObject.transform, "PopupTopShadow", new Vector2(0, (size.y * 0.5f) - 16f), new Vector2(size.x - 36f, 10f), new Color(1f, 1f, 1f, 0.12f));
        CreatePanelStrip(panelObject.transform, "PopupHeader", new Vector2(0, (size.y * 0.5f) - 52f), new Vector2(size.x - 56f, 70f), accentColor);
        CreatePanelStrip(panelObject.transform, "PopupHeaderLine", new Vector2(0, (size.y * 0.5f) - 89f), new Vector2(size.x - 70f, 4f), new Color(1f, 0.78f, 0.34f, 0.85f));
    }

    private void StylePopupTitle(TMP_Text title, float maxSize)
    {
        title.color = new Color(1f, 0.94f, 0.78f, 1f);
        title.fontStyle = FontStyles.Bold;
        title.enableAutoSizing = true;
        title.fontSizeMin = 20;
        title.fontSizeMax = maxSize;
        title.enableWordWrapping = false;
        title.overflowMode = TextOverflowModes.Ellipsis;
        title.outlineColor = new Color(0.12f, 0.06f, 0.02f, 0.9f);
        title.outlineWidth = 0.08f;
    }

    private void StylePopupBodyText(TMP_Text text, float minSize, float maxSize, TextAlignmentOptions alignment)
    {
        text.color = new Color(0.16f, 0.10f, 0.06f, 1f);
        text.alignment = alignment;
        text.enableWordWrapping = true;
        text.enableAutoSizing = true;
        text.fontSizeMin = minSize;
        text.fontSizeMax = maxSize;
    }

    private TMP_Text StylePopupButton(TMP_Text labelText, Color fillColor, float maxSize)
    {
        Image image = labelText.GetComponentInParent<Image>();
        RectTransform rect = labelText.GetComponentInParent<RectTransform>();
        if (image != null && rect != null)
        {
            image.sprite = CreateRoundedRectSprite(rect.sizeDelta, fillColor, ShadeColor(fillColor, 0.55f), 14, 4);
            image.color = Color.white;
        }

        labelText.color = Color.white;
        labelText.fontStyle = FontStyles.Bold;
        labelText.enableAutoSizing = true;
        labelText.fontSizeMin = 13;
        labelText.fontSizeMax = maxSize;
        labelText.enableWordWrapping = true;
        labelText.outlineColor = new Color(0f, 0f, 0f, 0.45f);
        labelText.outlineWidth = 0.08f;
        return labelText;
    }

    private Sprite CreateRoundedRectSprite(Vector2 size, Color fillColor, Color borderColor, int cornerRadius, int border)
    {
        int width = Mathf.Max(24, Mathf.RoundToInt(size.x));
        int height = Mathf.Max(24, Mathf.RoundToInt(size.y));
        int radius = Mathf.Clamp(cornerRadius, 0, Mathf.Min(width, height) / 2);
        int borderSize = Mathf.Clamp(border, 0, Mathf.Min(width, height) / 3);
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        Color clear = new Color(0f, 0f, 0f, 0f);
        int innerWidth = width - (borderSize * 2);
        int innerHeight = height - (borderSize * 2);
        int innerRadius = Mathf.Max(0, radius - borderSize);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color pixel = clear;
                if (IsInsideRoundedRectPixel(x, y, width, height, radius))
                {
                    pixel = borderColor;
                }

                if (IsInsideRoundedRectPixel(x - borderSize, y - borderSize, innerWidth, innerHeight, innerRadius))
                {
                    pixel = fillColor;
                }

                texture.SetPixel(x, y, pixel);
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
    }

    private bool IsInsideRoundedRectPixel(int x, int y, int width, int height, int radius)
    {
        if (x < 0 || y < 0 || x >= width || y >= height)
        {
            return false;
        }

        int left = radius;
        int right = width - radius - 1;
        int bottom = radius;
        int top = height - radius - 1;

        if ((x >= left && x <= right) || (y >= bottom && y <= top))
        {
            return true;
        }

        int centerX = x < left ? left : right;
        int centerY = y < bottom ? bottom : top;
        int dx = x - centerX;
        int dy = y - centerY;
        return dx * dx + dy * dy <= radius * radius;
    }

    private void CreateTimedOfferButton(Transform parent)
    {
        offerButtonText = CreateButton(parent, "TimedOfferButton", "2x\n60", new Vector2(350, 245), new Vector2(104, 104), OpenTimedOfferPanel, new Color(0.18f, 0.38f, 0.82f), 22);
        offerButtonObject = offerButtonText.GetComponentInParent<Button>().gameObject;
        offerButtonObject.SetActive(false);
    }

    private void CreateTimedOfferPanel(Transform parent)
    {
        offerPanel = new GameObject("TimedOfferPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        offerPanel.transform.SetParent(parent, false);

        RectTransform panelRect = offerPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = new Vector2(0, 40);
        panelRect.sizeDelta = new Vector2(860, 390);

        StylePopupPanel(offerPanel, panelRect.sizeDelta, new Color(0.90f, 0.94f, 1f, 0.98f), new Color(0.18f, 0.38f, 0.82f, 1f));

        TMP_Text title = CreateText(offerPanel.transform, "TimedOfferTitle", "2x Coin", new Vector2(0, 145), new Vector2(760, 64), 38);
        StylePopupTitle(title, 34);

        offerPanelText = CreateText(offerPanel.transform, "TimedOfferText", "5 dakikaligina\n2x coin kazan", new Vector2(0, 42), new Vector2(760, 135), 34);
        StylePopupBodyText(offerPanelText, 22, 34, TextAlignmentOptions.Center);

        StylePopupButton(CreateButton(offerPanel.transform, "TimedOfferWatchButton", "Izle", new Vector2(0, -120), new Vector2(420, 92), WatchTimedOfferAd, new Color(0.18f, 0.38f, 0.82f), 34), new Color(0.18f, 0.38f, 0.82f), 34);
        offerPanel.transform.SetAsLastSibling();
        offerPanel.SetActive(false);
    }

    private void CreateAdBoostConfirmPanel(Transform parent)
    {
        adBoostConfirmPanel = new GameObject("AdBoostConfirmPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        adBoostConfirmPanel.transform.SetParent(parent, false);

        RectTransform panelRect = adBoostConfirmPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = new Vector2(0, 40);
        panelRect.sizeDelta = new Vector2(860, 430);

        StylePopupPanel(adBoostConfirmPanel, panelRect.sizeDelta, new Color(0.90f, 0.94f, 1f, 0.98f), new Color(0.18f, 0.38f, 0.82f, 1f));

        TMP_Text title = CreateText(adBoostConfirmPanel.transform, "AdBoostConfirmTitle", "2x Coin", new Vector2(0, 160), new Vector2(760, 70), 40);
        StylePopupTitle(title, 35);

        TMP_Text message = CreateText(adBoostConfirmPanel.transform, "AdBoostConfirmText", "Reklam izleyerek\n5 dakikalik 2x coin kazanmak\nister misiniz?", new Vector2(0, 38), new Vector2(760, 150), 28);
        StylePopupBodyText(message, 20, 28, TextAlignmentOptions.Center);

        StylePopupButton(CreateButton(adBoostConfirmPanel.transform, "AdBoostConfirmWatchButton", "Izle", new Vector2(-150, -145), new Vector2(260, 78), ConfirmAdBoostReward, new Color(0.18f, 0.38f, 0.82f), 30), new Color(0.18f, 0.38f, 0.82f), 30);
        StylePopupButton(CreateButton(adBoostConfirmPanel.transform, "AdBoostConfirmCancelButton", "Vazgec", new Vector2(170, -145), new Vector2(260, 78), CloseAdBoostConfirmPanel, new Color(0.20f, 0.20f, 0.20f), 28), new Color(0.20f, 0.20f, 0.20f), 28);

        adBoostConfirmPanel.transform.SetAsLastSibling();
        adBoostConfirmPanel.SetActive(false);
    }

    private void CreateBackgroundTapArea(Transform parent)
    {
        GameObject tapArea = new GameObject("BackgroundTapArea", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        tapArea.transform.SetParent(parent, false);

        RectTransform rect = tapArea.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = tapArea.GetComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.01f);

        Button button = tapArea.GetComponent<Button>();
        button.onClick.AddListener(SellCoffeeFromEmptySpace);
    }

    private void CreateCafeObjectHotspots(Transform parent)
    {
        cafeObjectHotspots.Clear();
        cafeObjectHotspots.Add(CreateCafeObjectHotspot(parent, "CoffeePackagesHotspot", new Vector2(312.4f, 365.85f), new Vector2(244.8f, 137.7f), OpenCoffeePanel));
        cafeObjectHotspots.Add(CreateCafeObjectHotspot(parent, "CoffeeMachineHotspot", new Vector2(-296.57f, 168.4247f), new Vector2(236.87f, 214.1495f), OpenMachinePanel));
        cafeObjectHotspots.Add(CreateCafeObjectHotspot(parent, "BaristaCounterHotspot", new Vector2(-48.988f, 224.85f), new Vector2(172.02f, 286.3f), OpenBaristaPanel));
    }

    private GameObject CreateCafeObjectHotspot(Transform parent, string objectName, Vector2 position, Vector2 size, UnityEngine.Events.UnityAction action)
    {
        GameObject hotspot = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        hotspot.transform.SetParent(parent, false);

        RectTransform rect = hotspot.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image image = hotspot.GetComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.001f);

        Button button = hotspot.GetComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.None;
        button.onClick.AddListener(action);
        return hotspot;
    }

    private void HideLegacyUpgradeButton(TMP_Text buttonText)
    {
        if (buttonText == null || buttonText.transform.parent == null)
        {
            return;
        }

        buttonText.transform.parent.gameObject.SetActive(false);
    }

    private void CreateCafeScene(Transform parent)
    {
        GameObject sceneRoot = new GameObject("CafeSceneRoot", typeof(RectTransform));
        sceneRoot.transform.SetParent(parent, false);
        RectTransform sceneRect = sceneRoot.GetComponent<RectTransform>();
        sceneRect.anchorMin = new Vector2(0.5f, 0.5f);
        sceneRect.anchorMax = new Vector2(0.5f, 0.5f);
        sceneRect.anchoredPosition = new Vector2(0, -5);
        sceneRect.sizeDelta = Vector2.zero;
        float sceneScale = GetResponsiveCafeSceneScale(parent);
        sceneRect.localScale = new Vector3(sceneScale, sceneScale, 1f);
        parent = sceneRoot.transform;

        isoFloorShadowGraphic = CreateSceneQuad(parent, "IsoFloorShadow", new Vector2(-620, 102), new Vector2(620, 102), new Vector2(705, -390), new Vector2(-705, -390), new Color(0.08f, 0.07f, 0.06f, 0.30f));
        isoFloorGraphic = CreateSceneQuad(parent, "IsoFloor", new Vector2(-535, 94), new Vector2(535, 94), new Vector2(670, -350), new Vector2(-670, -350), GetFloorColor());
        isoBackWallGraphic = CreateSceneQuad(parent, "IsoBackWall", new Vector2(-535, 360), new Vector2(535, 360), new Vector2(535, 94), new Vector2(-535, 94), GetWallColor());
        isoLeftWallGraphic = CreateSceneQuad(parent, "IsoLeftWall", new Vector2(-695, -332), new Vector2(-535, 94), new Vector2(-535, 360), new Vector2(-695, 30), ShadeColor(GetWallColor(), 0.86f));
        isoRightWallGraphic = CreateSceneQuad(parent, "IsoRightWall", new Vector2(535, 94), new Vector2(695, -332), new Vector2(695, 30), new Vector2(535, 360), ShadeColor(GetWallColor(), 0.80f));

        Image roomShadow = CreateSceneBox(parent, "RoomShadow", new Vector2(0, -118), new Vector2(1240, 690), new Color(0.16f, 0.14f, 0.13f, 0.0f));
        floorPreviewImage = CreateSceneBox(parent, "CafeFloor", new Vector2(0, -170), new Vector2(1110, 565), new Color(1f, 1f, 1f, 0f));
        backgroundTapImage = CreateSceneBox(parent, "BackWall", new Vector2(0, 227), new Vector2(980, 255), new Color(1f, 1f, 1f, 0f));
        leftWallImage = CreateSceneBox(parent, "LeftWall", new Vector2(-610, -22), new Vector2(190, 720), new Color(1f, 1f, 1f, 0f));
        rightWallImage = CreateSceneBox(parent, "RightWall", new Vector2(610, -22), new Vector2(190, 720), new Color(1f, 1f, 1f, 0f));
        backWallTopTrim = CreateSceneBox(parent, "BackWallTopTrim", new Vector2(0, 360), new Vector2(920, 18), new Color(0.30f, 0.24f, 0.20f));
        Image backTrim = CreateSceneBox(parent, "BackWallTrim", new Vector2(0, 94), new Vector2(1080, 18), new Color(0.36f, 0.28f, 0.22f));
        Image leftTrim = CreateSceneBox(parent, "LeftWallTrim", new Vector2(-610, -74), new Vector2(22, 540), new Color(0.36f, 0.28f, 0.22f));
        Image rightTrim = CreateSceneBox(parent, "RightWallTrim", new Vector2(610, -74), new Vector2(22, 540), new Color(0.36f, 0.28f, 0.22f));
        leftCornerLine = CreateSceneBox(parent, "LeftCornerLine", new Vector2(-535, 227), new Vector2(12, 266), new Color(0.22f, 0.18f, 0.15f, 0.65f));
        rightCornerLine = CreateSceneBox(parent, "RightCornerLine", new Vector2(535, 227), new Vector2(12, 266), new Color(0.22f, 0.18f, 0.15f, 0.65f));
        floorBackEdge = CreateSceneBox(parent, "FloorBackEdge", new Vector2(0, 94), new Vector2(1070, 12), new Color(0.20f, 0.17f, 0.14f, 0.35f));
        floorCenterLine = CreateSceneBox(parent, "FloorCenterLine", new Vector2(0, -125), new Vector2(8, 430), new Color(1f, 1f, 1f, 0.12f));
        floorLeftPerspectiveLine = CreateSceneBox(parent, "FloorLeftPerspectiveLine", new Vector2(-285, -122), new Vector2(8, 425), new Color(1f, 1f, 1f, 0.10f));
        floorRightPerspectiveLine = CreateSceneBox(parent, "FloorRightPerspectiveLine", new Vector2(285, -122), new Vector2(8, 425), new Color(1f, 1f, 1f, 0.10f));
        roomAccentStrip = CreateSceneBox(parent, "RoomAccentStrip", new Vector2(0, 196), new Vector2(780, 18), GetRoomAccentColor());

        RotateSceneBox(leftWallImage, -7f);
        RotateSceneBox(rightWallImage, 7f);
        RotateSceneBox(leftTrim, -18f);
        RotateSceneBox(rightTrim, 18f);
        RotateSceneBox(floorLeftPerspectiveLine, -24f);
        RotateSceneBox(floorRightPerspectiveLine, 24f);

        tablePreviewImage = CreateSceneBox(parent, "CafeCounter", new Vector2(-130, 22), new Vector2(600, 108), GetTableColor());
        Image counterTop = CreateSceneBox(parent, "CounterTop", new Vector2(-130, 86), new Vector2(650, 28), new Color(0.72f, 0.48f, 0.26f));
        Image coffeeMachine = CreateSceneBox(parent, "CoffeeMachineVisual", new Vector2(40, 116), new Vector2(150, 90), new Color(0.28f, 0.28f, 0.32f));
        Image machineTop = CreateSceneBox(parent, "CoffeeMachineTop", new Vector2(40, 166), new Vector2(120, 24), new Color(0.72f, 0.72f, 0.76f));
        Image cup = CreateSceneBox(parent, "CounterCup", new Vector2(245, 114), new Vector2(48, 42), new Color(0.96f, 0.92f, 0.82f));

        Image door = CreateSceneBox(parent, "CustomerDoor", new Vector2(620, -28), new Vector2(106, 198), new Color(0.38f, 0.24f, 0.15f));
        Image doorGlass = CreateSceneBox(parent, "DoorGlass", new Vector2(620, 8), new Vector2(66, 88), new Color(0.62f, 0.85f, 0.95f));
        Image window = CreateSceneBox(parent, "CafeWindow", new Vector2(-642, 34), new Vector2(94, 134), new Color(0.72f, 0.92f, 1f));
        Image menuBoard = CreateSceneBox(parent, "MenuBoard", new Vector2(-340, 172), new Vector2(140, 64), new Color(0.16f, 0.20f, 0.16f));
        Image plantPot = CreateSceneBox(parent, "PlantPot", new Vector2(-345, -242), new Vector2(64, 50), new Color(0.48f, 0.22f, 0.16f));
        Image plantLeaf = CreateSceneBox(parent, "PlantLeaf", new Vector2(-345, -194), new Vector2(90, 60), new Color(0.20f, 0.58f, 0.32f));
        Image customerSpot = CreateSceneBox(parent, "CustomerSpot", new Vector2(180, -252), new Vector2(240, 112), new Color(0.88f, 0.76f, 0.56f));
        productionConveyorVisual = CreateSceneBox(parent, "ProductionConveyor", new Vector2(-245, -112), new Vector2(470, 58), new Color(0.22f, 0.28f, 0.32f));
        productionMachineVisual = CreateSceneBox(parent, "ProductionMachine", new Vector2(-275, -35), new Vector2(210, 126), new Color(0.16f, 0.24f, 0.30f));
        vipRugVisual = CreateSceneBox(parent, "VipRug", new Vector2(200, -188), new Vector2(340, 154), new Color(0.42f, 0.18f, 0.34f));
        vipCounterVisual = CreateSceneBox(parent, "VipCounter", new Vector2(140, 20), new Vector2(420, 116), new Color(0.18f, 0.16f, 0.20f));
        vipPlantVisual = CreateSceneBox(parent, "VipPlant", new Vector2(410, -82), new Vector2(102, 150), new Color(0.20f, 0.48f, 0.32f));

        TMP_Text doorLabel = CreateSceneLabel(parent, "DoorLabel", "GIRIS", new Vector2(620, 82), new Vector2(90, 35), 15);
        TMP_Text counterLabel = CreateSceneLabel(parent, "CounterLabel", "TEZGAH", new Vector2(-130, 22), new Vector2(190, 34), 20);
        TMP_Text machineLabel = CreateSceneLabel(parent, "MachineLabel", "MAKINE", new Vector2(40, 116), new Vector2(112, 36), 14);
        TMP_Text menuLabel = CreateSceneLabel(parent, "MenuLabel", "MENU", new Vector2(-340, 172), new Vector2(110, 35), 19);
        TMP_Text customerSpotLabel = CreateSceneLabel(parent, "CustomerSpotLabel", "MUSTERI\nALANI", new Vector2(180, -252), new Vector2(165, 62), 16);
        Sprite cafeSceneSprite = GetWarmWoodCafeSceneNoMachineSprite();
        cafeSceneImage = CreateSceneBox(parent, "WarmWoodCafeScene", new Vector2(0, 20), new Vector2(1888, 1079), cafeSceneSprite != null ? Color.white : new Color(1f, 1f, 1f, 0f));
        cafeSceneImage.sprite = cafeSceneSprite;
        cafeSceneImage.preserveAspect = true;
        cafeSceneImage.raycastTarget = false;

        baristaLockedSceneImage = CreateSceneBox(parent, "BaristaLockedPlaceholder", new Vector2(-48.10552f, 203f), new Vector2(176.8f, 340f), Color.white);
        baristaLockedSceneImage.sprite = GetBaristaLockedPlaceholderSprite();
        baristaLockedSceneImage.preserveAspect = true;
        baristaLockedSceneImage.raycastTarget = false;
#if UNITY_EDITOR
        AttachEditorDragProbe(baristaLockedSceneImage, "BaristaLockedPlaceholder");
#endif

        baristaSceneImage = CreateSceneBox(parent, "BaristaSceneSprite", new Vector2(-48.10552f, 203f), new Vector2(174.5f, 340f), Color.white);
        baristaSceneImage.sprite = GetBaristaSceneSprite();
        baristaSceneImage.preserveAspect = true;
        baristaSceneImage.raycastTarget = false;
#if UNITY_EDITOR
        AttachEditorDragProbe(baristaSceneImage, "BaristaSceneSprite");
#endif

        baristaCounterForegroundImage = CreateSceneBox(parent, "BaristaCounterForeground", new Vector2(-130.9856f, -30.2488f), new Vector2(813.0144f, 304.8804f), Color.white);
        baristaCounterForegroundImage.sprite = GetBaristaCounterForegroundSprite();
        baristaCounterForegroundImage.preserveAspect = true;
        baristaCounterForegroundImage.raycastTarget = false;

        coffeeMachineSceneImage = CreateSceneBox(parent, "CoffeeMachineSceneSprite", new Vector2(-287.378f, 181.4737f), new Vector2(217.933f, 287.9426f), Color.white);
        coffeeMachineSceneImage.sprite = GetCoffeeMachineSceneSprite();
        coffeeMachineSceneImage.preserveAspect = true;
        coffeeMachineSceneImage.raycastTarget = false;

        coffeeMachineLockedPlaceholderImage = CreateSceneBox(parent, "CoffeeMachineLockedPlaceholder", new Vector2(-294.5363f, 164.3574f), new Vector2(216.5335f, 206.0149f), Color.white);
        coffeeMachineLockedPlaceholderImage.sprite = GetCoffeeMachineLockedPlaceholderSprite();
        coffeeMachineLockedPlaceholderImage.preserveAspect = true;
        coffeeMachineLockedPlaceholderImage.raycastTarget = false;
#if UNITY_EDITOR
        AttachEditorDragProbe(coffeeMachineLockedPlaceholderImage, "CoffeeMachineLockedPlaceholder");
#endif
        roomLockOverlay = CreateSceneBox(parent, "RoomLockOverlay", new Vector2(0, -10), new Vector2(1700, 920), new Color(0.05f, 0.06f, 0.07f, 1f));

        cafePrototypeVisuals.Clear();
        cafePrototypeVisuals.Add(isoFloorShadowGraphic.gameObject);
        cafePrototypeVisuals.Add(isoFloorGraphic.gameObject);
        cafePrototypeVisuals.Add(isoBackWallGraphic.gameObject);
        cafePrototypeVisuals.Add(isoLeftWallGraphic.gameObject);
        cafePrototypeVisuals.Add(isoRightWallGraphic.gameObject);
        cafePrototypeVisuals.Add(roomShadow.gameObject);
        cafePrototypeVisuals.Add(floorPreviewImage.gameObject);
        cafePrototypeVisuals.Add(backgroundTapImage.gameObject);
        cafePrototypeVisuals.Add(leftWallImage.gameObject);
        cafePrototypeVisuals.Add(rightWallImage.gameObject);
        cafePrototypeVisuals.Add(backWallTopTrim.gameObject);
        cafePrototypeVisuals.Add(backTrim.gameObject);
        cafePrototypeVisuals.Add(leftTrim.gameObject);
        cafePrototypeVisuals.Add(rightTrim.gameObject);
        cafePrototypeVisuals.Add(leftCornerLine.gameObject);
        cafePrototypeVisuals.Add(rightCornerLine.gameObject);
        cafePrototypeVisuals.Add(floorBackEdge.gameObject);
        cafePrototypeVisuals.Add(floorCenterLine.gameObject);
        cafePrototypeVisuals.Add(floorLeftPerspectiveLine.gameObject);
        cafePrototypeVisuals.Add(floorRightPerspectiveLine.gameObject);
        cafePrototypeVisuals.Add(roomAccentStrip.gameObject);
        cafePrototypeVisuals.Add(tablePreviewImage.gameObject);
        cafePrototypeVisuals.Add(counterTop.gameObject);
        cafePrototypeVisuals.Add(coffeeMachine.gameObject);
        cafePrototypeVisuals.Add(machineTop.gameObject);
        cafePrototypeVisuals.Add(cup.gameObject);
        cafePrototypeVisuals.Add(door.gameObject);
        cafePrototypeVisuals.Add(doorGlass.gameObject);
        cafePrototypeVisuals.Add(window.gameObject);
        cafePrototypeVisuals.Add(menuBoard.gameObject);
        cafePrototypeVisuals.Add(plantPot.gameObject);
        cafePrototypeVisuals.Add(plantLeaf.gameObject);
        cafePrototypeVisuals.Add(customerSpot.gameObject);
        cafePrototypeVisuals.Add(doorLabel.gameObject);
        cafePrototypeVisuals.Add(counterLabel.gameObject);
        cafePrototypeVisuals.Add(machineLabel.gameObject);
        cafePrototypeVisuals.Add(menuLabel.gameObject);
        cafePrototypeVisuals.Add(customerSpotLabel.gameObject);

        startingRoomVisuals.Clear();
        startingRoomVisuals.Add(cafeSceneImage.gameObject);
        startingRoomVisuals.Add(baristaLockedSceneImage.gameObject);
        startingRoomVisuals.Add(baristaSceneImage.gameObject);
        startingRoomVisuals.Add(baristaCounterForegroundImage.gameObject);
        startingRoomVisuals.Add(coffeeMachineSceneImage.gameObject);
        startingRoomVisuals.Add(coffeeMachineLockedPlaceholderImage.gameObject);

        roomShadow.raycastTarget = false;
        backWallTopTrim.raycastTarget = false;
        leftWallImage.raycastTarget = false;
        rightWallImage.raycastTarget = false;
        backTrim.raycastTarget = false;
        leftTrim.raycastTarget = false;
        rightTrim.raycastTarget = false;
        leftCornerLine.raycastTarget = false;
        rightCornerLine.raycastTarget = false;
        floorBackEdge.raycastTarget = false;
        floorCenterLine.raycastTarget = false;
        floorLeftPerspectiveLine.raycastTarget = false;
        floorRightPerspectiveLine.raycastTarget = false;
        roomAccentStrip.raycastTarget = false;
        baristaCounterForegroundImage.raycastTarget = false;
        coffeeMachineSceneImage.raycastTarget = false;
        counterTop.raycastTarget = false;
        door.raycastTarget = false;
        doorGlass.raycastTarget = false;
        window.raycastTarget = false;
        menuBoard.raycastTarget = false;
        coffeeMachine.raycastTarget = false;
        machineTop.raycastTarget = false;
        cup.raycastTarget = false;
        plantPot.raycastTarget = false;
        plantLeaf.raycastTarget = false;
        customerSpot.raycastTarget = false;
        productionConveyorVisual.raycastTarget = false;
        productionMachineVisual.raycastTarget = false;
        vipRugVisual.raycastTarget = false;
        vipCounterVisual.raycastTarget = false;
        vipPlantVisual.raycastTarget = false;
        roomLockOverlay.raycastTarget = false;
    }

    private Image CreateSceneBox(Transform parent, string objectName, Vector2 position, Vector2 size, Color color)
    {
        GameObject boxObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        boxObject.transform.SetParent(parent, false);

        RectTransform rect = boxObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image image = boxObject.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private float GetResponsiveCafeSceneScale(Transform parent)
    {
        RectTransform parentRect = parent as RectTransform;
        if (parentRect == null || parentRect.rect.width <= 0 || parentRect.rect.height <= 0)
        {
            return 0.96f;
        }

        float widthFit = (parentRect.rect.width - 90f) / 1410f;
        float heightFit = (parentRect.rect.height - 110f) / 700f;
        float scale = Mathf.Min(widthFit, heightFit, 1.38f);
        return Mathf.Clamp(scale, 1.02f, 1.38f);
    }

    private IsoQuadGraphic CreateSceneQuad(Transform parent, string objectName, Vector2 topLeft, Vector2 topRight, Vector2 bottomRight, Vector2 bottomLeft, Color color)
    {
        GameObject quadObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(IsoQuadGraphic));
        quadObject.transform.SetParent(parent, false);

        RectTransform rect = quadObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(1100, 750);

        IsoQuadGraphic quad = quadObject.GetComponent<IsoQuadGraphic>();
        quad.SetCorners(topLeft, topRight, bottomRight, bottomLeft);
        quad.color = color;
        quad.raycastTarget = false;
        return quad;
    }

    private void RotateSceneBox(Image image, float angle)
    {
        RectTransform rect = image.GetComponent<RectTransform>();
        rect.localRotation = Quaternion.Euler(0, 0, angle);
    }

    private Color ShadeColor(Color color, float multiplier)
    {
        return new Color(color.r * multiplier, color.g * multiplier, color.b * multiplier, color.a);
    }

    private TMP_Text CreateSceneLabel(Transform parent, string objectName, string text, Vector2 position, Vector2 size, float fontSize)
    {
        TMP_Text label = CreateText(parent, objectName, text, position, size, fontSize);
        label.raycastTarget = false;
        label.color = Color.black;
        return label;
    }

    private void CreateCustomerButton(Transform parent)
    {
        customerButtonText = CreateButton(parent, "CustomerButton", "Musteri\n2x tiklama + 1 XP", new Vector2(330, 105), new Vector2(165, 112), SellCoffeeToCustomer, new Color(0.18f, 0.58f, 0.32f), 18);
        customerButtonObject = customerButtonText.GetComponentInParent<Button>().gameObject;
        CreateVipBadge(customerButtonObject.transform);
        customerButtonObject.SetActive(false);
    }

    private void CreateVipBadge(Transform parent)
    {
        GameObject badge = new GameObject("VipBadge", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        badge.transform.SetParent(parent, false);

        RectTransform rect = badge.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(-92, 64);
        rect.sizeDelta = new Vector2(82, 42);

        Image image = badge.GetComponent<Image>();
        image.color = new Color(1f, 0.82f, 0.12f, 1f);

        vipBadgeText = CreateText(badge.transform, "Text", "VIP", Vector2.zero, rect.sizeDelta, 24);
        vipBadgeText.color = Color.black;
        vipBadgeObject = badge;
        vipBadgeObject.SetActive(false);
    }

    private Image CreateNotifyDot(Transform parent, Vector2 position)
    {
        GameObject dotObject = new GameObject("TaskNotifyDot", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        dotObject.transform.SetParent(parent, false);

        RectTransform rect = dotObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(42, 54);

        Image image = dotObject.GetComponent<Image>();
        image.sprite = GetNotifyExclamationSprite();
        image.color = Color.white;
        image.preserveAspect = true;
        image.raycastTarget = false;
        dotObject.SetActive(false);
        return image;
    }
    private TMP_Text CreateText(Transform parent, string objectName, string text, Vector2 position, Vector2 size, float fontSize)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        TMP_Text tmp = textObject.GetComponent<TMP_Text>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = Color.black;
        tmp.alignment = TextAlignmentOptions.Center;
        return tmp;
    }

    private TMP_Text CreateResourceBar(Transform parent, string objectName, Sprite icon, Vector2 position, Vector2 size, Color backgroundColor, Color outlineColor, float fontSize, float iconScale = 1f)
    {
        GameObject barObject = new GameObject(objectName, typeof(RectTransform));
        barObject.transform.SetParent(parent, false);

        RectTransform barRect = barObject.GetComponent<RectTransform>();
        barRect.anchorMin = new Vector2(0.5f, 0.5f);
        barRect.anchorMax = new Vector2(0.5f, 0.5f);
        barRect.anchoredPosition = position;
        barRect.sizeDelta = size;

        float backgroundOffset = size.y * 0.48f;
        Vector2 backgroundSize = new Vector2(size.x - backgroundOffset, size.y);
        GameObject backgroundObject = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        backgroundObject.transform.SetParent(barObject.transform, false);
        RectTransform backgroundRect = backgroundObject.GetComponent<RectTransform>();
        backgroundRect.anchorMin = new Vector2(0f, 0.5f);
        backgroundRect.anchorMax = new Vector2(0f, 0.5f);
        backgroundRect.pivot = new Vector2(0f, 0.5f);
        backgroundRect.anchoredPosition = new Vector2(backgroundOffset, 0f);
        backgroundRect.sizeDelta = backgroundSize;

        Image background = backgroundObject.GetComponent<Image>();
        background.sprite = CreateResourceBarBackgroundSprite(backgroundSize, backgroundColor, outlineColor);
        background.color = Color.white;
        backgroundObject.AddComponent<CurrencyBarPartSelector>().Bind(barObject);

        GameObject iconObject = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        iconObject.transform.SetParent(barObject.transform, false);
        RectTransform iconRect = iconObject.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0f, 0.5f);
        iconRect.anchorMax = new Vector2(0f, 0.5f);
        iconRect.anchoredPosition = new Vector2(33f, 0f);
        float iconSize = 80.24f * iconScale;
        iconRect.sizeDelta = new Vector2(iconSize, iconSize);
        Image iconImage = iconObject.GetComponent<Image>();
        iconImage.sprite = icon;
        iconImage.color = Color.white;
        iconImage.preserveAspect = true;
        iconObject.transform.SetAsLastSibling();
        iconObject.AddComponent<CurrencyBarPartSelector>().Bind(barObject);

        float amountLeftPadding = Mathf.Min(backgroundSize.x * 0.42f, iconSize * 0.58f);
        Vector2 amountTextSize = new Vector2(backgroundSize.x - amountLeftPadding - 8f, backgroundSize.y);
        Vector2 amountTextPosition = new Vector2(amountLeftPadding * 0.5f - 4f, 0f);
        TMP_Text amountText = CreateText(backgroundObject.transform, "AmountText", "0", amountTextPosition, amountTextSize, fontSize);
        amountText.color = Color.white;
        amountText.alignment = TextAlignmentOptions.Center;
        amountText.fontStyle = FontStyles.Bold;
        amountText.enableAutoSizing = true;
        amountText.fontSizeMin = 14;
        amountText.fontSizeMax = fontSize;
        amountText.enableWordWrapping = false;
        amountText.overflowMode = TextOverflowModes.Ellipsis;
        amountText.gameObject.AddComponent<CurrencyBarPartSelector>().Bind(barObject);
        return amountText;
    }

    private void CreateCoinInfoPopup(Transform parent, Vector2 position)
    {
        coinInfoPopup = new GameObject("CoinInfoPopup", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        coinInfoPopup.transform.SetParent(parent, false);

        RectTransform panelRect = coinInfoPopup.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = position;
        panelRect.sizeDelta = new Vector2(620, 310);

        Image panelImage = coinInfoPopup.GetComponent<Image>();
        panelImage.sprite = GetCoinInfoPanelSprite();
        panelImage.color = Color.white;
        panelImage.preserveAspect = true;

        for (int i = 0; i < coinInfoPopupRows.Length; i++)
        {
            float rowY = 90f - (i * 36f);
            TMP_Text rowText = CreateText(coinInfoPopup.transform, "CoinInfoRow" + (i + 1), "", new Vector2(0f, rowY), new Vector2(500f, 34f), 19);
            rowText.color = new Color(1f, 0.91f, 0.66f, 1f);
            rowText.fontStyle = FontStyles.Bold;
            rowText.alignment = TextAlignmentOptions.Left;
            rowText.enableAutoSizing = true;
            rowText.fontSizeMin = 14;
            rowText.fontSizeMax = 19;
            rowText.enableWordWrapping = false;
            rowText.overflowMode = TextOverflowModes.Ellipsis;
            rowText.raycastTarget = false;
            ApplyVipFont(rowText);
            coinInfoPopupRows[i] = rowText;
        }

        coinInfoPopup.transform.SetAsLastSibling();
        coinInfoPopup.SetActive(false);
    }

    private TMP_Text CreateXpBar(Transform parent, Vector2 position, Vector2 size)
    {
        GameObject barObject = new GameObject("XpBar", typeof(RectTransform));
        barObject.transform.SetParent(parent, false);

        RectTransform barRect = barObject.GetComponent<RectTransform>();
        barRect.anchorMin = new Vector2(0.5f, 0.5f);
        barRect.anchorMax = new Vector2(0.5f, 0.5f);
        barRect.anchoredPosition = position;
        barRect.sizeDelta = size;

        float backgroundOffset = size.y * 0.48f;
        Vector2 backgroundSize = new Vector2(size.x - backgroundOffset, size.y);
        GameObject backgroundObject = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        backgroundObject.transform.SetParent(barObject.transform, false);
        RectTransform backgroundRect = backgroundObject.GetComponent<RectTransform>();
        backgroundRect.anchorMin = new Vector2(0f, 0.5f);
        backgroundRect.anchorMax = new Vector2(0f, 0.5f);
        backgroundRect.pivot = new Vector2(0f, 0.5f);
        backgroundRect.anchoredPosition = new Vector2(backgroundOffset, 0f);
        backgroundRect.sizeDelta = backgroundSize;

        Image background = backgroundObject.GetComponent<Image>();
        background.sprite = CreateResourceBarBackgroundSprite(backgroundSize, new Color(0.06f, 0.10f, 0.22f, 0.93f), new Color(0.55f, 0.55f, 1f, 1f));
        background.color = Color.white;
        backgroundObject.AddComponent<CurrencyBarPartSelector>().Bind(barObject);

        GameObject fillObject = new GameObject("XpFill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        fillObject.transform.SetParent(backgroundObject.transform, false);
        RectTransform fillRect = fillObject.GetComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0f, 0.5f);
        fillRect.anchorMax = new Vector2(0f, 0.5f);
        fillRect.pivot = new Vector2(0f, 0.5f);
        fillRect.anchoredPosition = new Vector2(4f, 0f);
        fillRect.sizeDelta = new Vector2(0f, size.y - 8f);

        xpFillImage = fillObject.GetComponent<Image>();
        xpFillImage.sprite = CreateResourceBarBackgroundSprite(new Vector2(backgroundSize.x - 8f, size.y - 8f), new Color(0.45f, 0.42f, 1f, 0.95f), new Color(0.45f, 0.42f, 1f, 0f));
        xpFillImage.color = Color.white;
        fillObject.AddComponent<CurrencyBarPartSelector>().Bind(barObject);

        TMP_Text text = CreateText(backgroundObject.transform, "XpText", "", Vector2.zero, backgroundSize, 18);
        text.color = Color.white;
        text.fontStyle = FontStyles.Bold;
        text.gameObject.SetActive(false);
        text.gameObject.AddComponent<CurrencyBarPartSelector>().Bind(barObject);

        GameObject badgeObject = new GameObject("LevelBadge", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        badgeObject.transform.SetParent(barObject.transform, false);
        RectTransform badgeRect = badgeObject.GetComponent<RectTransform>();
        badgeRect.anchorMin = new Vector2(0f, 0.5f);
        badgeRect.anchorMax = new Vector2(0f, 0.5f);
        badgeRect.anchoredPosition = new Vector2(33f, 0f);
        badgeRect.sizeDelta = new Vector2(80f, 80f);

        Image badgeImage = badgeObject.GetComponent<Image>();
        badgeImage.sprite = GetLevelBadgeSprite();
        badgeImage.color = Color.white;
        badgeImage.preserveAspect = true;
        badgeObject.transform.SetAsLastSibling();
        badgeObject.AddComponent<CurrencyBarPartSelector>().Bind(barObject);

        levelBadgeText = CreateText(badgeObject.transform, "LevelText", "1", new Vector2(0f, -3f), new Vector2(52f, 34f), 28);
        levelBadgeText.color = Color.white;
        levelBadgeText.fontStyle = FontStyles.Bold;
        levelBadgeText.enableAutoSizing = true;
        levelBadgeText.fontSizeMin = 18;
        levelBadgeText.fontSizeMax = 28;
        levelBadgeText.outlineColor = new Color(0.08f, 0.07f, 0.16f, 0.95f);
        levelBadgeText.outlineWidth = 0.08f;
        levelBadgeText.gameObject.AddComponent<CurrencyBarPartSelector>().Bind(barObject);

        return text;
    }
    private Sprite CreateResourceBarBackgroundSprite(Vector2 size, Color fillColor, Color borderColor)
    {
        int width = Mathf.Max(32, Mathf.RoundToInt(size.x));
        int height = Mathf.Max(24, Mathf.RoundToInt(size.y));
        int border = 4;
        int radius = height / 2;
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        Color clear = new Color(0f, 0f, 0f, 0f);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color pixel = clear;
                if (IsInsideRoundedBarPixel(x, y, width, height, radius))
                {
                    pixel = borderColor;
                }

                if (IsInsideRoundedBarPixel(x, y, width - border * 2, height - border * 2, Mathf.Max(0, radius - border), border, border))
                {
                    pixel = fillColor;
                }

                texture.SetPixel(x, y, pixel);
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
    }

    private bool IsInsideRoundedBarPixel(int x, int y, int width, int height, int radius, int offsetX = 0, int offsetY = 0)
    {
        int localX = x - offsetX;
        int localY = y - offsetY;
        if (localX < 0 || localY < 0 || localX >= width || localY >= height)
        {
            return false;
        }

        int left = radius;
        int right = width - radius - 1;
        if (localX >= left && localX <= right)
        {
            return true;
        }

        int centerX = localX < left ? left : right;
        int centerY = height / 2;
        int dx = localX - centerX;
        int dy = localY - centerY;
        return dx * dx + dy * dy <= radius * radius;
    }

    private void MakeTextClickable(TMP_Text text, UnityEngine.Events.UnityAction action)
    {
        if (text == null)
        {
            return;
        }

        text.raycastTarget = true;
        Button button = text.GetComponent<Button>();
        if (button == null)
        {
            button = text.gameObject.AddComponent<Button>();
        }

        button.targetGraphic = text;
        button.onClick.AddListener(action);
    }

    private void MakeResourceBarClickable(TMP_Text barText, UnityEngine.Events.UnityAction action)
    {
        if (barText == null)
        {
            return;
        }

        Transform barTransform = barText.transform.parent != null ? barText.transform.parent.parent : null;
        if (barTransform == null)
        {
            return;
        }

        GameObject hitArea = new GameObject("TapArea", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        hitArea.transform.SetParent(barTransform, false);
        RectTransform hitRect = hitArea.GetComponent<RectTransform>();
        hitRect.anchorMin = new Vector2(0.5f, 0.5f);
        hitRect.anchorMax = new Vector2(0.5f, 0.5f);
        hitRect.anchoredPosition = Vector2.zero;
        hitRect.sizeDelta = barTransform.GetComponent<RectTransform>() != null ? barTransform.GetComponent<RectTransform>().sizeDelta : new Vector2(220, 52);

        Image hitImage = hitArea.GetComponent<Image>();
        hitImage.color = new Color(1f, 1f, 1f, 0.001f);

        Button button = hitArea.GetComponent<Button>();
        button.targetGraphic = hitImage;
        button.onClick.AddListener(action);
        hitArea.transform.SetAsLastSibling();
    }

    private TMP_Text CreateTransparentButton(Transform parent, string objectName, string label, Vector2 position, Vector2 size, UnityEngine.Events.UnityAction action, float fontSize, Color textColor)
    {
        GameObject buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.001f);

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(action);

        TMP_Text text = CreateText(buttonObject.transform, "Text", label, Vector2.zero, size, fontSize);
        text.color = textColor;
        text.fontStyle = FontStyles.Bold;
        text.raycastTarget = false;
        text.enableAutoSizing = true;
        text.fontSizeMin = Mathf.Max(12, fontSize - 8);
        text.fontSizeMax = fontSize;
        return text;
    }

    private Button CreateInvisibleButton(Transform parent, string objectName, Vector2 position, Vector2 size, UnityEngine.Events.UnityAction action)
    {
        GameObject buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.001f);

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(action);
        return button;
    }

    private TMP_Text CreateButton(Transform parent, string objectName, string label, Vector2 position, Vector2 size, UnityEngine.Events.UnityAction action, Color color, float fontSize)
    {
        GameObject buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image image = buttonObject.GetComponent<Image>();
        image.color = color;

        Button button = buttonObject.GetComponent<Button>();
        button.onClick.AddListener(action);

        TMP_Text labelText = CreateText(buttonObject.transform, "Text", label, Vector2.zero, size, fontSize);
        labelText.color = Color.white;
        return labelText;
    }

    private string FormatResourceAmount(int amount)
    {
        if (amount >= 1000000000)
        {
            return (amount / 1000000000f).ToString("0.#") + "B";
        }

        if (amount >= 1000000)
        {
            return (amount / 1000000f).ToString("0.#") + "M";
        }

        if (amount >= 10000)
        {
            return (amount / 1000f).ToString("0.#") + "K";
        }

        return amount.ToString();
    }

    private static void SetVipLockedTextSize(TMP_Text text, bool locked, float normalMaxSize)
    {
        text.fontSizeMin = locked ? 15f : Mathf.Max(10f, normalMaxSize - 5f);
        text.fontSizeMax = locked ? 19f : normalMaxSize;
    }

    private void UpdateUI()
    {
        if (coinText != null)
            coinText.text = FormatResourceAmount(Mathf.FloorToInt(coins));

        if (coinInfoPopup != null && coinInfoPopup.activeSelf)
        {
            string[] coinInfoRows = GetCoinInfoPopupRows();
            for (int i = 0; i < coinInfoPopupRows.Length; i++)
            {
                if (coinInfoPopupRows[i] != null)
                    coinInfoPopupRows[i].text = i < coinInfoRows.Length ? coinInfoRows[i] : "";
            }
        }

        if (passiveIncomePanelText != null)
            passiveIncomePanelText.text = GetPassiveIncomeBreakdownText();

        if (activeBoostText != null)
        {
            activeBoostText.text = "";
            activeBoostText.gameObject.SetActive(false);
            activeBoostText.color = new Color(0.02f, 0.18f, 0.82f);
        }

        if (gemText != null)
            gemText.text = FormatResourceAmount(gems);

        if (vipHudText != null)
            vipHudText.text = "";

        if (vipHudLevelText != null)
        {
            bool showVipLevel = vipLevel > 0;
            vipHudLevelText.gameObject.SetActive(showVipLevel);
            if (showVipLevel)
                vipHudLevelText.text = vipLevel.ToString();
        }

        if (vipNotifyDot != null)
        {
            bool shouldShowVipDot = CanUpgradeVipLevel();
            vipNotifyDot.gameObject.SetActive(shouldShowVipDot);
            if (shouldShowVipDot)
            {
                Color dotColor = Color.white;
                dotColor.a = 0.72f + (Mathf.PingPong(Time.time * 3f, 1f) * 0.28f);
                vipNotifyDot.color = dotColor;
            }
        }

        if (chestText != null)
            chestText.gameObject.SetActive(false);

        if (xpText != null)
        {
            int xpNeeded = GetXpNeededForNextLevel();
            xpText.text = "";
            if (levelBadgeText != null)
                levelBadgeText.text = playerLevel.ToString();

            if (xpFillImage != null)
            {
                RectTransform fillRect = xpFillImage.GetComponent<RectTransform>();
                if (fillRect != null)
                {
                    float ratio = xpNeeded > 0 ? Mathf.Clamp01((float)xp / xpNeeded) : 0f;
                    RectTransform parentRect = xpFillImage.transform.parent.GetComponent<RectTransform>();
                    float maxFillWidth = parentRect != null ? Mathf.Max(0f, parentRect.sizeDelta.x - 8f) : 0f;
                    fillRect.sizeDelta = new Vector2(maxFillWidth * ratio, fillRect.sizeDelta.y);
                }
            }
        }

        if (roomNameText != null)
            roomNameText.gameObject.SetActive(false);

        if (roomUnlockButtonText != null)
        {
            Button roomButton = roomUnlockButtonText.GetComponentInParent<Button>();
            bool roomUnlocked = IsCurrentRoomUnlocked();
            int unlockCost = GetCurrentRoomUnlockCost();
            roomUnlockButtonText.text = roomUnlocked ? "" : "Odayi Ac: " + unlockCost + " coin";
            if (roomButton != null)
            {
            roomButton.gameObject.SetActive(!roomUnlocked);
            roomButton.interactable = !roomUnlocked && coins >= unlockCost;
        }
        }

        if (customerStatsText != null)
            customerStatsText.text = "Satis: " + totalCoffeeSold + " | Musteri: " + customerSales + " | Kacan: " + missedCustomers;

        if (taskPanelText != null)
        {
            taskPanelText.text = GetTaskText();
        }

        if (taskRewardButtonText != null)
        {
            Button rewardButton = taskRewardButtonText.GetComponentInParent<Button>();
            bool canClaim = CanClaimCurrentTask();
            bool hasTask = HasVisibleTask();
            taskRewardButtonText.text = hasTask ? "Ödülü Al" : "Görev Yok";
            if (rewardButton != null)
                rewardButton.interactable = canClaim;
        }

        if (taskNotifyDot != null)
        {
            bool shouldShowDot = ShouldShowTaskNotifyDot();
            taskNotifyDot.gameObject.SetActive(shouldShowDot);
            if (shouldShowDot)
            {
                Color dotColor = Color.white;
                dotColor.a = 0.35f + (Mathf.PingPong(Time.time * 2.5f, 1f) * 0.65f);
                taskNotifyDot.color = dotColor;
            }
        }

        if (dailyTasksNotifyDot != null)
        {
            bool shouldShowDailyDot = CanClaimAnyDailyTask();
            dailyTasksNotifyDot.gameObject.SetActive(shouldShowDailyDot);
            if (shouldShowDailyDot)
            {
                Color dotColor = Color.white;
                dotColor.a = 0.35f + (Mathf.PingPong(Time.time * 2.5f, 1f) * 0.65f);
                dailyTasksNotifyDot.color = dotColor;
            }
        }

        if (adBoostButtonText != null)
        {
            if (bonusTimer >= MaxBoostSeconds)
                adBoostButtonText.text = "MAX";
            else if (bonusTimer > 0)
                adBoostButtonText.text = "AKTIF";
            else
                adBoostButtonText.text = "AD";
            Button adButton = adBoostButtonText.GetComponentInParent<Button>();
            if (adButton != null)
                adButton.interactable = bonusTimer < MaxBoostSeconds;
        }

        if (offerButtonObject != null)
        {
            offerButtonObject.SetActive(offerVisibleTimer > 0);
            if (offerVisibleTimer > 0)
            {
                offerButtonObject.transform.SetAsLastSibling();
            }
        }

        if (offerButtonText != null)
            offerButtonText.text = "2x\n" + Mathf.CeilToInt(offerVisibleTimer) + " sn";

        if (offerPanelText != null)
            offerPanelText.text = "5 dakikaligina\n2x coin kazan";

        if (customerButtonObject != null)
        {
            bool showCustomerInCurrentRoom = currentRoomIndex == 0 && customerVisibleTimer > 0;
            customerButtonObject.SetActive(showCustomerInCurrentRoom);
            bool panelIsOpen = (offlinePanel != null && offlinePanel.activeSelf)
                || (taskPanel != null && taskPanel.activeSelf)
                || (coffeePanel != null && coffeePanel.activeSelf)
                || (machinePanel != null && machinePanel.activeSelf)
                || (baristaPanel != null && baristaPanel.activeSelf)
                || (passiveIncomePanel != null && passiveIncomePanel.activeSelf)
                || (foodPanel != null && foodPanel.activeSelf)
                || (dailyRewardPanel != null && dailyRewardPanel.activeSelf)
                || (chestPanel != null && chestPanel.activeSelf)
                || (decorPanel != null && decorPanel.activeSelf)
                || (testPanel != null && testPanel.activeSelf)
                || (vipPanel != null && vipPanel.activeSelf)
                || (vipInfoPanel != null && vipInfoPanel.activeSelf)
                || (dailyTasksPanel != null && dailyTasksPanel.activeSelf)
                || (offerPanel != null && offerPanel.activeSelf)
                || (adBoostConfirmPanel != null && adBoostConfirmPanel.activeSelf);
            if (showCustomerInCurrentRoom && !panelIsOpen)
            {
                customerButtonObject.transform.SetAsLastSibling();
            }
        }

        if (customerButtonText != null)
        {
            int shownXpReward = Mathf.CeilToInt(currentCustomerXpReward * GetVipXpMultiplier());
            customerButtonText.text = currentCustomerName + "\n+" + GetCurrentCustomerCoinReward() + " coin +" + shownXpReward + " XP\n" + Mathf.CeilToInt(customerVisibleTimer) + " sn";
            Image customerImage = customerButtonText.GetComponentInParent<Image>();
            if (customerImage != null)
                customerImage.color = currentCustomerColor;
        }

        if (vipBadgeObject != null)
        {
            bool showVipBadge = currentRoomIndex == 0 && customerVisibleTimer > 0 && currentCustomerName == "VIP Müşteri";
            vipBadgeObject.SetActive(showVipBadge);
            if (showVipBadge)
            {
                Image badgeImage = vipBadgeObject.GetComponent<Image>();
                if (badgeImage != null)
                {
                    Color badgeColor = new Color(1f, 0.82f, 0.12f, 1f);
                    badgeColor.a = 0.55f + (Mathf.PingPong(Time.time * 3f, 1f) * 0.45f);
                    badgeImage.color = badgeColor;
                }
            }
        }

        if (offlinePanel != null)
        {
            offlinePanel.SetActive(lastOfflineCoins > 0);
            offlinePanel.transform.SetAsLastSibling();
        }

        if (offlineAmountText != null)
            offlineAmountText.text = "+" + FormatResourceAmount(lastOfflineCoins) + " COIN";

        if (offlineText != null)
            offlineText.text = "Sen yokken baristan çalıştı";

        if (dailyRewardPanelText != null)
            dailyRewardPanelText.text = GetDailyRewardText();

        RefreshDailyRewardLiveOverlay();

        if (dailyRewardButtonText != null)
        {
            Button dailyButton = dailyRewardButtonText.GetComponentInParent<Button>();
            bool canClaimDaily = CanClaimDailyReward();
            dailyRewardButtonText.text = canClaimDaily ? "Ödülü Al" : "";
            if (dailyButton != null)
                dailyButton.interactable = canClaimDaily;
        }

        if (dailyRewardPanel != null && dailyRewardPanel.activeSelf && lastOfflineCoins <= 0)
        {
            if (dailyRewardDimmer != null)
            {
                dailyRewardDimmer.SetActive(true);
                dailyRewardDimmer.transform.SetAsLastSibling();
            }
            dailyRewardPanel.transform.SetAsLastSibling();
        }
        else if (dailyRewardDimmer != null && (dailyRewardPanel == null || !dailyRewardPanel.activeSelf))
        {
            dailyRewardDimmer.SetActive(false);
        }

        if (chestPanelText != null)
        {
            chestPanelText.text = "Siradan Sandik: " + chests
                + "\nNadir Sandik: " + rareChests
                + "\nOzel Sandik: " + luxuryChests
                + "\nTahta: " + woodPieces
                + GetPaintInventorySummary();
        }
        if (commonChestCountText != null) commonChestCountText.text = "SIRADAN  ×" + chests;
        if (rareChestCountText != null) rareChestCountText.text = "NADIR  ×" + rareChests;
        if (luxuryChestCountText != null) luxuryChestCountText.text = "ÖZEL  ×" + luxuryChests;

        if (chestOpenButtonText != null)
        {
            Button chestButton = chestOpenButtonText.GetComponentInParent<Button>();
            chestOpenButtonText.text = chests > 0 ? "SIRADAN AÇ" : "SANDIK YOK";
            if (chestButton != null)
                chestButton.interactable = chests > 0;
        }

        if (rareChestOpenButtonText != null)
        {
            Button rareButton = rareChestOpenButtonText.GetComponentInParent<Button>();
            rareChestOpenButtonText.text = rareChests > 0 ? "NADIR AÇ" : "SANDIK YOK";
            if (rareButton != null)
                rareButton.interactable = rareChests > 0;
        }

        if (luxuryChestOpenButtonText != null)
        {
            Button luxuryButton = luxuryChestOpenButtonText.GetComponentInParent<Button>();
            luxuryChestOpenButtonText.text = luxuryChests > 0 ? "ÖZEL AÇ" : "SANDIK YOK";
            if (luxuryButton != null)
                luxuryButton.interactable = luxuryChests > 0;
        }

        if (chestPanel != null && chestPanel.activeSelf && lastOfflineCoins <= 0)
            chestPanel.transform.SetAsLastSibling();

        if (chestRewardPanel != null && chestRewardPanel.activeSelf && lastOfflineCoins <= 0)
            chestRewardPanel.transform.SetAsLastSibling();

        if (passiveIncomePanel != null && passiveIncomePanel.activeSelf && lastOfflineCoins <= 0)
            passiveIncomePanel.transform.SetAsLastSibling();

        if (vipPanelText != null)
            vipPanelText.text = GetVipPanelText();

        if (vipLevelSummaryText != null)
            vipLevelSummaryText.gameObject.SetActive(false);

        int shownVipLevel = selectedVipPreviewLevel > 0
            ? Mathf.Clamp(selectedVipPreviewLevel, 1, MaxVipLevel)
            : Mathf.Clamp(vipLevel, 0, MaxVipLevel);
        string shownVipName = shownVipLevel > 0 ? GetVipTierName(shownVipLevel) : "VIP Yok";

        if (vipProfessionalTierTitleText != null)
        {
            vipProfessionalTierTitleText.text = shownVipLevel > 0 ? shownVipName.ToUpperInvariant() : "VIP YOK";
            Color tierColor = shownVipLevel > 0 ? GetVipTierColor(shownVipLevel) : new Color(0.46f, 0.27f, 0.10f, 1f);
            tierColor.a = 1f;
            vipProfessionalTierTitleText.color = tierColor;
        }

        if (vipProfessionalStatusText != null)
        {
            bool activeTier = shownVipLevel > 0 && shownVipLevel == vipLevel;
            bool openTier = shownVipLevel > 0 && shownVipLevel < vipLevel;
            vipProfessionalStatusText.text = activeTier ? "AKTİF" : openTier ? "AÇIK" : "KİLİTLİ";
            vipProfessionalStatusText.color = activeTier
                ? new Color(0.91f, 1f, 0.63f, 1f)
                : openTier
                    ? new Color(1f, 0.91f, 0.58f, 1f)
                    : Color.white;
        }

        bool vipAtMaximum = vipLevel >= MaxVipLevel;
        int nextVipLevel = GetVipPurchaseTargetLevel();
        if (vipProfessionalUpgradeText != null)
            vipProfessionalUpgradeText.text = vipAtMaximum ? "EN YÜKSEK VIP" : GetVipTierName(nextVipLevel).ToUpperInvariant() + "'E GEÇ";

        if (vipProfessionalUpgradeCostText != null)
            vipProfessionalUpgradeCostText.text = vipAtMaximum ? "MAX" : GetVipPurchaseCost().ToString();

        if (vipProfessionalUpgradeGemImage != null)
            vipProfessionalUpgradeGemImage.gameObject.SetActive(!vipAtMaximum);

        if (vipProfessionalUpgradeButton != null)
            vipProfessionalUpgradeButton.interactable = !vipAtMaximum && gems >= GetVipPurchaseCost();

        if (vipProfessionalClaimButton != null)
            vipProfessionalClaimButton.interactable = CanClaimVipDailyReward();

        bool shownVipIsLocked = shownVipLevel <= 0;

        if (vipCoinBonusText != null)
        {
            vipCoinBonusText.text = shownVipIsLocked ? "KİLİTLİ" : shownVipName + "\nCoin " + GetVipBonusValueText(GetVipCoinBonusPercent(shownVipLevel));
            SetVipLockedTextSize(vipCoinBonusText, shownVipIsLocked, 16f);
        }

        if (vipXpBonusText != null)
        {
            vipXpBonusText.text = shownVipIsLocked ? "KİLİTLİ" : shownVipName + "\nXP " + GetVipBonusValueText(GetVipXpBonusPercent(shownVipLevel));
            SetVipLockedTextSize(vipXpBonusText, shownVipIsLocked, 16f);
        }

        if (vipCoffeeBonusText != null)
        {
            vipCoffeeBonusText.text = shownVipIsLocked ? "KİLİTLİ" : shownVipName + "\nMüşteri " + GetVipBonusValueText(GetVipPatienceBonusPercent(shownVipLevel));
            SetVipLockedTextSize(vipCoffeeBonusText, shownVipIsLocked, 16f);
        }

        if (vipPassiveBonusText != null)
        {
            vipPassiveBonusText.text = shownVipIsLocked ? "KİLİTLİ" : shownVipName + "\nPasif Gelir " + GetVipBonusValueText(GetVipPassiveBonusPercent(shownVipLevel));
            SetVipLockedTextSize(vipPassiveBonusText, shownVipIsLocked, 16f);
        }

        if (vipDecorBonusText != null)
        {
            vipDecorBonusText.text = shownVipIsLocked ? "KİLİTLİ" : shownVipName + "\nİndirim " + GetVipDiscountBonusText(shownVipLevel);
            SetVipLockedTextSize(vipDecorBonusText, shownVipIsLocked, 16f);
        }

        if (vipLeftCoinPillText != null)
        {
            vipLeftCoinPillText.text = shownVipIsLocked ? "KİLİTLİ" : "COIN +%" + GetVipCoinBonusPercent(shownVipLevel);
            SetVipLockedTextSize(vipLeftCoinPillText, shownVipIsLocked, 18f);
            vipLeftCoinPillText.rectTransform.anchoredPosition = shownVipIsLocked
                ? new Vector2(-74.2052f, -12.75f)
                : new Vector2(-75.7052f, -11.75f);
        }

        if (vipLeftXpPillText != null)
        {
            vipLeftXpPillText.text = shownVipIsLocked ? "KİLİTLİ" : "XP +%" + GetVipXpBonusPercent(shownVipLevel);
            SetVipLockedTextSize(vipLeftXpPillText, shownVipIsLocked, 18f);
            vipLeftXpPillText.rectTransform.anchoredPosition = shownVipIsLocked
                ? new Vector2(120.5f, -12.75f)
                : new Vector2(119f, -11.75f);
        }

        if (vipLeftCustomerPillText != null)
        {
            vipLeftCustomerPillText.text = shownVipIsLocked ? "KİLİTLİ" : "MÜŞTERİ +%" + GetVipPatienceBonusPercent(shownVipLevel);
            SetVipLockedTextSize(vipLeftCustomerPillText, shownVipIsLocked, 17f);
            vipLeftCustomerPillText.rectTransform.anchoredPosition = shownVipIsLocked
                ? new Vector2(-74.205f, -74.8f)
                : new Vector2(-75.705f, -73.8f);
        }

        if (vipLeftPassivePillText != null)
        {
            vipLeftPassivePillText.text = shownVipIsLocked ? "KİLİTLİ" : "PASİF +%" + GetVipPassiveBonusPercent(shownVipLevel);
            SetVipLockedTextSize(vipLeftPassivePillText, shownVipIsLocked, 17f);
            vipLeftPassivePillText.rectTransform.anchoredPosition = shownVipIsLocked
                ? new Vector2(128.5f, -74.80001f)
                : new Vector2(127f, -73.80001f);
        }

        if (vipLeftClaimButtonText != null)
            vipLeftClaimButtonText.text = vipLevel <= 0 ? "KİLİTLİ" : CanClaimVipDailyReward() ? "AL" : "BEKLE";

        if (vipUpgradeButtonText != null)
        {
            Button vipButton = vipUpgradeButtonText.GetComponentInParent<Button>();
            bool maxed = vipLevel >= MaxVipLevel;
            int cost = GetVipPurchaseCost();
            vipUpgradeButtonText.text = maxed ? "VIP MAX" : "AÇ: " + cost;
            if (vipUpgradeGemIconImage != null)
                vipUpgradeGemIconImage.gameObject.SetActive(!maxed);
            if (vipButton != null)
                vipButton.interactable = !maxed && gems >= cost;
        }

        for (int i = 0; i < MaxVipLevel; i++)
        {
            int cardLevel = i + 1;
            bool cardLocked = vipLevel < cardLevel;
            bool cardActive = vipLevel == cardLevel;
            bool useDarkCardText = cardLevel == 2 || cardLevel == 3 || cardLevel == 5;
            Color readableCardText = useDarkCardText
                ? new Color(0.20f, 0.09f, 0.025f, 1f)
                : new Color(1f, 0.95f, 0.72f, 1f);
            Color readableCardOutline = useDarkCardText
                ? new Color(1f, 0.84f, 0.45f, 0.78f)
                : new Color(0.16f, 0.055f, 0.015f, 0.96f);
            if (i < vipLevelCardTexts.Count)
            {
                vipLevelCardTexts[i].text = cardLocked ? "KİLİTLİ" : cardActive ? "AKTİF" : "AÇIK";
                vipLevelCardTexts[i].color = readableCardText;
                vipLevelCardTexts[i].outlineColor = readableCardOutline;
                vipLevelCardTexts[i].outlineWidth = 0.12f;
                vipLevelCardTexts[i].fontSizeMin = cardLocked ? 14f : 11f;
                vipLevelCardTexts[i].fontSizeMax = cardLocked ? 19f : 16f;
            }

            if (i < vipLevelCardTitleTexts.Count)
            {
                vipLevelCardTitleTexts[i].text = GetVipTierName(cardLevel);
                vipLevelCardTitleTexts[i].color = readableCardText;
                vipLevelCardTitleTexts[i].outlineColor = readableCardOutline;
                vipLevelCardTitleTexts[i].outlineWidth = 0.12f;
            }

            if (i < vipLevelCardBonusTexts.Count)
            {
                vipLevelCardBonusTexts[i].text = GetVipLevelBonusLine(cardLevel);
                vipLevelCardBonusTexts[i].color = readableCardText;
                vipLevelCardBonusTexts[i].outlineColor = readableCardOutline;
                vipLevelCardBonusTexts[i].outlineWidth = 0.09f;
            }

            if (i < vipLevelCardImages.Count)
            {
                Color cardColor;
                if (vipLevel < cardLevel)
                {
                    cardColor = new Color(0.70f, 0.70f, 0.70f, 0.96f);
                }
                else if (vipLevel == cardLevel)
                {
                    cardColor = new Color(0.90f, 0.90f, 0.90f, 1f);
                }
                else
                {
                    cardColor = new Color(0.86f, 0.86f, 0.86f, 1f);
                }

                vipLevelCardImages[i].color = cardColor;
            }

            if (i < vipLevelLockImages.Count)
            {
                bool locked = vipLevel < cardLevel;
                vipLevelLockImages[i].gameObject.SetActive(locked);
                vipLevelLockImages[i].color = new Color(1f, 1f, 1f, locked ? 0.62f : 0f);
            }

            if (i < vipLevelSelectImages.Count)
            {
                bool selected = selectedVipPreviewLevel == cardLevel;
                bool unlockFlash = vipUnlockFlashLevel == cardLevel && Time.time < vipUnlockFlashEndTime;
                float alpha = selected ? 0.46f + (Mathf.PingPong(Time.time * 2.4f, 1f) * 0.26f) : 0f;
                if (unlockFlash)
                    alpha = Mathf.Max(alpha, 0.62f + (Mathf.PingPong(Time.time * 7.5f, 1f) * 0.30f));
                vipLevelSelectImages[i].color = new Color(1f, 0.78f, 0.18f, alpha);
            }

            if (i < vipLevelButtonRects.Count)
            {
                bool selected = selectedVipPreviewLevel == cardLevel;
                bool unlockFlash = vipUnlockFlashLevel == cardLevel && Time.time < vipUnlockFlashEndTime;
                Vector2 basePosition = new Vector2(-92f + (i * 151f), 18f);
                Vector2 targetPosition = selected ? basePosition + new Vector2(0f, -8f) : basePosition;
                Vector3 targetScale = selected ? new Vector3(0.94f, 0.94f, 1f) : Vector3.one;
                if (unlockFlash && !selected)
                    targetScale = Vector3.one * (1f + Mathf.Sin(Time.time * 22f) * 0.018f);
                float t = Mathf.Clamp01(Time.deltaTime * 12f);
                vipLevelButtonRects[i].anchoredPosition = Vector2.Lerp(vipLevelButtonRects[i].anchoredPosition, targetPosition, t);
                vipLevelButtonRects[i].localScale = Vector3.Lerp(vipLevelButtonRects[i].localScale, targetScale, t);

                if (i < vipLevelBottomStarRects.Count)
                {
                    RectTransform starRect = vipLevelBottomStarRects[i];
                    starRect.anchoredPosition = Vector2.Lerp(starRect.anchoredPosition, targetPosition + new Vector2(0f, -107f), t);
                    starRect.localScale = Vector3.Lerp(starRect.localScale, targetScale, t);
                    starRect.SetAsLastSibling();
                }
            }
        }

        if (vipPanel != null && vipPanel.activeSelf && lastOfflineCoins <= 0)
            vipPanel.transform.SetAsLastSibling();

        if (vipInfoPanel != null && vipInfoPanel.activeSelf && lastOfflineCoins <= 0)
            vipInfoPanel.transform.SetAsLastSibling();

        if (foodPanel != null && foodPanel.activeSelf && lastOfflineCoins <= 0)
            foodPanel.transform.SetAsLastSibling();

        UpdateFoodUpgradePanelUi();

        if (decorPanelText != null)
        {
            DecorItemData previewItem = GetPreviewDecorItem();
            bool previewUnlocked = IsPreviewDecorUnlocked();
            string statusText = previewUnlocked ? "Acik" : "Kilitli";
            decorPanelText.text = GetPreviewDecorName()
                + "\n" + GetPreviewDecorTypeText() + " | " + statusText
                + "\n" + (previewUnlocked ? "Sahip olundu" : "Uretim icin kaynak gerekiyor");

            UpdateDecorTabs(previewItem);
            UpdateDecorResourceRow(previewItem, previewUnlocked);
        }

        UpdateDecorButton(pinkWallButtonText, GetDecorItemData(1));
        UpdateDecorButton(mintWallButtonText, GetDecorItemData(2));
        UpdateDecorButton(creamFloorButtonText, GetDecorItemData(3));
        UpdateDecorButton(roseTableButtonText, GetDecorItemData(4));

        if (decorActionButtonText != null && decorPanelText != null)
        {
            Button actionButton = decorActionButtonText.GetComponentInParent<Button>();
            bool unlocked = IsPreviewDecorUnlocked();
            bool canUnlock = CanUnlockPreviewDecor();
            if (unlocked)
            {
                decorActionButtonText.text = "Kullan";
            }
            else
            {
                decorActionButtonText.text = "Uret";
            }

            if (actionButton != null)
            {
                actionButton.interactable = unlocked || canUnlock;
                Image actionImage = actionButton.GetComponent<Image>();
                if (actionImage != null)
                {
                    if (unlocked)
                    {
                        actionImage.color = new Color(0.25f, 0.56f, 0.34f, 1f);
                    }
                    else if (canUnlock)
                    {
                        actionImage.color = new Color(0.82f, 0.55f, 0.18f, 1f);
                    }
                    else
                    {
                        actionImage.color = new Color(0.28f, 0.26f, 0.24f, 1f);
                    }
                }
            }
        }

        if (decorPanel != null && decorPanel.activeSelf && workshopMaterialCountTexts.Count > 0)
            RefreshWorkshopPanel();

        if (decorPanel != null && decorPanel.activeSelf && lastOfflineCoins <= 0)
            decorPanel.transform.SetAsLastSibling();

        if (testPanel != null && testPanel.activeSelf && lastOfflineCoins <= 0)
            testPanel.transform.SetAsLastSibling();

        if (dailyTasksPanelText != null)
            dailyTasksPanelText.text = GetDailyTasksText();

        UpdateDailyTaskButton(dailyTaskCoffeeButtonText, dailyCoffeeRewardClaimed, dailyCoffeeSold >= 20, "20 kahve sat", dailyCoffeeSold, 20, "Ödül: 1 Elmas");
        UpdateDailyTaskButton(dailyTaskCustomerButtonText, dailyCustomerRewardClaimed, dailyCustomerSales >= 5, "5 müşteriye satış yap", dailyCustomerSales, 5, "Ödül: 1 Sandık");
        UpdateDailyTaskButton(dailyTaskChestButtonText, dailyChestRewardClaimed, dailyChestsOpened >= 1, "1 sandık aç", dailyChestsOpened, 1, "Ödül: 1 Boya Kutusu");

        if (dailyTasksPanel != null && dailyTasksPanel.activeSelf && lastOfflineCoins <= 0)
            dailyTasksPanel.transform.SetAsLastSibling();

        ApplyDecorations();

        if (coffeeUpgradeButtonText != null)
            coffeeUpgradeButtonText.text = "Kah";

        if (basicCoffeeButtonText != null)
        {
            bool maxed = coffeeUpgradeLevel >= UpgradeMaxLevel;
            int shownCost = GetDiscountedUpgradeCost(coffeeUpgradeCost);
            basicCoffeeButtonText.text = maxed
                ? "Daha Iyi Kahve Lv." + coffeeUpgradeLevel + "/" + UpgradeMaxLevel + "\nMAX"
                : "Daha Iyi Kahve Lv." + coffeeUpgradeLevel + "/" + UpgradeMaxLevel + "\n+" + BasicCoffeeTapGain + " tiklama | " + shownCost + " coin";
            Button button = basicCoffeeButtonText.GetComponentInParent<Button>();
            if (button != null)
                button.interactable = !maxed && coins >= shownCost;
        }

        if (premiumCoffeeButtonText != null)
        {
            bool unlocked = playerLevel >= PremiumTierUnlockLevel && coffeeUpgradeLevel >= NextTierRequiredLevel;
            bool maxed = premiumCoffeeLevel >= UpgradeMaxLevel;
            int shownCost = GetDiscountedUpgradeCost(premiumCoffeeCost);
            premiumCoffeeButtonText.text = !unlocked
                ? "Premium Cekirdek\nKilitli: Lv." + PremiumTierUnlockLevel + " + onceki Lv." + NextTierRequiredLevel
                : maxed
                    ? "Premium Cekirdek Lv." + premiumCoffeeLevel + "/" + UpgradeMaxLevel + "\nMAX"
                    : "Premium Cekirdek Lv." + premiumCoffeeLevel + "/" + UpgradeMaxLevel + "\n+" + PremiumCoffeeTapGain + " tiklama | " + shownCost + " coin";
            Button button = premiumCoffeeButtonText.GetComponentInParent<Button>();
            if (button != null)
                button.interactable = unlocked && !maxed && coins >= shownCost;
        }

        if (signatureCoffeeButtonText != null)
        {
            bool unlocked = playerLevel >= SignatureTierUnlockLevel && premiumCoffeeLevel >= NextTierRequiredLevel;
            bool maxed = signatureCoffeeLevel >= UpgradeMaxLevel;
            int shownCost = GetDiscountedUpgradeCost(signatureCoffeeCost);
            signatureCoffeeButtonText.text = !unlocked
                ? "Ozel Harman\nKilitli: Lv." + SignatureTierUnlockLevel + " + onceki Lv." + NextTierRequiredLevel
                : maxed
                    ? "Ozel Harman Lv." + signatureCoffeeLevel + "/" + UpgradeMaxLevel + "\nMAX"
                    : "Ozel Harman Lv." + signatureCoffeeLevel + "/" + UpgradeMaxLevel + "\n+" + SignatureCoffeeTapGain + " tiklama | " + shownCost + " coin";
            Button button = signatureCoffeeButtonText.GetComponentInParent<Button>();
            if (button != null)
                button.interactable = unlocked && !maxed && coins >= shownCost;
        }

        if (machineUpgradeButtonText != null)
            machineUpgradeButtonText.text = "Mak";

        if (basicMachineButtonText != null)
        {
            bool maxed = machineUpgradeLevel >= UpgradeMaxLevel;
            int shownCost = GetDiscountedUpgradeCost(machineUpgradeCost);
            basicMachineButtonText.text = maxed
                ? "Basit Kahve Makinesi Lv." + machineUpgradeLevel + "/" + UpgradeMaxLevel + "\nMAX"
                : "Basit Kahve Makinesi Lv." + machineUpgradeLevel + "/" + UpgradeMaxLevel + "\n+" + BasicMachineIncomeGain + "/sn | " + shownCost + " coin";
            Button button = basicMachineButtonText.GetComponentInParent<Button>();
            if (button != null)
                button.interactable = !maxed && coins >= shownCost;
        }

        if (espressoMachineButtonText != null)
        {
            bool unlocked = playerLevel >= PremiumTierUnlockLevel && machineUpgradeLevel >= NextTierRequiredLevel;
            bool maxed = espressoMachineLevel >= UpgradeMaxLevel;
            int shownCost = GetDiscountedUpgradeCost(espressoMachineCost);
            espressoMachineButtonText.text = !unlocked
                ? "Espresso Makinesi\nKilitli: Lv." + PremiumTierUnlockLevel + " + onceki Lv." + NextTierRequiredLevel
                : maxed
                    ? "Espresso Makinesi Lv." + espressoMachineLevel + "/" + UpgradeMaxLevel + "\nMAX"
                    : "Espresso Makinesi Lv." + espressoMachineLevel + "/" + UpgradeMaxLevel + "\n+" + EspressoMachineIncomeGain + "/sn | " + shownCost + " coin";
            Button button = espressoMachineButtonText.GetComponentInParent<Button>();
            if (button != null)
                button.interactable = unlocked && !maxed && coins >= shownCost;
        }

        if (premiumMachineButtonText != null)
        {
            bool unlocked = playerLevel >= SignatureTierUnlockLevel && espressoMachineLevel >= NextTierRequiredLevel;
            bool maxed = premiumMachineLevel >= UpgradeMaxLevel;
            int shownCost = GetDiscountedUpgradeCost(premiumMachineCost);
            premiumMachineButtonText.text = !unlocked
                ? "Premium Makine\nKilitli: Lv." + SignatureTierUnlockLevel + " + onceki Lv." + NextTierRequiredLevel
                : maxed
                    ? "Premium Makine Lv." + premiumMachineLevel + "/" + UpgradeMaxLevel + "\nMAX"
                    : "Premium Makine Lv." + premiumMachineLevel + "/" + UpgradeMaxLevel + "\n+" + PremiumMachineIncomeGain + "/sn | " + shownCost + " coin";
            Button button = premiumMachineButtonText.GetComponentInParent<Button>();
            if (button != null)
                button.interactable = unlocked && !maxed && coins >= shownCost;
        }

        if (baristaUpgradeButtonText != null)
            baristaUpgradeButtonText.text = "Bar";

        if (baristaStatusText != null)
            baristaStatusText.text = GetBaristaStatusText();

        if (juniorBaristaButtonText != null)
        {
            bool maxed = baristaUpgradeLevel >= UpgradeMaxLevel;
            int shownCost = GetDiscountedUpgradeCost(baristaUpgradeCost);
            juniorBaristaButtonText.text = maxed
                ? "Barista Lv." + baristaUpgradeLevel + "/" + UpgradeMaxLevel + "\nMAX"
                : baristaUpgradeLevel <= 0
                    ? "Barista Al\nOtomatik servis + offline kazanç | " + shownCost + " coin"
                    : "Barista Lv." + baristaUpgradeLevel + "/" + UpgradeMaxLevel + "\n+" + JuniorBaristaIncomeGain + "/sn | " + shownCost + " coin | " + GetBaristaAutoServeSeconds().ToString("0.#") + " sn";
            Button button = juniorBaristaButtonText.GetComponentInParent<Button>();
            if (button != null)
                button.interactable = !maxed && coins >= shownCost;
        }

        if (seniorBaristaButtonText != null)
        {
            bool unlocked = playerLevel >= SeniorBaristaUnlockLevel && baristaUpgradeLevel >= NextTierRequiredLevel;
            bool maxed = seniorBaristaLevel >= UpgradeMaxLevel;
            int shownCost = GetDiscountedUpgradeCost(seniorBaristaCost);
            seniorBaristaButtonText.text = !unlocked
                ? "Usta Barista\nKilitli: Lv." + SeniorBaristaUnlockLevel + " + onceki Lv." + NextTierRequiredLevel
                : maxed
                    ? "Usta Barista Lv." + seniorBaristaLevel + "/" + UpgradeMaxLevel + "\nMAX"
                    : "Usta Barista Lv." + seniorBaristaLevel + "/" + UpgradeMaxLevel + "\n+" + SeniorBaristaIncomeGain + "/sn | " + shownCost + " coin";
            Button button = seniorBaristaButtonText.GetComponentInParent<Button>();
            if (button != null)
                button.interactable = unlocked && !maxed && coins >= shownCost;
        }

        if (masterBaristaButtonText != null)
        {
            bool unlocked = playerLevel >= MasterBaristaUnlockLevel && seniorBaristaLevel >= NextTierRequiredLevel;
            bool maxed = masterBaristaLevel >= UpgradeMaxLevel;
            int shownCost = GetDiscountedUpgradeCost(masterBaristaCost);
            masterBaristaButtonText.text = !unlocked
                ? "Sef Barista\nKilitli: Lv." + MasterBaristaUnlockLevel + " + onceki Lv." + NextTierRequiredLevel
                : maxed
                    ? "Sef Barista Lv." + masterBaristaLevel + "/" + UpgradeMaxLevel + "\nMAX"
                    : "Sef Barista Lv." + masterBaristaLevel + "/" + UpgradeMaxLevel + "\n+" + MasterBaristaIncomeGain + "/sn | " + shownCost + " coin";
            Button button = masterBaristaButtonText.GetComponentInParent<Button>();
            if (button != null)
                button.interactable = unlocked && !maxed && coins >= shownCost;
        }

        if (coffeeUpgradeButton != null)
            coffeeUpgradeButton.interactable = true;

        if (machineUpgradeButton != null)
            machineUpgradeButton.interactable = true;

        if (baristaUpgradeButton != null)
            baristaUpgradeButton.interactable = true;
    }

    private void UpdateDecorButton(TMP_Text buttonText, DecorItemData item)
    {
        if (buttonText == null || item == null)
        {
            return;
        }

        bool unlocked = IsDecorItemUnlocked(item);
        bool selected = previewDecorItem == item.PreviewId;
        bool canUnlock = CanUnlockDecorItem(item);

        Button button = buttonText.GetComponentInParent<Button>();
        RectTransform buttonRect = button != null ? button.GetComponent<RectTransform>() : null;
        bool compactButton = buttonRect != null && buttonRect.sizeDelta.x <= 100f;
        if (selected)
        {
            buttonText.text = compactButton ? "" : item.DisplayName + "\nOnizleme";
        }
        else if (unlocked)
        {
            buttonText.text = compactButton ? "" : item.DisplayName + "\nAcik";
        }
        else if (canUnlock)
        {
            buttonText.text = compactButton ? "" : item.DisplayName + "\nUretilebilir";
        }
        else
        {
            buttonText.text = compactButton ? "" : item.DisplayName + "\nKilitli";
        }

        if (button != null)
        {
            button.interactable = true;
            Image buttonImage = button.GetComponent<Image>();
            if (buttonImage != null)
            {
                if (selected)
                {
                    buttonImage.color = compactButton ? new Color(0.72f, 0.45f, 0.12f, 1f) : new Color(0.86f, 0.58f, 0.18f, 1f);
                }
                else if (unlocked)
                {
                    buttonImage.color = compactButton ? new Color(0.18f, 0.14f, 0.10f, 1f) : new Color(0.26f, 0.45f, 0.34f, 1f);
                }
                else if (canUnlock)
                {
                    buttonImage.color = compactButton ? new Color(0.18f, 0.14f, 0.10f, 1f) : new Color(0.45f, 0.32f, 0.20f, 1f);
                }
                else
                {
                    buttonImage.color = compactButton ? new Color(0.10f, 0.08f, 0.07f, 1f) : new Color(0.22f, 0.20f, 0.19f, 1f);
                }
            }
        }
    }

    private void UpdateFoodUpgradePanelUi()
    {
        UpdateFoodCategoryInfo();

        for (int i = 0; i < foodUpgradeCardUis.Count; i++)
        {
            FoodUpgradeCardUi ui = foodUpgradeCardUis[i];
            FoodUpgradeData data = ui.Data;
            bool unlocked = IsFoodUpgradeUnlocked(data);
            bool purchased = data.Level > 0;
            int cost = GetDiscountedUpgradeCost(GetFoodUpgradeCost(data));

            if (ui.CardImage != null)
            {
                ui.CardImage.sprite = GetFoodCardFrameSprite();
                ui.CardImage.color = unlocked ? Color.white : new Color(0.58f, 0.56f, 0.52f, 1f);
            }

            if (ui.IconImage != null)
            {
                ui.IconImage.color = unlocked ? Color.white : new Color(0.72f, 0.69f, 0.63f, 0.72f);
            }

            if (ui.LevelBadgeImage != null)
            {
                ui.LevelBadgeImage.gameObject.SetActive(true);
            }

            if (ui.LevelRowImage != null)
            {
                ui.LevelRowImage.gameObject.SetActive(true);
                ui.LevelRowImage.rectTransform.anchoredPosition = unlocked
                    ? new Vector2(18f, -35f)
                    : new Vector2(9.815f, -26.4f);
                ui.LevelRowImage.rectTransform.sizeDelta = unlocked
                    ? new Vector2(120f, 30f)
                    : new Vector2(100.57f, 21.959f);
            }

            if (ui.LevelLabelText != null)
            {
                ui.LevelLabelText.gameObject.SetActive(true);
                ui.LevelLabelText.rectTransform.anchoredPosition = unlocked
                    ? new Vector2(17.99998f, -30.04402f)
                    : new Vector2(11.714f, -26.024f);
                ui.LevelLabelText.rectTransform.sizeDelta = new Vector2(85.427f, 22.5021f);
            }

            if (ui.OrderStripImage != null)
            {
                ui.OrderStripImage.gameObject.SetActive(true);
            }

            if (ui.PassiveStripImage != null)
            {
                ui.PassiveStripImage.gameObject.SetActive(true);
            }

            if (ui.BeanLeftImage != null) ui.BeanLeftImage.gameObject.SetActive(unlocked);
            if (ui.BeanCenterImage != null) ui.BeanCenterImage.gameObject.SetActive(unlocked);
            if (ui.BeanRightImage != null) ui.BeanRightImage.gameObject.SetActive(unlocked);

            if (ui.LockedBadgeImage != null) ui.LockedBadgeImage.gameObject.SetActive(false);
            if (ui.LockedIconImage != null) ui.LockedIconImage.gameObject.SetActive(false);
            if (ui.UnlockStripImage != null) ui.UnlockStripImage.gameObject.SetActive(false);
            if (ui.UnlockText != null)
            {
                ui.UnlockText.gameObject.SetActive(false);
                ui.UnlockText.text = "Seviye " + data.UnlockPlayerLevel + "'de açılır";
            }

            if (ui.OrderIconImage != null)
            {
                ui.OrderIconImage.gameObject.SetActive(true);
                ui.OrderIconImage.color = unlocked ? Color.white : new Color(0.72f, 0.69f, 0.63f, 0.78f);
            }

            if (ui.PassiveIconImage != null)
            {
                ui.PassiveIconImage.gameObject.SetActive(true);
                ui.PassiveIconImage.color = unlocked ? Color.white : new Color(0.72f, 0.69f, 0.63f, 0.78f);
            }

            if (ui.NameText != null)
            {
                ui.NameText.text = data.DisplayName;
                ui.NameText.color = unlocked ? new Color(1f, 0.89f, 0.66f) : new Color(0.72f, 0.68f, 0.60f);
            }

            if (ui.LevelText != null)
            {
                ui.LevelText.gameObject.SetActive(true);
                ui.LevelText.text = data.UnlockPlayerLevel.ToString();
            }

            if (ui.OrderText != null)
            {
                ui.OrderText.gameObject.SetActive(true);
                ui.OrderText.color = unlocked ? new Color(0.18f, 0.10f, 0.04f) : new Color(0.36f, 0.33f, 0.29f);
                ui.OrderText.text = "Satış Geliri  +%" + data.SalesBonusPercent;
            }

            if (ui.PassiveText != null)
            {
                ui.PassiveText.gameObject.SetActive(true);
                ui.PassiveText.color = unlocked ? new Color(0.18f, 0.10f, 0.04f) : new Color(0.36f, 0.33f, 0.29f);
                ui.PassiveText.text = "Pasif Gelir  +%" + data.PassiveBonusPercent;
            }

            if (ui.PriceText != null)
            {
                ui.PriceText.gameObject.SetActive(unlocked && !purchased);
                ui.PriceText.text = unlocked && !purchased ? FormatResourceAmount(cost) : "";
            }

            if (ui.PriceCoinImage != null)
            {
                ui.PriceCoinImage.gameObject.SetActive(unlocked && !purchased);
            }

            if (ui.ButtonText != null)
            {
                ui.ButtonText.rectTransform.anchoredPosition = unlocked && !purchased ? new Vector2(-29, 0) : Vector2.zero;
                ui.ButtonText.rectTransform.sizeDelta = unlocked && !purchased ? new Vector2(60, 25) : new Vector2(116, 26);
                ui.ButtonText.text = purchased ? "Alındı" : unlocked ? "Satın Al" : "Kilitli";
            }

            if (ui.Button != null)
            {
                ui.Button.interactable = unlocked && !purchased && coins >= cost;
                Image buttonImage = ui.Button.GetComponent<Image>();
                RectTransform buttonRect = ui.Button.GetComponent<RectTransform>();
                if (buttonImage != null && buttonRect != null)
                {
                    buttonImage.sprite = GetFoodUpgradeButtonSprite();
                    buttonImage.color = unlocked && !purchased ? Color.white : purchased ? new Color(0.70f, 0.86f, 0.56f, 1f) : new Color(0.52f, 0.47f, 0.40f, 1f);
                    buttonImage.type = Image.Type.Simple;
                }
            }
        }
    }

    private bool IsPreviewingDecor()
    {
        return decorPanel != null && decorPanel.activeSelf && previewDecorItem > 0;
    }

    private string GetPreviewDecorName()
    {
        DecorItemData item = GetPreviewDecorItem();
        if (item != null) return item.DisplayName;
        return "Dekor";
    }

    private bool IsPreviewDecorUnlocked()
    {
        return IsDecorItemUnlocked(GetPreviewDecorItem());
    }

    private bool CanUnlockPreviewDecor()
    {
        return CanUnlockDecorItem(GetPreviewDecorItem());
    }

    private string GetPreviewDecorCostText()
    {
        return GetDecorCostText(GetPreviewDecorItem());
    }

    private string GetPreviewDecorTypeText()
    {
        DecorItemData item = GetPreviewDecorItem();
        if (item == null)
        {
            return "Dekor";
        }

        if (item.Slot == DecorSlot.Wall) return "Duvar";
        if (item.Slot == DecorSlot.Floor) return "Zemin";
        if (item.Slot == DecorSlot.Table) return "Masa";
        return "Dekor";
    }

    private void UpdateDecorResourceRow(DecorItemData item, bool previewUnlocked)
    {
        bool hasItem = item != null;
        bool usesWood = hasItem && item.CostResource == "wood";
        int available = usesWood ? woodPieces : GetAvailablePaintCanCount();
        bool hasEnough = previewUnlocked || (hasItem && available >= item.CostAmount);
        Color readyColor = new Color(0.72f, 1f, 0.66f, 1f);
        Color missingColor = new Color(1f, 0.72f, 0.58f, 1f);
        Color normalColor = new Color(0.96f, 0.91f, 0.82f, 1f);
        Color valueColor = hasEnough ? readyColor : missingColor;

        if (decorDetailIconImage != null)
        {
            decorDetailIconImage.sprite = GetDecorIconSprite(item);
            decorDetailIconImage.gameObject.SetActive(hasItem && decorDetailIconImage.sprite != null);
        }

        if (decorDetailTitleText != null)
        {
            decorDetailTitleText.text = hasItem ? item.DisplayName : "";
        }

        if (decorDetailTypeText != null)
        {
            decorDetailTypeText.text = hasItem ? "Dekor / " + GetPreviewDecorTypeText() : "";
        }

        if (decorDetailStatusText != null)
        {
            decorDetailStatusText.text = previewUnlocked ? "Acik" : "Kilitli";
            decorDetailStatusText.color = previewUnlocked ? readyColor : missingColor;
        }

        Color wallColor = GetWallColor();
        Color floorColor = GetFloorColor();
        Color tableColor = GetTableColor();
        if (decorPreviewBackWallImage != null) decorPreviewBackWallImage.color = wallColor;
        if (decorPreviewLeftWallImage != null) decorPreviewLeftWallImage.color = ShadeColor(wallColor, 0.84f);
        if (decorPreviewRightWallImage != null) decorPreviewRightWallImage.color = ShadeColor(wallColor, 0.78f);
        if (decorPreviewFloorImage != null) decorPreviewFloorImage.color = floorColor;
        if (decorPreviewTableTopImage != null) decorPreviewTableTopImage.color = tableColor;
        if (decorPreviewBeforeSwatchImage != null)
        {
            if (hasItem && item.Slot == DecorSlot.Wall) decorPreviewBeforeSwatchImage.color = GetSelectedWallColor();
            else if (hasItem && item.Slot == DecorSlot.Floor) decorPreviewBeforeSwatchImage.color = GetSelectedFloorColor();
            else decorPreviewBeforeSwatchImage.color = GetSelectedTableColor();
        }
        if (decorPreviewAfterSwatchImage != null)
        {
            if (hasItem && item.Slot == DecorSlot.Wall) decorPreviewAfterSwatchImage.color = wallColor;
            else if (hasItem && item.Slot == DecorSlot.Floor) decorPreviewAfterSwatchImage.color = floorColor;
            else decorPreviewAfterSwatchImage.color = tableColor;
        }

        if (decorCostIconImage != null)
        {
            decorCostIconImage.sprite = usesWood ? GetWoodIconSprite() : GetCraftPaintIconSprite();
            decorCostIconImage.color = previewUnlocked ? new Color(1f, 1f, 1f, 0.55f) : Color.white;
            decorCostIconImage.gameObject.SetActive(hasItem);
        }

        if (decorCostText != null)
        {
            string resourceName = usesWood ? "Tahta" : "Boya Kutusu";
            decorCostText.text = previewUnlocked ? resourceName + " tamam" : resourceName + "     " + available + "/" + (hasItem ? item.CostAmount : 0);
            decorCostText.color = previewUnlocked ? readyColor : valueColor;
            decorCostText.gameObject.SetActive(hasItem);
        }

        if (decorStoragePaintIconImage != null)
        {
            decorStoragePaintIconImage.sprite = GetCraftPaintIconSprite();
            decorStoragePaintIconImage.color = Color.white;
        }

        if (decorStoragePaintText != null)
        {
            decorStoragePaintText.text = "x" + GetAvailablePaintCanCount();
            decorStoragePaintText.color = normalColor;
        }

        if (decorStorageWoodIconImage != null)
        {
            decorStorageWoodIconImage.sprite = GetWoodIconSprite();
            decorStorageWoodIconImage.color = Color.white;
        }

        if (decorStorageWoodText != null)
        {
            decorStorageWoodText.text = "x" + woodPieces;
            decorStorageWoodText.color = normalColor;
        }
    }

    private DecorItemData GetPreviewDecorItem()
    {
        return GetDecorItemData(previewDecorItem);
    }

    private DecorItemData GetDecorItemData(int previewId)
    {
        for (int i = 0; i < decorItemDatabase.Count; i++)
        {
            if (decorItemDatabase[i].PreviewId == previewId)
            {
                return decorItemDatabase[i];
            }
        }

        return null;
    }

    private bool IsDecorItemUnlocked(DecorItemData item)
    {
        if (item == null)
        {
            return false;
        }

        if (item.PreviewId == 1) return pinkWallUnlocked;
        if (item.PreviewId == 2) return mintWallUnlocked;
        if (item.PreviewId == 3) return creamFloorUnlocked;
        if (item.PreviewId == 4) return roseTableUnlocked;
        return false;
    }

    private bool CanUnlockDecorItem(DecorItemData item)
    {
        if (item == null)
        {
            return false;
        }

        if (item.CostResource == "paint")
        {
            return GetAvailablePaintCanCount() >= item.CostAmount;
        }

        if (item.CostResource == "wood")
        {
            return woodPieces >= item.CostAmount;
        }

        return false;
    }

    private bool TrySpendDecorCost(DecorItemData item)
    {
        if (item == null)
        {
            return false;
        }

        if (item.CostResource == "paint")
        {
            return TrySpendPaintCans(item.CostAmount);
        }

        if (item.CostResource == "wood")
        {
            if (woodPieces < item.CostAmount)
            {
                return false;
            }

            woodPieces -= item.CostAmount;
            return true;
        }

        return false;
    }

    private int GetAvailablePaintCanCount()
    {
        int total = paintPieces;
        for (int i = 0; i < itemDatabase.Count; i++)
        {
            ItemData item = itemDatabase[i];
            if (item.Category == ItemCategory.Material && item.Id.StartsWith("paint_can_", StringComparison.Ordinal))
            {
                total += GetInventoryItemCount(item.Id);
            }
        }

        return total;
    }

    private bool TrySpendPaintCans(int amount)
    {
        if (amount <= 0)
        {
            return true;
        }

        if (GetAvailablePaintCanCount() < amount)
        {
            return false;
        }

        int remaining = amount;
        int legacySpend = Mathf.Min(paintPieces, remaining);
        paintPieces -= legacySpend;
        remaining -= legacySpend;

        for (int i = 0; i < itemDatabase.Count && remaining > 0; i++)
        {
            ItemData item = itemDatabase[i];
            if (item.Category != ItemCategory.Material || !item.Id.StartsWith("paint_can_", StringComparison.Ordinal))
            {
                continue;
            }

            int count = GetInventoryItemCount(item.Id);
            if (count <= 0)
            {
                continue;
            }

            int spend = Mathf.Min(count, remaining);
            itemInventory[item.Id] = count - spend;
            remaining -= spend;
        }

        return true;
    }

    private void SetDecorItemUnlocked(DecorItemData item, bool unlocked)
    {
        if (item == null)
        {
            return;
        }

        if (item.PreviewId == 1)
        {
            pinkWallUnlocked = unlocked;
        }
        else if (item.PreviewId == 2)
        {
            mintWallUnlocked = unlocked;
        }
        else if (item.PreviewId == 3)
        {
            creamFloorUnlocked = unlocked;
        }
        else if (item.PreviewId == 4)
        {
            roseTableUnlocked = unlocked;
        }
    }

    private string GetDecorCostText(DecorItemData item)
    {
        if (item == null)
        {
            return "";
        }

        string resourceName = item.CostResource == "wood" ? "Tahta" : "Boya Kutusu";
        return item.CostAmount + " " + resourceName;
    }

    private void ApplyDecorations()
    {
        if (isoBackWallGraphic != null)
            isoBackWallGraphic.color = GetWallColor();

        if (isoLeftWallGraphic != null)
            isoLeftWallGraphic.color = ShadeColor(GetWallColor(), 0.86f);

        if (isoRightWallGraphic != null)
            isoRightWallGraphic.color = ShadeColor(GetWallColor(), 0.80f);

        if (isoFloorGraphic != null)
            isoFloorGraphic.color = GetFloorColor();

        if (isoFloorShadowGraphic != null)
            isoFloorShadowGraphic.color = new Color(0.08f, 0.07f, 0.06f, 0.30f);

        if (backgroundTapImage != null)
            backgroundTapImage.color = new Color(1f, 1f, 1f, 0f);

        if (leftWallImage != null)
            leftWallImage.color = new Color(1f, 1f, 1f, 0f);

        if (rightWallImage != null)
            rightWallImage.color = new Color(1f, 1f, 1f, 0f);

        if (floorPreviewImage != null)
            floorPreviewImage.color = new Color(1f, 1f, 1f, 0f);

        if (tablePreviewImage != null)
            tablePreviewImage.color = GetTableColor();

        if (backWallTopTrim != null)
            backWallTopTrim.color = ShadeColor(GetRoomAccentColor(), 0.65f);

        if (roomAccentStrip != null)
            roomAccentStrip.color = GetRoomAccentColor();

        if (floorBackEdge != null)
            floorBackEdge.color = new Color(0.18f, 0.14f, 0.12f, currentRoomIndex == 1 ? 0.48f : 0.34f);

        Color floorLineColor = currentRoomIndex == 1
            ? new Color(0.74f, 0.86f, 0.92f, 0.18f)
            : new Color(1f, 1f, 1f, 0.12f);
        if (floorCenterLine != null)
            floorCenterLine.color = floorLineColor;
        if (floorLeftPerspectiveLine != null)
            floorLeftPerspectiveLine.color = floorLineColor;
        if (floorRightPerspectiveLine != null)
            floorRightPerspectiveLine.color = floorLineColor;

        UpdateRoomVisuals();
    }

    private Color GetWallColor()
    {
        WorkshopDecorProduct workshopProduct = GetDisplayedWorkshopProduct(WorkshopDecorCategory.Wall);
        if (workshopProduct != null) return GetWorkshopVariantColor(workshopProduct.VariantKey, new Color(0.88f, 0.95f, 1f, 1f));
        int wallStyle = selectedWallStyle;
        DecorItemData previewItem = GetPreviewDecorItem();
        if (IsPreviewingDecor() && previewItem != null && previewItem.Slot == DecorSlot.Wall)
        {
            wallStyle = previewItem.StyleId;
        }

        if (wallStyle == 1)
            return new Color(1f, 0.72f, 0.84f, 1f);

        if (wallStyle == 2)
            return new Color(0.70f, 0.92f, 0.84f, 1f);

        if (currentRoomIndex == 1)
            return new Color(0.66f, 0.72f, 0.76f, 1f);

        if (currentRoomIndex == 2)
            return new Color(0.76f, 0.70f, 0.82f, 1f);

        return new Color(0.88f, 0.95f, 1f, 1f);
    }

    private Color GetSelectedWallColor()
    {
        int wallStyle = selectedWallStyle;
        if (wallStyle == 1)
            return new Color(1f, 0.72f, 0.84f, 1f);

        if (wallStyle == 2)
            return new Color(0.70f, 0.92f, 0.84f, 1f);

        if (currentRoomIndex == 1)
            return new Color(0.66f, 0.72f, 0.76f, 1f);

        if (currentRoomIndex == 2)
            return new Color(0.76f, 0.70f, 0.82f, 1f);

        return new Color(0.88f, 0.95f, 1f, 1f);
    }

    private Color GetFloorColor()
    {
        WorkshopDecorProduct workshopProduct = GetDisplayedWorkshopProduct(WorkshopDecorCategory.Floor);
        if (workshopProduct != null) return GetWorkshopVariantColor(workshopProduct.VariantKey, new Color(0.64f, 0.50f, 0.38f, 1f));
        int floorStyle = selectedFloorStyle;
        DecorItemData previewItem = GetPreviewDecorItem();
        if (IsPreviewingDecor() && previewItem != null && previewItem.Slot == DecorSlot.Floor)
        {
            floorStyle = previewItem.StyleId;
        }

        if (floorStyle == 1)
            return new Color(0.86f, 0.74f, 0.55f, 1f);

        if (currentRoomIndex == 1)
            return new Color(0.42f, 0.46f, 0.48f, 1f);

        if (currentRoomIndex == 2)
            return new Color(0.50f, 0.42f, 0.56f, 1f);

        return new Color(0.64f, 0.50f, 0.38f, 1f);
    }

    private Color GetSelectedFloorColor()
    {
        int floorStyle = selectedFloorStyle;
        if (floorStyle == 1)
            return new Color(0.86f, 0.74f, 0.55f, 1f);

        if (currentRoomIndex == 1)
            return new Color(0.42f, 0.46f, 0.48f, 1f);

        if (currentRoomIndex == 2)
            return new Color(0.50f, 0.42f, 0.56f, 1f);

        return new Color(0.64f, 0.50f, 0.38f, 1f);
    }

    private Color GetTableColor()
    {
        WorkshopDecorProduct workshopProduct = GetDisplayedWorkshopProduct(WorkshopDecorCategory.Table);
        if (workshopProduct != null) return GetWorkshopVariantColor(workshopProduct.VariantKey, new Color(0.45f, 0.25f, 0.14f, 1f));
        int tableStyle = selectedTableStyle;
        DecorItemData previewItem = GetPreviewDecorItem();
        if (IsPreviewingDecor() && previewItem != null && previewItem.Slot == DecorSlot.Table)
        {
            tableStyle = previewItem.StyleId;
        }

        if (tableStyle == 1)
            return new Color(0.86f, 0.38f, 0.54f, 1f);

        if (currentRoomIndex == 1)
            return new Color(0.26f, 0.30f, 0.34f, 1f);

        if (currentRoomIndex == 2)
            return new Color(0.24f, 0.18f, 0.28f, 1f);

        return new Color(0.45f, 0.25f, 0.14f, 1f);
    }

    private Color GetSelectedTableColor()
    {
        int tableStyle = selectedTableStyle;
        if (tableStyle == 1)
            return new Color(0.86f, 0.38f, 0.54f, 1f);

        if (currentRoomIndex == 1)
            return new Color(0.26f, 0.30f, 0.34f, 1f);

        if (currentRoomIndex == 2)
            return new Color(0.24f, 0.18f, 0.28f, 1f);

        return new Color(0.45f, 0.25f, 0.14f, 1f);
    }

    private WorkshopDecorProduct GetDisplayedWorkshopProduct(WorkshopDecorCategory category)
    {
        if (decorPanel != null && decorPanel.activeSelf && selectedWorkshopCategory == category)
        {
            WorkshopDecorProduct preview = GetWorkshopProduct(selectedWorkshopPreviewId);
            if (preview != null && preview.Category == category) return preview;
        }

        if (selectedWorkshopProducts.TryGetValue(category, out string selectedId))
        {
            WorkshopDecorProduct product = GetWorkshopProduct(selectedId);
            if (product != null && unlockedWorkshopProducts.Contains(product.Id)) return product;
        }
        return null;
    }

    private static Color GetWorkshopVariantColor(string variantKey, Color fallback)
    {
        if (string.IsNullOrEmpty(variantKey)) return fallback;
        if (variantKey.Contains("mint")) return new Color(0.52f, 0.78f, 0.70f, 1f);
        if (variantKey.Contains("rose")) return new Color(0.78f, 0.48f, 0.54f, 1f);
        if (variantKey.Contains("burgundy")) return new Color(0.40f, 0.10f, 0.14f, 1f);
        if (variantKey.Contains("navy")) return new Color(0.12f, 0.20f, 0.34f, 1f);
        if (variantKey.Contains("charcoal") || variantKey.Contains("black")) return new Color(0.22f, 0.22f, 0.23f, 1f);
        if (variantKey.Contains("cream") || variantKey.Contains("marble")) return new Color(0.90f, 0.82f, 0.66f, 1f);
        if (variantKey.Contains("olive")) return new Color(0.46f, 0.48f, 0.26f, 1f);
        if (variantKey.Contains("terracotta")) return new Color(0.66f, 0.31f, 0.18f, 1f);
        if (variantKey.Contains("walnut") || variantKey.Contains("oak")) return new Color(0.48f, 0.27f, 0.12f, 1f);
        return fallback;
    }

    private Color GetRoomAccentColor()
    {
        if (currentRoomIndex == 1)
            return new Color(0.22f, 0.42f, 0.52f, 1f);

        if (currentRoomIndex == 2)
            return new Color(0.72f, 0.54f, 0.22f, 1f);

        return new Color(0.48f, 0.32f, 0.22f, 1f);
    }

    private void UpdateRoomVisuals()
    {
        bool unlocked = IsCurrentRoomUnlocked();
        bool showStartingRoom = currentRoomIndex == 0;
        if (cafeSceneImage != null)
        {
            cafeSceneImage.sprite = GetActiveWorkshopWallSceneSprite(false);
        }
        bool useCafeSceneImage = showStartingRoom && cafeSceneImage != null && cafeSceneImage.sprite != null;
        for (int i = 0; i < startingRoomVisuals.Count; i++)
        {
            if (startingRoomVisuals[i] != null)
            {
                startingRoomVisuals[i].SetActive(showStartingRoom);
            }
        }

        for (int i = 0; i < cafePrototypeVisuals.Count; i++)
        {
            if (cafePrototypeVisuals[i] != null)
            {
                cafePrototypeVisuals[i].SetActive(!showStartingRoom || !useCafeSceneImage);
            }
        }

        for (int i = 0; i < cafeObjectHotspots.Count; i++)
        {
            if (cafeObjectHotspots[i] != null)
            {
                cafeObjectHotspots[i].SetActive(showStartingRoom && unlocked);
            }
        }

        SetSceneImageActive(baristaLockedSceneImage, showStartingRoom && unlocked && !HasBaristaUnlocked());
        SetSceneImageActive(baristaSceneImage, showStartingRoom && unlocked && HasBaristaUnlocked());
        SetSceneImageActive(baristaCounterForegroundImage, showStartingRoom && unlocked);
        SetSceneImageActive(coffeeMachineSceneImage, showStartingRoom && unlocked && machineUpgradeLevel > 0);
        SetSceneImageActive(coffeeMachineLockedPlaceholderImage, showStartingRoom && unlocked && machineUpgradeLevel <= 0);

        SetSceneImageActive(productionConveyorVisual, currentRoomIndex == 1 && unlocked);
        SetSceneImageActive(productionMachineVisual, currentRoomIndex == 1 && unlocked);
        SetSceneImageActive(vipRugVisual, currentRoomIndex == 2 && unlocked);
        SetSceneImageActive(vipCounterVisual, currentRoomIndex == 2 && unlocked);
        SetSceneImageActive(vipPlantVisual, currentRoomIndex == 2 && unlocked);

        if (roomLockOverlay != null)
        {
            roomLockOverlay.gameObject.SetActive(!unlocked);
            roomLockOverlay.color = GetLockedRoomOverlayColor();
            if (!unlocked)
            {
                roomLockOverlay.transform.SetAsLastSibling();
            }
        }
    }

    private Color GetLockedRoomOverlayColor()
    {
        if (currentRoomIndex == 1)
        {
            return new Color(0.10f, 0.15f, 0.18f, 1f);
        }

        if (currentRoomIndex == 2)
        {
            return new Color(0.13f, 0.09f, 0.16f, 1f);
        }

        return new Color(0.05f, 0.06f, 0.07f, 1f);
    }

    private void SetSceneImageActive(Image image, bool active)
    {
        if (image != null)
        {
            image.gameObject.SetActive(active);
        }
    }

    private void SetSceneGraphicActive(Graphic graphic, bool active)
    {
        if (graphic != null)
        {
            graphic.gameObject.SetActive(active);
        }
    }

    private bool IsCurrentRoomUnlocked()
    {
        if (currentRoomIndex == 0)
        {
            return true;
        }

        if (currentRoomIndex == 1)
        {
            return productionRoomUnlocked;
        }

        return vipRoomUnlocked;
    }

    private int GetCurrentRoomUnlockCost()
    {
        if (currentRoomIndex == 1)
        {
            return ProductionRoomUnlockCost;
        }

        if (currentRoomIndex == 2)
        {
            return VipRoomUnlockCost;
        }

        return 0;
    }

    private string GetCurrentRoomName()
    {
        if (currentRoomIndex == 1)
        {
            return productionRoomUnlocked ? "Uretim Odasi" : "Kilitli: Uretim Odasi";
        }

        if (currentRoomIndex == 2)
        {
            return vipRoomUnlocked ? "VIP Servis Odası" : "Kilitli: VIP Servis Odası";
        }

        return "Baslangic Odasi";
    }

    private void RefreshDailyTasksIfNeeded()
    {
        if (string.IsNullOrEmpty(lastDailyTasksUtc) || !long.TryParse(lastDailyTasksUtc, out long ticks))
        {
            ResetDailyTasks();
            return;
        }

        DateTime lastResetTime = new DateTime(ticks, DateTimeKind.Utc);
        if ((DateTime.UtcNow - lastResetTime).TotalHours >= DailyTasksResetHours)
        {
            ResetDailyTasks();
            SaveGame();
        }
    }

    private void ResetDailyTasks()
    {
        dailyCoffeeSold = 0;
        dailyCustomerSales = 0;
        dailyChestsOpened = 0;
        dailyCoffeeRewardClaimed = false;
        dailyCustomerRewardClaimed = false;
        dailyChestRewardClaimed = false;
        lastDailyTasksUtc = DateTime.UtcNow.Ticks.ToString();
    }

    private bool CanClaimAnyDailyTask()
    {
        return (!dailyCoffeeRewardClaimed && dailyCoffeeSold >= 20)
            || (!dailyCustomerRewardClaimed && dailyCustomerSales >= 5)
            || (!dailyChestRewardClaimed && dailyChestsOpened >= 1);
    }

    private string GetDailyTasksText()
    {
        return "Yenilenme: " + GetDailyTasksWaitText()
            + "\nBugun tamamlanan oduller: " + GetClaimedDailyTaskCount() + "/3";
    }

    private int GetClaimedDailyTaskCount()
    {
        int count = 0;
        if (dailyCoffeeRewardClaimed) count++;
        if (dailyCustomerRewardClaimed) count++;
        if (dailyChestRewardClaimed) count++;
        return count;
    }

    private string GetDailyTasksWaitText()
    {
        if (string.IsNullOrEmpty(lastDailyTasksUtc) || !long.TryParse(lastDailyTasksUtc, out long ticks))
        {
            return "hazirlaniyor";
        }

        DateTime lastResetTime = new DateTime(ticks, DateTimeKind.Utc);
        double remainingSeconds = (lastResetTime.AddHours(DailyTasksResetHours) - DateTime.UtcNow).TotalSeconds;
        return FormatLongTime((float)Math.Max(0, remainingSeconds));
    }

    private void UpdateDailyTaskButton(TMP_Text buttonText, bool claimed, bool ready, string taskName, int progress, int target, string rewardText)
    {
        if (buttonText == null)
        {
            return;
        }

        Button button = buttonText.GetComponentInParent<Button>();
        if (claimed)
        {
            buttonText.text = taskName + ": " + target + "/" + target + "\nAlindi";
        }
        else
        {
            int shownProgress = Mathf.Min(progress, target);
            buttonText.text = taskName + ": " + shownProgress + "/" + target + "\n" + rewardText + (ready ? " | Ödülü Al" : "");
        }

        if (button != null)
        {
            button.interactable = ready && !claimed;
        }
    }


    private bool HasVisibleTask()
    {
        if (!firstTaskRewardClaimed)
        {
            return true;
        }

        if (playerLevel >= 2 && !secondTaskRewardClaimed)
        {
            return true;
        }

        if (playerLevel >= 3 && !thirdTaskRewardClaimed)
        {
            return true;
        }

        if (playerLevel >= 4 && !fourthTaskRewardClaimed)
        {
            return true;
        }

        if (playerLevel >= 5 && !fifthTaskRewardClaimed)
        {
            return true;
        }

        return false;
    }

    private int GetCurrentTaskId()
    {
        if (!firstTaskRewardClaimed)
        {
            return 1;
        }

        if (playerLevel >= 2 && !secondTaskRewardClaimed)
        {
            return 2;
        }

        if (playerLevel >= 3 && !thirdTaskRewardClaimed)
        {
            return 3;
        }

        if (playerLevel >= 4 && !fourthTaskRewardClaimed)
        {
            return 4;
        }

        if (playerLevel >= 5 && !fifthTaskRewardClaimed)
        {
            return 5;
        }

        return 0;
    }

    private bool ShouldShowTaskNotifyDot()
    {
        int currentTaskId = GetCurrentTaskId();
        bool hasNewUnseenTask = currentTaskId > 0 && currentTaskId != seenTaskId;
        return hasNewUnseenTask;
    }

    private bool CanClaimCurrentTask()
    {
        if (!firstTaskRewardClaimed)
        {
            return totalCoffeeSold >= 10;
        }

        if (playerLevel >= 2 && !secondTaskRewardClaimed)
        {
            return GetTotalMachineLevel() >= 1;
        }

        if (playerLevel >= 3 && !thirdTaskRewardClaimed)
        {
            return customerSales >= 5;
        }

        if (playerLevel >= 4 && !fourthTaskRewardClaimed)
        {
            return GetTotalBaristaLevel() >= 1;
        }

        if (playerLevel >= 5 && !fifthTaskRewardClaimed)
        {
            return vipCustomerSales >= 3;
        }

        return false;
    }

    private string GetTaskText()
    {
        if (!firstTaskRewardClaimed)
        {
            int progress = Mathf.Min(totalCoffeeSold, 10);
            return "Görev\n10 kahve sat: " + progress + "/10\nÖdül: 1 Elmas + 25 XP";
        }

        if (playerLevel >= 2 && !secondTaskRewardClaimed)
        {
            int progress = Mathf.Min(GetTotalMachineLevel(), 1);
            return "Görev\nHerhangi 1 Kahve Makinesi al: " + progress + "/1\nÖdül: 2 Elmas + 50 XP";
        }

        if (playerLevel >= 3 && !thirdTaskRewardClaimed)
        {
            int progress = Mathf.Min(customerSales, 5);
            return "Görev\n5 müşteriye kahve sat: " + progress + "/5\nÖdül: 2 Elmas + 75 XP";
        }

        if (playerLevel >= 4 && !fourthTaskRewardClaimed)
        {
            int progress = Mathf.Min(GetTotalBaristaLevel(), 1);
            return "Görev\nHerhangi 1 Barista al: " + progress + "/1\nÖdül: 3 Elmas + 100 XP";
        }

        if (playerLevel >= 5 && !fifthTaskRewardClaimed)
        {
            int progress = Mathf.Min(vipCustomerSales, 3);
            return "Görev\n3 VIP müşteriye satış yap: " + progress + "/3\nÖdül: 5 Elmas + 150 XP";
        }

        return "Görev\n\nYeni görev yok";
    }

    private bool CanClaimDailyReward()
    {
        if (string.IsNullOrEmpty(lastDailyRewardUtc) || !long.TryParse(lastDailyRewardUtc, out long ticks))
        {
            return true;
        }

        DateTime lastRewardTime = new DateTime(ticks, DateTimeKind.Utc);
        return (DateTime.UtcNow - lastRewardTime).TotalHours >= DailyRewardCooldownHours;
    }

    private bool CanClaimVipDailyReward()
    {
        if (vipLevel <= 0)
        {
            return false;
        }

        if (string.IsNullOrEmpty(lastVipDailyRewardUtc) || !long.TryParse(lastVipDailyRewardUtc, out long ticks))
        {
            return true;
        }

        DateTime lastRewardTime = new DateTime(ticks, DateTimeKind.Utc);
        return (DateTime.UtcNow - lastRewardTime).TotalHours >= DailyRewardCooldownHours;
    }

    private string GetDailyRewardText()
    {
        string text = "";
        for (int i = 0; i < DailyRewardCycleDays; i++)
        {
            int displayDay = i + 1;
            string marker = i == dailyRewardDay ? "Bugun: " : "";
            text += marker + "Gun " + displayDay + ": " + GetDailyRewardLine(i) + "\n";
        }

        if (CanClaimDailyReward())
        {
            return text + "\nHazir: Gun " + (dailyRewardDay + 1) + " odulunu alabilirsin.";
        }

        return text + "\nSiradaki odul: " + GetDailyRewardWaitText();
    }

    private void RefreshDailyRewardLiveOverlay()
    {
        bool canClaim = CanClaimDailyReward();
        int currentDay = Mathf.Clamp(dailyRewardDay, 0, DailyRewardCycleDays - 1);

        for (int i = 0; i < dailyRewardDayHighlightImages.Count; i++)
        {
            Image highlight = dailyRewardDayHighlightImages[i];
            if (highlight == null) continue;
            bool active = i == currentDay;
            highlight.gameObject.SetActive(active);
            if (active)
            {
                float pulse = 0.18f + Mathf.PingPong(Time.time * 1.6f, 0.10f);
                highlight.color = canClaim
                    ? new Color(1f, 0.82f, 0.22f, pulse)
                    : new Color(0.15f, 0.78f, 0.72f, pulse * 0.85f);
            }
        }

        if (dailyRewardClaimTextImage != null)
        {
            dailyRewardClaimTextImage.gameObject.SetActive(canClaim);
            if (canClaim)
            {
                dailyRewardClaimTextImage.sprite = dailyRewardClaimSprite;
            }
        }

        for (int i = 0; i < dailyRewardStepStatusImages.Count; i++)
        {
            Image statusImage = dailyRewardStepStatusImages[i];
            if (statusImage == null) continue;
            bool isLockedStatus = false;

            if (i < currentDay)
            {
                statusImage.sprite = dailyRewardStatusClaimedSprite;
            }
            else if (i == currentDay)
            {
                if (canClaim)
                {
                    statusImage.sprite = dailyRewardStatusAvailableSprite;
                }
                else
                {
                    statusImage.sprite = i == DailyRewardCycleDays - 1 ? dailyRewardStatusLockedLongSprite : dailyRewardStatusLockedSprite;
                    isLockedStatus = true;
                }
            }
            else
            {
                statusImage.sprite = i == DailyRewardCycleDays - 1 ? dailyRewardStatusLockedLongSprite : dailyRewardStatusLockedSprite;
                isLockedStatus = true;
            }

            RectTransform statusRect = statusImage.GetComponent<RectTransform>();
            if (statusRect != null && i < dailyRewardStepStatusReferencePositions.Count)
            {
                Vector2 referencePosition = dailyRewardStepStatusReferencePositions[i];
                if (isLockedStatus)
                {
                    referencePosition.y -= 5f;
                }
                statusRect.anchoredPosition = DailyRewardLayoutPosition(referencePosition);
            }

            statusImage.gameObject.SetActive(true);
        }

        for (int i = 0; i < dailyRewardStepStatusTexts.Count; i++)
        {
            TMP_Text statusText = dailyRewardStepStatusTexts[i];
            Image badgeImage = i < dailyRewardStepBadgeImages.Count ? dailyRewardStepBadgeImages[i] : null;
            if (statusText == null) continue;
            statusText.gameObject.SetActive(true);
            if (badgeImage != null) badgeImage.gameObject.SetActive(false);

            string status;
            Color textColor;

            if (i < currentDay)
            {
                status = "AL";
                textColor = new Color(1f, 0.96f, 0.72f, 1f);
            }
            else if (i == currentDay)
            {
                status = canClaim ? "BUGÜN" : "KİLİT";
                textColor = new Color(1f, 0.92f, 0.58f, 1f);
            }
            else
            {
                status = i == DailyRewardCycleDays - 1 ? "KİLİTLİ" : "KİLİT";
                textColor = new Color(0.86f, 0.72f, 0.52f, 1f);
            }

            statusText.text = status;
            statusText.color = textColor;
        }

        if (dailyRewardTodayAmountText != null)
        {
            dailyRewardTodayAmountText.text = GetDailyRewardShortLine(currentDay).ToUpperInvariant();
        }

        if (dailyRewardClaimStateText != null)
        {
            dailyRewardClaimStateText.text = canClaim ? "ÖDÜLÜ AL" : "";
            dailyRewardClaimStateText.color = canClaim ? new Color(1f, 0.86f, 0.48f, 1f) : new Color(0.75f, 0.62f, 0.44f, 1f);
        }

        if (dailyRewardTimerText != null)
        {
            dailyRewardTimerText.text = canClaim ? "Bugünün ödülü hazır" : "Sonraki ödül: " + GetDailyRewardWaitText();
        }
    }

    private string GetDailyRewardLine(int dayIndex)
    {
        int gemReward = GetDailyGemReward(dayIndex);
        int commonChestReward = GetDailyCommonChestReward(dayIndex);
        int rareChestReward = GetDailyRareChestReward(dayIndex);
        int luxuryChestReward = GetDailyLuxuryChestReward(dayIndex);
        string reward = gemReward + " Elmas";
        if (commonChestReward > 0) reward += " + " + commonChestReward + " Sıradan Sandık";
        if (rareChestReward > 0) reward += " + " + rareChestReward + " Nadir Sandık";
        if (luxuryChestReward > 0) reward += " + " + luxuryChestReward + " Özel Sandık";
        return reward;
    }

    private string GetDailyRewardShortLine(int dayIndex)
    {
        int gemReward = GetDailyGemReward(dayIndex);
        int commonChestReward = GetDailyCommonChestReward(dayIndex);
        int rareChestReward = GetDailyRareChestReward(dayIndex);
        int luxuryChestReward = GetDailyLuxuryChestReward(dayIndex);

        List<string> parts = new List<string>();
        if (gemReward > 0) parts.Add(gemReward + " Elmas");
        if (commonChestReward > 0) parts.Add(commonChestReward + " Sandık");
        if (rareChestReward > 0) parts.Add(rareChestReward + " Nadir");
        if (luxuryChestReward > 0) parts.Add(luxuryChestReward + " Özel");
        return string.Join("\n", parts.ToArray());
    }

    private string GetDailyRewardWaitText()
    {
        if (string.IsNullOrEmpty(lastDailyRewardUtc) || !long.TryParse(lastDailyRewardUtc, out long ticks))
        {
            return "hazir";
        }

        DateTime lastRewardTime = new DateTime(ticks, DateTimeKind.Utc);
        double remainingSeconds = (lastRewardTime.AddHours(DailyRewardCooldownHours) - DateTime.UtcNow).TotalSeconds;
        return FormatLongTime((float)Math.Max(0, remainingSeconds));
    }

    private int GetDailyGemReward(int dayIndex)
    {
        switch (dayIndex)
        {
            case 0: return 1;
            case 1: return 1;
            case 2: return 2;
            case 3: return 2;
            case 4: return 3;
            case 5: return 4;
            default: return 7;
        }
    }

    private int GetDailyCommonChestReward(int dayIndex)
    {
        switch (dayIndex)
        {
            case 1: return 1;
            case 3: return 1;
            case 4: return 1;
            default: return 0;
        }
    }

    private int GetDailyRareChestReward(int dayIndex)
    {
        return dayIndex == 5 ? 1 : 0;
    }

    private int GetDailyLuxuryChestReward(int dayIndex)
    {
        return dayIndex == 6 ? 1 : 0;
    }

    private string FormatTime(float seconds)
    {
        int totalSeconds = Mathf.CeilToInt(seconds);
        int minutes = totalSeconds / 60;
        int secs = totalSeconds % 60;
        return minutes + ":" + secs.ToString("00");
    }

    private string FormatCompactTime(float seconds)
    {
        int totalSeconds = Mathf.CeilToInt(seconds);
        int minutes = totalSeconds / 60;
        int secs = totalSeconds % 60;

        if (minutes > 0)
        {
            return minutes + "dk";
        }

        return secs + "sn";
    }

    private string FormatLongTime(float seconds)
    {
        int totalSeconds = Mathf.CeilToInt(seconds);
        int hours = totalSeconds / 3600;
        int minutes = (totalSeconds % 3600) / 60;
        int secs = totalSeconds % 60;

        if (hours > 0)
        {
            return hours + "s " + minutes + "dk";
        }

        return minutes + "dk " + secs + "sn";
    }

    private string SerializeItemInventory()
    {
        string serialized = "";
        foreach (KeyValuePair<string, int> entry in itemInventory)
        {
            if (entry.Value <= 0)
            {
                continue;
            }

            if (!string.IsNullOrEmpty(serialized))
            {
                serialized += "|";
            }

            serialized += entry.Key + ":" + entry.Value;
        }

        return serialized;
    }

    private void LoadItemInventory(string serialized)
    {
        itemInventory.Clear();
        if (string.IsNullOrEmpty(serialized))
        {
            return;
        }

        string[] entries = serialized.Split('|');
        for (int i = 0; i < entries.Length; i++)
        {
            string[] pair = entries[i].Split(':');
            if (pair.Length != 2)
            {
                continue;
            }

            if (string.IsNullOrEmpty(pair[0]) || !int.TryParse(pair[1], out int count) || count <= 0)
            {
                continue;
            }

            string itemId = pair[0] == "paint_can_marble_001" ? "paint_can_lavender_001" : pair[0];
            if (!itemInventory.ContainsKey(itemId))
            {
                itemInventory[itemId] = 0;
            }

            itemInventory[itemId] += count;
        }
    }

    private string SerializeWorkshopUnlockedProducts()
    {
        List<string> ids = new List<string>(unlockedWorkshopProducts);
        ids.Sort(StringComparer.Ordinal);
        return string.Join("|", ids.ToArray());
    }

    private void LoadWorkshopUnlockedProducts(string value)
    {
        unlockedWorkshopProducts.Clear();
        if (string.IsNullOrEmpty(value)) return;
        string[] ids = value.Split('|');
        for (int i = 0; i < ids.Length; i++)
        {
            if (GetWorkshopProduct(ids[i]) != null) unlockedWorkshopProducts.Add(ids[i]);
        }
    }

    private string SerializeWorkshopSelectedProducts()
    {
        List<string> entries = new List<string>();
        foreach (KeyValuePair<WorkshopDecorCategory, string> pair in selectedWorkshopProducts)
        {
            entries.Add(((int)pair.Key) + "=" + pair.Value);
        }
        entries.Sort(StringComparer.Ordinal);
        return string.Join("|", entries.ToArray());
    }

    private void LoadWorkshopSelectedProducts(string value)
    {
        selectedWorkshopProducts.Clear();
        if (string.IsNullOrEmpty(value)) return;
        string[] entries = value.Split('|');
        for (int i = 0; i < entries.Length; i++)
        {
            string[] pair = entries[i].Split('=');
            if (pair.Length != 2 || !int.TryParse(pair[0], out int categoryValue)) continue;
            if (!Enum.IsDefined(typeof(WorkshopDecorCategory), categoryValue)) continue;
            WorkshopDecorProduct product = GetWorkshopProduct(pair[1]);
            if (product == null || !unlockedWorkshopProducts.Contains(product.Id)) continue;
            WorkshopDecorCategory category = (WorkshopDecorCategory)categoryValue;
            if (product.Category == category) selectedWorkshopProducts[category] = product.Id;
        }
    }

    private void SaveGame()
    {
        PlayerPrefs.SetFloat("coins", coins);
        PlayerPrefs.SetInt("coinsPerClick", coinsPerClick);
        PlayerPrefs.SetFloat("coinsPerSecond", coinsPerSecond);
        PlayerPrefs.SetInt("coffeeUpgradeLevel", coffeeUpgradeLevel);
        PlayerPrefs.SetInt("coffeeUpgradeCost", coffeeUpgradeCost);
        PlayerPrefs.SetInt("premiumCoffeeLevel", premiumCoffeeLevel);
        PlayerPrefs.SetInt("premiumCoffeeCost", premiumCoffeeCost);
        PlayerPrefs.SetInt("signatureCoffeeLevel", signatureCoffeeLevel);
        PlayerPrefs.SetInt("signatureCoffeeCost", signatureCoffeeCost);
        PlayerPrefs.SetInt("machineUpgradeLevel", machineUpgradeLevel);
        PlayerPrefs.SetInt("machineUpgradeCost", machineUpgradeCost);
        PlayerPrefs.SetInt("espressoMachineLevel", espressoMachineLevel);
        PlayerPrefs.SetInt("espressoMachineCost", espressoMachineCost);
        PlayerPrefs.SetInt("premiumMachineLevel", premiumMachineLevel);
        PlayerPrefs.SetInt("premiumMachineCost", premiumMachineCost);
        PlayerPrefs.SetInt("baristaUpgradeLevel", baristaUpgradeLevel);
        PlayerPrefs.SetInt("baristaUpgradeCost", baristaUpgradeCost);
        PlayerPrefs.SetInt("seniorBaristaLevel", seniorBaristaLevel);
        PlayerPrefs.SetInt("seniorBaristaCost", seniorBaristaCost);
        PlayerPrefs.SetInt("masterBaristaLevel", masterBaristaLevel);
        PlayerPrefs.SetInt("masterBaristaCost", masterBaristaCost);
        for (int i = 0; i < foodUpgradeDatabase.Count; i++)
        {
            FoodUpgradeData data = foodUpgradeDatabase[i];
            PlayerPrefs.SetInt("foodUpgradeLevel_" + data.Id, data.Level);
        }
        PlayerPrefs.SetInt("gems", gems);
        PlayerPrefs.SetInt("chests", chests);
        PlayerPrefs.SetInt("rareChests", rareChests);
        PlayerPrefs.SetInt("luxuryChests", luxuryChests);
        PlayerPrefs.SetInt("paintPieces", paintPieces);
        PlayerPrefs.SetInt("woodPieces", woodPieces);
        PlayerPrefs.SetString("itemInventory", SerializeItemInventory());
        PlayerPrefs.SetString("workshopUnlockedProducts", SerializeWorkshopUnlockedProducts());
        PlayerPrefs.SetString("workshopSelectedProducts", SerializeWorkshopSelectedProducts());
        PlayerPrefs.SetString("lastChestRewardText", lastChestRewardText);
        PlayerPrefs.SetInt("pinkWallUnlocked", pinkWallUnlocked ? 1 : 0);
        PlayerPrefs.SetInt("mintWallUnlocked", mintWallUnlocked ? 1 : 0);
        PlayerPrefs.SetInt("creamFloorUnlocked", creamFloorUnlocked ? 1 : 0);
        PlayerPrefs.SetInt("roseTableUnlocked", roseTableUnlocked ? 1 : 0);
        PlayerPrefs.SetInt("selectedWallStyle", selectedWallStyle);
        PlayerPrefs.SetInt("selectedFloorStyle", selectedFloorStyle);
        PlayerPrefs.SetInt("selectedTableStyle", selectedTableStyle);
        PlayerPrefs.SetInt("currentRoomIndex", currentRoomIndex);
        PlayerPrefs.SetInt("productionRoomUnlocked", productionRoomUnlocked ? 1 : 0);
        PlayerPrefs.SetInt("vipRoomUnlocked", vipRoomUnlocked ? 1 : 0);
        PlayerPrefs.SetInt("vipLevel", vipLevel);
        PlayerPrefs.SetString("lastVipDailyRewardUtc", lastVipDailyRewardUtc);
        PlayerPrefs.SetInt("vipInfoSeen", vipInfoSeen ? 1 : 0);
        PlayerPrefs.SetInt("xp", xp);
        PlayerPrefs.SetInt("playerLevel", playerLevel);
        PlayerPrefs.SetInt("dailyRewardDay", dailyRewardDay);
        PlayerPrefs.SetString("lastDailyRewardUtc", lastDailyRewardUtc);
        PlayerPrefs.SetInt("totalCoffeeSold", totalCoffeeSold);
        PlayerPrefs.SetInt("customerSales", customerSales);
        PlayerPrefs.SetInt("dailyCoffeeSold", dailyCoffeeSold);
        PlayerPrefs.SetInt("dailyCustomerSales", dailyCustomerSales);
        PlayerPrefs.SetInt("dailyChestsOpened", dailyChestsOpened);
        PlayerPrefs.SetInt("dailyCoffeeRewardClaimed", dailyCoffeeRewardClaimed ? 1 : 0);
        PlayerPrefs.SetInt("dailyCustomerRewardClaimed", dailyCustomerRewardClaimed ? 1 : 0);
        PlayerPrefs.SetInt("dailyChestRewardClaimed", dailyChestRewardClaimed ? 1 : 0);
        PlayerPrefs.SetString("lastDailyTasksUtc", lastDailyTasksUtc);
        PlayerPrefs.SetInt("vipCustomerSales", vipCustomerSales);
        PlayerPrefs.SetInt("missedCustomers", missedCustomers);
        PlayerPrefs.SetInt("firstTaskRewardClaimed", firstTaskRewardClaimed ? 1 : 0);
        PlayerPrefs.SetInt("secondTaskRewardClaimed", secondTaskRewardClaimed ? 1 : 0);
        PlayerPrefs.SetInt("thirdTaskRewardClaimed", thirdTaskRewardClaimed ? 1 : 0);
        PlayerPrefs.SetInt("fourthTaskRewardClaimed", fourthTaskRewardClaimed ? 1 : 0);
        PlayerPrefs.SetInt("fifthTaskRewardClaimed", fifthTaskRewardClaimed ? 1 : 0);
        PlayerPrefs.SetInt("seenTaskId", seenTaskId);
        PlayerPrefs.SetFloat("bonusTimer", bonusTimer);
        PlayerPrefs.SetFloat("offerCooldownTimer", offerCooldownTimer);
        PlayerPrefs.SetFloat("offerVisibleTimer", offerVisibleTimer);
        PlayerPrefs.SetString("lastSaveUtc", DateTime.UtcNow.Ticks.ToString());
        PlayerPrefs.Save();
    }

    private void LoadGame()
    {
        coins = PlayerPrefs.GetFloat("coins", 0);
        coinsPerClick = PlayerPrefs.GetInt("coinsPerClick", 1);
        coinsPerSecond = PlayerPrefs.GetFloat("coinsPerSecond", 0);
        coffeeUpgradeLevel = PlayerPrefs.GetInt("coffeeUpgradeLevel", 0);
        coffeeUpgradeCost = PlayerPrefs.GetInt("coffeeUpgradeCost", 25);
        premiumCoffeeLevel = PlayerPrefs.GetInt("premiumCoffeeLevel", 0);
        premiumCoffeeCost = PlayerPrefs.GetInt("premiumCoffeeCost", 750);
        signatureCoffeeLevel = PlayerPrefs.GetInt("signatureCoffeeLevel", 0);
        signatureCoffeeCost = PlayerPrefs.GetInt("signatureCoffeeCost", 5000);
        machineUpgradeLevel = PlayerPrefs.GetInt("machineUpgradeLevel", 0);
        machineUpgradeCost = PlayerPrefs.GetInt("machineUpgradeCost", 100);
        espressoMachineLevel = PlayerPrefs.GetInt("espressoMachineLevel", 0);
        espressoMachineCost = PlayerPrefs.GetInt("espressoMachineCost", 1000);
        premiumMachineLevel = PlayerPrefs.GetInt("premiumMachineLevel", 0);
        premiumMachineCost = PlayerPrefs.GetInt("premiumMachineCost", 10000);
        baristaUpgradeLevel = PlayerPrefs.GetInt("baristaUpgradeLevel", 0);
        baristaUpgradeCost = PlayerPrefs.GetInt("baristaUpgradeCost", 250);
        seniorBaristaLevel = PlayerPrefs.GetInt("seniorBaristaLevel", 0);
        seniorBaristaCost = PlayerPrefs.GetInt("seniorBaristaCost", 2500);
        masterBaristaLevel = PlayerPrefs.GetInt("masterBaristaLevel", 0);
        masterBaristaCost = PlayerPrefs.GetInt("masterBaristaCost", 20000);
        for (int i = 0; i < foodUpgradeDatabase.Count; i++)
        {
            FoodUpgradeData data = foodUpgradeDatabase[i];
            data.Level = PlayerPrefs.GetInt("foodUpgradeLevel_" + data.Id, 0) > 0 ? 1 : 0;
        }
        gems = PlayerPrefs.GetInt("gems", 0);
        chests = PlayerPrefs.GetInt("chests", 0);
        rareChests = PlayerPrefs.GetInt("rareChests", 0);
        luxuryChests = PlayerPrefs.GetInt("luxuryChests", 0);
        paintPieces = PlayerPrefs.GetInt("paintPieces", PlayerPrefs.GetInt("decorPieces", 0));
        woodPieces = PlayerPrefs.GetInt("woodPieces", PlayerPrefs.GetInt("premiumDecorPieces", 0));
        LoadItemInventory(PlayerPrefs.GetString("itemInventory", ""));
        LoadWorkshopUnlockedProducts(PlayerPrefs.GetString("workshopUnlockedProducts", ""));
        LoadWorkshopSelectedProducts(PlayerPrefs.GetString("workshopSelectedProducts", ""));
        lastChestRewardText = PlayerPrefs.GetString("lastChestRewardText", "Henuz sandik acilmadi.");
        pinkWallUnlocked = PlayerPrefs.GetInt("pinkWallUnlocked", 0) == 1;
        mintWallUnlocked = PlayerPrefs.GetInt("mintWallUnlocked", 0) == 1;
        creamFloorUnlocked = PlayerPrefs.GetInt("creamFloorUnlocked", 0) == 1;
        roseTableUnlocked = PlayerPrefs.GetInt("roseTableUnlocked", 0) == 1;
        selectedWallStyle = PlayerPrefs.GetInt("selectedWallStyle", 0);
        selectedFloorStyle = PlayerPrefs.GetInt("selectedFloorStyle", 0);
        selectedTableStyle = PlayerPrefs.GetInt("selectedTableStyle", 0);
        currentRoomIndex = Mathf.Clamp(PlayerPrefs.GetInt("currentRoomIndex", 0), 0, RoomCount - 1);
        productionRoomUnlocked = PlayerPrefs.GetInt("productionRoomUnlocked", 0) == 1;
        vipRoomUnlocked = PlayerPrefs.GetInt("vipRoomUnlocked", 0) == 1;
        vipLevel = Mathf.Clamp(PlayerPrefs.GetInt("vipLevel", 0), 0, MaxVipLevel);
        lastVipDailyRewardUtc = PlayerPrefs.GetString("lastVipDailyRewardUtc", "");
        vipInfoSeen = PlayerPrefs.GetInt("vipInfoSeen", 0) == 1;
        if (selectedWallStyle == 1 && !pinkWallUnlocked) selectedWallStyle = 0;
        if (selectedWallStyle == 2 && !mintWallUnlocked) selectedWallStyle = 0;
        if (selectedFloorStyle == 1 && !creamFloorUnlocked) selectedFloorStyle = 0;
        if (selectedTableStyle == 1 && !roseTableUnlocked) selectedTableStyle = 0;
        xp = PlayerPrefs.GetInt("xp", 0);
        playerLevel = PlayerPrefs.GetInt("playerLevel", 1);
        dailyRewardDay = PlayerPrefs.GetInt("dailyRewardDay", 0);
        dailyRewardDay = Mathf.Clamp(dailyRewardDay, 0, DailyRewardCycleDays - 1);
        lastDailyRewardUtc = PlayerPrefs.GetString("lastDailyRewardUtc", "");
        totalCoffeeSold = PlayerPrefs.GetInt("totalCoffeeSold", 0);
        customerSales = PlayerPrefs.GetInt("customerSales", 0);
        dailyCoffeeSold = PlayerPrefs.GetInt("dailyCoffeeSold", 0);
        dailyCustomerSales = PlayerPrefs.GetInt("dailyCustomerSales", 0);
        dailyChestsOpened = PlayerPrefs.GetInt("dailyChestsOpened", 0);
        dailyCoffeeRewardClaimed = PlayerPrefs.GetInt("dailyCoffeeRewardClaimed", 0) == 1;
        dailyCustomerRewardClaimed = PlayerPrefs.GetInt("dailyCustomerRewardClaimed", 0) == 1;
        dailyChestRewardClaimed = PlayerPrefs.GetInt("dailyChestRewardClaimed", 0) == 1;
        lastDailyTasksUtc = PlayerPrefs.GetString("lastDailyTasksUtc", "");
        vipCustomerSales = PlayerPrefs.GetInt("vipCustomerSales", 0);
        missedCustomers = PlayerPrefs.GetInt("missedCustomers", 0);
        firstTaskRewardClaimed = PlayerPrefs.GetInt("firstTaskRewardClaimed", 0) == 1;
        secondTaskRewardClaimed = PlayerPrefs.GetInt("secondTaskRewardClaimed", 0) == 1;
        thirdTaskRewardClaimed = PlayerPrefs.GetInt("thirdTaskRewardClaimed", 0) == 1;
        fourthTaskRewardClaimed = PlayerPrefs.GetInt("fourthTaskRewardClaimed", 0) == 1;
        fifthTaskRewardClaimed = PlayerPrefs.GetInt("fifthTaskRewardClaimed", 0) == 1;
        seenTaskId = Mathf.Clamp(PlayerPrefs.GetInt("seenTaskId", 0), 0, 5);
        bonusTimer = PlayerPrefs.GetFloat("bonusTimer", 0);
        incomeMultiplier = bonusTimer > 0 ? 2 : 1;
        offerCooldownTimer = PlayerPrefs.GetFloat("offerCooldownTimer", 10f);
        offerVisibleTimer = PlayerPrefs.GetFloat("offerVisibleTimer", 0);
    }

    private void ApplyOfflineEarnings()
    {
        if (!HasBaristaUnlocked())
        {
            return;
        }

        string lastSaveValue = PlayerPrefs.GetString("lastSaveUtc", "");
        if (string.IsNullOrEmpty(lastSaveValue) || !long.TryParse(lastSaveValue, out long ticks))
        {
            return;
        }

        DateTime lastSaveTime = new DateTime(ticks, DateTimeKind.Utc);
        double elapsedSeconds = Math.Max(0, (DateTime.UtcNow - lastSaveTime).TotalSeconds);
        double cappedSeconds = Math.Min(elapsedSeconds, MaxOfflineEarningsSeconds);
        float offlineIncomePerSecond = GetOfflinePassiveIncomePerSecond();
        int offlineCoins = Mathf.FloorToInt(offlineIncomePerSecond * (float)cappedSeconds);

        if (offlineCoins <= 0)
        {
            return;
        }

        lastOfflineCoins = offlineCoins;
        PlayerPrefs.SetInt("lastOfflineCoins", offlineCoins);
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveGame();
            return;
        }

        ApplyOfflineEarnings();
        UpdateUI();
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }
}

public class FloatingWindowDrag : MonoBehaviour, IBeginDragHandler, IDragHandler
{
    public RectTransform target;

    private Vector2 pointerStartPosition;
    private Vector2 panelStartPosition;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (target == null)
        {
            return;
        }

        RectTransform parentRect = target.parent as RectTransform;
        if (parentRect == null)
        {
            return;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, eventData.position, eventData.pressEventCamera, out pointerStartPosition);
        panelStartPosition = target.anchoredPosition;
        target.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (target == null)
        {
            return;
        }

        RectTransform parentRect = target.parent as RectTransform;
        if (parentRect == null)
        {
            return;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, eventData.position, eventData.pressEventCamera, out Vector2 currentPointerPosition);
        target.anchoredPosition = panelStartPosition + (currentPointerPosition - pointerStartPosition);
    }
}

public class IsoQuadGraphic : MaskableGraphic
{
    private Vector2 topLeft;
    private Vector2 topRight;
    private Vector2 bottomRight;
    private Vector2 bottomLeft;

    public void SetCorners(Vector2 newTopLeft, Vector2 newTopRight, Vector2 newBottomRight, Vector2 newBottomLeft)
    {
        topLeft = newTopLeft;
        topRight = newTopRight;
        bottomRight = newBottomRight;
        bottomLeft = newBottomLeft;
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = color;

        vertex.position = topLeft;
        vh.AddVert(vertex);

        vertex.position = topRight;
        vh.AddVert(vertex);

        vertex.position = bottomRight;
        vh.AddVert(vertex);

        vertex.position = bottomLeft;
        vh.AddVert(vertex);

        vh.AddTriangle(0, 1, 2);
        vh.AddTriangle(2, 3, 0);
    }
}

public class TextureQuadGraphic : MaskableGraphic
{
    private Vector2 topLeft;
    private Vector2 topRight;
    private Vector2 bottomRight;
    private Vector2 bottomLeft;
    private Sprite sprite;

    public override Texture mainTexture
    {
        get
        {
            if (sprite != null && sprite.texture != null)
            {
                return sprite.texture;
            }

            return s_WhiteTexture;
        }
    }

    public void SetCorners(Vector2 newTopLeft, Vector2 newTopRight, Vector2 newBottomRight, Vector2 newBottomLeft)
    {
        topLeft = newTopLeft;
        topRight = newTopRight;
        bottomRight = newBottomRight;
        bottomLeft = newBottomLeft;
        SetVerticesDirty();
    }

    public void SetSprite(Sprite newSprite)
    {
        if (sprite == newSprite)
        {
            return;
        }

        sprite = newSprite;
        SetMaterialDirty();
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        Rect uvRect = new Rect(0f, 0f, 1f, 1f);
        if (sprite != null && sprite.texture != null)
        {
            Rect textureRect = sprite.textureRect;
            Texture2D texture = sprite.texture;
            uvRect = new Rect(
                textureRect.x / texture.width,
                textureRect.y / texture.height,
                textureRect.width / texture.width,
                textureRect.height / texture.height);
        }

        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = color;

        vertex.position = topLeft;
        vertex.uv0 = new Vector2(uvRect.xMin, uvRect.yMax);
        vh.AddVert(vertex);

        vertex.position = topRight;
        vertex.uv0 = new Vector2(uvRect.xMax, uvRect.yMax);
        vh.AddVert(vertex);

        vertex.position = bottomRight;
        vertex.uv0 = new Vector2(uvRect.xMax, uvRect.yMin);
        vh.AddVert(vertex);

        vertex.position = bottomLeft;
        vertex.uv0 = new Vector2(uvRect.xMin, uvRect.yMin);
        vh.AddVert(vertex);

        vh.AddTriangle(0, 1, 2);
        vh.AddTriangle(2, 3, 0);
    }
}

public class CurrencyBarPartSelector : MonoBehaviour
{
    private GameObject parentBar;

    public void Bind(GameObject barObject)
    {
        parentBar = barObject;
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (parentBar != null && Selection.activeGameObject == gameObject)
        {
            Selection.activeGameObject = parentBar;
        }
#endif
    }
}

public class PressAnimator : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerClickHandler
{
    private RectTransform target;
    private Vector2 restPosition;
    private Vector2 pressedOffset;
    private Vector2 pressedPosition;
    private Vector3 restScale = Vector3.one;
    private Vector3 pressedScale = Vector3.one;
    private bool isPressed;
    private bool clickPulse;
    private float clickPulseEndTime;

    public void Bind(RectTransform newTarget, Vector2 pressedOffset, float newPressedScale)
    {
        target = newTarget;
        restPosition = target.anchoredPosition;
        this.pressedOffset = pressedOffset;
        pressedPosition = restPosition + this.pressedOffset;
        restScale = target.localScale;
        pressedScale = restScale * newPressedScale;
    }

    public void CaptureCurrentAsRestPosition()
    {
        if (target == null)
        {
            return;
        }

        restPosition = target.anchoredPosition;
        pressedPosition = restPosition + pressedOffset;
        restScale = target.localScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        clickPulse = false;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPressed = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        clickPulse = true;
        clickPulseEndTime = Time.unscaledTime + 0.12f;
    }

    private void Update()
    {
        if (target == null)
        {
            return;
        }

        if (clickPulse && Time.unscaledTime >= clickPulseEndTime)
        {
            clickPulse = false;
        }

        float pulse = clickPulse ? 0.018f * Mathf.Sin((clickPulseEndTime - Time.unscaledTime) * 70f) : 0f;
        Vector2 targetPosition = isPressed ? pressedPosition : restPosition;
        Vector3 targetScale = isPressed ? pressedScale : restScale * (1f + Mathf.Max(0f, pulse));
        float t = Mathf.Clamp01(Time.unscaledDeltaTime * 18f);

        target.anchoredPosition = Vector2.Lerp(target.anchoredPosition, targetPosition, t);
        target.localScale = Vector3.Lerp(target.localScale, targetScale, t);
    }
}

#if UNITY_EDITOR
public class EditorDragProbe : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform target;
    private RectTransform parentRect;
    private PressAnimator pressAnimator;
    private Vector2 pointerStartPosition;
    private Vector2 startPosition;
    private string probeLabel;

    public void Bind(RectTransform rectTransform, string label)
    {
        target = rectTransform;
        parentRect = target != null ? target.parent as RectTransform : null;
        pressAnimator = target != null ? target.GetComponent<PressAnimator>() : null;
        probeLabel = label;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (target == null || parentRect == null)
        {
            return;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, eventData.position, eventData.pressEventCamera, out pointerStartPosition);
        startPosition = target.anchoredPosition;
        if (pressAnimator != null)
        {
            pressAnimator.enabled = false;
        }

        target.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (target == null || parentRect == null)
        {
            return;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, eventData.position, eventData.pressEventCamera, out Vector2 currentPointerPosition);
        target.anchoredPosition = startPosition + (currentPointerPosition - pointerStartPosition);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (target == null)
        {
            return;
        }

        Vector2 pos = target.anchoredPosition;
        Vector2 size = target.sizeDelta;
        Debug.Log(probeLabel + " position: new Vector2(" + pos.x.ToString("0.####") + "f, " + pos.y.ToString("0.####") + "f), size: new Vector2(" + size.x.ToString("0.####") + "f, " + size.y.ToString("0.####") + "f)");
        if (pressAnimator != null)
        {
            pressAnimator.CaptureCurrentAsRestPosition();
            pressAnimator.enabled = true;
        }
    }
}
#endif
