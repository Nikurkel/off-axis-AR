using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Virtual_Tracking : MonoBehaviour
{
    //The Tracking is poorly implemented -> not flexible at all

    public Camera projectionCam;

    private Vector3 initialPlayerPosition;
    private Vector3 newPlayerPosition;
    private Vector3 initialProjectionPosition;
    private Vector3 newProjectionPosition;

    [Header("ScreenSize / ProjectionPlaneSize")]
    public float sizeRatio = 0.1f;
    public bool flipX = false;
    public bool flipY = false;
    public bool flipZ = false;
    public bool lockX = false;
    public bool lockY = false;
    public bool lockZ = false;
    public bool switchXZ = false;

    private void Awake()
    {
        //Get Camera starting positions
        initialPlayerPosition = gameObject.transform.localPosition;
        initialProjectionPosition = projectionCam.transform.localPosition;
        
    }

    void Update()
    {
        //Move ProjectionCamera like PlayerCamera (both need to be positioned relative to their planes)
        newPlayerPosition = gameObject.transform.localPosition;
        Vector3 playerDiff = initialPlayerPosition - newPlayerPosition;
        if (flipX) playerDiff.x = -playerDiff.x;
        if (flipY) playerDiff.y = -playerDiff.y;
        if (flipZ) playerDiff.z = -playerDiff.z;
        if (lockX) playerDiff.x = 0;
        if (lockY) playerDiff.y = 0;
        if (lockZ) playerDiff.z = 0;
        if (switchXZ)
        {
            playerDiff = new Vector3(playerDiff.z,playerDiff.y,playerDiff.x);
        }

        newProjectionPosition = initialProjectionPosition - playerDiff * sizeRatio;
        projectionCam.transform.localPosition = newProjectionPosition;
    }

    //LateUpdate() to change Position after PlayerMovement
    private void LateUpdate()
    {
        //Teleport Player to initialPlayerPosition
        if (Input.GetKey(KeyCode.R))
        {
            gameObject.transform.localPosition = initialPlayerPosition;
        }
    }
}
