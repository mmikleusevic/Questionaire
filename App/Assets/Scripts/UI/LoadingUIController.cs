using UnityEngine.UIElements;

namespace UI
{
    public class LoadingUIController : SafeArea
    {
        private Label loadingText;

        private VisualElement loadingUI;
        public static LoadingUIController Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

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