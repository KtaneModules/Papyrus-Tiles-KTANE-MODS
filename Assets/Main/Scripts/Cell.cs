using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rnd = UnityEngine.Random;

public class Cell  {
    public int Row { get; private set; }
    public int Col { get; private set; }
    public int Heuristic { get; set; }
    public int FinalCost { get; set; }
    public int G { get; set; }
    public bool Valid { get; set; }

    public CellSelectable Button { get; private set; }
    public Tile Tile { get; private set; }

    public static Color red { get; set; }
    public static Color orange { get; set; }
    public static Color green { get; set; }
    public static Color blue { get; set; }
    public static Color purple { get; set; }
    public static Color pink { get; set; }

    private const float epsilon = 0.0001f;

    public Cell Up { get; set; }
    public Cell Right { get; set; }
    public Cell Down { get; set; }
    public Cell Left { get; set; }

    public List<Cell> Neighbors { get { return new List<Cell>() { Up, Right, Down, Left }; } }
    public Cell Parent { get; set; }
    public bool Visited { get; set; }

    public bool HasPlayer { get; set; }

    public TextMesh ColorBlidTextMesh { get; set; } 

    public Cell(int row, int col, CellSelectable button)
    {
        Row = row;
        Col = col;
        Button = button;
        HasPlayer = false;
    }

    public Cell(int row, int col, CellSelectable button, Tile t)
    {
        Row = row;
        Col = col;
        Button = button;

        if (button != null)
            SetTile(t);
    }

    private void SetTile(Tile t)
    {
        Tile = t;
        SetColor(t);
    }

    public void SetRandomTile()
    {
        Tile = (Tile)Rnd.Range(0, 6);
        SetColor(Tile);
    }

    private void SetColor(Tile t)
    {
        switch (t)
        {
            case Tile.Pink: Button.Color = pink; break;
            case Tile.Red: Button.Color = red; break;
            case Tile.Orange: Button.Color = orange; break;
            case Tile.Purple: Button.Color = purple; break;
            case Tile.Green: Button.Color = green; break;
            case Tile.Blue: Button.Color = blue; break;
        }
    }

    public override string ToString()
    {
        return $"({Row + 1} {Col + 1})";
    }

    public string GetColor()
    {
        return Tile.ToString();
    }

    public void SetColorBlindTextMeshVisisbilty(bool visible)
    {
        ColorBlidTextMesh.gameObject.SetActive(visible);
    }
}
