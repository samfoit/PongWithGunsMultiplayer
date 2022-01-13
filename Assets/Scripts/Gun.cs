using System.Collections;
using UnityEngine;
using MLAPI;
using MLAPI.NetworkVariable;
using MLAPI.Spawning;
using MLAPI.Messaging;

public class Gun : NetworkBehaviour
{
    public NetworkVariable<ulong> pId = new NetworkVariable<ulong>(new NetworkVariableSettings
    {
        ReadPermission = NetworkVariablePermission.Everyone,
        WritePermission = NetworkVariablePermission.Everyone
    });

    public NetworkVariable<bool> playerOneGun = new NetworkVariable<bool>(new NetworkVariableSettings
    {
        ReadPermission = NetworkVariablePermission.Everyone,
        WritePermission = NetworkVariablePermission.Everyone
    });

    public Transform player;
    public Transform firePoint;

    private bool switched = false;

    public SpriteRenderer muzzleFlash;
    public AudioSource localFireAudio;

    private void Start()
    {
        NetworkObject playerId = NetworkSpawnManager.SpawnedObjects[pId.Value];

        if (playerId != null)
        {
            player = playerId.GetComponent<Transform>();
        }
        else
        {
            Debug.LogError("Player Id could not be found");
        }

        if (!playerOneGun.Value)
        {
            transform.position = new Vector2(6.6f,0f);
        }

        localFireAudio = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {

        if (!switched && !playerOneGun.Value)
        {
            transform.position = new Vector2(6.6f, 0f);
            transform.localScale = new Vector3(-1, 1, 1);
            NetworkObject playerId = NetworkSpawnManager.SpawnedObjects[pId.Value];
            player = playerId.GetComponent<Transform>();
            switched = true;
        }

        if (IsOwner)
        {
            if (playerOneGun.Value)
            {
                float yPos = Camera.main.ScreenToWorldPoint(Input.mousePosition).y;

                // Mirror position on y-axis with the mouse but clamp it
                transform.position = new Vector3(transform.position.x, Mathf.Clamp(yPos, player.position.y - 1.5f, player.position.y + 1.5f), 0);

                // Rotates the gun towards the camera (hopefully)
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2 direction = mousePos - transform.position;

                float angle = Vector2.SignedAngle(Vector2.right, direction);
                transform.eulerAngles = new Vector3(0, 0, angle);
            }
            else if (!playerOneGun.Value)
            {
                float yPos = Camera.main.ScreenToWorldPoint(Input.mousePosition).y;

                // Mirror position on y-axis with the mouse but clamp it
                transform.position = new Vector3(transform.position.x, Mathf.Clamp(yPos, player.position.y - 1.5f, player.position.y + 1.5f), 0);

                // Rotates the gun towards the camera (hopefully)
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2 direction = mousePos - transform.position;

                float angle = Vector2.SignedAngle(Vector2.left, direction);
                //angle *= -1f;
                transform.eulerAngles = new Vector3(0, 0, angle);
            }
        }
    }

    [ClientRpc]
    public void ActivateMuzzleFlashClientRpc()
    {
        localFireAudio.Play();
        muzzleFlash.enabled = true;
        StartCoroutine(DisableMuzzleFlash());
    }

    IEnumerator DisableMuzzleFlash()
    {
        yield return new WaitForSecondsRealtime(0.1f);
        muzzleFlash.enabled = false;
    }
}
