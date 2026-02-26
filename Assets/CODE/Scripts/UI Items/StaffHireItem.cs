using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StaffHireItem : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] Image iconImage;
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI typeText;
    [SerializeField] TextMeshProUGUI costText;
    [SerializeField] TextMeshProUGUI bonusDescriptionText;
    [SerializeField] Button hireButton;

    private StaffData _staffData;
    private Action<StaffData> _onHire;
    private GalleryRoom _room;

    public void Initialize(StaffData staffData, GalleryRoom room, Action<StaffData> onHire)
    {
        _staffData = staffData;
        _room = room;
        _onHire = onHire;

        UpdateDisplay();

        if (hireButton) hireButton.onClick.AddListener(OnHireClicked);

        GameManager.Instance.OnCashChanged += UpdateDisplay;
    }

    public void UpdateDisplay()
    {
        if (!_staffData) return;

        if (iconImage && _staffData.StaffIcon) iconImage.sprite = _staffData.StaffIcon;
        if (nameText) nameText.text = _staffData.StaffName;
        if (typeText) typeText.text = _staffData.StaffType.ToString();
        if (costText) costText.text = $"${_staffData.HireCost:N0}";
        if (bonusDescriptionText) bonusDescriptionText.text = GetBonusDescription();

        // Check if you can afford
        if (hireButton)
        {
            bool canAfford = GameManager.Instance.CurrentCash >= _staffData.HireCost;
            bool isMaxed = StaffManager.Instance.GetStaffInRoom(_room).Count >= StaffManager.Instance.MaxStaffPerRoom;
            hireButton.interactable = canAfford && !isMaxed;
        }
    }

    private string GetBonusDescription()
    {
        return _staffData.StaffType switch
        {
            StaffType.Salesperson => $"+{_staffData.SaleSpeedBonus * 100}% Sale Speed per level",
            StaffType.Marketer => $"+{_staffData.CustomerAttractionBonus * 100}% Customers per level",
            StaffType.Manager => $"+{_staffData.ProfitBonus * 100}% Profit per level",
            StaffType.Mechanic => $"+{_staffData.MergeSpeedBonus * 100}% Merge Speed per level",
            _ => "Unknown bonus"
        };
    }

    private void OnHireClicked()
    {
        if (StaffManager.Instance.TryHireStaff(_staffData))
        {
            // Auto-assign to the current room if available
            StaffMember newStaff = StaffManager.Instance.HiredStaff[^1];
            if (_room != null) newStaff.AssignToRoom(_room);
            _onHire?.Invoke(_staffData);

            transform.DOComplete();
            transform.DOPunchScale(Vector3.one * 0.2f, 0.3f);
        }
        else
        {
            Debug.Log("Cannot hire - insufficient funds!");
        }
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnCashChanged -= UpdateDisplay;
    }
}