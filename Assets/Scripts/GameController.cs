using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI; 

public class GameController : MonoBehaviour
{
    public GameObject[] trashPrefabs; 
    public Transform[] spawnPoints;   
    public Transform recycleBin;      
    public Transform nonRecycleBin;   

    public TextMeshProUGUI scoreText; 
    public TextMeshProUGUI timerText; 
    public TextMeshProUGUI gameOverText; 
    public Button startButton;        
    public Button restartButton;      

    private List<GameObject> currentTrashBatch; 
    private int currentTrashIndex = 0;          
    private int remainingTrashCount = 0;        

    private int score = 0;                      
    private float timer = 30.0f;                
    private bool gameStarted = false;           

    public AudioSource correctSound;
    public AudioSource wrongSound;

    public GameObject correctStreakImage;  
    public GameObject wrongStreakImage;    
    private Vector3 correctStreakStartPos; 
    private Vector3 wrongStreakStartPos;   

    private int correctStreak = 0;  
    private int wrongStreak = 0;    
    void Start()
    {
        // Initialize score and time
        scoreText.text = "Score: 0";
        timerText.text = "Time: 30";
        timerText.gameObject.SetActive(false); //Hide timer
        gameOverText.gameObject.SetActive(false); // Hide results
        startButton.gameObject.SetActive(true); // Show start button
        restartButton.gameObject.SetActive(false); // hide restart button
        startButton.onClick.AddListener(StartGame); // Startgame
        restartButton.onClick.AddListener(RestartGame); // Restart game
    }

    void Update()
    {
        if (!gameStarted) return; // pause

        // timer
        timer -= Time.deltaTime;
        timerText.text = "Time: " + Mathf.Ceil(timer).ToString();

        // stop game
        if (timer <= 0)
        {
            timerText.text = "Time: 0";
            EndGame();
            return;
        }

        // player input
        if (currentTrashIndex < currentTrashBatch.Count)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                ThrowTrash(recycleBin);
            }
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                ThrowTrash(nonRecycleBin);
            }
        }
    }

    // Function with parameter：direct trash to trash game.
    void ThrowTrash(Transform targetBin)
    {
        if (currentTrashIndex < currentTrashBatch.Count)
        {
            GameObject currentTrash = currentTrashBatch[currentTrashIndex];

            currentTrash.GetComponent<Rigidbody2D>().AddForce((targetBin.position - currentTrash.transform.position) * 100);

            //Check trash
            CheckTrash(currentTrash, targetBin);

            currentTrashIndex++; //start next trash
        }
    }

    // Function with return：Check trash in the correct bin or not.
    bool IsTrashCorrect(GameObject trash, Transform targetBin)
    {
        TrashType trashType = trash.GetComponent<TrashType>();
        return (trashType.isRecyclable && targetBin == recycleBin) || (!trashType.isRecyclable && targetBin == nonRecycleBin);
    }

    // 检查垃圾并处理结果
    void CheckTrash(GameObject trash, Transform targetBin)
    {
        if (IsTrashCorrect(trash, targetBin))
        {
            Debug.Log("Correct!");
            score++; // Plus 1
            correctStreak++; 
            wrongStreak = 0; 

            
            StartCoroutine(PlayAudioAfterDelay(correctSound, 0.25f));

           
            if (correctStreak == 10)
            {
                
                correctStreak = 0; 
            }

            Destroy(trash, 0.5f);
            Invoke("OnTrashDestroyed", 0.5f); 
        }
        else
        {
            Debug.Log("Wrong bin!");
            score--; // Minus 1
            wrongStreak++;
            correctStreak = 0; 

            trash.GetComponent<Rigidbody2D>().AddForce(new Vector2(100, 100)); //bounce

            // play audio
            StartCoroutine(PlayAudioAfterDelay(wrongSound, 0.25f));

           
            if (wrongStreak == 3)
            {
                
                wrongStreak = 0; 
            }

            // change trash direction
            StartCoroutine(ChangeTrashDirectionAfterDelay(trash));

            Destroy(trash, 1.0f); // destroy
            Invoke("OnTrashDestroyed", 1.0f); 
        }

        
        scoreText.text = "Score: " + score;
    }

    // Use coroutine to play audio
    IEnumerator PlayAudioAfterDelay(AudioSource audioSource, float delay)
    {
        yield return new WaitForSeconds(delay);
        audioSource.Play(); 
    }

    // Use Coroutine to change direction of trash
    IEnumerator ChangeTrashDirectionAfterDelay(GameObject trash)
    {
        //delay 025
        yield return new WaitForSeconds(0.25f);

        
        ChangeTrashDirection(trash);
    }

    // Function changes the direction
    
    void ChangeTrashDirection(GameObject trash)
    {
        Rigidbody2D rb = trash.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            
            Vector2 currentVelocity = rb.velocity;

            
            Vector2 newDirection = new Vector2(-currentVelocity.y, currentVelocity.x);

           
            float newSpeed = 15.0f; 

            
            rb.velocity = newDirection.normalized * newSpeed;
        }
    }

    

    // 回归原位的函数
    


    void OnTrashDestroyed()
    {
        remainingTrashCount--;
        if (remainingTrashCount <= 0)
        {
            //Spawn new trash
            SpawnTrashBatch();
        }
    }

    // Spawn new trash
    void SpawnTrashBatch()
    {
        currentTrashBatch = new List<GameObject>();
        remainingTrashCount = 3; // spawn 3
        for (int i = 0; i < 3; i++)
        {
            int randomIndex = Random.Range(0, trashPrefabs.Length);
            GameObject trash = Instantiate(trashPrefabs[randomIndex], spawnPoints[i].position, Quaternion.identity);
            currentTrashBatch.Add(trash);
        }
        currentTrashIndex = 0; 
    }

    
    void StartGame()
    {
        gameStarted = true; 
        timerText.gameObject.SetActive(true); 
        startButton.gameObject.SetActive(false); 
        gameOverText.gameObject.SetActive(false); 
        SpawnTrashBatch(); 
    }


    void EndGame()
    {
        Debug.Log("Game Over!");
        gameStarted = false;
        timerText.gameObject.SetActive(false);

       
        if (score >= 45)
        {
            gameOverText.text = "Win!";
        }
        else
        {
            gameOverText.text = "Game Over";
        }

        gameOverText.gameObject.SetActive(true);
        restartButton.gameObject.SetActive(true);

        
        foreach (GameObject trash in currentTrashBatch)
        {
            if (trash != null)
            {
                Destroy(trash);
            }
        }

        currentTrashBatch.Clear(); 
        Time.timeScale = 0; 
    }


    void RestartGame()
    {
        gameStarted = false;
        Time.timeScale = 1; 
        score = 0; 
        timer = 30.0f; 
        currentTrashIndex = 0;
        scoreText.text = "Score: 0"; 
        timerText.text = "Time: 30"; 
        gameOverText.gameObject.SetActive(false); 
        restartButton.gameObject.SetActive(false);
        StartGame(); 
    }
}