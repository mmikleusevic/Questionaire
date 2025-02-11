using UnityEngine.UIElements;

namespace UI
{
    public class ErrorModalUIController : SafeArea
    {
        public static ErrorModalUIController Instance { get; private set; }
        
        private VisualElement errorModalUI;
        private Label errorText;
        private Button okButton;
        
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
            VisualElement root = GetComponent<UIDocument>().rootVisualElement;
            errorModalUI = root.Q<VisualElement>("errorModalUI");
            errorText = root.Q<Label>("errorText");
            okButton = root.Q<Button>("okButton");
            
            Hide();

            if (okButton != null) okButton.clicked += Hide;
        }

        private void OnDestroy()
        {
            if (okButton != null) okButton.clicked -= Hide;
        }

        public void ShowMessage(string text)
        {
            errorText.text = text;
            
            Show();
        }

        private void Show()
        {
            errorModalUI.style.display = DisplayStyle.Flex;
        }

        private void Hide()
        {
            errorModalUI.style.display = DisplayStyle.None;
        }
    }
}
