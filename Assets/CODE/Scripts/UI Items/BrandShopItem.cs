using System;
using TUTORIAL_SYSTEM;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Utilities.Extensions;

public class BrandShopItem : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image brandLogoImage;
    [SerializeField] private TextMeshProUGUI brandNameText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI bikeCountText;
    [SerializeField] private Button signDealButton;

    private BrandData _brandData;
    private Action<BrandData> _onSignDeal;

    public void Initialize(BrandData brand, Action<BrandData> onSignDeal)
    {
        _brandData = brand;
        _onSignDeal = onSignDeal;

        UpdateDisplay();

        if (signDealButton)
        {
            signDealButton.onClick.AddListener(OnSignDealClicked);
            if (!TutorialManager.Instance.AllTutorialsCompleted) TutorialManager.Instance.DelayedExecution(0.1f, SetupTutorial);
        }

        GameManager.Instance.OnCashChanged += UpdateDisplay;
    }

    private void SetupTutorial()
    {
        var tuto = TutorialManager.Instance.Tutorials[1];

        if (!tuto.Stages[0].IsStageCompleted) return; // Ensure previous stage is completed

        var secStage = tuto.Stages[1];
        if (secStage == null || secStage.EndButtonTarget || secStage.IsStageCompleted) return;

        secStage.StageEndTrigger = TutorialManager.TriggerType.ButtonClick;
        secStage.EndButtonTarget = signDealButton;

        var hand = secStage.MyModules[0] as TutorialModule_DynamicHand;
        if (hand)
        {
            var point = new TutorialModule_DynamicHand.HandPointStruct { Point = signDealButton.transform, Offset = new Vector3(0.9f, -0.1f), HandEventType = TutorialManager.HandEventType.Click };
            hand.Points = new() { point };
        }

        secStage.StartTheStage();
        secStage.StageStarted();
    }

    public void UpdateDisplay()
    {
        if (!_brandData) return;

        if (brandLogoImage && _brandData.BrandLogo) brandLogoImage.sprite = _brandData.BrandLogo;
        if (brandNameText) brandNameText.text = _brandData.BrandName;
        if (costText) costText.text = $"${_brandData.BasicDealCost:N0}";
        if (bikeCountText) bikeCountText.text = $"{_brandData.BasicBikes.Count} Bikes, {_brandData.GetRoyaltyPercentage(PartnershipLevel.Basic) * 100}% Royalty";

        // Check if the player can afford
        if (signDealButton) signDealButton.interactable = GameManager.Instance.CurrentCash >= _brandData.BasicDealCost;
    }

    private void OnSignDealClicked()
    {
        if (!BrandManager.Instance.TrySignBrandDeal(_brandData)) return;

        transform.DOComplete();
        transform.DOPunchScale(Vector3.one * 0.2f, 0.3f);

        _onSignDeal?.Invoke(_brandData);
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnCashChanged -= UpdateDisplay;
    }
}