using UnityEngine;

[DisallowMultipleComponent]
public class StarFieldBackground : MonoBehaviour
{
    private const int TextureWidth = 256;
    private const int TextureHeight = 512;
    private const int StarCount = 100;
    private const int StarSortingOrder = -20;
    private const int StarsBeginAtBlock = 90;
    private const int StarsFullyVisibleAtBlock = 120;
    private const float MaximumAlpha = 0.82f;

    private Camera targetCamera;
    private GameObject starObject;
    private SpriteRenderer starRenderer;
    private Texture2D starTexture;
    private Sprite starSprite;
    private float currentAlpha;

    public void Initialize(Camera cameraToFollow)
    {
        targetCamera = cameraToFollow;
        CreateStarField();
    }

    public void UpdateProgress(int landedBlocks)
    {
        if (targetCamera == null || starRenderer == null)
            return;

        float progress = Mathf.InverseLerp(StarsBeginAtBlock, StarsFullyVisibleAtBlock, landedBlocks);
        float targetAlpha = Mathf.SmoothStep(0f, MaximumAlpha, progress);
        currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, Time.deltaTime * 0.22f);
        starRenderer.color = new Color(1f, 1f, 1f, currentAlpha);
        starObject.SetActive(currentAlpha > 0.001f);

        float worldHeight = targetCamera.orthographicSize * 2f;
        float worldWidth = worldHeight * targetCamera.aspect;
        starObject.transform.position = new Vector3(
            targetCamera.transform.position.x,
            targetCamera.transform.position.y,
            targetCamera.transform.position.z + 15f
        );
        starObject.transform.localScale = new Vector3(
            worldWidth / starSprite.bounds.size.x,
            worldHeight / starSprite.bounds.size.y,
            1f
        );
    }

    private void CreateStarField()
    {
        starTexture = new Texture2D(TextureWidth, TextureHeight, TextureFormat.RGBA32, false)
        {
            name = "Generated Star Field",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.HideAndDontSave
        };

        Color32[] pixels = new Color32[TextureWidth * TextureHeight];
        var random = new System.Random(90317);
        for (int i = 0; i < StarCount; i++)
        {
            int centerX = random.Next(2, TextureWidth - 2);
            int centerY = random.Next(2, TextureHeight - 2);
            int radius = random.Next(1, 3);
            byte brightness = (byte)random.Next(175, 256);

            for (int y = -radius; y <= radius; y++)
            for (int x = -radius; x <= radius; x++)
            {
                if (x * x + y * y > radius * radius)
                    continue;

                byte alpha = x == 0 && y == 0 ? brightness : (byte)(brightness * 0.35f);
                pixels[(centerY + y) * TextureWidth + centerX + x] = new Color32(255, 255, 255, alpha);
            }
        }

        starTexture.SetPixels32(pixels);
        starTexture.Apply(false, true);
        starSprite = Sprite.Create(
            starTexture,
            new Rect(0f, 0f, TextureWidth, TextureHeight),
            new Vector2(0.5f, 0.5f),
            100f
        );
        starSprite.name = "Generated Star Field Sprite";
        starSprite.hideFlags = HideFlags.HideAndDontSave;

        starObject = new GameObject("Background Stars");
        starObject.transform.SetParent(transform, false);
        starRenderer = starObject.AddComponent<SpriteRenderer>();
        starRenderer.sprite = starSprite;
        starRenderer.sortingOrder = StarSortingOrder;
        starObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (starSprite != null)
            Destroy(starSprite);
        if (starTexture != null)
            Destroy(starTexture);
    }
}
