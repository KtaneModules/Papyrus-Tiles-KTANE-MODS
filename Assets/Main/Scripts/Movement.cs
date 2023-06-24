using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement  {
    public Cell start;
    public Cell end;
    public Smell smell { get; private set; }

    public Movement(Cell start, Cell end)
    {
        this.start = start;
        this.end = end;

        smell = end.GetColor() == "Orange" ? Smell.Orange : end.GetColor() == "Purple" ? Smell.Lemon : Smell.None; 


        if (smell == Smell.None)
        {
            smell = start.GetColor() == "Orange" ? Smell.Orange : start.GetColor() == "Purple" ? Smell.Lemon : Smell.None;
        }
    }



    public string Direction()
    {
        return start.Up == end ? "UP" : start.Down == end ? "DOWN" : start.Left == end ? "LEFT" : "RIGHT"; 
    }
}
