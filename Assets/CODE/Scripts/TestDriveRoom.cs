using System;
using System.Collections.Generic;
using TouchCameraSystem;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utilities.Extensions;
using Lean.Pool;
using TMPro;
using DG.Tweening;

[RequireComponent(typeof(AudioSource))]
public class TestDriveRoom : MonoBehaviour, IPointerClickHandler
{
    [Title("Unlock Settings")]
    public bool IsUnlocked;
    [HideIf("IsUnlocked")][SerializeField] private Button unlockButton;
    [HideIf("IsUnlocked")][SerializeField] private int unlockCost = 10000;
    [HideIf("IsUnlocked")][SerializeField] private GameObject lockedVisuals;

    [Title("Upgrade Settings")]
    [Min(1)] public int CurrentLevel = 1;
    [SerializeField] private UpgradeData[] upgrades;
    [SerializeField] private GameObject upgradeVfx;
    [ReadOnly] public List<int> PurchasedSegmentIndices = new();

    [Title("Current Bonuses")]
    [ReadOnly] public int MaxTestDrivers = 3;
    [ReadOnly] public float PurchaseChanceBonus = 0f;
    [ReadOnly] public float TestDriveSpeedMultiplier = 1f;
    [ReadOnly] public List<BikeData> AvailableTestBikes = new();
    [ReadOnly] public List<DriveRoute> TestRoutes = new();

    [Title("Sounds")]
    [SerializeField] private AudioClip unlockSound;
    [SerializeField] private AudioClip upgradeSound;
    [SerializeField] private AudioClip levelCompleteSound;
    [SerializeField] private AudioSource audioSource;

    [Space(20f), ReadOnly] public List<CustomerAI> ActiveTestDrivers = new();

    private void Start()
    {
        if (!audioSource) audioSource = GetComponent<AudioSource>();
        DisableAllUpgrades();
    }

    public bool TryUnlock()
    {
        if (IsUnlocked) return false;
        if (!GameManager.Instance.TrySpendCash(unlockCost)) return false;

        IsUnlocked = true;
        CurrentLevel = 1;
        PurchasedSegmentIndices.Clear();

        if (lockedVisuals) lockedVisuals.SetActive(false);

        // Activate starting content (first segment of level 1)
        if (upgrades.Length > 0 && upgrades[0].Segments.Length > 0)
        {
            ActivateSegment(upgrades[0].Segments[0]);
        }

        if (audioSource) audioSource.PlayOneShot(unlockSound);
        if (upgradeVfx) LeanPool.Despawn(LeanPool.Spawn(upgradeVfx, transform.position, Quaternion.identity), 3f);

        SaveToES3();

        Debug.Log("Test Drive Room unlocked!".RichColor(Color.coral));
        return true;
    }

    public bool TryBuySegment(int segmentIndex)
    {
        if (!CanUpgrade(out bool segComplete) || segComplete) return false;

        int levelIndex = CurrentLevel - 1;
        var segments = upgrades[levelIndex].Segments;

        if (segmentIndex < 0 || segmentIndex >= segments.Length) return false;

        if (PurchasedSegmentIndices.Contains(segmentIndex))
        {
            Debug.LogWarning("Segment already purchased!");
            return false;
        }

        var segment = segments[segmentIndex];

        if (!GameManager.Instance.TrySpendCash(segment.Cost)) return false;

        PurchasedSegmentIndices.Add(segmentIndex);

        ActivateSegment(segment);
        ApplySegmentBonuses(segment);
        SaveToES3();

        audioSource.PlayOneShot(upgradeSound);

        Debug.Log($"Purchased test drive segment: {segment.Name}".RichColor(Color.cyan));
        return true;
    }

    private void ActivateSegment(UpgradeSegment segment, bool animate = true)
    {
        // Activate models
        if (!segment.ActivateModels.IsNullOrEmpty())
        {
            var seq = DOTween.Sequence();

            foreach (var model in segment.ActivateModels)
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

                if (upgradeVfx)
                {
                    seq.AppendCallback(() =>
                    {
                        var effect = LeanPool.Spawn(upgradeVfx, model.position, Quaternion.identity);
                        effect.GetComponent<ParticleSystem>().Play();
                        LeanPool.Despawn(effect, 3f);
                    });
                }
            }

            seq.Play();
        }

        // Deactivate models
        if (!segment.DeactivateModels.IsNullOrEmpty())
        {
            foreach (var model in segment.DeactivateModels)
            {
                if (model) model.gameObject.SetActive(false);
            }
        }
    }

    private void ApplySegmentBonuses(UpgradeSegment segment)
    {
        // Update max values (use highest)
        MaxTestDrivers = Mathf.Max(MaxTestDrivers, segment.MaxTestDrivers);
        TestDriveSpeedMultiplier = Mathf.Max(TestDriveSpeedMultiplier, segment.TestDriveSpeedMultiplier);

        // Add bonus (additive)
        PurchaseChanceBonus += segment.PurchaseChanceBonus;

        // Add unlocked bikes
        if (!segment.UnlockBikes.IsNullOrEmpty())
        {
            foreach (var bike in segment.UnlockBikes)
            {
                if (!AvailableTestBikes.Contains(bike))
                    AvailableTestBikes.Add(bike);
            }
        }

        // Add unlocked routes
        if (!segment.UnlockRoutes.IsNullOrEmpty())
        {
            foreach (var route in segment.UnlockRoutes)
            {
                if (!TestRoutes.Contains(route))
                    TestRoutes.Add(route);
            }
        }
    }

    public void CompleteLevelUpgrade()
    {
        CurrentLevel++;
        PurchasedSegmentIndices.Clear();

        // Disable previous level's models
        int prevLevelIndex = CurrentLevel - 2;
        if (prevLevelIndex >= 0)
        {
            upgrades[prevLevelIndex].Segments.ForEach(seg =>
            {
                seg.ActivateModels.ForEach(m => { if (m) m.gameObject.SetActive(false); });
            });
        }

        // Activate starting content of new level
        var currentLevel = upgrades[CurrentLevel - 1];
        if (currentLevel.Segments.Length > 0)
        {
            ActivateSegment(currentLevel.Segments[0]);
        }

        audioSource.PlayOneShot(levelCompleteSound);
        SaveToES3();

        Debug.Log($"Test Drive Room upgraded to Level {CurrentLevel}!".RichColor(Color.green));
    }

    public bool CanUpgrade(out bool segmentsComplete)
    {
        int totalSegments = upgrades[CurrentLevel - 1].Segments.Length;
        segmentsComplete = PurchasedSegmentIndices.Count >= totalSegments;

        if (CurrentLevel < upgrades.Length) return true;
        if (CurrentLevel == upgrades.Length && !segmentsComplete) return true;

        return false;
    }

    public bool CanCompleteLevel()
    {
        int totalSegments = upgrades[CurrentLevel - 1].Segments.Length - 1; // Skip first as starting content
        return PurchasedSegmentIndices.Count >= totalSegments && CurrentLevel < upgrades.Length;
    }

    public bool IsSegmentPurchased(int segmentIndex) => PurchasedSegmentIndices.Contains(segmentIndex);

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
                seg.ActivateModels.ForEach(m => { if (m) m.gameObject.SetActive(false); });
            });
        }
    }

    public float GetTotalPurchaseChance() => 0.30f + PurchaseChanceBonus; // Base 30% + bonuses

    public void OnPointerClick(PointerEventData eventData)
    {
        GameManager.Instance.TouchCamera.GetComponent<FocusCameraOnItem>().FocusCameraOnTarget(transform.position, Quaternion.identity);

        if (IsUnlocked) UIManager.Instance.OpenTestDriveRoomPanel(this);
        else UIManager.Instance.CloseRoomManagement();
    }

    public bool CanTestDrive() => IsUnlocked && ActiveTestDrivers.Count < MaxTestDrivers;

    public BikeData GetRandomTestBike() => AvailableTestBikes.IsNullOrEmpty() ? null : AvailableTestBikes.GetRandom();

    #region Save/Load

    private void SaveToES3()
    {
        ES3.Save("TestDriveRoomUnlocked", IsUnlocked);
        ES3.Save("TestDriveRoomLevel", CurrentLevel);
        ES3.Save("TestDriveSegments", PurchasedSegmentIndices);
    }

    public string Load()
    {
        IsUnlocked = ES3.Load("TestDriveRoomUnlocked", false);

        if (lockedVisuals) lockedVisuals.SetActive(!IsUnlocked);

        if (!IsUnlocked)
        {
            unlockButton.GetComponentInChildren<TMP_Text>().text = $"Unlock ${unlockCost:N0}";
            unlockButton.onClick.AddListener(() =>
            {
                if (TryUnlock()) Destroy(unlockButton.transform.parent.gameObject);
            });
        }
        else
        {
            Destroy(unlockButton.transform.parent.gameObject);

            CurrentLevel = ES3.Load("TestDriveRoomLevel", 1);
            PurchasedSegmentIndices = ES3.Load("TestDriveSegments", new List<int>());

            return RestoreUpgradeState();
        }

        return string.Empty;
    }

    private string RestoreUpgradeState()
    {
        DisableAllUpgrades();

        // Apply all purchased segments from current level
        if (CurrentLevel <= upgrades.Length)
        {
            var currentSegments = GetCurrentLevelSegments();

            // Always activate the first segment (starting content)
            if (currentSegments.Length > 0)
            {
                ActivateSegment(currentSegments[0], false);
                ApplySegmentBonuses(currentSegments[0]);
            }

            // Activate purchased segments
            foreach (int segIndex in PurchasedSegmentIndices)
            {
                if (segIndex > 0 && segIndex < currentSegments.Length)
                {
                    ActivateSegment(currentSegments[segIndex], false);
                    ApplySegmentBonuses(currentSegments[segIndex]);
                }
            }
        }

        return $"Test Drive Room restored: Level {CurrentLevel}, {PurchasedSegmentIndices.Count} segments purchased";
    }

    #endregion

    #region Data Structures

    [Serializable]
    public struct UpgradeData
    {
        [ListDrawerSettings(ListElementLabelName = "Name")] public UpgradeSegment[] Segments;
    }

    [Serializable]
    public struct UpgradeSegment
    {
        [HorizontalGroup("Info", Width = 50f), PreviewField(Alignment = ObjectFieldAlignment.Left), HideLabel] public Sprite Icon;
        [HorizontalGroup("Info"), VerticalGroup("Info/Data")] public string Name;
        [HorizontalGroup("Info"), VerticalGroup("Info/Data")] public int Cost;

        [Title("Bonuses")]
        [Tooltip("Added to base 30% purchase chance (0.10 = +10%)")]
        [Range(0f, 0.50f)] public float PurchaseChanceBonus;
        [Tooltip("Speed multiplier for test drives (1.5 = 50% faster, only highest applies)")]
        [Range(1f, 3f)] public float TestDriveSpeedMultiplier;
        [Tooltip("Max number of concurrent test drivers (only highest applies)")]
        [Range(0, 10)] public int MaxTestDrivers;

        [Title("Unlocks")]
        public List<BikeData> UnlockBikes;
        public List<DriveRoute> UnlockRoutes;

        [Title("Visual Models")]
        public List<Transform> ActivateModels;
        public List<Transform> DeactivateModels;
    }

    #endregion
}