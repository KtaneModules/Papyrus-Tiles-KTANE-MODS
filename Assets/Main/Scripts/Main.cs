using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;
using UnityEngine.UI;
using System;


public class Main : MonoBehaviour
{
    //todo add the monster shaking when hit
    //todo remove the line in the manual that says the monster health scales
    //todo reword the manual: once a green button is pressed the module will switch to fighting mode
    //todo add in the manual press space will cause an atack



    private KMBombInfo Bomb;
    private KMAudio Audio;

    private Cell[,] grid;

    private KMSelectable[] buttons;

    [SerializeField]
    private Animator animator;

    [SerializeField]
    private SpriteRenderer spriteRenderer;

    [SerializeField]
    private Material[] materials; // red, orange, green, blue, purple, pink

    [SerializeField]
    private GameObject orange;

    [SerializeField]
    private GameObject lemon;

    [SerializeField]
    private GameObject heart;

    [SerializeField]
    private GameObject exclamationPoint;

    [SerializeField]
    private GameObject gridGameObject;

    [SerializeField]
    private GameObject fightingGameObjects;

    [SerializeField]
    private Material[] enemyMaterials;

    [SerializeField]
    private GameObject bar;

    [SerializeField]
    private MeshRenderer enemyRenderer;

    [SerializeField]
    private Image currentHealthBar;
    private RectTransform rectTransform;

    [SerializeField]
    private AudioClip[] audioClips; //knife, encounter 1, encounter 2, love, hit, walk, victory, chomp

    private KMSelectable resetButon;

    private bool focused;

    private Smell currentSmell;

    private bool recursionAtGoal;
    private Cell recursionCurrentCell;
    private List<Cell> recursionCellList;
    private List<Cell> recursionCellListSimplified;

    private List<string> recursionDirections;

    static int ModuleIdCounter = 1;
    private int ModuleId;
    private bool ModuleSolved;
    private bool debug = false;
    private bool pressable;
    private bool fightingMonster;
    private float monsterHealth;
    private float maxHealth;
    private float currentPercentage;
    


    void Awake()
    {
        exclamationPoint.SetActive(false);
        rectTransform = currentHealthBar.GetComponent<RectTransform>();
        Bomb = GetComponent<KMBombInfo>();
        Audio = GetComponent<KMAudio>();
        ModuleSolved = false;
        ModuleId = ModuleIdCounter++;

        heart.SetActive(false);
        KMSelectable[] tempButtons = GetComponent<KMSelectable>().Children;

        buttons = Enumerable.Range(0, tempButtons.Length - 1).Select(i => tempButtons[i]).ToArray();

        resetButon = tempButtons[tempButtons.Length - 1];

        resetButon.OnInteract += delegate () { if (pressable && !fightingMonster && !ModuleSolved) ResetModule(); return false; };


        Cell.redMaterial = materials[0];
        Cell.orangeMaterial = materials[1];
        Cell.greenMaterial = materials[2];
        Cell.blueMaterial = materials[3];
        Cell.purpleMaterial = materials[4];
        Cell.pinkMaterial = materials[5];

        GetComponent<KMSelectable>().OnFocus += delegate () { focused = true; };
        GetComponent<KMSelectable>().OnDefocus += delegate () { focused = false; };

        grid = new Cell[6, 8];
        if (!debug)
        {
            for (int row = 0; row < 6; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    int index = row * 8 + col;
                    grid[row, col] = new Cell(row, col, buttons[index]);

                    buttons[index].OnInteract += delegate () { if(pressable && !fightingMonster && !ModuleSolved) StartCoroutine(ButtonPress(buttons[index])); return false; };
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

            if (count == 100 && !validMaze)
            {
                Logging("Couldn't generate a good maze. Generate default maze...");

                for (int row = 0; row < 6; row++)
                {
                    Tile tile = row == 2 || row == 3 ? Tile.Pink : Tile.Red;

                    for (int col = 0; col < 8; col++)
                    {
                        int index = row * 8 + col;
                        grid[row, col] = new Cell(row, col, buttons[index], tile);
                    }
                }
            }
        }

        else
        {
            GenerateDebugMaze();
            SetNeighbors();
        }

        SetSmell(Smell.None);
    }

    bool GenerateMaze()
    {
        //dont have purple spawn on edges

        foreach (Cell c in grid)
        {
            c.SetRandomTile();
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

        GetThroughMaze();

       return AtGoal();
    }

    void GenerateDebugMaze()
    {
        int[,] grid1 = new int[,]
        {
            { 2, 5, 0, 1, 0, 3, 0, 3},
            { 5, 1, 4, 4, 4, 4, 3, 2},
            { 5, 2, 2, 5, 5, 1, 2, 2},
            { 0, 3, 1, 3, 4, 4, 0, 5},
            { 0, 1, 4, 3, 2, 3, 1, 3},
            { 0, 2, 1, 1, 5, 5, 2, 0},
        };

        int[,] grid2 = new int[,]
        {
            { 0, 3, 3, 0, 3, 2, 3, 5},
            {1, 5, 5, 5, 3, 0, 2, 1},
            { 0, 0, 2, 2, 4, 2, 3, 5},
            { 0, 2, 2, 1, 1, 3, 2, 0},
            { 3, 3, 1, 5, 4, 3, 4, 1},
            { 1, 1, 2, 1, 2, 0, 1, 2},
        };

        int[,] grid3 = new int[,]
        {
            { 1, 0, 0, 1, 3, 0, 5, 2},
            { 1, 4, 3, 3, 1, 5, 4, 3},
            { 2, 3, 1, 0, 2, 4, 0, 1},
            { 0, 5, 4, 4, 0, 1, 0, 2},
            { 5, 2, 3, 1, 0, 5, 2, 5},
            { 0, 5, 2, 2, 1, 1, 2, 4},
        };

        int[,] grid4 = new int[,]
        {
            { 3, 0, 1, 5, 0, 1, 1, 3},
            { 1, 4, 5, 3, 4, 4, 1, 2},
            { 5, 1, 4, 2, 2, 2, 2, 3},
            { 3, 0, 3, 4, 0, 2, 4, 5},
            { 2, 3, 0, 2, 2, 2, 4, 3},
            { 2, 3, 0, 1, 1, 2, 1, 5},
        };

        int[,] grid5 = new int[,]
        {
            { 1, 1, 1, 5, 1, 5, 0, 3},
            { 5, 2, 1, 3, 1, 4, 2, 0},
            { 3, 3, 1, 1, 2, 4, 4, 3},
            { 1, 1, 5, 2, 0, 3, 3, 2},
            { 3, 0, 0, 1, 4, 0, 4, 5},
            { 3, 5, 2, 0, 3, 3, 2, 3},
        };

        int[,] grid6 = new int[,]
        {
        { 1, 2, 2, 2, 1, 0, 1, 5},
        { 2, 5, 0, 5, 1, 4, 1, 5},
        { 1, 2, 1, 2, 5, 4, 4, 3},
        { 5, 0, 5, 3, 4, 3, 4, 5},
        { 2, 1, 3, 5, 4, 5, 4, 2},
        { 0, 1, 2, 0, 2, 1, 2, 0},
        };

        /*
        int[,] grid2 = new int[,]
        {
        { , , , , , , , },
        { , , , , , , , },
        { , , , , , , , },
        { , , , , , , , },
        { , , , , , , , },
        { , , , , , , , },

        };*/

        for (int row = 0; row < 6; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                int index = row * 8 + col;
                grid[row, col] = new Cell(row, col, buttons[index], (Tile)grid6[row, col]);
            }
        }

        SetNeighbors();

        GetThroughMaze();
    }

    Cell FindPlayer()
    {
        foreach (Cell c in grid)
        {
            if (c.HasPlayer)
            {
                return c;
            }
        }

        return null;
    }

    IEnumerator SetPlayer(Cell currentCell, bool firstPress, float maxTime)
    {
        foreach (Cell c in grid)
        {
            if (c.HasPlayer)
            {
                c.HasPlayer = false;
            }
        }
        
        currentCell.HasPlayer = true;
        float elaspedTime = 0f;

        Vector3 finalDestiantion = currentCell.Button.transform.position;
        Vector3 oldHeartPosition = heart.transform.position;
        
        if (!firstPress)
        {
            if (maxTime == audioClips[5].length)
            {
                Audio.PlaySoundAtTransform(audioClips[5].name, transform);
            }
            while (elaspedTime < maxTime)
            {
                float t = elaspedTime / maxTime;
                Vector3 newPos = Vector3.Lerp(oldHeartPosition, finalDestiantion, t);
                heart.transform.position = new Vector3(newPos.x, oldHeartPosition.y, newPos.z);
                elaspedTime += Time.deltaTime;
                yield return null;
            }
        }

        else
        {
            heart.transform.position = new Vector3(finalDestiantion.x, oldHeartPosition.y, finalDestiantion.z);
        }  
    }


    void GetThroughMaze()
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

        bool foundPath = false;

        for (int startIndex = 0; startIndex < startingCells.Count; startIndex++)
        {
            for (int endIndex = 0; endIndex < endingCells.Count; endIndex++)
            {
                Cell start = startingCells[startIndex];
                Cell end = endingCells[endIndex];

                foundPath = FindPathRecursion(start, end);

                if (foundPath)
                {
                    break;
                }
            }

            if (foundPath)
            {
                break;
            }
        }

        if (foundPath)
        {
            recursionCellListSimplified = SimplifyAnswer(recursionCellList);
            Logging($"Final Answer: " + LogList(recursionCellList));
            if (recursionCellListSimplified.Count == recursionCellList.Count)
            {
                Logging($"Final Answer (Simplified): " + LogList(recursionCellListSimplified));
            }
            Debug.Log($"#{ModuleId} {(recursionCellListSimplified.Count == recursionCellList.Count ? "Lists are the same" : "Lists are different")}");
        }

        else
        {
            Debug.Log("Couldn't find a path to complete this maze");
        }
    }

    bool FindPathRecursion(Cell start, Cell end)
    {
        recursionDirections = new List<string>();
        recursionCellList = new List<Cell>();
        foreach (Cell c in grid)
        {
            c.Visited = false;
            c.Valid = c.Tile != Tile.Red;
        }

        recursionCurrentCell = start;
        recursionCellList.Add(start);

        if (MoveNorth(end) || MoveEast(end) || MoveSouth(end) || MoveWest(end))
        {
            return true;
        }

        else
        {
            Debug.Log($"Could not find path from {start} to {end}");
            return false;
        }

    }

    bool MoveNorth(Cell end)
    {
        //if we can move up, and we didnt move done before, go up

        Cell next = recursionCurrentCell.Up;


        Debug.Log($"Attempting to move up to {next}\n");

        bool validPathMovingNorth = false;
        bool checkValidity = false;
        if (next != null && (recursionDirections.Count == 0 || recursionDirections.Last() != "DOWN") && next.Valid && UnVistedNonPurpleCell(next))
        {
            recursionCurrentCell = recursionCurrentCell.Up;
            recursionDirections.Add("UP");
            recursionCellList.Add(recursionCurrentCell);
            Debug.Log($"Now at {next}\n");

            //check to see if this is valid path so far
            validPathMovingNorth = ValidPath(recursionCellList);
            checkValidity = true;
        }

        //if he player is at the goal, set goal as true
        if (AtGoal(end) && validPathMovingNorth)
        {
            recursionAtGoal = true;
        }

        else
        {
            //only continue to go up if path is valid
            if (validPathMovingNorth)
            {
                recursionAtGoal = MoveNorth(end);

                //if movig north doesn't work, move east
                if (!recursionAtGoal)
                {
                    recursionAtGoal = MoveEast(end);

                    //if moving east doesn't work, move south
                    if (!recursionAtGoal)
                    {
                        recursionAtGoal = MoveSouth(end);

                        //if moving south doesn't work, move west
                        if (!recursionAtGoal)
                        {
                            recursionAtGoal = MoveWest(end);

                            //if moving west doesn't work, mark this position as
                            //unavailable and move back south
                            if (!recursionAtGoal)
                            {
                                Debug.Log($"Up doesn't lead anywhere. Moving back down.\n");
                                recursionCurrentCell.Valid = false;
                                recursionCurrentCell = recursionCurrentCell.Down;
                                recursionDirections.Remove(recursionDirections.Last());
                                recursionCellList.Remove(recursionCellList.Last());
                            }
                        }
                    }
                }
            }

            //otherwise, go back down and say you couldnt go north
            else
            {
                if (checkValidity)
                {
                    recursionAtGoal = false;
                    recursionCurrentCell = recursionCurrentCell.Down;
                    recursionDirections.Remove(recursionDirections.Last());
                    recursionCellList.Remove(recursionCellList.Last());
                }

                Debug.Log($"Moving up lead to an invalid path. Going back down to {recursionCurrentCell}\n");
            }
        }

        return recursionAtGoal;
    }

    bool MoveEast(Cell end)
    {
        //if we can move east, and we didnt move west before, go east
        Cell next = recursionCurrentCell.Right;
        Debug.Log($"Attempting to move right to {next}\n");


        bool validPathMovingEast = false;
        bool checkValidity = false;

        if (next != null && (recursionDirections.Count == 0 || recursionDirections.Last() != "LEFT") && next.Valid && UnVistedNonPurpleCell(next))
        {
            recursionCurrentCell = recursionCurrentCell.Right;
            recursionDirections.Add("RIGHT");
            recursionCellList.Add(recursionCurrentCell);
            Debug.Log($"Now at {next}\n");

            //check to see if this is valid path so far
            validPathMovingEast = ValidPath(recursionCellList);
            checkValidity = true;

        }

        //if he player is at the goal, set goal as true
        if (AtGoal(end) && validPathMovingEast)
        {
            recursionAtGoal = true;
        }

        else
        {
            //only continue to go east if path is valid
            if (validPathMovingEast)
            {
                recursionAtGoal = MoveEast(end);

                //if movig east doesn't work, move south
                if (!recursionAtGoal)
                {
                    recursionAtGoal = MoveSouth(end);

                    //if moving south doesn't work, move west
                    if (!recursionAtGoal)
                    {
                        recursionAtGoal = MoveWest(end);

                        //if moving west doesn't work, move north
                        if (!recursionAtGoal)
                        {
                            recursionAtGoal = MoveNorth(end);

                            //if moving east doesn't work, mark this position as
                            //unavailable and move back west
                            if (!recursionAtGoal)
                            {
                                Debug.Log($"Right doesn't lead anywhere. Moving back Left.\n");
                                recursionCurrentCell.Valid = false;
                                recursionCurrentCell = recursionCurrentCell.Left;
                                recursionDirections.Remove(recursionDirections.Last());
                                recursionCellList.Remove(recursionCellList.Last());
                            }
                        }
                    }
                }
            }

            //otherwise, go back weast and say you couldnt go east
            else
            {
                if (checkValidity)
                {
                    recursionAtGoal = false;
                    recursionCurrentCell = recursionCurrentCell.Left;
                    recursionDirections.Remove(recursionDirections.Last());
                    recursionCellList.Remove(recursionCellList.Last());
                }

                Debug.Log($"Moving right lead to an invalid path. Going back left to {recursionCurrentCell}\n");
            }
        }

        return recursionAtGoal;
    }

    bool MoveSouth(Cell end)
    {
        //if we can move south, and we didnt move north before, go south
        Cell next = recursionCurrentCell.Down;
        Debug.Log($"Attempting to move down to {next}\n");
        bool validPathMovingSouth = false;
        bool checkValidity = false;


        if (next != null && (recursionDirections.Count == 0 || recursionDirections.Last() != "UP") && next.Valid && UnVistedNonPurpleCell(next))
        {
            recursionCurrentCell = recursionCurrentCell.Down;
            recursionDirections.Add("DOWN");
            recursionCellList.Add(recursionCurrentCell);
            Debug.Log($"Now at {next}\n");

            //check to see if this is valid path so far
            validPathMovingSouth = ValidPath(recursionCellList);
            checkValidity = true;
        }

        //if he player is at the goal, set goal as true
        if (AtGoal(end) && validPathMovingSouth)
        {
            recursionAtGoal = true;
        }

        else
        {
            //only continue to go south if path is valid
            if (validPathMovingSouth)
            {
                recursionAtGoal = MoveSouth(end);

                //if movig south doesn't work, move west
                if (!recursionAtGoal)
                {
                    recursionAtGoal = MoveWest(end);

                    //if moving west doesn't work, move north
                    if (!recursionAtGoal)
                    {
                        recursionAtGoal = MoveNorth(end);

                        //if moving north doesn't work, move east
                        if (!recursionAtGoal)
                        {
                            recursionAtGoal = MoveEast(end);

                            //if moving east doesn't work, mark this position as
                            //unavailable and move back north
                            if (!recursionAtGoal)
                            {
                                Debug.Log($"Down doesn't lead anywhere. Moving back up.\n");
                                recursionCurrentCell.Valid = false;
                                recursionCurrentCell = recursionCurrentCell.Up;
                                recursionDirections.Remove(recursionDirections.Last());
                                recursionCellList.Remove(recursionCellList.Last());
                            }
                        }
                    }
                }
            }

            //otherwise, go back north and say you couldnt go south
            else
            {
                if (checkValidity)
                {
                    recursionAtGoal = false;
                    recursionCurrentCell = recursionCurrentCell.Up;
                    recursionDirections.Remove(recursionDirections.Last());
                    recursionCellList.Remove(recursionCellList.Last());
                }

                Debug.Log($"Moving down lead to an invalid path. Going back up to {recursionCurrentCell}\n");
            }
        }

        return recursionAtGoal;
    }

    bool MoveWest(Cell end)
    {
        //if we can move west, and we didnt move east before, go west
        Cell next = recursionCurrentCell.Left;
        Debug.Log($"Attempting to move left to {next}\n");
        bool validPathMovingWest = false;
        bool checkValidity = false;

        if (next != null && (recursionDirections.Count == 0 || recursionDirections.Last() != "RIGHT") && next.Valid && UnVistedNonPurpleCell(next))
        {
            recursionCurrentCell = recursionCurrentCell.Left;
            recursionDirections.Add("LEFT");
            recursionCellList.Add(recursionCurrentCell);
            Debug.Log($"Now at {next}\n");

            //check to see if this is valid path so far
            validPathMovingWest = ValidPath(recursionCellList);
            checkValidity = true;
        }

        //if he player is at the goal, set goal as true
        if (AtGoal(end) && validPathMovingWest)
        {
            recursionAtGoal = true;
        }

        else
        {
            //only continue to go west if path is valid
            if (validPathMovingWest)
            {
                recursionAtGoal = MoveWest(end);

                //if movig west doesn't work, move north
                if (!recursionAtGoal)
                {
                    recursionAtGoal = MoveNorth(end);

                    //if moving north doesn't work, move east
                    if (!recursionAtGoal)
                    {
                        recursionAtGoal = MoveEast(end);

                        //if moving east doesn't work, move south
                        if (!recursionAtGoal)
                        {
                            recursionAtGoal = MoveSouth(end);

                            //if moving south doesn't work, mark this position as
                            //unavailable and move back east
                            if (!recursionAtGoal)
                            {
                                Debug.Log($"Left doesn't lead anywhere. Moving back right.\n");
                                recursionCurrentCell.Valid = false;
                                recursionCurrentCell = recursionCurrentCell.Right;
                                recursionDirections.Remove(recursionDirections.Last());
                                recursionCellList.Remove(recursionCellList.Last());
                            }
                        }
                    }
                }
            }

            //otherwise, go back east and say you couldnt go west
            else
            {
                if (checkValidity)
                {
                    recursionAtGoal = false;
                    recursionCurrentCell = recursionCurrentCell.Right;
                    recursionDirections.Remove(recursionDirections.Last());
                    recursionCellList.Remove(recursionCellList.Last());
                }

                Debug.Log($"Moving left lead to an invalid path. Going back right to {recursionCurrentCell}\n");
            }
        }

        return recursionAtGoal;
    }

    bool UnVistedNonPurpleCell(Cell c)
    {
        if (!recursionCellList.Contains(c))
        {
            return true;
        }

        if (recursionCellList.Contains(c) && c.Tile == Tile.Purple)
        {
            return true;
        }

        return false;
    }

    bool AtGoal(Cell end)
    {
        return recursionCellList.Contains(end);
    }

    bool AtGoal()
    {
        return recursionCellList.Any(x => x.Col == 7);
    }

    bool ValidPath(List<Cell> path)
    {
        Smell smell = Smell.None;

        for (int i = 0; i < path.Count; i++)
        {
            Cell c = path[i];
            string color = c.GetColor();
            //cant land on blue smelling like oranges
            if (color == "Blue" && smell == Smell.Orange)
            {
                return false;
            }

            if (color == "Orange")
            {
                smell = Smell.Orange;
            }

            else if (color == "Purple")
            {
                smell = Smell.Lemon;

                if (i != path.Count - 1)
                {
                    Cell next = path[i + 1];

                    //if you on a purple cell, and you are not going the same direction that you wernt before, this is not valid

                    Cell previous = path[i - 1];
                    Cell actualNext = previous.Up == c ? c.Up : previous.Right == c ? c.Right : previous.Down == c ? c.Down : c.Left;

                    if (actualNext != next)
                    {
                        return false;
                    }

                    if (next.GetColor() == "Red")
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    List<Cell> SimplifyAnswer(List<Cell> list)
    {
        List<Cell> newList = new List<Cell>();

        for (int i = 0; i < list.Count; i++)
        {
            Cell current = list[i];
            Cell previous = null;

            if (i != 0)
            {
                previous = list[i - 1];
            }

            newList.Add(current);

            if (previous != null && previous.GetColor() == "Purple")
            {
                newList.Remove(newList.Last());
            }

            if (newList.Any(x => x.Col == 7) || current.Col == 7)
            {
                if (current.Col == 7)
                {
                    newList.Add(current);
                }
                break;
            }

        }

        return newList.Distinct().ToList();
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
        foreach (Cell c in grid)
        {
            c.HasPlayer = false;
        }

        heart.SetActive(false);
        SetSmell(Smell.None);
    }

    IEnumerator ButtonPress(KMSelectable button)
    {
        pressable = false;
        button.AddInteractionPunch(.1f);
        Cell selectedCell = GetCell(button);
        Cell playerCell = FindPlayer();

        float walkingTime = audioClips[5].length;
        float runningTime = audioClips[7].length;

        //if the user is not on the grid make sure they press a button in the first column
        if (playerCell == null)
        {
            if (selectedCell.Col != 0)
            {
                pressable = true;
                yield break;
            }

            else
            {
                if (selectedCell.Tile == Tile.Red)
                {
                    pressable = true;
                    yield break;
                }

                if (selectedCell.Tile == Tile.Orange)
                {
                    SetSmell(Smell.Orange);
                }

                Logging("Pressed " + selectedCell.ToString());
                heart.SetActive(true);
                yield return SetPlayer(selectedCell, true, walkingTime);

                if (selectedCell.Tile == Tile.Green)
                {
                    yield return HandleGreenTiles();
                }
                pressable = true;
            }
        }

        else
        {
            //only neighbor cells can be interacted wih

            List<Cell> neighbors = playerCell.Neighbors;

            if (!neighbors.Contains(selectedCell))
            {
                yield break;
            }

            else
            {
                if (selectedCell.Tile == Tile.Red)
                {
                    pressable = true;
                    yield break;
                }

                Logging("Pressed: " + selectedCell.ToString());
                Debug.Log("fighting: " + fightingMonster);
                Debug.Log("pressable: " + pressable);

                switch (selectedCell.Tile)
                {
                    case Tile.Orange:
                        Debug.Log("orange");
                        SetSmell(Smell.Orange);
                        break;
                    case Tile.Blue:
                        if (currentSmell == Smell.Orange)
                        {
                            Debug.Log("blue");
                            yield return SetPlayer(selectedCell, false, walkingTime);
                            //todo have a chomping noise play
                            Audio.PlaySoundAtTransform(audioClips[7].name, transform);
                            Strike("Strike! Got bit by pirahnas. Moving back to " + playerCell.ToString());
                            yield return SetPlayer(playerCell, false, runningTime);
                            pressable = true;
                            yield break;
                        }
                        break;
                    case Tile.Purple:
                        Debug.Log("purple");

                        string direction = playerCell.Up == selectedCell ? "up" :
                                           playerCell.Down == selectedCell ? "down" :
                                           playerCell.Right == selectedCell ? "right" : "left";

                        Cell currentCell = playerCell;

                        do
                        {
                            Cell nextCell = direction == "up" ? currentCell.Up :
                                            direction == "down" ? currentCell.Down :
                                            direction == "right" ? currentCell.Right : currentCell.Left;

                            yield return SetPlayer(nextCell, false, walkingTime);
                            SetSmell(Smell.Lemon);
                            currentCell = nextCell;

                        } while (currentCell.Tile == Tile.Purple);

                        Logging("Moved to " + currentCell.ToString());

                        if (currentCell.Tile == Tile.Red)
                        {
                            Strike("Strike! Slid to a red tile. Restarting module...");
                            ResetModule();
                        }

                        else if (currentCell.Tile == Tile.Orange)
                        {
                            SetSmell(Smell.Orange);
                        }

                        else if (currentCell.Tile == Tile.Green)
                        {
                            yield return HandleGreenTiles();
                        }

                        else if (currentCell.Col == 7)
                        {
                            Solve();
                        }
                        pressable = true;
                        yield break;

                    case Tile.Green:
                        Debug.Log("green");

                        yield return SetPlayer(selectedCell, false, walkingTime);
                        yield return HandleGreenTiles();
                        pressable = true;
                        yield break;
                }
                yield return SetPlayer(selectedCell, false, walkingTime);
                pressable = true;


                if (FindPlayer().Col == 7)
                {
                    Solve();
                }
            }
        }
    }
    IEnumerator HandleGreenTiles()
    {
        fightingMonster = true;
        enemyRenderer.materials = new Material[] { enemyMaterials[Rnd.Range(0, enemyMaterials.Length)] };
        currentHealthBar.rectTransform.anchorMax = new Vector2(1, 1);
        Vector3 heartPos = heart.transform.localPosition;
        exclamationPoint.transform.localPosition = new Vector3(heartPos.x, heartPos.y, heartPos.z + 0.00961666f);
        exclamationPoint.SetActive(true);
        monsterHealth = 9;
        maxHealth = 9;
        currentPercentage = 1f;
        //todo play sound of fight encounter
        Audio.PlaySoundAtTransform(audioClips[1].name, transform);
        yield return new WaitForSeconds(audioClips[1].length + .1f);
        float flashLength = 0.12f;
        exclamationPoint.SetActive(false);
        Audio.PlaySoundAtTransform(audioClips[2].name, transform);

        for (int i = 0; i < 3; i++)
        {
            heart.SetActive(false);
            yield return new WaitForSeconds(flashLength / 2);
            heart.SetActive(true);
            yield return new WaitForSeconds(flashLength / 2);
        }

        yield return new WaitForSeconds(audioClips[2].length - flashLength - .6f);
        gridGameObject.SetActive(false);
        fightingGameObjects.SetActive(true);

        do 
        {
            yield return MoveBar();

        } while (monsterHealth > 0);
        Audio.PlaySoundAtTransform(audioClips[3].name, transform);
        fightingMonster = false;
        fightingGameObjects.SetActive(false);
        gridGameObject.SetActive(true);
        fightingMonster = false;
    }

    IEnumerator MoveBar()
    {
        bool spacePress = false;
        float moveWhiteMaxTime = 1.25f;
        float elaspedTime;


        Vector3 leftPos = new Vector3(-0.1674f, -0.0633f, 0.0541f);
        Vector3 rightPos = new Vector3(-0.0079f, -0.0633f, 0.0541f);

        bar.transform.localPosition = leftPos;
        do
        {
            elaspedTime = 0f;
            while (elaspedTime < moveWhiteMaxTime)
            {
                if (focused && Input.GetKeyDown(KeyCode.Space))
                {
                    spacePress = true;
                    break;
                }

                float t = elaspedTime / moveWhiteMaxTime;
                bar.transform.localPosition = Vector3.Lerp(leftPos, rightPos, t);
                elaspedTime += Time.deltaTime;
                yield return null;
            }

            if (spacePress)
            {
                break;
            }
            elaspedTime = 0f;
            while (elaspedTime < moveWhiteMaxTime)
            {
                if (focused && Input.GetKeyDown(KeyCode.Space))
                {
                    spacePress = true;
                    break;
                }

                float t = elaspedTime / moveWhiteMaxTime;
                bar.transform.localPosition = Vector3.Lerp(rightPos, leftPos, t);
                elaspedTime += Time.deltaTime;
                yield return null;
            }

        } while (!spacePress);

        animator.SetTrigger("Trigger Knife Swing");
        Audio.PlaySoundAtTransform(audioClips[0].name, transform);
        yield return new WaitForSeconds(audioClips[0].length);

        Audio.PlaySoundAtTransform(audioClips[4].name, transform);

        //deplete health

        float[] greenHit = new float[] { -0.0881f, -0.0868f };
        float[] yellowHit = new float[] { -0.1159f, -0.0586f };

        float barY = bar.transform.localPosition.y;
        if (barY >= greenHit[0] && barY <= greenHit[1])
        {
            monsterHealth -= maxHealth / 2;
        }

        else if (barY >= yellowHit[0] && barY <= yellowHit[1])
        {
            monsterHealth -= maxHealth / 3;
        }

        else
        {
            monsterHealth -= maxHealth / 4;
        }

        float newPercentage = monsterHealth / maxHealth;

        elaspedTime = 0f;
        float maxDepleteHealthTime = audioClips[4].length;
        while (elaspedTime < maxDepleteHealthTime)
        {
            float t = elaspedTime / maxDepleteHealthTime;
            rectTransform.anchorMax = new Vector2(Mathf.Lerp(currentPercentage, newPercentage, t), 1f);
            elaspedTime += Time.deltaTime;
            yield return null;
        }

        rectTransform.anchorMax = new Vector2(newPercentage, 1f);
        currentPercentage = newPercentage;
    }

    void Start()
    {
        gridGameObject.SetActive(true);
        fightingGameObjects.SetActive(false);
        pressable = true;
        fightingMonster = false;
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

    void RegenerateCell(Cell c)
    {
        while (c.Tile.ToString() == "Purple")
        {
            c.SetRandomTile();
        }
    }

    private string LogList(List<Cell> list)
    {
        return string.Join(" ", list.Select(x => x.ToString()).ToArray());
    }

    private void Logging(string s)
    {
        if (s == "")
        {
            return;
        }

        Debug.LogFormat($"[Papyrus Tiles #{ModuleId}] {s}");
            
    }

    private void Solve()
    {
        Audio.PlaySoundAtTransform(audioClips[6].name, transform);
        Logging("Module solved");
        GetComponent<KMBombModule>().HandlePass();
        ModuleSolved = true;
    }

    private void Strike(string s)
    {
        GetComponent<KMBombModule>().HandleStrike();
        Logging(s);
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
