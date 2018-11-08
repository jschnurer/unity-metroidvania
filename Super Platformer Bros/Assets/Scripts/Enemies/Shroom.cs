using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shroom : MonoBehaviour {
    public GameObject GroundCheck;
    public float GroundCheckRadius;
    public LayerMask GroundMask;
    public LayerMask PhaseWalkMask;
    public float MoveSpeed;
    bool facingRight = true;

    Rigidbody2D rbody;

    // Use this for initialization
    void Start () {
        rbody = GetComponent<Rigidbody2D>();
	}
	
	// Update is called once per frame
	void Update () {
        RaycastHit2D hit = Physics2D.Raycast(transform.position,
           new Vector2(transform.localScale.x, 0),
           .66f,
           (LayerMask)GroundMask | (LayerMask)PhaseWalkMask);

        if (hit.collider)
        {
            facingRight = !facingRight;
            transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y);
        }

        DoGroundCheck();

        // physics
        rbody.velocity = new Vector2(transform.localScale.x * MoveSpeed,
            rbody.velocity.y);
    }

    private void DoGroundCheck()
    {
        var groundHit = Physics2D.OverlapCircle(GroundCheck.transform.position,
            GroundCheckRadius,
            (LayerMask)GroundMask | (LayerMask)PhaseWalkMask);

        if (!groundHit)
        {
            facingRight = !facingRight;
            transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y);
        }
    }

    public void OnDrawGizmos()
    {
        Gizmos.DrawSphere(GroundCheck.transform.position, GroundCheckRadius);
        Gizmos.DrawLine(transform.position, transform.position + new Vector3(.66f, 0, 0));
    }
}
