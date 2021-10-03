using System.Collections.Generic;
using UnityEngine;
public class Physics : MonoBehaviour {
  public int TetraCount = 0;
  public float PointDistance = .1f;
  public float Gravity = .0001f;
  public int ConstraintUpdateCount = 1;
  int tetraCount;
  Transform tetra;
  List<MeshFilter> mF = new List<MeshFilter>();
  List<Vector3> pos = new List<Vector3>();
  List<Vector3> lastPos = new List<Vector3>();
  List<Vector3> gravity = new List<Vector3>();
  int pointCount;
  void Awake() {
    tetra = transform.Find("Tetra");
    var mesh = tetra.GetComponent<MeshFilter>().mesh;
    mesh.vertices = new Vector3[24];
    mesh.triangles = new int[] {
      0, 6, 12,
      1, 7, 18,
      2, 13, 8,
      3, 14, 19,
      4, 20, 9,
      5, 21, 15,
      10, 16, 22,
      11, 23, 17
    };
    tetra.gameObject.SetActive(true);
  }
  void Update() {
    // Tetra add/remove
    if (TetraCount != tetraCount) {
      if (TetraCount < tetraCount) {
        for (int i = tetraCount - 1; TetraCount <= i; i--) {
          Destroy(mF[i].gameObject);
          mF.RemoveAt(i);
          int i4 = i * 4;
          pos.RemoveRange(i4, 4);
          lastPos.RemoveRange(i4, 4);
          gravity.RemoveRange(i4, 4);
        }
      } else {
        for (int i = tetraCount; i < TetraCount; i++) {
          mF.Add(Instantiate(tetra, Vector3.zero, Quaternion.identity).GetComponent<MeshFilter>());
          for (int p = 0; p < 4; p++) {
            var point = Random.insideUnitSphere;
            pos.Add(point);
            lastPos.Add(point);
            gravity.Add(Random.onUnitSphere);
          }
        }
      }
      (tetraCount, pointCount) = (TetraCount, TetraCount * 4);
    }
    // Velocity, Gravity
    for (int p = 0; p < pointCount; p++) {
      var neo = pos[p] * 2 - lastPos[p] + gravity[p] * Gravity;
      lastPos[p] = pos[p];
      pos[p] = neo;
    }
    // Constrain
    for (int c = 0; c < ConstraintUpdateCount; c++) {
      // Form maintenance
      for (int t = 0; t < tetraCount; t++) {
        (var correction, int t4) = (new Vector3[4], t * 4);
        for (int p = 0; p < 3; p++)
          for (int o = p + 1; o < 4; o++) {
            Vector3 pO = pos[t4 + p] - pos[t4 + o];
            Vector3 off = pO * (PointDistance - pO.magnitude) / pO.magnitude / 3 / 2; // devided by 3 as each point is being pulled by 3 others
            (correction[p], correction[o]) = (correction[p] + off, correction[o] - off);
          }
        for (int p = 0; p < 4; p++)
          pos[t4 + p] += correction[p];
      }
      // Wall bounce
      for (int p = 0; p < pointCount; p++) {
        if (1 < pos[p].magnitude) {
          var vel = pos[p] - lastPos[p];
          pos[p] = pos[p].normalized * 2 - pos[p];
          lastPos[p] = pos[p] + vel;
          gravity[p] = Random.onUnitSphere;
        }
      }
    }
    // Update meshes
    for (int t = 0; t < tetraCount; t++) {
      Vector3[] newVerts = new Vector3[24];
      for (int p = 0; p < 4; p++)
        for (int i = 0; i < 6; i++)
          newVerts[p * 6 + i] = pos[t * 4 + p];
      mF[t].mesh.vertices = newVerts;
      mF[t].mesh.RecalculateNormals();
    }
  }
}
/* // Collision
  var bounce = new Vector3[pointCount];
  for (int t = 0; t < tetraCount; t++) {
    int t4 = t * 4;
    // Defining Planes
    var trigPoints = new int[,] {
      {t4 + 1, t4 + 2, t4 + 3},
      {t4 + 0, t4 + 2, t4 + 3},
      {t4 + 0, t4 + 1, t4 + 3},
      {t4 + 0, t4 + 1, t4 + 2}
    };
    var mains = new Vector3[4];
    var plane = new Vector3[4];
    for (int o = 0; o < 4; o++) {
      mains[o] = pos[trigPoints[o, 0]];
      var cross = Vector3.Cross(pos[trigPoints[o, 1]] - mains[o], pos[trigPoints[o, 2]] - mains[o]).normalized;
      plane[o] = Vector3.Dot(cross, pos[t4 + o] - mains[o]) < 0 ? -cross : cross;
    }
    // Checking for points in tetra
    void check(int p) {
      bool inside = true;
      for (int i = 0; i < 4; i++)
        if (Vector3.Dot(pos[p] - mains[i], plane[i]) <= 0)
          inside = false;
      if (inside) {
        int point = 0;
        float dist = (pos[t4] - pos[p]).magnitude;
        for (int o = 1; o < 4; o++) {
          float newD = (pos[t4 + o] - pos[p]).magnitude;
          if (dist < newD)
            (point, dist) = (o, newD);
        }
        var mainToPoint = pos[p] - mains[point];
        var displacement = Vector3.Project(mainToPoint, plane[point]) / 2;
        bounce[p] -= displacement;
        var hitPoint = mainToPoint - displacement * 2; //
        var q = new float[3];
        float sum = 0;
        for (int w = 0; w < 3; w++) {
          q[w] = 1 / (pos[trigPoints[point, w]] - hitPoint).magnitude;
          sum += q[w];
        }
        for (int w = 0; w < 3; w++)
          bounce[trigPoints[point, w]] += displacement * q[w] / sum;
      }
    }
    for (int p = 0; p < t * 4; p++)
      check(p);
    for (int p = (t + 1) * 4; p < pointCount; p++)
      check(p);
  }
  for (int p = 0; p < pointCount; p++)
    pos[p] += bounce[p];
*/
