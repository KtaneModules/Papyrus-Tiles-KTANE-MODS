using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rnd = UnityEngine.Random;

public class Cell  {
    public int Row { get; private set; }
    public int Col { get; private set; }
    public KMSelectable Button { get; private set; }
    public Tile tile;

    public static Material redMaterial { get; set; }
    public static Material orangeMaterial { get; set; }
    public static Material greenMaterial { get; set; }
    public static Material blueMaterial { get; set; }
    public static Material purpleMaterial { get; set; }
    public static Material pinkMaterial { get; set; }

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
        if (m == redMaterial)
        {
            tile = Tile.Red;
        }

        if (m == orangeMaterial)
        {
            tile = Tile.Orange;
        }

        if (m == greenMaterial)
        {
            tile = Tile.Green;
        }

        if (m == blueMaterial)
        {
            tile = Tile.Blue;
        }

        if (m == purpleMaterial)
        {
            tile = Tile.Purple;
        }


        else
        {
            tile = Tile.Pink;
        }

    }

    public void SetMaterial(Material m)
    {
        Mesh.material = m;
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
        return "" + tile.ToString();
    }

    public string GetMaterialName()
    {
        return Mesh.material.name;

    }
}
