using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeTouch : MonoBehaviour
{
    public int DamageOnTouch;
    public bool FlinchOnTouch = true;

    private void OnCollisionStay2D(Collision2D collision)
    {
        collision.gameObject.GetComponent<PlayerDamage>().TakeDamage(DamageOnTouch,
            FlinchOnTouch,
            gameObject.tag);
    }
}
