using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerMovement : NetworkBehaviour
{
    #region Player Related
    [SerializeField] private Animator playerAnimControl;
    [SerializeField] private SkinnedMeshRenderer playerTeamColorRenderer;       //Had to change this from Renderer to SkinnedMeshRenderer due to the asset being used; Renderer should work normally
    [SerializeField] private Color[] teamColors;

    private NetworkVariable<byte> currentTeamIndex = new NetworkVariable<byte>();
    //private NetworkVariable<byte> currentTeamIndex = new NetworkVariable<byte>(NetworkVariableReadPermission.OwnerOnly);
    #endregion

    [SerializeField] private GameObject testParticles;

    private void Update()
    {
        //Check only for our player character rather than for all players in scene
        //if(!IsOwner) { return; }

        //if(!Input.GetKeyDown(KeyCode.Space)) { return; }

        //Owner sends message to server to execute something
        //SpawnParticleServerRpc();

        //We do it here because this instance is done locally and thus the owner does not have to wait for the server to execute this stuff
        //(e.g. shoot a gun then bullet damage is on server side but the muzzle flash and the recoil anims can be done on the client itself immediately)
        //Instantiate(testParticles, transform.position, transform.rotation);
    }


    //With the RPC we can use a normal C# method to do whatever is needed; for example we spawn in a particle, but we can even make it to where we can interact with stuff
    //Any Rpc method needs to end with either "ServerRpc" or "ClienRpc" - that is the syntax set by Netcode; also add [ServerRpc] before any method to explicitly declare it
    //[ServerRpc] means that the client sends a message to the server saying "hey execute this block of code for me pls", instead of executing it itself
    //Also ServerRpc methods broadcasts to all clients, so in essence we request something to the server individually, and then the server executes it and tells all other clients how to respond
    //RpcDelivery means how important it is to execute this; meaning if for some reason it is not executed should the server actually care about it or just move on
    //[ServerRpc(Delivery = RpcDelivery.Unreliable)]
    //private void SpawnParticleServerRpc()
    //{
    //    //Server sends message to all clients to execute what was asked for by original owner
    //    SpawnParticleClientRpc();
    //}

    //If this method is called on the server, server sends message to all clients and all clients will run this method
    //If our message from server-to-client fails, then dont bother sending it again (improves performance)
    //[ClientRpc(Delivery = RpcDelivery.Unreliable)]
    //private void SpawnParticleClientRpc()
    //{
    //    if(IsOwner) { return; }

    //    Instantiate(testParticles, transform.position, transform.rotation);
    //}

    [ServerRpc]
    public void SetTeamServerRpc(byte userNewTeamIndex)
    {
        //Since we have only 4 teams: 0, 1, 2, 3 - we need to have a valid team index
        if(userNewTeamIndex > 3) { return; }

        //Update the team index variable according to whatever color was picked
        currentTeamIndex.Value = userNewTeamIndex;
    }

    //Start listening for currentTeamIndex value being updated
    private void OnEnable()
    {
        currentTeamIndex.OnValueChanged += OnTeamChanged;
    }

    //Stop listening for currentTeamIndex value being updated
    private void OnDisable()
    {
        currentTeamIndex.OnValueChanged -= OnTeamChanged;
    }

    //TEAM CHANGE WORKS ON HOST BUT NOT CLIENT FOR SOME REASON
    private void OnTeamChanged(byte oldTeamIndex, byte newTeamIndex)
    {
        //Only clients need to update the renderer; this gets done locally but the data that it has been changed is communicated to all clients
        if(!IsClient) { return; }

        //Update color
        //playerTeamColorRenderer.material.SetColor("_BaseColor", teamColors[newTeamIndex]); //Use this way to set color for materials in URP
        playerTeamColorRenderer.material.color = teamColors[newTeamIndex];
    }

}
