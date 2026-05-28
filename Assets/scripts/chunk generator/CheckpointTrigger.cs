using UnityEngine;

public class CheckpointTrigger : MonoBehaviour
{
    [Header("Settings")]
    public int pointsToAward = 10; // How many points this specific chunk gives

    // A safety lock to prevent Unity's physics engine from double-firing
    private bool hasTriggered = false; 

    // Using 3D trigger detection because you are using 3D physics constraints
    private void OnTriggerEnter(Collider other)
    {
        // 1. Check if the object passing through is the Player, and if we haven't fired yet
        if (other.CompareTag("Player") && !hasTriggered)
        {
            // 2. Lock the trigger immediately
            hasTriggered = true; 

            // 3. Talk to the Singleton Spawner and tell it to run its massive code block
            if (LevelSpawner.Instance != null)
            {
                LevelSpawner.Instance.AddScoreAndSpawn(pointsToAward);
            }
            else
            {
                Debug.LogWarning("LevelSpawner Instance not found in the scene!");
            }
        }
    }
}