using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDamage : MonoBehaviour
{
    AudioSource audioSource_Pain;
    Inventory inventory;
    Player_Move_Prot playerMove;
    Rigidbody2D rbody;
    SpriteRenderer spriteRenderer;

    public Color HurtColor;
    private Color NormalColor = new Color(1f, 1f, 1f, 1f);

    public float FlinchVelocityX = 350;
    public float FlinchVelocityY = 600;
    public float FlinchDuration = .2f;

    public float InvulnerabilityDuration = 2f;
    public float InvulnerabilityFlashFreq = .33f;
    private bool invulnerable = false;
    private float invulnerabilityFlashTime = 0f;
    private bool isTransparent = false;

    Coroutine disableControlsRef;
    Coroutine invulnerabilityRef;

    // Use this for initialization
    void Start()
    {
        audioSource_Pain = GetComponents<AudioSource>()[2];
        inventory = GetComponent<Inventory>();
        rbody = GetComponent<Rigidbody2D>();
        playerMove = GetComponent<Player_Move_Prot>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (invulnerable)
        {
            invulnerabilityFlashTime += Time.deltaTime;
            if (invulnerabilityFlashTime >= InvulnerabilityFlashFreq)
            {
                invulnerabilityFlashTime -= InvulnerabilityFlashFreq;

                var baseColor = playerMove.InControl
                    ? NormalColor
                    : HurtColor;

                if (isTransparent)
                    spriteRenderer.color = new Color(baseColor.r, baseColor.g, baseColor.b, 1f);
                else
                    spriteRenderer.color = new Color(baseColor.r, baseColor.g, baseColor.b, .5f);
                isTransparent = !isTransparent;
            }
        }
        else if (isTransparent)
        {
            spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
            isTransparent = false;
        }
    }

    public int TakeDamage(int amount, bool flinch, string sourceTag)
    {
        if (invulnerable || (playerMove.IsPhaseWalking() && sourceTag == "Enemy"))
        {
            return 999999;
        }
        
        int healthRemaining = inventory.LoseHealth(amount);

        if (flinch)
        {
            audioSource_Pain.Play();

            rbody.velocity = Vector2.zero;
            rbody.AddForce(new Vector2(-transform.localScale.x * FlinchVelocityX,
                FlinchVelocityY));

            if (disableControlsRef != null)
            {
                StopCoroutine(disableControlsRef);
            }

            disableControlsRef = StartCoroutine(DisableControls());

            if (invulnerabilityRef != null)
            {
                StopCoroutine(invulnerabilityRef);
            }

            invulnerabilityRef = StartCoroutine(StartInvulnerability());
        }

        return healthRemaining;
    }

    IEnumerator DisableControls()
    {
        spriteRenderer.color = HurtColor;
        playerMove.InControl = false;
        yield return new WaitForSeconds(FlinchDuration);
        playerMove.InControl = true;
    }

    IEnumerator StartInvulnerability()
    {
        invulnerable = true;
        spriteRenderer.color = new Color(1f, 1f, 1f, .5f);
        yield return new WaitForSeconds(InvulnerabilityDuration);
        invulnerable = false;
        spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
    }
}
