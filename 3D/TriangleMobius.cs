using UnityEngine;

public class TriangleMobius : MonoBehaviour {
  [Range(10, 1000)]
  public int Tetras = 100;
  MeshFilter meshFilter;
  void Awake() => meshFilter = GetComponentInChildren<MeshFilter>();
  int tetras;
  void Update() {
    if (tetras != Tetras) {
      int vertsLen = Tetras + 2;
      var verts = new Vector3[vertsLen];
      for (int vert = 0; vert < vertsLen; vert++) {
        float perTau = (float)vert / vertsLen * Mathf.PI * 2;
        (float perTau3, float hor) = (perTau * 3, .4f + Mathf.Cos(perTau) * .1f);
        verts[vert] = new Vector3(Mathf.Sin(perTau3) * hor, Mathf.Sin(perTau) * .1f, Mathf.Cos(perTau3) * hor);
      }
      (var trigs, int third) = (new int[vertsLen * 6], vertsLen / 3);
      for (int vert = 0; vert < vertsLen; vert++) {
        (int index, int next) = (vert * 6, vert + 1);
        int otherNext = (next + third) % vertsLen;
        trigs[index++] = vert;
        trigs[index++] = next % vertsLen;
        trigs[index++] = otherNext;
        trigs[index++] = vert;
        trigs[index++] = otherNext;
        trigs[index] = (vert + third) % vertsLen;
      }
      var mesh = new Mesh {
        vertices = verts,
        triangles = trigs,
      };
      mesh.RecalculateNormals();
      (meshFilter.mesh, tetras) = (mesh, Tetras);
    }
  }
}
