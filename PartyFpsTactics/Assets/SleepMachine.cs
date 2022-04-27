using System.Collections;
using System.Collections.Generic;
using MrPink;
using UnityEngine;

public class SleepMachine : MonoBehaviour
{
    public void PlayerInside(bool inside)
    {
        GameManager.Instance.SetPlayerSleepTimeScale(inside);
        Game.Player.CharacterNeeds.SetSleeping(inside);
    }
}
