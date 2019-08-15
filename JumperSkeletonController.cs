using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumperSkeletonController : MonoBehaviour {

    private Animator jumperAnim;
    private PlayerController player;
    private Rigidbody2D jumperRigidbody;

    [SerializeField] float health;
    [SerializeField] GameObject bones;
    [SerializeField] bool dontAllowAmmoCreation;
    private bool allowBullet = true;
    private bool dashAttackAllow = true;

    //** Sounds **//
    [SerializeField] AudioClip jumperDie;

    //* Pickups *//
    [SerializeField] GameObject pistolAmmoGO;
    [SerializeField] GameObject shotgunAmmoGO;
    [SerializeField] GameObject healthGO;

    // Use this for initialization
    void Start () {
        player = FindObjectOfType<PlayerController>();
        jumperAnim = GetComponent<Animator>();
        jumperRigidbody = GetComponent<Rigidbody2D>();

        if (player.transform.position.x > transform.position.x)
        {
            transform.localScale = new Vector2(-1f, 1f);
        }
    }

    private void CreateAmmoPickUp()
    {
        int randNum = Random.Range(1, 3);
        int healthRandNum = Random.Range(1, 7);

        if(randNum == 1)
        {
            Instantiate(pistolAmmoGO, transform.position, Quaternion.identity);
        } else if(randNum == 2)
        {
            Instantiate(shotgunAmmoGO, transform.position, Quaternion.identity);
        }  else
        {
            return;
        }

        if(healthRandNum == 3)
        {
            Vector2 newPos = new Vector2(transform.position.x + 0.3f, transform.position.y);
            Instantiate(healthGO, newPos, Quaternion.identity);
        }
    }

    private void KillJumper()
    {
        jumperRigidbody.velocity = new Vector2(0f, 0f);
        jumperRigidbody.bodyType = RigidbodyType2D.Kinematic;
        jumperRigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
        Destroy(GetComponent<CapsuleCollider2D>());

        AudioSource.PlayClipAtPoint(jumperDie, transform.position, 1f);
        Instantiate(bones, transform.position, Quaternion.identity);
        if(!dontAllowAmmoCreation)
        CreateAmmoPickUp(); // Create ammo

        Destroy(gameObject, 0.1f);
    }

    private void Attacking()
    {
        if(player.transform.position.x < transform.position.x)
        {
            transform.localScale = new Vector2(1f, 1f);
            jumperRigidbody.velocity = new Vector2(-3f, 4f);

        } else if(player.transform.position.x > transform.position.x)
        {
            transform.localScale = new Vector2(-1f, 1f);
            jumperRigidbody.velocity = new Vector2(3f, 4f);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
         if(collision.collider.CompareTag("Walls") || collision.collider.CompareTag("Bridge"))
         {
            jumperRigidbody.gravityScale = 0.8f;
            jumperAnim.enabled = true;
         }

        if (collision.collider.CompareTag("Player"))
        {
            Physics2D.IgnoreCollision(collision.collider, GetComponent<CapsuleCollider2D>());
        }
    }

    private void OnTriggerEnter2D(Collider2D coll)
    {
        if( (coll.CompareTag("Bullet") || coll.CompareTag("ShotgunBullet")) && allowBullet)
        {
            allowBullet = false;
            Destroy(coll.gameObject);
            KillJumper();
        }
    }

    private void OnTriggerStay2D(Collider2D coll)
    {
        if(coll.CompareTag("Player") && player.myAnim.GetCurrentAnimatorStateInfo(0).IsName("Attack") && player.isAllowAttack)
        {
            player.isAllowAttack = false;
            player.PlayBaseballHitTwo();
            KillJumper();
        }

        /// Dash
        if (coll.CompareTag("Player") && player.myAnim.GetCurrentAnimatorStateInfo(0).IsName("Dash") && dashAttackAllow)
        {
            dashAttackAllow = false;
            KillJumper();
        }
    }
}
