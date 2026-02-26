using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlacedBikeItem : MonoBehaviour
{
    [SerializeField] Image bikeIconImage;
    [SerializeField] private TextMeshProUGUI bikeNameText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI upgradeCostText;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button removeButton;

    private BikeInstance _bikeInstance;
    private DisplayStation _station;
    private System.Action _onRemoved;

    public void Initialize(DisplayStation station, System.Action onRemoved = null)
    {
        _station = station;
        _bikeInstance = station.CurrentBike;
        _onRemoved = onRemoved;

        if (!_bikeInstance)
        {
            gameObject.SetActive(false);
            return;
        }

        // Setup buttons
        if (upgradeButton) upgradeButton.onClick.AddListener(() =>
        {
            if (_bikeInstance.TryUpgrade())
            {
                transform.DOKill();
                transform.DOPunchScale(Vector3.one * 0.2f, 0.3f);
            }
        });

        if (removeButton) removeButton.onClick.AddListener(OnRemoveClicked);

        UpdateDisplay();

        GameManager.Instance.OnCashChanged += UpdateDisplay;
        BikesManager.Instance.OnBikeUpgraded += OnBikeUpgraded;
    }

    public void UpdateDisplay()
    {
        if (!_bikeInstance || !_bikeInstance.BikeData)
        {
            gameObject.SetActive(false);
            return;
        }

        var bikeData = _bikeInstance.BikeData;

        if (bikeIconImage && bikeData.BikeIcon) bikeIconImage.sprite = bikeData.BikeIcon;
        if (bikeNameText) bikeNameText.text = bikeData.DetailedName;
        if (levelText) levelText.text = $"Lv. {_bikeInstance.CurrentLevel}";

        // Upgrade button state
        if (upgradeButton)
        {
            bool canUpgrade = !_bikeInstance.IsMaxLevel && GameManager.Instance.CurrentCash >= _bikeInstance.GetUpgradeCost;
            upgradeButton.interactable = canUpgrade;
        }

        if (upgradeCostText) upgradeCostText.text = _bikeInstance.IsMaxLevel ? "MAX" : $"${_bikeInstance.GetUpgradeCost}";
    }

    private void OnRemoveClicked()
    {
        if (!_bikeInstance || !_station) return;

        _station.RemoveBike();

        _onRemoved?.Invoke();

        Destroy(gameObject);
    }

    private void OnBikeUpgraded(BikeInstance bike) { if (bike == _bikeInstance) UpdateDisplay(); }

    private void OnDestroy()
    {
        BikesManager.Instance.OnBikeUpgraded -= OnBikeUpgraded;
        GameManager.Instance.OnCashChanged -= UpdateDisplay;
    }
}