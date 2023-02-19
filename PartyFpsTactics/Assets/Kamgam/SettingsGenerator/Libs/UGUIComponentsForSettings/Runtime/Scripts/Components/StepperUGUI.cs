using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Kamgam.UGUIComponentsForSettings
{
    [SelectionBase]
    public class StepperUGUI : MonoBehaviour
    {
        public delegate void OnValueChangedDelegate(float value);

        /// <summary>
        /// The index of the selected option.
        /// </summary>
        public UnityEvent<float> OnValueChangedEvent;
        public OnValueChangedDelegate OnValueChanged;

        public float MinValue = 0f;
        public float MaxValue = 100f;
        public float StepSize = 10f;
        public bool WholeNumbers = true;
        /// <summary>
        /// Number format in string. See: https://learn.microsoft.com/en-us/dotnet/standard/base-types/standard-numeric-format-strings
        /// </summary>
        public string ValueFormat = "{0:N0} %";
        [Tooltip("Should the buttons be disabled if the limits (min,max) are reached?")]
        public bool DisableButtons = true;

        public Button DecreaseButton;
        public Button IncreaseButton;

        protected AutoNavigationOverrides decreaseButtonNavigationOverrides;
        public AutoNavigationOverrides DecreaseButtonNavigationOverrides
        {
            get
            {
                if (DecreaseButton == null)
                    return null;

                if (decreaseButtonNavigationOverrides == null)
                {
                    decreaseButtonNavigationOverrides = DecreaseButton.GetComponent<AutoNavigationOverrides>();
                }
                return decreaseButtonNavigationOverrides;
            }
        }

        protected AutoNavigationOverrides increaseButtonNavigationOverrides;
        public AutoNavigationOverrides IncreaseButtonNavigationOverrides
        {
            get
            {
                if (IncreaseButton == null)
                    return null;

                if (increaseButtonNavigationOverrides == null)
                {
                    increaseButtonNavigationOverrides = IncreaseButton.GetComponent<AutoNavigationOverrides>();
                }
                return increaseButtonNavigationOverrides;
            }
        }

        protected float _value;
        public float Value
        {
            get => WholeNumbers ? Mathf.Round(_value) : _value;
            set
            {
                if (Mathf.Abs(_value - value) <= Mathf.Epsilon)
                    return;

                updateValue(value);
                updateButtons();
            }
        }

        public int IntValue
        {
            get => Mathf.RoundToInt(_value);
        }

        public TextMeshProUGUI TextTf;
        public TextMeshProUGUI ValueTf;

        public string Text
        {
            get => TextTf.text;
            set
            {
                if (value == Text)
                    return;

                updateText(value);
                updateButtons();
            }
        }

        protected void updateValue(float value)
        {
            float newValue = WholeNumbers ? Mathf.Round(value) : value;
            newValue = ConvertToStepValue(newValue);

            _value = Mathf.Clamp(newValue, MinValue, MaxValue);
            ValueTf.text = string.Format(ValueFormat, _value);
        }

        protected void updateText(string text)
        {
            TextTf.text = text;
        }

        public void OnEnable()
        {
            updateText(Text);
            updateValue(Value);
            updateButtons();
        }

        public float ConvertToStepValue(float value)
        {
            // set the new value to the closest stepped value;
            float minDelta = float.MaxValue;
            float minDeltaValue = value;
            float refValue = MinValue;
            int steps = Mathf.CeilToInt((MaxValue - MinValue) / StepSize) + 1;
            for (int i = 0; i < steps; i++)
            {
                float delta = Mathf.Abs(value - refValue);
                if (delta < minDelta)
                {
                    minDelta = delta;
                    minDeltaValue = refValue;
                }
                refValue += StepSize;
            }

            if (WholeNumbers)
                minDeltaValue = Mathf.Round(minDeltaValue);

            return minDeltaValue;
        }

        public void Increase()
        {
            Step(1);
        }

        public void Decrease()
        {
            Step(-1);
        }

        public void Step(int steps)
        {
            Value = _value + steps * StepSize;
            
            if(steps != 0)
            {
                OnValueChangedEvent?.Invoke(Value);
                OnValueChanged?.Invoke(Value);
            }
        }

        protected void updateButtons()
        {
            if (!DisableButtons)
            {
                DecreaseButton.enabled = true;
                IncreaseButton.enabled = true;
                return;
            }

            bool enableIncrease = Mathf.Abs(_value - MaxValue) > float.Epsilon;
            IncreaseButton.interactable = enableIncrease;

            bool enableDecrease = Mathf.Abs(_value - MinValue) > float.Epsilon;
            DecreaseButton.interactable = enableDecrease;
        }
    }
}
