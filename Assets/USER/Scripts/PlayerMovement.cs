using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerMovement : NetworkBehaviour
{
    #region Player Related
    [SerializeField] private Animator playerAnimControl;
    [SerializeField] private Renderer playerTeamColorRenderer;
    [SerializeField] private Color[] teamColors;
    #endregion

    [SerializeField] private GameObject testParticles;

    private void Update()
    {
        //Check only for our player character rather than for all players in scene
        if(!IsOwner) { return; }

        if(!Input.GetKeyDown(KeyCode.Space)) { return; }

        //Owner sends message to server to execute something
        SpawnParticleServerRpc();

        //We do it here because this instance is done locally and thus the owner does not have to wait for the server to execute this stuff
        //(e.g. shoot a gun then bullet damage is on server side but the muzzle flash and the recoil anims can be done on the client itself immediately)
        Instantiate(testParticles, transform.position, transform.rotation);
    }


    //With the RPC we can use a normal C# method to do whatever is needed; for example we spawn in a particle, but we can even make it to where we can interact with stuff
    //Any Rpc method needs to end with either "ServerRpc" or "ClienRpc" - that is the syntax set by Netcode; also add [ServerRpc] before any method to explicitly declare it
    //[ServerRpc] means that the client sends a message to the server saying "hey execute this block of code for me pls", instead of executing it itself
    //Also ServerRpc methods broadcasts to all clients, so in essence we request something to the server individually, and then the server executes it and tells all other clients how to respond
    //RpcDelivery means how important it is to execute this; meaning if for some reason it is not executed should the server actually care about it or just move on
    [ServerRpc(Delivery = RpcDelivery.Unreliable)]
    private void SpawnParticleServerRpc()
    {
        //Server sends message to all clients to execute what was asked for by original owner
        SpawnParticleClientRpc();
    }

    //If this method is called on the server, server sends message to all clients and all clients will run this method
    //If our message from server-to-client fails, then dont bother sending it again (improves performance)
    [ClientRpc(Delivery = RpcDelivery.Unreliable)]
    private void SpawnParticleClientRpc()
    {
        if(IsOwner) { return; }

        Instantiate(testParticles, transform.position, transform.rotation);
    }
}
