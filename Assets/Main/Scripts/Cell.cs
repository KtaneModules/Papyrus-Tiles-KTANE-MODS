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

    public KMSelectable Button { get; private set; }
    public Tile Tile { get; private set; }

    public static Material redMaterial { get; set; }
    public static Material orangeMaterial { get; set; }
    public static Material greenMaterial { get; set; }
    public static Material blueMaterial { get; set; }
    public static Material purpleMaterial { get; set; }
    public static Material pinkMaterial { get; set; }

    private const float epsilon = 0.0001f;

    public MeshRenderer Mesh { get; private set; }

    public Cell Up { get; set; }
    public Cell Right { get; set; }
    public Cell Down { get; set; }
    public Cell Left { get; set; }

    public List<Cell> Neighbors { get { return new List<Cell>() { Up, Right, Down, Left }; } }
    public Cell Parent { get; set; }
    public bool Visited { get; set; }

    public bool HasPlayer { get; set; }

    public Cell(int row, int col, KMSelectable button)
    {
        Row = row;
        Col = col;
        Button = button;
        HasPlayer = false;

        if (button != null)
        {
            Mesh = button.GetComponent<MeshRenderer>();
        }
    }

    public Cell(int row, int col, KMSelectable button, Tile t)
    {
        Row = row;
        Col = col;
        Button = button;

        if (button != null)
        {
            Mesh = button.GetComponent<MeshRenderer>();
            SetTile(t);
        }
    }

    private void SetTile(Tile t)
    {
        Tile = t;
        SetMaterial(t);
    }

    public void SetRandomTile()
    {
        Tile = (Tile)Rnd.Range(0, 6);
        SetMaterial(Tile);
    }

    private void SetMaterial(Tile t)
    {
        switch (t)
        {
            case Tile.Pink:
                Mesh.material = pinkMaterial;
                break;
            case Tile.Red:
                Mesh.material = redMaterial;
                break;
            case Tile.Orange:
                Mesh.material = orangeMaterial;
                break;
            case Tile.Purple:
                Mesh.material = purpleMaterial;
                break;
            case Tile.Green:
                Mesh.material = greenMaterial;
                break;
            case Tile.Blue:
                Mesh.material = blueMaterial;
                break;
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

    public static bool SameColor(Color c1, Color c2)
    {
        return Mathf.Abs(c1.r - c2.r) < epsilon &&
               Mathf.Abs(c1.g - c2.g) < epsilon &&
               Mathf.Abs(c1.b - c2.b) < epsilon;
    }
}
