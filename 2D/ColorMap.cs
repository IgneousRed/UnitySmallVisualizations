using UnityEngine;
public class ColorMap : MonoBehaviour {
  public (int x, int y) Size;
  public int SizeTotal;
  MeshRenderer rend;
  void Awake() => rend = GetComponent<MeshRenderer>();
  Color[] Map;
  Texture2D texture;
  public void SetSize(int x, int y, TextureWrapMode mode = TextureWrapMode.Clamp) {
    (Size, SizeTotal) = ((x, y), x * y);
    Map = new Color[SizeTotal];
    texture = new Texture2D(y, x) {
      filterMode = FilterMode.Point,
      wrapMode = mode,
    };
  }
  public void SetSize(int val, TextureWrapMode mode = TextureWrapMode.Clamp) => SetSize(val, val, mode);
  public void SetPixel(int index, Color color) => Map[index] = color;
  public void SetPixel(int x, int y, Color color) => SetPixel(x + y * Size.y, color);
  public void FillWhole(Color color) {
    for (int i = 0; i < SizeTotal; i++)
      SetPixel(i, color);
  }
  public void Apply() {
    texture.SetPixels(Map);
    texture.Apply();
    rend.material.mainTexture = texture;
  }
}
