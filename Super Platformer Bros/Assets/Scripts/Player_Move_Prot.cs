using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Player_Move_Prot : MonoBehaviour
{
    public GameObject MapManagerObj;
    private MapManager mapManager;

    public AudioClip snd_Shoot1;
    public AudioClip snd_Shoot2;
    private AudioSource audioSource_shoot;
    private AudioSource audioSource_melting;
    private AudioSource audioSource_charging;
    private AudioSource audioSource_blink;
    private AudioSource audioSource_phasewalk;

    bool crouching = false;
    private bool facingRight = true;
    private float moveX;
    Inventory inventory;
    DialogManager dialogManager;
    Animator anim;
    float energizeTime = 0;
    private bool _chargingBlink;
    private bool chargingBlink
    {
        get { return _chargingBlink; }
        set
        {
            _chargingBlink = value;
            var emiss = blinkParticleSystem.emission;
            emiss.enabled = value;
        }
    }
    private bool _phaseWalking;
    private bool phaseWalking
    {
        get { return _phaseWalking; }
        set
        {
            _phaseWalking = value;
            var emiss = phaseWalkParticleSystem.emission;
            emiss.enabled = value;
            if (value)
            {
                audioSource_phasewalk.Play();
            }
            else
            {
                audioSource_phasewalk.Stop();
            }
        }
    }
    private Rigidbody2D rigidBody;
    private float phaseWalkEnergyDrain;
    bool CanAttack = true;
    bool IsPointing = false;
    Coroutine pointWeaponCo;

    public GameObject ShotPrefab;
    public GameObject ShotSpawnLocation;
    public GameObject CrouchShotSpawnLocation;
    GameObject PhasewalkTiles;
    public GameObject BlinkPoint;
    public Material NormalTiles;
    public Material TransparentTiles;

    public int playerSpeed = 10;
    public int playerJumpPower = 200;
    public float playerMaxFallSpeed = 10;
    private bool _isGrounded;
    public bool isGrounded
    {
        get { return _isGrounded; }
        set
        {
            anim.SetBool("Grounded", value);
            _isGrounded = value;
        }
    }
    public int extraJumpsUsed = 0;

    public int BlinkEnergyCost;
    public float BlinkChargeTime;
    public bool IsChargingBlink()
    {
        return chargingBlink;
    }

    public bool IsPhaseWalking()
    {
        return phaseWalking;
    }
    public GameObject groundCheck;
    public GameObject phasewalkCheck;
    public LayerMask groundMask;
    public LayerMask phaseWalkMask;
    public float groundCheckRadius;
    public float phasewalkCheckRadius;

    public int PhaseWalkEnergyCost;
    public float PhaseWalkEnergyPerSec;
    public int PhaseWalkHealthPerSec;

    public bool InControl = true;

    public float AttackCooldown = 0;
    public int AttackDamage = 10;
    public float PointCooldown;

    private ParticleSystem phaseWalkParticleSystem;
    private ParticleSystem blinkParticleSystem;

    private void Start()
    {
        mapManager = MapManagerObj.GetComponent<MapManager>();

        // Find phasewalk tiles
        PhasewalkTiles = GameObject.FindGameObjectWithTag("PhasewalkTiles");

        var audioSources = GetComponents<AudioSource>();
        audioSource_shoot = audioSources[0];
        audioSource_melting = audioSources[1];
        audioSource_charging = audioSources[3];
        audioSource_blink = audioSources[4];
        audioSource_phasewalk = audioSources[5];

        anim = GetComponent<Animator>();
        dialogManager = DialogManager.Find();
        inventory = GetComponent<Inventory>();
        rigidBody = GetComponent<Rigidbody2D>();
        phaseWalkParticleSystem = gameObject.GetChildByName("PhasewalkParticleHolder").GetComponent<ParticleSystem>();
        blinkParticleSystem = gameObject.GetChildByName("BlinkParticleHolder").GetComponent<ParticleSystem>();
    }

    Coroutine meltingFadeOut;

    public void EnterLava()
    {
        if (meltingFadeOut != null)
            StopCoroutine(meltingFadeOut);
        audioSource_melting.volume = .75f;
        if (!audioSource_melting.isPlaying)
            audioSource_melting.Play();
    }

    public void ExitLava()
    {
        if (meltingFadeOut != null)
            StopCoroutine(meltingFadeOut);
        meltingFadeOut = StartCoroutine(Utilities.FadeOutAudio(audioSource_melting, 1));
    }

    // Update is called once per frame
    void Update()
    {
        mapManager.RecordPlayerPosition(transform.position);

        GroundCheck();

        if (InControl)
        {
            if (Input.GetButtonUp("Start"))
            {
                //dialogManager.ShowMessage("Paused");
                GameObject.Find("MapManager").GetComponent<MapManager>().ShowMap();
                InControl = false;
            }
            else
            {
                if (phaseWalking)
                {
                    phaseWalkEnergyDrain += (Time.deltaTime * PhaseWalkEnergyPerSec);

                    if (phaseWalkEnergyDrain > 1.0)
                    {
                        phaseWalkEnergyDrain -= 1;

                        if (!inventory.LoseEnergy(1))
                        {
                            // Instead of ending phasewalk, instead lose health!
                            int remainingHealth = inventory.LoseHealth(PhaseWalkHealthPerSec);
                            if (remainingHealth == 0)
                            {
                                // Player has died. Stop phasewalking.
                                phaseWalking = false;
                            }
                        }
                    }
                }

                PlayerMove();
            }
        }

        // Cap max fall speed
        rigidBody.velocity = new Vector2(rigidBody.velocity.x,
            Mathf.Clamp(rigidBody.velocity.y, playerMaxFallSpeed, 10000));
    }

    void Blink(int x, int y)
    {
        energizeTime = 0;
        chargingBlink = false;

        int mask = 0;

        if (!phaseWalking)
            mask = groundMask | phaseWalkMask;
        else
            mask = groundMask;

        RaycastHit2D hit = Physics2D.Raycast(BlinkPoint.transform.position,
            new Vector2(x, y),
            Mathf.Infinity,
            mask);

        if (hit)
        {
            if (audioSource_charging.isPlaying)
                audioSource_charging.Stop();

            audioSource_blink.Play();
            inventory.LoseEnergy(BlinkEnergyCost);

            if (hit.collider.name == "Outer Space")
            {
                DialogManager.Find().ShowMessage("You have used your Blink Drive to blink into the depths of space."
                        + " You lose consciousness within seconds."
                        + " :(");
                inventory.LoseHealth(9999);
            }
            else
            {
                var blinkPoint = new Vector3(hit.point.x, hit.point.y);

                var overlapData = Physics2D.OverlapCapsule(blinkPoint,
                    new Vector2(0.9f, 1.84f),
                    CapsuleDirection2D.Vertical,
                    0f,
                    mask);

                while (overlapData != null)
                {
                    if (x > 0)
                        blinkPoint = new Vector3(blinkPoint.x - 0.5f, blinkPoint.y);
                    else if (x < 0)
                        blinkPoint = new Vector3(blinkPoint.x + 0.5f, blinkPoint.y);
                    else if (y > 0)
                        blinkPoint = new Vector3(blinkPoint.x, blinkPoint.y - 0.5f);
                    else if (y < 0)
                        blinkPoint = new Vector3(blinkPoint.x, blinkPoint.y + 0.5f);

                    overlapData = Physics2D.OverlapCapsule(blinkPoint,
                        new Vector2(0.9f, 1.84f),
                        CapsuleDirection2D.Vertical,
                        0f,
                        groundMask);
                }

                transform.position = blinkPoint;

                if ((x == -1 && facingRight) || (x == 1 && !facingRight))
                    FlipPlayer();
            }
        }
    }

    void PlayerMove()
    {
        if (!crouching
            && isGrounded
            && rigidBody.velocity == Vector2.zero
            && Input.GetButton("Energize")
            && inventory.Energy >= BlinkEnergyCost
            && inventory.Equipment.Any(x => x == EquipmentEnum.BlinkDrive))
        {
            chargingBlink = true;
            energizeTime += Time.deltaTime;

            if (energizeTime >= BlinkChargeTime)
            {
                chargingBlink = false;
                float blinkX = Input.GetAxis("Horizontal");
                if (blinkX < 0 || blinkX > 0)
                {
                    Blink(blinkX < 0 ? -1 : 1, 0);
                }
                else
                {
                    float blinkY = Input.GetAxis("Vertical");
                    if (blinkY < 0 || blinkY > 0)
                    {
                        Blink(0, blinkY < 0 ? 1 : -1);
                    }
                }
            }
            else
            {
                if (!audioSource_charging.isPlaying)
                    audioSource_charging.Play();
            }

            return;
        }

        if (chargingBlink || energizeTime > 0)
        {
            chargingBlink = false;
            energizeTime = 0;
            if (audioSource_charging.isPlaying)
                audioSource_charging.Stop();
        }

        if (inventory.Equipment.Any(x => x == EquipmentEnum.PhaseWalk))
            PhaseWalk();

        // controls
        float moveY = Input.GetAxis("Vertical");
        moveX = Input.GetAxis("Horizontal");
        if (crouching)
        {
            // Check to see if should stand up.
            if (Mathf.Abs(moveX) > .125 || moveY < -.5)
            {
                crouching = false;
                anim.SetBool("Crouching", crouching);
            }
        }
        else
        {
            // Check to see if should crouch.
            if (moveY > .5)
            {
                crouching = true;
                anim.SetBool("Crouching", crouching);
            }
        }

        if (Mathf.Abs(moveY) > .9)
        {
            crouching = true;
            rigidBody.velocity = new Vector2(0,
                rigidBody.velocity.y);
            anim.SetFloat("Speed", Mathf.Abs(moveX));
        }
        else
        {
            anim.SetFloat("Speed", Mathf.Abs(moveX));
            rigidBody.velocity = new Vector2(moveX * playerSpeed,
                rigidBody.velocity.y);
        }

        if (Input.GetButtonDown("Jump") && (isGrounded || extraJumpsUsed < inventory.NumOfEquipment(EquipmentEnum.CloudTread)))
            Jump();

        if ((moveX < 0 && facingRight) || (moveX > 0 && !facingRight))
            FlipPlayer();

        if (Input.GetButtonDown("Attack"))
        {
            Attack();
        }
    }

    void DoPhaseWalk(bool on)
    {
        if (on)
        {
            // Enter phasewalk
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"),
                LayerMask.NameToLayer("Phasewalk"));
            phaseWalkEnergyDrain = 0;
            var mats = PhasewalkTiles.GetComponent<MeshRenderer>().materials;
            mats[0] = TransparentTiles;
            PhasewalkTiles.GetComponent<MeshRenderer>().materials = mats;
        }
        else
        {
            // Drop out of phasewalk
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"),
                LayerMask.NameToLayer("Phasewalk"),
                false);
            phaseWalking = false;
            var mats = PhasewalkTiles.GetComponent<MeshRenderer>().materials;
            mats[0] = NormalTiles;
            PhasewalkTiles.GetComponent<MeshRenderer>().materials = mats;
        }
    }

    void PhaseWalk()
    {
        if (Input.GetButtonUp("Phase") && (phaseWalking || inventory.LoseEnergy(PhaseWalkEnergyCost)))
        {
            if (!phaseWalking)
            {
                // If not already phasewalking, phasewalk!
                phaseWalking = true;
                DoPhaseWalk(true);
            }
            else
            {
                // If already phasewalking, can't end phasewalk while inside a wall!
                if (!IsInPhasewall())
                    DoPhaseWalk(false);
            }
        }
    }

    void Jump()
    {
        crouching = false;
        anim.SetBool("Crouching", crouching);
        rigidBody.velocity = new Vector2(rigidBody.velocity.x, 0);
        rigidBody.AddForce(Vector2.up * playerJumpPower, ForceMode2D.Impulse);

        if (!isGrounded
            && inventory.NumOfEquipment(EquipmentEnum.CloudTread) > 0
            && extraJumpsUsed < inventory.NumOfEquipment(EquipmentEnum.CloudTread))
            extraJumpsUsed++;

        isGrounded = false;
    }

    void FlipPlayer()
    {
        facingRight = !facingRight;
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y);
    }

    public void OnDrawGizmos()
    {
        //Gizmos.DrawSphere(phasewalkCheck.transform.position, phasewalkCheckRadius);
        //Gizmos.DrawSphere(groundCheck.transform.position, groundCheckRadius);
    }

    private void GroundCheck()
    {
        var groundHit = Physics2D.OverlapCircle(groundCheck.transform.position,
            groundCheckRadius,
            phaseWalking
                ? (LayerMask)groundMask
                : (LayerMask)(groundMask | phaseWalkMask));

        if (groundHit)
        {
            isGrounded = true;
            extraJumpsUsed = 0;
        }
        else
            isGrounded = false;
    }

    private bool IsInPhasewall()
    {
        var phasewallHit = Physics2D.OverlapCircle(phasewalkCheck.transform.position,
            phasewalkCheckRadius,
            phaseWalkMask);

        return phasewallHit != null;
    }

    private void Attack()
    {
        if (!CanAttack)
            return;

        if (Random.Range(1, 100) >= 50)
            audioSource_shoot.clip = snd_Shoot1;
        else
            audioSource_shoot.clip = snd_Shoot2;
        audioSource_shoot.Play();

        var shot = Instantiate(ShotPrefab,
            crouching
                ? CrouchShotSpawnLocation.transform.position
                : ShotSpawnLocation.transform.position,
            Quaternion.identity);
        shot.name = "Shot";
        shot.transform.localScale = new Vector3(shot.transform.localScale.x * (facingRight ? 1 : -1),
            1, 1);
        shot.GetComponent<Shot>().Damage = AttackDamage;

        StartCoroutine(DisableAttack());

        if (pointWeaponCo != null)
            StopCoroutine(pointWeaponCo);

        pointWeaponCo = StartCoroutine(PointWeapon());
    }

    IEnumerator DisableAttack()
    {
        CanAttack = false;
        yield return new WaitForSeconds(AttackCooldown);
        CanAttack = true;
    }

    IEnumerator PointWeapon()
    {
        IsPointing = true;
        anim.SetBool("Pointing", IsPointing);

        yield return new WaitForSeconds(PointCooldown);

        IsPointing = false;
        anim.SetBool("Pointing", IsPointing);
    }
}
