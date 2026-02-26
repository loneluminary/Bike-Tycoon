using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Sirenix.OdinInspector;
using TouchCameraSystem;
using UnityEngine;
using Utilities.Extensions;
using Random = UnityEngine.Random;

public class RacingManager : MonoSingleton<RacingManager>
{
    [Title("Race Settings")]
    [SerializeField] Transform racerSpawnParent;
    [SerializeField] DriveRoute raceRoute;

    [SerializeField] Vector2 trackBoundaryMin;
    [SerializeField] Vector2 trackBoundaryMax;

    [Title("Current Race")]
    [ReadOnly] public RaceEvent CurrentRace;
    [ReadOnly] public bool IsRaceActive;

    [Title("Brand Discounts")]
    public Dictionary<string, float> BrandDiscounts = new(); // BrandName -> Discount (0.5 = 50% off)

    private DateTime _lastRaceDate;

    /// camera orignal values befoure racing
    private float _orignalCameraZoom, _orignalCameraZoomMax;
    private Vector4 _orignalCameraBounds;
    private Vector3 _orignalCameraPosition;
    private Quaternion _orignalCameraRotation;

    // Events
    public event Action<RaceEvent> OnRaceStarted;
    public event Action<RaceEvent> OnRaceUpdate;
    public event Action<RaceResults> OnRaceFinished;
    public event Action<bool> OnRaceAvailabilityChanged;

    private void Start()
    {
        _lastRaceDate = ES3.Load("LastRaceDate", DateTime.Now.AddDays(-7)); // Allow race immediately on first play
    }

    public bool IsRaceAvailable()
    {
        TimeSpan timeSinceLastRace = DateTime.Now - _lastRaceDate;
        return timeSinceLastRace.TotalDays >= 7 && !IsRaceActive;
    }

    public TimeSpan GetTimeUntilNextRace()
    {
        TimeSpan timeSinceLastRace = DateTime.Now - _lastRaceDate;
        TimeSpan timeRemaining = TimeSpan.FromDays(7) - timeSinceLastRace;
        return timeRemaining.TotalSeconds > 0 ? timeRemaining : TimeSpan.Zero;
    }

    public float GetBrandDiscount(string brandName)
    {
        return BrandDiscounts.TryGetValue(brandName, out float discount) ? discount : 0f;
    }

    public void ClearBrandDiscount(string brandName)
    {
        if (BrandDiscounts.ContainsKey(brandName)) BrandDiscounts.Remove(brandName);
    }

    public bool CanEnterRace(BikeInstance bike)
    {
        if (bike == null) return false;
        if (!IsRaceAvailable()) return false;
        return true;
    }

    public void StartRace(BikeInstance playerBike, RaceType raceType)
    {
        if (!CanEnterRace(playerBike)) return;

        IsRaceActive = true;

        ClearRacers();

        // Generate NPCs and add player
        var allParticipants = GenerateNPCRacers(3);
        allParticipants.Add(new() { Name = "You", IsPlayer = true });

        // Shuffle to random starting positions
        allParticipants = allParticipants.OrderBy(x => Random.Range(0f, 1f)).ToList();

        // Calculate effective speed for each participant based on race type
        foreach (var participant in allParticipants)
        {
            if (participant.IsPlayer)
            {
                float statMultiplier = playerBike.BikeData.GetStatMultiplier(playerBike.CurrentLevel);
                participant.EffectiveSpeed = CalculateEffectiveSpeed
                (
                    playerBike.BikeData.Speed * statMultiplier,
                    playerBike.BikeData.Handling * statMultiplier,
                    playerBike.BikeData.Acceleration * statMultiplier,
                    playerBike.BikeData.Durability * statMultiplier,
                    raceType
                );
            }
            else
            {
                participant.EffectiveSpeed = CalculateEffectiveSpeed
                (
                    participant.Bike.Speed,
                    participant.Bike.Handling,
                    participant.Bike.Acceleration,
                    participant.Bike.Durability,
                    raceType
                );
            }
        }

        // Create a race event
        CurrentRace = new RaceEvent { RaceType = raceType, RaceRoute = raceRoute, PlayerBike = playerBike, Participants = allParticipants };

        SpawnRacers(CurrentRace);

        var camera = GameManager.Instance.TouchCamera;

        // save camera values
        _orignalCameraZoom = camera.CamZoom;
        _orignalCameraZoomMax = camera.CamZoomMax;
        _orignalCameraBounds = new(camera.BoundaryMin.x, camera.BoundaryMin.y, camera.BoundaryMax.x, camera.BoundaryMax.y);
        _orignalCameraPosition = camera.transform.position;
        _orignalCameraRotation = camera.transform.rotation;

        camera.BoundaryMin = new(trackBoundaryMin.x, trackBoundaryMin.y);
        camera.BoundaryMax = new(trackBoundaryMax.x, trackBoundaryMax.y);
        camera.CamZoomMax = 120f;

        Vector3 boundaryCenter = new((trackBoundaryMin.x + trackBoundaryMax.x) / 2f, 0f, (trackBoundaryMin.y + trackBoundaryMax.y) / 2f);
        camera.GetComponent<FocusCameraOnItem>().FocusCameraOnTarget(boundaryCenter, Quaternion.Euler(90f, -90f, 0f), camera.CamZoomMax);
    }

    public void RaceStarted()
    {
        if (!IsRaceActive) return;

        for (int i = 0; i < CurrentRace.Participants.Count; i++)
        {
            CurrentRace.Participants[i].Racer.ToggleFeedbacks(true);
        }

        OnRaceStarted?.Invoke(CurrentRace);

        // Simulate race
        StartCoroutine(SimulateRace());
    }

    private IEnumerator SimulateRace()
    {
        // Only check the person in the very last slot
        // Since the list is sorted, if they are done, everyone is done.
        while (CurrentRace.Participants[^1].Progress < CurrentRace.RaceRoute.RouteLength)
        {
            UpdateRacersPosition(CurrentRace);
            yield return null;
        }

        // Get final results
        var playerParticipant = CurrentRace.Participants.First(p => p.IsPlayer);
        RaceResults results = new() { PlayerPosition = playerParticipant.Position, RaceType = CurrentRace.RaceType };

        IsRaceActive = false;
        _lastRaceDate = DateTime.Now;
        ES3.Save("LastRaceDate", _lastRaceDate);

        ClearRacers();

        // restore camera values
        var camera = GameManager.Instance.TouchCamera;
        camera.CamZoomMax = _orignalCameraZoomMax;
        camera.BoundaryMin = new(_orignalCameraBounds.x, _orignalCameraBounds.y);
        camera.BoundaryMax = new(_orignalCameraBounds.z, _orignalCameraBounds.w);
        DOTween.To(() => camera.CamZoom, (x) => camera.CamZoom = x, _orignalCameraZoom, 1f);
        camera.transform.DOMove(_orignalCameraPosition, 1f);
        camera.transform.DORotateQuaternion(_orignalCameraRotation, 1f).OnComplete(() => camera.ResetCameraBoundaries());

        OnRaceFinished?.Invoke(results);
        OnRaceAvailabilityChanged?.Invoke(IsRaceAvailable());
    }

    private void UpdateRacersPosition(RaceEvent raceEvent)
    {
        // Update progress
        for (int i = 0; i < raceEvent.Participants.Count; i++)
        {
            var p = raceEvent.Participants[i];
            float randomFactor = Random.Range(0.85f, 1.15f);
            p.Progress += p.EffectiveSpeed * randomFactor / 1.5f * Time.deltaTime;
        }

        raceEvent.Participants.Sort((a, b) => b.Progress.CompareTo(a.Progress));

        for (int i = 0; i < raceEvent.Participants.Count; i++)
        {
            var p = raceEvent.Participants[i];

            p.Position = i + 1;
            p.Racer.UpdatePosition();
        }

        OnRaceUpdate?.Invoke(raceEvent);
    }

    public void SpawnRacers(RaceEvent raceEvent)
    {
        if (!raceRoute)
        {
            Debug.LogWarning("No race route assigned to RaceManager!");
            return;
        }

        // Grid configuration
        int racersPerRow = 2;
        float horizontalSpacing = 3.5f;
        float verticalSpacing = 5.0f;

        // Calculate orientation at the start line
        Vector3 startPos = raceRoute.GetWaypoint(0);
        Vector3 nextPoint = raceRoute.GetWaypoint(1);
        Vector3 forward = (nextPoint - startPos).normalized;
        Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;

        for (int i = 0; i < raceEvent.Participants.Count; i++)
        {
            var participant = raceEvent.Participants[i];

            // Create and Initialize
            var racerVisual = new GameObject(participant.Name).AddComponent<RacerVisual>();
            racerVisual.transform.SetParent(racerSpawnParent);

            BikeData bikeData = participant.IsPlayer ? raceEvent.PlayerBike.BikeData : participant.Bike;
            racerVisual.Initialize(participant, raceRoute, bikeData);
            racerVisual.AddComponent<OnClickToggleCameraFollow>();

            // Physics/Collider Setup
            var col = racerVisual.GetOrAddComponent<BoxCollider>();
            var bounds = racerVisual.transform.CalculateLocalBounds();
            col.size = bounds.size;
            col.center = bounds.center;
            col.isTrigger = true;

            // Grid Positioning Logic
            int row = i / racersPerRow;
            int column = i % racersPerRow;

            // Offset math: 
            // Horizontal: Centers the racers (-1.75 and +1.75 for 2 columns)
            // Vertical: Moves them back row by row
            float hOffset = (column - (racersPerRow - 1) * 0.5f) * horizontalSpacing;
            float vOffset = -row * verticalSpacing;

            Vector3 gridOffset = (right * hOffset) + (forward * vOffset);

            racerVisual.transform.SetPositionAndRotation(startPos + gridOffset, Quaternion.LookRotation(forward));

            participant.Racer = racerVisual;
        }
    }

    public void ClearRacers()
    {
        GameManager.Instance.TouchCamera.GetComponent<FocusCameraOnItem>().StopFollowing();

        if (CurrentRace == null || CurrentRace.Participants.IsNullOrEmpty()) return;

        for (int i = 0; i < CurrentRace.Participants.Count; i++)
        {
            var racer = CurrentRace.Participants[i].Racer;
            if (racer) Destroy(racer.gameObject);
        }
    }

    /// Calculates effective speed based on bike stats and race type.
    private float CalculateEffectiveSpeed(float speed, float handling, float acceleration, float durability, RaceType raceType)
    {
        float effectiveSpeed = 0f;

        switch (raceType)
        {
            case RaceType.Sprint:
                effectiveSpeed = speed * 0.7f + acceleration * 0.3f;
                break;
            case RaceType.Circuit:
                effectiveSpeed = handling * 0.6f + speed * 0.4f;
                break;
            case RaceType.Endurance:
                effectiveSpeed = durability * 0.5f + speed * 0.3f + handling * 0.2f;
                break;
            case RaceType.Drag:
                effectiveSpeed = acceleration * 0.8f + speed * 0.2f;
                break;
        }


        return effectiveSpeed;
    }

    private List<RaceParticipant> GenerateNPCRacers(int count)
    {
        var npcs = new List<RaceParticipant>();

        for (int i = 0; i < count; i++)
        {
            RaceParticipant npc = new()
            {
                Name = RuntimeUtilities.GetRandomName(),
                Bike = BikesManager.Instance.AllBikesData.GetRandom()
            };

            npcs.Add(npc);
        }

        return npcs;
    }

    public void AwardPrizes(RaceResults results)
    {
        switch (results.PlayerPosition)
        {
            case 1:
                // 50% discount on next brand deal
                // Store discount for ALL brands
                foreach (var brand in BrandManager.Instance.GetAvailableBrands())
                {
                    BrandDiscounts[brand.BrandName] = 0.5f;
                }
                Debug.Log("1st Place! Next brand deal 50% off!".RichColor(Color.gold));
                break;

            case 2:
                // Cash prize
                int cashPrize2nd = 4000;
                GameManager.Instance.AddCash(cashPrize2nd);
                UIManager.Instance.CashAddingAnimation(new(Screen.width / 2f, Screen.height / 2f));
                Debug.Log($"2nd Place! Earned ${cashPrize2nd}".RichColor(Color.cyan));
                break;

            case 3:
                // Cash prize
                int cashPrize3rd = 2000;
                GameManager.Instance.AddCash(cashPrize3rd);
                UIManager.Instance.CashAddingAnimation(new(Screen.width / 2f, Screen.height / 2f));
                Debug.Log($"3rd Place! Earned ${cashPrize3rd}".RichColor(Color.cyan));
                break;

            default:
                // Small consolation prize
                GameManager.Instance.AddCash(500);
                UIManager.Instance.CashAddingAnimation(new(Screen.width / 2f, Screen.height / 2f));
                Debug.Log($"{results.PlayerPosition} Place. Better luck next time!");
                break;
        }
    }

    private void OnDrawGizmos()
    {
        // Draw starting positions
        if (raceRoute && raceRoute.GetWaypointCount() > 0)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(raceRoute.GetWaypoint(0), 2f);
        }
    }

    #region Helper Methods

    public static string GetPositionText(int position)
    {
        return position switch
        {
            1 => "Position: 1ST!",
            2 => "Position: 2ND!",
            3 => "Position: 3RD",
            _ => $"Position: #{position}"
        };
    }

    public static string GetRewardText(int position)
    {
        return position switch
        {
            1 => "Reward: 50% OFF Brand Deal!",
            2 => "Reward: +$4,000",
            3 => "Reward: +$2,000",
            _ => "Reward: +$500"
        };
    }

    public static string GetRaceDescription(RaceType raceType)
    {
        return raceType switch
        {
            RaceType.Sprint => "Short track\nSpeed matters most",
            RaceType.Circuit => "Technical track\nHandling matters",
            RaceType.Endurance => "Long race\nDurability matters",
            RaceType.Drag => "Pure acceleration",
            _ => ""
        };
    }

    #endregion
}

#region Custom Classes

[Serializable]
public class RaceEvent
{
    public RaceType RaceType;
    public DriveRoute RaceRoute;
    public BikeInstance PlayerBike;
    public List<RaceParticipant> Participants;
}

[Serializable]
public class RaceParticipant
{
    public string Name;
    public BikeData Bike;
    public RacerVisual Racer;
    public float EffectiveSpeed; // Calculated once at race start based on stats and race type
    public bool IsPlayer;
    public int Position;
    public float Progress;
}

[Serializable]
public class RaceResults
{
    public int PlayerPosition;
    public RaceType RaceType;
}

public enum RaceType
{
    Sprint,
    Circuit,
    Endurance,
    Drag
}

#endregion