using UnityEngine;
using MLAPI;

public class UIManager : MonoBehaviour
{
    public GameObject startMenu;
    public GameObject gameUI;
    public GameObject winScreen;
    public GameObject loseScreen;


    public void PlayButtonPress()
    {
        startMenu.SetActive(false);
        gameUI.SetActive(true);
        NetworkManager.Singleton.StartClient();
        StartCoroutine(GameManager.instance.SpawnBall());
    }

    public void StartButtonPress()
    {
        startMenu.SetActive(false);
        gameUI.SetActive(true);
        NetworkManager.Singleton.StartHost();
    }

    public void ActivateWinScreen()
    {
        winScreen.SetActive(true);
    }

    public void ActivateLoseScreen()
    {
        loseScreen.SetActive(true);
    }

    public void PlayAgain()
    {
        winScreen.SetActive(false);
        loseScreen.SetActive(false);

        if (NetworkManager.Singleton.IsHost)
        {
            GameManager.instance.p1PlayAgain.Value = true;
        }
        if (NetworkManager.Singleton.IsClient)
        {
            GameManager.instance.p2PlayAgain.Value = true;
        }
    }

    public void Quit()
    {
        //TODO: Signals across all clients to do this (clientRPC?)
        //TODO: Ejects players from network
        //TODO: Activate Main men deactivates game UI
        Player[] players = FindObjectsOfType<Player>();

        for (int i = 0; i < players.Length; i++)
        {
            players[i].Disconnect();
        }

    }

    public void PlayAgainButtonPress(Transform trans)
    {
        if (trans.position.x > 1)
        {
            GameManager.instance.p2PlayAgain.Value = true;
        }
        else
        {
            GameManager.instance.p1PlayAgain.Value = false;
        }
    }
}
