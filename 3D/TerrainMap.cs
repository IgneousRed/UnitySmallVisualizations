using UnityEngine;

public class TerrainMap : MonoBehaviour {
  [Min(5)]
  public int MapSize = 100;
  [Range(2, 10)]
  public float MapPerDotSqrt = 5;
  [Range(1, 10)]
  public int Octaves = 4;
  public float Height = .1f;
  public int Seed = 0;
  int mapSize;
  float mapPerDotSqrt;
  int octaves;
  float height;
  int seed;
  MeshFilter[] meshFilter;
  void Awake() => meshFilter = GetComponentsInChildren<MeshFilter>();
  float fadeLerp(float first, float seccond, float t) {
    return Mathf.Lerp(first, seccond, 6 * Mathf.Pow(t, 5) - 15 * Mathf.Pow(t, 4) + 10 * Mathf.Pow(t, 3));
  }
  float[,] wrapping((int x, int y) mapSize, (int x, int y) dotCount) {
    Vector2[,] dots = new Vector2[dotCount.x, dotCount.y];
    for (int x = 0; x < dotCount.x; x++)
      for (int y = 0; y < dotCount.y; y++)
        dots[x, y] = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
    (float x, float y) scale = ((float)dotCount.x / mapSize.x, (float)dotCount.y / mapSize.y);
    (float x, float y) offSet = (Random.Range(0f, 1f / mapSize.x), Random.Range(0f, 1f / mapSize.y));
    float[,] result = new float[mapSize.x, mapSize.y];
    for (int x = 0; x < mapSize.x; x++) {
      float posX = ((float)x + offSet.x) * scale.x;
      int xLow = (int)posX;
      int xHigh = xLow + 1;
      int xHighIndex = xHigh < dotCount.x ? xHigh : 0;
      for (int y = 0; y < mapSize.y; y++) {
        float posY = ((float)y + offSet.y) * scale.y;
        (int yLow, Vector2 pos) = ((int)posY, new Vector2(posX, posY));
        (int yHigh, float yLerp) = (yLow + 1, posY - yLow);
        int yHighIndex = yHigh < dotCount.y ? yHigh : 0;
        result[x, y] = fadeLerp(
          fadeLerp(Vector2.Dot(dots[xLow, yLow], pos - new Vector2(xLow, yLow)),
            Vector2.Dot(dots[xLow, yHighIndex], pos - new Vector2(xLow, yHigh)), yLerp),
          fadeLerp(Vector2.Dot(dots[xHighIndex, yLow], pos - new Vector2(xHigh, yLow)),
            Vector2.Dot(dots[xHighIndex, yHighIndex], pos - new Vector2(xHigh, yHigh)), yLerp),
          posX - (int)posX
        );
      }
    }
    return result;
  }
  float[,] WrappingMap((int x, int y) mapSize, (int x, int y) dotCount, int octaves, int seed) {
    Random.InitState(seed);
    (float[,] map, float scale) = (wrapping(mapSize, dotCount), 1);
    for (int o = 1; o < octaves; o++) {
      (dotCount, scale) = ((dotCount.x * 2, dotCount.y * 2), scale * .5f);
      float[,] another = wrapping(mapSize, dotCount);
      for (int x = 0; x < mapSize.x; x++)
        for (int y = 0; y < mapSize.y; y++)
          map[x, y] += another[x, y] * scale;
    }
    return map;
  }
  float[,] MakeUniform(float[,] map) {
    (int len, float[] flat, int[] insts) = (map.GetLength(0), new float[map.Length], new int[map.Length]);
    for (int i = 0; i < map.Length; i++)
      (flat[i], insts[i]) = (map[i / len, i % len], i);
    void sortAndInstruct(int start, int end) {
      (float[], int[]) interweave((int start, int end) lesser, (int start, int end) greater) {
        int size = lesser.end - lesser.start + greater.end - greater.start;
        (float[] sorted, int[] newInsts, int indexSorted) = (new float[size], new int[size], 0);
        void pick(int index) {
          (sorted[indexSorted], newInsts[indexSorted++]) = (flat[index], insts[index]);
        }
        void pickGreater() {
          pick(greater.start);
          greater.start += 1;
        }
        while (lesser.start != lesser.end) {
          if (flat[greater.start] < flat[lesser.start])
            pickGreater();
          else
            pick(lesser.start++);
        }
        while (indexSorted < size)
          pickGreater();
        return (sorted, newInsts);
      }
      if (1 < end - start) {
        int mid = (start + end) / 2;
        sortAndInstruct(start, mid);
        sortAndInstruct(mid, end);
        (float[] map, int[] insts) sorted = flat[end - 1] < flat[mid - 1] ?
          interweave((mid, end), (start, mid)) :
          interweave((start, mid), (mid, end));
        for (int i = 0; i < sorted.map.Length; i++) {
          int ind = start + i;
          (flat[ind], insts[ind]) = (sorted.map[i], sorted.insts[i]);
        }
      }
    }
    sortAndInstruct(0, map.Length);
    float[,] result = new float[len, len];
    for (int i = 0; i < map.Length; i++)
      result[insts[i] / len, insts[i] % len] = (float)i / (map.Length - 1);
    return result;
  }
  void Update() {
    if (mapSize != MapSize || mapPerDotSqrt != MapPerDotSqrt || octaves != Octaves || height != Height || seed != Seed) {
      (mapSize, mapPerDotSqrt, octaves, height, seed) = (MapSize, MapPerDotSqrt, Octaves, Height, Seed);
      (int dotCount, int mapSize1, float mapSizeRcp) = (Mathf.CeilToInt(mapSize / mapPerDotSqrt / mapPerDotSqrt), mapSize + 1, 1f / mapSize);
      var map = MakeUniform(WrappingMap((mapSize, mapSize), (dotCount, dotCount), octaves, seed));
      (var verts, var trigs) = (new Vector3[map.Length + mapSize * 2 + 1], new int[map.Length * 6]);
      for (int x = 0; x < mapSize; x++) {
        (int iX1, int iX, float posX) = (mapSize1 * x, mapSize * x, mapSizeRcp * x - .5f);
        for (int y = 0; y < mapSize; y++) {
          int i = iX1 + y;
          verts[i] = new Vector3(posX, map[x, y] * height, mapSizeRcp * y - .5f);
          (int trigI, int pY, int pX) = ((iX + y) * 6, i + 1, mapSize1 + i);
          (trigs[trigI], trigs[trigI + 1], trigs[trigI + 2]) = (i, pY, pX);
          (trigs[trigI + 3], trigs[trigI + 4], trigs[trigI + 5]) = (pY, pX + 1, pX);
        }
      }
      for (int x = 0; x < mapSize; x++)
        verts[mapSize + mapSize1 * x] = new Vector3(mapSizeRcp * x - .5f, map[x, 0] * height, .5f);
      int lastLine = mapSize1 * mapSize;
      for (int y = 0; y < mapSize; y++)
        verts[lastLine + y] = new Vector3(.5f, map[0, y] * height, mapSizeRcp * y - .5f);
      verts[lastLine + mapSize] = new Vector3(.5f, map[0, 0] * height, .5f);
      var mesh = new Mesh {
        vertices = verts,
        triangles = trigs
      };
      mesh.RecalculateNormals();
      for (int mF = 0; mF < 4; mF++)
        meshFilter[mF].mesh = mesh;
    }
  }
}
