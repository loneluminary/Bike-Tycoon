using UnityEngine;
using TMPro;
using System.Collections;
using Sirenix.OdinInspector;

public class DayNightCycleManager : MonoSingleton<DayNightCycleManager>
{
	[Title("Time Settings")]
	public float startTimeOfDay = 12f;
	public float cycleSpeed = 0.1f;
	public float TimeOfDay => _timeOfDay;
	private float _timeOfDay;
	private bool _isTimeRunning = true;

	[Title("Cycles Settings")]
	public TimeSettings daybreak;
	public TimeSettings midday;
	public TimeSettings sunset;
	public TimeSettings night;

	[Title("Directional Light Settings")]
	public Light sunLight;
	public Transform rotationPivot; // Parent transform for offset control
	[Range(0, 259)] public float rotationOffsetY = 0f; // Rotation offset along Y-axis
	public Camera sceneCamera;

	[Title("UI Settings")]
	public TextMeshProUGUI timeText;

	[Title("Fog Control")]
	public bool enableFogControl = true; // If false, user controls fog manually.

	[Title("Reset Settings")]
	public float resetSpeed = 1f;
	public bool forwardsOnly = false; // If true, time can only reset forward
	private Coroutine _timeResetCoroutine;

	private static readonly System.Text.StringBuilder _timeStringBuilder = new System.Text.StringBuilder();
	private float _lastTimeUpdated = -1f;

	void Start()
	{
		_timeOfDay = startTimeOfDay;
		UpdateTimeUI();  // Initialize the UI with the correct time
		UpdateLighting(); // Ensure lighting is correct from the start
	}

	void Update()
	{
		if (_isTimeRunning)
		{
			UpdateTime();
			UpdateLighting();
		}
	}

	private void UpdateTime()
	{
		_timeOfDay += cycleSpeed * Time.deltaTime;
		if (_timeOfDay >= 24f) _timeOfDay = 0f;

		int currentMinute = Mathf.FloorToInt((_timeOfDay - Mathf.FloorToInt(_timeOfDay)) * 60); // Get exact minute

		// Only update UI when the minute changes
		if (currentMinute != _lastTimeUpdated)
		{
			_lastTimeUpdated = currentMinute;
			UpdateTimeUI();
		}
	}

	private void UpdateTimeUI()
	{
		if (timeText == null) return;

		int hours = Mathf.FloorToInt(_timeOfDay);
		int minutes = Mathf.FloorToInt((_timeOfDay - hours) * 60);

		_timeStringBuilder.Clear();
		_timeStringBuilder.Append("Time: ");
		_timeStringBuilder.Append(hours.ToString("00"));
		_timeStringBuilder.Append(":");
		_timeStringBuilder.Append(minutes.ToString("00"));

		timeText.text = _timeStringBuilder.ToString();
	}

	private void UpdateLighting()
	{
		if (sceneCamera == null || sunLight == null) return;

		float timePercent = _timeOfDay / 24f;
		float xRotation = (timePercent * 360f) - 90f;

		if (rotationPivot.localRotation.eulerAngles.x != xRotation || rotationPivot.localRotation.eulerAngles.y != rotationOffsetY)
		{
			rotationPivot.localRotation = Quaternion.Euler(new Vector3(xRotation, rotationOffsetY, 0));
		}

		TimeSettings from, to;
		float blend;

		if (_timeOfDay < 6f) { from = night; to = daybreak; blend = _timeOfDay / 6f; }
		else if (_timeOfDay < 12f) { from = daybreak; to = midday; blend = (_timeOfDay - 6f) / 6f; }
		else if (_timeOfDay < 18f) { from = midday; to = sunset; blend = (_timeOfDay - 12f) / 6f; }
		else { from = sunset; to = night; blend = (_timeOfDay - 18f) / 6f; }

		RenderSettings.ambientLight = Color.Lerp(from.ambientColor, to.ambientColor, blend);
		sunLight.color = Color.Lerp(from.sunColor, to.sunColor, blend);
		sunLight.intensity = Mathf.Lerp(from.sunIntensity, to.sunIntensity, blend);
		sunLight.shadowStrength = Mathf.Lerp(from.shadowStrength, to.shadowStrength, blend);
		sceneCamera.backgroundColor = Color.Lerp(from.backgroundColor, to.backgroundColor, blend);

		if (enableFogControl && RenderSettings.fog)
		{
			RenderSettings.fogColor = Color.Lerp(from.fogColor, to.fogColor, blend);
			RenderSettings.fogDensity = Mathf.Lerp(from.fogDensity, to.fogDensity, blend);
		}
	}

	public void StopTime() => _isTimeRunning = false;
	public void StartTime() => _isTimeRunning = true;
	public void ResetTimeSmoothly(float targetTime) { if (_timeResetCoroutine != null) StopCoroutine(_timeResetCoroutine); _timeResetCoroutine = StartCoroutine(SmoothTimeReset(targetTime)); }

	private IEnumerator SmoothTimeReset(float targetTime)
	{
		float originalTime = _timeOfDay;
		float elapsedTime = 0f;

		// Adjust for forward-only mode
		if (forwardsOnly && originalTime > targetTime)
		{
			targetTime += 24f; // Ensure forward movement
		}

		// Ensure blending works across midnight (e.g., 12 PM → 12 AM)
		bool crossesMidnight = (originalTime > targetTime) && (Mathf.Abs(originalTime - targetTime) > 12f);
		float adjustedTargetTime = crossesMidnight ? targetTime + 24f : targetTime;

		while (elapsedTime < 1f)
		{
			elapsedTime += Time.deltaTime * resetSpeed;

			// Interpolate time smoothly, handling midnight transitions properly
			_timeOfDay = Mathf.Lerp(originalTime, adjustedTargetTime, elapsedTime) % 24f;

			UpdateLighting(); // Ensure smooth color transitions
			yield return null;
		}

		// Ensure final time is exactly `_startTimeOfDay`
		_timeOfDay = targetTime % 24f;
		UpdateLighting();
		UpdateTimeUI();
	}

	[Button("Set to Daybreak", Icon = SdfIconType.Sunrise)]
	public void SetToDaybreak() => SetTimeInstantly(6f);
	[Button("Set to Midday", Icon = SdfIconType.Sun)]
	public void SetToMidday() => SetTimeInstantly(12f);
	[Button("Set to Sunset", Icon = SdfIconType.Sunset)]
	public void SetToSunset() => SetTimeInstantly(18f);
	[Button("Set to Night", Icon = SdfIconType.Moon)]
	public void SetToNight() => SetTimeInstantly(0f);

	// Instantly sets time without blending
	public void SetTimeInstantly(float targetTime)
	{
		_timeOfDay = targetTime % 24f; // Ensure it's within valid range
		UpdateLighting();  // Apply the correct lighting immediately
		UpdateTimeUI();    // Update the UI instantly
	}

	[System.Serializable]
	public struct TimeSettings
	{
		[LabelText("Scene Ambient")] public Color ambientColor;
		[LabelText("Sun Color")] public Color sunColor;
		[LabelText("Camera Background")] public Color backgroundColor;
		[LabelText("Sun Intensity")] public float sunIntensity;
		[LabelText("Shadow Strength"), Range(0f, 1f)] public float shadowStrength;

		[Title("Fog Settings")]
		[LabelText("Fog Color")] public Color fogColor;
		[LabelText("Fog Density")] public float fogDensity;
	}
}