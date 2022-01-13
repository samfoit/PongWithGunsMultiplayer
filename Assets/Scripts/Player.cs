using UnityEngine;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;

public class Player : NetworkBehaviour
{
    [SerializeField] private float speed;

    public Rigidbody2D rb;
    private float movement;

    public GameObject bulletPrefab;
    public NetworkObject gunPrefab;

    public Transform firePoint;

    private float fireDelta = 0.5f;
    private float fireTimer = 0f;

    [SerializeField] private int fireCount = 3;
    private float reloadTimer = 0f;
    private float reloadDelta = 3f;
    private bool reloading = false;

    // Might need to make this networked var
    // On value change you can set the players animator
    public NetworkVariable<bool> hit = new NetworkVariable<bool>(new NetworkVariableSettings { WritePermission = NetworkVariablePermission.Everyone, ReadPermission= NetworkVariablePermission.Everyone }, false);
    public bool alive = true;

    public AudioSource hitSfx;

    private void Start()
    {
        if (IsHost)
        {
            SpawnGunServerRpc(true);
        }
        else
        {
            transform.position = new Vector3(8, 0, 0);
            SpawnGunServerRpc(false);
        }
    }


    private void Update()
    {
        if (!IsOwner) { return; }

        if (Time.timeScale == 0) { return; }

        if (alive)
        {
            // TODO: Check if you own this object so you don't move the other player
            movement = Input.GetAxisRaw("Vertical");

            rb.velocity = new Vector2(rb.velocity.x, movement * speed);

            fireTimer += Time.deltaTime;

            if (Input.GetButton("Fire1") && fireTimer > fireDelta)
            {
                if (reloading)
                {
                    return;
                }
                else
                {
                    ShootServerRpc(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                    FindObjectOfType<ScreenShakeController>().StartShake(0.25f, 0.2f);

                    fireCount--;
                    if (fireCount <= 0)
                    {
                        reloading = true;
                    }

                    fireTimer = 0f;
                }
            }

            if (reloading)
            {
                reloadTimer += Time.deltaTime;

                if (reloadTimer >= reloadDelta)
                {
                    fireCount = 3;
                    reloadTimer = 0f;
                    reloading = false;
                }
            }
        }
        if (hit.Value)
        {
            alive = false;

            GetComponent<Animator>().SetBool("hit", hit.Value);
            // TODO: Play sound fx and set hit to false on end
            hit.Value = false;
        }

    }

    public void HitSfx()
    {
        hitSfx.Play();
    }

    public void HitAnimationEnd()
    {
        hit.Value = false;
        GetComponent<Animator>().SetBool("hit", hit.Value);
    }

    public void Respawn()
    {
        alive = true;
    }

    [ServerRpc]
    public void ShootServerRpc(Vector3 rotate)
    {
        //...setting shoot direction
        Vector3 shootDirection = rotate;
        shootDirection.z = 0;
        shootDirection -= firePoint.position;

        //...instantiating the bullet
        Rigidbody2D bulletInstance = Instantiate(bulletPrefab, new Vector3(firePoint.position.x, firePoint.position.y, 0),
            Quaternion.identity).GetComponent<Rigidbody2D>();
        bulletInstance.velocity = new Vector2(shootDirection.x, shootDirection.y);
        bulletInstance.velocity = bulletInstance.velocity.normalized * 12f;
        bulletInstance.transform.rotation = Quaternion.AngleAxis(Mathf.Atan2(bulletInstance.velocity.y, bulletInstance.velocity.x) * Mathf.Rad2Deg, Vector3.forward);
        bulletInstance.GetComponent<NetworkObject>().Spawn();
        firePoint.GetComponentInParent<Gun>().ActivateMuzzleFlashClientRpc();
    }

    [ServerRpc]
    private void SpawnGunServerRpc(bool playerOneGun)
    {
        NetworkObject gun = Instantiate(gunPrefab);
        gun.GetComponent<Gun>().pId.Value = GetComponent<NetworkObject>().NetworkObjectId;
        gun.GetComponent<Gun>().playerOneGun.Value = playerOneGun;
        firePoint = gun.GetComponent<Gun>().firePoint;
        gun.SpawnWithOwnership(OwnerClientId);
    }

    public void Disconnect()
    {
        if (IsHost)
        {
            Time.timeScale = 1;
            NetworkManager.Singleton.StopHost();
        }
        else if (IsClient)
        {
            NetworkManager.Singleton.StopClient();
        }
        else if (IsServer)
        {
            NetworkManager.Singleton.StopServer();
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene");
    }
}
