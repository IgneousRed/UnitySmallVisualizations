using UnityEngine;
public class Viewer : MonoBehaviour {
  public bool FlipHorizonalWhenUpsideDown = true;
  public bool Orthographic = false;
  public float OrthographicSize = 0.75f;
  public float PerspectiveFOW = 60f;
  public float PerspectiveDist = 1.5f;
  public float Speed = 0.25f;
  public Vector3 Target = Vector3.zero;
  new Camera camera;
  Transform cameraTransform;
  void Awake() {
    camera = GetComponentInChildren<Camera>();
    cameraTransform = camera.GetComponent<Transform>();
  }
  float hor;
  float ver;
  Vector3 up = Vector3.up;
  void Update() {
    if (camera.orthographic != Orthographic || camera.orthographicSize != OrthographicSize || camera.fieldOfView != PerspectiveFOW)
      (camera.orthographic, camera.orthographicSize, camera.fieldOfView) = (Orthographic, OrthographicSize, PerspectiveFOW);
    float dist = Speed * Time.deltaTime;
    float horDlt = ((Input.GetKey(KeyCode.LeftArrow) ? 1 : 0) + (Input.GetKey(KeyCode.A) ? 1 : 0)
     - (Input.GetKey(KeyCode.RightArrow) ? 1 : 0) - (Input.GetKey(KeyCode.D) ? 1 : 0)) * dist;
    hor = Mathf.Repeat(hor + (FlipHorizonalWhenUpsideDown ? horDlt * up.y : horDlt), 1);
    ver += ((Input.GetKey(KeyCode.UpArrow) ? 1 : 0) + (Input.GetKey(KeyCode.W) ? 1 : 0)
     - (Input.GetKey(KeyCode.DownArrow) ? 1 : 0) - (Input.GetKey(KeyCode.S) ? 1 : 0)) * 2 * dist * up.y;
    if (ver < -0.5f || ver > 0.5f)
      (hor, ver, up.y) = (hor + 0.5f, Mathf.PingPong(ver + 0.5f, 1) - 0.5f, up.y * -1);
    float verPi = ver * Mathf.PI;
    (float horTau, float horScale) = (hor * Mathf.PI * 2, Mathf.Cos(verPi));
    var dir = new Vector3(Mathf.Sin(horTau) * horScale, Mathf.Sin(verPi), Mathf.Cos(horTau) * horScale);
    (cameraTransform.position, cameraTransform.rotation) = (Target + (Orthographic ? dir : dir * PerspectiveDist), Quaternion.LookRotation(-dir, up));
  }
}
