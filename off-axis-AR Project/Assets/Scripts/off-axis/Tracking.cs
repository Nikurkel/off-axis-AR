using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tracking : MonoBehaviour
{
    private GameObject arPlane;
    public GameObject arCamera;

    void Update()
    {
        if (arPlane == null){
            if (GameObject.Find("tracking point") != null){
                arPlane = GameObject.Find("tracking point").transform.parent.gameObject;
            }
        }else{
            float x = arCamera.transform.localPosition.x - arPlane.transform.localPosition.x;
            float y = arCamera.transform.localPosition.y - arPlane.transform.localPosition.y;
            float z = arCamera.transform.localPosition.z - arPlane.transform.localPosition.z;
            gameObject.transform.localPosition = new Vector3(x* 20,y* 20,z* 20) ;
        }
        
    }
}
