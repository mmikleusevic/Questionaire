using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public class LoadingUIController : MonoBehaviour
    {
        private VisualElement loadingUI;
        
        private void Start()
        {   
            loadingUI = GetComponent<UIDocument>().rootVisualElement.Q("loadingUI");
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