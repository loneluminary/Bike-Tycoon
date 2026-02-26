using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Linq;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class SpecialRoomUpgradeItem : MonoBehaviour
{
    [SerializeField] Image icon;
    [SerializeField] private TextMeshProUGUI upgradeCostText;
    [SerializeField] private TextMeshProUGUI stackCountText;
    [SerializeField] private Button upgradeButton;

    [Title("Bonuses")]
    [SerializeField] private Transform nextBonusesContainer;
    [SerializeField] private TextMeshProUGUI bonusTemplateText;

    // Stack of segment indices
    private List<int> _segmentIndices = new();

    // Generic functions (not tied to any specific room type)
    private Func<int, bool> _isSegmentPurchased;
    private Func<int, int> _getSegmentCost;
    private Func<int, bool> _tryPurchaseSegment;
    private Func<bool> _onPurchaseCallback;

    private Func<bool> _canUpgrade;
    private Func<int> _getUpgradeCost;
    private Func<bool> _tryUpgrade;

    /// Initialize with a single segment (non-stacked)
    public void Initialize(Sprite iconSprite, Func<bool> canUpgrade, Func<int> getUpgradeCost, Func<bool> tryUpgrade)
    {
        _canUpgrade = canUpgrade;
        _getUpgradeCost = getUpgradeCost;
        _tryUpgrade = tryUpgrade;

        if (icon) icon.sprite = iconSprite;

        if (upgradeButton) upgradeButton.onClick.AddListener(OnUpgradeClicked);

        if (bonusTemplateText) bonusTemplateText.gameObject.SetActive(false);
        if (stackCountText) stackCountText.gameObject.SetActive(false);

        UpdateDisplay();

        GameManager.Instance.OnCashChanged += UpdateDisplay;
        GameManager.Instance.OnCashChanged += UpdateStackedDisplay;
    }

    /// Initialize with stacked segments (multiple of same type)
    public void InitializeStacked(Sprite iconSprite, List<int> segmentIndices, Func<int, bool> isSegmentPurchased, Func<int, int> getSegmentCost, Func<int, bool> tryPurchaseSegment, Func<bool> onPurchaseCallback = null)
    {
        _segmentIndices = new List<int>(segmentIndices);
        _isSegmentPurchased = isSegmentPurchased;
        _getSegmentCost = getSegmentCost;
        _tryPurchaseSegment = tryPurchaseSegment;
        _onPurchaseCallback = onPurchaseCallback;

        if (icon) icon.sprite = iconSprite;
        if (upgradeButton) upgradeButton.onClick.AddListener(OnStackedUpgradeClicked);
        if (bonusTemplateText) bonusTemplateText.gameObject.SetActive(false);

        UpdateStackedDisplay();
    }

    public void UpdateDisplay()
    {
        bool canUpgrade = _canUpgrade();
        int cost = _getUpgradeCost();

        if (upgradeButton) upgradeButton.interactable = canUpgrade && GameManager.Instance.CurrentCash >= cost;

        if (upgradeCostText)
        {
            if (canUpgrade) upgradeCostText.text = $"${cost:N0}";
            else upgradeCostText.text = "MAX LEVEL";
        }
    }

    public void UpdateStackedDisplay()
    {
        int nextSegmentIndex = GetNextUnpurchasedSegmentIndex();

        if (nextSegmentIndex == -1)
        {
            // All purchased - hide or disable
            if (upgradeButton) upgradeButton.interactable = false;
            if (upgradeCostText) upgradeCostText.text = "PURCHASED";
            if (stackCountText) stackCountText.gameObject.SetActive(false);
            return;
        }

        int cost = _getSegmentCost(nextSegmentIndex);

        if (upgradeButton) upgradeButton.interactable = GameManager.Instance.CurrentCash >= cost;
        if (upgradeCostText) upgradeCostText.text = $"${cost:N0}";

        if (stackCountText)
        {
            int remainingCount = GetRemainingStackCount();

            if (remainingCount > 1)
            {
                if (stackCountText) stackCountText.gameObject.SetActive(true);
                stackCountText.text = $"x{remainingCount}";
            }
            else
            {
                if (stackCountText) stackCountText.gameObject.SetActive(false);
            }
        }
    }

    public int GetNextUnpurchasedSegmentIndex()
    {
        if (_isSegmentPurchased == null) return -1;

        foreach (int index in _segmentIndices)
        {
            if (!_isSegmentPurchased(index))
            {
                return index;
            }
        }

        return -1; // All purchased
    }

    public int GetRemainingStackCount()
    {
        if (_isSegmentPurchased == null) return 0;

        int count = 0;
        foreach (int index in _segmentIndices)
        {
            if (!_isSegmentPurchased(index))
            {
                count++;
            }
        }
        return count;
    }

    private void OnUpgradeClicked()
    {
        if (_tryUpgrade())
        {
            transform.DOComplete();
            transform.DOPunchScale(Vector3.one * 0.2f, 0.5f);
            UpdateDisplay();
        }
    }

    private void OnStackedUpgradeClicked()
    {
        int nextSegmentIndex = GetNextUnpurchasedSegmentIndex();
        if (nextSegmentIndex == -1) return;

        if (_tryPurchaseSegment(nextSegmentIndex))
        {
            transform.DOComplete();
            transform.DOPunchScale(Vector3.one * 0.2f, 0.5f);

            // Update display to show next in stack or mark as complete
            UpdateStackedDisplay();

            // Invoke callback
            _onPurchaseCallback?.Invoke();
        }
    }

    public void AddText(string bonusText)
    {
        if (!nextBonusesContainer || !bonusTemplateText) return;

        var text = Instantiate(bonusTemplateText, nextBonusesContainer);
        text.text = $"• {bonusText}";

        text.gameObject.SetActive(true);
    }

    public void ClearAllTexts()
    {
        foreach (Transform child in nextBonusesContainer) Destroy(child.gameObject);
    }

    /// Get the sprite this item is using (for grouping)
    public Sprite GetIcon() => icon ? icon.sprite : null;

    private void OnDestroy()
    {
        GameManager.Instance.OnCashChanged -= UpdateDisplay;
        GameManager.Instance.OnCashChanged -= UpdateStackedDisplay;
    }
}