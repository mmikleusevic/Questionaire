using System;
using UnityEngine;
using UnityEngine.UIElements;

public class ErrorModalUIController : MonoBehaviour
{
    private VisualElement errorModalUI;
    private Label errorText;
    private Button okButton;

    private void Start()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        errorModalUI = root.Q<VisualElement>("errorModalUI");
        errorText = root.Q<Label>("errorText");
        okButton = root.Q<Button>("okButton");

        if (okButton != null) okButton.clicked += Hide;
    }

    private void OnDestroy()
    {
        if (okButton != null) okButton.clicked -= Hide;
    }

    public void Show(string errorText)
    {
        errorModalUI.style.display = DisplayStyle.Flex;
        this.errorText.text = errorText;
    }

    private void Hide()
    {
        errorModalUI.style.display = DisplayStyle.None;
    }
}
