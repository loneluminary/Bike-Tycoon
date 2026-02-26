using System;
using System.Collections;
using System.Linq;
using DG.Tweening;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.EventSystems;
using Utilities.Extensions;
using Random = UnityEngine.Random;

public class CustomerAI : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private CustomerTier tier;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float browsingTime = 2f;

    [Header("Test Drive Settings")]
    [SerializeField] private float testDriveSpeed = 5f;
    [SerializeField] private float testDriveChance = 0.3f; // 30% chance to want a test drive

    private DisplayStation _targetStation;
    private Vector3 _currentQueueTarget;

    public event Action<CustomerAI> OnCustomerLeft;

    private bool _isMoving;
    private Animation _animation;

    public void Initialize(CustomerTier customerTier, Animation visual)
    {
        _animation = Instantiate(visual, transform, false);

        tier = customerTier;
        // Start the behavior immediately
        StartCoroutine(CustomerBehavior());
    }

    /// Main State Machine Coroutine
    private IEnumerator CustomerBehavior()
    {
        // 1. Spawn to Register Queue
        yield return StartCoroutine(MoveToRegisterQueue());

        // 2. Wait in line until front, then wait random interval
        yield return StartCoroutine(WaitInLine());

        // 3. Move to Entrance
        yield return StartCoroutine(MoveToEntrance());

        // 4. Browse Bikes
        yield return StartCoroutine(BrowseBikes());

        // 5. Possibly do a Test Drive
        yield return StartCoroutine(TryTestDrive());

        // 5. Exit Gallery
        yield return StartCoroutine(ExitGallery());

        // Cleanup
        OnCustomerLeft?.Invoke(this);
        Destroy(gameObject);
    }

    #region Movement Logic

    private IEnumerator MoveTo(Vector3 targetPosition, float? overrideSpeed = null)
    {
        _isMoving = true;
        _animation.Play("walk");

        transform.DOLookAt(targetPosition.WithY(transform.position.y), 0.3f);
        yield return transform.DOMove(targetPosition.WithY(transform.position.y), overrideSpeed ?? moveSpeed).SetSpeedBased().SetEase(Ease.Linear).WaitForCompletion();

        _isMoving = false;
        _animation.Play("idle");
    }

    /// Sets the target for the queue logic called by Manager
    public void SetQueueTarget(Vector3 position)
    {
        _currentQueueTarget = position;
    }

    private IEnumerator MoveToRegisterQueue()
    {
        // Join the manager's queue system
        CustomerManager.Instance.EnqueueCustomer(this);

        // Move to the initially assigned spot
        // We move to _currentQueueTarget which was set by EnqueueCustomer
        yield return StartCoroutine(MoveTo(_currentQueueTarget));
    }

    private IEnumerator WaitInLine()
    {
        // Keep waiting until we are at the front of the list
        while (!CustomerManager.Instance.IsCustomerAtFrontOfQueue(this))
        {
            // Update position in case the line moved forward while we were waiting back here
            if (Vector3.Distance(transform.position, _currentQueueTarget) > 0.1f) yield return StartCoroutine(MoveTo(_currentQueueTarget));

            yield return new WaitForSeconds(0.5f);
        }

        if (Vector3.Distance(transform.position, _currentQueueTarget) > 0.1f) yield return StartCoroutine(MoveTo(_currentQueueTarget));

        // Now we are at the front! Wait a random interval (simulating service time)
        float randomWait = Random.Range(2f, 5f);
        yield return new WaitForSeconds(randomWait);
    }

    private IEnumerator MoveToEntrance()
    {
        // Remove ourselves from the queue list so others can move up
        CustomerManager.Instance.DequeueCustomer(this);

        // Move to Entrance Point
        Vector3 target = CustomerManager.Instance.EntrancePoint ? CustomerManager.Instance.EntrancePoint.position : Vector3.zero;

        yield return StartCoroutine(MoveTo(target));
    }

    #endregion

    #region Browse & Exit Logic

    private IEnumerator BrowseBikes()
    {
        // Find occupied stations
        var occupiedStations = FindObjectsByType<DisplayStation>(FindObjectsSortMode.None).Where(s => s.CurrentBike).ToList();
        if (occupiedStations.Count > 0)
        {
            int stationsToVisit = Random.Range(1, Mathf.Min(3, occupiedStations.Count + 1));

            for (int i = 0; i < stationsToVisit; i++)
            {
                DisplayStation station = occupiedStations[Random.Range(0, occupiedStations.Count)];
                yield return StartCoroutine(VisitStation(station));
                occupiedStations.Remove(station);
            }
        }
    }

    private IEnumerator VisitStation(DisplayStation station)
    {
        _targetStation = station;

        // Move to the station's entrance/room
        if (station.ParentRoom)
        {
            Vector3 roomPos = station.ParentRoom.transform.position;
            yield return StartCoroutine(MoveTo(roomPos));

            // Move to random standing position inside room
            Vector3 standPos = station.ParentRoom.GetRandomPositionInsideArea();
            yield return StartCoroutine(MoveTo(standPos));
        }
        else
        {
            // Fallback if room not defined
            yield return StartCoroutine(MoveTo(station.transform.position));
        }

        yield return transform.DOLookAt(station.transform.position.WithY(transform.position.y), 0.3f).WaitForCompletion();

        // Show Interest
        string[] dialogs =
        {
            "Hmm, interesting...",
            "Kinda like this one",
            "Not bad!",
            "Ooh, nice!",
            "I could see this in my Garage",
            "What a beauty!",
            "Meh, seen better",
            "Now THAT'S art!",
            "Pretty cool design",
            "Love the colors!"
        };
        ChatBubble.Create(dialogs.GetRandom(), $"E{Random.Range(1, 75)}", transform.position + Vector3.up * 3f, transform, lifeTime: browsingTime);

        // Browse
        yield return new WaitForSeconds(browsingTime);

        // Try to purchase the bike
        if (station.CurrentBike)
        {
            // Base sale chance
            float saleChance = station.BaseSaleChance * station.ParentRoom.CurrentLevel;

            // Apply staff bonus
            float staffBonus = StaffManager.Instance.GetStationSaleSpeedBonus(station);
            saleChance *= 1 + staffBonus;

            // Apply customer tier multiplier
            saleChance *= GetPurchaseProbabilityMultiplier();

            if (Random.Range(0f, 1f) <= saleChance)
            {
                station.SellCurrentBike();
            }
        }
    }

    private IEnumerator ExitGallery()
    {
        Vector3 exitPos = CustomerManager.Instance.ExitPoint ? CustomerManager.Instance.ExitPoint.position : transform.position + transform.forward * 10f;

        yield return StartCoroutine(MoveTo(exitPos));
    }

    #endregion

    #region Test Drive Logic

    private IEnumerator TryTestDrive()
    {
        // Check if the customer wants a test drive
        if (Random.value > testDriveChance) yield break;

        // Find an available test drive room
        var testDriveRoom = GalleryManager.Instance.TestDriveRoom;
        if (testDriveRoom == null || !testDriveRoom.CanTestDrive()) yield break;

        // Pick a random bike from available test bikes.
        var testDriveBike = testDriveRoom.GetRandomTestBike();
        if (testDriveBike == null) yield break;

        // Pick a random route
        var route = testDriveRoom.TestRoutes.GetRandom();
        if (route == null) yield break;

        // Move to the test drive room
        yield return StartCoroutine(MoveTo(RuntimeUtilities.GetRandomPositionInRadius(testDriveRoom.transform.position, 2f)));

        // Register as an active test driver
        testDriveRoom.ActiveTestDrivers.Add(this);

        // Show getting on bike
        ChatBubble.Create("Let me take this for a spin!", $"E{Random.Range(1, 75)}", transform.position + Vector3.up * 3f, transform, lifeTime: 1.5f);
        yield return new WaitForSeconds(1.5f);

        yield return MoveTo(route.GetWaypoint(0));

        transform.position = transform.position.WithAddY(1.8f);
        var bikeVisual = Instantiate(testDriveBike.BikePrefab, transform.position.WithAddY(-1.8f), Quaternion.LookRotation(transform.forward), transform);

        // Resize the collider to fit the bike too
        var col = transform.GetOrAddComponent<BoxCollider>();
        var bounds = transform.CalculateLocalBounds();
        Vector3 orignalCenter = col.center, orignalSize = col.size;
        col.size = bounds.size; col.center = bounds.center;

        if (bikeVisual.TryGetComponentInChildren(out MMF_Player feedbacks, true))
        {
            feedbacks.gameObject.SetActive(true);
            feedbacks.PlayFeedbacks();
        }

        // Ride through waypoints skip the first one.
        for (int i = 1; i < route.GetWaypointCount(); i++)
        {
            yield return MoveTo(route.GetWaypoint(i), testDriveBike.Speed);
        }

        col.size = orignalSize; col.center = orignalCenter;
        Destroy(bikeVisual);
        transform.position = transform.position.WithY(0f);

        // Return to room
        yield return StartCoroutine(MoveTo(RuntimeUtilities.GetRandomPositionInRadius(testDriveRoom.transform.position, 2f)));

        // Unregister from the room
        testDriveRoom.ActiveTestDrivers.RemoveAt(testDriveRoom.ActiveTestDrivers.Count - 1);

        // Show reaction
        string[] reactions = { "That was awesome!", "Great ride!", "I need this bike!", "Not bad at all!" };
        ChatBubble.Create(reactions.GetRandom(), $"E{Random.Range(1, 75)}", transform.position + Vector3.up * 3f, transform, lifeTime: 2f);
        yield return new WaitForSeconds(2f);

        // Higher chance to purchase after a test driver
        float purchaseChance = 0.3f * GetPurchaseProbabilityMultiplier(); // 30% base chance, multiplied by tier bonus
        if (Random.value <= purchaseChance)
        {
            // station.SellCurrentBike();

            int salePrice = testDriveBike.GetPriceAtLevel(1);
            GameManager.Instance.AddCash(salePrice);
            UIManager.Instance.CashAddingAnimation(Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 2f));
            BrandManager.Instance.RecordSale(testDriveBike.BrandData.BrandName);

            Debug.Log($"Customer purchased {testDriveBike.BikeName} after test drive!".RichColor(Color.green));
        }
    }

    #endregion

    public float GetPurchaseProbabilityMultiplier()
    {
        return tier switch
        {
            CustomerTier.Regular => 1.0f,
            CustomerTier.Enthusiast => 1.5f,
            CustomerTier.Collector => 2.0f,
            CustomerTier.VIP => 3.0f,
            _ => 1.0f
        };
    }
}

public enum CustomerTier
{
    Regular,
    Enthusiast,
    Collector,
    VIP
}