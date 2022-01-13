using System.Collections;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;

public class GameManager : NetworkBehaviour
{
    public static GameManager instance;

    public NetworkObject ball;

    private NetworkObject activeBall;

    public NetworkVariable<int> p1Score = new NetworkVariable<int>(new NetworkVariableSettings {
        ReadPermission = NetworkVariablePermission.Everyone, WritePermission = NetworkVariablePermission.ServerOnly });

    public NetworkVariable<int> p2Score = new NetworkVariable<int>(new NetworkVariableSettings
    {
        ReadPermission = NetworkVariablePermission.Everyone,
        WritePermission = NetworkVariablePermission.ServerOnly
    });

    public NetworkVariable<bool> p1PlayAgain = new NetworkVariable<bool>(new NetworkVariableSettings
    {
        ReadPermission = NetworkVariablePermission.Everyone,
        WritePermission = NetworkVariablePermission.Everyone
    });

    public NetworkVariable<bool> p2PlayAgain = new NetworkVariable<bool>(new NetworkVariableSettings
    {
        ReadPermission = NetworkVariablePermission.Everyone,
        WritePermission = NetworkVariablePermission.Everyone
    });

    public UIManager localUIManager;


    private void Start()
    {
        instance = this;
        //DontDestroyOnLoad(this);

        Cursor.visible = false;

        localUIManager = FindObjectOfType<UIManager>();
    }

    private void Update()
    {
        if (p1PlayAgain.Value && p2PlayAgain.Value)
        {
            PlayAgainServerRpc();
        }
    }

    [ServerRpc]
    private void PlayAgainServerRpc()
    {
        Bullet[] bullet = FindObjectsOfType<Bullet>();

        for (int i = 0; i < bullet.Length; i++)
        {
            Destroy(bullet[i].gameObject);
        }
        //FindObjectOfType<UIManager>().PlayAgain();
        p1Score.Value = 0;
        p2Score.Value = 0;

        p1PlayAgain.Value = false;
        p2PlayAgain.Value = false;
        Time.timeScale = 1;
    }

    public void GameOver(bool p1Win)
    {
        if (p1Win)
        {
            if (NetworkManager.Singleton.IsHost)
            {
                // Activate Win Screen
                localUIManager.ActivateWinScreen();
                ActivateClientScreenClientRpc(false);
            }
        }
        else
        {
            if (NetworkManager.Singleton.IsHost)
            {
                // Activate Lose Screen
                localUIManager.ActivateLoseScreen();
                ActivateClientScreenClientRpc(true);
            }
        }
        Time.timeScale = 0;
    }

    [ClientRpc]
    private void ActivateClientScreenClientRpc(bool win)
    {
        if (IsOwner) { return; }

        if (win)
        {
            localUIManager.ActivateWinScreen();
        }
        else
        {
            localUIManager.ActivateLoseScreen();
        }
    }


    [ServerRpc (RequireOwnership = false)]
    public void SpawnBallServerRpc()
    {
        activeBall = Instantiate(ball);

        activeBall.Spawn();
    }

    public IEnumerator SpawnBall()
    {
        yield return new WaitForSeconds(2f);

        SpawnBallServerRpc();
    }

    public void PlayAgain()
    {
        if (IsHost)
        {
            p1PlayAgain.Value = true;
        }
        if (IsClient)
        {
            p2PlayAgain.Value = false;
        }
    }
}
