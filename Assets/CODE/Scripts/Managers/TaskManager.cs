using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities.Extensions;

[RequireComponent(typeof(AudioSource))]
public class TaskManager : MonoSingleton<TaskManager>
{
    [Title("Settings")]
    [SerializeField] private int maxActiveTasks = 3;
    [InfoBox("Tasks will be given incrementally from this pool. When pool is exhausted, new tasks are generated dynamically.")]
    [SerializeField] private List<TaskSO> initialTaskPool = new();
    [SerializeField] private TaskGenerationConfig generationConfig;

    [Title("Sounds")]
    [SerializeField] private AudioClip newTaskSound;
    [SerializeField] private AudioClip taskCompletedSound;
    [SerializeField] private AudioSource audioSource;

    [Title("Runtime")]
    public List<TaskData> ActiveTasks = new();
    [ReadOnly, ShowInInspector] private int currentPoolIndex = 0;
    [ReadOnly, ShowInInspector] private bool usingDynamicGeneration = false;

    public event Action<TaskData> OnTaskAdded;
    public event Action<TaskData> OnTaskCompleted;
    public event Action<TaskData> OnTaskProgressUpdated;

    private void Start()
    {
        BikesManager.Instance.OnBikeSold += OnBikeSold;
        BikesManager.Instance.OnBikeAdded += OnBikePurchased;
        BrandManager.Instance.OnPartnershipSigned += OnBrandDealSigned;
        GameManager.Instance.OnCashChanged += OnCashChanged;
        StaffManager.Instance.OnStaffHired += OnStaffHired;

        if (!audioSource) audioSource = GetComponent<AudioSource>();

        // Load saved data
        currentPoolIndex = ES3.Load("TasksPoolIndex", 0);

        this.DelayedExecution(5f, () =>
        {
            LoadActiveTasks();

            // If no tasks or all tasks somehow completed, generate new batch
            if (ActiveTasks.Count == 0 || ActiveTasks.All(t => t.IsCompleted))
            {
                if (ActiveTasks.Count > 0) ActiveTasks.Clear();

                for (int i = 0; i < maxActiveTasks; i++) GenerateTask();
            }
            else if (ActiveTasks.Count < maxActiveTasks)
            {
                // Generate remaining tasks to reach maxActiveTasks
                int tasksToGenerate = maxActiveTasks - ActiveTasks.Count;
                for (int i = 0; i < tasksToGenerate; i++) GenerateTask();
            }
        });
    }

    private void Update()
    {
        UpdateTimeLimits();
    }

    private void LoadActiveTasks()
    {
        if (!ES3.KeyExists("ActiveTasksData")) return;

        List<SavedTaskData> savedTasks = ES3.Load<List<SavedTaskData>>("ActiveTasksData");

        foreach (var savedTask in savedTasks)
        {
            TaskSO taskSO = null;

            // Try to find the task in initial pool first
            if (savedTask.PoolIndex >= 0 && savedTask.PoolIndex < initialTaskPool.Count) taskSO = initialTaskPool[savedTask.PoolIndex];
            // If not from pool (poolIndex == -1), it was dynamically generated - we can't restore these perfectly
            // So we'll just skip them and generate new ones

            if (taskSO != null)
            {
                TaskData taskData = new(taskSO)
                {
                    CurrentProgress = savedTask.CurrentProgress,
                    TimeRemaining = savedTask.TimeRemaining,
                    IsCompleted = savedTask.IsCompleted
                };

                ActiveTasks.Add(taskData);
                OnTaskAdded?.Invoke(taskData);

                // If task was already completed, trigger the completed UI immediately
                if (taskData.IsCompleted) OnTaskCompleted?.Invoke(taskData);
            }
        }
    }

    private void SaveActiveTasks()
    {
        List<SavedTaskData> savedTasks = new();

        foreach (var task in ActiveTasks)
        {
            // Find which pool index this task came from
            int poolIndex = initialTaskPool.IndexOf(task.so);

            savedTasks.Add(new SavedTaskData
            {
                PoolIndex = poolIndex, // -1 if dynamically generated
                CurrentProgress = task.CurrentProgress,
                TimeRemaining = task.TimeRemaining,
                IsCompleted = task.IsCompleted
            });
        }

        ES3.Save("ActiveTasksData", savedTasks);
        ES3.Save("TasksPoolIndex", currentPoolIndex);
    }

    private void GenerateTask()
    {
        TaskSO taskToUse;

        // Check if we still have tasks in the initial pool
        if (currentPoolIndex < initialTaskPool.Count)
        {
            // Use task from pool incrementally
            taskToUse = initialTaskPool[currentPoolIndex];
            currentPoolIndex++;

            // Save immediately after incrementing index
            ES3.Save("TasksPoolIndex", currentPoolIndex);
        }
        else
        {
            // Pool exhausted - generate dynamic task
            if (!usingDynamicGeneration)
            {
                usingDynamicGeneration = true;
                Debug.Log("Initial task pool exhausted. Switching to dynamic generation.".RichColor(Color.yellow));
            }

            taskToUse = GenerateDynamicTask();

            if (taskToUse == null)
            {
                Debug.LogError("Failed to generate dynamic task!");
                return;
            }
        }

        TaskData newTaskData = new(taskToUse);
        ActiveTasks.Add(newTaskData);

        OnTaskAdded?.Invoke(newTaskData);
        audioSource.PlayOneShot(newTaskSound);

        // Save active tasks after adding new one
        SaveActiveTasks();
    }

    private TaskSO GenerateDynamicTask()
    {
        if (generationConfig == null)
        {
            Debug.LogError("TaskGenerationConfig is not assigned! Cannot generate dynamic tasks.");
            return null;
        }

        return generationConfig.GenerateRandomTask();
    }

    private void UpdateTimeLimits()
    {
        for (int i = ActiveTasks.Count - 1; i >= 0; i--)
        {
            var task = ActiveTasks[i];

            if (task.so.TimeLimit > 0)
            {
                task.TimeRemaining -= Time.deltaTime;

                if (task.TimeRemaining <= 0)
                {
                    // Task expired
                    ActiveTasks.RemoveAt(i);
                    SaveActiveTasks(); // Save after removing expired task
                    Debug.Log($"Task expired: {task.so.TaskTitle}".RichColor(Color.red));
                }
            }
        }
    }

    private void OnBikeSold(BikeInstance bike)
    {
        UpdateTaskProgress(TaskType.SellBikes, 1);
        UpdateTaskProgress(TaskType.SellSpecificBrand, 1, bike.BikeData.BrandData);
    }

    private void OnBikePurchased(BikeData bike, int amount)
    {
        UpdateTaskProgress(TaskType.PurchaseBikes, amount);
        UpdateTaskProgress(TaskType.PurchaseSpecificBike, amount, bike);
    }

    private void OnBrandDealSigned(BrandPartnership partnership)
    {
        UpdateTaskProgress(TaskType.SignBrandDeal, 1);
    }

    private void OnCashChanged()
    {
        int newCash = GameManager.Instance.CurrentCash;

        for (int i = ActiveTasks.Count - 1; i >= 0; i--)
        {
            var task = ActiveTasks[i];

            if (task.IsCompleted) continue;
            if (task.so.Type != TaskType.ReachCash) continue;

            task.CurrentProgress = newCash;
            OnTaskProgressUpdated?.Invoke(task);

            if (newCash >= task.so.RequiredCash) CompleteTask(task);
        }

        // Save progress for ReachCash tasks
        SaveActiveTasks();
    }

    public void OnBikePlaced(BikeData bike)
    {
        UpdateTaskProgress(TaskType.PlaceBikes, 1);
    }

    public void OnStaffHired(StaffMember staff)
    {
        UpdateTaskProgress(TaskType.HireStaff, 1);
        UpdateTaskProgress(TaskType.HireSpecificStaff, 1, staff.StaffData);
        UpdateTaskProgress(TaskType.HireStaffInSpecificRoom, 1, staff.AssignedRoom);
    }

    public void OnBikeUpgraded(BikeData bike)
    {
        UpdateTaskProgress(TaskType.UpgradeBikes, 1);
    }

    public void OnRoomUpgraded(GalleryRoom room, int newLevel)
    {
        for (int i = ActiveTasks.Count - 1; i >= 0; i--)
        {
            var task = ActiveTasks[i];
            if (task.IsCompleted) continue;
            if (task.so.Type != TaskType.UpgradeRoom) continue;

            if (newLevel >= task.so.RequiredRoomLevel)
            {
                task.CurrentProgress = newLevel;
                CompleteTask(task);
            }
        }
    }

    private void UpdateTaskProgress(TaskType type, int amount, object target = null)
    {
        bool progressMade = false;

        for (int i = ActiveTasks.Count - 1; i >= 0; i--)
        {
            var task = ActiveTasks[i];

            if (task.IsCompleted) continue;
            if (task.so.Type != type) continue;

            // Check if specific target matches (for brand/bike/staff specific tasks)
            if (!CheckTargetMatch(task, target)) continue;

            task.CurrentProgress += amount;
            OnTaskProgressUpdated?.Invoke(task);
            progressMade = true;

            if (task.CheckCompletion())
                CompleteTask(task);
        }

        // Save after any progress update
        if (progressMade) SaveActiveTasks();
    }

    private bool CheckTargetMatch(TaskData taskData, object target)
    {
        return taskData.so.Type switch
        {
            TaskType.SellSpecificBrand => target == null || (target is BrandData brand && brand == taskData.so.TargetBrand),
            TaskType.PurchaseSpecificBike => target == null || (target is BikeData bike && bike == taskData.so.TargetBike),
            TaskType.HireSpecificStaff => target == null || (target is StaffData staff && staff.StaffType == taskData.so.TargetStaffType),
            TaskType.HireStaffInSpecificRoom => target == null || (target is GalleryRoom room && room.ID == taskData.so.RequiredStaffRoomID),
            _ => true
        };
    }

    public void CompleteTask(TaskData taskData)
    {
        taskData.IsCompleted = true;

        // Give rewards
        GameManager.Instance.AddCash(taskData.so.CashReward);
        audioSource.PlayOneShot(taskCompletedSound);

        OnTaskCompleted?.Invoke(taskData);

        Debug.Log($"Task completed: {taskData.so.TaskTitle} | Reward: ${taskData.so.CashReward}".RichColor(Color.green));

        // Check if ALL tasks are now completed
        if (ActiveTasks.All(t => t.IsCompleted))
        {
            Debug.Log("All tasks completed! Generating new batch...".RichColor(Color.cyan));

            ActiveTasks.Clear();
            SaveActiveTasks(); // Save the cleared state
            this.DelayedExecution(3f, UIManager.Instance.RemoveAllTaksUI); // Clear UI after delay
            this.DelayedExecution(5f, () => { for (int i = 0; i < maxActiveTasks; i++) GenerateTask(); }); // Generate new batch of tasks
        }
        else SaveActiveTasks(); // Not all tasks completed yet, save current state
    }
}

[Serializable]
public class SavedTaskData
{
    public int PoolIndex; // Index in initialTaskPool, or -1 if dynamically generated
    public int CurrentProgress;
    public float TimeRemaining;
    public bool IsCompleted;
}

[Serializable]
public class TaskData
{
    public string ID;
    public TaskSO so;
    public int CurrentProgress;
    public bool IsCompleted;
    public float TimeRemaining;
    public DateTime StartTime;

    public TaskData(TaskSO so)
    {
        ID = Guid.NewGuid().ToString();
        this.so = so;
        CurrentProgress = 0;
        IsCompleted = false;
        TimeRemaining = so.TimeLimit;
        StartTime = DateTime.Now;
    }

    public float GetProgress()
    {
        int required = GetRequiredAmount();
        return required > 0 ? Mathf.Clamp01((float)CurrentProgress / required) : 0f;
    }

    public int GetRequiredAmount()
    {
        return so.Type switch
        {
            TaskType.SellBikes => so.RequiredSales,
            TaskType.SellSpecificBrand => so.RequiredBrandSales,
            TaskType.HireStaff => so.RequiredStaffCount,
            TaskType.HireSpecificStaff => so.RequiredSpecificStaff,
            TaskType.PurchaseBikes => so.RequiredBikePurchases,
            TaskType.PurchaseSpecificBike => so.RequiredSpecificBikes,
            TaskType.PlaceBikes => so.RequiredPlacedBikes,
            TaskType.UpgradeBikes => so.RequiredUpgrades,
            TaskType.ReachCash => so.RequiredCash,
            TaskType.SignBrandDeal => so.RequiredBrandDeals,
            TaskType.UpgradeRoom => so.RequiredRoomLevel,
            _ => 0
        };
    }

    public bool CheckCompletion()
    {
        return CurrentProgress >= GetRequiredAmount();
    }

#if UNITY_EDITOR
    [Button(ButtonSizes.Medium), HideIf("IsCompleted")] public void Complete() => TaskManager.Instance.CompleteTask(this);
#endif
}