/*ing UnityEngine;
using System.Collections.Generic; // Required to use Lists

// 1. Your Data Container (This is NOT a MonoBehaviour)
[System.Serializable]
public class DifficultyTier
{
    public string tierName;
    public int scoreThreshold;
    public GameObject[] chunkPrefabs;
}

// 2. The Spawner Brain (This IS a MonoBehaviour)
public class LevelSpawner : MonoBehaviour
{
    [Header("Difficulty Settings")]
    // This creates the list of tiers in your Unity Inspector
    public List<DifficultyTier> difficultyTiers; 
    
    // You will eventually add your spawn logic (the trigger checks) here!
}*/

/*using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class DifficultyTier
{
    public string tierName;
    public int scoreThreshold;
    public GameObject[] chunkPrefabs;
}

public class LevelSpawner : MonoBehaviour
{
    public static LevelSpawner Instance; // Singleton so the triggers can find this script easily

    [Header("Difficulty Tiers")]
    public List<DifficultyTier> difficultyTiers;

    [Header("Spawning Metrics")]
    public float chunkLength = 20f; // Must match the exact physical X-length of your prefab!
    private float spawnXPosition = 0f; // Tracks where the next chunk should snap to

    [Header("Game State")]
    public int currentScore = 0;
    
    // We keep track of active chunks so we can delete the old ones
    private List<GameObject> activeChunks = new List<GameObject>(); 

    void Awake()
    {
        // Set up the Singleton
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        // Spawn the first three chunks immediately so the player has a starting runway
        SpawnNextChunk();
        SpawnNextChunk();
        SpawnNextChunk();
    }

    public void SpawnNextChunk()
    {
        // 1. Figure out which difficulty tier we are currently in based on the score
        DifficultyTier currentTier = difficultyTiers[0]; 
        foreach (DifficultyTier tier in difficultyTiers)
        {
            if (currentScore >= tier.scoreThreshold)
            {
                currentTier = tier; // Upgrade to harder tier if score is high enough
            }
        }

        // 2. Randomly pick a chunk prefab from that specific tier
        int randomIndex = Random.Range(0, currentTier.chunkPrefabs.Length);
        GameObject chunkToSpawn = currentTier.chunkPrefabs[randomIndex];

        // 3. Spawn the chunk at the exact spawnXPosition, keeping Y and Z at 0
        GameObject newChunk = Instantiate(chunkToSpawn, new Vector3(spawnXPosition, 0, 0), Quaternion.identity);
        activeChunks.Add(newChunk);

        // 4. Move the spawn point forward by exactly one chunk length for the next time!
        spawnXPosition += chunkLength;

        // 5. Memory Management: If we have more than 4 chunks behind us, delete the oldest one
        if (activeChunks.Count > 4)
        {
            Destroy(activeChunks[0]); // Destroy the object in the scene
            activeChunks.RemoveAt(0); // Remove it from our tracking list
        }
    }

    // A public method to be called when the player hits the trigger
    public void AddScoreAndSpawn(int pointsAwarded)
    {
        currentScore += pointsAwarded;
        Debug.Log("Score Updated: " + currentScore);
        SpawnNextChunk();
    }
}*/

using UnityEngine;
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

    [Header("Boss Transition (The Fly-In)")]
    public GameObject bossArenaPrefab; // The cylindrical arena
    public float emptySpaceGap = 80f;  // How far the player has to fly through dead space

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

        if (activeChunks.Count > 4)
        {
            Destroy(activeChunks[0]); 
            activeChunks.RemoveAt(0); 
        }
    }

    public void AddScoreAndSpawn(int pointsAwarded)
    {
        if (currentState != GameState.EndlessRunner) return; 

        currentScore += pointsAwarded;

        if (currentScore >= bossScoreThreshold)
        {
            StartBossTransition();
        }
        else
        {
            SpawnNextChunk(); 
        }
    }

    // --- THE NEW FLY-IN LOGIC ---
    private void StartBossTransition()
    {
        currentState = GameState.BossTransition;
        Debug.Log("SCORE THRESHOLD REACHED! The tunnel ends. Spawning Boss Arena in the distance...");
        
        // 1. Calculate a spawn point far in the distance, leaving a massive gap
        float bossSpawnPosition = spawnXPosition + emptySpaceGap;

        // 2. Spawn the giant cylindrical boss arena waiting at the end of the void
        Instantiate(bossArenaPrefab, new Vector3(bossSpawnPosition, 0, 0), Quaternion.identity);
    }
}