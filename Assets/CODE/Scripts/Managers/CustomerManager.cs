using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities.Extensions;

public class CustomerManager : MonoSingleton<CustomerManager>
{
    [Title("Customer Settings")]
    [SerializeField] private CustomerAI customerPrefab;
    public Animation[] CustomersVisuals;
    [SerializeField] private Transform customersContainer;
    [SerializeField] private int baseMaxActiveCustomers = 5;

    [Title("Points")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private Transform registerPoint; // The front of the line
    public Transform EntrancePoint; // Where they go after registering
    public Transform ExitPoint;

    [Title("Queue Settings")]
    [SerializeField] private float queueSpacing = 1.5f; // Distance between customers in line

    private readonly List<CustomerAI> _activeCustomers = new();
    private readonly List<CustomerAI> _registerQueue = new();

    private float _lastSpawnTime;

    private void Update()
    {
        if (Time.time - _lastSpawnTime >= GetCustomerSpawnInterval())
        {
            TrySpawnCustomer();
            _lastSpawnTime = Time.time;
        }
    }

    private float GetCustomerSpawnInterval()
    {
        float baseInterval = 5f;

        // Apply Marketing Room bonus
        var marketingRoom = GalleryManager.Instance.MarketingRoom;
        if (marketingRoom && marketingRoom.IsUnlocked)
        {
            float marketingBonus = marketingRoom.GetTotalCustomerSpawnBonus();
            baseInterval /= 1 + marketingBonus; // Higher bonus = faster spawns
        }

        // Apply Entrance bonus (now consistent with Marketing)
        if (GameManager.Instance.Entrance)
        {
            float entranceBonus = GameManager.Instance.Entrance.CustomerSpawnRateBonus;
            baseInterval /= 1 + entranceBonus; // +20% bonus → 1.2x faster
        }

        return baseInterval;
    }

    private int GetMaxActiveCustomers()
    {
        int maxCustomers = baseMaxActiveCustomers;

        // Apply Entrance max customer bonus
        if (GameManager.Instance.Entrance) maxCustomers = GameManager.Instance.Entrance.MaxSimultaneousCustomers;

        return maxCustomers;
    }

    private void TrySpawnCustomer()
    {
        if (_activeCustomers.Count >= GetMaxActiveCustomers()) return;
        if (BikesManager.Instance.ActiveBikes.Count == 0) return;

        SpawnCustomer();
    }

    private void SpawnCustomer()
    {
        if (!customerPrefab) return;

        Vector3 spawnPos = !spawnPoints.IsNullOrEmpty() ? spawnPoints.GetRandom().position : Vector3.zero;
        var customer = Instantiate(customerPrefab, spawnPos, Quaternion.identity, customersContainer);
        if (customer)
        {
            customer.Initialize(GetRandomCustomerTier(), CustomersVisuals.GetRandom());

            customer.OnCustomerLeft += OnCustomerLeft;
            _activeCustomers.Add(customer);
        }
    }

    /// Adds customer to the queue and assigns them a position.
    public void EnqueueCustomer(CustomerAI customer)
    {
        if (!_registerQueue.Contains(customer)) _registerQueue.Add(customer);
        UpdateQueuePositions();
    }

    /// Removes customer from queue and updates everyone else's position.
    public void DequeueCustomer(CustomerAI customer)
    {
        if (_registerQueue.Contains(customer)) _registerQueue.Remove(customer);
        UpdateQueuePositions();
    }

    /// Recalculates positions for all customers currently in the register queue.
    private void UpdateQueuePositions()
    {
        if (!registerPoint) return;

        // Direction the queue should extend (backwards from register)
        Vector3 queueDirection = -registerPoint.forward;
        if (queueDirection == Vector3.zero) queueDirection = Vector3.back;

        for (int i = 0; i < _registerQueue.Count; i++)
        {
            // Position: RegisterPoint + (BackwardOffset * Index)
            Vector3 targetPos = registerPoint.position + queueDirection * (i + 1) * queueSpacing;
            _registerQueue[i].SetQueueTarget(targetPos);
        }
    }

    public bool IsCustomerAtFrontOfQueue(CustomerAI customer) => _registerQueue.Count > 0 && _registerQueue[0] == customer;

    private void OnCustomerLeft(CustomerAI customer)
    {
        _activeCustomers.Remove(customer);
        // Ensure they are removed from the queue if they leave abruptly
        DequeueCustomer(customer);
    }

    private CustomerTier GetRandomCustomerTier()
    {
        float roll = Random.Range(0f, 1f);
        return roll switch
        {
            < 0.60f => CustomerTier.Regular,
            < 0.85f => CustomerTier.Enthusiast,
            < 0.95f => CustomerTier.Collector,
            _ => CustomerTier.VIP
        };
    }
}