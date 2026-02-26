using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities.Extensions;

public class StaffManager : MonoSingleton<StaffManager>
{
    public int MaxStaffPerRoom = 3;

    [Title("Staff Database")]
    public List<StaffData> AvailableStaffTypes = new();

    [Title("Prefabs")]
    public StaffMember StaffMemberPrefab;

    [Title("Runtime")]
    public List<StaffMember> HiredStaff = new();

    public event System.Action<StaffMember> OnStaffHired;

    public bool TryHireStaff(StaffData staffData, bool ignoreCost = false)
    {
        if (!ignoreCost && !GameManager.Instance.TrySpendCash(staffData.HireCost)) return false;

        var staff = Instantiate(StaffMemberPrefab, Vector3.zero, Quaternion.identity);
        if (staff)
        {
            staff.Initialize(staffData);
            HiredStaff.Add(staff);
            OnStaffHired?.Invoke(staff);

            if (!ignoreCost) Debug.Log($"Hired {staffData.StaffName.RichColor(Color.green)} for ${staffData.HireCost.ToString().RichColor(Color.red)}");
            return true;
        }

        return false;
    }

    public void FireStaff(StaffMember staff)
    {
        HiredStaff.Remove(staff);
        Destroy(staff.gameObject);
    }

    // Get all staff of a specific type
    public List<StaffMember> GetStaffByType(StaffType type)
    {
        return HiredStaff.Where(s => s.StaffData.StaffType == type).ToList();
    }

    // Get staff assigned to a room
    public List<StaffMember> GetStaffInRoom(GalleryRoom room)
    {
        return HiredStaff.Where(s => s.AssignedRoom == room).ToList();
    }

    // Get staff assigned to a station
    public StaffMember GetStaffAtStation(DisplayStation station)
    {
        return HiredStaff.FirstOrDefault(s => s.AssignedStation == station);
    }

    // Calculate total bonuses for a room
    public float GetRoomSaleSpeedBonus(GalleryRoom room)
    {
        return GetStaffInRoom(room).Sum(staff => staff.GetSaleSpeedBonus());
    }

    public float GetRoomCustomerBonus(GalleryRoom room)
    {
        return GetStaffInRoom(room).Sum(staff => staff.GetCustomerAttractionBonus());
    }

    public float GetGalleryProfitBonus()
    {
        return GetStaffByType(StaffType.Manager).Sum(staff => staff.GetProfitBonus());
    }

    // Calculate station bonus (if staff assigned directly to the station)
    public float GetStationSaleSpeedBonus(DisplayStation station)
    {
        StaffMember staff = GetStaffAtStation(station);
        return staff ? staff.GetSaleSpeedBonus() : 0f;
    }

    public string SaveStaff()
    {
        List<StaffMember.StaffSaveData> staffList = new();

        foreach (var staff in HiredStaff)
        {
            if (!staff || !staff.StaffData) continue;

            int roomID = staff.AssignedRoom ? staff.AssignedRoom.ID : -1;
            bool isAssignedToStation = staff.AssignedStation != null;
            int stationIndex = -1;

            // Find station index if assigned to station
            if (isAssignedToStation && staff.AssignedRoom)
            {
                stationIndex = staff.AssignedRoom.Stations.IndexOf(staff.AssignedStation);
            }

            StaffMember.StaffSaveData saveData = new(staff.StaffData.StaffName, (int)staff.StaffData.StaffType, staff.CurrentLevel, roomID, isAssignedToStation, stationIndex);

            staffList.Add(saveData);
        }

        ES3.Save("HiredStaff", staffList);

        return $"Saved {staffList.Count} staff members";
    }

    public string LoadStaff()
    {
        var staffList = ES3.Load("HiredStaff", new List<StaffMember.StaffSaveData>());

        int loadedCount = 0;

        if (!staffList.IsNullOrEmpty())
        {
            var unlockedRooms = GalleryManager.Instance.UnlockedRooms;

            foreach (var saveData in staffList)
            {
                // Find the staff data by name
                StaffData staffData = AvailableStaffTypes.FirstOrDefault(s => s.StaffName == saveData.StaffName);
                if (!staffData)
                {
                    Debug.LogWarning($"Staff data not found for: {saveData.StaffName}");
                    continue;
                }

                // Try to hire the staff
                if (!TryHireStaff(staffData, true)) continue;

                StaffMember hiredStaff = HiredStaff[^1];
                hiredStaff.CurrentLevel = saveData.CurrentLevel;

                // Assign to room or station
                if (saveData.RoomID >= 0)
                {
                    GalleryRoom room = unlockedRooms.FirstOrDefault(r => r.ID == saveData.RoomID);
                    if (!room)
                    {
                        Debug.LogWarning($"Room not found with ID: {saveData.RoomID}");
                        continue;
                    }

                    if (saveData.IsAssignedToStation && saveData.StationIndex >= 0 && saveData.StationIndex < room.Stations.Count)
                    {
                        // Assign to specific station
                        DisplayStation station = room.Stations[saveData.StationIndex];
                        if (station) hiredStaff.AssignToStation(station);
                    }
                    else
                    {
                        // Assign to room
                        hiredStaff.AssignToRoom(room);
                    }
                }

                loadedCount++;
            }
        }

        return $"Loaded {loadedCount}/{staffList.Count} staff members";
    }
}