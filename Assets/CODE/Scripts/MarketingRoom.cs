using System.Collections.Generic;
using TouchCameraSystem;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utilities.Extensions;
using DG.Tweening;
using Lean.Pool;
using TMPro;

[RequireComponent(typeof(AudioSource))]
public class MarketingRoom : MonoBehaviour, IPointerClickHandler
{
    [Title("Unlock Settings")]
    public bool IsUnlocked;
    [HideIf("IsUnlocked")][SerializeField] private Button unlockButton;
    [HideIf("IsUnlocked")][SerializeField] private int unlockCost = 15000;
    [HideIf("IsUnlocked")][SerializeField] private GameObject lockedVisuals;

    [Title("Upgrade Settings")]
    [Min(1)] public int CurrentLevel = 1;
    [SerializeField] private UpgradeData[] upgrades;
    [SerializeField] private GameObject upgradeVfx;
    [ReadOnly] public List<int> PurchasedSegmentIndices = new();

    [Title("Current Bonuses")]
    [ReadOnly] public float CustomerSpawnRateBonus = 0f;
    [ReadOnly] public float SalePriceBonus = 0f;
    [ReadOnly] public float BrandReputationBonus = 0f;
    [ReadOnly] public int MaxActiveCampaigns = 1;

    [Title("Campaigns")]
    [SerializeField] List<Sprite> campaignIcons = new();
    [ReadOnly] public List<MarketingCampaign> ActiveCampaigns = new();

    [Title("Sounds")]
    [SerializeField] private AudioClip unlockSound;
    [SerializeField] private AudioClip upgradeSound;
    [SerializeField] private AudioClip levelCompleteSound;
    [SerializeField] private AudioClip campaignLaunchSound;
    [SerializeField] private AudioSource audioSource;

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

        Save();

        Debug.Log("Marketing Room unlocked!".RichColor(Color.coral));
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
        Save();

        audioSource.PlayOneShot(upgradeSound);

        Debug.Log($"Purchased marketing segment: {segment.Name}".RichColor(Color.cyan));
        return true;
    }

    private void ActivateSegment(UpgradeSegment segment, bool animate = true)
    {
        if (segment.Models.IsNullOrEmpty()) return;

        var seq = DOTween.Sequence();

        foreach (var model in segment.Models)
        {
            if (!model) continue;

            if (animate)
            {
                if (!model.gameObject.activeSelf) // if model is already active then assume it is being upgraded so dont animate just play vfx
                {
                    var scale = model.localScale;
                    var originalPos = model.localPosition;

                    model.localScale = Vector3.zero;

                    seq.Append(model.DOScale(scale, 0.2f).SetEase(Ease.OutBounce).OnUpdate(() =>
                    {
                        float t = model.localScale.x / scale.x;
                        model.localPosition = originalPos * t;
                    }));
                }

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
            
            model.gameObject.SetActive(true);
        }

        seq.Play();
    }

    private void ApplySegmentBonuses(UpgradeSegment segment)
    {
        // Add bonuses from this segment
        CustomerSpawnRateBonus += segment.CustomerSpawnRateBonus;
        SalePriceBonus += segment.SalePriceBonus;
        BrandReputationBonus += segment.BrandReputationBonus;
        MaxActiveCampaigns = Mathf.Max(MaxActiveCampaigns, segment.MaxActiveCampaigns);
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
                seg.Models.ForEach(m => { if (m) m.gameObject.SetActive(false); });
            });
        }

        // Activate starting content of new level
        var currentLevel = upgrades[CurrentLevel - 1];
        if (currentLevel.Segments.Length > 0)
        {
            ActivateSegment(currentLevel.Segments[0]);
        }

        audioSource.PlayOneShot(levelCompleteSound);
        Save();

        Debug.Log($"Marketing Room upgraded to Level {CurrentLevel}!".RichColor(Color.green));
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
                seg.Models.ForEach(m => { if (m) m.gameObject.SetActive(false); });
            });
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        GameManager.Instance.TouchCamera.GetComponent<FocusCameraOnItem>().FocusCameraOnTarget(transform.position, Quaternion.identity);

        if (IsUnlocked) UIManager.Instance.OpenMarketingRoomPanel(this);
        else UIManager.Instance.CloseRoomManagement();
    }

    #region Campaign Management

    public bool TryStartCampaign(MarketingCampaignType type, int duration, int cost)
    {
        if (ActiveCampaigns.Count >= MaxActiveCampaigns) return false;
        if (!GameManager.Instance.TrySpendCash(cost)) return false;

        MarketingCampaign campaign = new()
        {
            Type = type,
            Icon = GetCampaignIcon(type),
            Duration = duration,
            TimeRemaining = duration,
            IsActive = true
        };

        ActiveCampaigns.Add(campaign);

        audioSource.PlayOneShot(campaignLaunchSound);

        Debug.Log($"Started {type} campaign for {duration} seconds".RichColor(Color.yellow));
        return true;
    }

    private void Update()
    {
        if (!IsUnlocked) return;

        // Update campaigns
        for (int i = ActiveCampaigns.Count - 1; i >= 0; i--)
        {
            MarketingCampaign campaign = ActiveCampaigns[i];
            campaign.TimeRemaining -= Time.deltaTime;

            if (campaign.TimeRemaining <= 0)
            {
                ActiveCampaigns.RemoveAt(i);
                Debug.Log($"{campaign.Type} campaign ended".RichColor(Color.gray));
            }
        }
    }

    public float GetTotalCustomerSpawnBonus()
    {
        float total = CustomerSpawnRateBonus;

        foreach (var campaign in ActiveCampaigns)
        {
            total += campaign.Type switch
            {
                MarketingCampaignType.SocialMedia => 0.50f,
                MarketingCampaignType.Billboard => 0.30f,
                MarketingCampaignType.RadioAd => 0.20f,
                _ => 0f
            };
        }

        return total;
    }

    public float GetTotalSalePriceBonus()
    {
        float total = SalePriceBonus;

        foreach (var campaign in ActiveCampaigns)
        {
            if (campaign.Type == MarketingCampaignType.PrestigeEvent)
                total += 0.15f;
        }

        return total;
    }

    public Sprite GetCampaignIcon(MarketingCampaignType campaignType)
    {
        return campaignType switch
        {
            MarketingCampaignType.Billboard => campaignIcons[0],
            MarketingCampaignType.PrestigeEvent => campaignIcons[1],
            MarketingCampaignType.RadioAd => campaignIcons[2],
            MarketingCampaignType.SocialMedia => campaignIcons[3],
            _ => campaignIcons[0],
        };
    }

    #endregion

    #region Save/Load

    public void Save()
    {
        ES3.Save("MarketingRoomUnlocked", IsUnlocked);
        ES3.Save("MarketingRoomLevel", CurrentLevel);
        ES3.Save("MarketingSegments", PurchasedSegmentIndices);
    }

    public string Load()
    {
        IsUnlocked = ES3.Load("MarketingRoomUnlocked", false);

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
            CurrentLevel = ES3.Load("MarketingRoomLevel", 1);
            PurchasedSegmentIndices = ES3.Load("MarketingSegments", PurchasedSegmentIndices);

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

        return $"Marketing Room restored: Level {CurrentLevel}, {PurchasedSegmentIndices.Count} segments purchased";
    }

    #endregion

    #region Static Helper Methods

    public static string GetCampaignDescription(MarketingCampaignType type)
    {
        return type switch
        {
            MarketingCampaignType.SocialMedia => "+50% Customer Spawn",
            MarketingCampaignType.Billboard => "+30% Customer Spawn",
            MarketingCampaignType.RadioAd => "+20% Customer Spawn",
            MarketingCampaignType.PrestigeEvent => "+15% Sale Prices",
            _ => ""
        };
    }

    public static string GetCampaignName(MarketingCampaignType type)
    {
        return type switch
        {
            MarketingCampaignType.SocialMedia => "Social Media",
            MarketingCampaignType.Billboard => "Billboard",
            MarketingCampaignType.RadioAd => "Radio Ad",
            MarketingCampaignType.PrestigeEvent => "Prestige Event",
            _ => "Unknown"
        };
    }

    #endregion

    #region Data Structures

    [System.Serializable]
    public struct UpgradeData
    {
        [ListDrawerSettings(ListElementLabelName = "Name")] public UpgradeSegment[] Segments;
    }

    [System.Serializable]
    public struct UpgradeSegment
    {
        [HorizontalGroup("Info", Width = 50f), PreviewField(Alignment = ObjectFieldAlignment.Left), HideLabel] public Sprite Icon;
        [HorizontalGroup("Info"), VerticalGroup("Info/Data")] public string Name;
        [HorizontalGroup("Info"), VerticalGroup("Info/Data")] public int Cost;

        [Title("Bonuses")]
        [Tooltip("Increases customer spawn rate (0.20 = +20%)")]
        [Range(0f, 1f)] public float CustomerSpawnRateBonus;
        [Tooltip("Increases all sale prices (0.10 = +10%)")]
        [Range(0f, 0.50f)] public float SalePriceBonus;
        [Tooltip("Increases brand reputation gain")]
        [Range(0f, 1f)] public float BrandReputationBonus;
        [Tooltip("Max number of campaigns (only highest applies)")]
        [Range(0f, 10)] public int MaxActiveCampaigns;

        [Space(20f)] public Transform[] Models;
    }

    [System.Serializable]
    public class MarketingCampaign
    {
        public MarketingCampaignType Type;
        public Sprite Icon;
        public float Duration;
        public float TimeRemaining;
        public bool IsActive;
    }

    public enum MarketingCampaignType
    {
        SocialMedia,    // +50% customer spawn for duration
        Billboard,      // +30% customer spawn for duration
        RadioAd,        // +20% customer spawn for duration
        PrestigeEvent   // +15% sale prices for duration
    }

    #endregion
}