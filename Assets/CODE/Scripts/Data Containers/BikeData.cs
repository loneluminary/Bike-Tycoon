using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "New Bike", menuName = "Moto Gallery/Bike Data")]
public class BikeData : ScriptableObject
{
    [Title("Basic Info")] 
    [ReadOnly] public int ID;
    public string BikeName;
    public BrandData BrandData;
    public Sprite BikeIcon;
    public GameObject BikePrefab;
    public Material[] Upgrades;
    
    [Title("Pricing")]
    public int BasePrice;
    public int BulkQuantity = 10;
    
    [Title("Stats")]
    public float Speed = 50f;
    public float Handling = 30f;
    public float Acceleration = 40f;
    public float Durability = 20f;

    [Title("Rarity")]
    public BikeRarity Rarity;

    public string DetailedName => $"{BikeName} ({BrandData.BrandName})";
    
    public int GetPriceAtLevel(int level) => Mathf.RoundToInt(BasePrice * (1 + (level - 1) * 0.75f));
    
    public float GetStatMultiplier(int level) => 1 + (level - 1) * 0.5f;
    
    public int GetUpgradeCost(int currentLevel)
    {
        // Upgrade costs more at higher levels
        return Mathf.RoundToInt(BasePrice * 0.5f * currentLevel);
    }

    public int MaxLevel => Upgrades?.Length > 0 ? Upgrades.Length : 1;
}

public enum BikeRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}