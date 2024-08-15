using Unity.Netcode;
using UnityEngine;

public class Emote : NetworkBehaviour
{
    [SerializeField] GameObject emoteHappy;
    [SerializeField] GameObject emoteAngry;
    [SerializeField] GameObject emoteSad;
    [SerializeField] GameObject emoteShocked;

    
    private void Update()
    {
        if(IsOwner && IsSpawned)
        {
            //Instantiates EmoteEffect depending on input
            if (Input.GetKeyDown("v"))
                SendEmoteRPC(0);
            if (Input.GetKeyDown("b"))
                SendEmoteRPC(1);
            if (Input.GetKeyDown("n"))
                SendEmoteRPC(2);
            if (Input.GetKeyDown("m"))
                SendEmoteRPC(3);
        }
    }


    [Rpc(SendTo.Everyone)]
    private void SendEmoteRPC(byte emoteIndex)
    {
        switch(emoteIndex)
        {
            case 0:
                Instantiate(emoteHappy, transform);
                break;

            case 1:
                Instantiate(emoteAngry, transform);
                break;

            case 2:
                Instantiate(emoteSad, transform);
                break;

            case 3:
                Instantiate(emoteShocked, transform);
                break;
        }        
    }
}
