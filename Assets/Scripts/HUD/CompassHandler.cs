using UnityEngine;
using UnityEngine.UI;

public class CompassHandler : MonoBehaviour
{
    public RawImage compass;
    public Transform player;

    private void Update() {
        
        compass.uvRect = new Rect(player.localEulerAngles.y / 360f, 0, 1, 1);
    }

    //public Transform player;
    //public float compassWidthPixels;
    //Vector3 startPos;
    //float angle;

    //void Start() {

    //    startPos = transform.position;
    //    angle = compassWidthPixels / 360f;
    //}

    //void Update() {

    //    Vector3 perp = Vector3.Cross(Vector3.forward, player.forward);
    //    float dir = Vector3.Dot(perp, Vector3.up);
    //    transform.position = startPos + new Vector3(Vector3.Angle(player.forward, Vector3.forward) * Mathf.Sign(dir) * angle, 0, 0);
    //}
}
