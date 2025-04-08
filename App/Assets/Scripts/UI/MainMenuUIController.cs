using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public class MainMenuUIController : SafeArea
    {
        [SerializeField] private PlayUIController playUIController;

        private Button playButton;
        private Button quitButton;

        private void Start()
        {
            VisualElement root = GetComponent<UIDocument>().rootVisualElement;
            playButton = root.Q<Button>("playButton");
            quitButton = root.Q<Button>("quitButton");

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
            playUIController.Show();
        }

        private void QuitPressed()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#endif
            Application.Quit();
        }
    }
}