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

    [SerializeField]
    GameObject prefab;


    static int ModuleIdCounter = 1;
    int ModuleId;
    private bool ModuleSolved;
    bool debug = false;

    void Awake()
    {

        ModuleSolved = false;
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

        if (!debug)
        {
            GenerateMaze();

        }

       else
        {

        }
    }

    void KeypadPress(KMSelectable button)
    {
        Debug.Log(GetCell(button));
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
                endingCells.Add(grid[i, 7]);
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

        if (answer == null)
        {
            Debug.Log("L Bozo");
        }
        else
        {
            string.Join(" ", answer.Select(x => x.ToString()).ToArray());
        }
    }

    void GenereatDebugMaze()
    {

    }

    Cell GetCell(KMSelectable button)
    {
        foreach (Cell c in grid)
        {
            if (c.Button == button)
            {
                return c;
            }
        }

        return null;
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
        pathSmell = Smell.None;

        foreach (Cell c in grid)
        {
            c.Visited = false;
        }

        const int limit = 1000;
        List<Cell> path = new List<Cell>();
        List<Movement> allMoves = new List<Movement>();

        Queue<Cell> q = new Queue<Cell>();

        q.Enqueue(start);
        
        int count = 0;

        while (q.Count != 0 && count < limit)
        {
            Cell next = q.Dequeue();
            next.Visited = true;

            if (next == end)
            {
                break;
            }

            List<Cell> neighbors = new List<Cell> { next.Up, next.Left, next.Down, next.Right };

            neighbors = GetRidOfBadNeighbors(neighbors, next, q);
            //Debug.Log("Neighbors Count:" + neighbors.Count);

            foreach (Cell c in neighbors)
            {
                q.Enqueue(c);
                allMoves.Add(new Movement(next,c));
            }

            count++;
        }

        if (count == limit)
        {
            Debug.Log("Infinite loop has been found");
            return null;
        }

        //start at end and work backwards to start

        Debug.Log("All moves count: " + allMoves.Count);

        Movement lastMove;

        try
        {
            lastMove = allMoves.First(x => x.end == end);
        }

        catch
        {
            Debug.Log("Had problems with finding last move");
            return null;
        }

        List<Movement> relMovements = new List<Movement>() { lastMove };

        while (lastMove.start != start)
        {
            lastMove = allMoves.First(x => x.end == lastMove.start);
            relMovements.Add(lastMove);
        }

        List<Cell> tempPath = new List<Cell>();

        for (int i = relMovements.Count - 1; i > -1; i--)
        {
            tempPath.Add(relMovements[i].end);
        }

        foreach (Cell c in tempPath)
        {
            Debug.Log(c);
        }

        return tempPath;

    }

    void RegenerateCell(Cell c)
    {
        Material purple = materials[4];

        while (c.Mesh.material == purple)
        {
            c.SetRandomMaterial();
        }
    }

    List<Cell> GetRidOfBadNeighbors(List<Cell> list, Cell current, Queue<Cell> q)
    {
        List<Cell> newList = new List<Cell>();

        for (int i = 0; i < list.Count; i++)
        {
            Cell c = list[i];

            //if it's null, ....yeah
            if (c == null || c.Visited)
            {
                continue;
            }

           

            //if it's blue and the smell is orange, it's not safe
            if (c.Mesh.material == materials[3] && pathSmell == Smell.Orange)
            {
                continue;
            }

            //if it's red it's not safe
            if (c.Mesh.material == materials[0])
            {
                continue;
            }

            //if it's in the queue and not purple dont add it
            if (c.Mesh.material == materials[4] && q.Contains(c))
            {
                continue;
            }

            //if it's purple, check next cell:
            if (c.Mesh.material == materials[4])
            {
                Cell next = current.Up == c ? c.Up : current.Right == c ? c.Right : current.Down == c ? c.Down : c.Left;

                //-if next cell is blue and small like orange it's not safe
                if (c.Mesh.material == materials[3] && pathSmell == Smell.Orange)
                {
                    continue;
                }

                //-if next cell is red it's not safe
                if (c.Mesh.material == materials[0])
                {
                    continue;
                }

            }
            newList.Add(c);
        }
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
