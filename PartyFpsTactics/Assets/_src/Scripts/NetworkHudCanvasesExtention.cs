using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Managing;
using Steamworks;
using UnityEngine;

public class NetworkHudCanvasesExtention : MonoBehaviour
{
    [SerializeField] private NetworkHudCanvases _networkHudCanvases;

    /*
    void NetworkHudCanvases_OnClickServer()
    {
        var steamId = SteamUser.GetSteamID();
        var newId = (int)steamId.m_SteamID;
        _networkHudCanvases.SetClientAddress(newId);
    }
    void NetworkHudCanvases_OnClickClient()
    {
        _networkHudCanvases.SetClientAddressFromInputField();
    }
    void OnClickCopyOwnId()
    {
        var steamId = SteamUser.GetSteamID();
        var newId = steamId.m_SteamID.ToString();
        GUIUtility.systemCopyBuffer = newId;
    }*/
}
