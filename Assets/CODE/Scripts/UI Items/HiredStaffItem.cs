using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HiredStaffItem : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] Image iconImage;
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] TextMeshProUGUI assignmentText;
    [SerializeField] TextMeshProUGUI currentBonusText;
    [SerializeField] TextMeshProUGUI upgradeCostText;
    [SerializeField] Button UpgradeButton;
    [SerializeField] Button AssignButton;
    [SerializeField] Button FireButton;

    private StaffMember _staff;
    private Action<StaffMember> _onUpgrade;
    private Action<StaffMember> _onAssign;
    private Action<StaffMember> _onFire;

    public void Initialize(StaffMember staff, Action<StaffMember> onUpgrade = null, Action<StaffMember> onAssign = null, Action<StaffMember> onFire = null)
    {
        _staff = staff;
        _onUpgrade = onUpgrade;
        _onAssign = onAssign;
        _onFire = onFire;

        UpdateDisplay();

        if (UpgradeButton) UpgradeButton.onClick.AddListener(OnUpgradeClicked);
        if (AssignButton) AssignButton.onClick.AddListener(OnAssignClicked);
        if (FireButton) FireButton.onClick.AddListener(OnFireClicked);

        GameManager.Instance.OnCashChanged += UpdateDisplay;
    }

    public void UpdateDisplay()
    {
        if (!_staff) return;

        if (iconImage && _staff.StaffData.StaffIcon) iconImage.sprite = _staff.StaffData.StaffIcon;
        if (nameText) nameText.text = $"{_staff.StaffData.StaffName} ({_staff.StaffData.StaffType})";
        if (levelText) levelText.text = $"Level {_staff.CurrentLevel}";
        if (assignmentText) assignmentText.text = _staff.AssignedRoom ? _staff.AssignedRoom.RoomName : "Not Assigned";

        if (currentBonusText) currentBonusText.text = GetCurrentBonus();

        // Upgrade button
        if (UpgradeButton)
        {
            bool canUpgrade = _staff.CanUpgrade();
            UpgradeButton.interactable = canUpgrade && GameManager.Instance.CurrentCash >= _staff.GetUpgradeCost();
            if (upgradeCostText) upgradeCostText.text = canUpgrade ? $"${FormatWithK(_staff.GetUpgradeCost())}" : "MAX";
        }

        string FormatWithK(int value)
        {
            return value >= 1000 ? $"{value / 1000f:0.#}k" : value.ToString();
        }
    }

    private string GetCurrentBonus()
    {
        return _staff.StaffData.StaffType switch
        {
            StaffType.Salesperson => $"+{_staff.GetSaleSpeedBonus() * 100:F0}% Sale Speed",
            StaffType.Marketer => $"+{_staff.GetCustomerAttractionBonus() * 100:F0}% Customers",
            StaffType.Manager => $"+{_staff.GetProfitBonus() * 100:F0}% Profit",
            StaffType.Mechanic => $"+{_staff.GetMergeSpeedBonus() * 100:F0}% Merge Speed",
            _ => "No bonus"
        };
    }

    private void OnUpgradeClicked()
    {
        transform.DOComplete();
        transform.DOPunchScale(Vector3.one * 0.2f, 0.3f);

        _onUpgrade?.Invoke(_staff);
        if (_staff.TryUpgrade()) UpdateDisplay();
    }

    private void OnAssignClicked()
    {
        _onAssign?.Invoke(_staff);
    }

    private void OnFireClicked()
    {
        _onFire?.Invoke(_staff);
        StaffManager.Instance.FireStaff(_staff);
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnCashChanged -= UpdateDisplay;
    }
}