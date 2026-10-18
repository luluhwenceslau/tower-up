using UnityEngine;

[DisallowMultipleComponent]
public class MilestoneAnnouncer : MonoBehaviour
{
    private static readonly int[] Milestones = { 25, 50, 75, 120 };
    private static readonly string[] Messages =
    {
        "☁ Above the Clouds!",
        "✈ Air Traffic!",
        "🛰 Edge of the Atmosphere!",
        "🚀 Space Reached!"
    };

    private const float MessageDuration = 3.2f;
    private const float FadeInDuration = 0.3f;
    private const float FadeOutStart = 2.55f;

    private int nextMilestoneIndex;
    private string activeMessage;
    private float shownAt;
    private bool isVisible;
    private GUIStyle messageStyle;
    private Vector2 messageScreenPosition;

    private void Awake()
    {
        messageScreenPosition = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
    }

    public static int Count => Milestones.Length;

    public static int GetFloor(int index) => Milestones[index];

    public static string GetMessage(int index) => Messages[index];

    public static bool IsUnlocked(int index) => PlayerPrefs.GetInt("TowerUp_Achievement_" + Milestones[index], 0) != 0;

    public void UpdateProgress(int landedBlocks)
    {
        if (isVisible || nextMilestoneIndex >= Milestones.Length)
            return;

        if (landedBlocks < Milestones[nextMilestoneIndex])
            return;

        PlayerPrefs.SetInt("TowerUp_Achievement_" + Milestones[nextMilestoneIndex], 1);
        PlayerPrefs.Save();
        activeMessage = Messages[nextMilestoneIndex];
        nextMilestoneIndex++;
        shownAt = Time.unscaledTime;
        isVisible = true;
    }

    public void Hide()
    {
        isVisible = false;
    }

    public void SetDisplayPosition(Camera camera, Vector3 worldPosition)
    {
        if (camera == null)
            return;

        Vector3 screenPosition = camera.WorldToScreenPoint(worldPosition);
        messageScreenPosition = new Vector2(screenPosition.x, Screen.height - screenPosition.y);
    }

    private void OnGUI()
    {
        if (!isVisible)
            return;

        float elapsed = Time.unscaledTime - shownAt;
        if (elapsed >= MessageDuration)
        {
            isVisible = false;
            return;
        }

        EnsureStyles();

        float fadeIn = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / FadeInDuration));
        float fadeOut = 1f - Mathf.SmoothStep(0f, 1f,
            Mathf.Clamp01((elapsed - FadeOutStart) / (MessageDuration - FadeOutStart)));
        float alpha = Mathf.Min(fadeIn, fadeOut);
        float pop = Mathf.Lerp(0.86f, 1f, fadeIn);
        Matrix4x4 previousMatrix = GUI.matrix;
        Vector3 center = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
        GUI.matrix = Matrix4x4.TRS(center, Quaternion.identity, new Vector3(pop, pop, 1f))
            * Matrix4x4.TRS(-center, Quaternion.identity, Vector3.one)
            * previousMatrix;

        float messageSize = Mathf.Clamp(Screen.width / 15f, 48f, 86f);
        messageStyle.fontSize = Mathf.RoundToInt(messageSize);
        float availableWidth = Screen.width - 40f;
        float measuredWidth = messageStyle.CalcSize(new GUIContent(activeMessage)).x;
        if (measuredWidth > availableWidth)
        {
            messageSize *= availableWidth / measuredWidth;
            messageStyle.fontSize = Mathf.RoundToInt(messageSize);
        }
        float messageHeight = messageSize * 1.65f;
        float top = Mathf.Clamp(messageScreenPosition.y - messageHeight * 0.5f, 8f, Screen.height - messageHeight - 8f);
        DrawOutlinedLabel(
            new Rect(12f, top, Screen.width - 24f, messageHeight),
            activeMessage,
            messageStyle,
            alpha,
            3f
        );

        GUI.matrix = previousMatrix;
    }

    private void EnsureStyles()
    {
        if (messageStyle != null)
            return;

        messageStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold
        };
    }

    private static void DrawOutlinedLabel(Rect rect, string text, GUIStyle style, float alpha, float outlineSize)
    {
        Color previousColor = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, alpha * 0.85f);
        for (int x = -1; x <= 1; x++)
        for (int y = -1; y <= 1; y++)
        {
            if (x == 0 && y == 0)
                continue;

            GUI.Label(new Rect(rect.x + x * outlineSize, rect.y + y * outlineSize,
                rect.width, rect.height), text, style);
        }

        GUI.color = new Color(1f, 1f, 1f, alpha);
        GUI.Label(rect, text, style);
        GUI.color = previousColor;
    }
}
