using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rnd = UnityEngine.Random;

public class Cell  {
    public int Row { get; private set; }
    public int Col { get; private set; }
    public KMSelectable Button { get; private set; }
    public Tile Tile { get; private set; }

    public static Material redMaterial { get; set; }
    public static Material orangeMaterial { get; set; }
    public static Material greenMaterial { get; set; }
    public static Material blueMaterial { get; set; }
    public static Material purpleMaterial { get; set; }
    public static Material pinkMaterial { get; set; }

    public static Color redColor { get; set; }
    public static Color orangeColor { get; set; }
    public static Color greenColor { get; set; }
    public static Color blueColor { get; set; }
    public static Color purpleColor { get; set; }
    public static Color pinkColor { get; set; }

    private const float epsilon = 0.0001f;


    private Material[] materials;

    public MeshRenderer Mesh { get; private set; }

    public Cell Up { get; set; }
    public Cell Right { get; set; }
    public Cell Down { get; set; }
    public Cell Left { get; set; }
    public Cell Parent { get; set; }
    public bool Visited { get; set; }

    public Cell(int row, int col, KMSelectable button)
    {
        materials = new Material[] {redMaterial, orangeMaterial, greenMaterial, blueMaterial, purpleMaterial, pinkMaterial };
        Row = row;
        Col = col;
        Button = button;

        if (button != null)
        {
            Mesh = button.GetComponent<MeshRenderer>();
        }
    }

    private void SetTileColor(Material m)
    {
        Debug.Log($"Material Color: {m.color.r} {m.color.g} {m.color.g}");
        Debug.Log($"Red Color: {redMaterial.color.r} {redMaterial.color.g} {redMaterial.color.b}");
        //Debug.Log($"Blue Color: {blueMaterial.color.r} {blueMaterial.color.g} {blueMaterial.color.b}");

        if (SameColor(m.color, redColor))
        {
            Tile = Tile.Red;
        }

        else if (SameColor(m.color, orangeColor))
        {
            Tile = Tile.Orange;
        }


        else if(SameColor(m.color, greenColor))
        {
            Tile = Tile.Green;
        }


        else if(SameColor(m.color, blueColor))
        {
            Tile = Tile.Blue;
        }


        else if(SameColor(m.color, purpleColor))
        {
            Tile = Tile.Purple;
        }

        else
        {
            Tile = Tile.Pink;
        }

    }

    public void SetMaterial(Material m)
    {
        Mesh.sharedMaterial = m;
        SetTileColor(m);
    }

    public void SetRandomMaterial()
    {
        SetMaterial(materials[Rnd.Range(0, materials.Length)]);
    }

    public override string ToString()
    {
        return $"{Row} {Col}";
    }

    public string GetColor()
    {
        return Tile.ToString();
    }

    private bool SameColor(Color c1, Color c2)
    {
        return Mathf.Abs(c1.r - c2.r) < epsilon &&
               Mathf.Abs(c1.g - c2.g) < epsilon &&
               Mathf.Abs(c1.b - c2.b) < epsilon;
    }
}
