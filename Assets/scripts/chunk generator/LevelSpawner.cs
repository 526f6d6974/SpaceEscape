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