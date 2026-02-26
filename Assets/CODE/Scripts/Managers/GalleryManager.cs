using System;
using UnityEngine;
using System.Linq;
using Sirenix.OdinInspector;
using Utilities.Extensions;
using System.Collections.Generic;

public class GalleryManager : MonoSingleton<GalleryManager>
{
    public List<GalleryRoom> AllRooms = new();

    public TestDriveRoom TestDriveRoom;
    public MarketingRoom MarketingRoom;

    [ReadOnly] public List<GalleryRoom> UnlockedRooms = new();
    [ReadOnly] public List<GalleryRoom> LockedRooms = new();

    public Action OnRoomUnlocked;

    private const string SaveKey = "Gallery_Rooms";

    private void Awake()
    {
        UnlockedRooms = new();
        LockedRooms = new();

        for (int i = 0; i < AllRooms.Count; i++)
        {
            var room = AllRooms[i];
            room.ID = i;

            if (room.IsUnlocked) UnlockedRooms.TryAdd(room);
            else LockedRooms.TryAdd(room);
        }

        if (!TestDriveRoom) TestDriveRoom = FindFirstObjectByType<TestDriveRoom>();
        if (!MarketingRoom) MarketingRoom = FindFirstObjectByType<MarketingRoom>();
    }

    public string SaveRooms()
    {
        var saveDataList = UnlockedRooms.Select(room => new RoomSaveData
        {
            ID = room.ID,
            Level = room.CurrentLevel,
            PurchasedSegmentIndices = new List<int>(room.PurchasedSegmentIndices)
        }).ToList();

        ES3.Save(SaveKey, saveDataList);

        return $"Saved {saveDataList.Count} unloaked rooms";
    }

    public string LoadRooms()
    {
        var saveDataList = ES3.Load(SaveKey, new List<RoomSaveData>());

        foreach (var saveData in saveDataList)
        {
            var room = AllRooms.FirstOrDefault(r => r.ID == saveData.ID);
            if (room == null) continue;

            // Restore room state
            room.RestoreFromSave(saveData);

            // Update manager lists
            UnlockedRooms.TryAdd(room);
            LockedRooms.TryRemove(room);
        }

        TestDriveRoom.Load();
        MarketingRoom.Load();

        return $"Loaded {saveDataList.Count} unloaked rooms";
    }
}

[Serializable]
public class RoomSaveData
{
    public int ID;
    public int Level;
    public List<int> PurchasedSegmentIndices;
}