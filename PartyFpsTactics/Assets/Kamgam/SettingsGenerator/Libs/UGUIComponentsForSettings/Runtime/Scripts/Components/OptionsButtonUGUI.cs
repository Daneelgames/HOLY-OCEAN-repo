using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Kamgam.UGUIComponentsForSettings
{
    public class OptionsButtonUGUI : MonoBehaviour
    {
        public static string UndefinedText = "-";

        public TextMeshProUGUI TextTf;

        public delegate void OnValueChangedDelegate(int optionIndex);

        /// <summary>
        /// Set this to override the default option to text conversion.<br />
        /// Useful for text modifications (translations, sprite inserts, ...).
        /// </summary>
        public System.Func<string, string> OptionToTextFunc;

        /// <summary>
        /// The index of the selected option.
        /// </summary>
        public UnityEvent<int> OnValueChangedEvent;
        public OnValueChangedDelegate OnValueChanged;

        [SerializeField]
        protected List<string> _options = new List<string>();

        /// <summary>
        /// Cache for the output of GetOptions().
        /// </summary>
        protected List<string> _getOptionsCache = new List<string>();

        protected int _value;
        public int SelectedIndex
        {
            get => _value;
            set
            {
                if (value == _value)
                    return;

                _value = value % _options.Count;
                if (_value < 0)
                    _value = _options.Count + _value;
                UpdateText();

                OnValueChangedEvent?.Invoke(_value);
                OnValueChanged?.Invoke(_value);
            }
        }

        public int NumOfOptions => _options.Count;

        public void Start()
        {
            UpdateText();
        }

        public void SetOptions(IList<string> options)
        {
            _options.Clear();
            _options.AddRange(options);

            UpdateText();
        }

        public List<string> GetOptions()
        {
            _getOptionsCache.Clear();
            foreach (var option in _options)
            {
                _getOptionsCache.Add(option);
            }
            return _getOptionsCache;
        }

        public void UpdateText()
        {
            if (_options.Count == 0 || _options.Count >= _value)
                TextTf.text = UndefinedText;

            if (OptionToTextFunc == null)
                TextTf.text = _options[_value];
            else
                TextTf.text = OptionToTextFunc(_options[_value]);
        }

        public void ClearOptions()
        {
            _options.Clear();
            UpdateText();
        }

        public void Prev()
        {
            if (_options.Count == 0)
                return;

            SelectedIndex = SelectedIndex - 1;
        }

        public void Next()
        {
            if (_options.Count == 0)
                return;

            SelectedIndex = SelectedIndex + 1;
        }
    }
}
