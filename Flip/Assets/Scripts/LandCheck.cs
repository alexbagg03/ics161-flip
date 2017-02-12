using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandCheck : MonoBehaviour {

    public float landWinTimer = 0.5f;
    public bool landed = false;
    public AudioClip successSound;

    private float currLandWinTime;
    private GameObject bottle;
	
    void Start()
    {
        currLandWinTime = landWinTimer;
    }
    void Update()
    {
        
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Land")
        {
            currLandWinTime = 0;
            bottle = GameObject.FindGameObjectWithTag("Bottle");

            // Make bottle "heavier" on landing
            bottle.GetComponent<Rigidbody2D>().gravityScale = 10;
        }
    }
    void OnTriggerStay2D(Collider2D other)
    {
        if (GameManager.Instance.winState)
        {
            return;
        }

        if (other.tag == "Land")
        {
            if (currLandWinTime < landWinTimer)
            {
                currLandWinTime += Time.deltaTime;
            }
            else if (currLandWinTime != landWinTimer)
            {
                landed = true;

                GameManager.Instance.IncrementFlipsLanded();

                if (GameManager.Instance.flipsLanded == GameManager.Instance.requiredFlips)
                {
                    GameManager.Instance.WinState();
                }
                else
                {
                    AudioSource.PlayClipAtPoint(successSound, Camera.main.transform.position, 0.5f);
                    GameManager.Instance.IncrementBottleCount();
                    bottle.GetComponent<BottleController>().Reset();
                }
            }
        }
    }

}
