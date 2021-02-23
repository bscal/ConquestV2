using Conquest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class DebugController : MonoBehaviour
{
    public static DebugController Singleton { get; private set; }

    private const int BUFFER_SIZE = 128;
    private const int VIEW_LENGTH = 16;

    private const float LINE_SIZE = 16;
    private const float VIEW_HEIGHT = VIEW_LENGTH * LINE_SIZE;
    private const float VIEW_BORDER_SIZE = LINE_SIZE / 2;

    private const int LAST_LIST_SIZE = 8;
    private const int TABLE_WIDTH_SIZE = 48;

    private Controls m_controls;
    private bool m_showConsole;

    private string m_input = "";
    private string m_current;
    private int m_index = 0;
    private int m_cursorIndex = 0;
    private Vector2 m_scroll;
    private Vector2 m_hintScroll;
    private Queue<ConsoleText> m_buffer = new Queue<ConsoleText>(BUFFER_SIZE);
    private List<string> m_hints;
    private List<string> m_last = new List<string>();

    private GUIStyle m_textStyle = new GUIStyle();
    private GUIStyle m_textInfoStyle = new GUIStyle();
    private GUIStyle m_textWarnStyle = new GUIStyle();
    private GUIStyle m_textErrStyle = new GUIStyle();
    private GUIStyle m_textActStyle = new GUIStyle();
    private GUIStyle m_hintStyle = new GUIStyle();
    private GUIStyle m_hintSelectStyle = new GUIStyle();

    private List<DebugCommandBase> m_commandList = new List<DebugCommandBase>();
    private HashSet<char> m_keywordChars = new HashSet<char>();

    [SerializeField]
    private Font m_font;

    // Start is called before the first frame update
    void Awake()
    {
        Singleton = this;
        DontDestroyOnLoad(this);

        m_controls = new Controls();
        m_controls.Enable();

        m_keywordChars.Add('\b');
        m_keywordChars.Add('\t');
        m_keywordChars.Add('`');
        m_keywordChars.Add((char)37);
        m_keywordChars.Add((char)38);
        m_keywordChars.Add((char)39);
        m_keywordChars.Add((char)40);

        m_controls.Keyboard.Console.performed += OnConsoleKey;
        m_controls.Keyboard.Return.performed += OnReturn;
        m_controls.UI.Arrows.performed += OnArrowKey;
        m_controls.Keyboard.Tab.performed += OnTab;
        m_controls.UI.Backspace.performed += OnBackspace;
        Keyboard.current.onTextInput += OnTextInput;

        m_textStyle.fontSize = 14;
        m_textStyle.font = m_font;
        m_textStyle.normal.textColor = Color.white;

        m_textInfoStyle.fontSize = 14;
        m_textInfoStyle.font = m_font;
        m_textInfoStyle.normal.textColor = new Color(99f / 255f, 171f / 255f, 201f / 255f);

        m_textWarnStyle.fontSize = 14;
        m_textWarnStyle.font = m_font;
        m_textWarnStyle.normal.textColor = new Color(1, .55f, .2f);

        m_textErrStyle.fontSize = 14;
        m_textErrStyle.font = m_font;
        m_textErrStyle.normal.textColor = new Color(1, .3f, .3f);

        m_textActStyle.fontSize = 14;
        m_textActStyle.font = m_font;
        m_textActStyle.normal.textColor = new Color(182f / 255f, 201f / 255f, 99f / 255f);

        m_hintStyle.fontSize = 14;
        m_hintStyle.font = m_font;
        m_hintStyle.normal.textColor = new Color(.6f, .6f, .6f);

        m_hintSelectStyle.fontSize = 14;
        m_hintSelectStyle.font = m_font;
        m_hintSelectStyle.normal.textColor = new Color(200 / 255, 200 / 255, 200 / 255);

        var ICEAGE = new DebugCommand("iceage", "start an iceage", "iceage", args => {
            GameManager.Singleton.World.worldTemp.StartIceAge(new TemperatureEvent(-10, 3, 10, 6), 9);
        });
        m_commandList.Add(ICEAGE);

        var WORLD_INFO = new DebugCommand("world_info", "Prints info on world", "world_info", args => {
            World world = GameManager.Singleton.World;
            PrintConsoleTable("World Info", 48, new string[] { "Temp Type" }, new object[] { world.worldTemp.changeType });
        });
        m_commandList.Add(WORLD_INFO);
    }

    private void OnGUI()
    {
        if (!m_showConsole) return;

        float y = 0;

        GUI.Box(new Rect(0, y, Screen.width, VIEW_HEIGHT), "");

        Rect viewport = new Rect(0, 0, Screen.width - 30, LINE_SIZE * BUFFER_SIZE);

        m_scroll = GUI.BeginScrollView(new Rect(0, y + VIEW_BORDER_SIZE, Screen.width, LINE_SIZE * VIEW_LENGTH - VIEW_BORDER_SIZE), m_scroll, viewport);
        int i = 0;
        foreach (ConsoleText line in m_buffer)
        {
            if (string.IsNullOrEmpty(line.text)) continue;
            Rect labelRect = new Rect(VIEW_BORDER_SIZE, LINE_SIZE * i, viewport.width - 100, LINE_SIZE);
            GUI.Label(labelRect, line.text, LogTypeToStyle(line.type));
            i++;
        }

        GUI.EndScrollView();

        y += VIEW_HEIGHT;

        GUI.Box(new Rect(0, y, Screen.width, LINE_SIZE * 2), "");

        GUI.Label(new Rect(VIEW_BORDER_SIZE, y + 9, Screen.width - LINE_SIZE, LINE_SIZE + 8), m_input, m_textStyle);
        m_textStyle.DrawCursor(new Rect(VIEW_BORDER_SIZE, y + 8, Screen.width - LINE_SIZE, LINE_SIZE), new GUIContent(m_input), 0, Mathf.Min(m_input.Length, m_cursorIndex));
        //m_input = GUI.TextField(r, m_input, m_textStyle);

        y += LINE_SIZE + LINE_SIZE / 2;
        if (!string.IsNullOrEmpty(m_input))
        {
            m_hintScroll = GUI.BeginScrollView(
                new Rect(0, y + VIEW_BORDER_SIZE, Screen.width, LINE_SIZE * 8 - VIEW_BORDER_SIZE),
                m_hintScroll,
                new Rect(0, 0, Screen.width - 30, LINE_SIZE * 8));

            m_hints = MatchStringToCommand(m_input);
            for (int j = 0; j < m_hints.Count; j++)
            {
                if (m_hints[j] == null || string.IsNullOrEmpty(m_hints[j])) continue;
                GUI.Box(new Rect(0, (LINE_SIZE + 10) * j, Screen.width - 30, LINE_SIZE + 10), "");
                Rect labelRect = new Rect(VIEW_BORDER_SIZE, (LINE_SIZE + 10) * j + 5, viewport.width - 100, LINE_SIZE);
                if (j == m_index - 1)
                    GUI.Label(labelRect, m_hints[j], m_hintSelectStyle);
                else
                    GUI.Label(labelRect, m_hints[j], m_hintStyle);
            }
            GUI.EndScrollView();
        }
        GUI.backgroundColor = new Color(0, 0, 0, 0);
    }

    private void HandleInput()
    {
        for (int i = 0; i < m_commandList.Count; i++)
        {
            DebugCommandBase cmd = m_commandList[i];
            string[] split = m_input.Split(new char[] { ' ' });
            string cmdName = split[0];

            List<string> args = new List<string>();
            if (split.Length > 1) // Has a cmdName and args
            {
                for (int j = 1; j < split.Length; j++)
                {
                    if (string.IsNullOrEmpty(split[j])) continue;
                    args.Add(split[j]);
                }
            }

            if (cmdName.Equals(cmd.Name, StringComparison.OrdinalIgnoreCase))
            {
                if (cmd.GetType() == typeof(DebugCommand))
                {
                    ((DebugCommand)cmd).Invoke(cmdName, split);
                }
                else if (cmd.GetType() == typeof(DebugArgsCommand))
                {
                    ((DebugArgsCommand)cmd).Invoke(cmdName, args);
                }
                return;
            }
        }
        PrintConsole(m_input, LogType.NONE);
    }

    private List<string> MatchStringToCommand(string str)
    {
        List<string> res = new List<string>();
        string pattern = string.Format("^(?i:{0})", Regex.Escape(str));

        for (int i = 0; i < m_commandList.Count; i++)
        {
            var m = Regex.Match(m_commandList[i].Name, pattern);
            if (m.Success)
            {
                res.Add(m_commandList[i].Formatted);
            }
        }
        return res;
    }

    public void PrintConsole(string text, LogType type = LogType.INFO)
    {
        ConsoleText cText = new ConsoleText(FormatLog(text, type, false), type);
        AddText(cText);
    }

    public void PrintConsoleValues(string text, object[] values, LogType type = LogType.INFO)
    {
        StringBuilder sb = new StringBuilder(text);
        sb.Append(": ");
        for (int i = 0; i < values.Length; i++)
        {
            if (values[i] == null)
                sb.Append("NULL");
            else
                sb.Append(values[i].GetType().Name + " : " + values[i]);
            if (i != values.Length - 1) sb.Append(" | ");
        }
        ConsoleText cText = new ConsoleText(FormatLog(sb.ToString(), type, false), type);
        AddText(cText);
    }

    public void PrintConsoleTable(string text, int size, string[] keys, object[] values, LogType type = LogType.INFO)
    {
        const int MIN_SIZE = 16;
        const int MAX_SIZE = 128;
        size = Mathf.Clamp(size, MIN_SIZE, MAX_SIZE);

        ConsoleText cText = new ConsoleText(GetRepeatedStr('-', size), type);
        AddText(cText);

        for (int i = 0; i < values.Length; i++)
        {
            string key;
            object val;

            if (i >= keys.Length || string.IsNullOrEmpty(keys[i]))
                key = "";
            else
                key = keys[i];

            if (values[i] == null)
                val = "NULL";
            else
                val = values[i];

            ConsoleText ct = new ConsoleText(FormatLogTable(key, val.ToString(), "*", size), type);
            AddText(ct);
        }
        AddText(cText);
    }

    public void PrintObjAsTable(object obj, LogType type = LogType.INFO)
    {
        const int BORDER_SIZE = 64;


        ConsoleText cText = new ConsoleText(GetRepeatedStr('-', BORDER_SIZE), type);
        AddText(cText);

        ConsoleText headText = new ConsoleText(FormatHeaderTable(obj.GetType().Name, "", BORDER_SIZE), type);
        AddText(headText);

        AddText(cText);

        var fields = obj.GetType().GetFields();

        for (int i = 0; i < fields.Length; i++)
        {
            string val = "";
            if (fields[i].GetValue(obj) != null) val = fields[i].GetValue(obj).ToString();

            ConsoleText ct = new ConsoleText(FormatLogTable(fields[i].Name, val, "*", BORDER_SIZE), type);
            AddText(ct);
        }
        AddText(cText);
    }

    private void AddText(ConsoleText cText)
    {
        if (m_buffer.Count >= BUFFER_SIZE)
            m_buffer.Dequeue();
        m_buffer.Enqueue(cText);
    }

    private string FormatHeaderTable(string text, string border, int length)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("| ");
        sb.Append(text);
        for (int i = sb.Length; i < length - 1; i++)
        {
            sb.Append(" ");
        }
        sb.Append('|');
        return sb.ToString();
    }

    private string FormatLogTable(string key, string val, string border, int length)
    {
        int halfLength = length / 2;
        StringBuilder sb = new StringBuilder();

        sb.Append('|');
        sb.Append(" ");
        for (int i = 0; i < halfLength - 4; i++)
        {
            if (i < key.Length)
                sb.Append(key[i]);
            else
                sb.Append(" ");
        }
        sb.Append(" ");
        sb.Append('|');
        sb.Append(" ");
        for (int i = 0; i < halfLength - 4; i++)
        {
            if (i < val.Length)
                sb.Append(val[i]);
            else
                sb.Append(" ");
        }
        sb.Append(" ");
        sb.Append(" ");
        sb.Append('|');

        return sb.ToString();
    }

    private string GetRepeatedStr(char chr, int length)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < length; i++)
        {
            sb.Append(chr);
        }
        return sb.ToString();
    }

    private string FormatLog(string text, LogType type, bool isServer)
    {
        return string.Format("[{0}]{1}{2}: {3}",
            DateTime.Now.ToString("HH:mm:ss"),
            (isServer) ? "[Server] " : "",
            (type == LogType.NONE) ? "" : "[" + type.ToString() + "]",
            text);
    }

    private GUIStyle LogTypeToStyle(LogType type)
    {
        switch (type)
        {
            case LogType.INFO:
                return m_textInfoStyle;
            case LogType.WARNING:
                return m_textWarnStyle;
            case LogType.ERROR:
                return m_textErrStyle;
            case LogType.ACTION:
                return m_textActStyle;
            case LogType.NONE:
                return m_textStyle;
            default:
                return m_textInfoStyle;
        }
    }

    public void AddCommand(DebugCommand cmd)
    {
        if (m_commandList.Contains(cmd))
            return;

        m_commandList.Add(cmd);
    }

    private void OnConsoleKey(CallbackContext ctx)
    {
        m_showConsole = !m_showConsole;
    }

    private void OnReturn(CallbackContext ctx)
    {
        if (string.IsNullOrEmpty(m_input)) return;
        HandleInput();

        m_last.Add(m_input);

        m_index = 0;
        m_input = "";
        m_current = "";
    }

    private void OnArrowKey(CallbackContext ctx)
    {
        if (m_hints == null) return;
        var val = ctx.ReadValue<Vector2>();

        if (val.y > 0f)
        {
            if (m_index >= m_hints.Count) return;
            m_index++;
        }
        else if (val.y < 0f)
        {
            if (m_index <= -m_last.Count) return;
            m_index--;
        }

        if (val.x > 0f && m_cursorIndex < m_input.Length)
            m_cursorIndex++;
        else if (val.x < 0f && m_cursorIndex > -1)
            m_cursorIndex--;

        int i = m_last.Count - Mathf.Abs(m_index);
        if (m_index < 0 && !string.IsNullOrEmpty(m_last[i]))
            m_input = m_last[i];
        else if (m_index == 0)
            m_input = m_current;
    }

    private void OnTab(CallbackContext ctx)
    {
        if (m_hints == null || m_hints.Count < 1) return;
        m_input = m_hints[m_index].Split(new char[] { ' ' })[0];
        m_cursorIndex = m_input.Length;
    }

    private void OnBackspace(CallbackContext ctx)
    {
        if (m_input.Length < 1) return;
        m_input = m_input.Substring(0, m_input.Length - 1);
    }

    private void OnTextInput(char c)
    {
        if (!m_showConsole)
            return;

        m_cursorIndex = Mathf.Max(0, Mathf.Min(m_cursorIndex, m_input.Length));

        if (!m_keywordChars.Contains(c))
        {
            if (m_cursorIndex < m_input.Length)
                m_input = m_input.Insert(Mathf.Max(m_cursorIndex, 0), c.ToString());
            else
                m_input += c;
            m_cursorIndex++;
        }

        if (m_index == 0)
            m_current = m_input;
    }

}

public enum LogType
{
    NONE,
    INFO,
    WARNING,
    ERROR,
    ACTION
}

struct ConsoleText
{
    public string text;
    public LogType type;

    public ConsoleText(string text, LogType type)
    {
        this.text = text;
        this.type = type;
    }
}
