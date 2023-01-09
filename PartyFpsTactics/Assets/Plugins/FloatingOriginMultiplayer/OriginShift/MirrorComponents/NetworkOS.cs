#if MIRROR_43_0_OR_NEWER

using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.Networking.Types;

namespace twoloop
{
    /// <summary>
    /// Singleton network component that stores the host's offset.
    /// You should have this in your gameplay scene if you are doing multiplayer.
    /// </summary>
    public class NetworkOS : NetworkBehaviour
    {
        public static NetworkOS singleton;
        
        // Synced variable used to update hostOffset
        [SyncVar(hook = nameof(OnHostOffsetChanged))] private OriginShift.Offset _hostOffset;

        // The host of the server's world offset
        public static OriginShift.Offset hostOffset;

        private void Awake()
        {
            if (!singleton)
            {
                singleton = this;
            }
            else
            {
                Debug.LogError("There cannot be two NetworkOS's in the scene.");
                Destroy(this);
            }
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            if (!isServer)
            {
                RecenterNetworkIdentities();
            }
        }

        /// <summary>
        /// Fixes the position of scene objects that do not have FOUNet transform / rigidbody components on them.
        /// </summary>
        private void RecenterNetworkIdentities()
        {
            var all = FindObjectsOfType<NetworkIdentity>();
            foreach (var networkComponent in all)
            {
                if (!networkComponent.GetComponent<OSNetTransformBase>())
                {
                    networkComponent.transform.position =
                        OriginShift.RemoteToLocal(hostOffset, networkComponent.transform.position);
                }
            }
        }

        [Server]
        public void SetHostOffset(OriginShift.Offset value)
        {
            _hostOffset = value;
            hostOffset = value;
        }

        private void OnHostOffsetChanged(OriginShift.Offset oldValue, OriginShift.Offset newValue)
        {
            hostOffset = newValue;
        }
    }
}
#endif