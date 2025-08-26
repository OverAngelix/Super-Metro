using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class OSMTileManager : MonoBehaviour
{
    public static OSMTileManager Instance { get; private set; }

    [Header("Map configuration")]
    public int zoomLevel = 12;
    public double centerLat = 48.8566;
    public double centerLon = 2.3522;
    public int tileRadius = 3;
    public int tileSize = 10;
    public Material tileMaterial;

    private Dictionary<string, GameObject> loadedTiles = new();
    private Queue<string> tileLoadQueue = new();
    private bool isLoadingTile = false;

    // Cache des textures pour eviter les rechargements
    private Dictionary<string, Texture2D> textureCache = new();

    public GameObject spotPrefab;
    public GameObject stationPrefab;

    private ConnectStations connect;

    public GameObject trainPrefab;

    void Awake()
    {
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            connect = GetComponent<ConnectStations>();
            if (connect != null)
            {
                connect.Init();
            }
            SuperGlobal.SetStations();
            InitGame();
        }
    }

    private void InitGame()
    {
        InitLocation();
        InitStations();
    }

    // Instancier toutes les stations et les stocker
    private void InitStations()
    {
        foreach (TrainLine line in SuperGlobal.trainLines)
        {
            foreach (Station station in line.stations)
            {
                var sta = station;
                GameObject obj = PlacePoint(sta.lat, sta.lon, stationPrefab);
                station.controller = obj.GetComponent<StationController>();
                obj.name = sta.name;
                sta.obj = obj;
                StationController controller = obj.GetComponent<StationController>();
                controller.station = sta;
            }

            // Relier les stations
            connect.CreateLines(line.stations, line.lineColor);
        }

    }

    // Instancier toutes les locations
    private void InitLocation()
    {
        for (int i = 0; i < SuperGlobal.spots.Count; i++)
        {
            var spot = SuperGlobal.spots[i];
            GameObject obj = PlacePoint(spot.lat, spot.lon, spotPrefab);
            spot.obj = obj;
        }
    }

    [ContextMenu("Generer les tuiles")]
    public void GenerateTilesManually()
    {
        // Supprimer tous les enfants du manager
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        // Reinitialiser le dictionnaire et la queue
        loadedTiles.Clear();
        tileLoadQueue.Clear();
        isLoadingTile = false;

        // Generer les nouvelles tuiles
        LoadTilesAroundCenter();
        SuperGlobal.Log("Tuiles generees manuellement !");
    }

    public void LoadTilesAroundCenter()
    {

        // Convertir les coordonnees GPS en coordonnees de tuile
        var centerTile = GeoUtils.LatLonToTile(centerLat, centerLon, zoomLevel);

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
                StartCoroutine(LoadTileAndRoads(tileKey));
            }
            yield return null;
        }
    }

    // Nouvelle coroutine qui gère à la fois le chargement de la tuile et de ses routes
    IEnumerator LoadTileAndRoads(string tileKey)
    {
        isLoadingTile = true;

        string[] parts = tileKey.Split('_');
        int z = int.Parse(parts[0]);
        int x = int.Parse(parts[1]);
        int y = int.Parse(parts[2]);

        // URL OpenStreetMap
        string tileUrl = $"https://tile.openstreetmap.org/{z}/{x}/{y}.png";

        // Vérifier le cache d'abord
        if (textureCache.ContainsKey(tileKey))
        {
            CreateTileGameObject(tileKey, x, y, textureCache[tileKey]);
        }
        else
        {
            // Télécharger la tuile
            using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(tileUrl))
            {
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
        }

        // Une fois la tuile chargée, on lance la coroutine pour ses routes
        yield return StartCoroutine(CreateRoadsMeshForTile(x, y));
        yield return StartCoroutine(CreateHousesForTile(x, y));


        isLoadingTile = false;

        // Petite pause pour éviter de surcharger les serveurs OSM
        yield return new WaitForSeconds(0.1f);
    }

    void CreateTileGameObject(string tileKey, int tileX, int tileY, Texture2D texture)
    {
        // Creer un GameObject pour la tuile
        GameObject tileObj = GameObject.CreatePrimitive(PrimitiveType.Plane);
        tileObj.name = $"Tile_{tileKey}";
        tileObj.transform.parent = this.transform;

        // Utiliser les coordonnees precises de la tuile pour la positionner
        double lat = GeoUtils.TileToLat(tileY + 0.5, zoomLevel); // Ajouter 0.5 pour le centre de la tuile
        double lon = GeoUtils.TileToLon(tileX + 0.5, zoomLevel); // Ajouter 0.5 pour le centre de la tuile

        // Utiliser la meme methode que pour les points pour garantir l'alignement
        Vector3 position = GeoUtils.LatLonToUnityPosition(centerLat, centerLon, lat, lon, zoomLevel, tileSize);

        tileObj.transform.localPosition = position;


        tileObj.transform.localScale = new Vector3(tileSize * 0.1f, 1, tileSize * 0.1f);

        Renderer renderer = tileObj.GetComponent<Renderer>();
        if (tileMaterial != null)
        {
            renderer.sharedMaterial = new Material(tileMaterial);
        }
        //renderer.sharedMaterial.mainTexture = texture;

        loadedTiles[tileKey] = tileObj;
    }

    // Place this inside your OSMTileManager class
    public GameObject PlacePoint(double lat, double lon, GameObject typePoint)
    {
        Vector3 pointPosition = GeoUtils.LatLonToUnityPosition(centerLat, centerLon, lat, lon, zoomLevel, tileSize);
        GameObject point = Instantiate(typePoint, this.transform);
        if (typePoint == stationPrefab)
        {
            StationController stationController = stationPrefab.GetComponent<StationController>();
            stationController.trainPrefab = trainPrefab;
        }
        point.transform.localPosition = pointPosition;
        return point;
    }

    public void AddStationOnMap(TrainLine trainLine, Station station)
    {
        GameObject obj = PlacePoint(station.lat, station.lon, stationPrefab);
        obj.name = station.name;
        station.obj = obj;

        StationController controller = obj.GetComponent<StationController>();
        controller.station = station;
        station.controller = controller;
        foreach (var train in trainLine.trains)
        {
            controller.UpdateTrainPath(train.gameObject, trainLine);
        }

        connect.CreateLines(trainLine.stations, trainLine.lineColor);
    }


    [Header("Parametres de generation")]
    public Material roadMaterial;
    public Material houseMaterial;

    private const string ROADS_JSON_PATH_TEMPLATE = "Assets/Resources/JSON/Roads/roads-{0}-{1}.json";
    private const string HOUSES_JSON_PATH_TEMPLATE = "Assets/Resources/JSON/Houses/houses-{0}-{1}.json";

    [Serializable]
    public class OverpassResponse
    {
        public Element[] elements;
    }
    [Serializable]
    public class Element
    {
        public long id;
        public string type;
        public double lat;
        public double lon;
        public List<long> nodes;
        public Dictionary<string, string> tags;
    }

    #region FONCTIONNALITE POUR LES MAISONS
    IEnumerator CreateHousesForTile(int tileX, int tileY)
    {
        string jsonResponse = null;
        string filePath = string.Format(HOUSES_JSON_PATH_TEMPLATE, tileX, tileY);

        // Tenter de charger les données depuis un fichier JSON local
        if (File.Exists(filePath))
        {
            jsonResponse = File.ReadAllText(filePath);
            Debug.Log($"Données de bâtiments chargées depuis le fichier JSON : {filePath}");
        }
        else
        {
            // Le fichier n'existe pas, on lance une requête Overpass
            Debug.Log("Fichier JSON de bâtiments non trouvé. Lancement d'une requête Overpass...");

            // Déterminer la zone de la tuile
            double minLat = GeoUtils.TileToLat(tileY + 1, zoomLevel);
            double maxLat = GeoUtils.TileToLat(tileY, zoomLevel);
            double minLon = GeoUtils.TileToLon(tileX, zoomLevel);
            double maxLon = GeoUtils.TileToLon(tileX + 1, zoomLevel);

            string bbox = $"{minLat.ToString(CultureInfo.InvariantCulture)},{minLon.ToString(CultureInfo.InvariantCulture)},{maxLat.ToString(CultureInfo.InvariantCulture)},{maxLon.ToString(CultureInfo.InvariantCulture)}";
            string query = $"[out:json];way[building]({bbox});(._;>;);out;";
            string url = "http://overpass-api.de/api/interpreter";
            WWWForm form = new WWWForm();
            form.AddField("data", query);

            Debug.Log($"Requête Overpass pour les bâtiments de la tuile ({tileX},{tileY}) : " + query);

            using (UnityWebRequest www = UnityWebRequest.Post(url, form))
            {
                www.SetRequestHeader("User-Agent", "Unity-OSM-App/1.0");
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    jsonResponse = www.downloadHandler.text;
                    Debug.Log("Requête Overpass réussie. Sauvegarde du fichier...");
                    // Sauvegarder le JSON dans le dossier Resources/JSON
                    SaveJsonToFile(jsonResponse, HOUSES_JSON_PATH_TEMPLATE);
                }
                else
                {
                    Debug.LogError($"Erreur de requête Overpass pour la tuile ({tileX},{tileY}) : {www.error}");
                    yield break;
                }
            }
        }

        // Etape 2: Dessiner les bâtiments avec les données chargées ou récupérées
        OverpassResponse response = JsonUtility.FromJson<OverpassResponse>(jsonResponse);

        Dictionary<long, Element> nodesDict = new Dictionary<long, Element>();
        foreach (var element in response.elements)
        {
            if (element.type == "node")
            {
                nodesDict[element.id] = element;
            }
        }

        string tileKey = $"{zoomLevel}_{tileX}_{tileY}";
        if (!loadedTiles.ContainsKey(tileKey))
        {
            Debug.LogError($"Tuile {tileKey} non trouvée. Impossible de créer le maillage des bâtiments.");
            yield break;
        }
        GameObject tileObj = loadedTiles[tileKey];

        GameObject meshObj = new GameObject($"Houses_Mesh_{tileX}_{tileY}");
        meshObj.transform.parent = tileObj.transform;
        meshObj.transform.localPosition = new Vector3(0, 0.2f, 0);

        MeshFilter mf = meshObj.AddComponent<MeshFilter>();
        MeshRenderer mr = meshObj.AddComponent<MeshRenderer>();
        mr.material = houseMaterial;

        Mesh mesh = new Mesh();

        List<Vector3> vertices = new List<Vector3>();
        List<int> indices = new List<int>();

        foreach (var element in response.elements)
        {
            if (element.type != "way" || element.nodes == null) continue;

            List<Vector3> baseVerts = new List<Vector3>();
            foreach (long nodeId in element.nodes)
            {
                if (!nodesDict.TryGetValue(nodeId, out var node)) continue;
                Vector3 pos = GeoUtils.LatLonToUnityPosition(centerLat, centerLon, node.lat, node.lon, zoomLevel, tileSize)
                    - tileObj.transform.localPosition;
                baseVerts.Add(pos);
            }

            if (baseVerts.Count < 3) continue;

            int startIndex = vertices.Count;
            vertices.AddRange(baseVerts);

            // Créer des lignes pour chaque arête du polygone
            for (int i = 0; i < baseVerts.Count; i++)
            {
                int next = (i + 1) % baseVerts.Count;
                indices.Add(startIndex + i);
                indices.Add(startIndex + next);
            }
        }

        // Assigner les données au mesh
        mesh.SetVertices(vertices);
        mesh.SetIndices(indices.ToArray(), MeshTopology.Lines, 0);

        mf.mesh = mesh;

    }
    #endregion

    #region FONCTIONNALITE POUR LES ROUTES
    IEnumerator CreateRoadsMeshForTile(int tileX, int tileY)
    {
        string jsonResponse = null;
        string filePath = string.Format(ROADS_JSON_PATH_TEMPLATE, tileX, tileY);

        // Tenter de charger les données depuis un fichier JSON local
        if (File.Exists(filePath))
        {
            jsonResponse = File.ReadAllText(filePath);
            Debug.Log($"Données de routes chargées depuis le fichier JSON : {filePath}");
        }
        else
        {
            // Le fichier n'existe pas, on lance une requête Overpass
            Debug.Log("Fichier JSON de routes non trouvé. Lancement d'une requête Overpass...");

            // Déterminer la zone de la tuile
            double minLat = GeoUtils.TileToLat(tileY + 1, zoomLevel);
            double maxLat = GeoUtils.TileToLat(tileY, zoomLevel);
            double minLon = GeoUtils.TileToLon(tileX, zoomLevel);
            double maxLon = GeoUtils.TileToLon(tileX + 1, zoomLevel);

            string bbox = $"{minLat.ToString(CultureInfo.InvariantCulture)},{minLon.ToString(CultureInfo.InvariantCulture)},{maxLat.ToString(CultureInfo.InvariantCulture)},{maxLon.ToString(CultureInfo.InvariantCulture)}";
            string query = $"[out:json];way[highway]({bbox});(._;>;);out;";
            string url = "http://overpass-api.de/api/interpreter";
            WWWForm form = new WWWForm();
            form.AddField("data", query);

            Debug.Log($"Requête Overpass pour la tuile ({tileX},{tileY}) : " + query);

            using (UnityWebRequest www = UnityWebRequest.Post(url, form))
            {
                www.SetRequestHeader("User-Agent", "Unity-OSM-App/1.0");
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    jsonResponse = www.downloadHandler.text;
                    Debug.Log("Requête Overpass réussie. Sauvegarde du fichier...");
                    // Sauvegarder le JSON dans le dossier Resources/JSON
                    SaveJsonToFile(jsonResponse, ROADS_JSON_PATH_TEMPLATE);
                }
                else
                {
                    Debug.LogError($"Erreur de requête Overpass pour la tuile ({tileX},{tileY}) : {www.error}");
                    yield break;
                }
            }
        }

        // Etape 2: Dessiner les routes avec les données chargées ou récupérées
        OverpassResponse response = JsonUtility.FromJson<OverpassResponse>(jsonResponse);

        Dictionary<long, Element> nodesDict = new Dictionary<long, Element>();
        foreach (var element in response.elements)
        {
            if (element.type == "node")
            {
                nodesDict[element.id] = element;
            }
        }

        // Le maillage sera un enfant de la tuile
        string tileKey = $"{zoomLevel}_{tileX}_{tileY}";
        if (!loadedTiles.ContainsKey(tileKey))
        {
            Debug.LogError($"Tile {tileKey} not found. Cannot create road mesh.");
            yield break;
        }
        GameObject tileObj = loadedTiles[tileKey];

        GameObject roadsMeshObject = new GameObject($"Roads_Mesh_{tileX}_{tileY}");
        roadsMeshObject.transform.parent = tileObj.transform;
        roadsMeshObject.transform.localPosition = Vector3.zero;

        MeshFilter meshFilter = roadsMeshObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = roadsMeshObject.AddComponent<MeshRenderer>();

        meshRenderer.material = roadMaterial;

        List<Vector3> allVertices = new List<Vector3>();
        List<int> allTriangles = new List<int>();

        // Largeur des routes, réduite pour un meilleur visuel
        float roadWidth = 0.05f;
        int vertexIndexOffset = 0;

        foreach (var element in response.elements)
        {
            if (element.type == "way" && element.nodes != null && element.nodes.Count >= 2)
            {
                for (int i = 0; i < element.nodes.Count - 1; i++)
                {
                    long nodeId1 = element.nodes[i];
                    long nodeId2 = element.nodes[i + 1];

                    if (nodesDict.ContainsKey(nodeId1) && nodesDict.ContainsKey(nodeId2))
                    {
                        Element node1 = nodesDict[nodeId1];
                        Element node2 = nodesDict[nodeId2];

                        // Utiliser des positions locales par rapport à la tuile parente
                        Vector3 pos1 = GeoUtils.LatLonToUnityPosition(centerLat, centerLon, node1.lat, node1.lon, zoomLevel, tileSize) - tileObj.transform.localPosition;
                        Vector3 pos2 = GeoUtils.LatLonToUnityPosition(centerLat, centerLon, node2.lat, node2.lon, zoomLevel, tileSize) - tileObj.transform.localPosition;

                        pos1.y = 0.1f; // Ajouter une petite élévation pour éviter la superposition
                        pos2.y = 0.1f;

                        Vector3 direction = (pos2 - pos1).normalized;
                        Vector3 normal = Vector3.Cross(direction, Vector3.up).normalized;

                        allVertices.Add(pos1 + normal * roadWidth / 2f);
                        allVertices.Add(pos1 - normal * roadWidth / 2f);
                        allVertices.Add(pos2 + normal * roadWidth / 2f);
                        allVertices.Add(pos2 - normal * roadWidth / 2f);

                        allTriangles.Add(vertexIndexOffset + 0);
                        allTriangles.Add(vertexIndexOffset + 2);
                        allTriangles.Add(vertexIndexOffset + 1);

                        allTriangles.Add(vertexIndexOffset + 2);
                        allTriangles.Add(vertexIndexOffset + 3);
                        allTriangles.Add(vertexIndexOffset + 1);

                        vertexIndexOffset += 4;
                    }
                }
            }
        }

        if (allVertices.Count > 0)
        {
            Mesh mesh = new Mesh();
            mesh.vertices = allVertices.ToArray();
            mesh.triangles = allTriangles.ToArray();
            mesh.RecalculateNormals();
            meshFilter.mesh = mesh;
        }
        else
        {
            Destroy(roadsMeshObject);
        }
    }

    // Fonction d'aide pour sauvegarder les données JSON
    private void SaveJsonToFile(string jsonData, string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        string fullPath = path;
        File.WriteAllText(fullPath, jsonData);
        Debug.Log($"Données JSON sauvegardées à : {fullPath}");

        // Uniquement dans l'éditeur, pour que Unity rafraichisse les fichiers
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }
    #endregion
}