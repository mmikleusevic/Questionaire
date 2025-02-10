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
        
        private void ApplySafeArea(VisualElement rootElement)
        {
            Rect safeArea = Screen.safeArea;
            
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;
            
            float leftMargin = safeArea.x / screenWidth;
            float bottomMargin = safeArea.y / screenHeight;
            float rightMargin = (screenWidth - (safeArea.x + safeArea.width)) / screenWidth;
            float topMargin = (screenHeight - (safeArea.y + safeArea.height)) / screenHeight;
            
            rootElement.style.marginLeft = Length.Percent(leftMargin * 100);
            rootElement.style.marginRight = Length.Percent(rightMargin * 100);
            rootElement.style.marginTop = Length.Percent(topMargin * 100);
            rootElement.style.marginBottom = Length.Percent(bottomMargin * 100);
        }

        private void PlayPressed()
        {
            playUIController.LoadCategories();
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
