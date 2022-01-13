using UnityEngine;
using MLAPI;
using MLAPI.Messaging;

public class Bullet : NetworkBehaviour
{
    private Rigidbody2D rb;

    private float lifetime = 3f;
    private float timer = 0f;

    public AudioSource bounceSfx;

    float rotate;

    public Player hitPlayer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        Vector2 v = rb.velocity;
        var angle = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void Update()
    {
        if (IsServer)
        {
            Vector2 v = rb.velocity;
            rotate = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
        }
        transform.rotation = Quaternion.AngleAxis(rotate, Vector3.forward);

        timer += Time.deltaTime;

        if (timer >= lifetime && NetworkManager.Singleton.IsHost)
        {
            if (!IsOwner) { return; }
            DestroyBulletServerRpc();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        bounceSfx.Play();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            hitPlayer = collision.GetComponent<Player>();
            HitPlayerServerRpc();
            DestroyBulletServerRpc();
        }
    }

    [ServerRpc]
    private void HitPlayerServerRpc()
    {
        hitPlayer.hit.Value = true;
    }

    [ServerRpc]
    private void DestroyBulletServerRpc()
    {
        Destroy(gameObject);
    }

}
