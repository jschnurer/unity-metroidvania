using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shot : MonoBehaviour {
    public int Damage;
    public float Speed;

    CameraSystem camSys;
    Rigidbody2D rbody;

    public void Start()
    {
        camSys = GameObject.Find("Main Camera").GetComponent<CameraSystem>();
        rbody = GetComponent<Rigidbody2D>();
    }

    public void Update()
    {
        if (!camSys.IsInCamera(transform.position))
        {
            // outside view
            Destroy(gameObject);
            return;
        }

        rbody.velocity = new Vector2(transform.localScale.x * Speed,
            0);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.layer == LayerMask.NameToLayer("Phasewalk"))
        {
            Destroy(gameObject);
        }
    }
}
