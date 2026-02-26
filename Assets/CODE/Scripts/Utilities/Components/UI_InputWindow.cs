using System;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Utilities.Extensions;

public class UI_InputWindow : MonoSingleton<UI_InputWindow>
{
    [SerializeField] TextMeshProUGUI titleText;
    [SerializeField] TMP_InputField inputField;
    [SerializeField] Button okBtn, cancelBtn;

    [ReadOnly] public bool IsOpen;

    private Action _onCancel;
    private Action _onOk;

    private void Update()
    {
        if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame)
        {
            Hide();
            _onOk?.Invoke();
        }

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Hide();
            _onCancel?.Invoke();
        }
    }

    public void Show(string titleString, Action<string> onOk, Action onCancel = null, string defaultInput = "", string validCharacters = "", int characterLimit = 20)
    {
        IsOpen = true;
        gameObject.SetActive(true);
        transform.SetAsLastSibling();

        titleText.text = titleString;

        inputField.characterLimit = characterLimit;
        inputField.onValidateInput = (_, _, addedChar) => ValidateChar(validCharacters, addedChar);

        inputField.text = defaultInput;
        inputField.Select();

        _onCancel = () => { Hide(); onCancel?.Invoke(); };
        _onOk = () => { Hide(); onOk?.Invoke(inputField.text); };
        
        okBtn.onClick.RemoveAllListeners();
        okBtn.onClick.AddListener(_onOk.Invoke);
        
        cancelBtn.onClick.RemoveAllListeners();
        cancelBtn.onClick.AddListener(_onCancel.Invoke);
    }

    private void Hide()
    {
        IsOpen = false;
        gameObject.SetActive(false);
    }

    private char ValidateChar(string validCharacters, char addedChar)
    {
        if (validCharacters.IsNullOrEmpty()) return addedChar;
        return validCharacters.Contains(addedChar) ? addedChar : '\0';
    }
}