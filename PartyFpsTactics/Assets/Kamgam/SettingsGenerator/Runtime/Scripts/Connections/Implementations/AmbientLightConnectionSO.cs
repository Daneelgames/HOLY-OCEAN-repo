using UnityEngine;

namespace Kamgam.SettingsGenerator
{
    [CreateAssetMenu(fileName = "AmbientLightConnection", menuName = "SettingsGenerator/Connection/AmbientLightConnection", order = 4)]
    public class AmbientLightConnectionSO : FloatConnectionSO
    {
        protected AmbientLightConnection _connection;

        public override IConnection<float> GetConnection()
        {
            if(_connection == null)
                Create();

            return _connection;
        }

        public void Create()
        {
            _connection = new AmbientLightConnection();
        }

        public override void DestroyConnection()
        {
            if (_connection != null)
                _connection.Destroy();

            _connection = null;
        }
    }
}
