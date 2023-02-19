using UnityEngine;

namespace Kamgam.SettingsGenerator
{
    [CreateAssetMenu(fileName = "ResolutionConnection", menuName = "SettingsGenerator/Connection/ResolutionConnection", order = 4)]
    public class ResolutionConnectionSO : OptionConnectionSO
    {
        protected ResolutionConnection _connection;

        public override IConnectionWithOptions<string> GetConnection()
        {
            if(_connection == null)
                Create();

            return _connection;
        }

        public void Create()
        {
            _connection = new ResolutionConnection();
        }

        public override void DestroyConnection()
        {
            if (_connection != null)
                _connection.Destroy();

            _connection = null;
        }
    }
}
