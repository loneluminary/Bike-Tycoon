using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities.Extensions;

public class BrandManager : MonoSingleton<BrandManager>
{
    public List<BrandData> AllBrands = new();
    [ReadOnly] public List<BrandPartnership> ActivePartnerships = new();

    public event Action<BrandPartnership> OnPartnershipSigned;
    public event Action<BrandPartnership, PartnershipLevel> OnPartnershipUpgraded;
    public event Action<string, int> OnBrandSaleRecorded;

    public bool TrySignBrandDeal(BrandData brand)
    {
        if (brand == null) return false;
        if (HasPartnershipWith(brand)) return false;

        int cost = brand.BasicDealCost;

        // Apply race discount if available
        float discount = RacingManager.Instance.GetBrandDiscount(brand.BrandName);
        if (discount > 0f)
        {
            cost = Mathf.RoundToInt(cost * (1 - discount));
            Debug.Log($"Race discount applied! {discount * 100}% off".RichColor(Color.gold));
            RacingManager.Instance.ClearBrandDiscount(brand.BrandName);
        }

        if (!GameManager.Instance.TrySpendCash(cost)) return false;

        BrandPartnership partnership = new BrandPartnership
        {
            BrandData = brand,
            CurrentLevel = PartnershipLevel.Basic,
            TotalSales = 0,
        };

        ActivePartnerships.Add(partnership);
        OnPartnershipSigned?.Invoke(partnership);

        return true;
    }

    public bool TryUpgradePartnership(BrandData brand)
    {
        BrandPartnership partnership = GetPartnership(brand);
        if (partnership == null) return false;

        PartnershipLevel nextLevel = GetNextLevel(partnership.CurrentLevel);
        if (nextLevel == PartnershipLevel.None) return false;

        // Check if sales requirement is met
        int requiredSales = brand.GetRequiredSales(partnership.CurrentLevel);
        if (partnership.TotalSales < requiredSales) return false;

        // Check if cost can be paid (for initial upgrades)
        int cost = GetUpgradeCost(brand, nextLevel);
        if (cost > 0 && !GameManager.Instance.TrySpendCash(cost)) return false;

        partnership.CurrentLevel = nextLevel;
        OnPartnershipUpgraded?.Invoke(partnership, nextLevel);

        return true;
    }

    public void RecordSale(string brandName)
    {
        BrandPartnership partnership = ActivePartnerships.FirstOrDefault(p => p.BrandData.BrandName == brandName);
        if (partnership != null)
        {
            partnership.TotalSales++;
            OnBrandSaleRecorded?.Invoke(brandName, partnership.TotalSales);

            // Auto-check for upgrade eligibility
            TryAutoUpgrade(partnership);
        }
    }

    private void TryAutoUpgrade(BrandPartnership partnership)
    {
        int requiredSales = partnership.BrandData.GetRequiredSales(partnership.CurrentLevel);
        if (partnership.TotalSales >= requiredSales)
        {
            PartnershipLevel nextLevel = GetNextLevel(partnership.CurrentLevel);
            if (nextLevel != PartnershipLevel.None && GetUpgradeCost(partnership.BrandData, nextLevel) == 0)
            {
                partnership.CurrentLevel = nextLevel;
                OnPartnershipUpgraded?.Invoke(partnership, nextLevel);
            }
        }
    }

    public List<BikeData> GetAvailableBikes(BrandData brand)
    {
        BrandPartnership partnership = GetPartnership(brand);
        if (partnership == null) return new List<BikeData>();

        return brand.GetAvailableBikes(partnership.CurrentLevel);
    }

    public float GetCurrentRoyalty(BrandData brand)
    {
        BrandPartnership partnership = GetPartnership(brand);
        if (partnership == null) return 0.10f;

        return brand.GetRoyaltyPercentage(partnership.CurrentLevel);
    }

    public bool HasPartnershipWith(BrandData brand)
    {
        return ActivePartnerships.Any(p => p.BrandData == brand);
    }

    public BrandPartnership GetPartnership(BrandData brand)
    {
        return ActivePartnerships.FirstOrDefault(p => p.BrandData == brand);
    }

    public List<BrandData> GetAvailableBrands()
    {
        return AllBrands.Where(b => !HasPartnershipWith(b)).ToList();
    }

    public List<BrandPartnership> GetActivePartnerships()
    {
        return new List<BrandPartnership>(ActivePartnerships);
    }

    private PartnershipLevel GetNextLevel(PartnershipLevel current)
    {
        return current switch
        {
            PartnershipLevel.Basic => PartnershipLevel.Silver,
            PartnershipLevel.Silver => PartnershipLevel.Gold,
            PartnershipLevel.Gold => PartnershipLevel.Exclusive,
            _ => PartnershipLevel.None
        };
    }

    private int GetUpgradeCost(BrandData brand, PartnershipLevel targetLevel)
    {
        return targetLevel switch
        {
            PartnershipLevel.Silver => brand.SilverPartnerCost,
            PartnershipLevel.Gold => brand.GoldPartnerCost,
            PartnershipLevel.Exclusive => 0, // Earned through sales
            _ => 0
        };
    }

    // Save/Load
    public string SavePartnerships()
    {
        var saveData = ActivePartnerships.Select(p => new PartnershipSaveData
        {
            BrandName = p.BrandData.BrandName,
            Level = (int)p.CurrentLevel,
            Sales = p.TotalSales
        }).ToList();

        ES3.Save("BrandPartnerships", saveData);

        return $"Saved {saveData.Count} partnerships";
    }

    public string LoadPartnerships()
    {
        ActivePartnerships.Clear();

        var saveDataList = ES3.Load("BrandPartnerships", new List<PartnershipSaveData>());

        foreach (var data in saveDataList)
        {
            BrandData brand = AllBrands.FirstOrDefault(b => b.BrandName == data.BrandName);

            if (brand != null)
            {
                BrandPartnership partnership = new BrandPartnership
                {
                    BrandData = brand,
                    CurrentLevel = (PartnershipLevel)data.Level,
                    TotalSales = data.Sales,
                };
                ActivePartnerships.Add(partnership);
            }
        }

        return $"Loaded {saveDataList.Count} partnerships";
    }
}

[Serializable]
public struct PartnershipSaveData
{
    public string BrandName;
    public int Level;
    public int Sales;
}

[Serializable]
public class BrandPartnership
{
    public BrandData BrandData;
    public PartnershipLevel CurrentLevel;
    public int TotalSales;

    public int GetSalesUntilNextLevel()
    {
        int required = BrandData.GetRequiredSales(CurrentLevel);
        return Mathf.Max(0, required - TotalSales);
    }

    public float GetProgressToNextLevel()
    {
        int required = BrandData.GetRequiredSales(CurrentLevel);
        if (required == 0) return 1f;
        return Mathf.Clamp01((float)TotalSales / required);
    }

    public bool CanUpgrade()
    {
        return BrandData.CanUpgradeTo(CurrentLevel, TotalSales);
    }

    public int CompareTo(object obj)
    {
        throw new NotImplementedException();
    }
}

public enum PartnershipLevel
{
    None = 0,
    Basic = 1,      // 2 bikes, 10% royalty
    Silver = 2,     // 5 bikes, 7% royalty (after 50 sales)
    Gold = 3,       // All bikes, 5% royalty (after 200 sales)
    Exclusive = 4   // Special editions (after 500 sales + challenges)
}