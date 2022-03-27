using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MrPink.Cheats
{
    public class CheatApplier : MonoBehaviour
    {
        [SerializeField, AssetsOnly, Required] 
        private CheatConfig _config;

        [SerializeField] 
        private bool _isApplied = true;


        private void Start()
        {
            if (_isApplied)
                _config.ApplyAll();
        }
    }
}