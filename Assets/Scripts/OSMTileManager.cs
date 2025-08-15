using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;

public class OSMTileManager : MonoBehaviour
{
    [Header("Configuration Carte")]
    public int zoomLevel = 12;
    public double centerLat = 48.8566; // Paris latitude
    public double centerLon = 2.3522;  // Paris longitude
    public int tileRadius = 3; // Réduire le nombre de tuiles pour débugger

    [Header("Paramètres Visuels")]
    public Material tileMaterial;
    public float tileSize = 10f; // Taille d'une tuile dans Unity (doit correspondre aux dimensions du Plane)

    private Dictionary<string, GameObject> loadedTiles = new Dictionary<string, GameObject>();
    private Queue<string> tileLoadQueue = new Queue<string>();
    private bool isLoadingTile = false;

    // Cache des textures pour éviter les rechargements
    private Dictionary<string, Texture2D> textureCache = new Dictionary<string, Texture2D>();

    [ContextMenu("Générer les tuiles")]
    public void GenerateTilesManually()
    {
        // Supprimer tous les enfants du manager
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        // Réinitialiser le dictionnaire et la queue
        loadedTiles.Clear();
        tileLoadQueue.Clear();
        isLoadingTile = false;

        // Générer les nouvelles tuiles
        LoadTilesAroundCenter();
        Debug.Log("Tuiles générées manuellement !");
    }

    public void LoadTilesAroundCenter()
    {
        Debug.Log("Generation des tuiles manuellement !");

        // Convertir les coordonnées GPS en coordonnées de tuile
        var centerTile = LatLonToTile(centerLat, centerLon, zoomLevel);

        // Charger les tuiles dans un rayon autour du centre
        for (int x = centerTile.x - tileRadius; x <= centerTile.x + tileRadius; x++)
        {
            for (int y = centerTile.y - tileRadius; y <= centerTile.y + tileRadius; y++)
            {
                string tileKey = $"{zoomLevel}_{x}_{y}";
                if (!loadedTiles.ContainsKey(tileKey))
                {
                    tileLoadQueue.Enqueue(tileKey);
                }
            }
        }

        StartCoroutine(ProcessTileQueue());
    }

    IEnumerator ProcessTileQueue()
    {
        while (tileLoadQueue.Count > 0)
        {
            if (!isLoadingTile)
            {
                string tileKey = tileLoadQueue.Dequeue();
                StartCoroutine(LoadTile(tileKey));
            }
            yield return null;
        }
    }

    IEnumerator LoadTile(string tileKey)
    {
        isLoadingTile = true;

        string[] parts = tileKey.Split('_');
        int z = int.Parse(parts[0]);
        int x = int.Parse(parts[1]);
        int y = int.Parse(parts[2]);

        // URL OpenStreetMap
        string url = $"https://tile.openstreetmap.org/{z}/{x}/{y}.png";

        // Vérifier le cache d'abord
        if (textureCache.ContainsKey(tileKey))
        {
            CreateTileGameObject(tileKey, x, y, textureCache[tileKey]);
            isLoadingTile = false;
            yield break;
        }

        // Télécharger la tuile
        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
        {
            // Ajouter un User-Agent pour respecter la politique OSM
            www.SetRequestHeader("User-Agent", "Unity-OSM-App/1.0");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(www);
                textureCache[tileKey] = texture;
                CreateTileGameObject(tileKey, x, y, texture);
            }
            else
            {
                Debug.LogError($"Erreur de téléchargement pour la tuile {tileKey}: {www.error}");
            }
        }

        isLoadingTile = false;

        // Petite pause pour éviter de surcharger les serveurs OSM
        yield return new WaitForSeconds(0.1f);
    }

    void CreateTileGameObject(string tileKey, int tileX, int tileY, Texture2D texture)
    {
        // Créer un GameObject pour la tuile
        GameObject tileObj = GameObject.CreatePrimitive(PrimitiveType.Plane);
        tileObj.name = $"Tile_{tileKey}";
        tileObj.transform.parent = this.transform;

        // Calculer la tuile du centre une seule fois pour référence
        var centerTile = LatLonToTile(centerLat, centerLon, zoomLevel);

        Vector3 position = new Vector3(
            -(tileX - centerTile.x) * tileSize,  // Inverse X pour l'ordre correct
            0,
            (tileY - centerTile.y) * tileSize   //  Y pour l'ordre correct
        );
        tileObj.transform.localPosition = position;

        
        tileObj.transform.localScale = new Vector3(tileSize * 0.1f, 1, tileSize * 0.1f);

        Renderer renderer = tileObj.GetComponent<Renderer>();
        if (tileMaterial != null)
        {
            renderer.sharedMaterial = new Material(tileMaterial);
        }
        renderer.sharedMaterial.mainTexture = texture;

        DestroyImmediate(tileObj.GetComponent<MeshCollider>());

        loadedTiles[tileKey] = tileObj;
    }


    // Convertir latitude/longitude en coordonnées de tuile
    public static Vector2Int LatLonToTile(double lat, double lon, int zoom)
    {
        double latRad = lat * Math.PI / 180.0;
        double n = Math.Pow(2.0, zoom);

        int x = (int)Math.Floor((lon + 180.0) / 360.0 * n);
        int y = (int)Math.Floor((1.0 - Math.Asinh(Math.Tan(latRad)) / Math.PI) / 2.0 * n);

        return new Vector2Int(x, y);
    }

    // Convertir coordonnées de tuile en latitude/longitude
    public static Vector2 TileToLatLon(int x, int y, int zoom)
    {
        double n = Math.Pow(2.0, zoom);
        double lon = x / n * 360.0 - 180.0;
        double latRad = Math.Atan(Math.Sinh(Math.PI * (1 - 2 * y / n)));
        double lat = latRad * 180.0 / Math.PI;

        return new Vector2((float)lat, (float)lon);
    }

    // Nettoyer les tuiles trop éloignées (optionnel)
    public void CleanupDistantTiles(Vector3 playerPosition, float maxDistance)
    {
        List<string> tilesToRemove = new List<string>();

        foreach (var kvp in loadedTiles)
        {
            if (Vector3.Distance(kvp.Value.transform.position, playerPosition) > maxDistance)
            {
                tilesToRemove.Add(kvp.Key);
            }
        }

        foreach (string tileKey in tilesToRemove)
        {
            if (loadedTiles.ContainsKey(tileKey))
            {
                DestroyImmediate(loadedTiles[tileKey]);
                loadedTiles.Remove(tileKey);
            }
        }
    }
}