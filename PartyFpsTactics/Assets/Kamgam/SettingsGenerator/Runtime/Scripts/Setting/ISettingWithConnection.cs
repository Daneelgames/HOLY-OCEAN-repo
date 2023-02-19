namespace Kamgam.SettingsGenerator
{
    public interface ISettingWithConnection<TValue> : ISettingWithValue<TValue>
    {
        public void SetConnection(IConnection<TValue> connection);
        public IConnection<TValue> GetConnection();
    }
}