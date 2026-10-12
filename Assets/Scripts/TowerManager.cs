using UnityEngine;
using UnityEngine.InputSystem;

public class TowerManager : MonoBehaviour
{
    [Header("Blocos")]
    public GameObject blockPrefab;
    public Transform spawnPoint;
    public Color[] blockColors =
    {
        new Color(0.86f, 0.12f, 0.14f), // Vermelho
        new Color(0.90f, 0.35f, 0.03f), // Laranja
        new Color(0.78f, 0.54f, 0.02f), // Amarelo-ouro
        new Color(0.78f, 0.08f, 0.36f), // Rosa
        new Color(0.45f, 0.12f, 0.68f), // Roxo
        new Color(0.38f, 0.56f, 0.08f)  // Verde
    };

    [Header("Próximo bloco")]
    public float nextBlockSpawnDelay = 0.10f;
    public float spawnPointSettleTolerance = 0.1f;

    private GameObject currentBlock;
    private bool waitingForNextBlock = false;

    private int landedBlocks = 0;
    private Transform lastLandedBlock;
    private float nextBlockSpawnTime;
    private int previousColorIndex = -1;

    private bool gameOver = false;
    private GameOverUI gameOverUI;
    private GameHUD gameHUD;
    private TowerPresentation towerPresentation;
    private float gameStartTime;

    void Awake()
    {
        gameOverUI = GetComponent<GameOverUI>();
        if (gameOverUI == null)
            gameOverUI = gameObject.AddComponent<GameOverUI>();

        gameHUD = GetComponent<GameHUD>();
        if (gameHUD == null)
            gameHUD = gameObject.AddComponent<GameHUD>();

        towerPresentation = GetComponent<TowerPresentation>();
        if (towerPresentation == null)
            towerPresentation = gameObject.AddComponent<TowerPresentation>();
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
        gameHUD.SetProgress(Time.time - gameStartTime, landedBlocks);
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
        }
    }

    private void ApplyNextBlockColor(GameObject block)
    {
        if (blockColors == null || blockColors.Length == 0)
            return;

        int colorIndex = Random.Range(0, blockColors.Length);
        if (blockColors.Length > 1 && colorIndex == previousColorIndex)
        {
            colorIndex = (colorIndex + Random.Range(1, blockColors.Length)) % blockColors.Length;
        }

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

        landedBlocks++;
        lastLandedBlock = landedBlock;

        if (waitingForNextBlock)
            return;

        waitingForNextBlock = true;
        nextBlockSpawnTime = Time.time + (landedBlocks == 1 ? 0f : nextBlockSpawnDelay);
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

        float elapsedTime = Time.time - gameStartTime;
        float previousBestTime = PlayerPrefs.GetFloat("TowerUp_BestTime", 0f);
        int previousBestBlocks = PlayerPrefs.GetInt("TowerUp_BestBlocks", 0);

        gameOverUI.Show(elapsedTime, landedBlocks, previousBestTime, previousBestBlocks);
        gameHUD.Hide();

        PlayerPrefs.SetFloat("TowerUp_BestTime", Mathf.Max(previousBestTime, elapsedTime));
        PlayerPrefs.SetInt("TowerUp_BestBlocks", Mathf.Max(previousBestBlocks, landedBlocks));
        PlayerPrefs.Save();

        Debug.Log("GAME OVER!");
    }
}
