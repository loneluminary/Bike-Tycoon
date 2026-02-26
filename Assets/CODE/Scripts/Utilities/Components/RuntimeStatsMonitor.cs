using UnityEngine;

namespace Utilities.Components
{
	public class CustomStatsMonitor : MonoBehaviour
	{
		private GUIStyle headerStyle;
		private GUIStyle labelStyle;
		private GUIStyle boxStyle;

		private int frameCount;
		private float deltaTime;
		private float fps;
		private float lastUpdateTime;

		private void Update()
		{
			// Update FPS calculation
			frameCount++;
			deltaTime += Time.deltaTime;

			if (Time.realtimeSinceStartup > lastUpdateTime + 0.5f)
			{
				fps = frameCount / deltaTime;
				frameCount = 0;
				deltaTime = 0;
				lastUpdateTime = Time.realtimeSinceStartup;
			}
		}

		private void OnGUI()
		{
			// Ensure styles are initialized
			if (headerStyle == null) InitializeStyles();

			GUI.Box(new Rect(10, 10, 350, 200), GUIContent.none, boxStyle);

			GUILayout.BeginArea(new Rect(15, 15, 340, 190));
			GUILayout.Label("Runtime Statistics", headerStyle);

			GUILayout.Space(10f);

			// Graphics stats
			GUILayout.Label("Graphics:", headerStyle);
			GUILayout.Label($"FPS: {fps:0.0}  ({1000.0f / fps:0.0} ms)", labelStyle);
			GUILayout.Label($"Resolution: {Screen.width}x{Screen.height} @ {Screen.currentResolution.refreshRateRatio}Hz", labelStyle);
			GUILayout.Label($"Graphics Device: {SystemInfo.graphicsDeviceName}", labelStyle);
			GUILayout.Label($"Graphics Memory: {SystemInfo.graphicsMemorySize} MB", labelStyle);

			GUILayout.Space(5);

			// Miscellaneous stats
			GUILayout.Label($"CPU: {SystemInfo.processorType}", labelStyle);
			GUILayout.Label($"RAM: {SystemInfo.systemMemorySize} MB", labelStyle);

			GUILayout.EndArea();
		}

		private void InitializeStyles()
		{
			headerStyle = new GUIStyle { fontSize = 14, fontStyle = FontStyle.Bold, normal = { textColor = Color.white } };

			labelStyle = new GUIStyle { fontSize = 12, normal = { textColor = Color.white } };

			boxStyle = new GUIStyle(GUI.skin.box) { normal = { background = MakeTex(2, 2, Color.gray, 0.25f) } };
		}

		/// Utility to create textures for GUI backgrounds
		private Texture2D MakeTex(int width, int height, Color color, float alpha = 1f)
		{
			var pix = new Color[width * height];
			for (int i = 0; i < pix.Length; i++)
				pix[i] = new Color(color.r, color.g, color.b, alpha);

			Texture2D result = new Texture2D(width, height);
			result.SetPixels(pix);
			result.Apply();
			return result;
		}
	}
}