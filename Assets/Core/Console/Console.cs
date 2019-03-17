using System;
using System.Collections.Generic;
using UnityEngine;

public class Console : MonoBehaviour
{
    public GUISkin Skin;

    // Icons
    public Texture2D Close;
    public Texture2D Trash;
    public Texture2D Send;
    
    public Texture2D Next;
    public Texture2D Next2;
    public Texture2D Prev;
    public Texture2D Prev2;

    // Settings
    private const int sMin = 64;
    private const int sSpacing = 2;
    private const int sCells = 6;
    private const int sWidth = sMin * (sCells) + sSpacing * (sCells-1);

    // Data
    private List<Cell> cells = new List<Cell>();
    private Vector2 scroll;

    private Console()
    {
        Action IDLE = delegate() { };

        // Add(3);
        // Add(1, () => IconClose, IDLE);
        // Add(1, () => IconSend, IDLE);
        // Add(1, () => IconTrash, IDLE);

        Add(6, () => "USER_ID / STAT_ID");

        Add(1, () => Prev2, IDLE);
        Add(1, () => Prev, IDLE);
        Add(2, () => "Day #3\nForian_Fontain_Restoration", 3/4f);
        Add(1, () => Next, IDLE);
        Add(1, () => Next2, IDLE);

        // Add(6);

        Add(3, () => "Skip", IDLE);
        Add(3, () => "Lose", IDLE);
        Add(6, () => "Action", IDLE);

        Add(6);

        Add(3, () => "Progress", 1);
        Add(1, () => Prev, IDLE);
        Add(1, () => "335", 3/4f);
        Add(1, () => Next, IDLE);

        Add(3, () => "Life", 1);
        Add(1, () => Prev, IDLE);
        Add(1, () => "3", 3/4f);
        Add(1, () => Next, IDLE);

        Add(3, () => "Coins", 1);
        Add(1, () => Prev, IDLE);
        Add(1, () => "1100", 3/4f);
        Add(1, () => Next, IDLE);

        Add(3, () => "Life_Unlim", 1);
        Add(1, () => Prev, IDLE);
        Add(1, () => "30min", 3/4f);
        Add(1, () => Next, IDLE);
    }

    private void Add(int width) { cells.Add(new Cell(width)); }
    private void Add(int width, Func<string> text, Action action) { cells.Add(new Cell(width, text, action)); }
    private void Add(int width, Func<Texture2D> icon, Action action) { cells.Add(new Cell(width, icon, action)); }
    private void Add(int width, Func<string> text, float scale = 1f, TextAnchor anchor = TextAnchor.MiddleCenter)
    {
        cells.Add(new Cell(width, text, () => {
            var style = new GUIStyle(Skin.label);
            style.alignment = anchor;
            style.fontSize = Mathf.RoundToInt(style.fontSize * scale);
            return style;
        }));
    }

    private class Cell
    {
        public int Width;
        public Func<string> Text;
        public Func<Texture2D> Icon;
        public Action Action;
        public Func<GUIStyle> Style;

        public Cell(int width) { Width = width; }
        public Cell(int width, Func<string> text, Action action) : this(width) { Text = text; Action = action; }
        public Cell(int width, Func<Texture2D> icon, Action action) : this(width) { Icon = icon; Action = action; }
        public Cell(int width, Func<string> text, Func<GUIStyle> style) : this(width) { Text = text; Style = style; }

        public void Process(Rect rect)
        {
            if (Text != null && Action != null)
                GUI.Button(rect, Text());
            else if (Text != null)
                GUI.Label(rect, Text(), Style());
            else if (Icon != null)
                GUI.Button(rect, Icon());
        }
    }

    private void OnGUI()
    {
        GUI.skin = Skin;

        GUI.Box(new Rect(0, 0, sWidth, Screen.height), string.Empty);

        // sWidth + Mathf.CeilToInt(Skin.verticalScrollbar.fixedWidth)

        var cursor = 0;

        foreach (var cell in cells)
        {
            int cellX = cursor % sCells;
            int cellY = cursor / sCells;

            // Define Rect
            var x = cellX * (sMin + sSpacing);
            var y = cellY * (sMin + sSpacing);
            var w = sMin * cell.Width + sSpacing * (cell.Width - 1);
            var h = sMin;

            // Do GUI
            cell.Process(new Rect(x, y, w, h));

            // Move Cursor
            cursor += cell.Width;
        }
    }
}