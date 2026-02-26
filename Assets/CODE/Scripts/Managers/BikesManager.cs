using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using Sirenix.OdinInspector;
using Utilities.Extensions;

public class BikesManager : MonoSingleton<BikesManager>
{
    [AssetsOnly] public BikeInstance BikeInstancePrefab;

    public List<BikeData> AllBikesData = new();

    [ReadOnly] public List<BikeInstance> ActiveBikes = new();
    [ReadOnly] public List<InventoryItem> Inventory = new();

    // Events
    public event Action<BikeData, int> OnBikeAdded;
    public event Action<BikeData, int> OnBikeRemoved;
    public event Action OnInventoryChanged;

    public event Action<BikeInstance> OnBikeSold;
    public event Action<BikeInstance, BikeInstance> OnBikeMerged;
    public event Action<BikeInstance> OnBikeUpgraded;

    private void Awake()
    {
        for (int i = 0; i < AllBikesData.Count; i++)
        {
            var bike = AllBikesData[i];
            if (!bike)
            {
                AllBikesData.RemoveAt(i);
                continue;
            }

            bike.ID = i;
        }
    }

    public void OnBikeSoldFromInstance(BikeInstance bike)
    {
        GameManager.Instance.RecordSale();

        int inventoryCount = GetBikeCount(bike.BikeData);
        if (inventoryCount <= 0)
        {
            UIManager.Instance.ShowToastMessage($"Your stock of {bike.BikeData.DetailedName} has been sold out\nRemember to restock them from partnerships!", 6f);
            Destroy(bike.gameObject);
        }
        else // Check if we need to destroy any displayed bikes
        {
            int displayedCount = GetDisplayedBikeCount(bike.BikeData);
            int diffrene = displayedCount - inventoryCount;
            if (diffrene > 0)
            {
                var bikes = ActiveBikes.Where(b => b.BikeData == bike.BikeData).ToArray();
                for (int i = 0; i < diffrene; i++) Destroy(bikes[i].gameObject);
            }
        }

        OnBikeSold?.Invoke(bike);
    }

    public void OnBikesMerged(BikeInstance bike, BikeInstance with) => OnBikeMerged?.Invoke(bike, with);
    public void OnBikeUpgrades(BikeInstance bike) => OnBikeUpgraded?.Invoke(bike);

    /// Add bike to inventory
    public void AddBike(BikeData bike, int amount = 1)
    {
        if (!bike) return;

        // Check if bike already in inventory
        var existingItem = Inventory.FirstOrDefault(i => i.ID == bike.ID);

        if (existingItem != null)
        {
            existingItem.Units += amount;
        }
        else
        {
            Inventory.Add(new InventoryItem(bike.ID, amount));
        }

        OnBikeAdded?.Invoke(bike, amount);
        OnInventoryChanged?.Invoke();

        Debug.Log($"Added {amount}x {bike.BikeName} to inventory".RichColor(Color.green));
    }

    /// Remove bike from inventory
    public bool TryRemoveBike(BikeData bike, int amount = 1)
    {
        if (!bike) return false;

        var item = Inventory.FirstOrDefault(i => i.ID == bike.ID);
        if (item == null || item.Units < amount) return false;

        item.Units -= amount;

        // Remove from list if no units left
        if (item.Units <= 0)
        {
            Inventory.Remove(item);
        }

        OnBikeRemoved?.Invoke(bike, amount);
        OnInventoryChanged?.Invoke();

        Debug.Log($"Removed {amount}x {bike.BikeName} from inventory".RichColor(Color.yellow));
        return true;
    }

    /// Get unit count for a specific bike
    public int GetBikeCount(BikeData bike)
    {
        var item = Inventory.FirstOrDefault(i => i.ID == bike.ID);
        return item?.Units ?? 0;
    }

    /// Check if a bike is in inventory
    public bool HasBike(BikeData bike, int amount = 1)
    {
        return GetBikeCount(bike) >= amount;
    }

    /// Get the count of bikes currently displayed across all stations
    public int GetDisplayedBikeCount(BikeData bike)
    {
        int count = 0;
        foreach (var activeBike in ActiveBikes)
        {
            if (activeBike && activeBike.BikeData == bike)
            {
                count++;
            }
        }
        return count;
    }

    public string SaveBikes()
    {
        List<PlacedBikeSaveData> placedBikes = new();

        // Iterate through all unlocked rooms
        foreach (var room in GalleryManager.Instance.UnlockedRooms)
        {
            if (!room) continue;

            for (int i = 0; i < room.Stations.Count; i++)
            {
                DisplayStation station = room.Stations[i];
                if (!station || !station.CurrentBike || !station.CurrentBike.BikeData) continue;

                // Create save data for this placed bike
                PlacedBikeSaveData saveData = new(room.ID, i, station.CurrentBike.BikeData.ID, station.CurrentBike.CurrentLevel);
                placedBikes.Add(saveData);
            }
        }

        // Save to ES3
        ES3.Save("PlacedBikes", placedBikes);
        ES3.Save("BikesInventory", Inventory);

        return $"Saved {placedBikes.Count} placed bikes and {Inventory.Count} inventory bikes";
    }

    public string LoadBikes()
    {
        // Load save data
        var placedBikes = ES3.Load("PlacedBikes", new List<PlacedBikeSaveData>());
        Inventory = ES3.Load("BikesInventory", new List<InventoryItem>());

        System.Text.StringBuilder log = new();

        int loadedCount = 0;
        if (!placedBikes.IsNullOrEmpty())
        {
            foreach (PlacedBikeSaveData saveData in placedBikes)
            {
                GalleryRoom room = GalleryManager.Instance.UnlockedRooms.FirstOrDefault(r => r.ID == saveData.RoomID);
                if (!room)
                {
                    Debug.LogWarning($"Room not found of ID: {saveData.RoomID}");
                    continue;
                }

                // Validate station index
                if (saveData.StationIndex < 0 || saveData.StationIndex >= room.Stations.Count)
                {
                    Debug.LogWarning($"Invalid station index: {saveData.StationIndex} in room {room.RoomName}");
                    continue;
                }

                DisplayStation station = room.Stations[saveData.StationIndex];
                if (!station)
                {
                    Debug.LogWarning($"Station not found at index: {saveData.StationIndex}");
                    continue;
                }

                // Find bike data by ID
                BikeData bikeData = AllBikesData.FirstOrDefault(b => b && b.ID == saveData.BikeID);
                if (!bikeData)
                {
                    Debug.LogWarning($"BikeData not found for ID: {saveData.BikeID}");
                    continue;
                }

                // Spawn bike on station
                BikeInstance bike = station.SpawnBike(bikeData, saveData.BikeLevel);

                if (bike) loadedCount++;
            }
        }

        log.Append($"Loaded {loadedCount}/{placedBikes.Count} placed bikes and {Inventory.Count} inventory bikes");

        return log.ToString();
    }

    public BikeData GetBikeByID(int id)
    {
        return AllBikesData.FirstOrDefault(b => b && b.ID == id);
    }

    /// Get all bikes in inventory
    public List<InventoryItem> GetAllBikes()
    {
        return new List<InventoryItem>(Inventory);
    }
}

[Serializable]
public class InventoryItem
{
    public int ID;
    public int Units;

    public InventoryItem(int id, int units = 1)
    {
        ID = id;
        Units = units;
    }
}