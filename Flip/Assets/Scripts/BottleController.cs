using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BottleController : MonoBehaviour {

    public float landResetTimer = 1f;
    public AudioClip flipSound;
    public AudioClip bummerSound;
    public AudioClip resetSound;

    private Vector3 startPos;
    private GameObject anchor;
    private GameObject weight;
    private Vector3 anchorStartPos;
    private bool resetOnButton;
    private bool flipped;
    private float currLandResetTime;
    private float prevRotationZ;
    private float throwForce;
    private float timeCheck;
    private float angleCheck;

    public enum FacingDirection
    {
        UP = 270,
        DOWN = 90,
        LEFT = 180,
        RIGHT = 0
    }

    void Start ()
    {
        // Start position
        startPos = transform.position;

        // Anchor
        anchor = GameObject.FindGameObjectWithTag("Anchor");
        anchorStartPos = anchor.transform.position;

        // Weight
        weight = GameObject.FindGameObjectWithTag("Weight");

        // Reset timer upon landing
        currLandResetTime = landResetTimer;
    }
	void Update ()
    {
        if (!GameManager.Instance.roundActive)
        {
            return;
        }

        // Reset anchor and bottle
        if (transform.position.y <= -5)
        {
            CheckState();
        }

        if (flipped)
        {
            return;
        }

        // Mouse click down
        if (Input.GetMouseButton(0))
        {
            Cursor.visible = false;

            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // Angle of rotation calculation based on mouse position
            //float x = mousePosition.x - transform.position.x;

            //if (x > 0.5f)
            //{
            //    x += 5;
            //}
            //else if (x < -0.5f)
            //{
            //    x -= 5;
            //}

            //float d = Vector3.Distance(mousePosition, transform.position);
            //float theta = Mathf.Asin(x / d) * Mathf.Rad2Deg;

            //transform.rotation = Quaternion.Euler(0, 0, theta);
            //anchor.transform.rotation = Quaternion.Euler(0, 0, theta);

            transform.rotation = FaceObject(transform.position, mousePosition, FacingDirection.UP);
            float angle = -(transform.rotation.z * 100f);

            print("Angle = " + angle);

            timeCheck += Time.deltaTime;

            if (timeCheck >= 0.1f)
            {
                if (angleCheck < angle && angle > 0f)
                {
                    float angleDiff = Mathf.Abs(angleCheck) + Mathf.Abs(angle);
                    throwForce = GetForcePotentialFromAngle(angleDiff);
                }
                else
                {
                    throwForce = 0;
                    angleCheck = 0;
                }

                timeCheck = 0;
                angleCheck = angle;
            }
        }
        // Mouse click up
        else if (Input.GetMouseButtonUp(0))
        {
            if (!resetOnButton)
            {
                Throw();
            }
            else
            {
                resetOnButton = false;
            }
        }
    }
    void OnCollisionEnter2D(Collision2D coll)
    {
        if (coll.gameObject.tag == "Land")
        {
            currLandResetTime = 0;
        }
    }
    void OnCollisionStay2D(Collision2D coll)
    {
        if (coll.gameObject.tag == "Land")
        {
            if (GameManager.Instance.winState)
            {
                return;
            }

            // Round off rotations to 3 decimal places
            float prevZRounded = Mathf.Round(prevRotationZ * 1000.0f) / 1000.0f;
            float transZRounded = Mathf.Round(transform.localRotation.z * 1000.0f) / 1000.0f;

            // Don't continue reset timer if the bottle is still rotating
            if (prevZRounded != transZRounded)
            {
                prevRotationZ = transform.localRotation.z;
                return;
            }
            
            // Bottle is at rest, so start reset timer
            if (currLandResetTime < landResetTimer)
            {
                currLandResetTime += Time.deltaTime;
            }
            else if (currLandResetTime != landResetTimer && currLandResetTime > 0)
            {
                CheckState();
            }
        }
    }
    public void ResetOnButton()
    {
        Reset();
        resetOnButton = true;
    }
    public void Reset()
    {
        // Reset bottle and anchor positions
        anchor.transform.position = anchorStartPos;
        transform.position = startPos;
        transform.rotation = Quaternion.Euler(0, 0, 0);
       
        // Reset components
        GetComponent<Rigidbody2D>().gravityScale = 1f;
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        GetComponent<Rigidbody2D>().angularVelocity = 0;
        GetComponent<HingeJoint2D>().enabled = true;

        // Reset throw values
        throwForce = 0;
        angleCheck = 0;

        Cursor.visible = true;
        flipped = false;
    }
    private void Throw()
    {
        // Disable hinge joint and add "throw" force to bottle
        GetComponent<HingeJoint2D>().enabled = false;
        Vector2 weightPos = GameObject.FindGameObjectWithTag("Weight").transform.position;
        GetComponent<Rigidbody2D>().AddForceAtPosition(new Vector2(throwForce, throwForce), weightPos);

        Cursor.visible = true;
        flipped = true;
        GameManager.Instance.DecrementBottleCount();
        AudioSource.PlayClipAtPoint(flipSound, Camera.main.transform.position, 0.5f);
    }
    private void CheckState()
    {
        if (!GameManager.Instance.roundActive)
        {
            return;
        }

        if (GameManager.Instance.bottleCount == 0)
        {
            AudioSource.PlayClipAtPoint(bummerSound, Camera.main.transform.position, 0.5f);
            GameManager.Instance.LoseState();
        }
        else if (GameManager.Instance.bottleCount < (GameManager.Instance.requiredFlips - GameManager.Instance.flipsLanded))
        {
            AudioSource.PlayClipAtPoint(bummerSound, Camera.main.transform.position, 0.5f);
            GameManager.Instance.LoseState();
        }
        else
        {
            AudioSource.PlayClipAtPoint(resetSound, Camera.main.transform.position, 0.25f);
            Reset();
        }
    }
    public static Quaternion FaceObject(Vector2 startingPosition, Vector2 targetPosition, FacingDirection facing)
    {
        Vector2 direction = targetPosition - startingPosition;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        angle -= (float)facing;
        return Quaternion.AngleAxis(angle, Vector3.forward);
    }
    private float GetForcePotentialFromAngle(float angle)
    {
        float forcePotential = 0;

        if (angle > 0 && angle < 10f)
        {
            forcePotential = 50f;
        }
        else if (angle > 10f && angle < 20f)
        {
            forcePotential = 100f;
        }
        else if (angle > 20f && angle < 30f)
        {
            forcePotential = 150f;
        }
        else if (angle > 30f && angle < 40f)
        {
            forcePotential = 200f;
        }
        else if (angle > 40f && angle < 50f)
        {
            forcePotential = 2500f;
        }
        else if (angle > 50f && angle < 60f)
        {
            forcePotential = 300f;
        }
        else if (angle > 60f && angle < 70f)
        {
            forcePotential = 350f;
        }
        else if (angle > 70f && angle < 80f)
        {
            forcePotential = 400f;
        }
        else if (angle > 80f && angle < 90f)
        {
            forcePotential = 450f;
        }
        else if (angle > 90f && angle < 100f)
        {
            forcePotential = 500f;
        }
        else if (angle > 100f && angle < 120f)
        {
            forcePotential = 550f;
        }
        else if (angle > 120f && angle < 140f)
        {
            forcePotential = 600f;
        }
        else if (angle > 140f && angle < 180f)
        {
            forcePotential = 650f;
        }

        return forcePotential;
    }

}
