using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootTrapBehaviour : MonoBehaviour {

    [SerializeField] float speed = 1f;
    private Rigidbody2D trapRigidbody;
    private Animator trapAnimator;
    private bool allowRising;
    private PlayerController player;

    // Use this for initialization
    void Start () {
        trapRigidbody = GetComponent<Rigidbody2D>();
        trapAnimator = GetComponent<Animator>();
        allowRising = true;
        player = FindObjectOfType<PlayerController>();
    }

    private void Movement()
    {
        trapRigidbody.velocity = new Vector2(0f, speed);
        trapAnimator.SetBool("TrapOn", true);
        Destroy(transform.GetChild(0).gameObject);
    }

    private void PlayTrapSfx()
    {
        GetComponent<AudioSource>().Play();
    }


    private void OnTriggerEnter2D(Collider2D coll)
    {
        if(coll.CompareTag("Player") && allowRising)
        {
            allowRising = false;
            Movement();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Walls" && !allowRising)
        {
            Destroy(trapRigidbody);
            Destroy(GetComponent<CapsuleCollider2D>());
            player.SetActivateSoftDamage();
        }
    }
}
