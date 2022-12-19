using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Managing;
using Steamworks;
using UnityEngine;

public class NetworkHudCanvasesExtention : MonoBehaviour
{
    [SerializeField] private NetworkHudCanvases _networkHudCanvases;

    private void Awake()
    {
        _networkHudCanvases.OnClickServer.AddListener(OnClickServer);
        _networkHudCanvases.OnClickClient.AddListener(OnClickClient);
        _networkHudCanvases.OnClickCopyOwnId.AddListener(OnClickCopyOwnId);
    }

    void OnClickServer()
    {
        var steamId = SteamUser.GetSteamID();
        var newId = (int)steamId.m_SteamID;
        _networkHudCanvases.SetClientAddress(newId);
    }
    void OnClickClient()
    {
        _networkHudCanvases.SetClientAddressFromInputField();
    }
    void OnClickCopyOwnId()
    {
        var steamId = SteamUser.GetSteamID();
        var newId = steamId.m_SteamID.ToString();
        GUIUtility.systemCopyBuffer = newId;
    }
}
