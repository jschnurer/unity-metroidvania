using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTouch : MonoBehaviour {
    public int DamageOnTouch;
    public bool FlinchOnTouch = true;

    private void OnTriggerStay2D(Collider2D collision)
    {
        collision.gameObject.GetComponent<PlayerDamage>().TakeDamage(DamageOnTouch,
            FlinchOnTouch,
            gameObject.tag);
    }
}
