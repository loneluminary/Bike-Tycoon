using System;
using System.Linq;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using Lean.Pool;
using Utilities.Extensions;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TouchCameraSystem;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class Entrance : MonoBehaviour, IPointerClickHandler
{
    [Title("Upgrade Settings")]
    [SerializeField] private EntranceUpgrade[] upgrades;
    [SerializeField] private GameObject upgradeEffect;
    [ReadOnly] public int CurrentLevel = 1;
    [ReadOnly] public List<int> PurchasedSegmentIndices = new();

    [Title("Sounds")]
    [SerializeField] private AudioClip upgradeSound;
    [SerializeField] private AudioSource audioSource;

    [Title("Current Bonuses")]
    public int MaxSimultaneousCustomers = 5;
    public float CustomerSpawnRateBonus = 0f;
    public float CustomerPatienceBonus = 0f;
    public float CustomerSpendingBonus = 0f;

    public UnityEvent<Entrance> OnEntranceUpgraded;

    private void Awake()
    {
        if (!audioSource) audioSource = GetComponent<AudioSource>();

        DisableAllUpgrades();
        Load();
    }

    public bool TryBuySegment(int segmentIndex)
    {
        if (!CanUpgrade(out bool levelComplete)) return false;

        int levelIndex = CurrentLevel - 1;
        if (levelIndex >= upgrades.Length) return false;

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

        ApplySegmentBonuses(segment);
        ActivateSegment(segment);
        Save();

        audioSource.PlayOneShot(upgradeSound);

        Debug.Log($"Entrance upgraded: {segment.SegmentName} | Perks applied".RichColor(Color.cyan));
        return true;
    }

    private void ActivateSegment(EntranceSegment segment, bool animate = true)
    {
        if (segment.Models.IsNullOrEmpty()) return;

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
                    effect.GetComponent<ParticleSystem>().Play();
                    LeanPool.Despawn(effect, 3f);
                });
            }
        }

        seq.Play();
    }

    private void ApplySegmentBonuses(EntranceSegment segment)
    {
        // All additive bonuses
        CustomerSpawnRateBonus += segment.SpawnRateBonus;
        CustomerPatienceBonus += segment.PatienceBonus;
        CustomerSpendingBonus += segment.SpendingBonus;
        MaxSimultaneousCustomers += segment.MaxCustomersBonus;

        // Notify listeners (like CustomerManager)
        OnEntranceUpgraded?.Invoke(this);
    }

    public void CompleteLevelUpgrade()
    {
        CurrentLevel++;
        PurchasedSegmentIndices.Clear();

        // Disable previous level models
        int prevLevelIndex = CurrentLevel - 2;
        if (prevLevelIndex >= 0 && prevLevelIndex < upgrades.Length)
        {
            upgrades[prevLevelIndex].Segments.ForEach(seg =>
            {
                seg.Models.ForEach(m => { if (m) m.gameObject.SetActive(false); });
            });
        }

        // Activate new level starting segment
        if (CurrentLevel - 1 < upgrades.Length) ActivateSegment(upgrades[CurrentLevel - 1].Segments[0]);
        Save();

        audioSource.PlayOneShot(upgradeSound);

        // Notify listeners
        OnEntranceUpgraded?.Invoke(this);

        Debug.Log($"Entrance upgraded to Level {CurrentLevel}!".RichColor(Color.yellow));
    }

    public bool CanUpgrade(out bool levelComplete)
    {
        int levelIndex = CurrentLevel - 1;
        if (levelIndex >= upgrades.Length)
        {
            levelComplete = true;
            return false;
        }

        int totalSegments = upgrades[levelIndex].Segments.Length;
        levelComplete = PurchasedSegmentIndices.Count >= totalSegments;

        return true;
    }

    public bool CanCompleteLevel()
    {
        int levelIndex = CurrentLevel - 1;
        if (levelIndex >= upgrades.Length) return false;

        int totalSegments = upgrades[levelIndex].Segments.Length - 1;
        return PurchasedSegmentIndices.Count >= totalSegments && CurrentLevel < upgrades.Length;
    }

    public EntranceSegment[] GetCurrentLevelSegments()
    {
        int levelIndex = CurrentLevel - 1;
        return levelIndex >= upgrades.Length ? null : upgrades[levelIndex].Segments;
    }

    public bool IsSegmentPurchased(int segmentIndex) => PurchasedSegmentIndices.Contains(segmentIndex);

    public void OnPointerClick(PointerEventData eventData)
    {
        GameManager.Instance.TouchCamera.GetComponent<FocusCameraOnItem>().FocusCameraOnTarget(transform.position, Quaternion.identity);
        UIManager.Instance.OpenEnterence(this);
    }

    #region Save/Load

    public void Save()
    {
        ES3.Save("EntranceLevel", CurrentLevel);
        ES3.Save("EntranceSegments", PurchasedSegmentIndices);
    }

    public void Load()
    {
        CurrentLevel = ES3.Load("EntranceLevel", 1);
        PurchasedSegmentIndices = ES3.Load("EntranceSegments", PurchasedSegmentIndices);
        RestoreUpgradeState();
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

        return $"Entrence restored: Level {CurrentLevel}, {PurchasedSegmentIndices.Count} segments purchased";
    }

    #endregion

    private void DisableAllUpgrades()
    {
        foreach (var upgrade in upgrades)
        {
            upgrade.Segments.ForEach(seg =>
            {
                seg.Models.ForEach(m => { if (m) m.gameObject.SetActive(false); });
            });
        }
    }

    [Serializable]
    public struct EntranceUpgrade
    {
        [ListDrawerSettings(ListElementLabelName = "SegmentName")]
        public EntranceSegment[] Segments;
    }

    [Serializable]
    public struct EntranceSegment
    {
        [HorizontalGroup("Info", Width = 50f), PreviewField(Alignment = ObjectFieldAlignment.Left), HideLabel] public Sprite Icon;
        [HorizontalGroup("Info"), VerticalGroup("Info/Data"), LabelText("Name")] public string SegmentName;
        [HorizontalGroup("Info"), VerticalGroup("Info/Data")] public int Cost;

        [Space(10f), Title("Perks")]
        [Tooltip("Multiplier for customer spawn rate (0.1 = 10% faster)")] public float SpawnRateBonus;
        [Tooltip("Additional max customers allowed at once")] public int MaxCustomersBonus;
        [Tooltip("Multiplier for customer patience (0.2 = 20% more patient)")] public float PatienceBonus;
        [Tooltip("Multiplier for customer spending (0.15 = 15% more spending)")] public float SpendingBonus;

        [Space(20f)]
        public Transform[] Models;
    }
}