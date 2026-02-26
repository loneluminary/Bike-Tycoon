using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RaceParticipantItem : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI positionText;
    [SerializeField] Slider progressBar;
    [SerializeField] GameObject playerIndicator;

    [HideInInspector] public RaceParticipant Participant;

    public void Initialize(RaceParticipant participant)
    {
        Participant = participant;
        UpdateDisplay();

        gameObject.name = participant.Name;
    }

    public void UpdateDisplay()
    {
        if (positionText) positionText.text = $"#{Participant.Position}";
        if (nameText) nameText.text = Participant.Name;

        if (playerIndicator) playerIndicator.SetActive(Participant.IsPlayer);

        // Update sibling index for visual ordering
        transform.SetSiblingIndex(Participant.Position - 1);

        // Update progress bar with normalized value (0-1)
        if (progressBar)
        {
            float maxProgress = RacingManager.Instance.CurrentRace.RaceRoute.RouteLength;
            float normalizedProgress = maxProgress > 0 ? Participant.Progress / maxProgress : 0f;
            progressBar.value = Mathf.Clamp01(normalizedProgress);
        }
    }
}