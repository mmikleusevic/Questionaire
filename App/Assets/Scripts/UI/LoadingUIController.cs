using UnityEngine.UIElements;

namespace UI
{
    public class LoadingUIController : SafeArea
    {
        private VisualElement loadingUI;
        private Label loadingText;
        
        private void Start()
        {   
            loadingUI = GetComponent<UIDocument>().rootVisualElement.Q("loadingUI");
            loadingText = loadingUI.Q<Label>("loadingText");
            
            Hide();
        }

        public void ShowLoadingMessage(string message)
        {
            loadingText.text = message;
            
            Show();
        }

        private void Show()
        {
            loadingUI.style.display = DisplayStyle.Flex;
        }

        public void Hide()
        {
            loadingUI.style.display = DisplayStyle.None;
        }
    }
}