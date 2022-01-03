using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SpawnableObject : NetworkBehaviour
{
    [SerializeField] private Renderer spawnObjRenderer;

    private NetworkVariable<Color> spawnObjColor = new NetworkVariable<Color>();

    //NetworkStart() was removed; OnNetworkSpawn works
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        //Make sure we are the server
        if(!IsServer) { return; }

        //Server sends out code to change the color
        spawnObjColor.Value = Random.ColorHSV();
    }

    private void Update()
    {
        //Makes sure the object belongs to us
        if (!IsOwner) { return; }

        //Is activated only when spacebar is pressed
        if (!Input.GetKey(KeyCode.Space)) { return; }

        //Send message to the server to execute the method
        DestroyObjServerRpc();
    }

    //Start listening for color update on object
    private void OnEnable()
    {
        spawnObjColor.OnValueChanged += OnObjColorChanged;
    }

    //Stop listening for color update on object
    private void OnDisable()
    {
        spawnObjColor.OnValueChanged -= OnObjColorChanged;
    }

    private void OnObjColorChanged(Color oldObjColor, Color newObjColor)
    {
        //Only clients update the renderer
        if(!IsClient) { return; }

        //After server calls for color change, the client updates it locally
        spawnObjRenderer.material.color = newObjColor;
    }

    [ServerRpc]
    private void DestroyObjServerRpc()
    {
        //As long as we are doing it on the server side, all clients will be able to see it get destroyed
        //Destroy() destroys the instance completely from the game (server and client)
        //Use GetComponent<NetworkObject>().Despawn() if you want to keep the gameobject on the server but the clients will not see it; call GetComponent<NetworkObject>().Spawn() to bring it back
        Destroy(gameObject);
    }
}
