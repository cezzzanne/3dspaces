using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapPlayerScript : MonoBehaviour {
    // Start is called before the first frame update
    public float maxSizeParentX = -1.0f;
    public float maxSizeParentY = -1.0f;

    Rect rect;

    void Start() {
        rect = transform.parent.GetComponent<RectTransform>().rect;

    }

   

    public void MoveMarker(float xRatio, float zRatio) {
        float x = xRatio * (rect.max.x - rect.min.x);
        float y = zRatio * (rect.max.y - rect.min.y);
        transform.localPosition = new Vector3(rect.min.x + x, rect.min.y + y, 0);
    }
}
