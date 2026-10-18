using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class TowerManager : MonoBehaviour
{
    private const float PerfectFitHorizontalTolerance = 0.025f;

    [Header("Blocos")]
    public GameObject blockPrefab;
    public Transform spawnPoint;
    public Color[] blockColors =
    {
        new Color(0.94f, 0.48f, 0.48f), // Coral pastel
        new Color(1.00f, 0.68f, 0.46f), // Pêssego
        new Color(0.96f, 0.80f, 0.44f), // Amarelo manteiga
        new Color(0.94f, 0.55f, 0.68f), // Rosa
        new Color(0.68f, 0.58f, 0.88f), // Lavanda
        new Color(0.48f, 0.78f, 0.62f)  // Verde menta
    };

    [Header("Ícones da interface")]
    public Sprite clockIcon;

    [Header("Janelas dos blocos")]
    public Sprite[] windowModels;

    [Header("Cenário")]
    public Sprite[] cloudSprites;

    [Header("Próximo bloco")]
    public float nextBlockSpawnDelay = 0.10f;
    public float spawnPointSettleTolerance = 0.1f;

    private GameObject currentBlock;
    private bool waitingForNextBlock = false;

    private int landedBlocks = 0;
    private int perfectStreak = 0;
    private int bestPerfectStreakThisRun = 0;
    private Transform lastLandedBlock;
    private float nextBlockSpawnTime;
    private int previousColorIndex = -1;
    private int[] colorOrder;
    private int nextColorOrderIndex;
    private int nextWindowModelIndex;
    private readonly List<Rigidbody2D> landedBodies = new List<Rigidbody2D>();

    private bool gameOver = false;
    private GameOverUI gameOverUI;
    private GameHUD gameHUD;
    private TowerPresentation towerPresentation;
    private CloudBackground cloudBackground;
    private SkyProgression skyProgression;
    private MilestoneAnnouncer milestoneAnnouncer;
    private StarFieldBackground starFieldBackground;
    private float gameStartTime;

    void Awake()
    {
        gameOverUI = GetComponent<GameOverUI>();
        if (gameOverUI == null)
            gameOverUI = gameObject.AddComponent<GameOverUI>();

        gameHUD = GetComponent<GameHUD>();
        if (gameHUD == null)
            gameHUD = gameObject.AddComponent<GameHUD>();

        gameHUD.SetClockIcon(clockIcon);
        gameOverUI.SetClockIcon(clockIcon);

        SpriteRenderer blockRenderer = blockPrefab != null ? blockPrefab.GetComponent<SpriteRenderer>() : null;
        Sprite blockSprite = blockRenderer != null ? blockRenderer.sprite : null;
        gameHUD.SetBlockIcon(blockSprite);
        gameOverUI.SetBlockIcon(blockSprite);

        towerPresentation = GetComponent<TowerPresentation>();
        if (towerPresentation == null)
            towerPresentation = gameObject.AddComponent<TowerPresentation>();

        cloudBackground = GetComponent<CloudBackground>();
        if (cloudBackground == null)
            cloudBackground = gameObject.AddComponent<CloudBackground>();
        cloudBackground.Initialize(cloudSprites, towerPresentation.cameraTransform);

        skyProgression = GetComponent<SkyProgression>();
        if (skyProgression == null)
            skyProgression = gameObject.AddComponent<SkyProgression>();
        skyProgression.Initialize(towerPresentation.cameraTransform);

        milestoneAnnouncer = GetComponent<MilestoneAnnouncer>();
        if (milestoneAnnouncer == null)
            milestoneAnnouncer = gameObject.AddComponent<MilestoneAnnouncer>();

        starFieldBackground = GetComponent<StarFieldBackground>();
        if (starFieldBackground == null)
            starFieldBackground = gameObject.AddComponent<StarFieldBackground>();
        starFieldBackground.Initialize(towerPresentation.cameraTransform);
    }

    // Inicialização e atualização do jogo
    void Start()
    {
        gameStartTime = Time.time;
        SpawnBlock();
    }

    void Update()
    {
        if (gameOver)
            return;

        if (currentBlock != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            ReleaseBlock();
        }

        towerPresentation.UpdatePresentation(landedBlocks, lastLandedBlock);
        cloudBackground.UpdateClouds(landedBlocks);
        skyProgression.UpdateSky(landedBlocks);
        milestoneAnnouncer.UpdateProgress(landedBlocks);
        milestoneAnnouncer.SetDisplayPosition(towerPresentation.cameraTransform, GetMilestoneDisplayPosition());
        starFieldBackground.UpdateProgress(landedBlocks);
        gameHUD.SetProgress(Time.time - gameStartTime, landedBlocks, perfectStreak);
        TrySpawnNextBlock();

        if (currentBlock != null)
        {
            Rigidbody2D rb = currentBlock.GetComponent<Rigidbody2D>();

            if (rb.bodyType == RigidbodyType2D.Kinematic)
            {
                rb.MovePosition(spawnPoint.position);
            }
        }
    }

    private Vector3 GetMilestoneDisplayPosition()
    {
        Collider2D landedCollider = lastLandedBlock != null ? lastLandedBlock.GetComponent<Collider2D>() : null;
        float towerTopY = landedCollider != null ? landedCollider.bounds.max.y : spawnPoint.position.y;

        SpriteRenderer prefabRenderer = blockPrefab != null ? blockPrefab.GetComponent<SpriteRenderer>() : null;
        float hangingBlockHalfHeight = prefabRenderer != null ? prefabRenderer.bounds.extents.y : 0f;
        float hangingBlockBottomY = spawnPoint.position.y - hangingBlockHalfHeight;
        float centerY = (towerTopY + hangingBlockBottomY) * 0.5f;
        float centerX = lastLandedBlock != null
            ? (lastLandedBlock.position.x + spawnPoint.position.x) * 0.5f
            : spawnPoint.position.x;

        return new Vector3(centerX, centerY, 0f);
    }

    // Criação e liberação de blocos
    void SpawnBlock()
    {
        currentBlock = Instantiate(blockPrefab, spawnPoint.position, Quaternion.identity);
        ApplyNextBlockColor(currentBlock);

        Rigidbody2D rb = currentBlock.GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        Block block = currentBlock.GetComponent<Block>();

        if (block != null)
        {
            block.SetFollowTarget(spawnPoint);

            if (windowModels != null && windowModels.Length > 0)
            {
                block.AddWindowPair(windowModels[nextWindowModelIndex]);
                nextWindowModelIndex = (nextWindowModelIndex + 1) % windowModels.Length;
            }
        }
    }

    private void ApplyNextBlockColor(GameObject block)
    {
        if (blockColors == null || blockColors.Length == 0)
            return;

        if (colorOrder == null || colorOrder.Length != blockColors.Length || nextColorOrderIndex >= colorOrder.Length)
        {
            colorOrder = new int[blockColors.Length];
            for (int i = 0; i < colorOrder.Length; i++)
                colorOrder[i] = i;

            for (int i = colorOrder.Length - 1; i > 0; i--)
            {
                int swapIndex = Random.Range(0, i + 1);
                int temporary = colorOrder[i];
                colorOrder[i] = colorOrder[swapIndex];
                colorOrder[swapIndex] = temporary;
            }

            if (colorOrder.Length > 1 && colorOrder[0] == previousColorIndex)
            {
                int swapIndex = Random.Range(1, colorOrder.Length);
                int temporary = colorOrder[0];
                colorOrder[0] = colorOrder[swapIndex];
                colorOrder[swapIndex] = temporary;
            }

            nextColorOrderIndex = 0;
        }

        int colorIndex = colorOrder[nextColorOrderIndex++];

        SpriteRenderer spriteRenderer = block.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            spriteRenderer.color = blockColors[colorIndex];

        previousColorIndex = colorIndex;
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

        Rigidbody2D landedBody = landedBlock.GetComponent<Rigidbody2D>();
        if (landedBody != null && !landedBodies.Contains(landedBody))
            landedBodies.Add(landedBody);

        bool isPerfectFit = lastLandedBlock != null && TryPlayPerfectFitEffect(lastLandedBlock, landedBlock);
        if (isPerfectFit)
        {
            perfectStreak++;
            bestPerfectStreakThisRun = Mathf.Max(bestPerfectStreakThisRun, perfectStreak);
            StabilizePerfectTower();
        }
        else
        {
            perfectStreak = 0;
        }

        landedBlocks++;
        lastLandedBlock = landedBlock;

        if (waitingForNextBlock)
            return;

        waitingForNextBlock = true;
        nextBlockSpawnTime = Time.time + (landedBlocks == 1 ? 0f : nextBlockSpawnDelay);
    }

    private void StabilizePerfectTower()
    {
        if (!IsTowerBalancedOverBase())
            return;

        for (int i = 0; i < landedBodies.Count; i++)
        {
            Rigidbody2D body = landedBodies[i];
            if (body == null)
                continue;

            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
            body.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    private bool IsTowerBalancedOverBase()
    {
        if (landedBodies.Count == 0 || landedBodies[0] == null)
            return false;

        Collider2D baseCollider = landedBodies[0].GetComponent<Collider2D>();
        if (baseCollider == null)
            return false;

        float totalMass = 0f;
        float weightedCenterX = 0f;
        for (int i = 0; i < landedBodies.Count; i++)
        {
            Rigidbody2D body = landedBodies[i];
            if (body == null)
                continue;

            float mass = Mathf.Max(body.mass, 0.001f);
            totalMass += mass;
            weightedCenterX += body.worldCenterOfMass.x * mass;
        }

        if (totalMass <= 0f)
            return false;

        float centerOfMassX = weightedCenterX / totalMass;
        Bounds supportBounds = baseCollider.bounds;
        float safetyMargin = supportBounds.extents.x * 0.2f;
        return centerOfMassX >= supportBounds.min.x + safetyMargin
            && centerOfMassX <= supportBounds.max.x - safetyMargin;
    }

    private bool TryPlayPerfectFitEffect(Transform lowerBlock, Transform upperBlock)
    {
        if (Mathf.Abs(upperBlock.position.x - lowerBlock.position.x) > PerfectFitHorizontalTolerance)
            return false;

        float relativeRotation = Mathf.Abs(Mathf.DeltaAngle(
            lowerBlock.eulerAngles.z,
            upperBlock.eulerAngles.z
        ));
        if (relativeRotation > 1.5f)
            return false;

        Collider2D lowerCollider = lowerBlock.GetComponent<Collider2D>();
        Collider2D upperCollider = upperBlock.GetComponent<Collider2D>();
        if (lowerCollider == null || upperCollider == null)
            return false;

        float seamY = (lowerCollider.bounds.max.y + upperCollider.bounds.min.y) * 0.5f;
        float effectWidth = Mathf.Min(lowerCollider.bounds.size.x, upperCollider.bounds.size.x);
        SpriteRenderer spriteRenderer = upperBlock.GetComponent<SpriteRenderer>();
        int sortingLayerId = spriteRenderer != null ? spriteRenderer.sortingLayerID : 0;
        int sortingOrder = spriteRenderer != null ? spriteRenderer.sortingOrder + 2 : 2;
        Vector3 effectPosition = new Vector3(
            (lowerBlock.position.x + upperBlock.position.x) * 0.5f,
            seamY,
            upperBlock.position.z - 0.02f
        );

        PerfectFitEffect.Play(effectPosition, effectWidth, sortingLayerId, sortingOrder);
        return true;
    }

    void TrySpawnNextBlock()
    {
        if (!waitingForNextBlock || Time.time < nextBlockSpawnTime)
            return;

        if (landedBlocks == 1)
        {
            waitingForNextBlock = false;
            SpawnBlock();
            return;
        }

        if (!towerPresentation.IsSpawnPointAtTarget(lastLandedBlock, spawnPointSettleTolerance))
            return;

        waitingForNextBlock = false;
        SpawnBlock();
    }

    // Encerramento da partida
    public void TriggerGameOver(Transform landedBlock)
    {
        if (gameOver)
            return;

        FinishGameOver();

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

        FinishGameOver();

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

    private void FinishGameOver()
    {
        gameOver = true;
        waitingForNextBlock = false;
        milestoneAnnouncer.Hide();

        float elapsedTime = Time.time - gameStartTime;
        float previousBestTime = PlayerPrefs.GetFloat("TowerUp_BestTime", 0f);
        int previousBestBlocks = PlayerPrefs.GetInt("TowerUp_BestBlocks", 0);
        int previousBestPerfectStreak = PlayerPrefs.GetInt("TowerUp_BestPerfectStreak", 0);

        gameOverUI.Show(
            elapsedTime,
            landedBlocks,
            bestPerfectStreakThisRun,
            previousBestTime,
            previousBestBlocks,
            previousBestPerfectStreak
        );
        gameHUD.Hide();

        PlayerPrefs.SetFloat("TowerUp_BestTime", Mathf.Max(previousBestTime, elapsedTime));
        PlayerPrefs.SetInt("TowerUp_BestBlocks", Mathf.Max(previousBestBlocks, landedBlocks));
        PlayerPrefs.SetInt(
            "TowerUp_BestPerfectStreak",
            Mathf.Max(previousBestPerfectStreak, bestPerfectStreakThisRun)
        );
        PlayerPrefs.Save();

        Debug.Log("GAME OVER!");
    }
}
