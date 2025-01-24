using System;
using System.Collections.Generic;
using DefaultNamespace.Models;
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
    private List<Question> questions;
    
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

    private void Show()
    {
        gameUI.style.display = DisplayStyle.Flex;
    }
    
    private void Hide()
    {
        gameUI.style.display = DisplayStyle.None;
    }

    public void ShowDirect(List<Question> questions)
    {
        this.questions = questions;
        
        Show();
        
        ShowAnswers(false, true, false);
    }

    public void ShowOptions(List<Question> questions)
    {
        this.questions = questions;
        
        Show();

        ShowAnswers(true, true, true);
    }

    private void ShowAnswers(bool showFirst, bool showSecond, bool showThird)
    {
        answer1Text.visible = showFirst;
        answer2Text.visible = showSecond;
        answer3Text.visible = showThird;
    }
}
