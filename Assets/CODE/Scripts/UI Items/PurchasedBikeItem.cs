using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TUTORIAL_SYSTEM;
using Utilities.Extensions;

public class PurchasedBikeItem : MonoBehaviour
{
    [SerializeField] private Image bikeIconImage;
    [SerializeField] private TextMeshProUGUI bikeNameText;
    [SerializeField] private TextMeshProUGUI unitsText;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private Button placeButton;

    private GalleryRoom _room;
    private BikeData _bikeData;
    private System.Action _onPlaceBike;

    public void Initialize(BikeData bike, GalleryRoom room, System.Action onPlaceBike = null)
    {
        _bikeData = bike;
        _onPlaceBike = onPlaceBike;
        _room = room;

        UpdateDisplay();

        if (placeButton)
        {
            placeButton.onClick.AddListener(OnPlaceClicked);
            if (!TutorialManager.Instance.AllTutorialsCompleted) TutorialManager.Instance.DelayedExecution(0.1f, SetupTutorial);
        }

        // Subscribe to inventory changes
        BikesManager.Instance.OnInventoryChanged += UpdateDisplay;
    }

    private void SetupTutorial()
    {
        var tuto = TutorialManager.Instance.Tutorials[1];

        if (!tuto.Stages[5].IsStageCompleted) return; // Ensure previous stage is completed

        var sixthStage = tuto.Stages[6];
        if (sixthStage == null || sixthStage.EndButtonTarget || sixthStage.IsStageCompleted) return;

        sixthStage.StageEndTrigger = TutorialManager.TriggerType.ButtonClick;
        sixthStage.EndButtonTarget = placeButton;

        var hand = sixthStage.MyModules[0] as TutorialModule_DynamicHand;
        if (hand)
        {
            var point = new TutorialModule_DynamicHand.HandPointStruct { Point = placeButton.transform, Offset = new Vector3(1.1f, -0.1f), HandEventType = TutorialManager.HandEventType.Click };
            hand.Points = new System.Collections.Generic.List<TutorialModule_DynamicHand.HandPointStruct> { point };
        }

        sixthStage.StartTheStage();
        sixthStage.StageStarted();
    }

    public void UpdateDisplay()
    {
        if (!_bikeData) return;

        // Get the latest count from inventory and displayed
        int unitsInInventory = BikesManager.Instance.GetBikeCount(_bikeData);
        int displayedCount = BikesManager.Instance.GetDisplayedBikeCount(_bikeData);

        if (bikeIconImage && _bikeData.BikeIcon) bikeIconImage.sprite = _bikeData.BikeIcon;
        if (bikeNameText) bikeNameText.text = _bikeData.DetailedName;
        if (unitsText) unitsText.text = $"Units: {unitsInInventory}, Placed {displayedCount}";
        if (statsText) statsText.text = $"S: {_bikeData.Speed}, H: {_bikeData.Handling}, A: {_bikeData.Acceleration}";

        // Can place if: has units in inventory AND room has an available station
        if (placeButton)
        {
            bool hasUnits = displayedCount < unitsInInventory;
            bool hasStation = _room && _room.GetAvailableStation() != null;
            placeButton.interactable = hasUnits && hasStation;
        }
    }

    private void OnPlaceClicked()
    {
        if (!_room || !_bikeData) return;

        var station = _room.GetAvailableStation();
        if (!station)
        {
            Debug.LogWarning("No available station in this room!");
            return;
        }

        // Check if we have bike in inventory
        if (!BikesManager.Instance.HasBike(_bikeData, 1))
        {
            Debug.LogWarning("No units in inventory!");
            return;
        }

        // Spawn bike on station
        var bike = station.SpawnBike(_bikeData, 1);

        _onPlaceBike?.Invoke();

        Debug.Log($"Placed {_bikeData.BikeName} on station".RichColor(Color.green), bike);
    }

    private void OnDestroy()
    {
        if (BikesManager.Instance) BikesManager.Instance.OnInventoryChanged -= UpdateDisplay;
    }
}