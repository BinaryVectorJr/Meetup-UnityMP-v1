using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using TMPro;

public class PasswordNetworkManager : MonoBehaviour
{
    public UserGameManager gameManagerScript;

    [SerializeField] private TMP_InputField passwordInputField;
    [SerializeField] private GameObject passwordEntryUI;
    [SerializeField] private GameObject teamPickerUI;
    [SerializeField] private GameObject leaveButton;

    private void Start()
    {
        NetworkManager.Singleton.OnServerStarted += HandleServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisonnected;
    }

    private void OnDestroy()
    {
        if(NetworkManager.Singleton == null) { return; }

        NetworkManager.Singleton.OnServerStarted -= HandleServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisonnected;
    }

    public void Host()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        
        //Cannot add extra parameters to StartHost as there are no overloads, however the default callback seems to work.
        NetworkManager.Singleton.StartHost();

        //gameManagerScript.mainCam.SetActive(false);
    }

    public void Client()
    {
        NetworkManager.Singleton.NetworkConfig.ConnectionData = Encoding.ASCII.GetBytes(passwordInputField.text);
        NetworkManager.Singleton.StartClient();
    }

    public void Leave()
    {
        if(NetworkManager.Singleton.IsHost)
        {
            //Changelog: Removed NetworkManager's StopServer(), StopClient() and StopHost() methods and replaced with single NetworkManager.Shutdown() method for all (#1108)
            NetworkManager.Singleton.Shutdown();
            NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.Shutdown();
        }

        passwordEntryUI.SetActive(true);
        teamPickerUI.SetActive(false);
        leaveButton.SetActive(false);
        //gameManagerScript.mainCam.SetActive(true);
    }

    //OnClienConnected does not get called for the host when they themselves connect, so we do it manually (might be fixed later)
    private void HandleServerStarted()
    {
        if(NetworkManager.Singleton.IsHost)
        {
            HandleClientConnected(NetworkManager.Singleton.LocalClientId);
        }
    }

    //Called on the server every time a client joins; also called on client side whent they themselves join but not when others join
    private void HandleClientConnected(ulong userClientID)
    {
        if(userClientID == NetworkManager.Singleton.LocalClientId)
        {
            passwordEntryUI.SetActive(false);
            teamPickerUI.SetActive(true);
            leaveButton.SetActive(true);
        }
    }

    private void HandleClientDisonnected(ulong userClientID)
    {
        if (userClientID == NetworkManager.Singleton.LocalClientId)
        {
            passwordEntryUI.SetActive(true);
            teamPickerUI.SetActive(false);
            leaveButton.SetActive(false);
            //gameManagerScript.mainCam.SetActive(true);
        }
    }

    private void ApprovalCheck(byte[] userConnData, ulong userClientID, NetworkManager.ConnectionApprovedDelegate userCallback)
    {
        string hostPassword = Encoding.ASCII.GetString(userConnData);
        bool approveConn = hostPassword == passwordInputField.text;

        Vector3 currSpawnPos = Vector3.zero;
        Quaternion currSpawnRot = Quaternion.Euler(0f, 180f, 0f);       //So that the initial character faces the camera

        switch(NetworkManager.Singleton.ConnectedClients.Count)
        {
            case 1:
                currSpawnPos = gameManagerScript.spawnTransforms[0].position;
                currSpawnRot = gameManagerScript.spawnTransforms[0].rotation;
                break;
            case 2:
                currSpawnPos = gameManagerScript.spawnTransforms[1].position;
                currSpawnRot = gameManagerScript.spawnTransforms[1].rotation;
                break;

            //TODO: If player 2 leaves and rejoins, they become player 3 and then the code breaks. Fix it <-- Not a problem anymore so I think it has been fixed.
        }

        //Spawn players in if they have a correct connection
        userCallback(true, null, approveConn, currSpawnPos, currSpawnRot);
    }
}
