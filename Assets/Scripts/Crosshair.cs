using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crosshair : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        transform.position = new Vector2(worldPos.x, worldPos.y);
    }
}
