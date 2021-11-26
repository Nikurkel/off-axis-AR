using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float playerSpeed = 2f;
    public float sensitivity = 3f;
    public float maxYAngle = 90f;

    private CharacterController controller;
    private Vector2 currentRotation;

    private bool lockRotation;

    private void Start()
    {
        controller = gameObject.AddComponent<CharacterController>();
        //controller.radius = gameObject.GetComponentInChildren<CapsuleCollider>().radius;
        //controller.height = gameObject.GetComponentInChildren<CapsuleCollider>().height;
        lockRotation = true;
    }

    void Update()
    {
        //Get movement input
        Vector3 move = new Vector3(0,0,0);
        
        if (Input.GetKey(KeyCode.A))
        {
            move.x = -playerSpeed;
        }
        if (Input.GetKey(KeyCode.D))
        {
            move.x = playerSpeed;
        }
        if (Input.GetKey(KeyCode.W))
        {
            move.z = playerSpeed;
        }
        if (Input.GetKey(KeyCode.S))
        {
            move.z = -playerSpeed;
        }
        if (Input.GetKey(KeyCode.Space))
        {
            move.y = playerSpeed;
        }
        if (Input.GetKey(KeyCode.LeftShift))
        {
            move.y = -playerSpeed;
        }
        
        //Get camera rotation input
        if (!lockRotation)
        {
            currentRotation.x += Input.GetAxis("Mouse X") * sensitivity;
            currentRotation.y -= Input.GetAxis("Mouse Y") * sensitivity;
            currentRotation.x = Mathf.Repeat(currentRotation.x, 360);
            currentRotation.y = Mathf.Clamp(currentRotation.y, -maxYAngle, maxYAngle);
            gameObject.GetComponentInChildren<Camera>().transform.rotation = Quaternion.Euler(currentRotation.y, currentRotation.x, 0);
        }
        
        //Lock/Release mouse Cursor in Game
        if (Input.GetMouseButtonDown(0))
        {
            if(Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        //Lock Rotation to tweak stuff outside game
        if (Input.GetMouseButtonDown(1))
        {
            lockRotation = !lockRotation;
        }

        //Reset playerSpeed
        if (Input.GetMouseButtonDown(2))
        {
            playerSpeed = 2f;
        }

        //Change playerSpeed via Mouse ScrollWheel
        if (Input.GetAxis("Mouse ScrollWheel") != 0f)
        {
            playerSpeed += Input.GetAxis("Mouse ScrollWheel");
            if (playerSpeed < 0)
            {
                playerSpeed = 0;
            }
        }

        //Get forward and right direction of camera
        Vector3 forward = gameObject.GetComponentInChildren<Camera>().transform.forward;
        Vector3 right = gameObject.GetComponentInChildren<Camera>().transform.right;

        //don't change y direction to move
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        //Get direction and speed to move:
        Vector3 desiredMoveDirection = forward * move.z + right * move.x ;
        desiredMoveDirection.y = move.y;

        //Apply movement
        controller.Move(desiredMoveDirection * Time.deltaTime * playerSpeed);
    }
}