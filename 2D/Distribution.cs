using UnityEngine;

public class Distribution : MonoBehaviour {
  [Min(10)]
  public int Size = 200;
  [Min(0)]
  public int Speed = 100;
  ColorMap cM;
  void Awake() => cM = GetComponentInChildren<ColorMap>();
  int[] distrib;
  float sample() {
    float val = (Random.Range(0f, 1f) + Random.Range(0f, 1f)) / 2;

    // return val;

    return 6 * Mathf.Pow(val, 5) - 15 * Mathf.Pow(val, 4) + 10 * Mathf.Pow(val, 3);

    // return val > .5f ?
    //   (1 - Mathf.Pow(1 - (val - .5f) * 2, 2)) / 2 + .5f :
    //   Mathf.Pow(val * 2, 2) / 2;
  }
  void Update() {
    if (Size != cM.Size.x) {
      distrib = new int[Size];
      cM.SetSize(Size);
    }
    for (int i = 0; i < Speed; i++) {
      int ind = (int)(sample() * Size);
      if (ind == Size)
        ind -= 1;
      distrib[ind] += 1;
    }
    (int max, int index) = (Mathf.Max(distrib), 0);
    for (int x = 0; x < Size; x++) {
      float h = (float)distrib[x] / max * Size;
      int y = (int)h;
      for (int i = index; i < index + y; i++)
        cM.SetPixel(i, Color.black);
      if (y < Size) {
        float value = 1 - (h - y);
        cM.SetPixel(index + y, new Color(value, value, value));
        for (int i = index + y + 1; i < index + Size; i++)
          cM.SetPixel(i, Color.white);
      }
      index += Size;
    }
    cM.Apply();
  }
}
