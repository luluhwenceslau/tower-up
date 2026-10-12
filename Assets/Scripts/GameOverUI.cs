using UnityEngine;

[DisallowMultipleComponent]
public class GameOverUI : MonoBehaviour
{
    private bool isVisible;
    private float finalTime;
    private int finalBlocks;
    private float previousBestTime;
    private int previousBestBlocks;
    private GUIStyle titleStyle;
    private GUIStyle resultStyle;
    private GUIStyle recordStyle;
    private static readonly Color OverlayColor = new Color(0f, 0f, 0f, 0.4f);

    public void Show(float elapsedTime, int blocks, float bestTime, int bestBlocks)
    {
        isVisible = true;
        finalTime = elapsedTime;
        finalBlocks = blocks;
        previousBestTime = bestTime;
        previousBestBlocks = bestBlocks;
    }

    private void OnGUI()
    {
        if (!isVisible)
            return;

        if (titleStyle == null)
        {
            titleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 48,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };

            resultStyle = new GUIStyle(titleStyle)
            {
                fontSize = 38,
                fontStyle = FontStyle.Bold
            };

            recordStyle = new GUIStyle(resultStyle)
            {
                fontSize = 32,
                fontStyle = FontStyle.Normal
            };
        }

        Color previousColor = GUI.color;
        GUI.color = OverlayColor;
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = previousColor;

        string bestTimeText = previousBestTime > 0f ? FormatTime(previousBestTime) : "--:--";
        string bestBlocksText = previousBestBlocks > 0 ? previousBestBlocks.ToString() : "--";

        float titleHeight = titleStyle.fontSize + 48f;
        float resultHeight = resultStyle.fontSize + 25f;
        float recordHeight = recordStyle.fontSize + 19f;
        float contentHeight = titleHeight + 12f + resultHeight * 2f + 16f + recordHeight * 3f;
        float y = (Screen.height - contentHeight) * 0.5f;

        GUI.Label(new Rect(0, y, Screen.width, titleHeight), "GAME OVER", titleStyle);
        y += titleHeight + 12f;
        GUI.Label(new Rect(0, y, Screen.width, resultHeight), "TEMPO: " + FormatTime(finalTime), resultStyle);
        y += resultHeight;
        GUI.Label(new Rect(0, y, Screen.width, resultHeight), "BLOCOS EMPILHADOS: " + finalBlocks, resultStyle);
        y += resultHeight + 16f;
        GUI.Label(new Rect(0, y, Screen.width, recordHeight), "RECORDES ANTERIORES", recordStyle);
        y += recordHeight;
        GUI.Label(new Rect(0, y, Screen.width, recordHeight), "Tempo: " + bestTimeText, recordStyle);
        y += recordHeight;
        GUI.Label(new Rect(0, y, Screen.width, recordHeight), "Blocos: " + bestBlocksText, recordStyle);
    }

    private static string FormatTime(float seconds)
    {
        int totalSeconds = Mathf.Max(0, Mathf.FloorToInt(seconds));
        int hours = totalSeconds / 3600;
        int minutes = totalSeconds % 3600 / 60;
        int remainingSeconds = totalSeconds % 60;

        return hours > 0
            ? string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, remainingSeconds)
            : string.Format("{0:00}:{1:00}", minutes, remainingSeconds);
    }
}
