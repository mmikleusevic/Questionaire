using UnityEngine;
using UnityEngine.UIElements;

namespace UI.CustomUIElements
{
    [UxmlElement]
    public partial class SlideToggle : BaseField<bool>
    {
        private static readonly string containerUssClassName = "slideToggleContainer";
        private new static readonly string ussClassName = "slideToggle";
        private new static readonly string inputUssClassName = "slideToggleInput";
        private static readonly string inputKnobUssClassName = "slideToggleInputKnob";
        private static readonly string inputCheckedUssClassName = "slideToggleInputChecked";
        private static readonly string inputLabelUssClassName = "slideToggleInputLabel";

        private VisualElement container;
        private VisualElement input;
        private VisualElement knob;
        private Label textLabel;
        
        public SlideToggle() : this(null) { }

        private SlideToggle(string label) : base(label, null)
        {
            pickingMode = PickingMode.Ignore;
            base.focusable = false;
            
            AddToClassList(ussClassName);

            container = new VisualElement();
            container.name = "container";
            container.AddToClassList(containerUssClassName);
            Add(container);
            
            input = this.Q(className: BaseField<bool>.inputUssClassName);
            input.name = "slideToggleInput";
            input.AddToClassList(inputUssClassName);
            container.Add(input);
            
            knob = new VisualElement();
            knob.AddToClassList(inputKnobUssClassName);
            knob.name = "knob";
            input.Add(knob);
            
            textLabel = new Label("Text");
            textLabel.AddToClassList(inputLabelUssClassName);
            textLabel.name = "textLabel";
            container.Add(textLabel);
            
            container.RegisterCallback<ClickEvent>(OnClick);
            RegisterCallback<KeyDownEvent>(OnKeydownEvent);
            RegisterCallback<NavigationSubmitEvent>(OnSubmit);
        }

        private void OnClick(ClickEvent evt)
        {
            ToggleValue();
            evt.StopPropagation();
        }

        private static void OnSubmit(NavigationSubmitEvent evt)
        {
            var slideToggle = evt.currentTarget as SlideToggle;
            slideToggle.ToggleValue();

            evt.StopPropagation();
        }

        private static void OnKeydownEvent(KeyDownEvent evt)
        {
            var slideToggle = evt.currentTarget as SlideToggle;
            
            if (slideToggle.panel?.contextType == ContextType.Player)
                return;
            
            if (evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.Space)
            {
                slideToggle.ToggleValue();
                evt.StopPropagation();
            }
        }
        
        private void ToggleValue()
        {
            value = !value;
        }

        public override void SetValueWithoutNotify(bool newValue)
        {
            base.SetValueWithoutNotify(newValue);
            input.EnableInClassList(inputCheckedUssClassName, newValue);
        }
        
        protected override void HandleEventBubbleUp(EventBase evt)
        {
            base.HandleEventBubbleUp(evt);
            
            if (evt is ChangeEvent<bool> changeEvent)
            {
                input.EnableInClassList(inputCheckedUssClassName, changeEvent.newValue);
            }
            
            evt.StopPropagation();
        }
    }
}