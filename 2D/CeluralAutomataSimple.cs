using UnityEngine;

public class CeluralAutomataSimple : MonoBehaviour {
  public bool RandomStart = false;
  public bool Run = true;
  [Min(3)]
  public int Size = 200;
  [Min(1)]
  public int MaxAmount = 10;
  [Min(1)]
  public int Speed = 4;
  int maxAmount;
  float maxHalfRec;
  ColorMap cM;
  void Awake() => cM = GetComponentInChildren<ColorMap>();
  enum Clr {
    Red,
    Green,
    Blue,
    Black,
  }
  Clr[] clr;
  int[] amount;
  void Start() {
    cM.SetSize(Size);
    (maxAmount, clr, amount) = (MaxAmount, new Clr[cM.SizeTotal], new int[cM.SizeTotal]);
    if (RandomStart) for (int i = 0; i < cM.SizeTotal; i++)
      (clr[i], amount[i]) = ((Clr)Random.Range(0, 3), maxAmount);
    else {
      for (int i = 0; i < cM.SizeTotal; i++)
        clr[i] = Clr.Black;
      float maxHalf = maxAmount / 2;
      maxHalfRec = 1 / maxHalf;
      for (int i = 0; i < 3; i++) {
        (int sizeHalf, float rad) = (cM.Size.x / 2, i * 2.0f / 3 * Mathf.PI);
        float dist = Mathf.Min(maxHalf, cM.Size.x / 3f);
        int index =  sizeHalf + Mathf.RoundToInt(Mathf.Cos(rad) * dist) + (sizeHalf + Mathf.RoundToInt(Mathf.Sin(rad) * dist)) * cM.Size.x;
        (clr[index], amount[index]) = ((Clr)i, maxAmount);
        cM.SetPixel(index, Color.HSVToRGB(i * .333f, 1, 1));
      }
    }
  }
  void Update() {
    if (Run) {
      if (MaxAmount != maxAmount)
        (maxAmount, maxHalfRec) = (MaxAmount, 2 / MaxAmount);
      void setPixel(int i, Clr clr, float amount) => cM.SetPixel(i, Color.HSVToRGB((int)clr * .333f, 1, amount * maxHalfRec + .5f));
      if (Size != cM.Size.x) {
        int lastSize = cM.Size.x;
        cM.SetSize(Size);
        (var neoClr, var neoAmt) = (new Clr[cM.SizeTotal], new int[cM.SizeTotal]);
        int smallSq = (int)Mathf.Pow(Mathf.Min(lastSize, Size), 2);
        for (int i = 0; i < smallSq; i++) {
          (Clr clrCrnt, int amountCrnt) = (neoClr[i], neoAmt[i]) = (clr[i], amount[i]);
          setPixel(i, clrCrnt, amountCrnt);
        } if (lastSize < Size) for (int i = smallSq; i < cM.SizeTotal; i++)
            neoClr[i] = Clr.Black;
        (clr, amount) = (neoClr, neoAmt);
      } for (int s = 0; s < Speed; s++) {
        for (int i = 0; i < cM.SizeTotal; i++) {
          int wrap(int val) => (int)Mathf.Repeat(val, cM.Size.x);
          int look = wrap(i / cM.Size.x + Random.Range(-1, 2)) * cM.Size.x + wrap(i % cM.Size.x + Random.Range(-1, 2));
          (Clr selfClr, Clr lookClr, int lookAmnt) = (clr[i], clr[look], amount[look]);
          if (((int)selfClr + 1) % 3 == (int)lookClr && selfClr != Clr.Black) {
            (Clr clrCrnt, int amountCrnt) = 0 < amount[i] ?
              (clr[i], --amount[i]) :
              (clr[i], amount[i]) = (lookClr, maxAmount);
            setPixel(i, clrCrnt, amountCrnt);
          } else if (selfClr == Clr.Black && lookClr != Clr.Black && 0 < lookAmnt)
            setPixel(i, clr[i] = lookClr, amount[i] = lookAmnt - 1);
        }
      }
      cM.Apply();
    }
  }
}
