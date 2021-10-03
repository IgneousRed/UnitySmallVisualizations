using UnityEngine;
public class Maze : MonoBehaviour {
  public bool ShowClones = false;
  public bool ShowPath = true;
  [Min(2)]
  public int SqNum = 20;
  public int Seed = 0;
  [Min(0)]
  public int FillSpeed = 1;
  ColorMap cM;
  Material mat;
  void Awake() => (cM, mat) = (GetComponentInChildren<ColorMap>(), GetComponentInChildren<MeshRenderer>().materials[0]);
  bool showClones = false;
  bool showPath = false;
  int sqNum = 0;
  int seed = 0;
  int queueCounter;
  int[] dirQueue;
  bool[,] taken;
  (int x, int y)[] dirAdd;
  (int x, int y) position;
  (int x, int y) finishPos;
  int maxLen;
  bool headIsMax;
  int clrNum;
  int clrCrnt;
  void Update() {
    void take((int x, int y) newPos) => (position, taken[newPos.x, newPos.y]) = (newPos, true);
    (int, int) posAddDir(int dir) {
      (int x, int y) dirCrnt = dirAdd[dir];
      return ((position.x + dirCrnt.x) % sqNum, (position.y + dirCrnt.y) % sqNum);
    }
    void colorSpot((int x, int y) pos, Color clr) {
      int botLeft = (pos.x * 3 + 1) * cM.Size.y + pos.y * 3 + 1;
      cM.SetPixel(botLeft, clr);
      cM.SetPixel(botLeft + 1, clr);
      cM.SetPixel(botLeft + cM.Size.y, clr);
      cM.SetPixel(botLeft + cM.Size.y + 1, clr);
    }
    void clrWall(int dir, Color clr) {
      void openHorizontal((int x, int y) pos) {
        int val = pos.x * 3 * cM.Size.y + pos.y * 3 + 1;
        cM.SetPixel(val, clr);
        cM.SetPixel(val + 1, clr);
      }
      void openVertical((int x, int y) pos) {
        int val = (pos.x * 3 + 1) * cM.Size.y + pos.y * 3;
        cM.SetPixel(val, clr);
        cM.SetPixel(val + cM.Size.y, clr);
      }
      if (dir == 0) openHorizontal(position);
      else if (dir == 1) openVertical(position);
      else if (dir == 2) openHorizontal(((position.x + 1) % sqNum, position.y));
      else openVertical((position.x, (position.y + 1) % sqNum));
    }
    Color next() => Color.HSVToRGB(1 - .666f * ++clrCrnt / clrNum, 1, 1);
    // Color next() => Color.HSVToRGB(Mathf.Repeat(1 - .666f * queueCounter / 500, 1), 1, 1);
    void queueDir(int dir) => dirQueue[++queueCounter] = (dir + 2) % 4;                             //
    if (ShowClones != showClones || ShowPath != showPath || SqNum != sqNum || Seed != seed) {
      (showClones, showPath, sqNum, seed) = (ShowClones, ShowPath, SqNum, Seed);
      cM.SetSize(sqNum * 3, TextureWrapMode.Repeat);
      float scale = (cM.Size.x * (showClones ? 2 : 1) + 1f) / cM.Size.x;
      mat.mainTextureScale = new Vector2(scale, scale);
      int sqNumTotal = sqNum * sqNum;
      (dirQueue, taken, queueCounter) = (new int[sqNumTotal], new bool[sqNum, sqNum], -1);
      dirAdd = new (int, int)[] {
        (cM.Size.x - 1, 0),
        (0, cM.Size.y - 1),
        (1, 0),
        (0, 1),
      };
      int i = cM.Size.y + 1;
      for (int x = 0; x < sqNum; x++) {
        for (int y2 = 0; y2 < sqNum * 2; y2++) {
          cM.SetPixel(i, Color.white);
          cM.SetPixel(i + 1, Color.white);
          i += 3;
        }
        i += cM.Size.y;
      }
      Random.InitState(seed);
      take((Random.Range(0, sqNum), Random.Range(0, sqNum)));
      int dir = Random.Range(0, 4);
      finishPos = posAddDir(dir);
      colorSpot(position, Color.red);
      if (ShowPath) {
        (maxLen, headIsMax) = (0, true);
        clrWall(dir, Color.magenta);
        colorSpot(finishPos, Color.cyan);
      } else {
        (clrNum, clrCrnt) = (sqNumTotal * 2 - 1, 0);
        clrWall(dir, next());
        colorSpot(finishPos, next());
      }
      take(finishPos);
      queueDir(dir);
      }
    for (int s = 0; s < FillSpeed; s++) {
      ((int x, int y)[] possibility, int[] directions, int possibilityCount) = (new (int x, int y)[4], new int[4], 0);
      for (int d = 0; d < 4; d++) {
        (int x, int y) = posAddDir(d);
        if (!taken[x, y])
          (possibility[possibilityCount], directions[possibilityCount++]) = ((x, y), d);
      }
      if (0 < possibilityCount) {
        int result = Random.Range(0, possibilityCount);
        ((int, int) newPos, int newDir) = (possibility[result], directions[result]);
        queueDir(newDir);
        if (ShowPath) {
          clrWall(newDir, Color.magenta);
          colorSpot(position, Color.magenta);
          if (maxLen < queueCounter) {
            if (!headIsMax) {
              colorSpot(finishPos, Color.white);
              headIsMax = true;
            }
            (finishPos, maxLen) = (newPos, queueCounter);
            colorSpot(newPos, Color.cyan);
          } else
            colorSpot(newPos, Color.blue);
        } else {
          clrWall(newDir, next());
          colorSpot(newPos, next());
        }
        take(newPos);
      } else {
        if (ShowPath) {
          int lastDir = dirQueue[queueCounter];
          clrWall(lastDir, Color.white);
          if (0 < queueCounter) {
            queueCounter--;
            if (headIsMax) {
              colorSpot(position, Color.green);
              headIsMax = false;
            } else
              colorSpot(position, Color.white);
            position = posAddDir(lastDir);
            colorSpot(position, Color.blue);
          } else
            colorSpot(position, Color.white);
        } else if (0 < queueCounter)
          position = posAddDir(dirQueue[queueCounter--]);
      }
    }
    cM.Apply();
  }
}
