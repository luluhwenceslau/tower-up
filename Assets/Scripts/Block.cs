using UnityEngine;

public class Block : MonoBehaviour
{
    private const float LandedLinearDamping = 0.35f;
    private const float LandedAngularDamping = 4f;

    private bool released = false;
    private bool landed = false;
    private float landedY;

    private Transform followTarget;

    public float followSpeed = 8f;

    private Vector2 followVelocity;

    public float followSmoothTime = 0.18f;
    public float swayMultiplier = 2.5f;

    private void Awake()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        if (spriteRenderer == null || spriteRenderer.sprite == null || boxCollider == null)
            return;

        Bounds spriteBounds = spriteRenderer.sprite.bounds;
        boxCollider.size = new Vector2(spriteBounds.size.x, spriteBounds.size.y * 0.96f);
        boxCollider.offset = spriteBounds.center;
    }

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

    public void AddWindowPair(Sprite windowSprite)
    {
        SpriteRenderer blockRenderer = GetComponent<SpriteRenderer>();
        if (windowSprite == null || blockRenderer == null || blockRenderer.sprite == null)
            return;

        Bounds blockBounds = blockRenderer.sprite.bounds;
        Bounds windowBounds = windowSprite.bounds;
        if (blockBounds.size.x <= 0f || windowBounds.size.x <= 0f)
            return;

        float targetWindowWidth = blockBounds.size.x * 0.3f;
        float windowScale = targetWindowWidth / windowBounds.size.x;
        float scaledWindowWidth = windowBounds.size.x * windowScale;
        float sideGap = (blockBounds.size.x - scaledWindowWidth * 2f) / 3f;
        float leftCenterX = blockBounds.min.x + sideGap + scaledWindowWidth * 0.5f;
        float rightCenterX = blockBounds.max.x - sideGap - scaledWindowWidth * 0.5f;

        CreateWindow(windowSprite, leftCenterX, blockBounds.center.y, windowScale, blockRenderer, "Window_Left");
        CreateWindow(windowSprite, rightCenterX, blockBounds.center.y, windowScale, blockRenderer, "Window_Right");
    }

    private void CreateWindow(
        Sprite windowSprite,
        float centerX,
        float centerY,
        float windowScale,
        SpriteRenderer blockRenderer,
        string windowName
    )
    {
        GameObject windowObject = new GameObject(windowName);
        Transform windowTransform = windowObject.transform;
        windowTransform.SetParent(transform, false);
        windowTransform.localScale = Vector3.one * windowScale;
        windowTransform.localPosition = new Vector3(
            centerX - windowSprite.bounds.center.x * windowScale,
            centerY - windowSprite.bounds.center.y * windowScale,
            -0.01f
        );

        SpriteRenderer windowRenderer = windowObject.AddComponent<SpriteRenderer>();
        windowRenderer.sprite = windowSprite;
        windowRenderer.sortingLayerID = blockRenderer.sortingLayerID;
        windowRenderer.sortingOrder = blockRenderer.sortingOrder + 1;
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

        // Reduce accumulated tower wobble after each landing while keeping the
        // block dynamic, so badly placed blocks can still tip and fall.
        Rigidbody2D body = GetComponent<Rigidbody2D>();
        if (body != null)
        {
            body.linearDamping = LandedLinearDamping;
            body.angularDamping = LandedAngularDamping;
        }

        TowerManager towerManager = FindAnyObjectByType<TowerManager>();

        if (towerManager != null)
        {
            towerManager.BlockLanded(transform);
        }
    }
}
