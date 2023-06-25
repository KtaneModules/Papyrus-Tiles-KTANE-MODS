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

    static int ModuleIdCounter = 1;
    int ModuleId;
    private bool ModuleSolved;
    bool debug = true;

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

        Cell.redColor = Cell.redMaterial.color;
        Cell.orangeColor = Cell.orangeMaterial.color;
        Cell.greenColor = Cell.greenMaterial.color;
        Cell.blueColor = Cell.blueMaterial.color;
        Cell.purpleColor = Cell.purpleMaterial.color;
        Cell.pinkColor = Cell.pinkMaterial.color;



        grid = new Cell[6, 8];
        if (!debug)
        {
            for (int row = 0; row < 6; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    int index = row * 8 + col;
                    grid[row, col] = new Cell(row, col, buttons[index]);

                    buttons[index].OnInteract += delegate () { KeypadPress(buttons[index]); return false; };
                }
            }

            SetNeighbors();

            bool validMaze = false;

            int count = 0;
            do
            {
                count++;
                validMaze = GenerateMaze();
            } while (!validMaze && count < 100);

            if (count == 100)
            {
                Debug.Log("Couldn't generate a good maze");
            }
        }

        else
        {
            GenereatDebugMaze();
            SetNeighbors();
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

    bool GenerateMaze()
    {
        answer = null;

        //dont have purple spawn on edges

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

        return GetThroughMaze();
    }

    void GenereatDebugMaze()
    {
        Material[,] grid1 = new Material[,]
        {
            { materials[2], materials[5], materials[0], materials[1], materials[0], materials[3], materials[0], materials[3]},
            { materials[5], materials[1], materials[4], materials[4], materials[4], materials[4], materials[3], materials[2]},
            { materials[5], materials[2], materials[2], materials[5], materials[5], materials[1], materials[2], materials[2]},
            { materials[0], materials[3], materials[1], materials[3], materials[4], materials[4], materials[0], materials[5]},
            { materials[0], materials[1], materials[4], materials[3], materials[2], materials[3], materials[1], materials[3]},
            { materials[0], materials[2], materials[1], materials[1], materials[5], materials[5], materials[2], materials[0]},

        };

        for (int row = 0; row < 6; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                int index = row * 8 + col;
                grid[row, col] = new Cell(row, col, buttons[index], grid1[row, col]);

                buttons[index].OnInteract += delegate () { KeypadPress(buttons[index]); return false; };
            }
        }

        answer = FindPath(grid[2, 0], grid[2, 7]);

        if (answer == null)
        {
            Debug.Log("Could not find path from (2 0), (2, 7)");
        }

        //GetThroughMaze();
    }

    bool GetThroughMaze()
    {
        List<Cell> startingCells = new List<Cell>();
        List<Cell> endingCells = new List<Cell>();

        for (int i = 0; i < 6; i++)
        {
            if (grid[i, 0].Tile.ToString() != "Red")
            {
                startingCells.Add(grid[i, 0]);
            }

            if (grid[i, 7].Tile.ToString() != "Red")
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


                if (answer != null && !ValidPath(answer))
                {
                    answer = null;
                }

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
            Debug.Log("Final Answer: " + LogList(answer));
        }

        return answer != null;
    }

    void SetNeighbors()
    {
        for (int row = 0; row < 6; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                Cell c = grid[row, col];
                c.Up = row - 1 < 0 ? null : grid[row - 1, col];
                c.Down = row + 1 > 5 ? null : grid[row + 1, col];
                c.Left = col - 1 < 0 ? null : grid[row, col - 1];
                c.Right = col + 1 > 7 ? null : grid[row, col + 1];
            }
        }
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
        Debug.Log($"Start at {start}. End at {end}");

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
                Debug.Log("Path to end was found");
                break;
            }

            List<Cell> neighbors = new List<Cell> { next.Up, next.Left, next.Down, next.Right };

            neighbors = GetRidOfBadNeighbors(neighbors, next, q);

            foreach (Cell c in neighbors)
            {
                q.Enqueue(c);
                allMoves.Add(new Movement(next,c));
            }

            LogList(neighbors);

            count++;
        }

        if (count == limit)
        {
            Debug.Log("Infinite loop has been found");
            return null;
        }

        //start at end and work backwards to start
        Movement lastMove;

        try
        {
            Debug.Log("All moves count: " + allMoves.Count);
            lastMove = allMoves.First(x => x.end == end);
        }

        catch
        {
            Debug.Log("First move to the end can't be found");
            return null;
        }

        List<Movement> relMovements = new List<Movement>() { lastMove };

        while (lastMove.start != start)
        {
            lastMove = allMoves.First(x => x.end == lastMove.start);
            relMovements.Add(lastMove);
        }

        List<Cell> tempPath = new List<Cell>() { start };

        for (int i = relMovements.Count - 1; i > -1; i--)
        {
            tempPath.Add(relMovements[i].end);
        }

        Debug.Log(LogList(tempPath));

        return tempPath;

    }

    void RegenerateCell(Cell c)
    {
        while (c.Tile.ToString() == "Purple")
        {
            c.SetRandomMaterial();
        }
    }

    List<Cell> GetRidOfBadNeighbors(List<Cell> list, Cell current, Queue<Cell> q)
    {

        //we are not checking for smells becaue that's hard

        List<Cell> newList = new List<Cell>();

        for (int i = 0; i < list.Count; i++)
        {
            Cell c = list[i];

            //if it's null, ....yeah
            if (c == null || c.Visited)
            {
                continue;
            }

            //if it's red it's not safe
            if (c.Tile.ToString() == "Red")
            {
                continue;
            }

            //if it's in the queue and not purple dont add it
            if (c.Tile.ToString() == "Purple" && q.Contains(c))
            {
                continue;
            }

            //if it's purple, check next cell:
            if (c.Tile.ToString() == "Purple")
            {
                Cell next = current.Up == c ? c.Up : current.Right == c ? c.Right : current.Down == c ? c.Down : c.Left;

                //-if next cell is red it's not safe
                if (next.Tile.ToString() == "Red")
                {
                    continue;
                }
            }
            newList.Add(c);
        }

        return newList;
    }

    private string LogList(List<Cell> list)
    {
        return string.Join(" ", list.Select(x => x.ToString()).ToArray());
    }

    bool ValidPath(List<Cell> path)
    {
        Smell smell = Smell.None;


        for (int i = 0; i < path.Count; i++)
        {
            Cell c = path[i];

            if (c.GetColor() == "Orange")
            {
                smell = Smell.Orange;
            }

            else if (c.GetColor() == "Purple")
            {
                smell = Smell.Lemon;
            }

            if (i != path.Count - 1)
            {
                Cell next = path[i + 1];

                if (smell == Smell.Orange && c.GetColor() == "Blue")
                {
                    return false;
                }

                //if you on a purple cell, and you are not going the same direction that you wernt before, this is not valid
                if (i == 0)
                {
                    continue;
                }
                Cell previous = path[i - 1];

                string direction = previous.Up == c ? "Up" : previous.Right == c ? "Right" : previous.Down == c ? "Down" : "Left";

                bool b;
                switch (direction)
                {
                    case "Up":
                        b = path[i].Up == next;
                        break;
                    case "Right":
                        b = path[i].Right == next;
                        break;
                    case "Down":
                        b = path[i].Down == next;
                        break;
                    default:
                        b = path[i].Left == next;
                        break;
                }

                if (!b)
                {
                    return false;
                }
            }
        }

        return true;
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
