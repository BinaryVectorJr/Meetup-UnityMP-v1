using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

public class UserGameManager : MonoBehaviour
{
    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        //Singleton basically means only one instance of the class will exist at any given time
        //This is true since we will never have more than one network manager
        if(!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            StartButtons();
        }
        else
        {
            StatusLabels();
            SubmitNewPosition();
        }

        GUILayout.EndArea();
    }
    static void StartButtons()
    {
        if(GUILayout.Button("Host"))
        {
            NetworkManager.Singleton.StartHost();
        }
        if (GUILayout.Button("Client"))
        {
            NetworkManager.Singleton.StartClient();
        }
        if (GUILayout.Button("Server"))
        {
            NetworkManager.Singleton.StartServer();
        }
    }

    private void StatusLabels()
    {
        var mode = NetworkManager.Singleton.IsHost ? "Host" : NetworkManager.Singleton.IsServer ? "Server" : "Client";

        GUILayout.Label("Transport: " + NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name);
        GUILayout.Label("Mode: " + mode);
    }

    //Temporary method to move to new position
    private void SubmitNewPosition()
    {
        if(GUILayout.Button(NetworkManager.Singleton.IsServer ? "Move" : "Request Position Change"))
        {
            var playerObject = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
            var player = playerObject.GetComponent<PlayerScript>();
            player.Move();
        }
    }


}
