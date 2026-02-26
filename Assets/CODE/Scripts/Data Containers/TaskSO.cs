using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "NewTask", menuName = "Moto Gallery/Task Data")]
public class TaskSO : ScriptableObject
{
    [Title("Task Info")]
    public string TaskTitle = "Task Name";
    [TextArea(2, 4)] public string TaskDescription = "Complete this task!";
    public Sprite TaskIcon;
    public Sprite TaskCompleteIcon;
    
    [Title("Task Type")]
    public TaskType Type;
    
    [Title("Requirements")]
    [ShowIf("Type", TaskType.SellBikes)] public int RequiredSales = 10;
    [ShowIf("Type", TaskType.SellSpecificBrand)] public BrandData TargetBrand;
    [ShowIf("Type", TaskType.SellSpecificBrand)] public int RequiredBrandSales = 5;
    [ShowIf("Type", TaskType.HireStaff)] public int RequiredStaffCount = 3;
    [ShowIf("Type", TaskType.HireSpecificStaff)] public StaffType TargetStaffType;
    [ShowIf("Type", TaskType.HireSpecificStaff)] public int RequiredSpecificStaff = 1;
    [ShowIf("Type", TaskType.HireStaffInSpecificRoom)] public int RequiredStaffRoomID;
    [ShowIf("Type", TaskType.HireStaffInSpecificRoom)] public int RequiredStaffInRoom = 2;
    [ShowIf("Type", TaskType.PurchaseBikes)] public int RequiredBikePurchases = 5;
    [ShowIf("Type", TaskType.PurchaseSpecificBike)] public BikeData TargetBike;
    [ShowIf("Type", TaskType.PurchaseSpecificBike)] public int RequiredSpecificBikes = 3;
    [ShowIf("Type", TaskType.PlaceBikes)] public int RequiredPlacedBikes = 8;
    [ShowIf("Type", TaskType.UpgradeBikes)] public int RequiredUpgrades = 3;
    [ShowIf("Type", TaskType.ReachCash)] public int RequiredCash = 50000;
    [ShowIf("Type", TaskType.SignBrandDeal)] public int RequiredBrandDeals = 1;
    [ShowIf("Type", TaskType.UpgradeRoom)] public int RequiredRoomLevel = 2;
    
    [Title("Rewards")]
    public int CashReward = 1000;
    
    [Title("Duration")]
    public float TimeLimit = 0f; // 0 = no time limit
}

public enum TaskType
{
    SellBikes,              // Sell X bikes total
    SellSpecificBrand,      // Sell X bikes from specific brand
    HireStaff,              // Hire X staff members
    HireSpecificStaff,      // Hire specific staff type
    HireStaffInSpecificRoom, // Hire X staff in specific room
    PurchaseBikes,          // Purchase X bikes
    PurchaseSpecificBike,   // Purchase specific bike
    PlaceBikes,             // Place X bikes on stations
    UpgradeBikes,           // Upgrade X bikes
    ReachCash,              // Reach X cash amount
    SignBrandDeal,          // Sign X brand deals
    UpgradeRoom             // Upgrade any room to level X
}