using UnityEngine;
using UnityEngine.InputSystem;

public class TowerManager : MonoBehaviour
{
    public GameObject blockPrefab;
    public Transform spawnPoint;

    public Camera cameraTransform;

    public float cameraFollowSpeed = 3f;
    public float cameraRisePerBlock = 1f;

    private GameObject currentBlock;
    private bool waitingForNextBlock = false;

    private int landedBlocks = 0;
    private float cameraStartY;

    public Transform cableRig;

    public float swaySpeed = 1.5f;
    public float swayAmount = 2f;
    public float swayIncreasePerBlock = 0.15f;

    public float gameOverAngle = 15f;

    private bool gameOver = false;

    void Start()
    {
        cameraStartY = cameraTransform.transform.position.y;
        SpawnBlock();
    }

    void Update()
    {
        if (currentBlock != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            ReleaseBlock();
        }

        FollowTower();
        SwayCable();

        if (currentBlock != null)
        {
            Rigidbody2D rb = currentBlock.GetComponent<Rigidbody2D>();

            if (rb.bodyType == RigidbodyType2D.Kinematic)
            {
                rb.MovePosition(spawnPoint.position);
            }
        }
    }

    void SpawnBlock()
    {
        currentBlock = Instantiate(blockPrefab, spawnPoint.position, Quaternion.identity);

        Rigidbody2D rb = currentBlock.GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        Block block = currentBlock.GetComponent<Block>();

        if (block != null)
        {
            block.SetFollowTarget(spawnPoint);
        }
    }

    void ReleaseBlock()
    {
        Rigidbody2D rb = currentBlock.GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;

        Block block = currentBlock.GetComponent<Block>();

        if (block != null)
        {
            block.Release();
        }

        currentBlock = null;
    }

    public void BlockLanded(Transform landedBlock)
    {
        if (gameOver)
            return;

        landedBlocks++;

        CheckTowerTilt(landedBlock);

        if (gameOver)
            return;

        if (waitingForNextBlock)
            return;

        waitingForNextBlock = true;

        Invoke(nameof(SpawnNextBlock), 0.2f);
    }

    void SpawnNextBlock()
    {
        waitingForNextBlock = false;
        SpawnBlock();
    }

    void FollowTower()
    {
        if (cameraTransform == null)
            return;

        if (landedBlocks < 2)
            return;

        float targetY = cameraStartY + ((landedBlocks - 1) * cameraRisePerBlock);

        Vector3 targetPosition = new Vector3(
            cameraTransform.transform.position.x,
            targetY,
            cameraTransform.transform.position.z
        );

        cameraTransform.transform.position = Vector3.Lerp(
            cameraTransform.transform.position,
            targetPosition,
            cameraFollowSpeed * Time.deltaTime
        );
    }

    void SwayCable()
    {
        if (cableRig == null)
            return;

        if (landedBlocks <= 2)
        {
            cableRig.localPosition = Vector3.zero;
            return;
        }

        float amplitude = swayAmount + ((landedBlocks - 2) * swayIncreasePerBlock);

        float sway = Mathf.Sin(Time.time * swaySpeed) * amplitude;

        cableRig.localPosition = new Vector3(sway, 0f, 0f);
    }

    void CheckTowerTilt(Transform landedBlock)
    {
        float angle = Mathf.Abs(landedBlock.eulerAngles.z);

        if (angle > 180f)
            angle = 360f - angle;

        if (angle >= gameOverAngle)
        {
            TriggerGameOver(landedBlock);
        }
    }

    public void TriggerGameOver(Transform landedBlock)
    {
        if (gameOver)
            return;

        gameOver = true;

        Debug.Log("GAME OVER!");

        Rigidbody2D rb = landedBlock.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.AddTorque(8f, ForceMode2D.Impulse);
        }
    }

    public void TriggerGameOver()
    {
        if (gameOver)
            return;

        gameOver = true;

        Debug.Log("GAME OVER!");

        if (currentBlock != null)
        {
            Rigidbody2D rb = currentBlock.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                rb.AddTorque(8f, ForceMode2D.Impulse);
            }

            currentBlock = null;
        }
    }
}
