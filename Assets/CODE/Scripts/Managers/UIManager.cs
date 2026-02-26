using TMPro;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using Utilities.Extensions;
using System.Collections.Generic;
using Poke.UI;
using System;
using TUTORIAL_SYSTEM;

public class UIManager : MonoSingleton<UIManager>
{
    #region Variables

    [TabGroup("Group_1", "Main HUD")][SerializeField] TextMeshProUGUI cashText;
    [TabGroup("Group_1", "Main HUD")][SerializeField] TextMeshProUGUI salesText;
    [TabGroup("Group_1", "Main HUD")][SerializeField] TextMeshProUGUI counterText;
    [TabGroup("Group_1", "Main HUD")][SerializeField] TMP_Text[] showRoomNameTexts;
    [TabGroup("Group_1", "Main HUD")][SerializeField] GameObject panelsButtons;

    [TabGroup("Group_1", "Tasks")][SerializeField] Transform taskListContainer;
    [TabGroup("Group_1", "Tasks")][SerializeField] TaskItemUI taskItemPrefab;
    [TabGroup("Group_1", "Tasks")] private readonly List<TaskItemUI> _activeTaskItems = new();

    [TabGroup("Group_1", "Toasts")][SerializeField] private RectTransform toastPopupContainer;
    [TabGroup("Group_1", "Toasts")][SerializeField] private CanvasGroup toastPopupTemplate;
    [TabGroup("Group_1", "Toasts")] private readonly List<string> _currentToasts = new();

    [TabGroup("Group_1", "MISC")] public ChatBubble ChatBubble;
    [TabGroup("Group_1", "MISC")] public Button RoomButton; // Used to floating above the rooms for actions.
    [TabGroup("Group_1", "MISC")][SerializeField] private Transform cashIcon;

    [TabGroup("Group_2", "Main Panel")][SerializeField] GameObject mainPanel;
    [TabGroup("Group_2", "Main Panel")][SerializeField] TMP_Text mainPanelText;
    [TabGroup("Group_2", "Main Panel")][SerializeField] Transform mainPanelContainer;
    [TabGroup("Group_2", "Main Panel")][SerializeField] Button mainPanelCloseButton;
    [TabGroup("Group_2", "Main Panel"), BoxGroup("Group_2/Main Panel/Items")][SerializeField] BrandShopItem brandItemPrefab;
    [TabGroup("Group_2", "Main Panel"), BoxGroup("Group_2/Main Panel/Items")][SerializeField] BikeShopItem bikeItemPrefab;
    [TabGroup("Group_2", "Main Panel"), BoxGroup("Group_2/Main Panel/Items")][SerializeField] PartnershipItem partnershipItemPrefab;

    [TabGroup("Group_2", "Tabs Panel")][SerializeField] GameObject roomManagementPanel;
    [TabGroup("Group_2", "Tabs Panel")][SerializeField] Transform roomManagementContainer;
    [TabGroup("Group_2", "Tabs Panel")][SerializeField] Button closeRoomManagementButton;
    [TabGroup("Group_2", "Tabs Panel")][SerializeField] TextMeshProUGUI roomNameText, roomLevelText;
    [TabGroup("Group_2", "Tabs Panel")][SerializeField] GameObject roomManagePanelMessage;
    [TabGroup("Group_2", "Tabs Panel")][SerializeField] Button roomLevelUpButton;
    [TabGroup("Group_2", "Tabs Panel")][SerializeField] Transform roomPanelTabsContainer;
    [TabGroup("Group_2", "Tabs Panel")] private Toggle[] roomPanelTabs;
    [TabGroup("Group_2", "Tabs Panel"), BoxGroup("Group_2/Tabs Panel/Items")][SerializeField] SpecialRoomUpgradeItem specialRoomUpgradeItemPrefab;
    [TabGroup("Group_2", "Tabs Panel"), BoxGroup("Group_2/Tabs Panel/Items")][SerializeField] MarketingCampaignItem marketingCampaignItemPrefab;
    [TabGroup("Group_2", "Tabs Panel"), BoxGroup("Group_2/Tabs Panel/Items")][SerializeField] ActiveCampaignItem activeCampaignItemPrefab;
    [TabGroup("Group_2", "Tabs Panel"), BoxGroup("Group_2/Tabs Panel/Items")][SerializeField] RoomSegmentUpgradeItem segmentItemPrefab;
    [TabGroup("Group_2", "Tabs Panel"), BoxGroup("Group_2/Tabs Panel/Items")][SerializeField] StaffHireItem staffHireItemPrefab;
    [TabGroup("Group_2", "Tabs Panel"), BoxGroup("Group_2/Tabs Panel/Items")][SerializeField] PurchasedBikeItem purchasedBikeItemPrefab;
    [TabGroup("Group_2", "Tabs Panel"), BoxGroup("Group_2/Tabs Panel/Items")][SerializeField] PlacedBikeItem placedBikeItemPrefab;
    [TabGroup("Group_2", "Tabs Panel"), BoxGroup("Group_2/Tabs Panel/Items")][SerializeField] HiredStaffItem hiredStaffItemPrefab;

    [TabGroup("Group_2", "Racing Panel")][SerializeField] GameObject racingPanel;
    [TabGroup("Group_2", "Racing Panel")][SerializeField] TextMeshProUGUI racingTitleText;
    [TabGroup("Group_2", "Racing Panel")][SerializeField] Transform racingContainer;
    [TabGroup("Group_2", "Racing Panel")][SerializeField] Button closeRacingButton;
    [TabGroup("Group_2", "Racing Panel"), BoxGroup("Group_2/Racing Panel/Race Selection")][SerializeField] RaceBikeSelectionItem raceBikeSelectionPrefab;
    [TabGroup("Group_2", "Racing Panel"), BoxGroup("Group_2/Racing Panel/Race Selection")][SerializeField] GameObject racingSelectionPanel;
    [TabGroup("Group_2", "Racing Panel"), BoxGroup("Group_2/Racing Panel/Race Selection")][SerializeField] TMP_Dropdown raceTypeSelectionDropdown;
    [TabGroup("Group_2", "Racing Panel"), BoxGroup("Group_2/Racing Panel/Race Selection")][SerializeField] Button startRaceButton;
    [TabGroup("Group_2", "Racing Panel"), BoxGroup("Group_2/Racing Panel/Race Results")][SerializeField] RaceParticipantItem raceParticipantPrefab;
    [TabGroup("Group_2", "Racing Panel"), BoxGroup("Group_2/Racing Panel/Race Results")][SerializeField] GameObject racingResultsPanel;
    [TabGroup("Group_2", "Racing Panel"), BoxGroup("Group_2/Racing Panel/Race Results")][SerializeField] Button claimRewardButton;
    [TabGroup("Group_2", "Racing Panel"), BoxGroup("Group_2/Racing Panel/Race Results")][SerializeField] TextMeshProUGUI resultPositionText, resultRewardText;

    [ReadOnly, Space(20f)] public PanelType CurrentOpenPanel = PanelType.None;
    public enum PanelType { None, Main, Racing, RoomManagement, Popup }

    #endregion

    private void Awake()
    {
        if (mainPanel) mainPanel.SetActive(false);
        if (roomManagementPanel) roomManagementPanel.SetActive(false);
        if (racingPanel) racingPanel.SetActive(false);

        SetupEventListeners();

        showRoomNameTexts.ForEach(t => t.text = ES3.Load("ShowRoomName", defaultValue: "ShowRoom Name"));

        if (roomPanelTabsContainer) roomPanelTabs = roomPanelTabsContainer.GetComponentsInChildren<Toggle>();

        if (toastPopupTemplate)
        {
            toastPopupTemplate.alpha = 0f;
            toastPopupTemplate.gameObject.SetActive(false);
        }
    }

    private void SetupEventListeners()
    {
        TaskManager.Instance.OnTaskAdded += OnTaskAdded;
        TaskManager.Instance.OnTaskCompleted += OnTaskCompleted;
        TaskManager.Instance.OnTaskProgressUpdated += OnTaskProgressUpdated;

        GameManager.Instance.OnCashChanged += UpdateCashDisplay;
        GameManager.Instance.OnSalesChanged += UpdateSalesDisplay;

        // Button events
        if (closeRoomManagementButton) closeRoomManagementButton.onClick.AddListener(CloseRoomManagement);
        if (mainPanelCloseButton) mainPanelCloseButton.onClick.AddListener(CloseMainPanel);
        if (closeRacingButton) closeRacingButton.onClick.AddListener(CloseRacingPanel);
    }

    private void UpdateCashDisplay()
    {
        if (!cashText) return;

        int cash = GameManager.Instance.CurrentCash;

        cashText.DOComplete();
        if (int.TryParse(cashText.text, out int prev) && cash > prev) cashText.DOColor(Color.green, 0.2f).SetLoops(2, LoopType.Yoyo).SetUpdate(true);
        else cashText.DOColor(Color.red, 0.2f).SetLoops(2, LoopType.Yoyo).SetUpdate(true);

        cashText.text = $"${cash:N0}";
    }

    private void UpdateSalesDisplay()
    {
        if (!salesText) return;

        int sales = GameManager.Instance.TotalSales;

        salesText.DOComplete();
        salesText.transform.DOScale(1.1f, 0.2f).SetLoops(2, LoopType.Yoyo).SetUpdate(true);
        if (int.TryParse(salesText.text, out int prev) && sales > prev) salesText.DOColor(Color.green, 0.2f).SetLoops(2, LoopType.Yoyo).SetUpdate(true);
        else salesText.DOColor(Color.red, 0.2f).SetLoops(2, LoopType.Yoyo).SetUpdate(true);

        salesText.text = $"Sales: {sales:N0}";
    }

    #region Panels Open/Close

    public void OpenBrandShop()
    {
        if (!mainPanel) return;

        if (CurrentOpenPanel != PanelType.None && CurrentOpenPanel != PanelType.Main) return;
        CurrentOpenPanel = PanelType.Main;

        if (mainPanelText) mainPanelText.text = "Brand Shop";

        PopulateBrandList();

        mainPanel.SetActive(true);
    }

    public void OpenBikeSelection(BrandData brand)
    {
        if (!mainPanel) return;

        if (CurrentOpenPanel != PanelType.None && CurrentOpenPanel != PanelType.Main) return;
        CurrentOpenPanel = PanelType.Main;

        if (mainPanelText) mainPanelText.text = "Bike Shop";

        PopulateBikeList(brand);

        mainPanel.SetActive(true);
    }

    public void OpenPartnerships()
    {
        if (!mainPanel) return;

        if (CurrentOpenPanel != PanelType.None && CurrentOpenPanel != PanelType.Main) return;
        CurrentOpenPanel = PanelType.Main;

        if (mainPanelText) mainPanelText.text = "Partnerships";

        PopulatePartnershipList();

        mainPanel.SetActive(true);
    }

    public void OpenTestDriveRoomPanel(TestDriveRoom room)
    {
        if (!roomManagementPanel) return;

        if (CurrentOpenPanel != PanelType.None && CurrentOpenPanel != PanelType.RoomManagement) return;
        CurrentOpenPanel = PanelType.RoomManagement;

        if (roomPanelTabsContainer) roomPanelTabsContainer.gameObject.SetActive(false);
        // SetRoomTabs(("Upgrades", () => PopulateTestDriveRoomUpgradesList(room)));

        if (roomNameText) roomNameText.text = "Test Drive Center";
        if (roomLevelText) roomLevelText.text = $"{room.CurrentLevel}";

        roomManagePanelMessage?.SetActive(false);
        roomLevelUpButton?.gameObject.SetActive(false);

        PopulateTestDriveRoomUpgradesList(room);

        roomManagementPanel.SetActive(true);
    }

    public void OpenMarketingRoomPanel(MarketingRoom room)
    {
        if (!roomManagementPanel) return;

        if (CurrentOpenPanel != PanelType.None && CurrentOpenPanel != PanelType.RoomManagement) return;
        CurrentOpenPanel = PanelType.RoomManagement;

        SetRoomTabs(("Upgrades", () => PopulateMarketingRoomUpgradesList(room)),
        ("Campains", () => PopulateMarketingRoomCampains(room)),
        ("Active Campans", () => PopulateMarketingRoomActiveCampains(room)));

        // Set Room Info
        if (roomNameText) roomNameText.text = "Marketing Depart";
        if (roomLevelText) roomLevelText.text = $"{room.CurrentLevel}";

        // Default to Upgrades tab
        roomPanelTabs[0].isOn = false;
        roomPanelTabs[0].isOn = true;

        roomManagementPanel.SetActive(true);
    }

    public void OpenGalleryRoom(GalleryRoom room)
    {
        if (!roomManagementPanel) return;

        if (CurrentOpenPanel != PanelType.None && CurrentOpenPanel != PanelType.RoomManagement) return;
        CurrentOpenPanel = PanelType.RoomManagement;

        SetRoomTabs(("Upgrades", () => PopulateRoomUpgradesList(room)),
        ("Inventory Bikes", () => PopulateRoomBikesInventoryList(room)),
        ("Placed Bikes", () => PopulatePlacedBikesList(room)),
        ("Hire Staff", () => PopulateHireStaffList(room)),
        ("Manage Staff", () => PopulateManageStaffList(room)));

        // Set Room Info
        if (roomNameText) roomNameText.text = room.RoomName;
        if (roomLevelText) roomLevelText.text = $"{room.CurrentLevel}";

        // Default to Upgrades tab
        roomPanelTabs[0].isOn = false;
        roomPanelTabs[0].isOn = true;

        roomManagementPanel.SetActive(true);
    }

    public void OpenEnterence(Entrance entrance)
    {
        if (!roomManagementPanel) return;

        if (CurrentOpenPanel != PanelType.None && CurrentOpenPanel != PanelType.RoomManagement) return;
        CurrentOpenPanel = PanelType.RoomManagement;

        if (roomPanelTabsContainer) roomPanelTabsContainer.gameObject.SetActive(false);

        // Set Room Info
        if (roomNameText) roomNameText.text = "Enterence";
        if (roomLevelText) roomLevelText.text = $"{entrance.CurrentLevel}";

        roomManagePanelMessage?.SetActive(false);
        roomLevelUpButton?.gameObject.SetActive(false);

        PopulateEnternceUpgradesList(entrance);

        roomManagementPanel.SetActive(true);
    }

    public void OpenRacingPanel()
    {
        if (!racingPanel) return;

        // Check if race is available
        if (!RacingManager.Instance.IsRaceAvailable())
        {
            TimeSpan timeUntilNext = RacingManager.Instance.GetTimeUntilNextRace();
            int days = timeUntilNext.Days;
            int hours = timeUntilNext.Hours;

            ShowToastMessage($"Next race available in: {days}d {hours}h");
            return;
        }

        // Get all placed bikes
        var placedBikes = BikesManager.Instance.ActiveBikes.Where(b => b != null).DistinctBy(b => new { b.BikeData, b.CurrentLevel }).ToList();
        if (placedBikes.IsNullOrEmpty())
        {
            ShowToastMessage("No bikes available for racing.\nPlace bikes on stations first!");
            return;
        }

        if (CurrentOpenPanel != PanelType.None) return;
        CurrentOpenPanel = PanelType.Racing;

        racingTitleText.text = "Weekly Race Event";

        closeRacingButton.gameObject.SetActive(true);

        // Default to selection panel
        if (racingResultsPanel) racingResultsPanel.SetActive(false);
        if (racingSelectionPanel) racingSelectionPanel.SetActive(true);

        PopulateRacings(placedBikes);

        racingPanel.SetActive(true);
    }

    public void CloseMainPanel()
    {
        if (!mainPanel.activeSelf) return;

        CurrentOpenPanel = PanelType.None;

        if (mainPanel) mainPanel.SetActive(false);

        foreach (Transform child in mainPanelContainer) Destroy(child.gameObject);
    }

    public void CloseRoomManagement()
    {
        if (!roomManagementPanel.activeSelf) return;

        CurrentOpenPanel = PanelType.None;

        if (roomManagementPanel) roomManagementPanel.SetActive(false);
    }

    public void CloseRacingPanel()
    {
        if (!racingPanel || !racingPanel.activeSelf) return;

        CurrentOpenPanel = PanelType.None;

        racingPanel.SetActive(false);

        // Clear existing
        foreach (Transform child in racingContainer) Destroy(child.gameObject);
    }

    private void SetRoomTabs(params (string name, Action action)[] tabConfigs)
    {
        if (roomPanelTabsContainer) roomPanelTabsContainer.gameObject.SetActive(true);

        // Focus the first tab
        var tabs = roomPanelTabs[0].transform.parent as RectTransform;
        tabs.DOAnchorPosX(0f, 0.25f);

        for (int i = 0; i < roomPanelTabs.Length; i++)
        {
            var tab = roomPanelTabs[i];
            int index = i;

            if (index < tabConfigs.Length)
            {
                var config = tabConfigs[index]; // Access the tuple

                tab.onValueChanged.RemoveAllListeners();
                tab.onValueChanged.AddListener(isOn =>
                {
                    if (isOn)
                    {
                        roomManagePanelMessage?.SetActive(false);
                        roomLevelUpButton?.gameObject.SetActive(false);
                        config.action?.Invoke(); // Invoke the action from the tuple
                    }
                });

                if (tab.TryGetComponentInChildren(out TMP_Text text))
                    text.text = config.name; // Use the name from the tuple

                tab.gameObject.SetActive(true);
            }
            else tab.gameObject.SetActive(false);
        }
    }

    #endregion

    #region Panels Population

    private void PopulateTestDriveRoomUpgradesList(TestDriveRoom room)
    {
        if (!roomManagementContainer || !specialRoomUpgradeItemPrefab) return;

        // Clear existing
        foreach (Transform child in roomManagementContainer) Destroy(child.gameObject);

        if (roomLevelText) roomLevelText.text = $"{room.CurrentLevel}";

        if (UpdateCompleteLevelButton()) return;

        var segments = room.GetCurrentLevelSegments();
        if (segments.IsNullOrEmpty()) return;

        bool isAnyAvailable = Enumerable.Range(1, segments.Length - 1).Any(i => !room.IsSegmentPurchased(i));
        if (!isAnyAvailable)
        {
            if (roomManagePanelMessage)
            {
                roomManagePanelMessage.GetComponentInChildren<TMP_Text>().text = "Test Drive Room is Maxed Out.\nKeep An Eye On Updates!";
                roomManagePanelMessage.SetActive(true);
            }

            return;
        }

        // Group segments by icon (segments with same icon will be stacked)
        Dictionary<Sprite, List<int>> groupedSegments = new();

        // Skip first segment (starting content - free) and group unpurchased segments
        for (int i = 1; i < segments.Length; i++)
        {
            if (room.IsSegmentPurchased(i)) continue;

            isAnyAvailable = true;

            var segment = segments[i];
            Sprite icon = segment.Icon;

            if (!groupedSegments.ContainsKey(icon)) groupedSegments[icon] = new List<int>();
            groupedSegments[icon].Add(i);
        }

        // Create one UI item per group
        foreach (var group in groupedSegments)
        {
            Sprite icon = group.Key;
            List<int> segmentIndices = group.Value;

            var upgradeItem = Instantiate(specialRoomUpgradeItemPrefab, roomManagementContainer);

            // Initialize with stacked segments using generic functions
            upgradeItem.InitializeStacked(icon, segmentIndices, (index) => room.IsSegmentPurchased(index), (index) => segments[index].Cost, (index) => room.TryBuySegment(index), () =>
            {
                // All segments done, refresh to show complete button
                if (room.PurchasedSegmentIndices.Count + 1 >= segments.Length) PopulateTestDriveRoomUpgradesList(room);
                else
                {
                    int nextSegmentIndex = upgradeItem.GetNextUnpurchasedSegmentIndex();

                    // If all purchased, destroy this UI item
                    if (nextSegmentIndex == -1) Destroy(upgradeItem.gameObject);
                    else AddAllTexts(upgradeItem, segments[upgradeItem.GetNextUnpurchasedSegmentIndex()]);
                }

                return true;
            });

            // Add bonuses from the first segment in stack
            AddAllTexts(upgradeItem, segments[segmentIndices[0]]);
        }

        void AddAllTexts(SpecialRoomUpgradeItem upgradeItem, TestDriveRoom.UpgradeSegment segment)
        {
            upgradeItem.ClearAllTexts();

            if (segment.MaxTestDrivers > 0) upgradeItem.AddText($"Max Drivers: {segment.MaxTestDrivers}");
            if (segment.PurchaseChanceBonus > 0) upgradeItem.AddText($"Purchase Chance: +{segment.PurchaseChanceBonus * 100:F0}%");
            if (segment.TestDriveSpeedMultiplier > 1f) upgradeItem.AddText($"Drive Speed: {segment.TestDriveSpeedMultiplier:F1}x");

            if (!segment.UnlockBikes.IsNullOrEmpty()) upgradeItem.AddText($"Unlocks: {segment.UnlockBikes.Count} Bike(s)");
            if (!segment.UnlockRoutes.IsNullOrEmpty()) upgradeItem.AddText($"Unlocks: {segment.UnlockRoutes.Count} Route(s)");
        }

        bool UpdateCompleteLevelButton()
        {
            if (!roomLevelUpButton) return false;

            bool canComplete = room.CanCompleteLevel();
            roomLevelUpButton.gameObject.SetActive(canComplete);

            if (canComplete)
            {
                roomLevelUpButton.onClick.RemoveAllListeners();
                roomLevelUpButton.onClick.AddListener(() =>
                {
                    if (GameManager.Instance.TrySpendCash(5000)) // hard coded 5k value for level up
                    {
                        room.CompleteLevelUpgrade();
                        PopulateTestDriveRoomUpgradesList(room);
                    }
                });

                roomLevelUpButton.GetComponentInChildren<TMP_Text>().text = $"Upgrade To Level {room.CurrentLevel + 1}\n<size=48>${5000:N0}</size>";
            }

            return canComplete;
        }
    }

    private void PopulateMarketingRoomActiveCampains(MarketingRoom room)
    {
        if (!activeCampaignItemPrefab || !roomManagementContainer) return;

        // Clear existing
        foreach (Transform child in roomManagementContainer) Destroy(child.gameObject);

        if (room.ActiveCampaigns.IsNullOrEmpty())
        {
            if (roomManagePanelMessage)
            {
                roomManagePanelMessage.GetComponentInChildren<TMP_Text>().text = "No Active Campaigns.\nLaunch a campaign from Campaigns Tab!";
                roomManagePanelMessage.SetActive(true);
            }

            return;
        }

        foreach (var campaign in room.ActiveCampaigns)
        {
            var item = Instantiate(activeCampaignItemPrefab, roomManagementContainer);
            item.Initialize(campaign);
        }
    }

    public void PopulateMarketingRoomCampains(MarketingRoom room)
    {
        if (!marketingCampaignItemPrefab || !roomManagementContainer) return;

        // Clear existing
        foreach (Transform child in roomManagementContainer) Destroy(child.gameObject);

        CreateCampaignLauncher(MarketingRoom.MarketingCampaignType.SocialMedia, 500, 60, room);
        CreateCampaignLauncher(MarketingRoom.MarketingCampaignType.Billboard, 1000, 120, room);
        CreateCampaignLauncher(MarketingRoom.MarketingCampaignType.RadioAd, 300, 45, room);
        CreateCampaignLauncher(MarketingRoom.MarketingCampaignType.PrestigeEvent, 2000, 180, room);

        void CreateCampaignLauncher(MarketingRoom.MarketingCampaignType type, int cost, int duration, MarketingRoom room)
        {
            var item = Instantiate(marketingCampaignItemPrefab, roomManagementContainer);
            item.Initialize(type, cost, duration, room, OnCampaignStarted);

            /// Refresh marketing panel
            void OnCampaignStarted()
            {
                foreach (Transform child in roomManagementContainer)
                {
                    var ui = child.GetComponent<MarketingCampaignItem>();
                    if (ui) ui.UpdateDisplay();
                }
            }
        }
    }

    private void PopulateMarketingRoomUpgradesList(MarketingRoom room)
    {
        if (!roomManagementContainer || !specialRoomUpgradeItemPrefab) return;

        // Clear existing
        foreach (Transform child in roomManagementContainer) Destroy(child.gameObject);

        if (roomLevelText) roomLevelText.text = $"{room.CurrentLevel}";

        if (UpdateCompleteLevelButton()) return;

        var segments = room.GetCurrentLevelSegments();
        if (segments.IsNullOrEmpty()) return;

        bool isAnyAvailable = Enumerable.Range(1, segments.Length - 1).Any(i => !room.IsSegmentPurchased(i));
        if (!isAnyAvailable)
        {
            if (roomManagePanelMessage)
            {
                roomManagePanelMessage.GetComponentInChildren<TMP_Text>().text = "Marketing Room is Maxed Out.\nKeep An Eye On Updates!";
                roomManagePanelMessage.SetActive(true);
            }
            return;
        }

        // Group segments by icon (segments with same icon will be stacked)
        Dictionary<Sprite, List<int>> groupedSegments = new();

        // Skip first segment (starting content - free) and group unpurchased segments
        for (int i = 1; i < segments.Length; i++)
        {
            if (room.PurchasedSegmentIndices.Contains(i)) continue;

            isAnyAvailable = true;

            var segment = segments[i];
            Sprite icon = segment.Icon;

            if (!groupedSegments.ContainsKey(icon)) groupedSegments[icon] = new List<int>();
            groupedSegments[icon].Add(i);
        }

        // Create one UI item per group
        foreach (var group in groupedSegments)
        {
            Sprite icon = group.Key;
            List<int> segmentIndices = group.Value;

            var upgradeItem = Instantiate(specialRoomUpgradeItemPrefab, roomManagementContainer);

            // Initialize with stacked segments using generic functions
            upgradeItem.InitializeStacked(icon, segmentIndices, (index) => room.IsSegmentPurchased(index), (index) => segments[index].Cost, (index) => room.TryBuySegment(index), () =>
            {
                // All segments done, refresh to show complete button
                if (room.PurchasedSegmentIndices.Count + 1 >= segments.Length) PopulateMarketingRoomUpgradesList(room);
                else
                {
                    int nextSegmentIndex = upgradeItem.GetNextUnpurchasedSegmentIndex();

                    // If all purchased, destroy this UI item
                    if (nextSegmentIndex == -1) Destroy(upgradeItem.gameObject);
                    else AddAllTexts(upgradeItem, segments[upgradeItem.GetNextUnpurchasedSegmentIndex()]);
                }

                return true;
            });

            // Add bonuses from the first segment in stack
            AddAllTexts(upgradeItem, segments[segmentIndices[0]]);
        }

        void AddAllTexts(SpecialRoomUpgradeItem upgradeItem, MarketingRoom.UpgradeSegment segment)
        {
            upgradeItem.ClearAllTexts();

            if (segment.CustomerSpawnRateBonus > 0) upgradeItem.AddText($"Customers: +{segment.CustomerSpawnRateBonus * 100:F0}%");
            if (segment.SalePriceBonus > 0) upgradeItem.AddText($"Sale Prices: +{segment.SalePriceBonus * 100:F0}%");
            if (segment.BrandReputationBonus > 0) upgradeItem.AddText($"Brand Rep: +{segment.BrandReputationBonus * 100:F0}%");
            if (segment.MaxActiveCampaigns > 0) upgradeItem.AddText($"Max Campaigns: {segment.MaxActiveCampaigns}");
        }

        bool UpdateCompleteLevelButton()
        {
            if (!roomLevelUpButton) return false;

            bool canComplete = room.CanCompleteLevel();
            roomLevelUpButton.gameObject.SetActive(canComplete);

            if (canComplete)
            {
                roomLevelUpButton.onClick.RemoveAllListeners();
                roomLevelUpButton.onClick.AddListener(() =>
                {
                    if (GameManager.Instance.TrySpendCash(5000)) // hard coded 5k value for level up
                    {
                        room.CompleteLevelUpgrade();
                        PopulateMarketingRoomUpgradesList(room);
                    }
                });

                roomLevelUpButton.GetComponentInChildren<TMP_Text>().text = $"Upgrade to Level {room.CurrentLevel + 1}\n<size=48>${5000:N0}</size>";
            }

            return canComplete;
        }
    }

    private void PopulateHireStaffList(GalleryRoom room)
    {
        if (!roomManagementContainer || !staffHireItemPrefab) return;

        // Clear existing items
        foreach (Transform child in roomManagementContainer) Destroy(child.gameObject);

        // Create an item for each staff type
        foreach (StaffData staffData in StaffManager.Instance.AvailableStaffTypes)
        {
            var item = Instantiate(staffHireItemPrefab, roomManagementContainer);
            if (item) item.Initialize(staffData, room, OnHireStaff);
        }

        void OnHireStaff(StaffData staffData)
        {
            // Refresh the hire staff list
            foreach (Transform child in roomManagementContainer)
            {
                var ui = child.GetComponent<StaffHireItem>();
                if (ui) ui.UpdateDisplay();
            }
        }
    }

    private void PopulateManageStaffList(GalleryRoom room)
    {
        if (!roomManagementContainer || !hiredStaffItemPrefab) return;

        // Clear existing items
        foreach (Transform child in roomManagementContainer) Destroy(child.gameObject);

        var hiredStaff = StaffManager.Instance.HiredStaff.Where(s => s.AssignedRoom == room).ToArray();

        if (hiredStaff.IsNullOrEmpty())
        {
            if (roomManagePanelMessage)
            {
                roomManagePanelMessage.GetComponentInChildren<TMP_Text>().text = "No Staff in inventory.\nHire staff from Hire Staff Tab!";
                roomManagePanelMessage.SetActive(true);
            }

            return;
        }

        // Create an item for each hired staff
        foreach (StaffMember staff in hiredStaff)
        {
            var item = Instantiate(hiredStaffItemPrefab, roomManagementContainer);
            if (item) item.Initialize(staff); // onAssign: _ => staff.AssignToRoom(room)
        }
    }

    private void PopulateRoomUpgradesList(GalleryRoom room)
    {
        if (!roomManagementContainer || !segmentItemPrefab) return;

        // Clear existing items
        foreach (Transform child in roomManagementContainer) Destroy(child.gameObject);

        if (roomLevelText) roomLevelText.text = $"{room.CurrentLevel}";

        if (UpdateCompleteLevelButton(room)) return;

        var segments = room.GetCurrentLevelSegments();
        if (segments.IsNullOrEmpty()) return;

        bool isAnyAvailable = Enumerable.Range(1, segments.Length - 1).Any(i => !room.IsSegmentPurchased(i));
        if (!isAnyAvailable)
        {
            if (roomManagePanelMessage)
            {
                roomManagePanelMessage.GetComponentInChildren<TMP_Text>().text = $"{room.RoomName} is Maxed Out.\nKeep An Eye On Updates!";
                roomManagePanelMessage.SetActive(true);
            }

            return;
        }

        // Create a UI item for each segment, skip the first one because it's going to be already purchased as a starting content. 
        for (int i = 1; i < segments.Length; i++)
        {
            var segment = segments[i];
            var item = Instantiate(segmentItemPrefab, roomManagementContainer);

            if (item) item.Initialize(room, segment, i, () =>
            {
                if (room.PurchasedSegmentIndices.Count + 1 >= segments.Length) PopulateRoomUpgradesList(room);
            });
        }

        // Focus the first item
        var container = roomManagementContainer as RectTransform;
        container.DOAnchorPosY(0f, 0.25f);

        bool UpdateCompleteLevelButton(GalleryRoom room)
        {
            if (!roomLevelUpButton) return false;

            bool canCompleteLevel = room.CanCompleteLevel();

            roomLevelUpButton.gameObject.SetActive(canCompleteLevel);

            if (canCompleteLevel)
            {
                roomLevelUpButton.onClick.RemoveAllListeners();
                roomLevelUpButton.onClick.AddListener(() =>
                {
                    if (GameManager.Instance.TrySpendCash(5000)) // hard coded 5k value for level up
                    {
                        room.CompleteLevelUpgrade();

                        // Refresh UI
                        if (roomLevelText) roomLevelText.text = $"{room.CurrentLevel}";
                        PopulateRoomUpgradesList(room);
                        UpdateCompleteLevelButton(room);
                    }
                });

                var buttonText = roomLevelUpButton.GetComponentInChildren<TMP_Text>();
                if (buttonText) buttonText.text = $"Upgrade To Level {room.CurrentLevel + 1}\n<size=48>${5000:N0}</size>";
            }

            return canCompleteLevel;
        }
    }

    private void PopulateRoomBikesInventoryList(GalleryRoom room)
    {
        if (!roomManagementContainer || !purchasedBikeItemPrefab) return;

        // Clear existing items
        foreach (Transform child in roomManagementContainer) Destroy(child.gameObject);

        // Get all bikes from inventory
        var inventoryBikes = BikesManager.Instance.GetAllBikes();

        if (inventoryBikes.Count == 0)
        {
            if (roomManagePanelMessage)
            {
                roomManagePanelMessage.GetComponentInChildren<TMP_Text>().text = "No bikes in inventory.\nPurchase bikes from Brand Shop!";
                roomManagePanelMessage.SetActive(true);
            }
            return;
        }

        roomManagePanelMessage?.SetActive(false);

        // Create a UI item for each bike in the inventory.
        foreach (var inventoryItem in inventoryBikes)
        {
            var item = Instantiate(purchasedBikeItemPrefab, roomManagementContainer);
            if (item)
            {
                var bike = BikesManager.Instance.AllBikesData.FirstOrDefault(b => b.ID == inventoryItem.ID);
                item.Initialize(bike, room, OnBikPlaced);
            }
        }

        void OnBikPlaced()
        {
            // Refresh all purchased bike items
            foreach (Transform child in roomManagementContainer)
            {
                var ui = child.GetComponent<PurchasedBikeItem>();
                if (ui) ui.UpdateDisplay();
            }
        }
    }

    private void PopulateEnternceUpgradesList(Entrance entrance)
    {
        if (!roomManagementContainer || !specialRoomUpgradeItemPrefab) return;

        // Clear existing items
        foreach (Transform child in roomManagementContainer) Destroy(child.gameObject);

        if (roomLevelText) roomLevelText.text = $"{entrance.CurrentLevel}";

        if (UpdateCompleteLevelButton()) return;

        var segments = entrance.GetCurrentLevelSegments();
        if (segments.IsNullOrEmpty()) return;

        bool isAnyAvailable = Enumerable.Range(1, segments.Length - 1).Any(i => !entrance.IsSegmentPurchased(i));
        if (!isAnyAvailable)
        {
            if (roomManagePanelMessage)
            {
                roomManagePanelMessage.GetComponentInChildren<TMP_Text>().text = "Entrence is Maxed Out.\nKeep An Eye On Updates!";
                roomManagePanelMessage.SetActive(true);
            }

            return;
        }

        // Group segments by icon (segments with same icon will be stacked)
        Dictionary<Sprite, List<int>> groupedSegments = new();

        // Skip first segment (starting content - free) and group unpurchased segments
        for (int i = 1; i < segments.Length; i++)
        {
            if (entrance.IsSegmentPurchased(i)) continue;

            var segment = segments[i];
            Sprite icon = segment.Icon;

            if (!groupedSegments.ContainsKey(icon)) groupedSegments[icon] = new List<int>();
            groupedSegments[icon].Add(i);
        }

        // Create one UI item per group
        foreach (var group in groupedSegments)
        {
            Sprite icon = group.Key;
            List<int> segmentIndices = group.Value;

            var upgradeItem = Instantiate(specialRoomUpgradeItemPrefab, roomManagementContainer);

            // Initialize with stacked segments using generic functions
            upgradeItem.InitializeStacked(icon, segmentIndices, (index) => entrance.IsSegmentPurchased(index), (index) => segments[index].Cost, (index) => entrance.TryBuySegment(index), () =>
            {
                // All segments done, refresh to show complete button
                if (entrance.PurchasedSegmentIndices.Count + 1 >= segments.Length) PopulateEnternceUpgradesList(entrance);
                else
                {
                    int nextSegmentIndex = upgradeItem.GetNextUnpurchasedSegmentIndex();

                    // If all purchased, destroy this UI item
                    if (nextSegmentIndex == -1) Destroy(upgradeItem.gameObject);
                    else AddAllTexts(upgradeItem, segments[upgradeItem.GetNextUnpurchasedSegmentIndex()]);
                }

                return true;
            });

            // Add bonuses from the first segment in stack
            AddAllTexts(upgradeItem, segments[segmentIndices[0]]);
        }

        void AddAllTexts(SpecialRoomUpgradeItem upgradeItem, Entrance.EntranceSegment segment)
        {
            upgradeItem.ClearAllTexts();

            if (segment.MaxCustomersBonus > 0) upgradeItem.AddText($"Max Que Customers: +{segment.MaxCustomersBonus}");
            if (segment.SpawnRateBonus > 0) upgradeItem.AddText($"Customers: +{segment.SpawnRateBonus * 100:F0}%");
        }

        bool UpdateCompleteLevelButton()
        {
            if (!roomLevelUpButton) return false;

            bool canComplete = entrance.CanCompleteLevel();
            roomLevelUpButton.gameObject.SetActive(canComplete);

            if (canComplete)
            {
                roomLevelUpButton.onClick.RemoveAllListeners();
                roomLevelUpButton.onClick.AddListener(() =>
                {
                    if (GameManager.Instance.TrySpendCash(5000)) // hard coded 5k value for level up
                    {
                        entrance.CompleteLevelUpgrade();
                        PopulateEnternceUpgradesList(entrance);
                    }
                });

                roomLevelUpButton.GetComponentInChildren<TMP_Text>().text = $"Upgrade To Level {entrance.CurrentLevel + 1}\n<size=48>${5000:N0}</size>";
            }

            return canComplete;
        }

    }

    private void PopulatePlacedBikesList(GalleryRoom room)
    {
        if (!roomManagementContainer || !placedBikeItemPrefab) return;

        // Clear existing items
        foreach (Transform child in roomManagementContainer) Destroy(child.gameObject);

        // Get all stations with bikes in this room
        var stationsWithBikes = room.Stations.Where(s => s.CurrentBike != null).ToList();

        if (stationsWithBikes.Count == 0)
        {
            if (roomManagePanelMessage)
            {
                roomManagePanelMessage.GetComponentInChildren<TMP_Text>().text = "No bikes placed in this room.\nPlace bikes from the Inventory tab!";
                roomManagePanelMessage.SetActive(true);
            }
            return;
        }

        // Create a UI item for each placed bike
        foreach (var station in stationsWithBikes)
        {
            var item = Instantiate(placedBikeItemPrefab, roomManagementContainer);
            if (item) item.Initialize(station, () => PopulatePlacedBikesList(room));
        }
    }

    private void PopulatePartnershipList()
    {
        if (!mainPanelContainer || !partnershipItemPrefab) return;

        // Clear existing items
        foreach (Transform child in mainPanelContainer) Destroy(child.gameObject);

        // Get active partnerships
        var partnerships = BrandManager.Instance.GetActivePartnerships();
        foreach (BrandPartnership partnership in partnerships)
        {
            var item = Instantiate(partnershipItemPrefab, mainPanelContainer);
            if (item) item.Initialize(partnership, OpenBikeSelection);
        }
    }

    private void PopulateBrandList()
    {
        if (!mainPanelContainer || !brandItemPrefab) return;

        // Clear existing items
        foreach (Transform child in mainPanelContainer) Destroy(child.gameObject);

        // Get available brands
        var availableBrands = BrandManager.Instance.GetAvailableBrands();
        foreach (BrandData brand in availableBrands)
        {
            var item = Instantiate(brandItemPrefab, mainPanelContainer);
            if (item) item.Initialize(brand, OpenBikeSelection);
        }
    }

    private void PopulateBikeList(params BrandData[] brands)
    {
        if (!mainPanelContainer || !bikeItemPrefab) return;

        // Clear existing items
        foreach (Transform child in mainPanelContainer) Destroy(child.gameObject);

        // Get available bikes for this brand
        var availableBikes = brands.SelectMany(brand => BrandManager.Instance.GetAvailableBikes(brand));

        foreach (BikeData bike in availableBikes)
        {
            var item = Instantiate(bikeItemPrefab, mainPanelContainer);
            if (item) item.Initialize(bike);
        }
    }

    private void PopulateRacings(List<BikeInstance> placedBikes)
    {
        if (!racingContainer) return;

        // Clear existing
        foreach (Transform child in racingContainer) Destroy(child.gameObject);

        var racingContainerLayout = racingContainer.GetComponent<Layout>();
        List<RaceParticipantItem> raceParticipantItems = new();
        BikeInstance selectedRaceBike = null;
        RaceType selectedRaceType = RaceType.Sprint;

        // Create race type selection
        if (raceTypeSelectionDropdown)
        {
            raceTypeSelectionDropdown.options.Clear();
            raceTypeSelectionDropdown.options.AddRange(Enum.GetNames(typeof(RaceType)).Select(rt => new TMP_Dropdown.OptionData(rt)).ToList());
            raceTypeSelectionDropdown.onValueChanged.RemoveAllListeners();
            raceTypeSelectionDropdown.onValueChanged.AddListener(index => selectedRaceType = (RaceType)index);
            raceTypeSelectionDropdown.RefreshShownValue();
        }

        // Start race button
        if (startRaceButton)
        {
            startRaceButton.gameObject.SetActive(true);
            startRaceButton.interactable = false;
            startRaceButton.onClick.RemoveAllListeners();
            startRaceButton.onClick.AddListener(OnStartRaceClicked);
        }

        // Create bike selection items
        foreach (var bike in placedBikes)
        {
            var item = Instantiate(raceBikeSelectionPrefab, racingContainer);
            item.Initialize(bike, OnBikeSelected);
        }

        void OnBikeSelected(BikeInstance bike)
        {
            selectedRaceBike = bike;
            // Update all bike items to show selection
            foreach (var item in racingContainer.GetComponentsInChildren<RaceBikeSelectionItem>()) item.SetSelected(item.Bike == selectedRaceBike);
            if (startRaceButton) startRaceButton.interactable = selectedRaceBike;
        }

        void OnStartRaceClicked()
        {
            if (!selectedRaceBike) return;

            closeRacingButton.gameObject.SetActive(false);
            if (startRaceButton) startRaceButton.interactable = false;

            // Disable hud elements to keep screen clean while racing
            if (taskListContainer) taskListContainer.gameObject.SetActive(false);
            if (panelsButtons) panelsButtons.SetActive(false);

            if (racingTitleText) racingTitleText.text = $"Racing - {RacingManager.Instance.CurrentRace.RaceType}";

            var panelRect = racingPanel.transform as RectTransform;
            panelRect.DOSizeDelta(new(panelRect.sizeDelta.x, 500f), 1f);
            panelRect.SetAnchor(AnchorPresets.BottomCenter);
            panelRect.DOAnchorPosY(270f, 1f).SetEase(Ease.OutExpo);

            // Subscribe to race events
            RacingManager.Instance.OnRaceStarted += OnRaceStart;
            RacingManager.Instance.OnRaceUpdate += OnRaceUpdate;
            RacingManager.Instance.OnRaceFinished += OnRaceFinished;

            RacingManager.Instance.StartRace(selectedRaceBike, selectedRaceType);

            if (counterText)
            {
                counterText.DOCounter(3, 0, 4).OnStart(() => counterText.gameObject.SetActive(true)).OnComplete(() =>
                {
                    RacingManager.Instance.RaceStarted();
                    counterText.gameObject.SetActive(false);
                }).SetDelay(1f);
            }
            else RacingManager.Instance.RaceStarted();

            // Create UI items for each participant
            foreach (Transform child in racingContainer) Destroy(child.gameObject);
            foreach (var participant in RacingManager.Instance.CurrentRace.Participants)
            {
                var item = Instantiate(raceParticipantPrefab, racingContainer);
                item.Initialize(participant);

                raceParticipantItems.Add(item);
            }
        }

        void OnRaceStart(RaceEvent raceEvent) => CurrentOpenPanel = PanelType.None; // Allow the camera to be moved

        void OnRaceUpdate(RaceEvent raceEvent)
        {
            foreach (var participant in raceParticipantItems) participant.UpdateDisplay();
        }

        void OnRaceFinished(RaceResults results)
        {
            // Unsubscribe
            RacingManager.Instance.OnRaceStarted -= OnRaceStart;
            RacingManager.Instance.OnRaceUpdate -= OnRaceUpdate;
            RacingManager.Instance.OnRaceFinished -= OnRaceFinished;

            CurrentOpenPanel = PanelType.Racing; // Disable the camera movement as it should be when panel is open

            if (taskListContainer) taskListContainer.gameObject.SetActive(true);
            if (panelsButtons) panelsButtons.SetActive(true);

            // Update title
            if (racingTitleText) racingTitleText.text = "Race Results";

            // Show results panel
            if (racingResultsPanel) racingResultsPanel.SetActive(true);
            if (racingSelectionPanel) racingSelectionPanel.SetActive(false);

            if (resultPositionText) resultPositionText.text = RacingManager.GetPositionText(results.PlayerPosition);
            if (resultRewardText) resultRewardText.text = RacingManager.GetRewardText(results.PlayerPosition);

            OnRaceUpdate(RacingManager.Instance.CurrentRace);

            // Setup claim button
            if (claimRewardButton)
            {
                claimRewardButton.onClick.RemoveAllListeners();
                claimRewardButton.onClick.AddListener(() =>
                {
                    CloseRacingPanel();
                    RacingManager.Instance.AwardPrizes(results);
                });
            }

            var panelRect = racingPanel.transform as RectTransform;
            panelRect.SetAnchor(AnchorPresets.MiddleCenter);

            panelRect.DOSizeDelta(new Vector2(panelRect.sizeDelta.x, 600f), 1f);
            panelRect.DOAnchorPosY(0f, 1f).SetEase(Ease.OutExpo);

            if (panelRect.TryGetComponentInChildren(out ScrollRect scroll))
            {
                var playerItem = raceParticipantItems.FirstOrDefault(i => i.Participant.IsPlayer).transform as RectTransform;
                scroll.ScrollToCeneter(playerItem);
            }
        }
    }

    #endregion

    #region Task Management

    private void OnTaskAdded(TaskData taskData)
    {
        TaskItemUI item = Instantiate(taskItemPrefab, taskListContainer);
        item.Initialize(taskData);
        _activeTaskItems.Add(item);
    }

    private void OnTaskCompleted(TaskData taskData)
    {
        TaskItemUI item = _activeTaskItems.FirstOrDefault(i => i.TaskData == taskData);
        if (item) item.ShowCompleted();
    }

    private void OnTaskProgressUpdated(TaskData taskData)
    {
        TaskItemUI item = _activeTaskItems.FirstOrDefault(i => i.TaskData == taskData);
        if (item) item.UpdateProgress();
    }

    public void RemoveAllTaksUI()
    {
        foreach (var item in _activeTaskItems)
        {
            Destroy(item.gameObject);
        }

        _activeTaskItems.Clear();
    }

    #endregion

    public void ShowToastMessage(string text, float duration = 3f)
    {
        if (!toastPopupContainer || !toastPopupTemplate || _currentToasts.Contains(text)) return;

        var popup = Instantiate(toastPopupTemplate, toastPopupContainer);
        popup.GetComponentInChildren<TMP_Text>().text = text;
        _currentToasts.Add(text);

        popup.gameObject.SetActive(true);
        popup.DOFade(1f, 0.3f).OnComplete(() =>
        {
            popup.DOFade(0f, 0.3f).SetDelay(duration).OnComplete(() =>
            {
                _currentToasts.Remove(text);
                Destroy(popup.gameObject);
            });
        });
    }

    public void ChangeShowRoomName()
    {
        if (showRoomNameTexts.IsNullOrEmpty()) return;

        var window = UI_InputWindow.HasInstance;
        if (!window || window.IsOpen || CurrentOpenPanel != PanelType.None) return;

        CurrentOpenPanel = PanelType.Popup;

        window.Show("ShowRoom Name", newName =>
        {
            CurrentOpenPanel = PanelType.None;
            if (newName.IsNullOrEmpty() || newName.IsNullOrWhiteSpace()) return;

            if (!TutorialManager.Instance.AllTutorialsCompleted)
            {
                var tuto = TutorialManager.Instance.Tutorials[0];
                if (!tuto.IsTutorialCompleted && tuto.Stages[0].IsStageCompleted) tuto.Stages[1].StageCompleted();
            }

            ES3.Save("ShowRoomName", newName);
            showRoomNameTexts.ForEach(t => t.text = newName);
        }, () => CurrentOpenPanel = PanelType.None, defaultInput: showRoomNameTexts[0].text, characterLimit: 15);
    }

    public void CashAddingAnimation(Vector3 startPos)
    {
        for (int i = 0; i < 7; i++)
        {
            var coin = Instantiate(cashIcon, startPos, Quaternion.identity, cashIcon.root);
            coin.gameObject.SetActive(true);
            coin.localScale = Vector3.zero;

            var random = new Vector3(UnityEngine.Random.Range(-50f, 50f), UnityEngine.Random.Range(-100f, 100f));

            var seq = DOTween.Sequence().OnComplete(() => Destroy(coin.gameObject)).SetUpdate(true);
            seq.Append(coin.DOScale(1f, 0.5f).SetEase(Ease.OutQuad));
            seq.Join(coin.DOMove(startPos + random, 0.5f).SetEase(Ease.InSine));
            seq.Append(coin.DOMove(cashText.transform.position.WithZ(0f), 0.5f).SetEase(Ease.InQuad));
        }
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnCashChanged -= UpdateCashDisplay;
        GameManager.Instance.OnSalesChanged -= UpdateSalesDisplay;

        TaskManager.Instance.OnTaskAdded -= OnTaskAdded;
        TaskManager.Instance.OnTaskCompleted -= OnTaskCompleted;
        TaskManager.Instance.OnTaskProgressUpdated -= OnTaskProgressUpdated;
    }
}