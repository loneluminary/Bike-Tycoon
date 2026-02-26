using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RaceBikeSelectionItem : MonoBehaviour
{
    [SerializeField] Image bikeIconImage;
    [SerializeField] TextMeshProUGUI bikeNameText;
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] TextMeshProUGUI statsText;
    [SerializeField] Button selectButton;

    [HideInInspector] public BikeInstance Bike;
    private System.Action<BikeInstance> _onSelected;

    public void Initialize(BikeInstance bike, System.Action<BikeInstance> onSelected)
    {
        Bike = bike;
        _onSelected = onSelected;

        UpdateDisplay();

        if (selectButton) selectButton.onClick.AddListener(OnSelectClicked);
    }

    public void UpdateDisplay()
    {
        if (!Bike || !Bike.BikeData) return;

        if (bikeIconImage && Bike.BikeData.BikeIcon) bikeIconImage.sprite = Bike.BikeData.BikeIcon;
        if (bikeNameText) bikeNameText.text = Bike.BikeData.DetailedName;
        if (levelText) levelText.text = $"Level {Bike.CurrentLevel}";
        if (statsText)
        {
            float multiplier = Bike.BikeData.GetStatMultiplier(Bike.CurrentLevel);
            statsText.text = $"S: {Bike.BikeData.Speed * multiplier:F0}, H: {Bike.BikeData.Handling * multiplier:F0}, A: {Bike.BikeData.Acceleration * multiplier:F0}, D: {Bike.BikeData.Durability * multiplier:F0}";
        }
    }

    private void OnSelectClicked()
    {
        _onSelected?.Invoke(Bike);
    }

    public void SetSelected(bool selected)
    {
        if (selectButton) selectButton.GetComponentInChildren<TMP_Text>().text = selected ? "Selected" : "Select";
    }
}