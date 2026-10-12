using UnityEngine;

public class Block : MonoBehaviour
{
    private bool released = false;
    private bool landed = false;
    private float landedY;

    private Transform followTarget;

    public float followSpeed = 8f;

    private Vector2 followVelocity;

    public float followSmoothTime = 0.18f;
    public float swayMultiplier = 2.5f;

    private void Update()
    {
        if (!released)
            return;

        Camera cam = Camera.main;

        if (cam != null)
        {
            Vector3 screenPosition = cam.WorldToViewportPoint(transform.position);

            bool fellOffScreenBeforeLanding = !landed && screenPosition.y < -0.2f;
            bool fellAfterLanding = landed && transform.position.y < landedY - 1.5f;

            if (fellOffScreenBeforeLanding || fellAfterLanding)
            {
                TowerManager towerManager = FindAnyObjectByType<TowerManager>();

                if (towerManager != null)
                {
                    towerManager.TriggerGameOver();
                }

                released = false;
            }
        }
    }

    public void SetFollowTarget(Transform target)
    {
        followTarget = target;
    }

    public void Release()
    {
        released = true;
        followTarget = null;
    }

    private void FixedUpdate()
    {
        if (!released && followTarget != null)
        {
            transform.position = followTarget.position;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!released || landed)
            return;

        landed = true;
        landedY = transform.position.y;

        TowerManager towerManager = FindAnyObjectByType<TowerManager>();

        if (towerManager != null)
        {
            towerManager.BlockLanded(transform);
        }
    }
}
