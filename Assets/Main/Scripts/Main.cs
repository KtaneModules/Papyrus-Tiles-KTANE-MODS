using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Rnd = UnityEngine.Random;


public class Main : MonoBehaviour
{
    //todo tp x
    //todo autosolve x
    //todo colorblind

    

    private KMAudio Audio;

    private Cell[,] grid;

    private bool colorBlindOn;

    [SerializeField]
    private TextMesh cbTextPrefab;

    [SerializeField]
    private CellSelectable[] buttons;

    [SerializeField]
    private Animator animator;

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

    private KMSelectable resetButton;

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
    private bool printDebugLines = false;
    private bool spacePress;
    private bool tpSpacePress;

    SpriteRenderer barSpriteRenderer;
    float[] greenHit = new float[] { -0.0927f, -0.0815f };
    float[] yellowHit = new float[] { -0.1159f, -0.0586f };



    void Awake()
    {
        colorBlindOn = GetComponent<KMColorblindMode>().ColorblindModeActive;
        barSpriteRenderer = bar.transform.GetComponent<SpriteRenderer>();
        exclamationPoint.SetActive(false);
        rectTransform = currentHealthBar.GetComponent<RectTransform>();
        Audio = GetComponent<KMAudio>();
        ModuleSolved = false;
        ModuleId = ModuleIdCounter++;

        heart.SetActive(false);

        resetButton = GetComponent<KMSelectable>().Children.Last();

        
        resetButton.OnInteract += delegate () { if (pressable && !fightingMonster && !ModuleSolved) { resetButton.AddInteractionPunch(.1f); ResetModule(); } return false; };


        Cell.red = materials[0].color;
        Cell.orange = materials[1].color;
        Cell.green = materials[2].color;
        Cell.blue = materials[3].color;
        Cell.purple = materials[4].color;
        Cell.pink = materials[5].color;

        GetComponent<KMSelectable>().OnFocus += delegate () { focused = true; };
        GetComponent<KMSelectable>().OnDefocus += delegate () { focused = false; };

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

        //color blind
        if (colorBlindOn)
        {
            foreach (Cell c in grid)
            {
                Transform parentTransform = c.Button.transform;
                c.ColorBlidTextMesh = Instantiate(cbTextPrefab, parentTransform);
                string color = c.GetColor();
                c.ColorBlidTextMesh.text = color == "Pink" ? "I" : "" + color[0];
            }
        }


        GetThroughMaze();

       return AtGoal();
    }

    void GenerateDebugMaze()
    {

        int[,] grid = new int[,]
       {
        {(int)Tile.Blue,(int)Tile.Blue ,(int)Tile.Blue ,(int)Tile.Pink ,(int)Tile.Orange ,(int)Tile.Red ,(int)Tile.Pink ,(int)Tile.Blue },
        {(int)Tile.Blue,(int)Tile.Blue ,(int)Tile.Blue ,(int)Tile.Purple ,(int)Tile.Pink ,(int)Tile.Red ,(int)Tile.Green ,(int)Tile.Blue },
        {(int)Tile.Blue,(int)Tile.Blue ,(int)Tile.Blue ,(int)Tile.Green ,(int)Tile.Blue ,(int)Tile.Pink ,(int)Tile.Purple ,(int)Tile.Orange },
        {(int)Tile.Blue,(int)Tile.Blue ,(int)Tile.Blue ,(int)Tile.Pink ,(int)Tile.Red ,(int)Tile.Blue ,(int)Tile.Pink ,(int)Tile.Green },
        {(int)Tile.Blue,(int)Tile.Blue ,(int)Tile.Blue ,(int)Tile.Purple ,(int)Tile.Green ,(int)Tile.Blue ,(int)Tile.Orange ,(int)Tile.Orange },
        {(int)Tile.Blue,(int)Tile.Blue ,(int)Tile.Blue ,(int)Tile.Orange ,(int)Tile.Orange ,(int)Tile.Blue ,(int)Tile.Blue ,(int)Tile.Red },
       };

        /*
        int[,] grid2 = new int[,]
        {
       {(int)Tile.,(int)Tile. ,(int)Tile. ,(int)Tile. ,(int)Tile. ,(int)Tile. ,(int)Tile. ,(int)Tile. },
        {(int)Tile.,(int)Tile. ,(int)Tile. ,(int)Tile. ,(int)Tile. ,(int)Tile. ,(int)Tile. ,(int)Tile. },
        {(int)Tile.,(int)Tile. ,(int)Tile. ,(int)Tile. ,(int)Tile. ,(int)Tile. ,(int)Tile. ,(int)Tile. },
        {(int)Tile.,(int)Tile. ,(int)Tile. ,(int)Tile. ,(int)Tile. ,(int)Tile. ,(int)Tile. ,(int)Tile. },
        {(int)Tile.,(int)Tile. ,(int)Tile. ,(int)Tile. ,(int)Tile. ,(int)Tile. ,(int)Tile. ,(int)Tile. },
        {(int)Tile.,(int)Tile. ,(int)Tile. ,(int)Tile. ,(int)Tile. ,(int)Tile. ,(int)Tile. ,(int)Tile. },

        };*/

        for (int row = 0; row < 6; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                int index = row * 8 + col;
                this.grid[row, col] = new Cell(row, col, buttons[index], (Tile)grid[row, col]);
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
                break;
            }
        }
        
        currentCell.HasPlayer = true;
        float elaspedTime = 0f;

        Vector3 finalDestination = currentCell.Button.transform.localPosition;
        Vector3 oldHeartPosition = heart.transform.localPosition;
        
        if (!firstPress)
        {
            if (maxTime == audioClips[5].length)
            {
                Audio.PlaySoundAtTransform(audioClips[5].name, transform);
            }
            while (elaspedTime < maxTime)
            {
                float t = elaspedTime / maxTime;
                Vector3 newPos = Vector3.Lerp(oldHeartPosition, finalDestination, t);
                heart.transform.localPosition = new Vector3(newPos.x, oldHeartPosition.y, newPos.z);
                elaspedTime += Time.deltaTime;
                yield return null;
            }
        }

        else
        {
            heart.transform.localPosition = new Vector3(finalDestination.x, oldHeartPosition.y, finalDestination.z);
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
                    //check to see if the path has duplicate cells if it does, the found path should not be true.
                    //this is more of a logic error with the path finding since it's using recursion
                    //Best way to avoid this is to use an algorithm that uses shortest path
                    if (recursionCellList.Count != recursionCellList.Distinct().Count())
                    {
                        foundPath = false;
                    }

                    else
                    {
                        break;
                    }
                }
            }

            if (foundPath)
            {
                break;
            }
        }

        recursionCellListSimplified = SimplifyAnswer(recursionCellList);
    }

    bool FindPathRecursion(Cell start, Cell end)
    {
        recursionDirections = new List<string>();
        recursionCellList = new List<Cell>();
        recursionAtGoal = false;

        foreach (Cell c in grid)
        {
            c.Visited = false;
            c.Valid = c.Tile != Tile.Red;
        }

        recursionCurrentCell = start;
        recursionCellList.Add(start);

        if (MoveEast(end) || MoveSouth(end) || MoveWest(end) || MoveNorth(end))
        {
            return true;
        }

        else
        {
            if(printDebugLines)
                Debug.Log($"Could not find path from {start} to {end}");
            return false;
        }

    }

    bool MoveNorth(Cell end)
    {
        //if we can move up, and we didnt move done before, go up

        Cell next = recursionCurrentCell.Up;

        if (printDebugLines)
            Debug.Log($"Attempting to move up to {next}\n");



        bool validPathMovingNorth = false;
        bool checkValidity = false;
        if (next != null && (recursionDirections.Count == 0 || recursionDirections.Last() != "DOWN") && next.Valid && UnVistedNonPurpleCell(next))
        {
            recursionCurrentCell = recursionCurrentCell.Up;
            recursionDirections.Add("UP");
            recursionCellList.Add(recursionCurrentCell);
            if (printDebugLines)
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
                                if (printDebugLines)
                                    Debug.Log($"Up doesn't lead anywhere. Moving back down.\n");
                                recursionCurrentCell.Valid = false;
                                recursionCurrentCell = recursionCurrentCell.Down;
                                recursionDirections.RemoveAt(recursionDirections.Count - 1);
                                recursionCellList.RemoveAt(recursionCellList.Count - 1);
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
                    recursionDirections.RemoveAt(recursionDirections.Count - 1);
                    recursionCellList.RemoveAt(recursionCellList.Count - 1);
                }

                if (printDebugLines)
                    Debug.Log($"Moving up lead to an invalid path. Going back down to {recursionCurrentCell}\n");
            }
        }

        return recursionAtGoal;
    }

    bool MoveEast(Cell end)
    {
        //if we can move east, and we didnt move west before, go east
        Cell next = recursionCurrentCell.Right;
        if (printDebugLines)
            Debug.Log($"Attempting to move right to {next}\n");


        bool validPathMovingEast = false;
        bool checkValidity = false;

        if (next != null && (recursionDirections.Count == 0 || recursionDirections.Last() != "LEFT") && next.Valid && UnVistedNonPurpleCell(next))
        {
            recursionCurrentCell = recursionCurrentCell.Right;
            recursionDirections.Add("RIGHT");
            recursionCellList.Add(recursionCurrentCell);
            if (printDebugLines)
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
                                if (printDebugLines)
                                    Debug.Log($"Right doesn't lead anywhere. Moving back Left.\n");

                                recursionCurrentCell.Valid = false;
                                recursionCurrentCell = recursionCurrentCell.Left;
                                recursionDirections.RemoveAt(recursionDirections.Count - 1);
                                recursionCellList.RemoveAt(recursionCellList.Count - 1);
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
                    recursionDirections.RemoveAt(recursionDirections.Count - 1);
                    recursionCellList.RemoveAt(recursionCellList.Count - 1);
                }

                if (printDebugLines)
                    Debug.Log($"Moving right lead to an invalid path. Going back left to {recursionCurrentCell}\n");
            }
        }

        return recursionAtGoal;
    }

    bool MoveSouth(Cell end)
    {
        //if we can move south, and we didnt move north before, go south
        Cell next = recursionCurrentCell.Down;
        if (printDebugLines)
            Debug.Log($"Attempting to move down to {next}\n");

        bool validPathMovingSouth = false;
        bool checkValidity = false;

        if (next != null && (recursionDirections.Count == 0 || recursionDirections.Last() != "UP") && next.Valid && UnVistedNonPurpleCell(next))
        {
            recursionCurrentCell = recursionCurrentCell.Down;
            recursionDirections.Add("DOWN");
            recursionCellList.Add(recursionCurrentCell);
            if (printDebugLines)
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
                                if (printDebugLines)
                                    Debug.Log($"Down doesn't lead anywhere. Moving back up.\n");

                                recursionCurrentCell.Valid = false;
                                recursionCurrentCell = recursionCurrentCell.Up;
                                recursionDirections.RemoveAt(recursionDirections.Count - 1);
                                recursionCellList.RemoveAt(recursionCellList.Count - 1);
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
                    recursionDirections.RemoveAt(recursionDirections.Count - 1);
                    recursionCellList.RemoveAt(recursionCellList.Count - 1);
                }

                if (printDebugLines)
                    Debug.Log($"Moving down lead to an invalid path. Going back up to {recursionCurrentCell}\n");
            }
        }

        return recursionAtGoal;
    }

    bool MoveWest(Cell end)
    {
        //if we can move west, and we didnt move east before, go west
        Cell next = recursionCurrentCell.Left;

        if (printDebugLines)
            Debug.Log($"Attempting to move left to {next}\n");

        bool validPathMovingWest = false;
        bool checkValidity = false;
        if (next != null && (recursionDirections.Count == 0 || recursionDirections.Last() != "RIGHT") && next.Valid && UnVistedNonPurpleCell(next))
        {
            recursionCurrentCell = recursionCurrentCell.Left;
            recursionDirections.Add("LEFT");
            recursionCellList.Add(recursionCurrentCell);
            if (printDebugLines)
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
                                if (printDebugLines)
                                    Debug.Log($"Left doesn't lead anywhere. Moving back right.\n");

                                recursionCurrentCell.Valid = false;
                                recursionCurrentCell = recursionCurrentCell.Right;
                                recursionDirections.RemoveAt(recursionDirections.Count - 1);
                                recursionCellList.RemoveAt(recursionCellList.Count - 1);
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
                    recursionDirections.RemoveAt(recursionDirections.Count - 1);
                    recursionCellList.RemoveAt(recursionCellList.Count - 1);
                }

                if (printDebugLines)
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
                if (current.Col == 7 && newList.Last().Tile != Tile.Purple)
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
            if (c.Button.Selectable == button)
            {
                return c;
            }
        }

        return null;
    }
    void ResetModule()
    {
        Logging("Resetting module...");
        foreach (Cell c in grid)
        {
            c.HasPlayer = false;

            if (colorBlindOn)
            {
                c.SetColorBlindTextMeshVisisbilty(true);
            }
        }

        heart.SetActive(false);
        SetSmell(Smell.None);
    }

    IEnumerator ButtonPress(KMSelectable button)
    {
        pressable = false;
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

                if (colorBlindOn)
                {
                    selectedCell.SetColorBlindTextMeshVisisbilty(false);
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

                if (colorBlindOn)
                {
                    playerCell.SetColorBlindTextMeshVisisbilty(true);
                }

                Logging("Pressed " + selectedCell.ToString());

                switch (selectedCell.Tile)
                {
                    case Tile.Orange:
                        SetSmell(Smell.Orange);
                        break;
                    case Tile.Blue:
                        if (currentSmell == Smell.Orange)
                        {
                            if (colorBlindOn)
                            {
                                selectedCell.SetColorBlindTextMeshVisisbilty(false);
                            }

                            yield return SetPlayer(selectedCell, false, walkingTime);
                            //todo have a chomping noise play
                            Audio.PlaySoundAtTransform(audioClips[7].name, transform);
                            Strike("Strike! Got bit by pirahnas. Moving back to " + playerCell.ToString());
                            yield return SetPlayer(playerCell, false, runningTime);

                            if (colorBlindOn)
                            {
                                playerCell.SetColorBlindTextMeshVisisbilty(false);
                                selectedCell.SetColorBlindTextMeshVisisbilty(true);
                            }
                            
                            pressable = true;
                            yield break;
                        }
                        break;
                    case Tile.Purple:

                        string direction = GetDirection(playerCell, selectedCell);
                        Cell currentCell = playerCell;

                        do
                        {
                            Cell nextCell = GetNewCellViaDirection(currentCell, direction);

                            if (colorBlindOn)
                            {
                                currentCell.SetColorBlindTextMeshVisisbilty(true);
                                nextCell.SetColorBlindTextMeshVisisbilty(false);
                            }

                            yield return SetPlayer(nextCell, false, walkingTime);
                            SetSmell(Smell.Lemon);
                            currentCell = nextCell;

                        } while (currentCell.Tile == Tile.Purple);

                        Logging("Moved to " + currentCell.ToString());

                        if (currentCell.Tile == Tile.Red)
                        {
                            Strike("Strike! Slid to a red tile.");
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

                        if (currentCell.Col == 7)
                        {
                            Solve();
                        }
                        pressable = true;
                        yield break;

                    case Tile.Green:

                        if (colorBlindOn)
                        { 
                            selectedCell.SetColorBlindTextMeshVisisbilty(false);
                        }

                        yield return SetPlayer(selectedCell, false, walkingTime);
                        yield return HandleGreenTiles();
                        if (FindPlayer().Col == 7)
                        {
                            Solve();
                        }
                        pressable = true;
                        yield break;
                }

                if (colorBlindOn)
                { 
                    selectedCell.SetColorBlindTextMeshVisisbilty(false);
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
        fightingGameObjects.SetActive(false);
        gridGameObject.SetActive(true);
        fightingMonster = false;
    }

    IEnumerator MoveBar()
    {
        spacePress = false;
        tpSpacePress = false;
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
                if (focused && (Input.GetKeyDown(KeyCode.Space) || tpSpacePress))
                {
                    spacePress = true;
                    break;
                }

                float t = elaspedTime / moveWhiteMaxTime;
                bar.transform.localPosition = Vector3.Lerp(leftPos, rightPos, t);
                elaspedTime += Time.deltaTime;

                float barX = bar.transform.localPosition.x;

                if (barX >= greenHit[0] && barX <= greenHit[1])
                {
                    barSpriteRenderer.color = Color.green;
                }

                else if (barX >= yellowHit[0] && barX <= yellowHit[1])
                {
                    barSpriteRenderer.color = Color.yellow;
                }

                else
                {
                    barSpriteRenderer.color = Color.white;
                }
                yield return null;
            }

            if (spacePress)
            {
                break;
            }


            elaspedTime = 0f;
            while (elaspedTime < moveWhiteMaxTime)
            {
                if (focused && (Input.GetKeyDown(KeyCode.Space) || tpSpacePress))
                {
                    spacePress = true;
                    break;
                }

                float t = elaspedTime / moveWhiteMaxTime;
                bar.transform.localPosition = Vector3.Lerp(rightPos, leftPos, t);
                elaspedTime += Time.deltaTime;

                float barX = bar.transform.localPosition.x;

                if (barX >= greenHit[0] && barX <= greenHit[1])
                {
                    barSpriteRenderer.color = Color.green;
                }

                else if (barX >= yellowHit[0] && barX <= yellowHit[1])
                {
                    barSpriteRenderer.color = Color.yellow;
                }

                else
                {
                    barSpriteRenderer.color = Color.white;
                }

                yield return null;
            }

        } while (!spacePress);

        yield return SwingKnife();
        yield return DepleteHealth(monsterHealth / maxHealth);
    }

    IEnumerator SwingKnife()
    {
        animator.SetTrigger("Trigger Knife Swing");
        Audio.PlaySoundAtTransform(audioClips[0].name, transform);
        yield return new WaitForSeconds(audioClips[0].length);

        float barX = bar.transform.localPosition.x;
        if (barX >= greenHit[0] && barX <= greenHit[1])
        {
            monsterHealth -= maxHealth / 2;
        }

        else if (barX >= yellowHit[0] && barX <= yellowHit[1])
        {
            monsterHealth -= maxHealth / 3;
        }

        else
        {
            monsterHealth -= maxHealth / 4;
        }
    }

    IEnumerator DepleteHealth(float newPercentage)
    {
        Audio.PlaySoundAtTransform(audioClips[4].name, transform);
        float elaspedTime;

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
        grid = new Cell[6, 8];
        for (int row = 0; row < 6; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                int index = row * 8 + col;
                grid[row, col] = new Cell(row, col, buttons[index]);
                buttons[index].Selectable.OnInteract += delegate () { buttons[index].Selectable.AddInteractionPunch(.1f); if (pressable && !fightingMonster && !ModuleSolved) StartCoroutine(ButtonPress(buttons[index].Selectable)); return false; };
            }
        }
        if (!debug)
        {
            SetNeighbors();

            bool validMaze = false;

            int count = 0;
            do
            {
                count++;
                validMaze = GenerateMaze();

                if (colorBlindOn && !validMaze)
                {
                    foreach (Cell c in grid)
                    {
                        Destroy(c.ColorBlidTextMesh.gameObject);
                    }
                }
            } while (!validMaze && count < 100);

            if (count == 100 && !validMaze)
            {
                Logging("Couldn't generate a good maze. Generating default maze...");

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
        gridGameObject.SetActive(true);
        fightingGameObjects.SetActive(false);
        pressable = true;
        fightingMonster = false;
        LogGrid();
        Logging($"Final Answer: " + LogList(recursionCellList));
        Logging($"Simplified Answer: " + LogList(recursionCellListSimplified));

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

    private void LogGrid()
    {
        string s = "";
        for (int row = 0; row < 6; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                s += grid[row, col].GetColor() == "Pink" ? "I " : grid[row, col].GetColor()[0] + " ";
            }


            Logging(s);
            s = "";

        }

        Logging(s);
    }

    private string GetDirection(Cell c1, Cell c2)
    {
        return c1.Up == c2 ? "up" : c1.Down == c2 ? "down" : c1.Right == c2 ? "right" : "left";
    }

    private Cell GetNewCellViaDirection(Cell c, string direction)
    {
        return direction == "up" ? c.Up :
               direction == "down" ? c.Down :
               direction == "right" ? c.Right : c.Left;
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Use `!{0} row col` to press the cell with `1 1` being top left. Put a comma between commands to chain them. Use `!{0} reset` to reset the module. If you land on a green tile, the fighting will be done for you.";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string Command)
    {
        yield return null;

        Command = Command.ToUpper();

        if (Command == "RESET")
        {
            resetButton.OnInteract();
            yield break;
        }

        string[] chainCommands = Command.ToUpper().Trim().Split(',').Select(x => x.Trim()).ToArray();

        foreach (string command in chainCommands) 
        {
            string response = ValidCommand(command);

            if (response != null)
            {
                yield return $"sendtochaterror {response}. Invalid command: `{command}`";
                yield break;
            }
        }

        foreach (string command in chainCommands)
        {
            string[] commands = command.Trim().Split(' ');

            int row = int.Parse(commands[0]);
            int col = int.Parse(commands[1]);

            row--;
            col--;



            KMSelectable button = buttons[row * 8 + col].gameObject.GetComponent<KMSelectable>();
            Cell cell = GetCell(button);
            Cell playerCell = FindPlayer();
            Cell upcomingGreenCell = null;

            if (playerCell == null && col != 0)
            {
                yield return $"sendtochaterror Your first command does not start in the first column. Given: `{command.Join("")}`";
                yield break;
            }

            if (cell.Tile == Tile.Red)
            {
                yield return $"sendtochat Stopping since trying to move on a red tile: {command}";
                yield break;
            }

            else if (cell.Tile == Tile.Purple)
            {
                //if the tile is purple, check to see where the player is (the player will always be on the grid)
                playerCell = FindPlayer();

                //depending on the direction the player is relative to the purple tile, check that direction one more unit and see if it's either purple or green
                string direction = GetDirection(playerCell, cell);

                Cell nextCell = GetNewCellViaDirection(cell, direction);

                //if it's purple, move in that direction again
                while (nextCell.Tile == Tile.Purple)
                {
                    nextCell = GetNewCellViaDirection(nextCell, direction);
                }

                //if it is green, then hold this cell in upcomingGreenCell
                if (nextCell.Tile == Tile.Green)
                {
                    upcomingGreenCell = nextCell;
                }
            }

            button.OnInteract();
            playerCell = FindPlayer();

            //if upcoming greenCell is not null, wait until the playerCell becomes this cell and call HandleFighting
            if (upcomingGreenCell != null)
            {
                while (playerCell != upcomingGreenCell)
                {
                    playerCell = FindPlayer();
                    yield return null;
                }
            }

            if (playerCell != null && playerCell.Tile == Tile.Green)
            {
                yield return HandleFighting();
            }

            while (!pressable)
            {
                yield return null;
            }
        }
    }

    string ValidCommand(string command)
    {
        string[] commands = command.Trim().Split(' ');

        if (commands.Length != 2)
        {
            return "not enough or too many commands to select cell";
        }

        int row, col;

        if (!int.TryParse(commands[0], out row) || !(row >= 1 && row <= 6))
        {
            return $"`{commands[0]}` is not a valid row";

        }

        if (!int.TryParse(commands[1], out col) || !(col >= 1 && col <= 8))
        {
            return $"`{commands[1]}` is not a valid column";
        }

        return null;
    }
    IEnumerator TwitchHandleForcedSolve()
    {
        focused = true;
        yield return ProcessTwitchCommand("Reset");

        foreach (Cell c in recursionCellListSimplified)
        {
            string s = c.ToString().Replace("(","").Replace(")", "");
            yield return ProcessTwitchCommand(s);
        }

        while (!ModuleSolved)
        {
            yield return null;
        }
    }

    IEnumerator HandleFighting()
    {
        //idk why but chaining commands causes focused to be false for some reason
        focused = true;
        //wait for fight to be active
        while (gridGameObject.activeSelf)
        {
            yield return null;
        }
        do 
        {
            float barX = bar.transform.localPosition.x;
            if (barX >= greenHit[0] && barX <= greenHit[1])
            {
                tpSpacePress = true;
            }
            yield return null;
        } while (!gridGameObject.activeSelf);
        focused = false;
    }
}
