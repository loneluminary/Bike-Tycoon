using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine;

public class TaskItemUI : MonoBehaviour
{
    [SerializeField] Image TaskIconImage;
    [SerializeField] TextMeshProUGUI TaskTitleText;
    [SerializeField] TextMeshProUGUI TaskDescriptionText;
    [SerializeField] TextMeshProUGUI ProgressText;
    [SerializeField] Slider ProgressSlider;
    [SerializeField] TextMeshProUGUI RewardText;

    public TaskData TaskData { get; private set; }

    public void Initialize(TaskData taskData)
    {
        TaskData = taskData;

        if (TaskIconImage && taskData.so.TaskIcon) TaskIconImage.sprite = taskData.so.TaskIcon;
        if (TaskTitleText) TaskTitleText.text = taskData.so.TaskTitle;
        if (TaskDescriptionText) TaskDescriptionText.text = taskData.so.TaskDescription;
        if (RewardText) RewardText.text = $"+${taskData.so.CashReward:N0}";

        UpdateProgress();

        // Entrance animation
        transform.DOPunchScale(Vector3.one * 0.1f, 0.3f);
    }

    public void UpdateProgress()
    {
        if (TaskData == null) return;

        int current = TaskData.CurrentProgress;
        int required = TaskData.GetRequiredAmount();
        float progress = TaskData.GetProgress();

        if (ProgressText) ProgressText.text = $"{current} / {required}";
        if (ProgressSlider) ProgressSlider.DOValue(progress, 0.3f);
    }

    public void ShowCompleted()
    {
        if (TaskIconImage && TaskData.so.TaskCompleteIcon) TaskIconImage.sprite = TaskData.so.TaskCompleteIcon;

        // Celebration animation
        transform.DOComplete();
        transform.DOPunchScale(Vector3.one * 0.2f, 0.5f);

        UIManager.Instance.CashAddingAnimation(transform.position);
    }
}