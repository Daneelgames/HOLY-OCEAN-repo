using UnityEngine;

namespace MrPink.PlayerSystem
{
    [SerializeField]
    public class MovementsState
    {
        public bool IsGrounded;
        public bool IsCrouching;

        public bool IsLeaning;

        public bool IsRunning;

        public bool IsMoving;
        
        public bool IsClimbing;
        
        public bool CanVault;
        public bool IsUnderwater;

    }
}