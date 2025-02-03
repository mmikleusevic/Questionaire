using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public class LoadingUIController : SafeArea
    {
        private VisualElement loadingUI;
        
        private void Start()
        {   
            loadingUI = GetComponent<UIDocument>().rootVisualElement.Q("loadingUI");
            
            Hide();
        }

        public void Show()
        {
            loadingUI.style.display = DisplayStyle.Flex;
        }

        public void Hide()
        {
            loadingUI.style.display = DisplayStyle.None;
        }
    }
}