using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement  {
    public Cell start;
    public Cell end;

    public Movement(Cell start, Cell end)
    {
        this.start = start;
        this.end = end;
    }

    public string Direction()
    {
        return start.Up == end ? "UP" : start.Down == end ? "DOWN" : start.Left == end ? "LEFT" : "RIGHT"; 
    }
}
