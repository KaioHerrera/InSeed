using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    [Header("Game Settings")]
    public GameObject seedPrefab;
    public int seedsPerRound = 15; // Aumentado para facilitar a coleta
    public int seedsNeededToPlant = 10;
    public int totalTreesToPlant = 4;
    
    [Header("Spawn Settings")]
    public Transform terrainCenter;
    public float spawnRadius = 50f; // Raio máximo de spawn a partir do centro do terreno
    public float minSpawnDistance = 2f; // Distância mínima entre sementes e do jogador
    
    private int collectedSeeds = 0;
    private int plantedTrees = 0;
    private List<GameObject> currentSeeds = new List<GameObject>();
    private GameObject player;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            player = FindFirstObjectByType<FirstPersonController>()?.gameObject;
        }
        
        if (terrainCenter == null)
        {
            terrainCenter = transform;
        }
        
        // Spawnar primeira rodada de sementes
        SpawnSeeds();
    }
    
    public void CollectSeed()
    {
        collectedSeeds++;
    }
    
    public bool CanPlant()
    {
        return collectedSeeds >= seedsNeededToPlant && plantedTrees < totalTreesToPlant;
    }
    
    public void PlantTree()
    {
        if (CanPlant())
        {
            collectedSeeds -= seedsNeededToPlant;
            plantedTrees++;
            
            if (plantedTrees >= totalTreesToPlant)
            {
                ShowWinScreen();
                return;
            }
            
            if (plantedTrees < totalTreesToPlant)
            {
                SpawnSeeds();
            }
        }
    }
    
    void SpawnSeeds()
    {
        foreach (GameObject seed in currentSeeds)
        {
            if (seed != null)
            {
                Destroy(seed);
            }
        }
        currentSeeds.Clear();
        
        collectedSeeds = 0;
        
        for (int i = 0; i < seedsPerRound; i++)
        {
            Vector3 spawnPosition = GetRandomSpawnPosition();
            if (seedPrefab != null)
            {
                GameObject seed = Instantiate(seedPrefab, spawnPosition, Quaternion.identity);
                currentSeeds.Add(seed);
            }
        }
    }
    
    Vector3 GetRandomSpawnPosition()
    {
        Vector3 position;
        int attempts = 0;
        int maxAttempts = 50;
        
        do
        {
            // Posição aleatória em um círculo a partir do centro do terreno
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float distance = Random.Range(minSpawnDistance, spawnRadius);
            
            // Calcular posição relativa ao centro do terreno
            float x = terrainCenter.position.x + Mathf.Cos(angle) * distance;
            float z = terrainCenter.position.z + Mathf.Sin(angle) * distance;
            
            // Raycast para encontrar o chão
            RaycastHit hit;
            float y = terrainCenter.position.y;
            
            Vector3 rayStart = new Vector3(x, terrainCenter.position.y + 10f, z);
            if (Physics.Raycast(rayStart, Vector3.down, out hit, 20f))
            {
                y = hit.point.y + 0.5f; // Levantar um pouco do chão
            }
            else
            {
                y = 1f; // Fallback
            }
            
            position = new Vector3(x, y, z);
            attempts++;
        }
        while (attempts < maxAttempts && IsPositionTooClose(position));
        
        return position;
    }
    
    bool IsPositionTooClose(Vector3 position)
    {
        // Verificar se está muito perto de outras sementes
        foreach (GameObject seed in currentSeeds)
        {
            if (seed != null && Vector3.Distance(position, seed.transform.position) < minSpawnDistance)
            {
                return true;
            }
        }
        
        // Verificar se está muito perto do jogador
        if (player != null && Vector3.Distance(position, player.transform.position) < minSpawnDistance)
        {
            return true;
        }
        
        return false;
    }
    
    public Vector3 GetPlayerPosition()
    {
        if (player != null)
        {
            return player.transform.position;
        }
        return Vector3.zero;
    }
    
    public int GetCollectedSeeds()
    {
        return collectedSeeds;
    }
    
    public int GetPlantedTrees()
    {
        return plantedTrees;
    }
    
    void ShowWinScreen()
    {
        WinScreen winScreen = FindFirstObjectByType<WinScreen>();
        if (winScreen != null)
        {
            winScreen.ShowWinScreen();
        }
    }
}
