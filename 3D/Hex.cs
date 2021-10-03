using UnityEngine;
public class Hex : MonoBehaviour {
  [Range(1, 10)]
  public int Count = 1;
  int count = 1;
  Viewer viewer;
  Transform[] blocks = new Transform[1000];
  Vector3 gridRight;
  Vector3 gridFront;
  Vector3 gridUp;
  void Awake() {
    viewer = GetComponent<Viewer>();
    float a = Mathf.Sin(Mathf.PI / 3);
    float b = a / 3;
    (gridRight, gridFront) = (new Vector3(1, 0, 0), new Vector3(.5f, 0, a));
    gridUp = new Vector3(.5f, Mathf.Sqrt(1 - Mathf.Pow(b * 2, 2)), b);
    Vector3[] otherSpheres = {
      -gridUp,
      gridRight - gridUp,
      gridFront - gridUp,
      -gridFront,
      gridRight - gridFront,
      -gridRight,
      gridRight,
      gridFront - gridRight,
      gridFront,
      gridUp - gridFront,
      gridUp - gridRight,
      gridUp,
    };
    Vector3 average4(int[] indexes) {
      Vector3 result = otherSpheres[indexes[0]];
      for (int i = 1; i < indexes.Length; i++)
        result += otherSpheres[indexes[i]];
      return result / 4;
    }
    Vector3[] verts = {
      average4(new int[] {0, 1, 2}),
      average4(new int[] {0, 1, 3, 4}),
      average4(new int[] {0, 2, 5, 7}),
      average4(new int[] {1, 2, 6, 8}),
      average4(new int[] {0, 3, 5}),
      average4(new int[] {1, 4, 6}),
      average4(new int[] {2, 7, 8}),
      average4(new int[] {3, 4, 9}),
      average4(new int[] {5, 7, 10}),
      average4(new int[] {6, 8, 11}),
      average4(new int[] {3, 5, 9, 10}),
      average4(new int[] {4, 6, 9, 11}),
      average4(new int[] {7, 8, 10, 11}),
      average4(new int[] {9, 10, 11}),
    };
    int[] trigs = {
      0, 1, 5,
      0, 2, 4,
      0, 3, 6,
      0, 4, 1,
      0, 5, 3,
      0, 6, 2,
      1, 4, 7,
      1, 7, 5,
      2, 6, 8,
      2, 8, 4,
      3, 5, 9,
      3, 9, 6,
      4, 8, 10,
      4, 10, 7,
      5, 7, 11,
      5, 11, 9,
      6, 9, 12,
      6, 12, 8,
      7, 10, 13,
      7, 13, 11,
      8, 12, 13,
      8, 13, 10,
      9, 11, 13,
      9, 13, 12,
    };
    (var vertsFlat, var trigsFlat) = (new Vector3[trigs.Length], new int[trigs.Length]);
    for (int i = 0; i < trigs.Length; i++)
      (vertsFlat[i], trigsFlat[i]) = (verts[trigs[i]], i);
    Mesh mesh = new Mesh {
      vertices = vertsFlat,
      triangles = trigsFlat,
    };
    mesh.RecalculateNormals();
    var filter = GetComponentInChildren<MeshFilter>();
    (filter.mesh, blocks[0]) = (mesh, filter.transform);
  }
  void Update() {
    if (Count != count) {
      int cubed(int val) => (int)Mathf.Pow(val, 3);
      if (Count < count) {
        for (int i = cubed(Count); i < cubed(count); i++)
          Destroy(blocks[i].gameObject);
        count = Count;
      } else while (count < Count) {
        int ind = cubed(count);
        void side(Vector3 baseDir, Vector3 major, Vector3 minor) {
          for (int a = 0; a < count; a++)
            for (int b = 0; b < count; b++)
              blocks[ind++] = Instantiate(blocks[0], baseDir * count + major * a + minor * b, Quaternion.identity);
        }
        side(gridRight, gridUp, gridFront);
        side(gridFront, gridUp, gridRight);
        side(gridUp, gridFront, gridRight);
        void edge(Vector3 baseDir, Vector3 dir) {
          for (int a = 0; a < count; a++)
            blocks[ind++] = Instantiate(blocks[0], baseDir * count + dir * a, Quaternion.identity);
        }
        edge(gridRight + gridFront, gridUp);
        edge(gridRight + gridUp, gridFront);
        edge(gridFront + gridUp, gridRight);
        blocks[ind] = Instantiate(blocks[0], (gridRight + gridFront + gridUp) * count++, Quaternion.identity);
      }
      viewer.Target = blocks[cubed(Count) - 1].position / 2;
      (viewer.OrthographicSize, viewer.PerspectiveDist) = (1.2f * Count - .5f, 1.5f + viewer.Target.magnitude);
    }
  }
}
