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

    [SerializeField]
    Material[] materials;

    [SerializeField]
    GameObject prefab;

    [SerializeField]
    GameObject orange;

    [SerializeField]
    GameObject lemon;

    enum Smell
    {
        Lemon,
        Orange,
        None
    }

    Smell currentSmell;

    static int ModuleIdCounter = 1;
    int ModuleId;
    private bool ModuleSolved;

    void Awake()
    {
        ModuleSolved = false;
        GameObject[] obj = new GameObject[48];
        ModuleId = ModuleIdCounter++;
        float xOffset = .02f;
        float yOffset = .02f;

        Vector3 position = prefab.transform.position;
        int row = 6;
        int col = 8;
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < col; j++)
            {
                GameObject g =  Instantiate(prefab, this.transform);

                Vector3 nwePos = g.transform.position;
                nwePos.x += j * xOffset;
                nwePos.z += i * yOffset;

                g.transform.position = nwePos;

                obj[i * col + j] = g;
            }
        }

        for (int i = 0; i < 48; i++)
        {
            Debug.Log(i);
            obj[i].GetComponent<MeshRenderer>().material = materials[Rnd.Range(0,6)];
        }

        /*
        foreach (KMSelectable object in keypad) {
            object.OnInteract += delegate () { keypadPress(object); return false; };
        }
        */

        //button.OnInteract += delegate () { buttonPress(); return false; };

    }

    void GenerateMaze()
    {

    }
    void ResetModule()
    {
        currentSmell = Smell.None;
        orange.SetActive(false);
        lemon.SetActive(false);
    }

    void Start()
    {

    }

    void Update()
    {

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
