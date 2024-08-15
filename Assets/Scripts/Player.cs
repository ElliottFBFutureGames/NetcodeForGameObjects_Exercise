using UnityEngine;
using Unity.Netcode;
public class Player : NetworkBehaviour
{
    [SerializeField] private float speed;

    [SerializeField] private GameObject bullet;
    [SerializeField] private GameObject model;
    [SerializeField] private TextMesh scoreText;
    [SerializeField] private TextMesh playerNumberText;
    private Rigidbody2D rb;
    private GameManager gameManager;

    private bool canControl = false;

    private NetworkVariable<int> score = new NetworkVariable<int>(writePerm: NetworkVariableWritePermission.Server);
    public NetworkVariable<int> Score { get { return score; } }


    public override void OnNetworkSpawn()
    {
        //updates score text for everyone
        score.OnValueChanged += ScoreTextUpdateRPC;
        
        gameManager = FindAnyObjectByType<GameManager>(); // server needs this
        score.OnValueChanged += gameManager.PlayerWinCheck;

        //sets player ids text
        foreach (Player p in FindObjectsOfType<Player>())
        {
            p.playerNumberText.text = (1 + (long)p.OwnerClientId).ToString();
        }


        if (!IsOwner)
        {
            enabled = false;      
        }
        else if (IsOwner)
        {
            rb = GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;

            //when the GameState changes, checks if the player can be controlled depending on the state
            gameManager.CurrentGameState.OnValueChanged += CanControlUpdate;
            CanControlUpdate(GameManager.GameState.PreMatch, gameManager.CurrentGameState.Value); //checks if it joined an active match

            ResetToStartingPosition();
        }
    }
    private void Update()
    {        
        if (IsOwner && IsSpawned)
        {
            if (canControl)
            {
                Vector2 moveInput = new Vector2(
                    Input.GetAxis("Horizontal"),
                    Input.GetAxis("Vertical"));

                //position
                rb.velocity = moveInput * speed; //* Time.deltaTime;

                //rotation
                if (moveInput != Vector2.zero)
                {
                    float angle = Vector2.SignedAngle(Vector2.right, moveInput.normalized);
                    model.transform.rotation = Quaternion.Euler(0, 0, angle);
                }

                //shoot
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    ShootBulletRPC();
                }
            }
            else
            {
                rb.velocity = Vector2.zero;
            }
        }
    }


    public void ResetToStartingPosition()
    {
        if (IsOwner)
        {
            if ((int)OwnerClientId < gameManager.StartingPositions.Count)
            {
                transform.position = gameManager.StartingPositions[(int)OwnerClientId].position;

            }
            else
            {
                //player 6, 7 and so on spawns in the center
                transform.position = Vector2.zero;
            }
        }
    }


    private void CanControlUpdate(GameManager.GameState previousValue, GameManager.GameState newValue)
    {
        if(newValue == GameManager.GameState.Playing)
        {
            canControl = true;
        }
        else
        {
            canControl = false;
        }
    }

    [Rpc(SendTo.Server)]
    private void ShootBulletRPC()
    {
        //instantiate bullet
        GameObject spawnObject = Instantiate(bullet);
        spawnObject.transform.position = transform.position;
        spawnObject.transform.rotation = model.transform.rotation;

        //spawn on network
        NetworkObject netObject = spawnObject.GetComponent<NetworkObject>();        
        netObject.SpawnWithOwnership(OwnerClientId);
        
    }


    [Rpc(SendTo.Server)]
    public void ScoreUpRPC()
    {
        score.Value++;
    }

    [Rpc(SendTo.Everyone)]
    private void ScoreTextUpdateRPC(int previousValue, int newValue)
    {
        scoreText.text = newValue.ToString();
    }
}
