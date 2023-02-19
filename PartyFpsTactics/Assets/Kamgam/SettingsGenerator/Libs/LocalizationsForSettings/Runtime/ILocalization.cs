using System.Collections.Generic;

namespace Kamgam.LocalizationForSettings
{
    public interface ILocalization
    {
        public enum LocalizationSourceBehaviour { PreferDynamic, PreferStatic }

        public delegate string TranslateTermCallback(string term, string language);
        public delegate void OnLanguageChangedDelegate(string language);

        public void SetDynamicLocalizationCallback(TranslateTermCallback translateTermCallback);

        public void SetLocalizationSourceBehaviour(LocalizationSourceBehaviour behaviour);

        /// <summary>
        /// Calls this to detect the current system language.<br />
        /// Returns the languageIndex of the current system language.
        /// </summary>
        /// <param name="setAsCurrent">If enabled the it will automatically set the detected language as the currently active language.</param>
        /// <returns></returns>
        public int DetectLanguage(bool setAsCurrent = true);

        /// <summary>
        /// Returns the name of the current language.
        /// </summary>
        /// <returns></returns>
        public string GetLanguage();

        /// <summary>
        /// Returns the index of the current language.
        /// </summary>
        /// <returns></returns>
        public int GetLanguageIndex();

        /// <summary>
        /// Alias for SetLanguage(index);
        /// </summary>
        /// <returns></returns>
        public void SetLanguageIndex(int languageIndex);

        /// <summary>
        /// Returns the name of the language at the index.
        /// </summary>
        /// <returns></returns>
        public string GetLanguageAt(int landuageIndex);

        /// <summary>
        /// Finds the language index or returns -1 if the language is not found.
        /// </summary>
        /// <param name="language"></param>
        /// <returns>The language index or -1 if the language is not found.</returns>
        public int GetLanguageIndex(string language);

        /// <summary>
        /// Returns a list of all languages which have at least one static localization term.
        /// </summary>
        /// <returns></returns>
        public List<string> GetLanguages();

        /// <summary>
        /// Returns the number of languages.
        /// </summary>
        /// <returns></returns>
        public int GetLanguageCount();

        /// <summary>
        /// Adds a new language and returns the index of the language.<br />
        /// If the language already exists it does nothing but returning the index of that language.
        /// </summary>
        /// <returns></returns>
        public int AddLanguage(string newLanguage);

        /// <summary>
        /// Changes the current language.
        /// </summary>
        /// <param name="language"></param>
        public void SetLanguage(string language);

        /// <summary>
        /// Changes the current language.
        /// </summary>
        /// <param name="languageIndex"></param>
        public void SetLanguage(int languageIndex);

        /// <summary>
        /// Add a single translation text to a term for the given language.<br />
        /// Creates a new term if it does not yet exist. Otherwise it updates the terms text.
        /// </summary>
        /// <param name="term"></param>
        /// <param name="language"></param>
        /// <param name="text"></param>
        /// <returns>The index of the translation.</returns>
        public int CreateOrUpdateTranslation(string term, string language, string text);

        public void DeleteTranslation(string term);

        /// <summary>
        /// Returns the number of translated terms.
        /// </summary>
        /// <returns></returns>
        public int GetTranslationCount();

        /// <summary>
        /// Return the translation at the given index.<br />
        /// Retuns null if no translation was found.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Translation GetTranslationAt(int index);

        public bool HasTerm(string term);

        /// <summary>
        /// Returns the localized term for the currently set language.
        /// </summary>
        /// <param name="term"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        public string Get(string term);

        /// <summary>
        /// Returns a copied list with each entry translated.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="terms"></param>
        public T LocalizeListAsCopy<T>(T terms) where T : IList<string>, new();

        /// <summary>
        /// Clears and then fills the 'target' list with translated entries.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="terms"></param>
        public void LocalizeList<T>(T terms, T target) where T : IList<string>, new();

        /// <summary>
        /// Called if the language is changed.
        /// </summary>
        public void AddOnLanguageChangedListener(OnLanguageChangedDelegate listener);

        public void RemoveOnLanguageChangedListener(OnLanguageChangedDelegate listener);

        public void Sort();
    }
}
