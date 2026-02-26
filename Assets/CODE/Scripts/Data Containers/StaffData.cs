using UnityEngine;

[CreateAssetMenu(fileName = "NewStaff", menuName = "Moto Gallery/Staff Data")]
public class StaffData : ScriptableObject
{
    [Header("Basic Info")]
    public string StaffName = "Staff Member";
    public StaffType StaffType;
    public Sprite StaffIcon;

    public Animation StaffModel;
    
    [Header("Costs")]
    public int HireCost = 1000;
    public int UpgradeCostPerLevel = 500;
    
    [Header("Bonuses (Per Level)")]
    [Tooltip("Salesperson: Increases sale speed")]
    public float SaleSpeedBonus = 0.10f; // +10% per level
    
    [Tooltip("Marketer: Attracts more customers")]
    public float CustomerAttractionBonus = 0.15f; // +15% per level
    
    [Tooltip("Manager: Increases profit per sale")]
    public float ProfitBonus = 0.05f; // +5% per level
    
    [Tooltip("Mechanic: Speeds up bike merges/maintenance")]
    public float MergeSpeedBonus = 0.20f; // +20% faster merges
    
    [Header("Max Level")]
    public int MaxLevel = 10;
}

public enum StaffType
{
    Salesperson,  // Increases sale probability/speed at stations
    Marketer,     // Attracts more customers to gallery
    Manager,      // Increases profit % from sales
    Mechanic      // Helps with merges, unlocks auto-merge features
}