using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using System;

public class UserGameManager : NetworkBehaviour
{
    //General knowledge, the Network Manager (NM) keeps track of all player prefabs, so we can change it through that

    [SerializeField] public List<Transform> spawnTransforms = new List<Transform>();
    [SerializeField] public GameObject mainCam;
    [SerializeField] private int tempInd;

    public NetworkClient currNetworkClient;
    public PlayerMovement currPlayerMovement;

    public void SelectTeam(int userTeamIndex)
    {
        tempInd = userTeamIndex;

        //Get local client's ID
        ulong localID = NetworkManager.Singleton.LocalClientId;

        //Tell NM to get object with ID
        //Return if failed
        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(localID, out NetworkClient networkClient))
        {
            return;
        }

        currNetworkClient = networkClient;

        //If object found, get the PlayerMovement component (or any component that houses the SetTeam method
        if (!networkClient.PlayerObject.TryGetComponent<PlayerMovement>(out PlayerMovement userPlayerMovement))
        {
            return;
        }

        //Everything is successful
        //If PlayerMovement found, call the server rpc method; send a message to the server to set the local client's team
        userPlayerMovement.SetTeamServerRpc((byte)userTeamIndex);
    }

    void OnGUI()
    {
        GUI.Label(new Rect(100, 100, 100, 20), tempInd.ToString());
    }

}

#region Old Notes (Manual Connection UI)
//private void OnGUI()
//{
//    GUILayout.BeginArea(new Rect(10, 10, 300, 300));
//    //Singleton basically means only one instance of the class will exist at any given time
//    //This is true since we will never have more than one network manager
//    if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
//    {
//        StartButtons();
//    }
//    else
//    {
//        StatusLabels();
//        SubmitNewPosition();
//    }

//    GUILayout.EndArea();
//}
//static void StartButtons()
//{
//    if (GUILayout.Button("Host"))
//    {
//        NetworkManager.Singleton.StartHost();
//    }
//    if (GUILayout.Button("Client"))
//    {
//        NetworkManager.Singleton.StartClient();
//    }
//    if (GUILayout.Button("Server"))
//    {
//        NetworkManager.Singleton.StartServer();
//    }
//}

//private void StatusLabels()
//{
//    var mode = NetworkManager.Singleton.IsHost ? "Host" : NetworkManager.Singleton.IsServer ? "Server" : "Client";

//    GUILayout.Label("Transport: " + NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name);
//    GUILayout.Label("Mode: " + mode);
//}

////Temporary method to move to new position
//private void SubmitNewPosition()
//{
//    if (GUILayout.Button(NetworkManager.Singleton.IsServer ? "Move" : "Request Position Change"))
//    {
//        var playerObject = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
//        var player = playerObject.GetComponent<PlayerScript>();
//        player.Move();
//    }
//}
#endregion
