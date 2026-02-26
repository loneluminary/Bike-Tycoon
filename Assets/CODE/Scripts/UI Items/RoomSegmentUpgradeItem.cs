using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomSegmentUpgradeItem : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Button upgradeButton;

    private GalleryRoom _room;
    private int _segmentIndex;
    private GalleryRoom.UpgradeSegment _segment;
    private Action _onSegmentPurchased;

    public void Initialize(GalleryRoom room, GalleryRoom.UpgradeSegment segment, int segmentIndex, Action onPurchased)
    {
        _room = room;
        _segment = segment;
        _segmentIndex = segmentIndex;
        _onSegmentPurchased = onPurchased;

        UpdateDisplay();

        if (upgradeButton) upgradeButton.onClick.AddListener(OnUpgradeClicked);

        GameManager.Instance.OnCashChanged += UpdateDisplay;
    }

    public void UpdateDisplay()
    {
        if (!_room) return;

        bool isPurchased = _room.IsSegmentPurchased(_segmentIndex);
        bool canAfford = GameManager.Instance.CurrentCash >= _segment.Cost;

        if (iconImage) iconImage.sprite = _segment.Icon;
        if (nameText) nameText.text = _segment.SegmentName;
        if (costText) costText.text = $"${_segment.Cost:N0}";
        if (statusText)
        {
            if (isPurchased) statusText.text = "Purchased";
            else if (canAfford) statusText.text = "Available";
            else statusText.text = "Need More Cash";
        }
        if (upgradeButton)
        {
            upgradeButton.interactable = !isPurchased && canAfford;
            upgradeButton.gameObject.SetActive(!isPurchased);
        }
    }

    private void OnUpgradeClicked()
    {
        if (_room.TryBuySegment(_segmentIndex))
        {
            transform.DOComplete();
            transform.DOPunchScale(Vector3.one * 0.2f, 0.3f);

            UpdateDisplay();
            _onSegmentPurchased?.Invoke();
        }
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnCashChanged -= UpdateDisplay;
    }
}