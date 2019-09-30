using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Grimm
{
    public class GameManager : MonoBehaviour
    {
        #region Variables
        //The Snake
        public GameObject applePrefab;
        public GameObject snakeBodyPrefab;
        public float moveRate = 0.1f;
        float timer;
        Vector2 dir;
        bool isMoving;
        List<Transform> tail = new List<Transform>();

        //The Score
        bool isScore;
        int currentScore;
        int highscore;

        //The Map
        public Transform cameraHolder;
        public Transform topBorder;
        public Transform bottomBorder;
        public Transform leftBorder;
        public Transform rightBorder;

        //Unity Events
        public UnityEvent onStart;
        public UnityEvent onGameOver;
        public UnityEvent onScore;

        bool isGameOver;

        //UI Elements
        public Text currentScoreText;
        public Text highscoreText;
        #endregion Variables

        #region Start
        private void Start()
        {
            onStart.Invoke();
        }

        public void StartNewGame()
        {
            transform.position = new Vector2(0, 0); //Places the player on the coordinates 0,0 
            isGameOver = false;
            isMoving = true;
            PlaceApple();
            UpdateScore();

            if (tail.Count > 0)
            {
                tail.Clear();
            }
        }
        #endregion Start

        #region Update
        private void Update()
        {
            GetDirection();

            timer += Time.deltaTime; //A timer that counts up each second
            if (timer > moveRate) //If timer is higher than move rate, move the player
            {
                timer = 0;
                MovePlayer();
            }

            if (isGameOver)
            {
                isMoving = false;

                if (Input.GetKeyDown(KeyCode.R))
                {
                    onStart.Invoke();
                    SceneManager.LoadScene(0); //Reloads the scene
                }
            }
        }

        private void MovePlayer()
        {
            if (isMoving)
            {
                Vector2 currentPos = transform.position; //Current position is the position of the head of the player
                transform.Translate(dir); //Moves the player 

                if (tail.Count > 0) //Moves the tail
                {
                    tail.Last().position = currentPos; //the position of the last object in index gets the position of the head  
                    tail.Insert(0, tail.Last()); //Insterts the last object first in the list (does it duplicate? thats why we have to remove?)
                    tail.RemoveAt(tail.Count - 1); //Removes the last  
                }

                if (isScore) //If we eat an apple
                {
                    isScore = false;

                    currentScore++;

                    if (currentScore >= PlayerPrefs.GetInt("highscore", 0)) //If our current score is higher than the highscore
                    {
                        highscore = currentScore; //The highscore is the same as the current score
                        PlayerPrefs.SetInt("highscore", highscore); //Updates the highscore
                    }

                    onScore.Invoke(); //Updates the both types of score

                    GameObject g = (GameObject)Instantiate(snakeBodyPrefab, currentPos, Quaternion.identity); //Creates a snakebody on the players current(technically the previous) position 

                    tail.Insert(0, g.transform); // Places the new snakebody first in the list
                }
            }
        }

        public void UpdateScore() //Updates the score
        {
            currentScoreText.text = currentScore.ToString();

            highscoreText.text = PlayerPrefs.GetInt("highscore", 0).ToString(); //PlayerPrefs saves the data on the computer
        }
        #endregion Update

        #region Utilities - Get direction, place apple, etc.
        void GetDirection() //Sets the direction the player moves next time
        {
            if (Input.GetButtonDown("Up")) //If you press 'up' 
            {
                if (dir != new Vector2(0, -1)) //And the current direction your going isn't down
                {
                    dir = Vector2.up; //Set the direction to up
                    Debug.Log("up");
                }
            }
            else if (Input.GetButtonDown("Down"))
            {
                if (dir != new Vector2(0, 1))
                {
                    dir = Vector2.down;
                    Debug.Log("down");
                }
            }
            else if (Input.GetButtonDown("Left"))
            {
                if (dir != new Vector2(1, 0))
                {
                    dir = Vector2.left;
                    Debug.Log("left");
                }
            }
            else if (Input.GetButtonDown("Right"))
            {
                if (dir != new Vector2(-1, 0))
                {
                    dir = Vector2.right;
                    Debug.Log("right");
                }
            }
        }

        void PlaceApple() //Places the apple somewhere within the borders
        {
            int x = (int)UnityEngine.Random.Range(leftBorder.position.x, rightBorder.position.x); //Gets the max and min values on the x axis (the left border x position is the minimum value)
            int y = (int)UnityEngine.Random.Range(topBorder.position.y, bottomBorder.position.y); //Gets the max and min values on the v axis
            Instantiate(applePrefab, new Vector2(x, y), Quaternion.identity); //Places the apple prefab within the borders 
        }
        #endregion Utilities

        #region Collision
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.tag == "Apple") //If it collides with an apple
            {
                Debug.Log("U ate the apple");
                isScore = true;
                Destroy(collision.gameObject); //Destroys the apple you ate
                PlaceApple(); //Crteates a new apple
            }
            else if (collision.tag == "Border" || collision.tag == "Body") //If it collides with something that isn't an apple
            {
                onGameOver.Invoke();
                isGameOver = true;
                Debug.Log("U lost");
            }
        }
        #endregion Collision
    }
}
