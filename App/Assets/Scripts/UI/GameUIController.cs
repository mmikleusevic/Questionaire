using System;
using UnityEngine;
using UnityEngine.UIElements;

public class GameUIController : MonoBehaviour
{
    private VisualElement gameUI;
    private Label questionText;
    private Label answer1Text;
    private Label answer2Text;
    private Label answer3Text;
    private Button exitButton;
    private Button backButton;
    private Button forwardButton;
    
    private void Start()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        gameUI = root.Q<VisualElement>("gameUI");
        questionText = root.Q<Label>("questionText");
        answer1Text = root.Q<Label>("answer1Text");
        answer2Text = root.Q<Label>("answer2Text");
        answer3Text = root.Q<Label>("answer3Text");
        exitButton = root.Q<Button>("exitButton");
        backButton = root.Q<Button>("backButton");
        forwardButton = root.Q<Button>("forwardButton");
        
        if (exitButton != null) exitButton.clicked += ExitPressed;
    }

    private void OnDestroy()
    {
        if (exitButton != null) exitButton.clicked -= ExitPressed;
    }

    private void ExitPressed()
    {
        Hide();
    }

    public void Show()
    {
        gameUI.visible = true;
    }
    
    private void Hide()
    {
        gameUI.visible = false;
    }
}
