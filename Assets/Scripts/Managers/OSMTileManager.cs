using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
        renderer.sharedMaterial.mainTexture = texture;

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
        foreach (var train in trainLine.trains)
        {
            controller.UpdateTrainPath(train.gameObject, trainLine);
        }

        connect.CreateLines(trainLine.stations, trainLine.lineColor);
    }

    #region FONCTIONNALITE POUR LES MAISONS
    private const string HOUSES_JSON_PATH_TEMPLATE = "Assets/Resources/JSON/houses-{0}-{1}.json";
    public Material houseMaterial;
    public float houseHeight = 1.0f;

    IEnumerator CreateHousesForTile(int tileX, int tileY)
    {
        string jsonResponse = null;
        string filePath = string.Format(HOUSES_JSON_PATH_TEMPLATE, tileX, tileY);
        SuperGlobal.Log(filePath);
        if (File.Exists(filePath))
        {
            jsonResponse = File.ReadAllText(filePath);
            Debug.Log($"Données de maisons chargées depuis le fichier JSON : {filePath}");
        }
        else
        {
            Debug.Log("Fichier JSON de maisons non trouvé. Lancement d'une requête Overpass...");

            double minLat = GeoUtils.TileToLat(tileY + 1, zoomLevel);
            double maxLat = GeoUtils.TileToLat(tileY, zoomLevel);
            double minLon = GeoUtils.TileToLon(tileX, zoomLevel);
            double maxLon = GeoUtils.TileToLon(tileX + 1, zoomLevel);

            string bbox = $"{minLat.ToString(CultureInfo.InvariantCulture)},{minLon.ToString(CultureInfo.InvariantCulture)},{maxLat.ToString(CultureInfo.InvariantCulture)},{maxLon.ToString(CultureInfo.InvariantCulture)}";
            string query = $"[out:json];(node[building]({bbox});way[building]({bbox});relation[building]({bbox}););out;";
            string url = "http://overpass-api.de/api/interpreter";
            WWWForm form = new();
            form.AddField("data", query);

            Debug.Log($"Requête Overpass pour les maisons de la tuile ({tileX},{tileY}) : " + query);

            using (UnityWebRequest www = UnityWebRequest.Post(url, form))
            {
                www.SetRequestHeader("User-Agent", "Unity-OSM-App/1.0");
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    jsonResponse = www.downloadHandler.text;
                    Debug.Log("Requête Overpass pour les maisons réussie. Sauvegarde du fichier...");
                    SaveJsonToFile(jsonResponse, string.Format("houses-{0}-{1}.json", tileX, tileY));
                }
                else
                {
                    Debug.LogError($"Erreur de requête Overpass pour les maisons de la tuile ({tileX},{tileY}) : {www.error}");
                    yield break;
                }
            }
        }

        OverpassResponse response = JsonUtility.FromJson<OverpassResponse>(jsonResponse);

        // Crée un dictionnaire pour un accès rapide aux nœuds
        Dictionary<long, Element> nodesDict = new();
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
            Debug.LogError($"Tile {tileKey} not found. Cannot create houses.");
            yield break;
        }
        GameObject tileObj = loadedTiles[tileKey];

        foreach (var element in response.elements)
        {
            if (element.type == "way" && element.nodes != null && element.nodes.Count > 0)
            {
                SuperGlobal.Log("MAISON " +element.id.ToString());
                GameObject house = new($"House_{element.id}");
                house.transform.parent = tileObj.transform;

                MeshFilter meshFilter = house.AddComponent<MeshFilter>();
                MeshRenderer meshRenderer = house.AddComponent<MeshRenderer>();
                meshRenderer.material = houseMaterial;

                Mesh mesh = new();
                List<Vector3> vertices = new();
                List<int> triangles = new();

                // Création du maillage de base (face inférieure)
                List<Vector3> footprintVertices = new();

                for (int i = 0; i < element.nodes.Count; i++)
                {
                    if (nodesDict.ContainsKey(element.nodes[i]))
                    {
                        Element node = nodesDict[element.nodes[i]];
                        Vector3 globalPos = GeoUtils.LatLonToUnityPosition(centerLat, centerLon, node.lat, node.lon, zoomLevel, tileSize);
                        Vector3 localPos = globalPos - tileObj.transform.localPosition;
                        footprintVertices.Add(new Vector3(localPos.x, 0, localPos.z));
                    }
                }

                // Triangulation de la base du polygone
                for (int i = 1; i < footprintVertices.Count - 1; i++)
                {
                    triangles.Add(0);
                    triangles.Add(i);
                    triangles.Add(i + 1);
                }

                // Extrusion du maillage pour ajouter les faces latérales et le toit
                int baseVertexCount = vertices.Count;
                int footprintCount = footprintVertices.Count;

                // Ajout des sommets du toit
                foreach (var v in footprintVertices)
                {
                    vertices.Add(v); // Base
                    vertices.Add(new Vector3(v.x, houseHeight, v.z)); // Toit
                }

                // Création des faces latérales
                for (int i = 0; i < footprintCount; i++)
                {
                    int currentBaseIndex = i * 2;
                    int nextBaseIndex = ((i + 1) % footprintCount) * 2;

                    // Premier triangle de la face
                    triangles.Add(currentBaseIndex);
                    triangles.Add(nextBaseIndex);
                    triangles.Add(currentBaseIndex + 1);

                    // Deuxième triangle de la face
                    triangles.Add(nextBaseIndex);
                    triangles.Add(nextBaseIndex + 1);
                    triangles.Add(currentBaseIndex + 1);
                }

                // Ajout des triangles pour le toit
                for (int i = 1; i < footprintCount - 1; i++)
                {
                    triangles.Add(baseVertexCount + 1);
                    triangles.Add(baseVertexCount + i * 2 + 1);
                    triangles.Add(baseVertexCount + (i + 1) * 2 + 1);
                }

                mesh.vertices = vertices.ToArray();
                mesh.triangles = triangles.ToArray();
                mesh.RecalculateNormals();
                meshFilter.mesh = mesh;
                SuperGlobal.Log("FIN CHARGEMENT MAISON " + $"House_{element.id}");
            }
        }

    }
    #endregion


    /*IEnumerator CreateHousesForTile(int tileX, int tileY)
    {
        string jsonResponse = null;
        string filePath = string.Format(HOUSES_JSON_PATH_TEMPLATE, tileX, tileY);

        // Tenter de charger les données depuis un fichier JSON local
        if (File.Exists(filePath))
        {
            jsonResponse = File.ReadAllText(filePath);
            Debug.Log($"Données de maisons chargées depuis le fichier JSON : {filePath}");
        }
        else
        {
            // Le fichier n'existe pas, on lance une requête Overpass
            Debug.Log("Fichier JSON de maisons non trouvé. Lancement d'une requête Overpass...");

            // Déterminer la zone de la tuile
            double minLat = GeoUtils.TileToLat(tileY + 1, zoomLevel);
            double maxLat = GeoUtils.TileToLat(tileY, zoomLevel);
            double minLon = GeoUtils.TileToLon(tileX, zoomLevel);
            double maxLon = GeoUtils.TileToLon(tileX + 1, zoomLevel);

            string bbox = $"{minLat.ToString(CultureInfo.InvariantCulture)},{minLon.ToString(CultureInfo.InvariantCulture)},{maxLat.ToString(CultureInfo.InvariantCulture)},{maxLon.ToString(CultureInfo.InvariantCulture)}";
            string query = $"[out:json];(node[building=house]({bbox});way[building=house]({bbox});relation[building=house]({bbox}););out center;";
            string url = "http://overpass-api.de/api/interpreter";
            WWWForm form = new WWWForm();
            form.AddField("data", query);

            Debug.Log($"Requête Overpass pour les maisons de la tuile ({tileX},{tileY}) : " + query);

            using (UnityWebRequest www = UnityWebRequest.Post(url, form))
            {
                www.SetRequestHeader("User-Agent", "Unity-OSM-App/1.0");
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    jsonResponse = www.downloadHandler.text;
                    Debug.Log("Requête Overpass pour les maisons réussie. Sauvegarde du fichier...");
                    SaveJsonToFile(jsonResponse, string.Format("houses-{0}-{1}.json", tileX, tileY));
                }
                else
                {
                    Debug.LogError($"Erreur de requête Overpass pour les maisons de la tuile ({tileX},{tileY}) : {www.error}");
                    yield break;
                }
            }
        }

        OverpassResponse response = JsonUtility.FromJson<OverpassResponse>(jsonResponse);

        string tileKey = $"{zoomLevel}_{tileX}_{tileY}";
        if (!loadedTiles.ContainsKey(tileKey))
        {
            Debug.LogError($"Tile {tileKey} not found. Cannot create houses.");
            yield break;
        }
        GameObject tileObj = loadedTiles[tileKey];


        foreach (var element in response.elements)
        {

            // Créer un cube pour représenter la maison
            GameObject house = GameObject.CreatePrimitive(PrimitiveType.Cube);
            house.name = $"House_{element.id}";
            house.transform.parent = tileObj.transform;

            // Ajuster la position en Z pour qu'il soit sur la tuile et la taille
            house.transform.localScale = new Vector3(0.1f, 1.0f, 0.1f);


            // Convert the geographic coordinates to a Unity position
            Vector3 globalPosition = GeoUtils.LatLonToUnityPosition(centerLat, centerLon, element.center.lat, element.center.lon, zoomLevel, tileSize);

            // The position relative to the tile's local position
            Vector3 localPosition = globalPosition - tileObj.transform.localPosition;

            // Set the house's local position
            house.transform.localPosition = new Vector3(localPosition.x, houseHeight, localPosition.z);

            // Apply material
            if (houseMaterial != null)
            {
                house.GetComponent<Renderer>().material = houseMaterial;
            }
        }

        Debug.Log($"Maisons pour la tuile ({tileX},{tileY}) placées : {response.elements.Length}");
    }
    #endregion*/




    #region FONCTIONNALITE POUR LES ROUTES
    [Header("Parametres de generation")]
    public Material roadMaterial;
    private const string ROADS_JSON_PATH_TEMPLATE = "Assets/Resources/JSON/routes-{0}-{1}.json";
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
        public Center center;
        public List<long> nodes;
        public Dictionary<string, string> tags;
    }


    [Serializable]
    public class Center
    {
        public double lat;
        public double lon;
    }



    // Étape 2: Générer un mesh de routes pour une tuile spécifique en faisant une requête ciblée
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
                    SaveJsonToFile(jsonResponse, string.Format("routes-{0}-{1}.json", tileX, tileY));
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
        float roadWidth = 0.1f;
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
            Debug.Log($"Routes fusionnées en un seul mesh pour la tuile ({tileX},{tileY}) avec {allVertices.Count} sommets.");
        }
        else
        {
            Destroy(roadsMeshObject);
        }
    }

    // Fonction d'aide pour sauvegarder les données JSON
    private void SaveJsonToFile(string jsonData, string filename)
    {
        // Le chemin pour les fichiers de ressources est toujours Resources/
        string path = Application.dataPath + "/Resources/JSON/";

        // S'assurer que le dossier existe
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        string fullPath = path + filename;
        File.WriteAllText(fullPath, jsonData);
        Debug.Log($"Données JSON sauvegardées à : {fullPath}");

        // Uniquement dans l'éditeur, pour que Unity rafraichisse les fichiers
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }
    #endregion
}