using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
	CharacterController CC;
	public float speed = 10f;
	private Vector3 moveVector = Vector3.zero;
	public float sensitivity = 1.0f;
	float verticalRotation = 0;
	public float upDownRange = 70.0f;
	public float jump = 10.0f;
	float grav = 0;

	
    // Start is called before the first fr
    // Start is called before the first frame update
    void Start()
    {
        CC = GetComponent<CharacterController>();
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
		Move();
    }


	//Own functions
	void Move()
	{
		        //Mouse Input
		float rotLeftRight = Input.GetAxis("Mouse X") * sensitivity;
		transform.Rotate(0, rotLeftRight, 0);

		
		verticalRotation -= Input.GetAxis("Mouse Y") * sensitivity;
		verticalRotation = Mathf.Clamp(verticalRotation, -upDownRange, upDownRange);
		Camera.main.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);

		//WASD Movement

		if(!CC.isGrounded)
		{
			grav += Physics.gravity.y * Time.deltaTime;
        }
        moveVector = new Vector3(Input.GetAxis("Horizontal") * Time.deltaTime, grav, Input.GetAxis("Vertical") * Time.deltaTime);
        moveVector = transform.rotation * moveVector * speed;
        CC.Move(moveVector);
	}
}
