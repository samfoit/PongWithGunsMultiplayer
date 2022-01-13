using UnityEngine;
using MLAPI;
using MLAPI.Messaging;

public class Ball : NetworkBehaviour
{
    public float speed;
    public Rigidbody2D rb;

    public AudioSource bounceSfx;
    public AudioSource scoreSfx;

    // Start is called before the first frame update
    void Start()
    {
        Launch();
    }

    private void Launch()
    {
        float x = Random.Range(0, 2) == 0 ? -1 : 1;
        float y = Random.Range(0, 2) == 0 ? -1 : 1;

        rb.velocity = new Vector2(speed * x, speed * y);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        bounceSfx.Play();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        scoreSfx.Play();
        if (!IsOwner) { return; }
        if (collision.tag == "P1Goal")
        {
            // Player 2 has scored
            GameManager.instance.p2Score.Value += 1;
            if (GameManager.instance.p2Score.Value >= 11 && GameManager.instance.p2Score.Value >= GameManager.instance.p1Score.Value + 2)
            {
                GameManager.instance.GameOver(false);
            }
            DestroyBall();
        }
        if (collision.tag == "P2Goal")
        {
            // Player 1 has scored
            GameManager.instance.p1Score.Value += 1;
            if (GameManager.instance.p1Score.Value >= 11 && GameManager.instance.p1Score.Value >= GameManager.instance.p2Score.Value + 2)
            {
                GameManager.instance.GameOver(true);
            }
            DestroyBall();
        }
    }

    public void DestroyBall()
    {
        if (!IsOwner) { return; }
        DestroyBallServerRpc();
        GameManager.instance.SpawnBallServerRpc();
    }

    [ServerRpc]
    public void DestroyBallServerRpc()
    {
        Destroy(gameObject);
    }
}
