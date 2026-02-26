using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MarketingCampaignItem : MonoBehaviour
{
    [SerializeField] private Image campaignIcon;
    [SerializeField] private TextMeshProUGUI campaignNameText;
    [SerializeField] private TextMeshProUGUI campaignDescriptionText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Button launchButton;

    private MarketingRoom.MarketingCampaignType _campaignType;
    private int _cost;
    private int _duration;
    private MarketingRoom _room;
    private System.Action _onCampaignLaunched;

    public void Initialize(MarketingRoom.MarketingCampaignType type, int cost, int duration, MarketingRoom room, System.Action onCampaignLaunched = null)
    {
        _campaignType = type;
        _cost = cost;
        _duration = duration;
        _room = room;
        _onCampaignLaunched = onCampaignLaunched;

        UpdateDisplay();

        if (launchButton) launchButton.onClick.AddListener(OnLaunchClicked);
    }

    public void UpdateDisplay()
    {
        if (campaignIcon) campaignIcon.sprite = _room.GetCampaignIcon(_campaignType);
        if (campaignNameText) campaignNameText.text = $"{MarketingRoom.GetCampaignName(_campaignType)} ({_duration}s)";
        if (campaignDescriptionText) campaignDescriptionText.text = MarketingRoom.GetCampaignDescription(_campaignType);
        if (costText) costText.text = $"${_cost:N0}";

        if (launchButton)
        {
            bool canAfford = GameManager.Instance.CurrentCash >= _cost;
            bool hasSlot = _room.ActiveCampaigns.Count < _room.MaxActiveCampaigns;
            launchButton.interactable = canAfford && hasSlot;
        }
    }

    private void OnLaunchClicked()
    {
        if (_room.TryStartCampaign(_campaignType, _duration, _cost))
        {
            transform.DOComplete();
            transform.DOPunchScale(Vector3.one * 0.2f, 0.3f);

            UpdateDisplay();
            _onCampaignLaunched?.Invoke();
        }
    }
}