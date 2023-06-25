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
    public Cell Parent { get; set; }
    public bool Visited { get; set; }

    public Cell(int row, int col, KMSelectable button)
    {
        Row = row;
        Col = col;
        Button = button;

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

    public void SetMaterial(Tile t)
    {
        switch (t)
        {
            case Tile.Pink:
                Mesh.sharedMaterial = pinkMaterial;
                break;
            case Tile.Red:
                Mesh.sharedMaterial = redMaterial;
                break;
            case Tile.Orange:
                Mesh.sharedMaterial = orangeMaterial;
                break;
            case Tile.Purple:
                Mesh.sharedMaterial = purpleMaterial;
                break;
            case Tile.Green:
                Mesh.sharedMaterial = greenMaterial;
                break;
            case Tile.Blue:
                Mesh.sharedMaterial = blueMaterial;
                break;
        }
    }

    public void SetRandomMaterial()
    {
        SetMaterial((Tile)Rnd.Range(0, 6));
    }

    public override string ToString()
    {
        return $"({Row} {Col})";
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
