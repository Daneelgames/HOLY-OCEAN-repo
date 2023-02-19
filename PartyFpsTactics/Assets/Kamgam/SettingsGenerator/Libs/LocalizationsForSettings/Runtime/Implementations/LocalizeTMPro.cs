using System;
using TMPro;
using UnityEngine;

namespace Kamgam.LocalizationForSettings
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class LocalizeTMPro : MonoBehaviour
    {
        public LocalizationProvider LocalizationProvider;
        public TextMeshProUGUI Textfield;
        public string Term;

        /// <summary>
        /// Translates the given term and sets the text of the TMPro Textfield.<br /><br />
        /// A string.Format(format) string can be specified. The translated text will
        /// always be appended as an additional LAST parameter to the parameters list.<br /><br />
        /// Example Format: "{0} %"
        /// </summary>
        [Tooltip("Translates the given term and sets the text of the TMPro Textfield.\n\n"+
            "A string.Format(format) string can be specified. The translated text will "+
            "always be appended as an additional LAST parameter to the parameters list.\n\n" +
            "Example Format: {0} %")]
        public string Format;

        /// <summary>
        /// EDITOR Setting: Update the term with the text from the textfield if the text
        /// in the textfield is a valid localization term.
        /// </summary>
        public bool UpdateTermFromText = true;

        // Remember format string and parameters to be able to re-localized on language change.
        protected object[] _lastUsedParameters;
        protected string _lastUsedFormat;

        public void Awake()
        {
            Textfield = this.GetComponent<TextMeshProUGUI>();

            if (LocalizationProvider != null && LocalizationProvider.HasLocalization())
            {
                var loc = LocalizationProvider.GetLocalization();
                if (loc != null)
                    loc.AddOnLanguageChangedListener(onLanguageChanged);
            }
        }

        private void onLanguageChanged(string language)
        {
            Localize();
        }

        public void OnEnable()
        {
            if (LocalizationProvider == null || !LocalizationProvider.HasLocalization())
                return;

            Clear();
            if (Term == null) {
                Term = Textfield.text;
            }
            Localize(Term);
        }

        /// <summary>
        /// Clears the last known format and parameters.</param>
        /// </summary>
        public void Clear()
        {
            _lastUsedFormat = null;
            _lastUsedParameters = null;
        }

        /// <summary>
        /// Updates the textfield with the current translation.
        /// </summary>
        public void Localize()
        {
            this.Localize(Term, _lastUsedFormat, _lastUsedParameters);
        }

        /// <summary>
        /// Translates the given term and sets the text of the TMPro Textfield.<br />
        /// A string.Format(format) string can be specified. The translated text will
        /// always be appended as an additional LAST parameter to the parameters list.
        /// <example>Example (where the term "cost" is localized as "Price"):
        /// <code>
        /// Localize("cost", "{0}: {1:C2}", 15.99f);
        ///            \_______^    ^_______/
        /// // Result: "Price: $15.99"
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="term">The term which to translate. If null then the current 'Term' of this object is used.</param>
        /// <param name="format">Custom format with the translation as the first parameter.</param>
        /// <param name="parameters">The parameters for the formatted string. Keep in mind that the translation is prepended as the FIRST parameter in the list (index 0).</param>
        public void Localize(string term, string format = null, params object[] parameters)
        {
            // Fall back on the 'Term' field if no term is specified. 
            if (string.IsNullOrEmpty(term))
                term = Term;

            if (string.IsNullOrEmpty(term))
                return;

            var loc = LocalizationProvider.GetLocalization();
            if (loc != null)
            {
                string translation = loc.Get(term);

                if (format == null)
                    format = Format;
                if (!string.IsNullOrEmpty(format))
                {
                    if (parameters == null || parameters.Length == 0)
                    {
                        translation = string.Format(format, translation);
                    }
                    else
                    {
                        object[] finalParameters = new object[parameters.Length+1];
                        finalParameters[0] = translation;
                        for (int i = 0; i < parameters.Length; i++)
                        {
                            finalParameters[i+1] = parameters[i];
                        }

                        translation = string.Format(format, finalParameters);
                    }
                }

                Textfield.text = translation;
            }
        }

#if UNITY_EDITOR

        public void OnValidate()
        {
            if(!string.IsNullOrEmpty(Term))
                Term = Term.Trim();

            if (!string.IsNullOrEmpty(Format))
                Format = Format.Trim();
        }

        public void Reset()
        {
            Clear();
            Textfield = this.GetComponent<TextMeshProUGUI>();
            Term = Textfield.text;

            autoAssignLocalizationProvider();
            markAsChangedIfInEditMode();
        }

        protected void autoAssignLocalizationProvider()
        {
            // Auto select if localization provider is null
            if (LocalizationProvider == null)
            {
                var providerGUIDs = UnityEditor.AssetDatabase.FindAssets("t:" + typeof(LocalizationProvider).Name);
                if (providerGUIDs.Length > 0)
                {
                    LocalizationProvider = UnityEditor.AssetDatabase.LoadAssetAtPath<LocalizationProvider>(UnityEditor.AssetDatabase.GUIDToAssetPath(providerGUIDs[0]));
                }
            }
        }

        protected void markAsChangedIfInEditMode()
        {
            if (UnityEditor.EditorApplication.isPlaying)
                return;

            // Schedule an update to the scene view will rerender (otherwise
            // the change would not be visible unless clicked into the scene view).
            UnityEditor.EditorApplication.QueuePlayerLoopUpdate();

            // Make sure the scene can be saved
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.EditorUtility.SetDirty(this.gameObject);
        }

#endif
    }
}
