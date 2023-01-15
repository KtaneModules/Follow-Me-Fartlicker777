using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class FollowMe : MonoBehaviour {

   public KMBombInfo Bomb;
   public KMAudio Audio;

   public GameObject Mod;

   public KMSelectable[] MoveSpots;
   public TextMesh[] Arrows;

   public Material Static;
   public Material Blackness;
   public Renderer Screen;

   public TextMesh BlunderText;

   string TPAPath = "";

   bool[] Highlighted = new bool[4];
   //bool[] HaveHighlighted = new bool[4];

   /*string[][] Maze = new string[][] {
     new string[] {"R" , "LDR" , "LR" , "LR" , "LR" , "LR" , "DL" , "DR" , "DL" , "D" , "D"},
     new string[] {"DR" , "UDL" , "D" , "DR" , "LR" , "LR" , "LU" , "U" , "UDR" , "LU" , "UD"},
     new string[] {"UD" , "UD" , "UR" , "LUR" , "LDR" , "LDR" , "LR" , "LR" , "LU" , "D" , "UD"},
     new string[] {"UD" , "UR" , "LR" , "DL" , "U" , "UR" , "LR" , "LD" , "DR" , "ULR" , "UDL"},
     new string[] {"UR" , "LDR" , "DL" , "UR" , "LD" , "R" , "LD" , "UD" , "UD" , "R" , "LU"},
     new string[] {"D" , "UD" , "UD" , "RD" , "LU" , "DR" , "UL" , "U" , "UR" , "LD" , "D"},
     new string[] {"UDR" , "LU" , "UD" , "UD" , "DR" , "UL" , "DR" , "LDR" , "LD" , "UDR" , "LU"},
     new string[] {"UD" , "D" , "UD" , "UD" , "UR" , "LR" , "ULD" , "UD" , "UD" , "UD" , "D"},
     new string[] {"UD" , "UD" , "UD" , "UD" , "DR" , "LD" , "UD" , "URD" , "LU" , "UD" , "UD"},
     new string[] {"UD" , "UDR" , "UDL" , "UR" , "UL" , "UR" , "UDL" , "UDR" , "L" , "UDR" , "LU"},
     new string[] {"UR" , "LU" , "UR" , "LR" , "L" , "R" , "LU" , "UR" , "LR" , "LRU" , "L"}
   };*/
   //string Arrows = "????";

   string[][] Maze = new string[][] { //Show where you can do
     new string[] {"R" , "LDR" , "LR" , "LR" , "LR" , "LR" , "DL" , "D"},
     new string[] {"DR" , "UDL" , "D" , "D" , "R" , "LR" , "ULDR" , "UL"},
     new string[] {"UD" , "UD" , "UR" , "LUR" , "LDR" , "LDR" , "ULR" , "L"},
     new string[] {"UD" , "UR" , "LR" , "DL" , "UD" , "UR" , "LR" , "LD"},
     new string[] {"UR" , "LDR" , "DL" , "UR" , "ULDR" , "LR" , "LD" , "UD"},
     new string[] {"D" , "UD" , "UD" , "RD" , "LU" , "DR" , "UL" , "U"},
     new string[] {"UDR" , "LU" , "UDR" , "UDL" , "DR" , "UL" , "DR" , "LD"},
     new string[] {"U" , "R" , "UL" , "U" , "UR" , "LR" , "UL" , "U"}
   };

   int[] InitPos = { 0, 0 }; //Row, column
   int[] Pos = { 0, 0 };

   int[] Goal = { 0, 0 };

   int LastDig;

   bool anim = false;

   List<string> Path = new List<string>() { };

   Coroutine PathDisplay;
   Coroutine StaticScreen;

   static int ModuleIdCounter = 1;
   int ModuleId;
   private bool ModuleSolved;

   void Awake () {
      ModuleId = ModuleIdCounter++;

      foreach (KMSelectable But in MoveSpots) {
         //But.OnHighlight += delegate () { ButHL(But); };
         //But.OnHighlightEnded += delegate () { ButHLE(But); };
         But.OnInteract += delegate () { BPress(But); return false; };
      }

   }

   /*void ButHL (KMSelectable B) {
      Highlighted[Array.IndexOf(MoveSpots, B)] = true;
      HaveHighlighted[Array.IndexOf(MoveSpots, B)] = true;
   }

   void ButHLE (KMSelectable B) {
      Highlighted[Array.IndexOf(MoveSpots, B)] = false;
   }*/

   void BPress (KMSelectable B) {
      Audio.PlaySoundAtTransform("click", B.transform);
      if (ModuleSolved || anim) {
         return;
      }
      if (MoveSpots[0] == B) {
         if (Maze[Pos[0]][Pos[1]].Contains("U")) {
            Pos[0]--;
         }
         else {
            Strike();
         }
      }
      else if (MoveSpots[1] == B) {
         if (Maze[Pos[0]][Pos[1]].Contains("L")) {
            Pos[1]--;
         }
         else {
            Strike();
         }
      }
      else if (MoveSpots[2] == B) {
         if (Maze[Pos[0]][Pos[1]].Contains("R")) {
            Pos[1]++;
         }
         else {
            Strike();
         }
      }
      else {
         if (Maze[Pos[0]][Pos[1]].Contains("D")) {
            Pos[0]++;
         }
         else {
            Strike();
         }
      }
      if (Pos[0] == Goal[0] && Pos[1] == Goal[1]) {
         Solve();
      }
   }

   /*bool Started () {
      return HaveHighlighted[0] & HaveHighlighted[1] & HaveHighlighted[2] & HaveHighlighted[3];
   }

   void Update () {
      if (Started() && !ModuleSolved) {
         int time = (int) Bomb.GetTime() % 10;
         if (time != LastDig) {
            if (Highlighted[0]) {
               if (Maze[Pos[0]][Pos[1]].Contains("U")) {
                  Pos[0]--;
               }
               else {
                  GetComponent<KMBombModule>().HandleStrike();
                  Reset();
                  return;
               }
            }
            if (Highlighted[1]) {
               if (Maze[Pos[0]][Pos[1]].Contains("L")) {
                  Pos[1]--;
               }
               else {
                  GetComponent<KMBombModule>().HandleStrike();
                  Reset();
                  return;
               }
            }
            if (Highlighted[2]) {
               if (Maze[Pos[0]][Pos[1]].Contains("R")) {
                  Pos[1]++;
               }
               else {
                  GetComponent<KMBombModule>().HandleStrike();
                  Reset();
                  return;
               }
            }
            if (Highlighted[3]) {
               if (Maze[Pos[0]][Pos[1]].Contains("D")) {
                  Pos[0]++;
               }
               else {
                  GetComponent<KMBombModule>().HandleStrike();
                  Reset();
                  return;
               }
            }
            if (Pos[0] == Goal[0] && Pos[1] == Goal[1]) {
               GetComponent<KMBombModule>().HandlePass();
               ModuleSolved = true;
            }
            LastDig = time;
         }
      }
   }*/

   void Reset () {
      /*for (int i = 0; i < 4; i++) {
         HaveHighlighted[i] = false;
      }*/
      Goal[0] = 0;
      Goal[1] = 0;
      Generate();
      //Pos[0] = InitPos[0];
      //Pos[1] = InitPos[1];
   }

   void Strike () {
      ClearScreenOfArrows();
      GetComponent<KMBombModule>().HandleStrike();
      StartCoroutine(StrikeAnim());
      //Reset();
   }

   IEnumerator StrikeAnim () {
      anim = true;
      Audio.PlaySoundAtTransform("Strike Maybe", transform);
      StaticScreen = StartCoroutine(ChangeStatic());
      BlunderText.text += "?";
      yield return new WaitForSeconds(.8f);
      BlunderText.text += "?";
      yield return new WaitForSeconds(.9f);
      BlunderText.text += "?";
      yield return new WaitForSeconds(.9f);
      BlunderText.text = "";
      StopCoroutine(StaticScreen);
      Screen.GetComponent<MeshRenderer>().material = Blackness;
      anim = false;
      Reset();
   }

   IEnumerator ChangeStatic () {

      while (true) {
         Screen.GetComponent<MeshRenderer>().material = Static;
         //Debug.Log(Static.mainTextureOffset.x);
         Screen.material.SetTextureOffset("_MainTex", new Vector2(Rnd.Range(0f, 1f), Rnd.Range(0f, 1f)));
         //Static.mainTextureOffset.x
         yield return null;
      }
   }

   void Solve () {
      StartCoroutine(YouSolvedItGoodJob());
      
      ModuleSolved = true;
      GetComponent<KMBombModule>().HandlePass();
   }

   IEnumerator YouSolvedItGoodJob () {
      ClearScreenOfArrows();
      Audio.PlaySoundAtTransform("solve", transform);
      for (int i = 0; i < 5; i++) {
         Mod.transform.Rotate(new Vector3(0, -9f, 0));
         yield return new WaitForSecondsRealtime(.01f);
      }
      yield return new WaitForSecondsRealtime(.5f);
      for (int i = 0; i < 125; i++) {
         Mod.transform.Rotate(new Vector3(0, 9f, 0));
         yield return new WaitForSecondsRealtime(.01f);
      }
      yield return new WaitForSecondsRealtime(.15f);
      BlunderText.color = new Color32(255, 255, 255, 255);
      BlunderText.text = "✓";
      yield return null;
   }

   void ClearScreenOfArrows () {
      StopCoroutine(PathDisplay);
      for (int i = 0; i < 4; i++) {
         Arrows[i].gameObject.SetActive(false);
      }
   }

   void Start () {
      Generate();
      //Pos[0] = InitPos[0];
      //Pos[1] = InitPos[1];
      //StartCoroutine(YouSolvedItGoodJob());
   }

   void GenerateGoal () {
      int vertCount = 0;
      int horiCount = 0;
      int upCount = 0;
      int downCount = 0;

      foreach (string dir in Path) {
         if (dir == "U") {
            vertCount++;
            upCount++;
         }
         else if (dir == "D") {
            vertCount++;
            downCount++;
         }
         else {
            horiCount++;
         }
      }

      Goal[0] += vertCount >= horiCount ? 4 : 0;
      Goal[0] += downCount >= upCount ? 2 : 0;
      Goal[0] += InitPos[1] != 0 && InitPos[1] != 4 ? 1 : 0;

      string letterCol = "ABCDEFGH";
      Goal[1] += !Bomb.GetSerialNumberLetters().Any(x => letterCol[InitPos[1]].ToString() == x.ToString()) ? 4 : 0;
      //Goal[1] += !Bomb.GetSerialNumberLetters().Any(x => letterCol.Contains(x.ToString())) ? 4 : 0;
      Goal[1] += !ExMath.IsPrime(InitPos[0] + 1) ? 2 : 0;
      Goal[1] += Path.Count() > 5 ? 1 : 0;
   }

   IEnumerator ShowPath () {
      for (int j = 0; j < 4; j++) {
         Arrows[j].gameObject.SetActive(false);
      }
      while (true) {
         for (int i = 0; i < Path.Count(); i++) {

            switch (Path[i]) {
               case "R":
                  Arrows[2].gameObject.SetActive(true);
                  break;
               case "U":
                  Arrows[1].gameObject.SetActive(true);
                  break;
               case "L":
                  Arrows[0].gameObject.SetActive(true);
                  break;
               case "D":
                  Arrows[3].gameObject.SetActive(true);
                  break;
            }

            yield return new WaitForSeconds(1f);
            for (int j = 0; j < 4; j++) {
               Arrows[j].gameObject.SetActive(false);
            }
            yield return new WaitForSeconds(.25f);
         }
         yield return new WaitForSeconds(.75f);
      }
   }

   void Generate () {
      Path.Clear();
      Pos = new int[2] { Rnd.Range(0, 8), Rnd.Range(0, 8) };
      InitPos[0] = Pos[0];
      InitPos[1] = Pos[1];
      GeneratePath();

      
      /*Debug.Log("ABCDEFGHIJK"[InitPos[1]].ToString() + (InitPos[0] + 1));
      for (int i = 0; i < Path.Count(); i++) {
         Debug.Log(Path[i]);
      }*/
      GenerateGoal();
      string pathLog = "";
      for (int i = 0; i < Path.Count(); i++) {
         pathLog += Path[i];
      }
      Debug.LogFormat("[Follow Me #{0}] Starting at position {1}{2}, the path goes: {3}.", ModuleId, "ABCDEFGH"[InitPos[1]], InitPos[0] + 1, pathLog);
      Debug.LogFormat("[Follow Me #{0}] Goal position is at {1}{2}.", ModuleId, "ABCDEFGH"[Goal[1]], Goal[0] + 1);
      PathDisplay = StartCoroutine(ShowPath());
   }

   void GeneratePath () {
      int count = 0;
      do {
         Path.Add(AddDirections());
         UpdatePos();
         if (Path.Last() == "NULL") {
            Path.Clear();
            InitPos[0] = Pos[0];
            InitPos[1] = Pos[1];
         }
         count++;
      } while (!CheckOnlyPaths() && count < 100000);
      //Debug.Log(count);
   }

   string AddDirections () {
      List<string> Combinations = new List<string>() { };
      for (int i = 0; i < Maze[Pos[0]][Pos[1]].Length; i++) {
         if (Path.Count() == 0) {
            Combinations.Add(Maze[Pos[0]][Pos[1]][i].ToString());
         }
         else {
            if (Maze[Pos[0]][Pos[1]][i] == 'L' && Path.Last() != "R") {
               Combinations.Add(Maze[Pos[0]][Pos[1]][i].ToString());
            }
            if (Maze[Pos[0]][Pos[1]][i] == 'U' && Path.Last() != "D") {
               Combinations.Add(Maze[Pos[0]][Pos[1]][i].ToString());
            }
            if (Maze[Pos[0]][Pos[1]][i] == 'R' && Path.Last() != "L") {
               Combinations.Add(Maze[Pos[0]][Pos[1]][i].ToString());
            }
            if (Maze[Pos[0]][Pos[1]][i] == 'D' && Path.Last() != "U") {
               Combinations.Add(Maze[Pos[0]][Pos[1]][i].ToString());
            }
         }
      }
      if (Combinations.Count() == 0) {
         return "NULL";
      }
      return Combinations[Rnd.Range(0, Combinations.Count())];
   }

   void UpdatePos () {
      switch (Path.Last()) {
         case "R":
            Pos[1]++;
            break;
         case "U":
            Pos[0]--;
            break;
         case "L":
            Pos[1]--;
            break;
         case "D":
            Pos[0]++;
            break;
      }
   }

   bool CheckOnlyPaths () {
      int Count = 0;
      for (int i = 0; i < 8; i++) {
         for (int j = 0; j < 8; j++) {
            if (CheckPathHelper(i, j)) {
               Count++;
            }
         }
      }
      return Count == 1;
   }

   bool CheckPathHelper (int row, int col) {
      if (Path.Count() == 0) {
         return false;
      }
      for (int i = 0; i < Path.Count(); i++) {
         if (Maze[row][col].Contains(Path[i])) {
            switch (Path[i]) {
               case "R":
                  col++;
                  break;
               case "L":
                  col--;
                  break;
               case "D":
                  row++;
                  break;
               case "U":
                  row--;
                  break;
            }
         }
         else {
            return false;
         }
      }
      return true;
   }

#pragma warning disable 414
   private readonly string TwitchHelpMessage = @"Use !{0} ULDR to press the buttons in those directions.";
#pragma warning restore 414

   IEnumerator ProcessTwitchCommand(string Command)
   {
        Command = Command.Replace(" ", "");
        for (int i = 0; i < Command.Length; i++)
        {
            if (!Command[i].EqualsAny('U', 'u', 'L', 'l', 'R', 'r', 'D', 'd'))
            {
                yield return "sendtochaterror I don't understand!";
                yield break;
            }
        }
        yield return null;
        for (int i = 0; i < Command.Length; i++)
        {
            if (anim)
                yield break;
            switch (Command[i])
            {
                case 'U':
                case 'u':
                    MoveSpots[0].OnInteract();
                    break;
                case 'L':
                case 'l':
                    MoveSpots[1].OnInteract();
                    break;
                case 'R':
                case 'r':
                    MoveSpots[2].OnInteract();
                    break;
                case 'D':
                default:
                    MoveSpots[3].OnInteract();
                    break;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        while (anim) yield return true;
        if (Pos[0] == Goal[0] && Pos[1] == Goal[1])
        {
            string paths = Maze[Pos[0]][Pos[1]];
            int rando = Rnd.Range(0, paths.Length);
            if (paths[rando] == 'U')
            {
                MoveSpots[0].OnInteract();
                yield return new WaitForSeconds(0.1f);
                MoveSpots[3].OnInteract();
                yield return new WaitForSeconds(0.1f);
            }
            else if (paths[rando] == 'L')
            {
                MoveSpots[1].OnInteract();
                yield return new WaitForSeconds(0.1f);
                MoveSpots[2].OnInteract();
                yield return new WaitForSeconds(0.1f);
            }
            else if (paths[rando] == 'D')
            {
                MoveSpots[3].OnInteract();
                yield return new WaitForSeconds(0.1f);
                MoveSpots[0].OnInteract();
                yield return new WaitForSeconds(0.1f);
            }
            else if (paths[rando] == 'R')
            {
                MoveSpots[2].OnInteract();
                yield return new WaitForSeconds(0.1f);
                MoveSpots[1].OnInteract();
                yield return new WaitForSeconds(0.1f);
            }
        }
        else
        {
            var q = new Queue<int[]>();
            var allMoves = new List<Movement>();
            var startPoint = Pos.ToArray();
            var target = Goal.ToArray();
            q.Enqueue(startPoint);
            while (q.Count > 0)
            {
                var next = q.Dequeue();
                if (next[0] == target[0] && next[1] == target[1])
                    goto readyToSubmit;
                string paths = Maze[next[0]][next[1]];
                var cell = paths.Replace(" ", "");
                var allDirections = "ULRD";
                var offsets = new int[,] { { -1, 0 }, { 0, -1 }, { 0, 1 }, { 1, 0 } };
                for (int i = 0; i < 4; i++)
                {
                    var check = new int[] { next[0] + offsets[i, 0], next[1] + offsets[i, 1] };
                    if (cell.Contains(allDirections[i]) && !allMoves.Any(x => x.start[0] == check[0] && x.start[1] == check[1]))
                    {
                        q.Enqueue(check);
                        allMoves.Add(new Movement { start = next, end = check, direction = i });
                    }
                }
            }
            throw new InvalidOperationException("Follow Me's TP autosolver failed to find a valid path.");
            readyToSubmit:
            var target2 = new int[] { target[0], target[1] };
            var lastMove = allMoves.First(x => x.end[0] == target2[0] && x.end[1] == target2[1]);
            var relevantMoves = new List<Movement> { lastMove };
            while (lastMove.start != startPoint)
            {
                lastMove = allMoves.First(x => x.end[0] == lastMove.start[0] && x.end[1] == lastMove.start[1]);
                relevantMoves.Add(lastMove);
            }
            for (int i = 0; i < relevantMoves.Count; i++)
            {
                MoveSpots[relevantMoves[relevantMoves.Count - 1 - i].direction].OnInteract();
                yield return new WaitForSeconds(.1f);
            }
        }
    }

    class Movement
    {
        public int[] start;
        public int[] end;
        public int direction;
    }
}
