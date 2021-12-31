using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerScript : NetworkBehaviour
{
    public NetworkVariable<Vector3> playerPos = new NetworkVariable<Vector3>();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if(IsOwner)
        {
            Move();
        }
    }

    public void Move()
    {
        if(NetworkManager.Singleton.IsServer)
        {
            var randomPos = GetRandomPositionOnPlane();
            transform.position = randomPos;
            playerPos.Value = randomPos;
        }
        else
        {
            SubmitPositionRequestServerRpc();
        }
    }

    [ServerRpc]
    void SubmitPositionRequestServerRpc(ServerRpcParams rpcParams = default)
    {
        playerPos.Value = GetRandomPositionOnPlane();
    }

    static Vector3 GetRandomPositionOnPlane()
    {
        return new Vector3(Random.Range(-3f, 3f), 1f, Random.Range(-3f, 3f));
    }

    private void Update()
    {
        transform.position = playerPos.Value;
    }
}
