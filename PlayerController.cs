using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
//CROSS_PLATFORM_INPUT;MOBILE_INPUT
public class PlayerController : MonoBehaviour {

    ///* Instatiate Gameobjects *///
    [HideInInspector] public Rigidbody2D myRigidBody;
    private BoxCollider2D myFeetCollider;
    private SpriteRenderer myRenderer;
    [HideInInspector] public Animator myAnim;
    [HideInInspector] public AudioSource myAudioSource;
    [HideInInspector] public GameSession gameSession;

    [HideInInspector] public int direction;

    [SerializeField] GameObject bullet; // creates bullet
    [SerializeField] GameObject shotgunBullet; // creates shotgun bullet
    [SerializeField] GameObject shell; // creates shell
    [SerializeField] GameObject shotgunShell; // creates shotgun shell
    [SerializeField] GameObject fireDust; // creates fire dust effect 
    [SerializeField] GameObject shotgunFireDust; // creates shotgun fire dust effect 
    [SerializeField] GameObject footDust; // creates foot dust effect 
    [SerializeField] GameObject bloodEffect; // creates blood effect
    private SkeletonController skeletonEnemy;
    private SpearSkeletonController spearSkeletonEnemy;
    private BaseballHitSfx baseballHitSfxGO;

    //* Player values *//
    [SerializeField] float speed;
    [HideInInspector] public bool dashSpeedAllow = false;
    [HideInInspector] public float playerMove;

    private float firstBloodValue = 0.2f;
    private float jumpSpeed = 6.1f;
    bool playerIsMoving;
    private int myJumps = 0;
    private bool attackRateCheck = true;
    private bool rotaryObtacleAllowDamage = true;
    private bool avoidBulletRep = true;
    [HideInInspector] public bool isAllowAttack = false;
    [HideInInspector] public bool softDamageAllow = true;
    [HideInInspector] public bool allowDash = true;
    ///

    //* Sounds *//
    [SerializeField] AudioClip playerPain;
    [SerializeField] AudioClip bloodSquirting;
    [SerializeField] AudioClip playerPainSoft;
    [SerializeField] AudioClip jumpSwing;
    [SerializeField] AudioClip baseballSwing;
    [SerializeField] AudioClip emptyGun;
    [SerializeField] AudioClip jumpLand;
    [SerializeField] AudioClip shotgunPump;
    [SerializeField] AudioClip dashSfx;
    ///

    // Use this for initialization
    void Start () {
        myRigidBody = GetComponent<Rigidbody2D>();
        myAnim = GetComponent<Animator>();
        myFeetCollider = GetComponent<BoxCollider2D>();
        myAudioSource = GetComponent<AudioSource>();
        myRenderer = GetComponent<SpriteRenderer>();

        if (gameSession.playerSpawnPos.x != 0 && gameSession.playerSpawnPos.y != 0)
            transform.position = gameSession.playerSpawnPos;
        else
            transform.position = gameSession.playerDefaultPos;

        baseballHitSfxGO = transform.GetChild(1).GetComponent<BaseballHitSfx>();
    }

    // Update is called once per frame
    void Update () {
        if(!gameSession.isPlayerAlive) {
            myRigidBody.velocity = new Vector2(0f, 0f);
            myAnim.SetFloat("VelocityY", myRigidBody.velocity.y);
            myRigidBody.gravityScale = 10;
            return;
         }
        myAnim.SetFloat("VelocityY", myRigidBody.velocity.y);

        if (gameSession != null)
        {
            Movement();
            FlipPlayer();
            Jump();
            CheckIfPlayerIsAlive();
            Attack();

            //Shooting();
            Dash();
        }
        else
        {
            gameSession = FindObjectOfType<GameSession>();
        }
    }

    private void Movement()
    {
        myAnim.SetBool("Running", false);

        //if (!myAnim.GetCurrentAnimatorStateInfo(0).IsName("Dash"))
        //{
        //    //***NOTE!: Set speed of player to 2.92f.
        //    playerMove = CrossPlatformInputManager.GetAxis("Horizontal");
        //}

        if (!myAnim.GetCurrentAnimatorStateInfo(0).IsName("Dash"))
        {
            //***NOTE!: Set speed of player to 2.75f.
            playerMove = gameSession.DirectionIsReady();
        }

        if (playerMove > 0)
        {
            gameSession.MoveRightBigSize();
        }
        else if (playerMove < 0)
        {
            gameSession.MoveLeftBigSize();
        }
        else if(playerMove == 0)
        {
            gameSession.MoveBothSmall();
        }

        playerIsMoving = (Mathf.Abs(myRigidBody.velocity.x)) > 0;

        myRigidBody.velocity = new Vector2(playerMove * speed, myRigidBody.velocity.y);
        myAnim.SetBool("Running", playerIsMoving);
    }

    public void Jump()
    {
        if ( myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Ground")) &&
           myRigidBody.velocity.y == 0)
        {
            gameSession.JumpSmallContr();

            myAnim.SetBool("IsJumping", false);
            myJumps = 0;
        }

        if (CrossPlatformInputManager.GetButtonDown("Jump") && myJumps < 2)
        {
            gameSession.JumpBigContr();

            AudioSource.PlayClipAtPoint(jumpSwing, transform.position);
            myAnim.SetBool("IsJumping", true);
            myJumps++;

            Vector2 jumpVelocityToAdd = new Vector2(0f, jumpSpeed);
            myRigidBody.velocity = jumpVelocityToAdd;
        }
    }

    public void Shooting()
    {
        //if (Input.GetKeyDown(KeyCode.X))
        //{
            InvokeRepeating("Fire", 0f, 0.68f);
        //}
        //else if (Input.GetKeyUp(KeyCode.X))
        //{
        //    CancelFire();
        //}
    }

    private void Attack()
    {
        if (!gameSession.isPlayerAlive) { return; }

            if (CrossPlatformInputManager.GetButtonDown("Fire1") && attackRateCheck && !myAnim.GetBool("IsDashing"))
            {
                gameSession.AttackBigContr();
                myAnim.SetBool("IsAttacking", true);
                Invoke("CancelAttacking", 0.4f);
                attackRateCheck = false;
                Invoke("SetAttackRateCheckBack", gameSession.baseballAttackSpeed);
            }
    }

    private void SetAttackRateCheckBack()
    {
        gameSession.AttackSmallContr();
        attackRateCheck = true;
    }

    private void Dash()
    {
        if (!gameSession.isPlayerAlive) { return; }

        if( (CrossPlatformInputManager.GetButtonDown("Fire2") || Input.GetKey(KeyCode.C)) && allowDash && gameSession.isRefilledDash
            && !myAnim.GetBool("IsAttacking"))
        {
            gameSession.DashBigContr();

            allowDash = false;
            gameSession.isRefilledDash = false;
            gameSession.EnergyBarDecreaserHandler();
            myAnim.SetBool("IsDashing", true);

            Invoke("CancelDash", 1.3f);
        }

        DashSpeed();
    }

    private void DashSpeed()
    {
        if (dashSpeedAllow)
        {
            if (transform.localScale.x == 1)
            {
                 myRigidBody.velocity = new Vector2(0f, 0f);
                 myRigidBody.AddForce(new Vector2(4, 0f), ForceMode2D.Impulse);
            }
            else
            {
                myRigidBody.velocity = new Vector2(0f, 0f);
                myRigidBody.AddForce(new Vector2(-4, 0f), ForceMode2D.Impulse);
            }
        }
    }

    private void PlayDashSfx()
    {
        myAudioSource.clip = dashSfx;
        myAudioSource.volume = 0.8f;
        myAudioSource.Play();
    }

    private void SetActiveDashSpeed()
    {
        dashSpeedAllow = true;
    }

    private void SetDeactiveDashSpeed()
    {
        dashSpeedAllow = false;
    }

    private void CancelDash()
    {
        myAnim.SetBool("IsDashing", false);
        allowDash = true;
      //  gameSession.DashSmallContr();
    }

    private void PlayBaseballSwing()
    {
        myAudioSource.clip = baseballSwing;
        myAudioSource.volume = 0.2f;
        myAudioSource.Play();
    }

    private void CancelAttacking()
    {
        myAnim.SetBool("IsAttacking", false);
    }

    private void Fire()
    {
        if(!gameSession.isPlayerAlive) { return; }

        if (gameSession.gunAllow)
        { // Check if player holds gun

            gameSession.AimBigContr();

            if (gameSession.pistolAmmo > 0 && avoidBulletRep)
            {
                avoidBulletRep = false;
                gameSession.DecreasePistolAmmo();

                // Check if player is running or standing while shooting
                if (myRigidBody.velocity.x == 0)
                {
                    myAnim.SetBool("Shooting", true);
                    myAnim.SetBool("ShootingWR", false);
                }
                else
                {
                    myAnim.SetBool("ShootingWR", true);
                    myAnim.SetBool("Shooting", false);
                }

                Vector2 bulPos;
                Vector2 shellPos;
                Vector2 fireDustPos;
                if (transform.localScale.x == 1)
                {
                    bulPos = new Vector2(transform.position.x + 0.20f, transform.position.y - 0.1f);
                    fireDustPos = new Vector2(transform.position.x + 0.23f, transform.position.y + 0.07f);
                    shellPos = new Vector2(transform.position.x + 0.13f, transform.position.y - 0.44f);
                }
                else
                {
                    bulPos = new Vector2(transform.position.x - 0.20f, transform.position.y - 0.1f);
                    fireDustPos = new Vector2(transform.position.x - 0.23f, transform.position.y + 0.07f);
                    shellPos = new Vector2(transform.position.x - 0.13f, transform.position.y - 0.44f);
                }

                Instantiate(bullet, bulPos, Quaternion.identity);
                Instantiate(fireDust, fireDustPos, Quaternion.identity);
                Instantiate(shell, shellPos, Quaternion.identity);

                Invoke("SetBackBulletRep", 0.3f);
            }
            else
            {
                myAnim.SetBool("ShootingWR", false);
                myAnim.SetBool("Shooting", false);

                if(gameSession.pistolAmmo <= 0)
                {
                    myAudioSource.clip = emptyGun;
                    myAudioSource.volume = 0.2f;
                    myAudioSource.Play();
                    /// Change color of pistol ammo text
                    gameSession.ChangeColorForPistolAmmo();
                }
            }

        } // Pistol shooting check ends here
          // Shotgun check begins
        else if (gameSession.shotgunAllow)
        {
                gameSession.AimBigContr();

                if (gameSession.shotgunAmmo > 0 && avoidBulletRep)
                {
                    avoidBulletRep = false;
                    gameSession.DecreaseShotgunAmmo();

                    // Check if player is running or standing while shooting
                    if (myRigidBody.velocity.x == 0)
                    {
                        myAnim.SetBool("ShotgunShooting", true);
                        myAnim.SetBool("ShotgunShootingWR", false);
                    }
                    else
                    {
                        myAnim.SetBool("ShotgunShootingWR", true);
                        myAnim.SetBool("ShotgunShooting", false);
                    }

                Vector2 shotgunBulPos;
                    Vector2 shellPos;
                    Vector2 fireDustPos;
                    if (transform.localScale.x == 1)
                    {
                        shotgunBulPos = new Vector2(transform.position.x + 0.41f, transform.position.y - 0.1f);
                        fireDustPos = new Vector2(transform.position.x + 0.41f, transform.position.y + 0.07f);
                        shellPos = new Vector2(transform.position.x + 0.13f, transform.position.y - 0.44f);
                    }
                    else
                    {
                        shotgunBulPos = new Vector2(transform.position.x - 0.41f, transform.position.y - 0.1f);
                        fireDustPos = new Vector2(transform.position.x - 0.41f, transform.position.y + 0.07f);
                        shellPos = new Vector2(transform.position.x - 0.13f, transform.position.y - 0.44f);
                    }

                    Instantiate(shotgunBullet, shotgunBulPos, Quaternion.identity);
                    Instantiate(shotgunFireDust, fireDustPos, Quaternion.identity);
                    Instantiate(shotgunShell, shellPos, Quaternion.identity);

                    Invoke("SetBackBulletRep", 0.3f);
            }
                else
                {
                    myAnim.SetBool("ShotgunShootingWR", false);
                    myAnim.SetBool("ShotgunShooting", false);

                    if (gameSession.shotgunAmmo <= 0)
                    {
                    myAudioSource.clip = emptyGun;
                    myAudioSource.volume = 0.2f;
                    myAudioSource.Play();
                    /// Change color of shotgun ammo text
                    gameSession.ChangeColorForShotgunAmmo();
                    }
                }
        }
    }

    private void SetBackBulletRep()
    {
        avoidBulletRep = true;
    }

    public void CancelFire()
    {
        gameSession.AimSmallContr();

        CancelInvoke("Fire");
        myAnim.SetBool("Shooting", false);
        myAnim.SetBool("ShootingWR", false);
        myAnim.SetBool("ShotgunShooting", false);
        myAnim.SetBool("ShotgunShootingWR", false);
    }

    private void FlipPlayer()
    {
        playerIsMoving = (Mathf.Abs(myRigidBody.velocity.x)) > 0;
        if (playerIsMoving)
        {
            transform.localScale = new Vector2(Mathf.Sign(myRigidBody.velocity.x), 1f);
        }
    }

    private void CheckIfPlayerIsAlive()
    {
        if (gameSession.health < 0)
        {
            gameSession.isPlayerAlive = false;
            myAnim.SetBool("Running", false);
            myAnim.SetBool("IsJumping", false);
            dashSpeedAllow = false;
            myAnim.SetBool("IsDashing", false);
            myAnim.SetTrigger("Die");
            gameSession.GetReviveOrDie();
        }
    }

    public void GetDamage()
    {
        CancelInvoke("GetDamage");

        if (gameSession.health >= 0 && skeletonEnemy.isEnemyAlive)
        {
        Vector2 bloodPos = new Vector2(transform.position.x, transform.position.y + 0.23f);
        Instantiate(bloodEffect, bloodPos, Quaternion.identity); // instantiates blood effect
        myRenderer.color = new Color(0.6f, 0f, 0f);
        Vector3 playerCurPos = new Vector3(transform.position.x, transform.position.y, -9f);
        AudioSource.PlayClipAtPoint(playerPain, playerCurPos);
        gameSession.GetDamageGameSession();
        Invoke("ChangeColorOfPlayer", 0.12f);
        firstBloodValue = 0.65f;
        gameSession.SetActiveDamageEffectPanel();
        }
    }

    private void GetDamageForMud(Color32 objColor)
    {
        if (gameSession.health >= 0)
        {
            Vector2 bloodPos = new Vector2(transform.position.x, transform.position.y + 0.23f);
            Instantiate(bloodEffect, bloodPos, Quaternion.identity); // instantiates blood effect
            myRenderer.color = objColor;
            myAudioSource.clip = bloodSquirting; 
            myAudioSource.volume = 0.2f;
            myAudioSource.Play();
            if (gameSession.isPlayerAlive)
            {
                /// Create soft grunt sfx
                Vector3 playerCurPos = new Vector3(transform.position.x, transform.position.y, -9f);
                AudioSource.PlayClipAtPoint(playerPainSoft, playerCurPos);
            }
            gameSession.GetDamageGameSession();
            Invoke("ChangeColorOfPlayer", 0.3f);
            gameSession.SetActiveDamageEffectPanel();
            Invoke("SetActivateSoftDamage", 0.1f);
        }
    }

    private void GetSpearDamage()
    {
        if (gameSession.health >= 0)
        {
            Vector2 bloodPos = new Vector2(transform.position.x, transform.position.y + 0.23f);
            Instantiate(bloodEffect, bloodPos, Quaternion.identity); // instantiates blood effect
            myRenderer.color = new Color(0.6f, 0f, 0f);
            myAudioSource.clip = playerPain;
            myAudioSource.volume = 1f;
            if (!myAudioSource.isPlaying)
                 myAudioSource.Play();
            gameSession.GetDamageGameSession();
            Invoke("ChangeColorOfPlayer", 0.12f);
            firstBloodValue = 0.65f;
            gameSession.SetActiveDamageEffectPanel();
        }
    }

    public void GetSoftDamage()
    {
        if (gameSession.health >= 0)
        {
            Vector2 bloodPos = new Vector2(transform.position.x, transform.position.y + 0.23f);
            Instantiate(bloodEffect, bloodPos, Quaternion.identity); // instantiates blood effect
            myRenderer.color = new Color(0.6f, 0f, 0f);
            myAudioSource.clip = bloodSquirting;
            myAudioSource.volume = 0.2f;
            myAudioSource.Play();
            if(gameSession.isPlayerAlive)
            {
                /// Create soft grunt sfx
                Vector3 playerCurPos = new Vector3(transform.position.x, transform.position.y, -9f);
                AudioSource.PlayClipAtPoint(playerPainSoft, playerCurPos);
            }
            gameSession.GetDamageGameSession();
            Invoke("ChangeColorOfPlayer", 0.3f);
            gameSession.SetActiveDamageEffectPanel();
        }
    }

    public void GetKingDashDamage()
    {
        if (gameSession.health > 0)
        {
            Vector2 bloodPos = new Vector2(transform.position.x, transform.position.y + 0.23f);
            Instantiate(bloodEffect, bloodPos, Quaternion.identity); // instantiates blood effect
            myRenderer.color = new Color(0.6f, 0f, 0f);
            myAudioSource.clip = bloodSquirting;
            myAudioSource.volume = 0.2f;
            myAudioSource.Play();
            if (gameSession.isPlayerAlive)
            {
                /// Create soft grunt sfx
                Vector3 playerCurPos = new Vector3(transform.position.x, transform.position.y, -9f);
                AudioSource.PlayClipAtPoint(playerPainSoft, playerCurPos);
            }
            gameSession.GetKingDamGameSession();
            Invoke("ChangeColorOfPlayer", 0.3f);
            gameSession.SetActiveDamageEffectPanel();
        } else
        {
            KillPlayer();
        }
    }

    public void KillPlayer()
    {
        if (gameSession.isPlayerAlive)
        {
            Vector2 bloodPos = new Vector2(transform.position.x, transform.position.y + 0.23f);
            Instantiate(bloodEffect, bloodPos, Quaternion.identity); // instantiates blood effect
            myRenderer.color = new Color(0.6f, 0f, 0f);
            myAudioSource.clip = playerPain;
            myAudioSource.volume = 1f;
            myAudioSource.Play();
            gameSession.KillPlayer();
            Invoke("ChangeColorOfPlayer", 0.3f);
            gameSession.SetActiveDamageEffectPanel();
        }
    }

    public void PlayerSeaKiller()
    {
        if (gameSession.isPlayerAlive)
        {
            GetComponent<SpriteRenderer>().color = new Color(0.6f, 0f, 0f);
            myAudioSource.clip = playerPain;
            myAudioSource.volume = 1f;
            myAudioSource.Play();
            gameSession.KillPlayer();
            Invoke("ChangeColorOfPlayer", 0.3f);
            gameSession.SetActiveDamageEffectPanel();
        }
    }

    private void ChangeColorOfPlayer()
    {
        myRenderer.color = Color.white;
    }

    private void PlayShotgunPumpSfx()
    {
        myAudioSource.clip = shotgunPump;
        myAudioSource.volume = 0.35f;
        myAudioSource.Play();
    }

    private void OnTriggerEnter2D(Collider2D coll)
    {
        if(coll.CompareTag("Enemy"))
        skeletonEnemy = coll.gameObject.GetComponent<SkeletonController>();

        if (coll.CompareTag("SpearEnemy"))
        spearSkeletonEnemy = coll.gameObject.GetComponent<SpearSkeletonController>();

        isAllowAttack = false;

        /// Enemy weapon attack
        if(coll.CompareTag("Spear") && gameSession.isPlayerAlive && !myAnim.GetCurrentAnimatorStateInfo(0).IsName("Dash")) {
            GetSpearDamage();
            Destroy(coll.gameObject);
        }

        /// Checkpoint
        if(coll.CompareTag("Checkpoint"))
        {
            if(gameSession != null)
            {
                gameSession.playerSpawnPos = coll.transform.position;
            } else
            {
                FindObjectOfType<GameSession>().playerSpawnPos = coll.transform.position;
            }
        }

        /// Rotary
        if(coll.CompareTag("RotaryObstacle") && rotaryObtacleAllowDamage && gameSession.isPlayerAlive)
        {
            rotaryObtacleAllowDamage = false;
            GetSoftDamage();
        }

        if (coll.CompareTag("RotaryKiller") && gameSession.isPlayerAlive)
        {
            KillPlayer();
        }

        /// Body coll
        if (coll.CompareTag("BodyColl") && softDamageAllow && !myAnim.GetCurrentAnimatorStateInfo(0).IsName("Dash")) {
            softDamageAllow = false;
            GetSoftDamage();
        }

        /// Sea killer
        if(coll.CompareTag("SeaKiller"))
        {
            PlayerSeaKiller();
        }

        /// Mud
        if (coll.CompareTag("Mud") && softDamageAllow)
        {
            softDamageAllow = false;
            GetDamageForMud(new Color(0.18f, 0.113f, 0.058f));
        }

        /// Rock
        if (coll.CompareTag("Rock") && softDamageAllow)
        {
            softDamageAllow = false;
            GetDamageForMud(new Color(0.38f, 0.388f, 0.388f));
        }
    }

    private void OnTriggerStay2D(Collider2D coll)
    {
        if (coll.CompareTag("Enemy"))
        skeletonEnemy = coll.gameObject.GetComponent<SkeletonController>();

        if (coll.CompareTag("SpearEnemy"))
        spearSkeletonEnemy = coll.gameObject.GetComponent<SpearSkeletonController>();

        if (!gameSession.isPlayerAlive) { return; }

        /// Skeleton enemies
        if (coll.CompareTag("Enemy") && gameSession.health >= 0 && skeletonEnemy.isEnemyAlive &&
            skeletonEnemy.enemyAnim.GetCurrentAnimatorStateInfo(0).IsName("Attacking"))
        {
            Invoke("GetDamage", firstBloodValue);
        }

        /// Spear Skeleton enemies
        if (coll.CompareTag("SpearEnemy") && gameSession.health >= 0 && spearSkeletonEnemy.isEnemyAlive &&
         spearSkeletonEnemy.enemyAnim.GetCurrentAnimatorStateInfo(0).IsName("Attacking"))
        {
            Invoke("GetDamage", firstBloodValue);
        }
    }

    public void PlayBaseballHit()
    {
        baseballHitSfxGO.BaseballHitPlayer();
    }

    public void PlayBaseballHitTwo()
    {
        baseballHitSfxGO.BaseballHitTwoPlayer();
    }

    public void AllowAttack()
    {
        isAllowAttack = true;
    }

    private void OnTriggerExit2D(Collider2D coll)
    {
        if (coll.CompareTag("Enemy"))
        {
            CancelInvoke("GetDamage");
            firstBloodValue = 0.2f;
            myRenderer.color = Color.white;
        }

        if (coll.CompareTag("RotaryObstacle"))
        {
            rotaryObtacleAllowDamage = true;
        }

        // Body Coll
        if (coll.CompareTag("BodyColl"))
        {
            softDamageAllow = true;
        }
    }

    public void SetActivateSoftDamage()
    {
        softDamageAllow = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Create jump land sound
        Vector3 playerCurPos = new Vector3(transform.position.x, transform.position.y, -6f);
        AudioSource.PlayClipAtPoint(jumpLand, playerCurPos);

        Vector2 footDustPos = new Vector2(transform.position.x, transform.position.y - 0.39f);
        Instantiate(footDust, footDustPos, Quaternion.identity);
    }
}
