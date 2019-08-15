using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShooting : MonoBehaviour {

    [SerializeField] float speed = 8f;
    [SerializeField] GameObject collision;

    private PlayerController player;
    private float scaleValue;
    private BoxCollider2D bullCollider;

    //* Sounds *//
    [SerializeField] AudioClip fireSound;
    ///

    // Boundary values
    Vector2 min, max;

    void Start()
    {
        player = FindObjectOfType<PlayerController>();
        // Fire sound
        Vector3 playerCurPos = new Vector3(player.transform.position.x, player.transform.position.y, -6f);
        AudioSource.PlayClipAtPoint(fireSound, playerCurPos);

        scaleValue = player.transform.localScale.x;

        min = Camera.main.ViewportToWorldPoint(new Vector2(1, 1)); // top-right
        max = Camera.main.ViewportToWorldPoint(new Vector2(0, 0)); // bottom-left 

        bullCollider = GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update () {
        BulletMovement();
    }

    private void BulletMovement()
    {
        Vector2 pos = transform.position;

        if (scaleValue == 1)
        {
            pos = new Vector2(pos.x + speed * Time.deltaTime, pos.y);
        } else
        {
            pos = new Vector2(pos.x - speed * Time.deltaTime, pos.y);
        }

        transform.position = pos;

        if(pos.x > min.x + 3f || pos.x < max.x - 3f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.CompareTag("Walls"))
        {
            // Destroy bullet when hits the wall
            if (bullCollider.IsTouchingLayers(LayerMask.GetMask("Ground")))
            {
                CreateCollEffect();
            }
        }
    }

    private void CreateCollEffect()
    {
        // Create collision effect
        Instantiate(collision, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
