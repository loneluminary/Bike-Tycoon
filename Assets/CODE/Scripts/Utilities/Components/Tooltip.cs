using System;
using TMPro;
using UnityEngine;

public class Tooltip : MonoSingleton<Tooltip>
{
    [SerializeField] TMP_Text headerText, contentText;
    private Func<string> headerFunc = () => "Default header", contentFunc = () => "Default content";

    private RectTransform _rect;

    private void Awake()
    {
        _rect = transform as RectTransform;

        HideTooltip();
    }

    private void Update()
    {
        if (!gameObject.activeSelf) return;
        
        headerText.text = headerFunc();
        contentText.text = contentFunc();

        #if ENABLE_LEGACY_INPUT_MANAGER
        _rect.position = Input.mousePosition;
        #else
        _rect.position = UnityEngine.InputSystem.Mouse.current.position.value;
        #endif
    }
    
    public void ShowTooltip(string header, string content)
    {
        transform.SetAsLastSibling();
        
        headerFunc = () => header;
        contentFunc = () => content;
        
        Update();
        
        gameObject.SetActive(true);
    }

    public void HideTooltip()
    {
        gameObject.SetActive(false);
    }
}