using UnityEngine;
using UnityEngine.UIElements;

public class SafeArea : MonoBehaviour
{
    private void OnEnable()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        
        ApplySafeArea(root);
    }
    
    private void ApplySafeArea(VisualElement root)
    {
        if (root == null) return;

        Rect safeArea = Screen.safeArea;
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        float leftMargin = safeArea.x / screenWidth;
        float bottomMargin = safeArea.y / screenHeight;
        float rightMargin = (screenWidth - (safeArea.x + safeArea.width)) / screenWidth;
        float topMargin = (screenHeight - (safeArea.y + safeArea.height)) / screenHeight;

        root.style.marginLeft = Length.Percent(leftMargin * 100);
        root.style.marginRight = Length.Percent(rightMargin * 100);
        root.style.marginTop = Length.Percent(topMargin * 100);
        root.style.marginBottom = Length.Percent(bottomMargin * 100);
    }
}