using UnityEngine;
public class CeluralAutomataExtended : MonoBehaviour {
  public bool RandomStart = true;
  public bool Run = true;
  [Min(3)]
  public int Size = 100;
  [Min(1)]
  public int MaxAmount = 50;
  [Min(1)]
  public int Range = 2;
  [Min(1)]
  public int Speed = 4;
  int maxAmount;
  float maxAmountRec;
  ColorMap cM;
  void Awake() => cM = GetComponentInChildren<ColorMap>();
  int[,] rGB;
  int[] sum;
  void Start() {
    cM.SetSize(Size);
    (maxAmount, maxAmountRec, rGB, sum) = (MaxAmount, 1f / MaxAmount, new int[cM.SizeTotal, 3], new int[cM.SizeTotal]);
    if (RandomStart)
      for (int i = 0; i < cM.SizeTotal; i++)
        rGB[i, Random.Range(0, 3)] = Random.Range(0, maxAmount + 1);
    else {
      for (int c = 0; c < 3; c++) {
        (int sizeHalf, float rad) = (cM.Size.x / 2, Mathf.PI * (c * 2.0f / 3));
        int dist = Mathf.Min(maxAmount / 2, cM.Size.x / 3);
        int index =  sizeHalf + Mathf.RoundToInt(Mathf.Cos(rad) * dist) + (sizeHalf + Mathf.RoundToInt(Mathf.Sin(rad) * dist)) * cM.Size.x;
        rGB[index, c] = maxAmount;
        cM.SetPixel(index, Color.HSVToRGB(c * .333f, 1, 1));
      }
    }
  }
  int[,] interact = {
    { 0, 2, 1 },
    { 1, 0, 2 },
    { 2, 1, 0 },
  };
  void Update() {
    if (Run) {
      if (MaxAmount != maxAmount)
        (maxAmount, maxAmountRec) = (MaxAmount, 1f / MaxAmount);
      void setPixel(int i) => cM.SetPixel(i, new Color((float)rGB[i, 0] * maxAmountRec, (float)rGB[i, 1] * maxAmountRec, (float)rGB[i, 2] * maxAmountRec));
      if (Size != cM.Size.x) {
        int lastSize = cM.Size.x;
        cM.SetSize(Size);
        var neoRGB = new int[cM.SizeTotal, 3];
        for (int i = 0; i < Mathf.Pow(Mathf.Min(lastSize, Size), 2); i++) {
          for (int c = 0; c < 3; c++)
            neoRGB[i, c] = rGB[i, c];
          setPixel(i);
        }
        (rGB, sum) = (neoRGB, new int[cM.SizeTotal]); // sum calc & instant change sum
      } for (int s = 0; s < Speed; s++) {
        for (int i = 0; i < cM.SizeTotal; i++)
          sum[i] = rGB[i, 0] + rGB[i, 1] + rGB[i, 2];
        for (int i = 0; i < cM.SizeTotal; i++) {
          int wrap(int val) => (int)Mathf.Repeat(val, cM.Size.x);
          float rad = Random.Range(0, Mathf.PI * 2);
          float dist = Random.Range(0f, Range);
          int look = wrap(i / cM.Size.x + Mathf.RoundToInt(Mathf.Cos(rad) * dist)) * cM.Size.x + wrap(i % cM.Size.x + Mathf.RoundToInt(Mathf.Sin(rad) * dist));
          if (0 < sum[look]) {
            int sumCrnt = sum[i];
            int pickClr(int i) {
              (int rnd, int red) = (Random.Range(0, sumCrnt), rGB[i, 0]);
              if (rnd < red)
                return 0;
              else {
                rnd -= red;
                if (rnd < rGB[i, 1])
                  return 1;
                else
                  return 2;
              }
            }
            int clrLook = pickClr(look);
            if (0 < sumCrnt) {
              void clrAdd(int clr, int add) => rGB[i, clr] = Mathf.Clamp(rGB[i, clr] + add, 0, maxAmount);
              int clrSelf = pickClr(i);
              int res = interact[clrSelf, clrLook];
              if (res == 0)
                clrAdd(clrSelf, 1);
              else if (res == 1) {
                clrAdd(clrSelf, 1);
                clrAdd(clrLook, -2);
              } else if (res == 2) {
                clrAdd(clrSelf, -2);
                clrAdd(clrLook, 1);
              }
            } else
              rGB[i, clrLook] = rGB[look, clrLook] - 1;
            setPixel(i);
          }
        }
      }
      cM.Apply();
    }
  }
}
