using System;
using TouchCameraSystem;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities.Extensions;
using System.Text;
using TUTORIAL_SYSTEM;

public class GameManager : MonoSingleton<GameManager>
{
    [Header("Economy")]
    public int CurrentCash = 5000;
    public int TotalSales;

    // Events
    public event Action OnCashChanged;
    public event Action OnSalesChanged;

    [ReadOnly] public MobileTouchCamera TouchCamera;
    [ReadOnly] public Entrance Entrance;

    private void Start()
    {
        Application.targetFrameRate = 60;

        LoadGame();

        TouchCamera = FindFirstObjectByType<MobileTouchCamera>();
        Entrance = FindFirstObjectByType<Entrance>();

        this.RepeatExecutionWhile(() => true, 5f, 5f, SaveGame);
    }

    private void Update()
    {
        if (TouchCamera) TouchCamera.enabled = UIManager.Instance.CurrentOpenPanel == UIManager.PanelType.None && TutorialManager.Instance.Tutorials[0].IsTutorialCompleted;
    }

    public void AddCash(int amount)
    {
        CurrentCash += amount;
        OnCashChanged?.Invoke();
    }

    public bool TrySpendCash(int amount)
    {
        if (CurrentCash < amount)
        {
            UIManager.Instance.ShowToastMessage("Not enough cash available");
            return false;
        }

        CurrentCash -= amount;
        OnCashChanged?.Invoke();
        return true;
    }

    public void SetCash(int amount)
    {
        CurrentCash = amount;
        OnCashChanged?.Invoke();
    }

    public void RecordSale()
    {
        TotalSales++;
        OnSalesChanged?.Invoke();
    }

    public void SetCash_DevelopersOption()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD

        var window = UI_InputWindow.HasInstance;
        if (!window || window.IsOpen) return;

        UIManager.Instance.CurrentOpenPanel = UIManager.PanelType.Popup;

        window.Show("Set Cash Amount", input =>
        {
            UIManager.Instance.CurrentOpenPanel = UIManager.PanelType.None;

            if (int.TryParse(input, out int newAmount)) SetCash(newAmount);

        }, () => UIManager.Instance.CurrentOpenPanel = UIManager.PanelType.None, defaultInput: CurrentCash.ToString(), validCharacters: "0123456789", characterLimit: 10);

#endif
    }

    // Simple save/load
    public void SaveGame()
    {
        string rooms = GalleryManager.Instance.SaveRooms();
        string bikes = BikesManager.Instance.SaveBikes();
        string staffs = StaffManager.Instance.SaveStaff();
        string partnerships = BrandManager.Instance.SavePartnerships();

        PlayerPrefs.SetInt("Cash", CurrentCash);
        PlayerPrefs.SetInt("Sales", TotalSales);
        PlayerPrefs.Save();

#if UNITY_EDITOR
        StringBuilder stringBuilder = new();
        stringBuilder.AppendLine(rooms);
        stringBuilder.AppendLine(bikes);
        stringBuilder.AppendLine(staffs);
        stringBuilder.AppendLine(partnerships);
        stringBuilder.AppendLine($"Saved {CurrentCash} Cash and {TotalSales} TotalSales");
        Debug.Log(stringBuilder.ToString().RichColor(Color.green));
#endif
    }

    public void LoadGame()
    {
        string rooms = GalleryManager.Instance.LoadRooms();
        string bikes = BikesManager.Instance.LoadBikes();
        string staffs = StaffManager.Instance.LoadStaff();
        string partnerships = BrandManager.Instance.LoadPartnerships();

        int cash = CurrentCash;
        CurrentCash = PlayerPrefs.GetInt("Cash", cash);
        TotalSales = PlayerPrefs.GetInt("Sales", 0);

        OnSalesChanged?.Invoke();
        OnCashChanged?.Invoke();

#if UNITY_EDITOR
        StringBuilder stringBuilder = new();
        stringBuilder.AppendLine(rooms);
        stringBuilder.AppendLine(bikes);
        stringBuilder.AppendLine(staffs);
        stringBuilder.AppendLine(partnerships);
        stringBuilder.AppendLine($"Loaded {CurrentCash} Cash and {TotalSales} TotalSales");
        Debug.Log(stringBuilder.ToString().RichColor(Color.green));
#endif
    }

    public static void DeleteAllSaveData()
    {
        ES3.DeleteFile();
        PlayerPrefs.DeleteAll();
    }

    private void OnApplicationQuit() => SaveGame();
}