using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class PlayerComponent : MonoBehaviour
{
    public Vector3 mousePosition;
    public Vector3 worldPosition;
    public LayerMask layersToHit;
    public GameObject UIStart;
    public GameObject UIEnd;
    public GameObject UIBlackScreen;
    public AudioSource crateDestroy;
    public GameObject timer;
    private Vector3 newDirection;

    public NavMeshAgent navMeshAgent;
    private bool gameOn = false;
    private bool gameWon = false;
    private bool gameLost = false;
    private bool startMovingMainScreen = false;
    public bool getMaxRotation = false;
    float countdown = 11.0f;
    public float speed = 5f;
    public float rotationSpeed = 720f;

    private int cratesDestroyed = 0;

    private void Start()
    {
        UIBlackScreen.SetActive(true);
        UIStart.SetActive(true);
        worldPosition = transform.position;
    }
    void FixedUpdate()
    {
        MoveStartUI();
        
        if (gameOn)
        {
            MovePlayer();
            Timer();
            if (cratesDestroyed == 3) EndGame("Win");
        }
        if (gameWon) SpinPlayer();        
        if (gameLost) moveEndUI();        
    }


    public void BeginGame()
    {
        startMovingMainScreen = true;
    }

    public void EndGame(string state)
    {
        gameOn = false;
        if(state == "Win") gameWon = true;
        else gameLost = true;                    
    }


    private void MovePlayer()
    {
        if (gameOn)
        {
            if (Input.GetMouseButton(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    worldPosition = new Vector3(hit.point.x, transform.position.y, hit.point.z);
                }

                Vector3 targetDirection = worldPosition - transform.position;
                if (Vector3.Distance(worldPosition, transform.position) > 0.5f)
                {
                    newDirection = Vector3.RotateTowards(transform.forward, targetDirection, speed * Time.deltaTime, 0.0f);
                    transform.rotation = Quaternion.LookRotation(newDirection);
                }
                transform.position = Vector3.MoveTowards(transform.position, worldPosition, speed * Time.deltaTime);
            }
        }
    }

    private void SpinPlayer()
    {
        if (!getMaxRotation)
        {
            if (rotationSpeed < 800f)
            {
                rotationSpeed = rotationSpeed + 3;
            }
            else
            {
                getMaxRotation = true;
            }
        }

        if (getMaxRotation)
        {
            GetComponent<Renderer>().material.SetColor("_Color", Color.red);
            if (rotationSpeed > 0f)
            {
                rotationSpeed = rotationSpeed - 3;
            }
            else
            {
                rotationSpeed = 0f;
                moveEndUI();
            }
        }

        Vector3 v3 = new Vector3(0, rotationSpeed, 0);
        transform.Rotate(v3 * Time.deltaTime);
    }

    private void MoveStartUI()
    {
        if (startMovingMainScreen)
        {
            int topSide = 1080 * 2;
            UIStart.transform.position = Vector3.MoveTowards(UIStart.transform.position, new Vector3(UIStart.transform.position.x, topSide, 0), 128 * speed * Time.deltaTime);
            if (UIStart.transform.position.y == topSide)
            {
                UIBlackScreen.SetActive(false);
                UIStart.SetActive(false);
                gameOn = true;
                startMovingMainScreen = false;
            }
        }
    }

    private void moveEndUI()
    {
        UIEnd.SetActive(true);
        if (gameWon) UIEnd.GetComponentInChildren<Text>().text = "You Win";
        Vector3 pos = UIEnd.GetComponentInChildren<Text>().transform.position;
        UIEnd.GetComponentInChildren<Text>().transform.position = Vector3.MoveTowards(pos, new Vector3(pos.x, 540, 0), 128 * speed * Time.deltaTime);
    }

    private void Timer()
    {
        if (countdown > 0)
        {
            countdown -= (Time.deltaTime);
            string s = countdown.ToString().Split(',')[0];
            timer.GetComponent<Text>().text = s;
        }
        else
        {
            timer.GetComponent<Text>().text = "0";
            EndGame("Lose");
        }
        
    }

    private void OnCollisionEnter(Collision other)
    {
        string tag = other.gameObject.tag;
        if (gameOn)
        {
            if (tag == "Item")
            {
                Destroy(other.gameObject);
                crateDestroy.Play();
                cratesDestroyed++;

            }
            if (tag == "Wall")
            {
                worldPosition = transform.position;
            }
        }
    }

}