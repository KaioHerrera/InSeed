using UnityEngine;

/// <summary>
/// Cria uma barreira invisível nas bordas do mapa para evitar que o jogador saia
/// </summary>
public class MapBoundary : MonoBehaviour
{
    [Header("Boundary Settings")]
    public float mapSize = 24f; // Tamanho do mapa (deve corresponder ao tamanho do terreno)
    public float boundaryHeight = 5f;
    public float boundaryThickness = 0.5f;
    public Material boundaryMaterial; // Opcional: material para visualizar as bordas
    
    void Start()
    {
        CreateBoundaries();
    }
    
    void CreateBoundaries()
    {
        float halfSize = mapSize / 2f;
        
        // Criar 4 paredes nas bordas do mapa
        CreateWall("NorthWall", new Vector3(0, boundaryHeight / 2f, halfSize), new Vector3(mapSize, boundaryHeight, boundaryThickness));
        CreateWall("SouthWall", new Vector3(0, boundaryHeight / 2f, -halfSize), new Vector3(mapSize, boundaryHeight, boundaryThickness));
        CreateWall("EastWall", new Vector3(halfSize, boundaryHeight / 2f, 0), new Vector3(boundaryThickness, boundaryHeight, mapSize));
        CreateWall("WestWall", new Vector3(-halfSize, boundaryHeight / 2f, 0), new Vector3(boundaryThickness, boundaryHeight, mapSize));
    }
    
    void CreateWall(string name, Vector3 position, Vector3 scale)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.position = position;
        wall.transform.localScale = scale;
        wall.transform.SetParent(transform);
        
        // Tornar invisível mas manter collider
        Renderer renderer = wall.GetComponent<Renderer>();
        if (renderer != null)
        {
            if (boundaryMaterial != null)
            {
                renderer.material = boundaryMaterial;
                // Tornar semi-transparente
                Color color = boundaryMaterial.color;
                color.a = 0.3f;
                renderer.material.color = color;
            }
            else
            {
                // Criar material transparente
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = new Color(1f, 1f, 1f, 0.1f); // Quase invisível
                mat.SetFloat("_Mode", 3); // Transparent mode
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
                renderer.material = mat;
            }
        }
        
        // Adicionar tag para identificar como boundary
        wall.tag = "Untagged";
        
        // Garantir que o collider está ativo
        Collider col = wall.GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = false; // Collider sólido para bloquear o jogador
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Visualizar as bordas no editor
        float halfSize = mapSize / 2f;
        Gizmos.color = Color.red;
        
        // Desenhar linhas nas bordas
        Gizmos.DrawLine(new Vector3(-halfSize, 0, -halfSize), new Vector3(halfSize, 0, -halfSize));
        Gizmos.DrawLine(new Vector3(halfSize, 0, -halfSize), new Vector3(halfSize, 0, halfSize));
        Gizmos.DrawLine(new Vector3(halfSize, 0, halfSize), new Vector3(-halfSize, 0, halfSize));
        Gizmos.DrawLine(new Vector3(-halfSize, 0, halfSize), new Vector3(-halfSize, 0, -halfSize));
    }
}
