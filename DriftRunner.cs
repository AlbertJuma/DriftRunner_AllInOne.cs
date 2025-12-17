using UnityEngine;
using System.Collections.Generic;

public class DriftRunner : MonoBehaviour
{
    // Player settings
    private GameObject player;
    private float playerSpeed = 10f;
    private float basePlayerSpeed = 10f;
    private float speedIncreaseRate = 0.5f;
    private float horizontalSpeed = 8f;
    private float inputSensitivity = 0.01f;
    private float trackWidth = 4f;
    private Vector3 targetPosition;
    
    // Obstacle settings
    private List<GameObject> obstacles = new List<GameObject>();
    private float obstacleSpawnDistance = 50f;
    private float lastObstacleZ = 0f;
    private float minSpawnDistance = 15f;
    private float maxSpawnDistance = 25f;
    private float obstacleDestroyDistance = 30f;
    
    // Camera settings
    private GameObject mainCamera;
    private Vector3 cameraOffset = new Vector3(0f, 5f, -10f);
    private float cameraSmoothSpeed = 5f;
    private float cameraTiltAmount = 5f;
    
    // Game state
    private bool isGameOver = false;
    private float score = 0f;
    private float distanceTraveled = 0f;
    
    // Input
    private Vector3 lastInputPosition;
    private bool isDragging = false;
    
    // GUI cache
    private GUIStyle scoreStyle;
    private GUIStyle gameOverStyle;
    private GUIStyle finalScoreStyle;
    private GUIStyle restartStyle;
    private bool guiInitialized = false;

    void Start()
    {
        InitializeGame();
    }

    void InitializeGame()
    {
        // Create player
        player = GameObject.CreatePrimitive(PrimitiveType.Cube);
        player.name = "Player";
        player.transform.position = new Vector3(0f, 0.5f, 0f);
        player.transform.localScale = new Vector3(1f, 1f, 1f);
        player.GetComponent<Renderer>().material.color = Color.green;
        targetPosition = player.transform.position;
        
        // Create camera
        mainCamera = Camera.main.gameObject;
        if (mainCamera == null)
        {
            mainCamera = new GameObject("Main Camera");
            mainCamera.AddComponent<Camera>();
            mainCamera.tag = "MainCamera";
        }
        mainCamera.transform.position = player.transform.position + cameraOffset;
        mainCamera.transform.LookAt(player.transform);
        
        // Create initial ground plane (visual reference)
        CreateGroundPlane();
        
        // Spawn initial obstacles
        for (int i = 0; i < 5; i++)
        {
            SpawnObstacle();
        }
        
        // Reset game state
        isGameOver = false;
        score = 0f;
        distanceTraveled = 0f;
        playerSpeed = basePlayerSpeed;
    }

    void CreateGroundPlane()
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = new Vector3(0f, 0f, 0f);
        ground.transform.localScale = new Vector3(10f, 1f, 100f);
        ground.GetComponent<Renderer>().material.color = new Color(0.3f, 0.3f, 0.3f);
    }

    void Update()
    {
        if (isGameOver)
        {
            HandleRestart();
            return;
        }
        
        HandleInput();
        MovePlayer();
        UpdateCamera();
        ManageObstacles();
        UpdateScore();
        IncreaseSpeed();
    }

    void HandleInput()
    {
        // Touch input for mobile
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            
            if (touch.phase == TouchPhase.Began)
            {
                lastInputPosition = touch.position;
                isDragging = true;
            }
            else if (touch.phase == TouchPhase.Moved && isDragging)
            {
                float deltaX = (touch.position.x - lastInputPosition.x) * inputSensitivity;
                targetPosition.x += deltaX * horizontalSpeed * Time.deltaTime;
                lastInputPosition = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                isDragging = false;
            }
        }
        // Mouse input for editor/PC
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                lastInputPosition = Input.mousePosition;
                isDragging = true;
            }
            else if (Input.GetMouseButton(0) && isDragging)
            {
                float deltaX = (Input.mousePosition.x - lastInputPosition.x) * inputSensitivity;
                targetPosition.x += deltaX * horizontalSpeed * Time.deltaTime;
                lastInputPosition = Input.mousePosition;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
            }
        }
        
        // Clamp horizontal position within track width
        targetPosition.x = Mathf.Clamp(targetPosition.x, -trackWidth, trackWidth);
    }

    void MovePlayer()
    {
        // Move forward automatically
        targetPosition.z += playerSpeed * Time.deltaTime;
        
        // Smooth interpolation to target position
        player.transform.position = Vector3.Lerp(
            player.transform.position,
            new Vector3(targetPosition.x, player.transform.position.y, targetPosition.z),
            Time.deltaTime * 10f
        );
        
        distanceTraveled += playerSpeed * Time.deltaTime;
    }

    void UpdateCamera()
    {
        // Smooth follow
        Vector3 desiredPosition = player.transform.position + cameraOffset;
        mainCamera.transform.position = Vector3.Lerp(
            mainCamera.transform.position,
            desiredPosition,
            Time.deltaTime * cameraSmoothSpeed
        );
        
        // Look at player
        mainCamera.transform.LookAt(player.transform.position + Vector3.up * 2f);
        
        // Slight tilt based on horizontal movement
        float tilt = (player.transform.position.x / trackWidth) * cameraTiltAmount;
        mainCamera.transform.rotation = Quaternion.Euler(
            mainCamera.transform.rotation.eulerAngles.x,
            mainCamera.transform.rotation.eulerAngles.y,
            -tilt
        );
    }

    void ManageObstacles()
    {
        // Spawn new obstacles
        float spawnThreshold = player.transform.position.z + obstacleSpawnDistance;
        
        while (lastObstacleZ < spawnThreshold)
        {
            SpawnObstacle();
        }
        
        // Destroy obstacles behind player
        for (int i = obstacles.Count - 1; i >= 0; i--)
        {
            if (obstacles[i] != null && 
                obstacles[i].transform.position.z < player.transform.position.z - obstacleDestroyDistance)
            {
                Destroy(obstacles[i]);
                obstacles.RemoveAt(i);
            }
        }
        
        // Check collisions
        CheckCollisions();
    }

    void SpawnObstacle()
    {
        float spawnDistance = Random.Range(minSpawnDistance, maxSpawnDistance);
        float zPosition = lastObstacleZ + spawnDistance;
        float xPosition = Random.Range(-trackWidth + 1f, trackWidth - 1f);
        
        GameObject obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obstacle.name = "Obstacle";
        obstacle.transform.position = new Vector3(xPosition, 0.5f, zPosition);
        obstacle.transform.localScale = new Vector3(1f, 1f, 1f);
        obstacle.GetComponent<Renderer>().material.color = Color.red;
        
        obstacles.Add(obstacle);
        lastObstacleZ = zPosition;
    }

    void CheckCollisions()
    {
        foreach (GameObject obstacle in obstacles)
        {
            if (obstacle != null)
            {
                float distance = Vector3.Distance(
                    new Vector3(player.transform.position.x, 0, player.transform.position.z),
                    new Vector3(obstacle.transform.position.x, 0, obstacle.transform.position.z)
                );
                
                if (distance < 1f) // Collision threshold
                {
                    GameOver();
                    return;
                }
            }
        }
    }

    void UpdateScore()
    {
        score = distanceTraveled;
    }

    void IncreaseSpeed()
    {
        playerSpeed = basePlayerSpeed + (distanceTraveled * speedIncreaseRate * 0.01f);
    }

    void GameOver()
    {
        isGameOver = true;
        player.GetComponent<Renderer>().material.color = Color.yellow;
    }

    void HandleRestart()
    {
        // Check for tap/click to restart
        bool restartInput = false;
        
        if (Input.touchCount > 0)
        {
            if (Input.GetTouch(0).phase == TouchPhase.Began)
            {
                restartInput = true;
            }
        }
        else if (Input.GetMouseButtonDown(0))
        {
            restartInput = true;
        }
        
        if (restartInput)
        {
            RestartGame();
        }
    }

    void RestartGame()
    {
        // Destroy all obstacles
        foreach (GameObject obstacle in obstacles)
        {
            if (obstacle != null)
            {
                Destroy(obstacle);
            }
        }
        obstacles.Clear();
        
        // Reset player position
        player.transform.position = new Vector3(0f, 0.5f, 0f);
        targetPosition = player.transform.position;
        player.GetComponent<Renderer>().material.color = Color.green;
        
        // Reset camera
        mainCamera.transform.position = player.transform.position + cameraOffset;
        mainCamera.transform.LookAt(player.transform);
        
        // Reset game state
        isGameOver = false;
        score = 0f;
        distanceTraveled = 0f;
        playerSpeed = basePlayerSpeed;
        lastObstacleZ = 0f;
        
        // Spawn initial obstacles
        for (int i = 0; i < 5; i++)
        {
            SpawnObstacle();
        }
    }

    void OnGUI()
    {
        // Initialize GUI styles once
        if (!guiInitialized)
        {
            scoreStyle = new GUIStyle(GUI.skin.label);
            scoreStyle.fontSize = 24;
            scoreStyle.normal.textColor = Color.white;
            
            gameOverStyle = new GUIStyle(GUI.skin.label);
            gameOverStyle.fontSize = 48;
            gameOverStyle.normal.textColor = Color.white;
            
            finalScoreStyle = new GUIStyle(GUI.skin.label);
            finalScoreStyle.fontSize = 32;
            finalScoreStyle.normal.textColor = Color.white;
            
            restartStyle = new GUIStyle(GUI.skin.label);
            restartStyle.fontSize = 24;
            restartStyle.normal.textColor = Color.white;
            
            guiInitialized = true;
        }
        
        if (!isGameOver)
        {
            GUI.Label(new Rect(10, 10, 300, 30), "Score: " + Mathf.FloorToInt(score), scoreStyle);
            GUI.Label(new Rect(10, 40, 300, 30), "Speed: " + Mathf.FloorToInt(playerSpeed), scoreStyle);
        }
        else
        {
            // Game Over screen
            GUI.Label(new Rect(Screen.width / 2 - 150, Screen.height / 2 - 100, 300, 60), "GAME OVER", gameOverStyle);
            GUI.Label(new Rect(Screen.width / 2 - 150, Screen.height / 2 - 30, 300, 40), "Score: " + Mathf.FloorToInt(score), finalScoreStyle);
            GUI.Label(new Rect(Screen.width / 2 - 150, Screen.height / 2 + 30, 300, 30), "Tap/Click to Restart", restartStyle);
        }
    }
}
