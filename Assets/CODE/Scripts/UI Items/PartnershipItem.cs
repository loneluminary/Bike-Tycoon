using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PartnershipItem : MonoBehaviour
{
    [SerializeField] private Image brandLogoImage;
    [SerializeField] private TextMeshProUGUI brandNameText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI salesText;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private Button viewBikesButton;
    [SerializeField] private Button upgradeButton;
    
    private BrandPartnership _partnership;
    private Action<BrandData> _onViewBikes;
    
    public void Initialize(BrandPartnership partnership, Action<BrandData> onViewBikes)
    {
        _partnership = partnership;
        _onViewBikes = onViewBikes;
        
        UpdateDisplay();
        
        if (viewBikesButton) viewBikesButton.onClick.AddListener(OnViewBikesClicked);
        if (upgradeButton) upgradeButton.onClick.AddListener(OnUpgradeClicked);

        GameManager.Instance.OnCashChanged += UpdateDisplay;
        GameManager.Instance.OnSalesChanged += UpdateDisplay;
    }
    
    public void UpdateDisplay()
    {
        if (_partnership == null || !_partnership.BrandData) 
        {
            gameObject.SetActive(false);
            return;
        }

        if (brandLogoImage && _partnership.BrandData.BrandLogo) brandLogoImage.sprite = _partnership.BrandData.BrandLogo;
        if (brandNameText) brandNameText.text = _partnership.BrandData.BrandName;
        if (levelText)
        {
            float royalty = _partnership.BrandData.GetRoyaltyPercentage(_partnership.CurrentLevel);
            levelText.text = $"Level: {_partnership.CurrentLevel}, {royalty * 100}% Royalty";
        }
        
        if (salesText)
        {
            int salesNeeded = _partnership.GetSalesUntilNextLevel() -  _partnership.TotalSales;
            salesText.text = salesNeeded > 0 ? $"Sales: {_partnership.TotalSales} ({salesNeeded} More To Upgrade)" : $"Sales: {_partnership.TotalSales} (Max Level)";
        }
  
        if (progressSlider) progressSlider.value = _partnership.GetProgressToNextLevel();
        
        // Update upgrade button
        if (upgradeButton)
        {
            int requiredSales = _partnership.BrandData.GetRequiredSales(_partnership.CurrentLevel);
            bool canUpgrade = _partnership.TotalSales >= requiredSales && _partnership.CurrentLevel != PartnershipLevel.Exclusive;
            upgradeButton.gameObject.SetActive(canUpgrade);
        }
    }
    
    private void OnViewBikesClicked()
    {
        _onViewBikes?.Invoke(_partnership.BrandData);
    }
    
    private void OnUpgradeClicked()
    {
        if (BrandManager.Instance.TryUpgradePartnership(_partnership.BrandData))
        {
            transform.DOComplete();
            transform.DOPunchScale(Vector3.one * 0.2f, 0.3f);

            UpdateDisplay();
        }
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnCashChanged -= UpdateDisplay;
        GameManager.Instance.OnSalesChanged -= UpdateDisplay;
    }
}