using UnityEngine;
public class Terittories : MonoBehaviour {
  [Range(2, 10)]
  public int TerittoryCount = 5;
  [Min(20)]
  public int Size = 20;
  public int Seed = 0;
  [Min(0)]
  public int FillSpeed = 1;
  ColorMap cM;
  void Awake() => cM = GetComponentInChildren<ColorMap>();
  int terittoryCount = 0;
  int seed = 0;
  (int x, int y)[] position;
  int[] queueCounter;
  int[][] dirQueue;
  bool[,] taken;
  Color[] clr;
  int[] space;
  bool[] done;
  (int x, int y)[] dirAdd;
  void Update() {
    if (TerittoryCount != terittoryCount || Size != cM.Size.x || Seed != seed) {
      (terittoryCount, seed) = (TerittoryCount, Seed);
      Random.InitState(seed);
      cM.SetSize(Size);
      (position, queueCounter, dirQueue) = (new (int, int)[terittoryCount], new int[terittoryCount], new int[terittoryCount][]);
      (taken, clr, space, done) = (new bool[cM.Size.x, cM.Size.y], new Color[terittoryCount], new int[terittoryCount], new bool[terittoryCount]);
      dirAdd = new (int, int)[] {
        (cM.SizeTotal - 1, 0),
        (0, cM.SizeTotal - 1),
        (1, 0),
        (0, 1),
      };
      for (int t = 0; t < terittoryCount; t++)
        (clr[t], dirQueue[t], queueCounter[t], position[t]) =
          (Color.HSVToRGB((float)t / terittoryCount, 1, 1), new int[cM.SizeTotal], -1, (Random.Range(0, Size), Random.Range(0, Size)));
      Debug.Log("Start");
    }
    (int, int) posAddDir(int t, int dir) {
      (int x, int y) = dirAdd[dir];
      return ((position[t].x + x) % cM.Size.x, (position[t].y + y) % cM.Size.y);
    } for (int s = 0; s < FillSpeed; s++) {
      for (int t = 0; t < terittoryCount; t++) {
        ((int x, int y)[] possibility, int[] directions, int possibilityCount) = (new (int x, int y)[4], new int[4], 0);
        for (int d = 0; d < 4; d++) {
          (int x, int y) = posAddDir(t, d);
          if (!taken[x, y])
            (possibility[possibilityCount], directions[possibilityCount++]) = ((x, y), d);
        } if (0 < possibilityCount) {
          int result = Random.Range(0, possibilityCount);
          ((int x, int y) newPos, int newDir) = (possibility[result], directions[result]);
          dirQueue[t][++queueCounter[t]] = (newDir + 2) % 4;
          cM.SetPixel(newPos.x, newPos.y, clr[t]);
          (position[t], taken[newPos.x, newPos.y]) = (newPos, true);
          space[t]++;
        } else if (0 < queueCounter[t])
          position[t] = posAddDir(t, dirQueue[t][queueCounter[t]--]);
        else if (!done[t]) {
          Color32 clr = Color.HSVToRGB((float)t / terittoryCount, 1, 1);
          Debug.Log("color " + t + " (" + clr.r + " " + clr.b + " " + clr.g + ") is of size " + space[t]);
          done[t] = true;
        }
      }
    }
    cM.Apply();
  }
}
