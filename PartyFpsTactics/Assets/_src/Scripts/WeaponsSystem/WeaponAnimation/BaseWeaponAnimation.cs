using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MrPink.WeaponsSystem
{
    public abstract class BaseWeaponAnimation : MonoBehaviour
    {
        public abstract UniTask Play();
    }
}