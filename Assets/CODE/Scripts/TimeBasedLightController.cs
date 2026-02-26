using UnityEngine;

[RequireComponent(typeof(Light))]
public class TimeBasedLightController : MonoBehaviour
{
    [Tooltip("Light will be enabled between these times (inclusive).")]
    [Range(0, 24)][SerializeField] float enableTime = 18f, disableTime = 6f;

    [Tooltip("If set, light intensity will follow this curve between enable and disable times.")]
    [SerializeField] AnimationCurve intensityCurve;

    private Light _light;

    private void Awake()
    {
        _light = GetComponent<Light>();
    }

    private void Update()
    {
        float time = DayNightCycleManager.Instance.TimeOfDay;

        bool shouldEnable = IsWithinActiveTime(time);

        if (_light.enabled != shouldEnable) _light.enabled = shouldEnable;

        if (_light.enabled && intensityCurve != null && intensityCurve.length > 1)
        {
            float t = GetNormalizedTime(time);
            _light.intensity = intensityCurve.Evaluate(t);
        }
        else if (!_light.enabled)
        {
            _light.intensity = 0f;
        }
    }

    // Returns true if current time is within the active range
    private bool IsWithinActiveTime(float time)
    {
        if (enableTime < disableTime)
        {
            // e.g. enable at 6, disable at 18 (day)
            return time >= enableTime && time < disableTime;
        }
        else
        {
            // e.g. enable at 18, disable at 6 (night, crosses midnight)
            return time >= enableTime || time < disableTime;
        }
    }

    // Returns normalized time (0-1) between enable and disable
    private float GetNormalizedTime(float time)
    {
        float start = enableTime;
        float end = disableTime;

        if (start < end)
        {
            // e.g. 6 to 18
            return Mathf.InverseLerp(start, end, time);
        }
        else
        {
            // e.g. 18 to 6 (crosses midnight)
            float duration = 24f - start + end;
            float t;
            if (time >= start) t = time - start;
            else t = 24f - start + time;
            return Mathf.Clamp01(t / duration);
        }
    }
}