using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class DifficultyTier
{
    public string tierName;
    public int scoreThreshold;
    public GameObject[] chunkPrefabs;
}

public enum GameState { EndlessRunner, BossTransition, BossFight }

public class LevelSpawner : MonoBehaviour
{
    public static LevelSpawner Instance; 

    [Header("Game State")]
    public GameState currentState = GameState.EndlessRunner; 
    public int currentScore = 0;
    public int bossScoreThreshold = 500; 

    [Header("Difficulty Tiers")]
    public List<DifficultyTier> difficultyTiers;

    [Header("Spawning Metrics")]
    public float chunkLength = 20f; 
    private float spawnXPosition = 0f; 
    private List<GameObject> activeChunks = new List<GameObject>(); 

    [Header("Cutscene Settings")]
    public GameObject bossArenaPrefab; 
    public Transform playerShip; 
    public float cutsceneDuration = 3.5f; 
    public float bossOrbitRadius = 35f; 

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        SpawnNextChunk();
        SpawnNextChunk();
        SpawnNextChunk();
    }

    public void SpawnNextChunk()
    {
        DifficultyTier currentTier = difficultyTiers[0]; 
        foreach (DifficultyTier tier in difficultyTiers)
        {
            if (currentScore >= tier.scoreThreshold)
            {
                currentTier = tier; 
            }
        }

        int randomIndex = Random.Range(0, currentTier.chunkPrefabs.Length);
        GameObject chunkToSpawn = currentTier.chunkPrefabs[randomIndex];

        GameObject newChunk = Instantiate(chunkToSpawn, new Vector3(spawnXPosition, 0, 0), Quaternion.identity);
        activeChunks.Add(newChunk);

        spawnXPosition += chunkLength;

        // Keep 5 chunks active in memory
        if (activeChunks.Count > 4)
        {
            Destroy(activeChunks[0]); 
            activeChunks.RemoveAt(0); 
        }
    }

    public void AddScoreAndSpawn(int pointsAwarded)
    {
        // Safety lock: Only count score/spawn chunks if we are in the runner phase
        if (currentState != GameState.EndlessRunner) return; 

        currentScore += pointsAwarded;

        if (currentScore >= bossScoreThreshold)
        {
            StartCoroutine(BossCutsceneSequence());
        }
        else
        {
            SpawnNextChunk(); 
        }
    }

    // --- THE CUTSCENE DIRECTOR ---
    private IEnumerator BossCutsceneSequence()
    {
        // 1. Pull the brake on the endless procedural generation
        currentState = GameState.BossTransition;
        
        // 2. Hijack the Player's Physics
        Rigidbody playerRb = playerShip.GetComponent<Rigidbody>();
        if (playerRb != null) playerRb.isKinematic = true; 

        // 3. Completely shut off the player's manual controls to stop errors!
        if (playerShip.GetComponent<movement>() != null) 
            playerShip.GetComponent<movement>().enabled = false;
        
        // 4. Spawn the Boss Arena ahead in the distance
        float bossSpawnPosition = spawnXPosition + 60f; 
        GameObject arena = Instantiate(bossArenaPrefab, new Vector3(bossSpawnPosition, 0, 0), Quaternion.identity);

        // --- PART 1: THE DUAL CAMERA TETHER ---
        Unity.Cinemachine.CinemachineCamera cutsceneCam = null;
        var allCams = arena.GetComponentsInChildren<Unity.Cinemachine.CinemachineCamera>();
        
        foreach (var cam in allCams)
        {
            if (cam.gameObject.name == "Cutscene_Camera")
            {
                cam.Follow = playerShip; 
                cutsceneCam = cam;
            }
            if (cam.gameObject.name == "Combat_Camera")
            {
                cam.Follow = playerShip; 

                // --- ADD THIS EXACT LINE ---
                cam.LookAt = playerShip; 
                // ---------------------------
            }
        }
        // --------------------------------------

        // 5. Calculate our start and end points for the autopilot
        Vector3 startingPos = playerShip.position;
        Vector3 parkingPos = new Vector3(bossSpawnPosition - bossOrbitRadius, startingPos.y, 0); 

        // 6. The Animation Loop (Smoothly move the ship over time)
        float elapsedTime = 0f;
        while (elapsedTime < cutsceneDuration)
        {
            playerShip.position = Vector3.Lerp(startingPos, parkingPos, elapsedTime / cutsceneDuration);
            elapsedTime += Time.deltaTime;
            yield return null; 
        }

        // 7. Snap it perfectly to the end position
        playerShip.position = parkingPos;

        // --- PART 2: THE CAMERA SWITCH (THE CINEMATIC SWOOP) ---
        if (cutsceneCam != null)
        {
            // Drops the cutscene camera, triggering your custom 2-second Ease In Out blend
            cutsceneCam.Priority = 0; 
        }

        // We force the game to wait for EXACTLY 2 seconds while the camera physically
        // flies around the ship into the side-combat position before starting the fight!
        yield return new WaitForSeconds(2f);
        // -------------------------------------------------------

        // 8. Hand control over to the Boss Phase
        currentState = GameState.BossFight;

        // --- CHUNK CLEANUP ---
        // The old level safely deletes entirely off-camera!
        foreach (GameObject chunk in activeChunks)
        {
            if (chunk != null) Destroy(chunk);
        }
        activeChunks.Clear(); 
        // ----------------------------
        
        // 9. Wake up the Boss Movement script so the player can fly!
        if (playerShip.GetComponent<BossMovement>() != null)
            playerShip.GetComponent<BossMovement>().enabled = true;

        Debug.Log("Cutscene Complete! Camera swooped, chunks cleaned, Boss Mode Active.");
    }
}