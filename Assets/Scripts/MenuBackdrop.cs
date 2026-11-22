using UnityEngine;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class MenuBackdrop : MonoBehaviour
{
    private readonly List<GameObject> previewBlocks = new List<GameObject>();
    private GameObject previewRoot;

    public void Initialize(GameObject blockPrefab, Color[] colors, Sprite[] windows)
    {
        previewRoot = new GameObject("Menu Tower Preview");
        previewRoot.transform.SetParent(transform, false);

        if (blockPrefab == null) return;
        SpriteRenderer prefabRenderer = blockPrefab.GetComponent<SpriteRenderer>();
        if (prefabRenderer == null || prefabRenderer.sprite == null) return;

        GameObject baseObject = GameObject.Find("Base");
        Collider2D baseCollider = baseObject != null ? baseObject.GetComponent<Collider2D>() : null;
        float baseTop = baseCollider != null
            ? baseCollider.bounds.max.y
            : Camera.main != null ? Camera.main.transform.position.y - Camera.main.orthographicSize + 0.5f : -4f;
        float centerX = Camera.main != null ? Camera.main.transform.position.x : 0f;
        const int previewHeight = 4;
        float blockHeight = prefabRenderer.bounds.size.y;
        float halfHeight = blockHeight * 0.5f;

        for (int i = 0; i < previewHeight; i++)
        {
            Vector3 position = new Vector3(centerX, baseTop + halfHeight + blockHeight * i, 0f);
            GameObject blockObject = Instantiate(blockPrefab, position, Quaternion.identity, previewRoot.transform);
            blockObject.name = "Menu Preview Block " + (i + 1);

            Rigidbody2D body = blockObject.GetComponent<Rigidbody2D>();
            if (body != null) body.simulated = false;
            Collider2D collider = blockObject.GetComponent<Collider2D>();
            if (collider != null) collider.enabled = false;

            Block block = blockObject.GetComponent<Block>();
            if (block != null)
            {
                if (windows != null && windows.Length > 0)
                    block.AddWindowPair(windows[i % windows.Length]);
                block.enabled = false;
            }

            SpriteRenderer renderer = blockObject.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.sortingOrder = 2;
                if (colors != null && colors.Length > 0) renderer.color = colors[i % colors.Length];
            }
            previewBlocks.Add(blockObject);
        }
    }

    public void SetVisible(bool isVisible)
    {
        if (previewRoot != null) previewRoot.SetActive(isVisible);
    }
}
