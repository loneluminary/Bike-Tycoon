using System.Collections;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Animations;
using Utilities.Extensions;

[RequireComponent(typeof(AudioSource))]
public class StaffMember : MonoBehaviour
{
    [Title("Staff Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float wanderWaitTime = 2f;

    [Title("Staff Info")]
    [SerializeField] TMP_Text nameText;
    [ReadOnly] public StaffData StaffData;
    [ReadOnly] public int CurrentLevel = 1;

    [Title("Assignment")]
    [ReadOnly] public GalleryRoom AssignedRoom;
    [ReadOnly] public DisplayStation AssignedStation;

    [Title("Sounds")]
    [SerializeField] private AudioClip upgradeSound;
    [SerializeField] private AudioSource audioSource;

    private bool _isMoving, _isWandering;
    private Animation _animation;
    private Coroutine _wanderCoroutine;

    public void Initialize(StaffData data)
    {
        _animation = Instantiate(data.StaffModel, transform, false);

        if (nameText) nameText.text = $"{data.StaffName} ({data.StaffType})";

        if (transform.TryGetComponentInChildren(out LookAtConstraint lookAt)) lookAt.AddSource(new ConstraintSource() { sourceTransform = Camera.main.transform, weight = 1f, });

        StaffData = data;
        CurrentLevel = 1;
        UpdateVisuals();

        audioSource = GetComponent<AudioSource>();
    }

    private IEnumerator MoveTo(Vector3 targetPosition, float? overrideSpeed = null)
    {
        // Stop any existing wander coroutine
        if (_wanderCoroutine != null) StopCoroutine(_wanderCoroutine);

        _isMoving = true;
        _animation.Play("walk");

        transform.DOLookAt(targetPosition.WithY(transform.position.y), 0.3f);
        yield return transform.DOMove(targetPosition.WithY(transform.position.y), overrideSpeed ?? moveSpeed).SetSpeedBased().SetEase(Ease.Linear).WaitForCompletion();

        _isMoving = false;
        _animation.Play("idle");
    }

    private IEnumerator WanderAround(GalleryRoom room)
    {
        _isWandering = true;

        while (_isWandering && room != null)
        {
            // Wait before moving to next location
            yield return new WaitForSeconds(wanderWaitTime);

            // Get random position in room
            Vector3 randomPosition = room.GetRandomPositionInsideArea();

            // Move to that position
            yield return StartCoroutine(MoveTo(randomPosition));
        }
    }

    public void AssignToRoom(GalleryRoom room)
    {
        AssignedRoom = room;
        AssignedStation = null;

        // Move staff to a room position
        if (room)
        {
            StartCoroutine(MoveTo(room.GetRandomPositionInsideArea()));
            // Start wandering after reaching initial position
            this.DelayedExecutionUntil(() => !_isMoving, () => _wanderCoroutine = StartCoroutine(WanderAround(room)));
        }

        Debug.Log($"{StaffData.StaffName} assigned to {room.RoomName}");
    }

    public void AssignToStation(DisplayStation station)
    {
        AssignedStation = station;

        // Move staff near the station
        if (station) StartCoroutine(MoveTo(RuntimeUtilities.GetRandomPositionInRadius(station.transform.position, 8f)));

        Debug.Log($"{StaffData.StaffName} assigned to a station in {station.ParentRoom.RoomName}");
    }

    public bool CanUpgrade()
    {
        return CurrentLevel < StaffData.MaxLevel;
    }

    public int GetUpgradeCost()
    {
        return StaffData.UpgradeCostPerLevel * CurrentLevel;
    }

    public bool TryUpgrade()
    {
        if (!CanUpgrade()) return false;

        int cost = GetUpgradeCost();
        if (!GameManager.Instance.TrySpendCash(cost)) return false;

        CurrentLevel++;
        UpdateVisuals();
        audioSource.PlayOneShot(upgradeSound);

        Debug.Log($"{StaffData.StaffName} upgraded to Level {CurrentLevel}!");
        return true;
    }

    // Calculate bonuses based on level
    public float GetSaleSpeedBonus()
    {
        if (StaffData.StaffType != StaffType.Salesperson) return 0f;
        return StaffData.SaleSpeedBonus * CurrentLevel;
    }

    public float GetCustomerAttractionBonus()
    {
        if (StaffData.StaffType != StaffType.Marketer) return 0f;
        return StaffData.CustomerAttractionBonus * CurrentLevel;
    }

    public float GetProfitBonus()
    {
        if (StaffData.StaffType != StaffType.Manager) return 0f;
        return StaffData.ProfitBonus * CurrentLevel;
    }

    public float GetMergeSpeedBonus()
    {
        if (StaffData.StaffType != StaffType.Mechanic) return 0f;
        return StaffData.MergeSpeedBonus * CurrentLevel;
    }

    private void UpdateVisuals()
    {
        transform.DOPunchScale(transform.lossyScale * 0.2f, 0.2f);
    }

    private void OnDestroy()
    {
        _isWandering = false;
        if (_wanderCoroutine != null) StopCoroutine(_wanderCoroutine);
    }

    private void OnDrawGizmos()
    {
        // Draw connection to the assigned station /room
        Gizmos.color = Color.green;
        if (AssignedStation)
        {
            Gizmos.DrawLine(transform.position, AssignedStation.transform.position);
        }
        else if (AssignedRoom)
        {
            Gizmos.DrawLine(transform.position, AssignedRoom.transform.position);
        }
    }

    [System.Serializable]
    public class StaffSaveData
    {
        public string StaffName;
        public int StaffType;
        public int CurrentLevel;
        public int RoomID;
        public bool IsAssignedToStation;
        public int StationIndex; // Only used if assigned to station

        public StaffSaveData(string staffName, int staffType, int currentLevel, int roomID, bool isAssignedToStation = false, int stationIndex = -1)
        {
            StaffName = staffName;
            StaffType = staffType;
            CurrentLevel = currentLevel;
            RoomID = roomID;
            IsAssignedToStation = isAssignedToStation;
            StationIndex = stationIndex;
        }
    }
}