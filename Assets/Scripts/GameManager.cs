using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
public class GameManager : NetworkBehaviour
{
    [SerializeField] private int winScore;
    [SerializeField] private float winScreenDurationSeconds;
    public int WinScore { get { return winScore; } }

    [SerializeField] private TMP_Text winText;
    [SerializeField] private TMP_Text announcementText;


    public enum GameState { PreMatch, Playing, WinScreen }

    private NetworkVariable<GameState> currentGameState = new NetworkVariable<GameState>(writePerm: NetworkVariableWritePermission.Server);
    public NetworkVariable<GameState> CurrentGameState { get { return currentGameState; } }

    [SerializeField] private List<Transform> startingPositions;
    public List<Transform> StartingPositions { get { return startingPositions; } }

    private void OnGUI()
    {
        if(IsHost && currentGameState.Value == GameState.PreMatch)
        {
            GUILayout.Space(50);
            if (GUILayout.Button("Start Match"))
            {
                currentGameState.Value = GameState.Playing;
            }
        }
    }

    public override void OnNetworkSpawn()
    {    
        if(IsServer)
        {
            currentGameState.Value = GameState.PreMatch;
        }
    }

    //just for announcementText
    private void Update()
    {
        if (currentGameState.Value == GameState.PreMatch)
        {
            if (IsHost)
            {
                announcementText.text = "Click 'Start Match' to start.";
            }
            else if (!IsServer && IsClient)
            {
                announcementText.text = "Waiting for host...";
            }
            else
            {
                announcementText.text = "Click 'Host' or 'Join'.";
            }
        }
        else
        {
            announcementText.text = "First to " + winScore + " points Wins!";
        }
    }

    //the players make the GameManager subscribe this method to their score.OnValueChanged

    public void PlayerWinCheck(int previousValue, int newValue)
    {
        if (newValue >= winScore) 
        {
            PlayerWin();
        }
    }

    private void PlayerWin()
    {
        int winningPlayerId = -1;

        foreach (Player p in FindObjectsOfType(typeof(Player)))
        {
            if (p.Score.Value >= WinScore)
            {
                winningPlayerId = (int)p.OwnerClientId;
            }
        }

        if (winningPlayerId == -1)
            Debug.LogError("A Player detected that they should have won, but GameManager didn't find any player that has met the winScore");

        winText.text = "Player " + (winningPlayerId + 1) + " Wins!";

        if (IsServer)
        {
            currentGameState.Value = GameState.WinScreen;
            StartCoroutine(WinScreenWait());
        }
    }


    IEnumerator WinScreenWait()
    {
        yield return new WaitForSeconds(winScreenDurationSeconds);

        currentGameState.Value = GameState.PreMatch;
        
        ResetToPreMatch();
    }





    private void ResetToPreMatch()
    {
        ResetWinTextRPC();
        ResetPositionsRPC();
        //resets scores
        foreach (Player p in FindObjectsOfType(typeof(Player)))
        {
            p.Score.Value = 0;
            
        }
    }

    [Rpc(SendTo.Everyone)]
    private void ResetPositionsRPC()
    {
        //finds this device's player
        foreach (Player p in FindObjectsOfType(typeof(Player)))
        {
            p.ResetToStartingPosition(); 
        }
    }

    [Rpc(SendTo.Everyone)]
    private void ResetWinTextRPC()
    {
        winText.text = "";
    }
}
