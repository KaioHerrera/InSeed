using UnityEngine;
using System.Collections.Generic;

public class TerrainDecorator : MonoBehaviour
{
    [Header("Decoration Settings")]
    public Transform terrainCenter;
    public float decorationRadius = 25f;
    public int grassCount = 200;
    public int bushCount = 50;
    public int rockCount = 30;
    public int treeCount = 20;
    
    [Header("Prefabs")]
    public GameObject[] grassPrefabs;
    public GameObject[] bushPrefabs;
    public GameObject[] rockPrefabs;
    public GameObject[] treePrefabs;
    
    [Header("Spawn Settings")]
    public float minDistanceBetweenObjects = 2.5f; // Aumentado para evitar sobreposição
    public LayerMask terrainLayer = -1;
    public int maxSpawnAttempts = 100; // Aumentado para mais tentativas
    
    private List<GameObject> spawnedObjects = new List<GameObject>();
    
    void Start()
    {
        // Não decorar em runtime - as decorações devem ser criadas no editor
        // DecorateTerrain();
    }
    
    public void DecorateTerrain()
    {
        // Limpar decorações antigas
        ClearDecorations();
        
        // Spawnar gramas
        SpawnObjects(grassPrefabs, grassCount, "Grass");
        
        // Spawnar arbustos
        SpawnObjects(bushPrefabs, bushCount, "Bush");
        
        // Spawnar pedras
        SpawnObjects(rockPrefabs, rockCount, "Rock");
        
        // Spawnar árvores decorativas (não as que o jogador planta)
        SpawnObjects(treePrefabs, treeCount, "Tree");
    }
    
    void SpawnObjects(GameObject[] prefabs, int count, string objectType)
    {
        if (prefabs == null || prefabs.Length == 0)
        {
            Debug.LogWarning($"Nenhum prefab de {objectType} foi atribuído!");
            return;
        }
        
        int spawnedCount = 0;
        int attempts = 0;
        int maxTotalAttempts = count * maxSpawnAttempts; // Limite total de tentativas
        
        while (spawnedCount < count && attempts < maxTotalAttempts)
        {
            GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
            Vector3 position = GetRandomPositionOnTerrain();
            
            if (position != Vector3.zero && !IsPositionTooClose(position, objectType))
            {
                GameObject obj = Instantiate(prefab, position, Quaternion.Euler(0, Random.Range(0, 360), 0));
                obj.transform.SetParent(transform);
                spawnedObjects.Add(obj);
                
                // Ajustar escala aleatória para variedade
                float scale = Random.Range(0.8f, 1.2f);
                obj.transform.localScale = Vector3.one * scale;
                
                // Adicionar collider se não tiver (para árvores, pedras e arbustos)
                if (objectType != "Grass") // Gramas não precisam de collider
                {
                    Collider existingCollider = obj.GetComponent<Collider>();
                    if (existingCollider == null)
                    {
                        // Tentar encontrar collider em filhos
                        existingCollider = obj.GetComponentInChildren<Collider>();
                    }
                    
                    if (existingCollider == null)
                    {
                        // Adicionar BoxCollider baseado no renderer
                        Renderer renderer = obj.GetComponent<Renderer>();
                        if (renderer == null)
                        {
                            renderer = obj.GetComponentInChildren<Renderer>();
                        }
                        
                        if (renderer != null)
                        {
                            BoxCollider boxCollider = obj.AddComponent<BoxCollider>();
                            Bounds bounds = renderer.bounds;
                            boxCollider.center = obj.transform.InverseTransformPoint(bounds.center);
                            boxCollider.size = obj.transform.InverseTransformVector(bounds.size);
                        }
                        else
                        {
                            // Fallback: collider genérico
                            CapsuleCollider capsuleCollider = obj.AddComponent<CapsuleCollider>();
                            capsuleCollider.radius = 0.5f;
                            capsuleCollider.height = 2f;
                        }
                    }
                }
                
                spawnedCount++;
            }
            
            attempts++;
        }
        
        if (spawnedCount < count)
        {
            Debug.LogWarning($"Apenas {spawnedCount} de {count} {objectType} foram spawnados (limite de tentativas atingido)");
        }
    }
    
    Vector3 GetRandomPositionOnTerrain()
    {
        int attempts = 0;
        int maxAttempts = maxSpawnAttempts;
        
        while (attempts < maxAttempts)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float distance = Random.Range(0f, decorationRadius);
            
            float x = terrainCenter.position.x + Mathf.Cos(angle) * distance;
            float z = terrainCenter.position.z + Mathf.Sin(angle) * distance;
            
            // Raycast para encontrar o chão
            RaycastHit hit;
            Vector3 rayStart = new Vector3(x, terrainCenter.position.y + 10f, z);
            
            if (Physics.Raycast(rayStart, Vector3.down, out hit, 20f, terrainLayer))
            {
                return hit.point;
            }
            
            attempts++;
        }
        
        return Vector3.zero;
    }
    
    bool IsPositionTooClose(Vector3 position, string objectType)
    {
        // Distância mínima baseada no tipo de objeto
        float minDistance = minDistanceBetweenObjects;
        
        // Árvores precisam de mais espaço
        if (objectType == "Tree")
        {
            minDistance = minDistanceBetweenObjects * 2f; // Dobro da distância para árvores
        }
        // Arbustos e pedras precisam de espaço médio
        else if (objectType == "Bush" || objectType == "Rock")
        {
            minDistance = minDistanceBetweenObjects * 1.5f;
        }
        
        // Verificar distância de todos os objetos já spawnados
        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null)
            {
                float distance = Vector3.Distance(position, obj.transform.position);
                
                // Verificar também usando OverlapSphere para objetos com collider
                Collider objCollider = obj.GetComponent<Collider>();
                if (objCollider != null)
                {
                    // Se o objeto tem collider, verificar se há sobreposição
                    Vector3 direction = (position - obj.transform.position).normalized;
                    float overlapDistance = distance - objCollider.bounds.extents.magnitude;
                    
                    if (overlapDistance < minDistance)
                    {
                        return true;
                    }
                }
                else if (distance < minDistance)
                {
                    return true;
                }
            }
        }
        
        return false;
    }
    
    public void ClearDecorations()
    {
        // Limpar objetos filhos
        while (transform.childCount > 0)
        {
#if UNITY_EDITOR
            DestroyImmediate(transform.GetChild(0).gameObject);
#else
            Destroy(transform.GetChild(0).gameObject);
#endif
        }
        
        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null)
            {
#if UNITY_EDITOR
                DestroyImmediate(obj);
#else
                Destroy(obj);
#endif
            }
        }
        spawnedObjects.Clear();
    }
    
    // Método para usar no editor
    public void DecorateTerrainEditor()
    {
        if (terrainCenter == null)
        {
            terrainCenter = transform;
        }
        
        DecorateTerrain();
    }
    
    void OnDestroy()
    {
        // Não limpar em runtime se as decorações foram criadas no editor
        // ClearDecorations();
    }
}
