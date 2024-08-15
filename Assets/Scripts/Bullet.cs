using UnityEngine;
using Unity.Netcode;
public class Bullet : NetworkBehaviour
{
    [SerializeField] private float speed;

    private Player owningPlayer;

    public override void OnNetworkSpawn()
    {
        if (IsOwner && IsSpawned)
        {
            
            //finds this device's player
            foreach (Player p in FindObjectsOfType(typeof(Player)))
            {
                if (p.OwnerClientId == OwnerClientId)
                {
                    owningPlayer = p;
                }
            }
            //logs error if it can't find this device's player
            if (owningPlayer == null)
                Debug.LogError("owningPlayer null >:(");
        }
    }

    // Update is called once per frame
    void Update()
    {       
        //the owner of the bullet updates it's position
        if (IsOwner && IsSpawned)
        {
            transform.position += transform.right * speed * Time.deltaTime;
        }
    }

    [Rpc(SendTo.Server)]
    private void DespawnBulletRPC()
    {
        if (GetComponent<NetworkObject>().IsSpawned)
        {
            GetComponent<NetworkObject>().Despawn();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsOwner && IsSpawned)
        {
            //give owning player a point if bullet hits opponent player
            if (collision.gameObject.TryGetComponent<Player>(out Player player) && !player.IsOwner)
            {
                owningPlayer.ScoreUpRPC();
                DespawnBulletRPC();
            }

            if(collision.gameObject.tag == "Wall")
            {
                DespawnBulletRPC();
            }
        }
    }
}
