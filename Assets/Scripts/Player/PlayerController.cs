using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerController : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 direction;
    public float forwardSpeed;
    public float maxSpeed;

    private int desiredLane = 1; //0: left, 1: middle, 2: right
    public float laneDistance = 4; //distance between 2 lanes
    private bool isSliding = false;

    public float jumpForce;
    public float Gravity = -30;

    public Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!PlayerManager.isGameStarted)
            return;

        if(forwardSpeed < maxSpeed)
            forwardSpeed += 0.1f * Time.deltaTime;
        
        bool isGrounded = controller.isGrounded;

        animator.SetBool("isGameStarted",true);
        animator.SetBool("isGrounded", isGrounded);

        direction.z = forwardSpeed;

        //Sliding
        if(SwipeManager.swipeDown && !isSliding)
        {
            StartCoroutine(Slide());
        }

        //Lane Change
        if(SwipeManager.swipeRight)
        {
            if(desiredLane != 2)
                desiredLane++;
        }
        if(SwipeManager.swipeLeft)
        {
            if(desiredLane != 0)
                desiredLane--;
        }
        //Jump
        if(isGrounded)
        {
            if (SwipeManager.swipeUp)
            {
                Jump();    
            }
        }
        else
        {
            direction.y += Gravity*Time.deltaTime;
        }
    }
    private void FixedUpdate() 
    {

        if (!PlayerManager.isGameStarted)
            return;
        
        controller.Move(direction * Time.fixedDeltaTime);   
        
        Vector3 targetPosition = transform.position.z*transform.forward + transform.position.y*transform.up;

        //Calculate New position
        if(desiredLane==0)
        {
            targetPosition += Vector3.left*laneDistance;
        }else if(desiredLane == 2)
        {
            targetPosition += Vector3.right*laneDistance;
        }

        /**transform.position = Vector3.Lerp(transform.position, targetPosition, 10*Time.deltaTime);
        controller.center = controller.center;**/
        if (transform.position != targetPosition)
        {   
            Vector3 diff = targetPosition - transform.position;
            Vector3 moveDir = diff.normalized * 25 * Time.deltaTime;
            if (moveDir.sqrMagnitude < diff.sqrMagnitude)
                controller.Move(moveDir);
            else
                controller.Move(diff);
        }
    }
    private void Jump()
    {
        direction.y = jumpForce;
    }

    //GameOver
    private void OnControllerColliderHit(ControllerColliderHit hit) 
    {
        if (hit.transform.tag=="Obstacle")
        {
            PlayerManager.gameOver = true;
            FindObjectOfType<AudioManager>().PlaySound("GameOver");
        }    
    }

    private IEnumerator Slide()
    {
        isSliding = true;

        animator.SetBool("isSliding",true);
        controller.center = new Vector3(0,-0.5f, 0);
        controller.height = 1;
        direction.y = -30;
        yield return new WaitForSeconds(1.3f);

        controller.center = new Vector3(0, 0, 0);
        controller.height = 2;
        animator.SetBool("isSliding",false);
        isSliding = false;
    }
}
