using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuUIController : MonoBehaviour
{
    private VisualElement root;
    private Button playButton;
    private Button quitButton;
    
    private void Start()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        playButton = root.Q<Button>("play");
        quitButton = root.Q<Button>("quit");

        if (playButton != null) playButton.clicked += PlayPressed;
        if (quitButton != null) quitButton.clicked += QuitPressed;
    }

    private void OnDestroy()
    {
        if (playButton != null) playButton.clicked -= PlayPressed;
        if (quitButton != null) quitButton.clicked -= QuitPressed;
    }

    private void PlayPressed()
    {
        
    }
    
    private void QuitPressed()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }
}
