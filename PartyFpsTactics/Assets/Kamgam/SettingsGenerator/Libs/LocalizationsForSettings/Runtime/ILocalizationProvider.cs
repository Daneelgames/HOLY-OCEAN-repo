namespace Kamgam.LocalizationForSettings
{
    public interface ILocalizationProvider
    {
        public bool HasLocalization();
        public ILocalization GetLocalization();
    }
}
