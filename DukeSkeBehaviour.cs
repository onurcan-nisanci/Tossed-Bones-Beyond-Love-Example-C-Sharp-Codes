using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DukeSkeBehaviour : MonoBehaviour {

    [SerializeField] GameObject bone;
    [SerializeField] GameObject knife;
    [SerializeField] GameObject bullColl;
    private PlayerController player;
    [HideInInspector] public GameSession gameSession;
    private AudioSource dukeAudioSource;
    private AudioSource jumpCollPlayer;
    private AudioSource crumblingSfxPlayer;
    private Animator dukeAnim;
    private Rigidbody2D dukeRb;
    private LevelHandler levelHandler;

    //* Boss values *//
    [SerializeField] float health;
    [SerializeField] float bulletDamage;
    [SerializeField] float shotgunDamage;
    [SerializeField] float baseballDamage;
    [SerializeField] DoorBehaviour steelDoorGO;
    [SerializeField] PlatformSectionBehaviour platformTrigger;
    [SerializeField] GameObject soilCollGO;
    private Animator platformSecAnim;
    private GameObject playerKillerGO;
    [SerializeField] GameObject bossHealthGO;
    [SerializeField] LevelPresents bossPresentsGO;
    private Image bossHealthBar;
    private Image bossHealthBarBG;
    private bool allowBulletDamage = true;
    private bool allowChangeDir = true;
    private bool allowCancelJump = false;
    private Vector2 startPos;

    private bool CheckIfAliveTrigger = true;
    private bool isDukeAlive = true;
    private bool allowSupportingPlayer;
    private float maxHealth = 100f;
    private float whiteValue = 1f;
    private float alphaValue = 1f;
    private bool allowHealthSender = true;
    private bool ammoHasTaken = false;

    private Vector2 ammoCreationPosMin;
    private Vector2 ammoCreationPosMax;

    //* Waves *//
    private bool allowFirstWave = true;
    private bool allowSecondWave = true;
    private bool allowThirdWave = true;

    //* Sounds *//
    [SerializeField] AudioClip bossColl;
    [SerializeField] AudioClip bossShotgunColl;
    [SerializeField] AudioClip bossDie;
    [SerializeField] AudioClip bossBoneBreak;
    [SerializeField] AudioClip coinInstantiateSfx;
    [SerializeField] AudioClip jewelryInstantiateSfx;
    [SerializeField] AudioClip shotgunInstantiateSfx;

    //* Pickups *//
    [SerializeField] GameObject coinGO;
    private int numberOfCoins = 20;
    [SerializeField] GameObject diamondGO;
    private int numberOfDiamonds = 40;
    [SerializeField] GameObject crownGO;
    private int numberOfCrowns = 33;
    [SerializeField] GameObject healthGO;
    private int numberOfHealth;
    [SerializeField] GameObject pistolAmmoGO;
    [SerializeField] GameObject superPistolAmmoGO;
    private int numberOfPistolAmmo;
    [SerializeField] GameObject shotgunAmmoGO;
    [SerializeField] GameObject superShotgunAmmoGO;
    private int numberOfShotgunAmmo = 3;
    [SerializeField] GameObject healthPackage;
    ///

    // Use this for initialization
    void Start () {
        player = FindObjectOfType<PlayerController>();
        gameSession = FindObjectOfType<GameSession>();
        dukeAudioSource = GetComponent<AudioSource>();
        jumpCollPlayer = transform.GetChild(3).GetComponent<AudioSource>();
        crumblingSfxPlayer = transform.GetChild(4).GetComponent<AudioSource>();
        playerKillerGO = transform.GetChild(5).gameObject;
        dukeAnim = GetComponent<Animator>();
        dukeRb = GetComponent<Rigidbody2D>();
        startPos = transform.position;
        allowSupportingPlayer = true;

        ammoCreationPosMin = transform.GetChild(0).transform.position;
        ammoCreationPosMax = transform.GetChild(1).transform.position;

        bossHealthGO.gameObject.SetActive(true);
        bossHealthBar = bossHealthGO.transform.GetChild(1).gameObject.GetComponent<Image>();
        bossHealthBarBG = bossHealthGO.transform.GetChild(0).gameObject.GetComponent<Image>();
        bossHealthBar.fillAmount = health / maxHealth;
        bossHealthBarBG.GetComponent<Image>().color = new Color(255f, 255f, 255f);

        platformSecAnim = platformTrigger.GetComponent<Animator>();
    }
	
	// Update is called once per frame
	void Update () {
        IsAlive();
        BossStates();

        if (gameSession.pistolAmmo == 0 && gameSession.shotgunAmmo == 0 && allowSupportingPlayer && isDukeAlive)
        {
            allowSupportingPlayer = false;
            StartCoroutine(SupportPlayer());
        }

        if(gameSession.pistolAmmo > 0 || gameSession.shotgunAmmo > 0)
        {
            allowSupportingPlayer = true;
        }

        /// Hide Boss health bar
        if (!gameSession.isPlayerAlive || gameSession.hearts == -1 || gameSession.health == -1)
        {
            dukeRb.constraints = RigidbodyConstraints2D.FreezePositionY;
            Time.timeScale = 1f;
        }
        /// Destroy both health if game is over
        if (gameSession.hearts < 0 && gameSession.health < 0)
        {
            Destroy(bossHealthGO);
        }
    }

    private void BossStates()
    {
        if (health <= 75 && allowFirstWave)
        {
            allowFirstWave = false;
            dukeAnim.SetBool("IsJumping", true);
            dukeAnim.SetBool("FirstWave", true);
        }
        else if (health <= 50 && !allowFirstWave && allowSecondWave)
        {
            allowSecondWave = false;
            dukeAnim.SetBool("IsJumping", true);
            dukeAnim.SetBool("FirstWave", false);
            dukeAnim.SetBool("SecondWave", true);
        }
        else if (health <= 20 && !allowFirstWave && !allowSecondWave && allowThirdWave)
        {
            allowThirdWave = false;
            dukeAnim.SetBool("IsJumping", true);
            dukeAnim.SetBool("FirstWave", false);
            dukeAnim.SetBool("SecondWave", false);
            dukeAnim.SetBool("ThirdWave", true);
        }
    }

    private void IsAlive()
    {
        if (health <= 0 && CheckIfAliveTrigger)
        {
            CheckIfAliveTrigger = false;
            Invoke("PauseBtnAndMusic", 1f);
            dukeAnim.SetBool("IsJumping", false);
            dukeRb.velocity = new Vector2(0, 0);
            dukeAnim.SetTrigger("Die");
            InvokeRepeating("ReduceAlpha", 0.4f, 0.1f);
            AudioSource.PlayClipAtPoint(bossDie, transform.position, 1f);
            dukeRb.constraints = RigidbodyConstraints2D.FreezePositionX;
            StartCoroutine(BreakBones());
            StartCoroutine(CreateCoins());
            InvokeRepeating("BlackoutHealthBar", 0.4f, 0.2f);
            if (bossPresentsGO != null)
            {
                bossPresentsGO.EndBossPresentation();
            }
            else
            {
                FindObjectOfType<LevelPresents>().EndBossPresentation();
            }
            Destroy(transform.GetChild(2).gameObject);
            Destroy(transform.GetChild(5).gameObject);
            Invoke("OpenDoor", 8f);
            Invoke("ExitLevel", 19.5f);
        }

        if (dukeAnim.GetCurrentAnimatorStateInfo(0).IsName("Die"))
        {
            isDukeAlive = false;
        }
        else
        {
            isDukeAlive = true;
        }
    }

    private void PauseBtnAndMusic()
    {
        if (gameSession.isPlayerAlive)
        {
            gameSession.HidePauseButton();
            gameSession.PauseMusicOutside();
        }
    }

    private void ReduceAlpha()
    {
        if (alphaValue >= 0f)
        {
            alphaValue -= 0.05f;
            GetComponent<SpriteRenderer>().color = new Color(255f, 255f, 255f, alphaValue);
        }
        else
        {
            CancelInvoke("ReduceAlpha");
        }
    }

    private IEnumerator SupportPlayer()
    {
        yield return new WaitForSeconds(5f);

        if (gameSession.pistolAmmo == 0 && gameSession.shotgunAmmo == 0 && isDukeAlive)
        {
            int ammoRand = Random.Range(1, 3);
            for (int i = 0; i < 2; i++)
            {
                float xPos = Random.Range(ammoCreationPosMin.x, ammoCreationPosMax.x);
                Vector2 newPos = new Vector2(xPos, ammoCreationPosMin.y);
                if (ammoRand == 1)
                    Instantiate(pistolAmmoGO, newPos, Quaternion.identity);
                else
                    Instantiate(shotgunAmmoGO, newPos, Quaternion.identity);

                yield return new WaitForSeconds(0.2f);
            }
        }
    }

    private IEnumerator ThrowAmmo()
    {
        int randNum = Random.Range(1, 9);

        switch (randNum)
        {
            case 1:
                {
                    Instantiate(superPistolAmmoGO, transform.position, Quaternion.identity);
                }
                break;
            case 2:
                {
                    Instantiate(superPistolAmmoGO, transform.position, Quaternion.identity);
                    yield return new WaitForSeconds(0.4f);
                    Instantiate(superPistolAmmoGO, transform.position, Quaternion.identity);
                }
                break;
            case 3:
                {
                    Instantiate(superShotgunAmmoGO, transform.position, Quaternion.identity);
                }
                break;
            case 4:
                {
                    Instantiate(superShotgunAmmoGO, transform.position, Quaternion.identity);
                    yield return new WaitForSeconds(0.4f);
                    Instantiate(superShotgunAmmoGO, transform.position, Quaternion.identity);
                }
                break;
            default:
                yield return null;
                break;
        }
    }

    private void SendHealth()
    {
        int randNum = Random.Range(1, 9);

        if(randNum == 3 && gameSession.health == 0)
        {
            Instantiate(healthPackage, transform.position, Quaternion.identity);
        }

        Invoke("SetBackHealthAllower", 1f);
    }

    private void SetBackHealthAllower()
    {
        allowHealthSender = true;
    }

    private void ThrowKnife()
    {
        knife.GetComponent<SpearBehaviour>().dukeBoss = GetComponent<DukeSkeBehaviour>();

        float additionY = Random.Range(-0.5f, 0.5f);

        Vector2 knifePos = new Vector2(transform.position.x - 0.1f, transform.position.y - additionY);
        Instantiate(knife, knifePos, Quaternion.identity);
    }

    private void ThrowDoubleKnife()
    {
        knife.GetComponent<SpearBehaviour>().dukeBoss = GetComponent<DukeSkeBehaviour>();

        float additionY = Random.Range(-0.5f, 0.5f);

        Vector2 knifePos = new Vector2(transform.position.x - 0.1f, transform.position.y - additionY);
        Vector2 knifePosTwo = new Vector2(transform.position.x - 0.1f, transform.position.y - (additionY - 0.275f) );

        Instantiate(knife, knifePos, Quaternion.identity);
        Instantiate(knife, knifePosTwo, Quaternion.identity);
    }

    private void ThrowTripleKnife()
    {
        knife.GetComponent<SpearBehaviour>().dukeBoss = GetComponent<DukeSkeBehaviour>();

        float additionY = Random.Range(-0.5f, 0.5f);

        Vector2 knifePos = new Vector2(transform.position.x - 0.1f, transform.position.y - additionY);
        Vector2 knifePosTwo = new Vector2(transform.position.x - 0.1f, transform.position.y - (additionY - 0.275f));
        Vector2 knifePosThree = new Vector2(transform.position.x - 0.1f, transform.position.y - (additionY + 0.275f));

        Instantiate(knife, knifePos, Quaternion.identity);
        Instantiate(knife, knifePosTwo, Quaternion.identity);
        Instantiate(knife, knifePosThree, Quaternion.identity);
    }

    private void StartJumping()
    {
        if(transform.localScale.x == 1f)
        {
            dukeRb.velocity = new Vector2(-3f, 4f);
        } else
        {
            dukeRb.velocity = new Vector2(3f, 4f);
        }
        playerKillerGO.gameObject.SetActive(false);
        dukeRb.constraints = RigidbodyConstraints2D.None;
        dukeRb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void StartDescending()
    {
        if (transform.localScale.x == 1f)
        {
            dukeRb.velocity = new Vector2(-1.5f, 0);
        }
        else
        {
            dukeRb.velocity = new Vector2(1.5f, 0);
        }
        dukeRb.gravityScale = 1f;
    }

    private void GetDamage(float damage, AudioClip enemyColl)
    {
        if (health > 0)
        {
            Vector2 bonePos = new Vector2(transform.position.x, transform.position.y + 0.4f);
            Instantiate(bone, bonePos, Quaternion.identity); // instantiates bone effect
            GetComponent<SpriteRenderer>().color = new Color(0.2f, 0.2f, 0.25f); // set dark gray color if enemy gets hit
            Invoke("SetColorBack", 0.2f);
            dukeAudioSource.clip = enemyColl;
            dukeAudioSource.Play();
            health -= damage;
            bossHealthBar.fillAmount = health / maxHealth;
            Invoke("SetBackAllowBullet", 0.1f);

            StartCoroutine(ThrowAmmo());
        }
    }

    private void SetBackAllowBullet()
    {
        allowBulletDamage = true;
    }

    private void SetColorBack()
    {
        GetComponent<SpriteRenderer>().color = Color.white;
    }

    private void BlackoutHealthBar()
    {
        if (whiteValue >= 0.35f)
        {
            whiteValue -= 0.05f;
            bossHealthBarBG.GetComponent<Image>().color = new Color(whiteValue, whiteValue, whiteValue);
            bossHealthGO.transform.GetChild(2).GetComponent<Image>().color = new Color(whiteValue, whiteValue, whiteValue);
            bossHealthGO.transform.GetChild(3).GetComponent<Image>().color = new Color(whiteValue, whiteValue, whiteValue);
            bossHealthGO.transform.GetChild(4).GetComponent<Image>().color = new Color(whiteValue, whiteValue, whiteValue);
        }
        else
        {
            CancelInvoke("BlackoutHealthBar");
        }
    }

    private IEnumerator BreakBones()
    {
        Time.timeScale = 0.4f;
        AudioSource.PlayClipAtPoint(bossBoneBreak, transform.position, 1f);
        Vector2 bonePos = new Vector2(transform.position.x, transform.position.y + 0.4f);
        Vector2 bonePosTwo = new Vector2(transform.position.x - 0.62f, transform.position.y + 0.96f);
        Instantiate(bone, bonePos, Quaternion.identity); // instantiates bone effect
        Instantiate(bone, bonePosTwo, Quaternion.identity); // instantiates bone effect

        yield return new WaitForSeconds(4f);
        Time.timeScale = 1f;
    }

    private IEnumerator CreateCoins()
    {
        yield return new WaitForSeconds(1f);

        dukeAudioSource.clip = coinInstantiateSfx;
        dukeAudioSource.volume = 0.7f;

        for (int i = 0; i < numberOfCoins; i++)
        {
            float xPos = Random.Range(-1f, 0.45f);
            float yPos = Random.Range(-0.5f, -1f);
            Vector2 coinPos = new Vector2(transform.position.x + xPos, transform.position.y - yPos);
            Instantiate(coinGO, coinPos, Quaternion.identity); // Create coin
            dukeAudioSource.Play();
            yield return new WaitForSeconds(0.04f);
        }
        dukeAudioSource.clip = jewelryInstantiateSfx;
        yield return new WaitForSeconds(0.6f);
        for (int i = 0; i < numberOfDiamonds; i++)
        {
            float xPos = Random.Range(-1.8f, 0.68f);
            float yPos = Random.Range(-0.5f, -1f);
            Vector2 diamondPos = new Vector2(transform.position.x + xPos, transform.position.y - yPos);
            Instantiate(diamondGO, diamondPos, Quaternion.identity); // Create diamond
            dukeAudioSource.Play();
            yield return new WaitForSeconds(0.08f);
        }
        yield return new WaitForSeconds(0.4f);
        for (int i = 0; i < numberOfCrowns; i++)
        {
            float xPos = Random.Range(-1.5f, 0.8f); // (-0.96f, 0.45f);
            float yPos = Random.Range(-0.5f, -1f);
            Vector2 crownPos = new Vector2(transform.position.x + xPos, transform.position.y - yPos);
            Instantiate(crownGO, crownPos, Quaternion.identity); // Create crown
            dukeAudioSource.Play();
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(0.3f);
        dukeAudioSource.clip = shotgunInstantiateSfx;
        for (int i = 0; i < numberOfShotgunAmmo; i++)
        {
            float xPos = Random.Range(-1.15f, 0.1f);
            float yPos = Random.Range(-0.5f, -1f);
            Vector2 bullPos = new Vector2(transform.position.x + xPos, transform.position.y - yPos);
            Instantiate(shotgunAmmoGO, bullPos, Quaternion.identity); // Create shotgun ammo
            dukeAudioSource.Play();
            yield return new WaitForSeconds(0.13f);
        }
    }

    private void OpenDoor()
    {
        steelDoorGO.openDoor = true;
    }

    private void ExitLevel()
    {
        gameSession.transform.GetChild(2).GetComponent<LevelHandler>().LevelAdmin();
    }

    private void CreateSoilColl()
    {
        Vector2 collPos = new Vector2(transform.position.x, transform.position.y - 0.48f);
        Instantiate(soilCollGO, collPos, Quaternion.identity);
    }

    private void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.CompareTag("Bullet") && allowBulletDamage)
        {
            allowBulletDamage = false;
            Destroy(coll.gameObject);
            GetDamage(bulletDamage, bossColl);
        }
        if (coll.CompareTag("ShotgunBullet") && allowBulletDamage)
        {
            allowBulletDamage = false;
            Destroy(coll.gameObject);
            GetDamage(shotgunDamage, bossShotgunColl);
        }

        if(coll.CompareTag("EnemyExitLeft") && allowChangeDir)
        {
            allowChangeDir = false;
            allowCancelJump = true;
            transform.localScale = new Vector2(-1f, 1f);
            Invoke("SetDirTrue", 2f);
        }
        if (coll.CompareTag("EnemyExitRight") && allowCancelJump)
        {
            allowCancelJump = false;
            dukeAnim.SetBool("IsJumping", false);
            transform.localScale = new Vector2(1f, 1f);
            dukeRb.velocity = new Vector2(0f, 0f);
            dukeRb.gravityScale = 0;
            transform.position = startPos;
            playerKillerGO.gameObject.SetActive(true);
            dukeRb.constraints = RigidbodyConstraints2D.FreezePositionX;
        }

        /// Attack with baseball bat
        if (coll.CompareTag("Player") && player.myAnim.GetCurrentAnimatorStateInfo(0).IsName("Attack") && player.isAllowAttack)
        {
            player.isAllowAttack = false;
            GetDamage(baseballDamage, bossColl);
            player.PlayBaseballHitTwo();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            Physics2D.IgnoreCollision(collision.collider, GetComponent<CapsuleCollider2D>());
        }

        if (dukeAnim.GetCurrentAnimatorStateInfo(0).IsName("Descend") && dukeAnim.GetBool("IsJumping") &&
            collision.collider.CompareTag("Walls"))
        {
            jumpCollPlayer.Play();
            platformSecAnim.SetBool("IsVibrationSection", true);
            crumblingSfxPlayer.Play();
            Invoke("SetVibrationOff", 1.1f);
            dukeRb.gravityScale = 0;
            Invoke("StartDescending", 0.5f);
            CreateSoilColl();

            if (allowHealthSender)
            SendHealth();
        }
    }

    private void SetDirTrue()
    {
        allowChangeDir = true;
    }

    private void SetVibrationOff()
    {
        platformSecAnim.SetBool("IsVibrationSection", false);
    }

    private void OnTriggerStay2D(Collider2D coll)
    {
            /// Attack with baseball bat
            if (coll.CompareTag("Player") && player.myAnim.GetCurrentAnimatorStateInfo(0).IsName("Attack") && player.isAllowAttack)
            {
                player.isAllowAttack = false;
                GetDamage(baseballDamage, bossColl);
                player.PlayBaseballHitTwo();
            }
    }
}
