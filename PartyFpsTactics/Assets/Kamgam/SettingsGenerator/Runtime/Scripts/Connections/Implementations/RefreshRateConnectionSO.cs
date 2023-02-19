using UnityEngine;

namespace Kamgam.SettingsGenerator
{
    [CreateAssetMenu(fileName = "RefreshRateConnection", menuName = "SettingsGenerator/Connection/RefreshRateConnection", order = 4)]
    public class RefreshRateConnectionSO : OptionConnectionSO
    {
        protected RefreshRateConnection _connection;

        public override IConnectionWithOptions<string> GetConnection()
        {
            if(_connection == null)
                Create();

            return _connection;
        }

        public void Create()
        {
            _connection = new RefreshRateConnection();
        }

        public override void DestroyConnection()
        {
            if (_connection != null)
                _connection.Destroy();

            _connection = null;
        }
    }
}
