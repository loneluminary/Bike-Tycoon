using TUTORIAL_SYSTEM;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Utilities.Extensions;

public class BikeShopItem : MonoBehaviour
{
    [SerializeField] private Image bikeIconImage;
    [SerializeField] private TextMeshProUGUI bikeNameText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private Button buyButton;

    private BikeData _bikeData;

    public void Initialize(BikeData bike)
    {
        _bikeData = bike;

        UpdateDisplay();

        if (buyButton)
        {
            buyButton.onClick.AddListener(OnBuyClicked);
            if (!TutorialManager.Instance.AllTutorialsCompleted) TutorialManager.Instance.DelayedExecution(0.1f, SetupTutorial);
        }

        GameManager.Instance.OnCashChanged += UpdateDisplay;
    }

    private void SetupTutorial()
    {
        var tuto = TutorialManager.Instance.Tutorials[1];

        if (!tuto.Stages[1].IsStageCompleted) return; // Ensure previous stage is completed

        var thirdStage = tuto.Stages[2];
        if (thirdStage == null || thirdStage.EndButtonTarget || thirdStage.IsStageCompleted) return;

        thirdStage.StageEndTrigger = TutorialManager.TriggerType.ButtonClick;
        thirdStage.EndButtonTarget = buyButton;

        var hand = thirdStage.MyModules[0] as TutorialModule_DynamicHand;
        if (hand)
        {
            var point = new TutorialModule_DynamicHand.HandPointStruct { Point = buyButton.transform, Offset = new Vector3(1.2f, -0.1f), HandEventType = TutorialManager.HandEventType.Click };
            hand.Points = new() { point };
        }

        thirdStage.StartTheStage();
        thirdStage.StageStarted();
    }

    public void UpdateDisplay()
    {
        if (!_bikeData) return;

        if (bikeIconImage && _bikeData.BikeIcon) bikeIconImage.sprite = _bikeData.BikeIcon;
        if (bikeNameText) bikeNameText.text = $"{_bikeData.BikeName} ({_bikeData.BulkQuantity} Units)";
        if (priceText) priceText.text = $"${_bikeData.BasePrice:N0}";
        if (statsText) statsText.text = $"Speed: {_bikeData.Speed}, " + $"Handling: {_bikeData.Handling}, " + $"Accel: {_bikeData.Acceleration}";

        // Check if a player can afford and has space
        if (buyButton)
        {
            bool canAfford = GameManager.Instance.CurrentCash >= _bikeData.BasePrice;
            buyButton.interactable = canAfford;
        }
    }

    private void OnBuyClicked()
    {
        if (!_bikeData) return;

        // Check if can afford
        if (!GameManager.Instance.TrySpendCash(_bikeData.BasePrice)) return;

        transform.DOComplete();
        transform.DOPunchScale(Vector3.one * 0.2f, 0.3f);

        // Add to inventory instead of placing directly
        BikesManager.Instance.AddBike(_bikeData, _bikeData.BulkQuantity);
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnCashChanged -= UpdateDisplay;
    }
}