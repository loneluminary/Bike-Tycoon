using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ActiveCampaignItem : MonoBehaviour
{
    [SerializeField] private Image campaignIcon;
    [SerializeField] private TextMeshProUGUI campaignNameText;
    [SerializeField] private TextMeshProUGUI campaignDescriptionText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Slider timerSlider;
    [SerializeField] private Image fillImage;

    private MarketingRoom.MarketingCampaign _campaign;
    private float _originalDuration;

    public void Initialize(MarketingRoom.MarketingCampaign campaign)
    {
        _campaign = campaign;
        _originalDuration = campaign.Duration;

        if (campaignIcon) campaignIcon.sprite = _campaign.Icon;
        if (campaignNameText) campaignNameText.text = MarketingRoom.GetCampaignName(campaign.Type);
        if (campaignDescriptionText) campaignDescriptionText.text = MarketingRoom.GetCampaignDescription(campaign.Type);

        if (fillImage)
        {
            fillImage.color = GetCampaignColor();
        }
    }

    private void Update()
    {
        if (_campaign == null) return;

        UpdateTimer();
    }

    private void UpdateTimer()
    {
        if (_campaign.TimeRemaining <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        if (timerText)
        {
            int minutes = Mathf.FloorToInt(_campaign.TimeRemaining / 60f);
            int seconds = Mathf.FloorToInt(_campaign.TimeRemaining % 60f);
            timerText.text = $"{minutes:00}:{seconds:00}";
        }

        if (timerSlider)
        {
            timerSlider.value = _campaign.TimeRemaining / _originalDuration;
        }
    }

    private Color GetCampaignColor()
    {
        return _campaign.Type switch
        {
            MarketingRoom.MarketingCampaignType.SocialMedia => new Color(0.2f, 0.6f, 1f),
            MarketingRoom.MarketingCampaignType.Billboard => new Color(1f, 0.8f, 0.2f),
            MarketingRoom.MarketingCampaignType.RadioAd => new Color(0.8f, 0.3f, 1f),
            MarketingRoom.MarketingCampaignType.PrestigeEvent => new Color(1f, 0.6f, 0.2f),
            _ => Color.white
        };
    }
}