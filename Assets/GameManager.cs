using System;
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
    private static readonly Vector2 VipUpgradeButtonTextPosition = new Vector2(-27f, -9f);
    private static readonly Vector2 VipUpgradeButtonTextSize = new Vector2(104f, 62f);
    private static readonly Vector2 VipUpgradeGemIconPosition = new Vector2(49f, -9f);
    private static readonly Vector2 VipUpgradeGemIconSize = new Vector2(42f, 42f);
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

    private TMP_Text coinText;
    private TMP_Text coinInfoPopupText;
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
    private TMP_Text chestPanelText;
    private TMP_Text chestRewardTitleText;
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
    private TMP_Text foodUpgradeButtonText;
    private TMP_Text foodTotalPassiveText;
    private TMP_Text foodTotalBonusText;
    private Image vipUpgradeGemIconImage;
    private TMP_FontAsset vipPaytoneFontAsset;
    private readonly List<TMP_Text> vipLevelCardTexts = new List<TMP_Text>();
    private readonly List<Image> vipLevelCardImages = new List<Image>();
    private readonly List<Image> vipLevelLockImages = new List<Image>();
    private readonly List<Image> vipLevelSelectImages = new List<Image>();
    private readonly List<Image> vipLevelBottomStarImages = new List<Image>();
    private readonly List<RectTransform> vipLevelBottomStarRects = new List<RectTransform>();
    private readonly List<RectTransform> vipLevelButtonRects = new List<RectTransform>();
    private readonly Dictionary<int, Sprite> vipCardSprites = new Dictionary<int, Sprite>();
    private readonly Dictionary<int, Sprite> vipCardBottomStarSprites = new Dictionary<int, Sprite>();
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
    private Image chestRewardIconImage;
    private Image xpFillImage;
    private Sprite commonPaintCanSprite;
    private Sprite rarePaintCanSprite;
    private Sprite specialPaintCanSprite;
    private readonly Dictionary<string, Sprite> paintCanSprites = new Dictionary<string, Sprite>();
    private readonly Dictionary<string, Sprite> decorIconSprites = new Dictionary<string, Sprite>();
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
    private Sprite foodLevelBadgeSprite;
    private Sprite foodCoinChipSprite;
    private Sprite foodPassiveChipSprite;
    private Sprite foodCloseButtonSprite;
    private Sprite warmWoodCafeSceneSprite;
    private Sprite warmWoodCafeSceneNoMachineSprite;
    private Sprite coffeeMachineSceneSprite;
    private Sprite coffeeMachineLockedPlaceholderSprite;
    private Sprite baristaSceneSprite;
    private Sprite baristaLockedPlaceholderSprite;
    private Sprite baristaCounterForegroundSprite;
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
    private int previewDecorItem = 1;
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
                customerSpawnTimer = CustomerSpawnSeconds;
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
        SellCoffeeToCustomer();
    }

    private bool HasBaristaUnlocked()
    {
        return GetTotalBaristaLevel() > 0;
    }

    private float GetBaristaAutoServeSeconds()
    {
        int totalBaristaLevel = Mathf.Clamp(GetTotalBaristaLevel(), 1, UpgradeMaxLevel * 3);
        float speedRatio = Mathf.Clamp01((totalBaristaLevel - 1) / 18f);
        return Mathf.Lerp(BaseBaristaAutoServeSeconds, FastBaristaAutoServeSeconds, speedRatio);
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

        if (roll < 10)
        {
            currentCustomerName = "VIP Müşteri";
            currentCustomerCoinMultiplier = VipCustomerCoinMultiplier;
            currentCustomerXpReward = VipCustomerXpReward;
            currentCustomerColor = new Color(0.78f, 0.50f, 0.08f);
            customerVisibleTimer = 8f * GetVipCustomerPatienceMultiplier();
        }
        else if (roll < 30)
        {
            currentCustomerName = "Aceleci Musteri";
            currentCustomerCoinMultiplier = RushCustomerCoinMultiplier;
            currentCustomerXpReward = RushCustomerXpReward;
            currentCustomerColor = new Color(0.82f, 0.28f, 0.18f);
            customerVisibleTimer = 6f * GetVipCustomerPatienceMultiplier();
        }
        else
        {
            currentCustomerName = "Musteri";
            currentCustomerCoinMultiplier = NormalCustomerCoinMultiplier;
            currentCustomerXpReward = NormalCustomerXpReward;
            currentCustomerColor = new Color(0.18f, 0.58f, 0.32f);
            customerVisibleTimer = CustomerVisibleSeconds * GetVipCustomerPatienceMultiplier();
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
        SaveGame();
    }

    public void SellCoffeeToCustomer()
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
        customerSpawnTimer = CustomerSpawnSeconds;
        if (customerButtonObject != null)
        {
            customerButtonObject.SetActive(false);
        }
        UpdateUI();
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
        Debug.Log("VipLeftClaimButton clicked");
    }

    public void UpgradeVipLevel()
    {
        if (vipLevel >= MaxVipLevel)
        {
            return;
        }

        int cost = GetVipUpgradeCost();
        if (gems < cost)
        {
            return;
        }

        gems -= cost;
        vipLevel++;
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
        gems = 0;
        chests = 0;
        rareChests = 0;
        luxuryChests = 0;
        paintPieces = 0;
        woodPieces = 0;
        itemInventory.Clear();
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
        amount = Mathf.CeilToInt(amount * GetVipXpMultiplier());
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
        return Mathf.CeilToInt(coinsPerClick * GetFoodSalesMultiplier() * incomeMultiplier * currentCustomerCoinMultiplier * GetVipCoinMultiplier());
    }

    private int GetCurrentTapPower()
    {
        return Mathf.CeilToInt(coinsPerClick * GetFoodSalesMultiplier() * incomeMultiplier * GetVipCoinMultiplier());
    }

    private float GetCurrentPassiveIncome()
    {
        return coinsPerSecond * GetFoodPassiveMultiplier() * incomeMultiplier * GetVipPassiveIncomeMultiplier();
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

    private string GetCoinInfoPopupText()
    {
        string boostText = bonusTimer > 0 ? "Aktif (" + FormatCompactTime(bonusTimer) + ")" : "Kapalı";
        string vipText = vipLevel > 0
            ? "+" + GetVipCoinBonusPercent(vipLevel) + "% coin, +" + GetVipPassiveBonusPercent(vipLevel) + "% pasif"
            : "Yok";

        return "Kahve Ücreti  +" + GetCurrentTapPower() + " coin"
            + "\nPasif Gelir  " + GetCurrentPassiveIncome().ToString("0.#") + " / sn"
            + "\nVIP Etkisi  " + vipText
            + "\n2x Reklam  " + boostText;
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
        CreateCoinInfoPopup(gameFrame, new Vector2(-245, 399));
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
        panelRect.anchoredPosition = new Vector2(0, 40);
        panelRect.sizeDelta = new Vector2(860, 390);

        StylePopupPanel(offlinePanel, panelRect.sizeDelta, new Color(1f, 0.94f, 0.78f, 0.98f), new Color(0.56f, 0.30f, 0.10f, 1f));

        offlineText = CreateText(offlinePanel.transform, "OfflineText", "", new Vector2(0, 82), new Vector2(760, 170), 34);
        StylePopupBodyText(offlineText, 22, 34, TextAlignmentOptions.Center);

        StylePopupButton(CreateButton(offlinePanel.transform, "OfflineCollectButton", "Topla", new Vector2(-220, -118), new Vector2(360, 92), CloseOfflinePanel, new Color(0.20f, 0.55f, 0.36f), 31), new Color(0.20f, 0.55f, 0.36f), 31);
        StylePopupButton(CreateButton(offlinePanel.transform, "OfflineDoubleButton", "2x Topla", new Vector2(220, -118), new Vector2(360, 92), CollectOfflineDouble, new Color(0.18f, 0.38f, 0.82f), 31), new Color(0.18f, 0.38f, 0.82f), 31);
        offlinePanel.transform.SetAsLastSibling();
        offlinePanel.SetActive(false);
    }

    public void OpenDailyRewardPanel()
    {
        if (dailyRewardPanel == null)
        {
            return;
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
    }

    public void ClaimDailyReward()
    {
        if (!CanClaimDailyReward())
        {
            return;
        }

        int gemReward = GetDailyGemReward(dailyRewardDay);
        int chestReward = GetDailyChestReward(dailyRewardDay);

        gems += gemReward;
        chests += chestReward;
        lastDailyRewardUtc = DateTime.UtcNow.Ticks.ToString();
        dailyRewardDay = (dailyRewardDay + 1) % DailyRewardCycleDays;

        if (dailyRewardPanel != null)
        {
            dailyRewardPanel.SetActive(false);
        }

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
        dailyRewardPanel = new GameObject("DailyRewardPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        dailyRewardPanel.transform.SetParent(parent, false);

        RectTransform panelRect = dailyRewardPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = new Vector2(0, 25);
        panelRect.sizeDelta = new Vector2(900, 650);

        StylePopupPanel(dailyRewardPanel, panelRect.sizeDelta, new Color(1f, 0.92f, 0.78f, 0.98f), new Color(0.72f, 0.36f, 0.16f, 1f));

        TMP_Text title = CreateText(dailyRewardPanel.transform, "DailyRewardTitle", "Günlük Giriş Ödülü", new Vector2(0, 270), new Vector2(820, 70), 42);
        StylePopupTitle(title, 34);

        dailyRewardPanelText = CreateText(dailyRewardPanel.transform, "DailyRewardText", "", new Vector2(0, 65), new Vector2(790, 330), 28);
        StylePopupBodyText(dailyRewardPanelText, 19, 27, TextAlignmentOptions.TopLeft);

        dailyRewardButtonText = StylePopupButton(CreateButton(dailyRewardPanel.transform, "DailyRewardClaimButton", "Ödülü Al", new Vector2(-190, -225), new Vector2(360, 82), ClaimDailyReward, new Color(0.84f, 0.38f, 0.22f), 30), new Color(0.84f, 0.38f, 0.22f), 30);
        StylePopupButton(CreateButton(dailyRewardPanel.transform, "DailyRewardCloseButton", "Kapat", new Vector2(230, -225), new Vector2(300, 82), CloseDailyRewardPanel, new Color(0.20f, 0.20f, 0.20f), 28), new Color(0.20f, 0.20f, 0.20f), 28);

        dailyRewardPanel.transform.SetAsLastSibling();
        dailyRewardPanel.SetActive(false);
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
        if (roll < 45)
        {
            gems += 1;
            lastChestRewardText = "Sıradan Sandıktan 1 Elmas çıktı.";
            ShowChestReward(lastChestRewardText, GetGemIconSprite());
        }
        else if (roll < 70)
        {
            woodPieces += 1;
            lastChestRewardText = "Sıradan Sandıktan 1 Tahta çıktı.";
            ShowChestReward(lastChestRewardText, GetWoodIconSprite());
        }
        else
        {
            AddRandomPaintCanReward(ItemRarity.Common, "Siradan Sandik");
        }
    }

    private void OpenRareChestReward(int roll)
    {
        if (roll < 35)
        {
            gems += 2;
            lastChestRewardText = "Nadir Sandıktan 2 Elmas çıktı.";
            ShowChestReward(lastChestRewardText, GetGemIconSprite());
        }
        else if (roll < 55)
        {
            woodPieces += 2;
            lastChestRewardText = "Nadir Sandıktan 2 Tahta çıktı.";
            ShowChestReward(lastChestRewardText, GetWoodIconSprite());
        }
        else
        {
            AddRandomPaintCanReward(ItemRarity.Rare, "Nadir Sandik");
        }
    }

    private void OpenLuxuryChestReward(int roll)
    {
        if (roll < 25)
        {
            gems += 5;
            lastChestRewardText = "Özel Sandıktan 5 Elmas çıktı.";
            ShowChestReward(lastChestRewardText, GetGemIconSprite());
        }
        else if (roll < 45)
        {
            woodPieces += 3;
            gems += 2;
            lastChestRewardText = "Özel Sandıktan 3 Tahta + 2 Elmas çıktı.";
            ShowChestReward(lastChestRewardText, GetWoodIconSprite());
        }
        else
        {
            AddRandomPaintCanReward(ItemRarity.Luxury, "Ozel Sandik");
        }
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

    private void CreateChestPanel(Transform parent)
    {
        chestPanel = new GameObject("ChestPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        chestPanel.transform.SetParent(parent, false);

        RectTransform panelRect = chestPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = new Vector2(0, 25);
        panelRect.sizeDelta = new Vector2(980, 620);

        StylePopupPanel(chestPanel, panelRect.sizeDelta, new Color(0.98f, 0.89f, 0.70f, 0.98f), new Color(0.58f, 0.32f, 0.12f, 1f));

        TMP_Text title = CreateText(chestPanel.transform, "ChestPanelTitle", "Sandiklar", new Vector2(0, 260), new Vector2(900, 64), 38);
        StylePopupTitle(title, 34);

        chestPanelText = CreateText(chestPanel.transform, "ChestPanelText", "", new Vector2(0, 45), new Vector2(850, 330), 20);
        StylePopupBodyText(chestPanelText, 14, 20, TextAlignmentOptions.TopLeft);

        chestOpenButtonText = StylePopupButton(CreateButton(chestPanel.transform, "ChestOpenButton", "Siradan", new Vector2(-330, -250), new Vector2(190, 70), OpenChest, new Color(0.64f, 0.36f, 0.16f), 22), new Color(0.64f, 0.36f, 0.16f), 22);
        rareChestOpenButtonText = StylePopupButton(CreateButton(chestPanel.transform, "RareChestOpenButton", "Nadir", new Vector2(-110, -250), new Vector2(190, 70), OpenRareChest, new Color(0.30f, 0.42f, 0.68f), 22), new Color(0.30f, 0.42f, 0.68f), 22);
        luxuryChestOpenButtonText = StylePopupButton(CreateButton(chestPanel.transform, "LuxuryChestOpenButton", "Ozel", new Vector2(110, -250), new Vector2(190, 70), OpenLuxuryChest, new Color(0.72f, 0.54f, 0.22f), 22), new Color(0.72f, 0.54f, 0.22f), 22);
        StylePopupButton(CreateButton(chestPanel.transform, "ChestCloseButton", "Kapat", new Vector2(330, -250), new Vector2(190, 70), CloseChestPanel, new Color(0.20f, 0.20f, 0.20f), 22), new Color(0.20f, 0.20f, 0.20f), 22);

        chestPanel.transform.SetAsLastSibling();
        chestPanel.SetActive(false);
    }

    private void CreateChestRewardPanel(Transform parent)
    {
        chestRewardPanel = new GameObject("ChestRewardPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        chestRewardPanel.transform.SetParent(parent, false);

        RectTransform panelRect = chestRewardPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = new Vector2(0, 15);
        panelRect.sizeDelta = new Vector2(560, 500);

        StylePopupPanel(chestRewardPanel, panelRect.sizeDelta, new Color(0.98f, 0.92f, 0.78f, 0.99f), new Color(0.62f, 0.42f, 0.16f, 1f));

        chestRewardTitleText = CreateText(chestRewardPanel.transform, "ChestRewardTitle", "Sandık Ödülü", new Vector2(0, 205), new Vector2(500, 60), 38);
        StylePopupTitle(chestRewardTitleText, 30);

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

        chestRewardText = CreateText(chestRewardPanel.transform, "ChestRewardText", "", new Vector2(0, -105), new Vector2(500, 110), 28);
        StylePopupBodyText(chestRewardText, 20, 27, TextAlignmentOptions.Center);

        StylePopupButton(CreateButton(chestRewardPanel.transform, "ChestRewardCloseButton", "Tamam", new Vector2(0, -210), new Vector2(260, 70), CloseChestRewardPanel, new Color(0.22f, 0.48f, 0.40f), 27), new Color(0.22f, 0.48f, 0.40f), 27);

        chestRewardPanel.transform.SetAsLastSibling();
        chestRewardPanel.SetActive(false);
    }

    public void CloseChestRewardPanel()
    {
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

        chestRewardPanel.SetActive(true);
        chestRewardPanel.transform.SetAsLastSibling();
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

    private Sprite GetWoodIconSprite()
    {
        if (woodIconSprite == null)
        {
            woodIconSprite = LoadSpriteFromResources("Art/Items/wood_bundle_001");
        }

        return woodIconSprite;
    }

    private Sprite GetCraftPaintIconSprite()
    {
        if (craftPaintIconSprite == null)
        {
            craftPaintIconSprite = LoadSpriteFromResources("Art/Items/paint_can_white_001");
        }

        return craftPaintIconSprite;
    }

    private Sprite GetDecorIconSprite(DecorItemData item)
    {
        if (item == null || string.IsNullOrEmpty(item.ItemId))
        {
            return null;
        }

        if (!decorIconSprites.ContainsKey(item.ItemId))
        {
            decorIconSprites[item.ItemId] = LoadSpriteFromResources("Art/DecorIcons/" + item.ItemId);
        }

        return decorIconSprites[item.ItemId];
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
            foodPanelBackgroundSprite = LoadImportedSpriteFromResources("Art/UI/Food/food_upgrade_panel_background");
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
            foodCategoryTabSelectedSprite = LoadImportedSpriteFromResources("Art/UI/Food/food_category_tab_selected");
        return foodCategoryTabSelectedSprite;
    }

    private Sprite GetFoodCategoryTabNormalSprite()
    {
        if (foodCategoryTabNormalSprite == null)
            foodCategoryTabNormalSprite = LoadImportedSpriteFromResources("Art/UI/Food/food_category_tab_normal");
        return foodCategoryTabNormalSprite;
    }

    private Sprite GetFoodLevelBadgeSprite()
    {
        if (foodLevelBadgeSprite == null)
            foodLevelBadgeSprite = LoadImportedSpriteFromResources("Art/UI/Food/food_level_badge");
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
            foodCloseButtonSprite = LoadImportedSpriteFromResources("Art/UI/Food/food_close_button");
        return foodCloseButtonSprite;
    }

    private Sprite GetVipCardSprite(int level)
    {
        int safeLevel = Mathf.Clamp(level, 1, MaxVipLevel);
        if (!vipCardSprites.ContainsKey(safeLevel))
        {
            vipCardSprites[safeLevel] = LoadSpriteFromResources("Art/UI/vip_card_" + safeLevel);
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

        if (previewDecorItem <= 0)
        {
            previewDecorItem = 1;
        }

        decorPanel.SetActive(true);
        decorPanel.transform.SetAsLastSibling();
        UpdateUI();
    }

    public void CloseDecorPanel()
    {
        if (decorPanel != null)
        {
            decorPanel.SetActive(false);
        }

        previewDecorItem = 0;
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
        DecorItemData item = GetPreviewDecorItem();
        UnlockOrSelectDecorItem(item);
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
        TMP_Text futureText = CreateText(contentObject.transform, "FutureTestText", "Yeni testler buraya eklenecek", new Vector2(0, -465), new Vector2(440, 70), 22);
        futureText.color = Color.black;

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
#if UNITY_EDITOR
        AttachEditorDragProbe(panelRect, "FoodUpgradePanel");
#endif

        TMP_Text title = CreateText(foodPanel.transform, "FoodPanelTitle", "Gıda Yükseltmeleri", new Vector2(0, 407), new Vector2(650, 72), 48);
        StyleFoodTitle(title, 48);

        TMP_Text closeText = CreateButton(foodPanel.transform, "FoodPanelCloseButton", "X", new Vector2(752, 407), new Vector2(72, 72), CloseFoodUpgradePanel, new Color(0.78f, 0.19f, 0.12f), 34);
        StyleFoodButton(closeText, new Color(0.78f, 0.19f, 0.12f), 34);
        Image closeImage = closeText.GetComponentInParent<Image>();
        if (closeImage != null)
        {
            closeImage.sprite = null;
            closeImage.color = Color.clear;
            closeText.text = "X";
            closeText.color = new Color(1f, 0.91f, 0.62f);
            closeText.outlineColor = new Color(0.24f, 0.08f, 0.02f, 1f);
            closeText.outlineWidth = 0.18f;
        }

        CreateFoodCategoryTab("FoodTabCoffee", "Kahve", FoodUpgradeCategory.Coffee, new Vector2(-686, 264), true);
        CreateFoodCategoryTab("FoodTabSweet", "Tatlı", FoodUpgradeCategory.Sweet, new Vector2(-686, 106), false);
        CreateFoodCategoryTab("FoodTabSnack", "Atıştırmalık", FoodUpgradeCategory.Snack, new Vector2(-686, -70), false);
        CreateFoodCategoryTab("FoodTabCold", "Soğuk", FoodUpgradeCategory.Cold, new Vector2(-686, -247), false);

        GameObject viewportObject = new GameObject("FoodCardsViewport", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(RectMask2D));
        viewportObject.transform.SetParent(foodPanel.transform, false);
        RectTransform viewportRect = viewportObject.GetComponent<RectTransform>();
        viewportRect.anchorMin = new Vector2(0.5f, 0.5f);
        viewportRect.anchorMax = new Vector2(0.5f, 0.5f);
        viewportRect.anchoredPosition = new Vector2(90f, 5f);
        viewportRect.sizeDelta = new Vector2(1320f, 650f);
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
        cardsGrid.childAlignment = TextAnchor.UpperCenter;
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

        CreateFoodSummaryBox("FoodTotalPassiveBox", "Pasif Gelir", new Vector2(-264, -402), true);
        CreateFoodSummaryBox("FoodTotalBonusBox", "Satış Geliri", new Vector2(340, -402), false);

        RefreshFoodCategoryLayout();

        foodPanel.transform.SetAsLastSibling();
        foodPanel.SetActive(false);
    }

    private void CreateFoodCategoryTab(string objectName, string label, FoodUpgradeCategory category, Vector2 position, bool selected)
    {
        TMP_Text tabText = CreateButton(foodPanel.transform, objectName, label, position, new Vector2(180, 116), delegate { SelectFoodCategory(category); }, Color.clear, 26);
        foodCategoryTabTexts[category] = tabText;
        Image tabImage = tabText.GetComponentInParent<Image>();
        RectTransform tabRect = tabText.GetComponentInParent<RectTransform>();
        if (tabImage != null && tabRect != null)
        {
            tabImage.sprite = null;
            tabImage.color = Color.clear;
            tabImage.preserveAspect = false;
#if UNITY_EDITOR
            AttachEditorDragProbe(tabRect, objectName);
#endif
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
            pair.Value.color = selected ? new Color(0.20f, 0.10f, 0.03f) : new Color(1f, 0.88f, 0.62f);
            pair.Value.fontSizeMax = selected ? 28f : 24f;
        }
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
#if UNITY_EDITOR
        AttachEditorDragProbe(cardRect, "FoodCard_" + data.Id);
#endif

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
        ui.LevelText.fontSizeMin = 8;
        ui.LevelText.fontSizeMax = 10;
        ui.LevelText.outlineColor = new Color(0.14f, 0.07f, 0.03f, 0.9f);
        ui.LevelText.outlineWidth = 0.12f;
        ApplyVipFont(ui.LevelText);

        TMP_Text levelLabel = CreateText(cardObject.transform, "LevelLabel", "Gerekli Seviye", new Vector2(17.99998f, -30.04402f), new Vector2(85.427f, 22.5021f), 13);
        StyleFoodCardText(levelLabel, 9, 13, TextAlignmentOptions.Center);
        ui.LevelLabelText = levelLabel;

        if (data.Id == "donut")
        {
            levelLabel.rectTransform.anchoredPosition = new Vector2(11.714f, -26.024f);
            levelLabel.rectTransform.sizeDelta = new Vector2(85.427f, 22.5021f);
            levelRow.rectTransform.anchoredPosition = new Vector2(9.815f, -26.4f);
            levelRow.rectTransform.sizeDelta = new Vector2(100.57f, 21.959f);
        }

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
#if UNITY_EDITOR
        AttachEditorDragProbe(rect, "FoodIcon_" + data.Id);
#endif
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
#if UNITY_EDITOR
        AttachEditorDragProbe(rect, objectName);
#endif
        return image;
    }

    private void CreateFoodSummaryBox(string objectName, string label, Vector2 position, bool passive)
    {
        GameObject boxObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        boxObject.transform.SetParent(foodPanel.transform, false);

        RectTransform rect = boxObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(485, 78);

        Image image = boxObject.GetComponent<Image>();
        image.sprite = null;
        image.color = Color.clear;

        TMP_Text labelText = CreateText(boxObject.transform, "Label", label, new Vector2(-75, 0), new Vector2(190, 42), 24);
        StyleFoodCardText(labelText, 16, 24, TextAlignmentOptions.Center);

        TMP_Text valueText = CreateText(boxObject.transform, "Value", "", new Vector2(128, 0), new Vector2(126, 42), 24);
        StyleFoodCardText(valueText, 16, 24, TextAlignmentOptions.Center);
        valueText.color = new Color(0.20f, 0.10f, 0.04f);

        if (passive)
        {
            foodTotalPassiveText = valueText;
        }
        else
        {
            foodTotalBonusText = valueText;
        }
#if UNITY_EDITOR
        AttachEditorDragProbe(rect, objectName);
#endif
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
        CreateVipInfoPngElements(vipPanel.transform);

        CreateVipPanelClearSelectionArea(vipPanel.transform);
        CreateVipLeftModularElements(vipPanel.transform);

        vipLevelCardTexts.Clear();
        vipLevelCardImages.Clear();
        vipLevelLockImages.Clear();
        vipLevelSelectImages.Clear();
        vipLevelBottomStarImages.Clear();
        vipLevelBottomStarRects.Clear();
        vipLevelButtonRects.Clear();
        for (int i = 1; i <= MaxVipLevel; i++)
        {
            CreateVipCardButton(vipPanel.transform, i, new Vector2(-75f + ((i - 1) * 151f), 18f));
        }

        for (int i = 1; i <= MaxVipLevel; i++)
        {
            CreateVipCardBottomStarOverlay(vipPanel.transform, i, new Vector2(-75f + ((i - 1) * 151f), -89f));
        }

        vipCoinBonusText = CreateVipBonusPill(vipPanel.transform, "Coin", "+0%", VipCoinBonusLayout);
        vipXpBonusText = CreateVipBonusPill(vipPanel.transform, "XP", "+0%", VipXpBonusLayout);
        vipCoffeeBonusText = CreateVipBonusPill(vipPanel.transform, "Müşteri", "+0%", VipCustomerBonusLayout);
        vipPassiveBonusText = CreateVipBonusPill(vipPanel.transform, "Pasif Gelir", "+0%", VipPassiveBonusLayout);
        vipDecorBonusText = CreateVipBonusPill(vipPanel.transform, "İndirim", "Kilitli", VipDiscountBonusLayout);

        vipUpgradeButtonText = CreateTransparentButton(vipPanel.transform, "VipUpgradeButton", "VIP Al", VipUpgradeButtonPosition, VipUpgradeButtonSize, UpgradeVipLevel, 24, new Color(0.42f, 0.22f, 0.04f, 1f));
        ApplyVipFont(vipUpgradeButtonText);
        vipUpgradeButtonText.rectTransform.anchoredPosition = VipUpgradeButtonTextPosition;
        vipUpgradeButtonText.rectTransform.sizeDelta = VipUpgradeButtonTextSize;
        vipUpgradeButtonText.fontSizeMax = 20f;
        vipUpgradeButtonText.fontSizeMin = 12f;
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
        switch (level)
        {
            case 1:
            case 2:
            case 4:
                return new Vector2(0f, 97.8f);
            case 3:
                return new Vector2(3.7f, 97.8f);
            case 5:
                return new Vector2(-8f, 97.8f);
            default:
                return new Vector2(0f, 86f);
        }
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
        rect.sizeDelta = new Vector2(148, 294);
        vipLevelButtonRects.Add(rect);

        Image image = buttonObject.GetComponent<Image>();
        image.sprite = GetVipCardSprite(level);
        image.color = Color.white;
        image.preserveAspect = false;
        image.raycastTarget = true;

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

        Image selectGlow = CreatePanelStrip(buttonObject.transform, "SelectedGlow", new Vector2(0, -154), new Vector2(112, 10), new Color(1f, 0.78f, 0.18f, 0f));
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
        panelRect.sizeDelta = new Vector2(340, 150);

        Image panelImage = coinInfoPopup.GetComponent<Image>();
        panelImage.sprite = CreateResourceBarBackgroundSprite(panelRect.sizeDelta, new Color(0.22f, 0.12f, 0.05f, 0.96f), new Color(0.96f, 0.69f, 0.19f, 1f));
        panelImage.color = Color.white;

        GameObject connectorObject = new GameObject("Connector", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        connectorObject.transform.SetParent(coinInfoPopup.transform, false);
        RectTransform connectorRect = connectorObject.GetComponent<RectTransform>();
        connectorRect.anchorMin = new Vector2(0.5f, 1f);
        connectorRect.anchorMax = new Vector2(0.5f, 1f);
        connectorRect.pivot = new Vector2(0.5f, 0f);
        connectorRect.anchoredPosition = new Vector2(0, -2);
        connectorRect.sizeDelta = new Vector2(118, 14);
        Image connectorImage = connectorObject.GetComponent<Image>();
        connectorImage.color = new Color(0.22f, 0.12f, 0.05f, 0.96f);
        connectorImage.raycastTarget = false;

        coinInfoPopupText = CreateText(coinInfoPopup.transform, "CoinInfoText", "", new Vector2(0, -8), new Vector2(302, 116), 19);
        coinInfoPopupText.color = new Color(1f, 0.94f, 0.78f, 1f);
        coinInfoPopupText.fontStyle = FontStyles.Bold;
        coinInfoPopupText.alignment = TextAlignmentOptions.Left;
        coinInfoPopupText.enableAutoSizing = true;
        coinInfoPopupText.fontSizeMin = 14;
        coinInfoPopupText.fontSizeMax = 19;
        coinInfoPopupText.raycastTarget = false;

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

    private void CreateInvisibleButton(Transform parent, string objectName, Vector2 position, Vector2 size, UnityEngine.Events.UnityAction action)
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

    private void UpdateUI()
    {
        if (coinText != null)
            coinText.text = FormatResourceAmount(Mathf.FloorToInt(coins));

        if (coinInfoPopupText != null)
            coinInfoPopupText.text = GetCoinInfoPopupText();

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

        if (offlineText != null)
            offlineText.text = "Barista çalıştı\nSen yokken " + lastOfflineCoins + " coin kazandın\nOffline kazanç: pasif gelirin 1/3'ü, en fazla 4 saat";

        if (dailyRewardPanelText != null)
            dailyRewardPanelText.text = GetDailyRewardText();

        if (dailyRewardButtonText != null)
        {
            Button dailyButton = dailyRewardButtonText.GetComponentInParent<Button>();
            bool canClaimDaily = CanClaimDailyReward();
            dailyRewardButtonText.text = canClaimDaily ? "Ödülü Al" : "Bekle";
            if (dailyButton != null)
                dailyButton.interactable = canClaimDaily;
        }

        if (dailyRewardPanel != null && dailyRewardPanel.activeSelf && lastOfflineCoins <= 0)
            dailyRewardPanel.transform.SetAsLastSibling();

        if (chestPanelText != null)
        {
            chestPanelText.text = "Siradan Sandik: " + chests
                + "\nNadir Sandik: " + rareChests
                + "\nOzel Sandik: " + luxuryChests
                + "\nTahta: " + woodPieces
                + GetPaintInventorySummary();
        }

        if (chestOpenButtonText != null)
        {
            Button chestButton = chestOpenButtonText.GetComponentInParent<Button>();
            chestOpenButtonText.text = chests > 0 ? "Siradan\nAc" : "Siradan\nYok";
            if (chestButton != null)
                chestButton.interactable = chests > 0;
        }

        if (rareChestOpenButtonText != null)
        {
            Button rareButton = rareChestOpenButtonText.GetComponentInParent<Button>();
            rareChestOpenButtonText.text = rareChests > 0 ? "Nadir\nAc" : "Nadir\nYok";
            if (rareButton != null)
                rareButton.interactable = rareChests > 0;
        }

        if (luxuryChestOpenButtonText != null)
        {
            Button luxuryButton = luxuryChestOpenButtonText.GetComponentInParent<Button>();
            luxuryChestOpenButtonText.text = luxuryChests > 0 ? "Ozel\nAc" : "Ozel\nYok";
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

        int shownVipLevel = selectedVipPreviewLevel > 0 ? selectedVipPreviewLevel : Mathf.Clamp(vipLevel > 0 ? vipLevel : 1, 1, MaxVipLevel);
        string shownVipName = GetVipTierName(shownVipLevel);

        if (vipCoinBonusText != null)
            vipCoinBonusText.text = shownVipName + "\nCoin " + GetVipBonusValueText(GetVipCoinBonusPercent(shownVipLevel));

        if (vipXpBonusText != null)
            vipXpBonusText.text = shownVipName + "\nXP " + GetVipBonusValueText(GetVipXpBonusPercent(shownVipLevel));

        if (vipCoffeeBonusText != null)
            vipCoffeeBonusText.text = shownVipName + "\nMüşteri " + GetVipBonusValueText(GetVipPatienceBonusPercent(shownVipLevel));

        if (vipPassiveBonusText != null)
            vipPassiveBonusText.text = shownVipName + "\nPasif Gelir " + GetVipBonusValueText(GetVipPassiveBonusPercent(shownVipLevel));

        if (vipDecorBonusText != null)
            vipDecorBonusText.text = shownVipName + "\nİndirim " + GetVipDiscountBonusText(shownVipLevel);

        if (vipLeftCoinPillText != null)
            vipLeftCoinPillText.text = "Coin " + GetVipBonusValueText(GetVipCoinBonusPercent(shownVipLevel));

        if (vipLeftXpPillText != null)
            vipLeftXpPillText.text = "XP " + GetVipBonusValueText(GetVipXpBonusPercent(shownVipLevel));

        if (vipLeftCustomerPillText != null)
            vipLeftCustomerPillText.text = "Müşteri " + GetVipBonusValueText(GetVipPatienceBonusPercent(shownVipLevel));

        if (vipLeftPassivePillText != null)
            vipLeftPassivePillText.text = "Pasif " + GetVipBonusValueText(GetVipPassiveBonusPercent(shownVipLevel));

        if (vipLeftClaimButtonText != null)
            vipLeftClaimButtonText.text = "AL";

        if (vipUpgradeButtonText != null)
        {
            Button vipButton = vipUpgradeButtonText.GetComponentInParent<Button>();
            bool maxed = vipLevel >= MaxVipLevel;
            int cost = GetVipUpgradeCost();
            vipUpgradeButtonText.text = maxed ? "VIP MAX" : "Aç: " + cost;
            if (vipUpgradeGemIconImage != null)
                vipUpgradeGemIconImage.gameObject.SetActive(!maxed);
            if (vipButton != null)
                vipButton.interactable = !maxed && gems >= cost;
        }

        for (int i = 0; i < MaxVipLevel; i++)
        {
            int cardLevel = i + 1;
            if (i < vipLevelCardTexts.Count)
                vipLevelCardTexts[i].text = "";

            if (i < vipLevelCardImages.Count)
            {
                Color cardColor = GetVipTierColor(cardLevel);
                if (vipLevel < cardLevel)
                {
                    cardColor = Color.Lerp(cardColor, new Color(0.05f, 0.05f, 0.06f, cardColor.a), 0.58f);
                    cardColor.a = 0.84f;
                }
                else if (vipLevel == cardLevel)
                {
                    cardColor = Color.Lerp(cardColor, Color.white, 0.15f);
                    cardColor.a = 1f;
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
                Vector2 basePosition = new Vector2(-75f + (i * 151f), 18f);
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

        if (decorActionButtonText != null)
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
        if (foodTotalPassiveText != null)
        {
            foodTotalPassiveText.text = "+%" + GetFoodPassiveBonusPercent();
        }

        if (foodTotalBonusText != null)
        {
            foodTotalBonusText.text = "+%" + GetFoodSalesBonusPercent();
        }

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
            }

            if (ui.LevelLabelText != null)
            {
                ui.LevelLabelText.gameObject.SetActive(true);
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
                ui.LevelText.text = "Sv.\n" + data.UnlockPlayerLevel;
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

    private string GetDailyRewardLine(int dayIndex)
    {
        int gemReward = GetDailyGemReward(dayIndex);
        int chestReward = GetDailyChestReward(dayIndex);

        if (chestReward <= 0)
        {
            return gemReward + " Elmas";
        }

        return gemReward + " Elmas + " + chestReward + " Sandik";
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

    private int GetDailyChestReward(int dayIndex)
    {
        switch (dayIndex)
        {
            case 1: return 1;
            case 3: return 1;
            case 4: return 1;
            case 5: return 1;
            case 6: return 2;
            default: return 0;
        }
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
        float offlineIncomePerSecond = coinsPerSecond * GetVipPassiveIncomeMultiplier() * OfflineIncomeRate;
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
