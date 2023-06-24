using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class Main : MonoBehaviour
{

    public KMBombInfo Bomb;
    public KMAudio Audio;

    Cell[,] grid;
    Cell currentPos;

    KMSelectable[] buttons;

    [SerializeField]
    Material[] materials; // red, orange, green, blue, purple, pink

    [SerializeField]
    GameObject orange;

    [SerializeField]
    GameObject lemon;

    List<Cell> answer;

    Smell currentSmell;
    Smell pathSmell;


    static int ModuleIdCounter = 1;
    int ModuleId;
    private bool ModuleSolved;

    void Awake()
    {
        ModuleSolved = false;
        GameObject[] obj = new GameObject[48];
        ModuleId = ModuleIdCounter++;

        buttons = GetComponent<KMSelectable>().Children;

        Cell.redMaterial = materials[0];
        Cell.orangeMaterial = materials[1];
        Cell.greenMaterial = materials[2];
        Cell.blueMaterial = materials[3];
        Cell.purpleMaterial = materials[4];
        Cell.pinkMaterial = materials[5];

        grid = new Cell[6, 8];

        for (int row = 0; row < 6; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                int index = row * 8 + col;

                grid[row, col] = new Cell(row, col, buttons[index]);

                buttons[index].OnInteract += delegate () { KeypadPress(buttons[index]); return false; };
            }
        }

        for (int row = 0; row < 6; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                Cell c = grid[row, col];
                c.Up = row - 1 < 0 ? null : grid[row - 1, col];
                c.Down = row + 1 > 5 ? null : grid[row + 1, col];
                c.Left = col - 1 < 0 ? null : grid[row, col -1];
                c.Right = col + 1 > 7 ? null : grid[row, col + 1];
            }
        }

        GenerateMaze();
    }

    void KeypadPress(KMSelectable button)
    {
        button.AddInteractionPunch(1f);

        if (ModuleSolved)
        {
            return;
        }
    }

    void GenerateMaze()
    {
        answer = null;

        //dont have purple spawn on edges
        Material purple = materials[4];

        do
        {
            foreach (Cell c in grid)
            {
                c.SetRandomMaterial();
            }

            for (int i = 0; i < 8; i++)
            {
                RegenerateCell(grid[0, i]);
                RegenerateCell(grid[5, i]);

                if (i < 6)
                {
                    RegenerateCell(grid[i, 7]);
                    RegenerateCell(grid[i, 0]);
                }
            }

            List<Cell> startingCells = new List<Cell>();
            List<Cell> endingCells = new List<Cell>();

            for (int i = 0; i < 6; i++)
            {
                if (grid[i, 0].Mesh.material != materials[0])
                {
                    startingCells.Add(grid[i, 0]);
                }

                if (grid[i, 7].Mesh.material != materials[0])
                {
                    endingCells.Add(grid[i, 0]);
                }
            }

            for (int startIndex = 0; startIndex < startingCells.Count; startIndex++)
            {
                for (int endIndex = 0; endIndex < endingCells.Count; endIndex++)
                {
                    Cell start = startingCells[startIndex];
                    Cell end = endingCells[endIndex];

                    answer = FindPath(start, end);

                    if (answer != null)
                    {
                        break;
                    }
                }

                if (answer != null)
                {
                    break;
                }
            }



        } while (answer == null);



    }
    void ResetModule()
    {
        currentSmell = Smell.None;
        currentPos = new Cell(-1, -1, null);
        SetSmell(Smell.None);
    }

    void Start()
    {

    }

    void Update()
    {

    }

    void SetSmell(Smell smell)
    {
        currentSmell = smell;

        switch (smell)
        {
            case Smell.Lemon:
                lemon.SetActive(true);
                orange.SetActive(false);
                break;
            case Smell.Orange:
                lemon.SetActive(false);
                orange.SetActive(true);
                break;
            default:
                lemon.SetActive(false);
                orange.SetActive(false);
                break;
        }
    }

    List<Cell> FindPath(Cell start, Cell end)
    {
        Debug.Log($"Start at " + start.ToString());
        Debug.Log($"End at " + end.ToString());

        SetHeristic(end);

        List<Cell> open = new List<Cell>();
        List<Cell> closed = new List<Cell>();
        pathSmell = Smell.None;

        start.G = 0;

        Cell currentCell = start;

        int count = 0;

        while (!open.Contains(end)) //change to when end is reached
        {
            count++;

            if (count == 100)
            {
                Debug.Log("An infiinte loop has occured");
                break;
            }

            //if cell is orange, apply orange smell
            if (currentCell.Mesh.material == materials[1])
            {
                pathSmell = Smell.Orange;
            }

            //if cell is purple, apply lemon smell and based off parent, go to the next cell immedialty
            else if (currentCell.Mesh.material == materials[4])
            {
                pathSmell = Smell.Lemon;

                if
            }




            //Debug.Log("Current cell is " + currentCell.ToString());

            List<Cell> neighbors = new List<Cell>();

            neighbors.Add(currentCell.Up);
            neighbors.Add(currentCell.Left);
            neighbors.Add(currentCell.Down);
            neighbors.Add(currentCell.Right);


            //Debug.Log("BEFORE neighbors are " + string.Join(" ", neighbors.Select(x => x == null ? "poop" : x.ToString()).ToArray()));

            neighbors = GetRidOfBadNeighbors(neighbors, closed);

            //Debug.Log("neighbors are " + string.Join(" ", neighbors.Select(x => ($"{x.ToString()} F={x.FinalCost}")).ToArray()));

            foreach (Cell c in neighbors)
            {
                if (!open.Contains(c))
                {
                    c.Parent = currentCell;
                    open.Add(c);
                    c.G = c.Parent.G + 1;
                    c.FinalCost = c.G + c.Heuristic;
                }

                else
                {
                    int potentialCost = c.Parent.G + 1;
                    if (c.FinalCost > potentialCost)
                    {
                        c.Parent = currentCell;
                        c.G = potentialCost;
                        c.FinalCost = c.G + c.Heuristic;
                    }
                }
            }

            open.Remove(currentCell);
            closed.Add(currentCell);

            if (open.Count == 0) //there is no path to end
            {
                return null;
            }
            open = open.OrderBy(x => x.FinalCost).ToList();
            currentCell = open.First();
            //Debug.Log("Cells in open list " + string.Join(" ", open.Select(x => ($"{x.ToString()} F={x.FinalCost}")).ToArray()));
            neighbors.Clear();
        }

        if (!open.Contains(end))
        {
            return null;
        }



        List<Cell> path = new List<Cell>();

        Cell current = end;
        while (!path.Contains(start))
        {
            path.Add(current);
            current = current.Parent;
        }
        path.Reverse();

        return path;
    }

    void SetHeristic(Cell end)
    {
        foreach (Cell c in grid)
        {
            c.Heuristic = Math.Abs(end.Row - c.Row) + Math.Abs(end.Col - c.Col);
            c.Parent = null;
            c.FinalCost = 0;
            c.G = 0;
        }
    }

    void RegenerateCell(Cell c)
    {
        Material purple = materials[4];

        while (c.Mesh.material == purple)
        {
            c.SetRandomMaterial();
        }
    }

    List<Cell> GetRidOfBadNeighbors(List<Cell> list, List<Cell> closed)
    {
        List<Cell> newList = new List<Cell>();



        //if it's blue and the smell is orange, it's not safe

        //if it's purple, check next cell:
        //-if next cell is blue and small like orange it's not safe
        //-if next cell is red it's not safe


        return newList;
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Use !{0} to do something.";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string Command)
    {
        yield return null;
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        yield return null;
    }
}
