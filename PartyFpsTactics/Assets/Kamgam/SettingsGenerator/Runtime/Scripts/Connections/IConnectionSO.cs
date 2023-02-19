namespace Kamgam.SettingsGenerator
{
    // The ConnectionSOs are just wrappers for connections so we
    // can used them as ScriptableObjects.

    public interface IConnectionSO<TConnection>
    {
        public TConnection GetConnection();
        public void DestroyConnection();
    }
}
