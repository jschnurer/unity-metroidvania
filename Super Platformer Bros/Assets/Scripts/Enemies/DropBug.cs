using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropBug : MonoBehaviour {
    AudioSource audioSource;
    public LayerMask playerLayerMask;
    Vector2 startPoint;
    Vector2 leftRaycastPoint;
    Vector2 rightRaycastPoint;
    Rigidbody2D rBody;
    bool climbing;
    public float climbSpeed;
    LineRenderer lineRenderer;

    void Start () {
        audioSource = GetComponent<AudioSource>();
        startPoint = transform.position;
        var bounds = GetComponent<BoxCollider2D>().bounds;
        rBody = GetComponent<Rigidbody2D>();
        leftRaycastPoint = new Vector2(bounds.min.x, bounds.max.y);
        rightRaycastPoint = new Vector2(bounds.max.x, bounds.max.y);
        lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.SetPosition(0, new Vector2(startPoint.x, bounds.max.y + .25f));
        lineRenderer.SetPosition(1, startPoint);
    }

    void Update () {
        if (rBody.isKinematic && !climbing)
        {
            RaycastHit2D leftHit = Physics2D.Raycast(leftRaycastPoint,
                Vector2.down,
                10,
                playerLayerMask);

            RaycastHit2D rightHit = Physics2D.Raycast(rightRaycastPoint,
                Vector2.down,
                10,
                playerLayerMask);

            if (leftHit || rightHit)
            {
                rBody.isKinematic = false;
                audioSource.Play();
            }
        }
        else if (climbing)
        {
            transform.position = Vector3.MoveTowards(transform.position, startPoint, climbSpeed * Time.deltaTime);

            if ((Vector2)transform.position == startPoint)
                climbing = false;

            lineRenderer.SetPosition(1, transform.position);
        }
        else
        {
            rBody.velocity = new Vector2(0, rBody.velocity.y);
            lineRenderer.SetPosition(1, transform.position);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.layer == LayerMask.NameToLayer("Phasewalk"))
        {
            climbing = true;
            rBody.isKinematic = true;
        }
    }
}
