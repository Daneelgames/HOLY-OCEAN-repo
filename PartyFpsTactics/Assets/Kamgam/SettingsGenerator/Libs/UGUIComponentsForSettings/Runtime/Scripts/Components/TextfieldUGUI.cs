using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Kamgam.UGUIComponentsForSettings
{
    public class TextfieldUGUI : MonoBehaviour
    {
        public TMP_InputField InputTf;
        
        public delegate void OnTextChangedDelegate(string text);

        /// <summary>
        /// The index of the selected option.
        /// </summary>
        public UnityEvent<string> OnTextChangedEvent;
        public OnTextChangedDelegate OnTextChanged;

        public string Text
        {
            get => InputTf.text;
            set
            {
                if (value == InputTf.text)
                    return;

                InputTf.text = value;

                OnTextChangedEvent?.Invoke(InputTf.text);
                OnTextChanged?.Invoke(InputTf.text);
            }
        }

        public void Start()
        {
            InputTf.onValueChanged.AddListener(onTextChanged);
        }

        private void onTextChanged(string text)
        {
            OnTextChanged?.Invoke(text);
            OnTextChangedEvent?.Invoke(text);
        }
    }
}
