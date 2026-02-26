using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using Utilities.Extensions;

[SelectionBase]
[RequireComponent(typeof(BoxCollider), typeof(AudioSource))]
public class BikeInstance : MonoBehaviour, IPointerClickHandler
{
    [Header("Bike Data")]
    public BikeData BikeData;
    public int CurrentLevel = 1;

    [Header("Selection Settings")]
    [SerializeField] private float selectBounceScale = 0.15f;
    [SerializeField] private GameObject mergeIndicator;

    [Title("Sounds")]
    [SerializeField] private AudioClip upgradeSound;
    [SerializeField] private AudioClip sellSound;
    [SerializeField] private AudioSource audioSource;

    // Properties
    public int GetUpgradeCost => BikeData?.GetUpgradeCost(CurrentLevel) ?? 0;
    public bool IsMaxLevel => CurrentLevel >= (BikeData?.MaxLevel ?? 1);

    private MeshRenderer[] _renderers;
    private Tween _selectionTween;
    private Tween _hoverTween;
    private Vector3 _originalScale;
    private bool _isSelected;

    public static BikeInstance SelectedBike_1, SelectedBike_2;

    public void Initialize(BikeData data, int level)
    {
        BikeData = data;
        CurrentLevel = level;
        _renderers = Instantiate(BikeData.BikePrefab, transform, false).GetComponentsInChildren<MeshRenderer>();
        _originalScale = transform.localScale;

        var bounds = transform.CalculateLocalBounds(false);
        var col = GetComponent<BoxCollider>();
        col.size = bounds.size;
        col.center = bounds.center;

        UpdateVisuals();

        BikesManager.Instance.ActiveBikes.TryAdd(this);

        if (!audioSource) audioSource = GetComponent<AudioSource>();
    }

    private void UpdateVisuals()
    {
        if (!BikeData.Upgrades.IsNullOrEmpty())
        {
            var upgrade = BikeData.Upgrades[CurrentLevel - 1];
            foreach (var renderer in _renderers) renderer.material = upgrade;
        }

        transform.DOComplete();
        transform.DOPunchScale(transform.lossyScale * 0.2f, 0.3f);
    }

    #region Merging & Selection

    public void OnPointerClick(PointerEventData eventData)
    {
        // If clicking on the already selected bike, deselect it
        if (_isSelected)
        {
            Deselect();
            return;
        }

        // Handle selection logic
        if (!SelectedBike_1)
        {
            SelectedBike_1 = this;
            Select();
        }
        else if (!SelectedBike_2 && SelectedBike_1 != this)
        {
            SelectedBike_2 = this;
            Select();

            // Try merge
            if (CanMerge(SelectedBike_1, SelectedBike_2))
            {
                Merge(SelectedBike_1, SelectedBike_2);
            }
            else
            {
                // Can't merge - show feedback and clear selection
                // Shake both bikes to indicate invalid merge
                SelectedBike_1?.transform.DOShakePosition(0.3f, 0.2f, 15);
                SelectedBike_2?.transform.DOShakePosition(0.3f, 0.2f, 15);
                ClearAllSelections();
            }
        }
    }

    private void Select()
    {
        _isSelected = true;

        _selectionTween?.Kill();

        // Show merge indicator
        if (mergeIndicator)
        {
            mergeIndicator.SetActive(true);
            mergeIndicator.transform.localScale = Vector3.zero;
            mergeIndicator.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack);
        }

        // Bounce and hover animation
        var seq = DOTween.Sequence();
        seq.Append(transform.DOPunchScale(Vector3.one * selectBounceScale, 0.3f, 8));
        seq.AppendCallback(() =>
        {
            // Continuous gentle hover while selected
            _hoverTween = transform.DOScale(_originalScale * 1.05f, 0.6f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
        });

        _selectionTween = seq;
    }

    public void Deselect()
    {
        _isSelected = false;

        _selectionTween?.Kill();
        _hoverTween?.Kill();

        // Hide merge indicator
        if (mergeIndicator)
        {
            mergeIndicator.transform.DOScale(0f, 0.2f).SetEase(Ease.InBack).OnComplete(() => mergeIndicator.SetActive(false));
        }

        // Return to original scale
        transform.DOScale(_originalScale, 0.2f).SetEase(Ease.OutSine);

        // Clear from static selection
        if (SelectedBike_1 == this) SelectedBike_1 = null;
        if (SelectedBike_2 == this) SelectedBike_2 = null;
    }

    public static void ClearAllSelections()
    {
        SelectedBike_1?.Deselect();
        SelectedBike_2?.Deselect();
        SelectedBike_1 = null;
        SelectedBike_2 = null;
    }

    public static bool CanMerge(BikeInstance first, BikeInstance sec)
    {
        bool isSameModelBike = first != sec && first.BikeData == sec.BikeData && first.CurrentLevel == sec.CurrentLevel;
        bool isSameLevel = first.CurrentLevel == sec.CurrentLevel && !first.IsMaxLevel;
        if (first.TryGetComponentInParent(out GalleryRoom room_1) && sec.TryGetComponentInParent(out GalleryRoom room_2)) return room_1 == room_2 && isSameModelBike && isSameLevel;
        return isSameModelBike && isSameLevel;
    }

    public static void Merge(BikeInstance first, BikeInstance sec)
    {
        // Play merge animation
        var seq = DOTween.Sequence();

        // Move first bike toward second
        seq.Append(first.transform.DOMove(sec.transform.position, 0.3f).SetEase(Ease.InQuad));
        seq.Join(first.transform.DOScale(0f, 0.3f).SetEase(Ease.InBack));

        // Pop effect on a merged bike
        seq.AppendCallback(() =>
        {
            sec.CurrentLevel++;
            sec.UpdateVisuals();
            sec.Deselect();

            BikesManager.Instance.OnBikesMerged(first, sec);

            Debug.Log($"Merged {first.BikeData.BikeName} with {sec.BikeData.BikeName} : Level {sec.CurrentLevel}");

            Destroy(first.gameObject);
        });

        // Remove one from the inventory.
        BikesManager.Instance.TryRemoveBike(first.BikeData, 1);

        // Clear selections
        SelectedBike_1 = null;
        SelectedBike_2 = null;
    }

    #endregion

    public int GetSalePrice()
    {
        int basePrice = BikeData.GetPriceAtLevel(CurrentLevel);
        float royalty = BrandManager.HasInstance?.GetCurrentRoyalty(BikeData.BrandData) ?? 0.1f;

        // Apply manager bonus
        float managerBonus = StaffManager.Instance.GetGalleryProfitBonus();
        basePrice = Mathf.RoundToInt(basePrice * (1 + managerBonus));

        // Apply marketing bonus
        var marketingRoom = FindFirstObjectByType<MarketingRoom>();
        if (marketingRoom && marketingRoom.IsUnlocked)
        {
            float marketingBonus = marketingRoom.GetTotalSalePriceBonus();
            basePrice = Mathf.RoundToInt(basePrice * (1 + marketingBonus));
        }

        return Mathf.RoundToInt(basePrice * (1 - royalty));
    }

    public void Sell()
    {
        int salePrice = GetSalePrice();
        GameManager.Instance.AddCash(salePrice);
        UIManager.Instance.CashAddingAnimation(Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 2f));
        BrandManager.Instance?.RecordSale(BikeData.BrandData.BrandName);

        // Remove from inventory
        BikesManager.Instance.TryRemoveBike(BikeData, 1);

        audioSource?.PlayOneShot(sellSound);

        BikesManager.Instance.OnBikeSoldFromInstance(this);
    }

    public bool TryUpgrade()
    {
        if (!BikeData || IsMaxLevel) return false;

        int cost = GetUpgradeCost;
        if (!GameManager.Instance.TrySpendCash(cost)) return false;

        // Upgrade the bike
        CurrentLevel++;
        UpdateVisuals();

        TaskManager.Instance.OnBikeUpgraded(BikeData);
        BikesManager.Instance.OnBikeUpgrades(this);

        audioSource.PlayOneShot(upgradeSound);

        Debug.Log($"Upgraded {BikeData.BikeName} to Level {CurrentLevel} for ${cost}".RichColor(Color.green));
        return true;
    }

    private void OnDestroy()
    {
        _selectionTween?.Kill();
        _hoverTween?.Kill();

        if (SelectedBike_1 == this) SelectedBike_1 = null;
        if (SelectedBike_2 == this) SelectedBike_2 = null;

        BikesManager.Instance.ActiveBikes.TryRemove(this);
    }
}

[Serializable]
public class PlacedBikeSaveData
{
    public int RoomID;
    public int StationIndex;
    public int BikeID;
    public int BikeLevel;

    public PlacedBikeSaveData(int roomID, int stationIndex, int bikeID, int bikeLevel)
    {
        RoomID = roomID;
        StationIndex = stationIndex;
        BikeID = bikeID;
        BikeLevel = bikeLevel;
    }
}