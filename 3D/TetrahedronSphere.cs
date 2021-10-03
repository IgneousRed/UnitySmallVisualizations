using System.Collections.Generic;
using UnityEngine;
public class TetrahedronSphere : MonoBehaviour {
  [Range(0, 7)]
  public int Iterations = 0;
  int iterations = -1;
  MeshFilter meshFilter;
  void Awake() => meshFilter = GetComponentInChildren<MeshFilter>();
  Mesh[] meshes;
  void Start() {
    float a = .5f / 3;
    float b = .25f - a * a;
    float c = Mathf.Sqrt(b);
    float d = c / 2;
    float e = Mathf.Sqrt(b - d * d);
    Vector3[] verts = {
      new Vector3(-e, -a, -d),
      new Vector3(0, -a, c),
      new Vector3(0, .5f, 0),
      new Vector3(e, -a, -d)
    };
    int[] trigs = {
      0, 1, 2,
      0, 2, 3,
      0, 3, 1,
      1, 3, 2
    };
    meshes = new Mesh[8];
    void save(int iter) {
      meshes[iter] = new Mesh {
        vertices = verts,
        triangles = trigs
      };
      meshes[iter].RecalculateNormals();
    }
    save(0);
    for (int iter = 1; iter < meshes.Length; iter++) {
      (var neoVerts, int neoVertsCount) = (new Vector3[verts.Length * 4 - 6], verts.Length);
      for (int i = 0; i < verts.Length; i++)
        neoVerts[i] = verts[i];
      (var neoTrigs, int neoTrigsCount) = (new int[trigs.Length * 4], 0);
      var dict = new Dictionary<(int, int), int>(6 * (int)Mathf.Pow(4, iter - 1));
      for (int trig = 0; trig < trigs.Length / 3; trig++) {
        (int i, var news) = (trig * 3, new int[3]);
        for (int t = 0; t < 3; t++) {
          (int first, int second) = (trigs[i + t], trigs[i + (t + 1) % 3]);
          if (second < first) {
            int temp = first;
            first = second;
            second = temp;
          }
          if (dict.ContainsKey((first, second)))
            news[t] = dict[(first, second)];
          else {
            int vertInd = neoVertsCount++;
            (neoVerts[vertInd], news[t]) = (Vector3.Lerp(neoVerts[first], neoVerts[second], .5f).normalized * .5f, vertInd);
            dict.Add((first, second), vertInd);
          }
        }
        void addTrigs(int vert, int newsIndex0, int newsIndex1) {
          neoTrigs[neoTrigsCount++] = vert;
          neoTrigs[neoTrigsCount++] = news[newsIndex0];
          neoTrigs[neoTrigsCount++] = news[newsIndex1];
        }
        for (int t = 0; t < 3; t++)
          addTrigs(trigs[i + t], t, (t + 2) % 3);
        addTrigs(news[0], 1, 2);
      }
      (verts, trigs) = (neoVerts, neoTrigs);
      save(iter);
    }
  }
  void Update() {
    if (iterations != Iterations)
      (meshFilter.mesh, iterations) = (meshes[Iterations], Iterations);
  }
}
