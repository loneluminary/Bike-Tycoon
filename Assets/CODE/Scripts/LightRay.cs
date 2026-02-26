using DG.Tweening;
using UnityEngine;
using Utilities.Extensions;

[RequireComponent(typeof(Renderer))]
public class LightRay : MonoBehaviour
{
    [SerializeField] Color[] colors;
    [SerializeField] float fadeDuration = 0.2f; // How long the transition takes
    [SerializeField] float changeInterval = 1.0f; // Time to wait between changes
    [SerializeField] bool loop = true;

    private Material _material;
    private int _currentIndex;

    private void OnEnable()
    {
        // Cache the material to prevent memory leaks from .material calls
        if (TryGetComponent(out Renderer renderer)) _material = renderer.material;
        if (colors.IsNullOrEmpty() || _material == null) return;

        _currentIndex = -1;

        CycleColor();
    }

    public void CycleColor()
    {
        _currentIndex++;
        if (_currentIndex >= colors.Length) _currentIndex = loop ? 0 : colors.Length - 1;

        _material.DOColor(colors[_currentIndex], "_BaseColor", fadeDuration).SetDelay(changeInterval).OnComplete(() =>
        {
            if (loop) CycleColor(); // Recursively call to start the next random color
        });
    }

    private void OnDestroy() => _material.DOKill();
    private void OnDisable() => _material.DOKill();
}