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
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private GameObject passwordEntryUI;
    [SerializeField] private GameObject teamPickerUI;
    [SerializeField] private GameObject leaveButton;

    private static Dictionary<ulong, PlayerData> clientDataDict;

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
        //As soon as Host starts hosting, we create a new dictionary for the session
        clientDataDict = new Dictionary<ulong, PlayerData>();
        //Add ourselves to the dictionary
        clientDataDict[NetworkManager.Singleton.LocalClientId] = new PlayerData(nameInputField.text);

        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        
        //Cannot add extra parameters to StartHost as there are no overloads, however the default callback seems to work.
        NetworkManager.Singleton.StartHost();

        //gameManagerScript.mainCam.SetActive(false);
    }

    public void Client()
    {
        var payload = JsonUtility.ToJson(new ConnectionPayload()
        {
            userPassword = passwordInputField.text,
            userPlayerName = nameInputField.text
        });

        byte[] payloadBytes = Encoding.ASCII.GetBytes(payload);

        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;
        NetworkManager.Singleton.StartClient();
    }

    public void Leave()
    {
        //Changelog: Removed NetworkManager's StopServer(), StopClient() and StopHost() methods and replaced with single NetworkManager.Shutdown() method for all (#1108)
        NetworkManager.Singleton.Shutdown();

        if(NetworkManager.Singleton.IsHost)
        {
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

    //Why did we add the "?" to the data type? without it return null does not work
    //Ans: Adding the "?" makes the Datatype nullible i.e. it can accept null data type if it typically couldnt
    public static PlayerData? GetPlayerData(ulong u_clientID)
    {
        if(clientDataDict.TryGetValue(u_clientID, out PlayerData u_playerData))
        {
            return u_playerData;
        }

        return null;
    }

    //OnClienConnected does not get called for the host when they themselves connect, so we do it manually (might be fixed later)
    private void HandleServerStarted()
    {
        if(NetworkManager.Singleton.IsHost)
        {
            HandleClientConnected(NetworkManager.Singleton.ServerClientId);
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
        if(NetworkManager.Singleton.IsServer)
        {
            clientDataDict.Remove(userClientID);
        }

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
        string payload = Encoding.ASCII.GetString(userConnData);
        var connectionPayload = JsonUtility.FromJson<ConnectionPayload>(payload);
        //Debug.Log(connectionPayload.ConnectionPayload.userPassword);

        bool approveConn = connectionPayload.userPassword == passwordInputField.text;

        Vector3 currSpawnPos = Vector3.zero;
        Quaternion currSpawnRot = Quaternion.identity;       //So that the initial character faces the camera

        if(approveConn)
        {
            switch(NetworkManager.Singleton.ConnectedClients.Count)
            {
                case 0:
                    currSpawnPos = Vector3.zero;
                    currSpawnRot = Quaternion.Euler(0f, 180f, 0f);
                    break;
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

            clientDataDict[userClientID] = new PlayerData(connectionPayload.userPlayerName);
        }

        //Spawn players in if they have a correct connection
        userCallback(true, null, approveConn, currSpawnPos, currSpawnRot);
    }
}
