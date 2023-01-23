using System;
using System.Collections;
using System.Collections.Generic;
using Crest;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using MrPink;
using MrPink.Health;
using MrPink.PlayerSystem;
using MrPink.Units;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _src.Scripts
{
    public class OceanTimeManager : NetworkBehaviour
    {
        [SyncVar] [SerializeField] [ReadOnly] private float ServerTime;
        [SerializeField] [ReadOnly] private float clientTime;
        [SerializeField] [ReadOnly] private float clientTimeOffset;
        [SerializeReference] private TimeProviderNetworked oceanTimeProvider;
        public static OceanTimeManager Instance;
        private void Awake()
        {
            Instance = this;
        }
        
        [Server]
        void Update()
        {
            ServerTime = Time.time;
        }
        public override void OnStartClient()
        {
            base.OnStartClient();

            if (base.IsHost)
                return;

            GetCurrentClientTimeOffset();
            ApplyClientTimeOffsetToOcean();
        }

        [Client]
        private void GetCurrentClientTimeOffset()
        {
            clientTime = Time.time;
            clientTimeOffset = ServerTime - clientTime;
        }
        [Client]
        private void ApplyClientTimeOffsetToOcean()
        {
            oceanTimeProvider.TimeOffsetToServer = clientTimeOffset;
        }
    }
}