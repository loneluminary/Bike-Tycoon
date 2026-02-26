using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewBrand", menuName = "Moto Gallery/Brand Data")]
public class BrandData : ScriptableObject
{
    [Header("Brand Identity")]
    public string BrandName = "Brand Name";
    public Sprite BrandLogo;
    
    [Header("Partnership Costs")]
    public int BasicDealCost = 5000;
    public int SilverPartnerCost = 25000;
    public int GoldPartnerCost = 100000;
    public int ExclusivePartnerCost; // Earned through challenges
    
    [Header("Royalty Percentages")]
    [Range(0.01f, 0.20f)] public float BasicRoyalty = 0.10f;    // 10%
    [Range(0.01f, 0.20f)] public float SilverRoyalty = 0.07f;   // 7%
    [Range(0.01f, 0.20f)] public float GoldRoyalty = 0.05f;     // 5%
    [Range(0.01f, 0.20f)] public float ExclusiveRoyalty = 0.03f; // 3%
    
    [Header("Sales Requirements")]
    public int SilverRequirement = 50;    // Sales needed for Silver
    public int GoldRequirement = 200;     // Sales needed for Gold
    public int exclusiveRequirement = 500; // Sales needed for Exclusive
    
    [Header("Bike Unlocks by Partnership Level")]
    public List<BikeData> BasicBikes = new();
    public List<BikeData> SilverBikes = new();
    public List<BikeData> GoldBikes = new();
    public List<BikeData> ExclusiveBikes = new();
    
    [Header("Brand Challenges (For Exclusive Level)")]
    public string[] ExclusiveChallenges = new string[3];
    
    // Public methods for BrandManager to use
    public int GetRequiredSales(PartnershipLevel currentLevel)
    {
        return currentLevel switch
        {
            PartnershipLevel.Basic => SilverRequirement,
            PartnershipLevel.Silver => GoldRequirement,
            PartnershipLevel.Gold => exclusiveRequirement,
            PartnershipLevel.Exclusive => 0, // Max level reached
            _ => 0
        };
    }
    
    public float GetRoyaltyPercentage(PartnershipLevel currentLevel)
    {
        return currentLevel switch
        {
            PartnershipLevel.Basic => BasicRoyalty,
            PartnershipLevel.Silver => SilverRoyalty,
            PartnershipLevel.Gold => GoldRoyalty,
            PartnershipLevel.Exclusive => ExclusiveRoyalty,
            _ => BasicRoyalty
        };
    }
    
    public List<BikeData> GetAvailableBikes(PartnershipLevel currentLevel)
    {
        List<BikeData> availableBikes = new List<BikeData>();
        
        // Always include basic bikes
        availableBikes.AddRange(BasicBikes);
        
        if (currentLevel >= PartnershipLevel.Silver)
            availableBikes.AddRange(SilverBikes);
        
        if (currentLevel >= PartnershipLevel.Gold)
            availableBikes.AddRange(GoldBikes);
        
        if (currentLevel >= PartnershipLevel.Exclusive)
            availableBikes.AddRange(ExclusiveBikes);
        
        return availableBikes;
    }
    
    public int GetUpgradeCost(PartnershipLevel targetLevel)
    {
        return targetLevel switch
        {
            PartnershipLevel.Silver => SilverPartnerCost,
            PartnershipLevel.Gold => GoldPartnerCost,
            PartnershipLevel.Exclusive => ExclusivePartnerCost,
            _ => 0
        };
    }
    
    public bool CanUpgradeTo(PartnershipLevel currentLevel, int totalSales)
    {
        if (currentLevel == PartnershipLevel.Exclusive) return false;
        
        int requiredSales = GetRequiredSales(currentLevel);
        return totalSales >= requiredSales;
    }
    
    public string GetExclusiveChallenge(int index)
    {
        if (index >= 0 && index < ExclusiveChallenges.Length)
            return ExclusiveChallenges[index];
        return "Complete brand challenges";
    }
    
    // Editor helper methods (for Unity Editor only)
    #if UNITY_EDITOR
    public void AddBasicBike(BikeData bike)
    {
        if (!BasicBikes.Contains(bike))
            BasicBikes.Add(bike);
    }
    
    public void AddSilverBike(BikeData bike)
    {
        if (!SilverBikes.Contains(bike))
            SilverBikes.Add(bike);
    }
    
    public void AddGoldBike(BikeData bike)
    {
        if (!GoldBikes.Contains(bike))
            GoldBikes.Add(bike);
    }
    
    public void AddExclusiveBike(BikeData bike)
    {
        if (!ExclusiveBikes.Contains(bike))
            ExclusiveBikes.Add(bike);
    }
    
    public void SetBrandInfo(string name, Sprite logo)
    {
        BrandName = name;
        BrandLogo = logo;
    }
    #endif
}