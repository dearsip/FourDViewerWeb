using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text;
using System.Linq;

// https://gist.github.com/udonba/2d5df5af2e327c0d75d4947d09b3e8db
public class LogText : MonoBehaviour
{
    private Text text;
    private StringBuilder builder = new StringBuilder();

    // Start is called before the first frame update
    void Awake()
    {
        text = GetComponent<Text>();
        text.verticalOverflow = VerticalWrapMode.Truncate;
        text.supportRichText = true;
        builder = new StringBuilder();
    }

    void OnEnable() {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable() {
        Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string logText, string stackTrace, LogType logType)
    {
        builder.Clear();

        builder.Append(string.Format("[{0}:{1:D3}] ", DateTime.Now.ToLongTimeString(), DateTime.Now.Millisecond));
        logText += "\r\n" + stackTrace;

        switch (logType)
        {
            case LogType.Assert:
            case LogType.Warning:
                logText = GetColoredString(logText, "yellow");
                break;
            case LogType.Error:
            case LogType.Exception:
                logText = GetColoredString(logText, "red");
                break;
            default:
                break;
        }

        builder.Append(logText);
        builder.Append(Environment.NewLine);

        text.text = builder.ToString() + text.text;

        AdjustText(text);
    }

    private string GetColoredString(string src, string color)
    {
        return string.Format("<color={0}>{1}</color>", color, src);
    }

    private void AdjustText(Text t)
    {
        TextGenerator generator = t.cachedTextGenerator;
        var settings = t.GetGenerationSettings(t.rectTransform.rect.size);
        generator.Populate(t.text, settings);

        int countVisible = generator.characterCountVisible;
        if (countVisible == 0 || t.text.Length <= countVisible)
            return;

        int truncatedCount = t.text.Length - countVisible;
        var lines = t.text.Split('\n').Reverse().ToArray();
        foreach (string line in lines)
        {
            t.text = t.text.Remove(t.text.Length - line.Length - 1, line.Length + 1);
            truncatedCount -= (line.Length + 1);
            if (truncatedCount <= 0)
                break;
        }
    }
}
