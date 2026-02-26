using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities.Extensions;

[CreateAssetMenu(fileName = "TaskGenerationConfig", menuName = "Moto Gallery/Task Generation Config")]
public class TaskGenerationConfig : ScriptableObject
{
    [Title("Task Generation Settings")]
    [InfoBox("Configure how dynamic tasks are generated once the initial pool is exhausted.")]

    [Title("Task Types")]
    [Tooltip("Which task types can be randomly generated")]
    public List<TaskType> availableTaskTypes = new()
    {
        TaskType.SellBikes,
        TaskType.SellSpecificBrand,
        TaskType.PurchaseBikes,
        TaskType.PurchaseSpecificBike,
        TaskType.SignBrandDeal,
        TaskType.HireStaff,
        TaskType.PlaceBikes,
        TaskType.UpgradeBikes,
        TaskType.ReachCash
    };

    [Title("Difficulty Scaling")]
    [Tooltip("Difficulty increases over time")]
    public bool enableDifficultyScaling = true;
    [ShowIf("enableDifficultyScaling")] public float difficultyMultiplier = 1.2f;
    [ShowIf("enableDifficultyScaling")] public int tasksUntilScaling = 5;

    [Title("Reward Settings")]
    [Range(500, 10000)] public int baseRewardMin = 1000;
    [Range(1000, 20000)] public int baseRewardMax = 5000;

    [Title("Requirement Ranges")]
    [BoxGroup("Sales")] public Vector2Int sellBikesRange = new(5, 15);
    [BoxGroup("Purchases")] public Vector2Int purchaseBikesRange = new(3, 10);
    [BoxGroup("Staff")] public Vector2Int hireStaffRange = new(1, 5);
    [BoxGroup("Placement")] public Vector2Int placeBikesRange = new(5, 12);
    [BoxGroup("Upgrades")] public Vector2Int upgradeBikesRange = new(2, 8);
    [BoxGroup("Cash")] public Vector2Int reachCashRange = new(10000, 100000);

    [Title("Visual Assets")]
    public List<Sprite> taskIcons = new();
    public List<Sprite> taskCompleteIcons = new();

    private int _tasksGenerated = 0;

    public TaskSO GenerateRandomTask()
    {
        if (availableTaskTypes.Count == 0)
        {
            Debug.LogError("No available task types configured!");
            return null;
        }

        // Pick random task type
        TaskType randomType = availableTaskTypes.GetRandom();
        // if task is to sign brand deal and theres no deal to sign regenerate
        if (randomType == TaskType.SignBrandDeal && BrandManager.Instance.ActivePartnerships.Count >= BrandManager.Instance.AllBrands.Count) GenerateRandomTask();
        int typeIndex = availableTaskTypes.IndexOf(randomType);

        // Create runtime instance
        TaskSO newTask = CreateInstance<TaskSO>();
        
        // Set basic properties
        newTask.Type = randomType;
        newTask.TaskIcon = (taskIcons != null && typeIndex < taskIcons.Count) ? taskIcons[typeIndex] : null;
        bool hasCompleteIcon = taskCompleteIcons != null && typeIndex < taskCompleteIcons.Count;
        bool hasAnyIcons = taskCompleteIcons != null && taskCompleteIcons.Count > 0;
        newTask.TaskCompleteIcon = hasCompleteIcon ? taskCompleteIcons[typeIndex] : (hasAnyIcons ? taskCompleteIcons[0] : null);

        // Calculate difficulty multiplier
        float difficultyScale = 1f;
        if (enableDifficultyScaling)
        {
            int scalingLevel = _tasksGenerated / tasksUntilScaling;
            difficultyScale = Mathf.Pow(difficultyMultiplier, scalingLevel);
        }

        // Set requirements based on type
        switch (randomType)
        {
            case TaskType.SellBikes:
                int sellAmount = Mathf.RoundToInt(Random.Range(sellBikesRange.x, sellBikesRange.y) * difficultyScale);
                newTask.RequiredSales = sellAmount;
                newTask.TaskTitle = $"Sell {sellAmount} Bikes";
                newTask.TaskDescription = $"Make {sellAmount} sales to customers";
                newTask.CashReward = Mathf.RoundToInt(sellAmount * Random.Range(80f, 150f));
                break;

            case TaskType.PurchaseBikes:
                int purchaseAmount = Mathf.RoundToInt(Random.Range(purchaseBikesRange.x, purchaseBikesRange.y) * difficultyScale);
                newTask.RequiredBikePurchases = purchaseAmount;
                newTask.TaskTitle = $"Purchase {purchaseAmount} Bikes";
                newTask.TaskDescription = $"Buy {purchaseAmount} bikes from suppliers";
                newTask.CashReward = Mathf.RoundToInt(purchaseAmount * Random.Range(100f, 200f));
                break;

            case TaskType.PurchaseSpecificBike:
                var allBikes = BikesManager.Instance.AllBikesData;
                if (!allBikes.IsNullOrEmpty())
                {
                    BikeData randomBike = allBikes.GetRandom();
                    int specificBikeAmount = Mathf.RoundToInt(Random.Range(1, 5) * difficultyScale);
                    newTask.TargetBike = randomBike;
                    newTask.RequiredSpecificBikes = specificBikeAmount;
                    newTask.TaskTitle = $"Purchase {specificBikeAmount} {randomBike.BikeName}";
                    newTask.TaskDescription = $"Buy {specificBikeAmount} {randomBike.BikeName} ({randomBike.BrandData.BrandName}) bikes";
                    newTask.CashReward = Mathf.RoundToInt(specificBikeAmount * Random.Range(200f, 400f));
                }
                break;

            case TaskType.SellSpecificBrand:
                var allBrands = BrandManager.Instance.AllBrands;
                if (!allBrands.IsNullOrEmpty())
                {
                    BrandData randomBrand = allBrands.GetRandom();
                    int brandSaleAmount = Mathf.RoundToInt(Random.Range(3, 10) * difficultyScale);
                    newTask.TargetBrand = randomBrand;
                    newTask.RequiredBrandSales = brandSaleAmount;
                    newTask.TaskTitle = $"Sell {brandSaleAmount} {randomBrand.BrandName}";
                    newTask.TaskDescription = $"Sell {brandSaleAmount} bikes from {randomBrand.BrandName}";
                    newTask.CashReward = Mathf.RoundToInt(brandSaleAmount * Random.Range(150f, 300f));
                }
                break;

            case TaskType.HireStaff:
                int staffAmount = Mathf.RoundToInt(Random.Range(hireStaffRange.x, hireStaffRange.y) * difficultyScale);
                newTask.RequiredStaffCount = staffAmount;
                newTask.TaskTitle = $"Hire {staffAmount} Staff";
                newTask.TaskDescription = $"Recruit {staffAmount} new staff members";
                newTask.CashReward = Mathf.RoundToInt(staffAmount * Random.Range(300f, 600f));
                break;

            case TaskType.PlaceBikes:
                int placeAmount = Mathf.RoundToInt(Random.Range(placeBikesRange.x, placeBikesRange.y) * difficultyScale);
                newTask.RequiredPlacedBikes = placeAmount;
                newTask.TaskTitle = $"Display {placeAmount} Bikes";
                newTask.TaskDescription = $"Place {placeAmount} bikes on display stations";
                newTask.CashReward = Mathf.RoundToInt(placeAmount * Random.Range(60f, 120f));
                break;

            case TaskType.UpgradeBikes:
                int upgradeAmount = Mathf.RoundToInt(Random.Range(upgradeBikesRange.x, upgradeBikesRange.y) * difficultyScale);
                newTask.RequiredUpgrades = upgradeAmount;
                newTask.TaskTitle = $"Upgrade {upgradeAmount} Bikes";
                newTask.TaskDescription = $"Improve {upgradeAmount} bikes with upgrades";
                newTask.CashReward = Mathf.RoundToInt(upgradeAmount * Random.Range(150f, 300f));
                break;

            case TaskType.ReachCash:
                int cashGoal = Mathf.RoundToInt(Random.Range(reachCashRange.x, reachCashRange.y) * difficultyScale);
                // Round to nearest 1000
                cashGoal = cashGoal / 1000 * 1000;
                newTask.RequiredCash = cashGoal;
                newTask.TaskTitle = $"Reach ${cashGoal:N0}";
                newTask.TaskDescription = $"Accumulate ${cashGoal:N0} in cash";
                newTask.CashReward = Mathf.RoundToInt(cashGoal * Random.Range(0.1f, 0.2f));
                break;

            case TaskType.SignBrandDeal:
                newTask.RequiredBrandDeals = 1;
                newTask.TaskTitle = "Sign a Brand Deal";
                newTask.TaskDescription = "Partner with a new brand";
                newTask.CashReward = Random.Range(baseRewardMin, baseRewardMax);
                break;
        }

        // Default reward if not set
        if (newTask.CashReward == 0) newTask.CashReward = Random.Range(baseRewardMin, baseRewardMax);

        _tasksGenerated++;

        Debug.Log($"Generated dynamic task: {newTask.TaskTitle} (Difficulty: {difficultyScale:F2}x)");

        return newTask;
    }

    [Button("Reset Generation Counter", ButtonSizes.Large)]
    private void ResetCounter() => _tasksGenerated = 0;
}