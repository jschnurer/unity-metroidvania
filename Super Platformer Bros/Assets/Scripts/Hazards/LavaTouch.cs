using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LavaTouch : MonoBehaviour {
    public float DamagePerSecond;
    private float AccruedDamage;
    private bool PlayerInLava;
    PlayerDamage pDamage;
    Player_Move_Prot pControls;
    public GameObject PlayerSmokeParticle;
    ParticleSystem smoke;

    private void Start()
    {
        var player = GameObject.Find("Player");
        pDamage = player.GetComponent<PlayerDamage>();
        pControls = player.GetComponent<Player_Move_Prot>();
        smoke = PlayerSmokeParticle.GetComponent<ParticleSystem>();
    }

    private void Update()
    {
        if (!PlayerInLava)
            return;

        AccruedDamage += Time.deltaTime * DamagePerSecond;

        if (AccruedDamage >= 1)
        {
            int healthRemaining = pDamage.TakeDamage((int)AccruedDamage,
                false,
                gameObject.tag);

            AccruedDamage -= (int)AccruedDamage;

            if (healthRemaining <= 0)
            {
                // Player died.
                ExitLava();
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        pControls.EnterLava();
        var emiss = smoke.emission;
        emiss.enabled = true;
        PlayerInLava = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        pControls.EnterLava();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        ExitLava();
    }

    private void ExitLava()
    {
        var emiss = smoke.emission;
        emiss.enabled = false;
        AccruedDamage = 0;
        PlayerInLava = false;
        pControls.ExitLava();
    }
}
