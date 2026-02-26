using System;
using System.Collections.Generic;
using System.Linq;
using TouchCameraSystem;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utilities.Extensions;
using Lean.Pool;

[RequireComponent(typeof(BoxCollider), typeof(AudioSource))]
public class GalleryRoom : MonoBehaviour, IPointerClickHandler
{
    [Title("Room Settings")]
    [ReadOnly] public int ID;
    public string RoomName = "Gallery Room";
    [SerializeField] Vector2 area = new(22, 20);
    [SerializeField] private Canvas roomCanvas; // Used to spawn floating action buttons.

    [Title("Unlock Settings")]
    public bool IsUnlocked;
    [HideIf("IsUnlocked")] public int UnlockCost = 5000;
    [HideIf("IsUnlocked")] public int RequiredSales; // 0 means only cash needed
    [HideIf("IsUnlocked")][SerializeField] GameObject lockedVisuals;

    [Title("Upgrade Settings")]
    [Min(1)] public int CurrentLevel = 1;
    [SerializeField] private UpgradeData[] upgrades;
    [SerializeField] private GameObject upgradeEffect;
    [ReadOnly] public List<int> PurchasedSegmentIndices = new(); // Track which segments are bought

    [Title("Sounds")]
    [SerializeField] private AudioClip unlockSound;
    [SerializeField] private AudioClip upgradeSound;
    [SerializeField] private AudioClip levelCompleteSound;
    [SerializeField] private AudioSource audioSource;

    [Space(20), ReadOnly] public List<DisplayStation> Stations = new();

    private List<Button> _currentActionButtons = new(5);

    private void Awake()
    {
        if (!audioSource) audioSource = GetComponent<AudioSource>();

        DisableAllUpgrades();

        lockedVisuals.SetActive(true);
        if (IsUnlocked)
        {
            UnlockCost = 0;
            RequiredSales = 0;
            roomCanvas.gameObject.SetActive(false);
            UnlockRoom();
        }
        else CreateActionButton($"Unlock ${UnlockCost:N0}", () => UnlockRoom(), false);

        if (roomCanvas) roomCanvas.transform.forward = Camera.main.transform.forward;
    }

    [PropertySpace(20), Button(ButtonSizes.Large, DrawResult = false), HideIf("IsUnlocked")]
    public bool UnlockRoom()
    {
        if (GameManager.Instance.TotalSales < RequiredSales) return false;
        if (!GameManager.Instance.TrySpendCash(UnlockCost)) return false;

        RemoveActionButton($"Unlock ${UnlockCost:N0}");

        IsUnlocked = true;
        lockedVisuals.SetActive(false);

        CurrentLevel = 1;
        PurchasedSegmentIndices.Clear(); // Start fresh

        GalleryManager.Instance.OnRoomUnlocked?.Invoke();
        GalleryManager.Instance.UnlockedRooms.TryAdd(this);
        GalleryManager.Instance.LockedRooms.TryRemove(this);

        // Activate Starting Content.
        ActivateSegment(upgrades[CurrentLevel - 1].Segments[0]);

        audioSource.PlayOneShot(unlockSound);

        Debug.Log($"Gallery Room unlocked! {RoomName}".RichColor(Color.coral));

        return true;
    }

    public bool TryBuySegment(int segmentIndex)
    {
        if (!CanUpgrade(out bool segComplete) || segComplete) return false;

        int levelIndex = CurrentLevel - 1;
        var segments = upgrades[levelIndex].Segments;

        // Check if the segment index is valid
        if (segmentIndex < 0 || segmentIndex >= segments.Length) return false;

        // Check if already purchased
        if (PurchasedSegmentIndices.Contains(segmentIndex))
        {
            Debug.LogWarning("Segment already purchased!");
            return false;
        }

        var segment = segments[segmentIndex];

        // Check if can afford.
        if (!GameManager.Instance.TrySpendCash(segment.Cost)) return false;

        // Mark as purchased
        PurchasedSegmentIndices.Add(segmentIndex);

        // Activate segment content
        ActivateSegment(segment);

        audioSource.PlayOneShot(upgradeSound);

        Debug.Log($"Purchased segment: {segment.SegmentName}".RichColor(Color.cyan));
        return true;
    }

    private void ActivateSegment(UpgradeSegment segment, bool animate = true)
    {
        // Activate models
        if (!segment.Models.IsNullOrEmpty())
        {
            var seq = DOTween.Sequence();

            foreach (var model in segment.Models)
            {
                if (!model) continue;

                model.gameObject.SetActive(true);

                if (!animate) continue;

                var scale = model.localScale;
                var originalPos = model.localPosition;

                model.localScale = Vector3.zero;

                seq.Append(model.DOScale(scale, 0.2f).SetEase(Ease.OutBounce).OnUpdate(() =>
                {
                    float t = model.localScale.x / scale.x;
                    model.localPosition = originalPos * t;
                }));

                if (upgradeEffect)
                {
                    seq.AppendCallback(() =>
                    {
                        var effect = LeanPool.Spawn(upgradeEffect, model.position, Quaternion.identity);
                        effect.GetComponent<ParticleSystem>().Play(); // Needed for pooled effects
                        LeanPool.Despawn(effect, 3f);
                    });
                }
            }

            seq.Play();
        }

        // Activate stations
        if (segment.NewStations != null)
        {
            foreach (var station in segment.NewStations)
            {
                if (!station) continue;

                station.gameObject.SetActive(true);
                Stations.TryAdd(station);
            }
        }
    }

    public void CompleteLevelUpgrade()
    {
        // Apply level-wide changes

        CurrentLevel++;
        PurchasedSegmentIndices.Clear(); // Reset for a new level.
        TaskManager.Instance?.OnRoomUpgraded(this, CurrentLevel);

        int prevLevelIndex = CurrentLevel - 2;
        if (prevLevelIndex >= 0)
        {
            upgrades[prevLevelIndex].Segments.ForEach(seg =>
            {
                seg.Models.ForEach(m => { if (m) m.gameObject.SetActive(false); });
            });
        }

        var currentLevel = upgrades[CurrentLevel - 1];

        // Reposition stations
        var repositions = currentLevel.RepositionStations;
        if (!repositions.IsNullOrEmpty())
        {
            var seq = DOTween.Sequence();
            foreach (var reposition in repositions)
            {
                if (!reposition.Station) continue;
                seq.Append(reposition.Station.transform.DOLocalMove(reposition.NewLocalPosition, 0.3f).SetEase(Ease.OutBack));
                seq.Join(reposition.Station.transform.DOLocalRotate(reposition.NewLocalRotation, 0.3f).SetEase(Ease.OutBack));
            }
            seq.Play();
        }

        // Activate Starting Content.
        ActivateSegment(currentLevel.Segments[0]);

        audioSource.PlayOneShot(levelCompleteSound);

        Debug.Log($"{RoomName} upgraded to Level {CurrentLevel}!".RichColor(Color.green));
    }

    /// If segments are completed then player should complete the level before upgrading.
    public bool CanUpgrade(out bool segmentsComplete)
    {
        int totalSegments = upgrades[CurrentLevel - 1].Segments.Length;

        // All segments complete when purchased count equals total
        segmentsComplete = PurchasedSegmentIndices.Count >= totalSegments;

        // Can upgrade if there are more levels available
        if (CurrentLevel < upgrades.Length) return true;

        // Or if we're on last level but haven't bought all segments
        if (CurrentLevel == upgrades.Length && !segmentsComplete) return true;

        return false;
    }

    // Check if all segments complete and there are no upgrades left.
    public bool CanCompleteLevel()
    {
        int totalSegments = upgrades[CurrentLevel - 1].Segments.Length - 1; // Skip first as a starting content.
        return PurchasedSegmentIndices.Count >= totalSegments && CurrentLevel < upgrades.Length;
    }

    public Button CreateActionButton(string actionName, Action action, bool destroyOnClick = true)
    {
        var button = Instantiate(UIManager.Instance.RoomButton, roomCanvas.transform, false);
        button.name = actionName;

        button.onClick.AddListener(() =>
        {
            action.Invoke();
            if (destroyOnClick) Destroy(button.gameObject);
        });

        if (button.TryGetComponentInChildren(out TMP_Text text)) text.text = actionName;

        roomCanvas.gameObject.SetActive(true);

        _currentActionButtons.Add(button);

        return button;
    }

    public void RemoveActionButton(string actionName)
    {
        var button = _currentActionButtons.FirstOrDefault(b => b.name == actionName);
        if (!button) return;

        _currentActionButtons.Remove(button);
        Destroy(button);

        if (_currentActionButtons.Count <= 0) roomCanvas.gameObject.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        var cam = GameManager.Instance.TouchCamera.Cam;
        float distance = Vector3.Distance(cam.transform.position, transform.position);

        // Use the room's area to calculate FOV
        float maxSize = Mathf.Max(area.x / cam.aspect, area.y);
        float fov = 2f * Mathf.Atan(maxSize / (2f * distance)) * Mathf.Rad2Deg;

        // Add padding
        fov *= 1.5f;

        GameManager.Instance.TouchCamera.GetComponent<FocusCameraOnItem>().FocusCameraOnTarget(transform.position, Quaternion.identity, fov);

        if (IsUnlocked) UIManager.Instance.OpenGalleryRoom(this);
        else UIManager.Instance.CloseRoomManagement();
    }

    public Vector3 GetRandomPositionInsideArea() => RuntimeUtilities.GetRandomPositionInRectangle(transform.position, area);

    public DisplayStation GetAvailableStation() => Stations.FirstOrDefault(s => !s.CurrentBike);

    public bool IsSegmentPurchased(int segmentIndex) => PurchasedSegmentIndices.Contains(segmentIndex);

    /// Restores room state from saved data. Called by GalleryManager on load.
    public void RestoreFromSave(RoomSaveData saveData)
    {
        if (saveData == null) return;

        RemoveActionButton($"Unlock ${UnlockCost:N0}");
        DisableAllUpgrades();

        IsUnlocked = true;

        CurrentLevel = saveData.Level;
        PurchasedSegmentIndices = new List<int>(saveData.PurchasedSegmentIndices ?? new List<int>());

        // Hide locked visuals
        if (lockedVisuals) lockedVisuals.SetActive(false);

        // Add all previous levels' display stations to the list
        for (int i = 0; i < CurrentLevel - 1; i++)
        {
            var upgrade = upgrades[i];

            // Add all stations from each segment of previous levels
            foreach (var segment in upgrade.Segments)
            {
                if (segment.NewStations != null)
                {
                    foreach (var station in segment.NewStations)
                    {
                        if (station)
                        {
                            station.gameObject.SetActive(true);
                            Stations.TryAdd(station);
                        }
                    }
                }
            }
        }

        // and apply repositions.
        for (int i = 0; i <= CurrentLevel - 1; i++)
        {
            var upgrade = upgrades[i];

            // Apply station repositions from previous levels (instant, no animation)
            foreach (var reposition in upgrade.RepositionStations)
            {
                if (reposition.Station)
                {
                    reposition.Station.transform.localPosition = reposition.NewLocalPosition;
                    reposition.Station.transform.localEulerAngles = reposition.NewLocalRotation;
                }
            }
        }

        // Activate all purchased segments for current level
        if (CurrentLevel <= upgrades.Length)
        {
            var currentSegments = GetCurrentLevelSegments();

            // Always activate the first segment (starting content)
            if (currentSegments.Length > 0) ActivateSegment(currentSegments[0], false);

            // Activate purchased segments
            foreach (int segIndex in PurchasedSegmentIndices)
            {
                if (segIndex > 0 && segIndex < currentSegments.Length)
                {
                    ActivateSegment(currentSegments[segIndex], false);
                }
            }
        }
    }

    public UpgradeSegment[] GetCurrentLevelSegments()
    {
        int levelIndex = CurrentLevel - 1;
        return levelIndex >= upgrades.Length ? null : upgrades[levelIndex].Segments;
    }

    private void DisableAllUpgrades()
    {
        foreach (var upgrade in upgrades)
        {
            upgrade.Segments.ForEach(seg =>
            {
                seg.Models.ForEach(m => { if (m) m.gameObject.SetActive(false); });
                seg.NewStations.ForEach(s => { if (s) s.gameObject.SetActive(false); });
            });
        }
    }

    [Serializable]
    public struct UpgradeData
    {
        [ListDrawerSettings(ListElementLabelName = "SegmentName")] public UpgradeSegment[] Segments;
        public StationReposition[] RepositionStations;
    }

    [Serializable]
    public struct UpgradeSegment
    {
        [HorizontalGroup("Info", Width = 50f), PreviewField(Alignment = ObjectFieldAlignment.Left), HideLabel] public Sprite Icon;
        [HorizontalGroup("Info"), VerticalGroup("Info/Data"), LabelText("Name")] public string SegmentName;
        [HorizontalGroup("Info"), VerticalGroup("Info/Data")] public int Cost;

        [Space(20f)]
        public Transform[] Models;
        public DisplayStation[] NewStations;
    }

    [Serializable]
    public struct StationReposition
    {
        public DisplayStation Station;
        public Vector3 NewLocalPosition;
        public Vector3 NewLocalRotation;
    }
}