using System.Collections.Generic;
using DG.Tweening;
using Lean.Pool;
using TMPro;
using UnityEngine;
using Utilities.Extensions;

public class ChatBubble : MonoBehaviour
{
	[SerializeField] SpriteRenderer _background;
	[SerializeField] TextMeshPro _text;

	private bool _faceCamera;
	private static Camera _camera;

	private static readonly Dictionary<ulong, ChatBubble> SpawnedBubbles = new();

	private void Awake()
	{
		if (!_camera) _camera = Camera.main;
	}

	private void Update()
	{
		if (_faceCamera && _camera) transform.DOLookAt(transform.position - (_camera.transform.position - transform.position), 0.3f);
	}

	public static ChatBubble Create(string text, string emoji, Vector3 position, Transform parent = null, bool alwaysFaceCamera = true, float lifeTime = -1f, ulong id = 0)
	{
		DespawnById(id);

		#if UNITY_EDITOR
		if (UnityEditor.EditorSettings.spritePackerMode == UnityEditor.SpritePackerMode.SpriteAtlasV2Build) emoji = string.Empty;
		#endif

		ChatBubble chatBubble = LeanPool.Spawn(UIManager.Instance.ChatBubble, position, Quaternion.identity, parent);
		chatBubble.Setup(emoji, text);

		chatBubble._faceCamera = alwaysFaceCamera;

		if (lifeTime != -1f) LeanPool.Despawn(chatBubble.gameObject, lifeTime);

		// Add this new bubble into the dictionary if it has a valid ID
		if (id != 0) SpawnedBubbles[id] = chatBubble;

		return chatBubble;
	}

	/// Check if we should despawn any existing bubbles with the same ID
	public static void DespawnById(ulong id)
	{
		if (id != 0 && SpawnedBubbles.TryGetValue(id, out var existingBubble))
		{
			if (existingBubble) LeanPool.Despawn(existingBubble.gameObject); // Despawn old bubble
			SpawnedBubbles.Remove(id); // Remove it from the dictionary
		}
	}

	private void Setup(string emoji, string text)
	{
		if (!emoji.IsNullOrEmpty()) text = $"<sprite name={emoji}>" + " " + text;

		_text.alpha = 0;
		// Temporarily set the full text but make it invisible for size calculations.
		_text.text = text;
		_text.ForceMeshUpdate();

		Vector2 textSize = _text.GetRenderedValues(false);
		Vector2 padding = new Vector2(7f, 3f);

		_background.size = textSize + padding;

		Vector2 center = new(_background.size.x / 2f, 0f);
		_background.transform.localPosition = center;
		_text.transform.localPosition = center;

		// Reset the text to empty and make it visible again
		_text.text = "";
		_text.alpha = 1;

		// Animate the text appearing character by character
		_text.DOText(text, 1.5f).SetEase(Ease.Linear);
	}

	private void OnDestroy()
	{
		if (SpawnedBubbles.ContainsValue(this))
		{
			SpawnedBubbles.Remove(SpawnedBubbles.GetKeyByValue(this));
		}
	}
}