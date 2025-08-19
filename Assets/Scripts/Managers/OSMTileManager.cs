using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System.Globalization;
using Unity.VisualScripting;

public class OSMTileManager : MonoBehaviour
{
    [Header("Configuration Carte")]
    public int zoomLevel = 12;
    public double centerLat = 48.8566; // Paris latitude
    public double centerLon = 2.3522;  // Paris longitude
    public int tileRadius = 3; // Reduire le nombre de tuiles pour debugger

    [Header("Parametres Visuels")]
    public Material tileMaterial;
    public float tileSize = 10f; // Taille d'une tuile dans Unity (doit correspondre aux dimensions du Plane)

    private Dictionary<string, GameObject> loadedTiles = new Dictionary<string, GameObject>();
    private Queue<string> tileLoadQueue = new Queue<string>();
    private bool isLoadingTile = false;

    // Cache des textures pour eviter les rechargements
    private Dictionary<string, Texture2D> textureCache = new Dictionary<string, Texture2D>();

    public GameObject spotPrefab;
    public GameObject stationPrefab;

    private ConnectStations connect;

    public bool isEditMode = false;
    public RawImage editionModeUI;
    public GameObject panelStation;
    public TMP_InputField stationName;
    public Button validationButtonUI;
    public Button closeButtonUI;
    public GameObject trainPrefab;

    public float newLat;
    public float newLon;


    //public GameObject trainStationPrefab; // Préfabriqué pour les gares

    // Classes pour désérialiser la réponse JSON de l'API Overpass (MAISONS ET GARES)
    /*[Serializable]
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
        public Dictionary<string, string> tags;
    }*/

    #region FONCTIONNALITE POUR LES MAISONS
    IEnumerator FetchAndPlaceHouses()
    {
        // 1. Définir le périmètre de recherche (bounding box)
        double minLat = TileYToLat(LatToTileY(centerLat, zoomLevel) + tileRadius, zoomLevel);
        double maxLat = TileYToLat(LatToTileY(centerLat, zoomLevel) - tileRadius, zoomLevel);
        double minLon = TileXToLon(LonToTileX(centerLon, zoomLevel) - tileRadius, zoomLevel);
        double maxLon = TileXToLon(LonToTileX(centerLon, zoomLevel) + tileRadius, zoomLevel);

        string bbox = $"{minLat.ToString(CultureInfo.InvariantCulture)},{minLon.ToString(CultureInfo.InvariantCulture)},{maxLat.ToString(CultureInfo.InvariantCulture)},{maxLon.ToString(CultureInfo.InvariantCulture)}";

        // 2. Construire la requête pour l'API Overpass
        // La requête demande tous les "nodes" (points) avec le tag "railway=station"
        //string query = $"[out:json];node[railway=station]({bbox});out;";

        // La requête demande les "nodes", "ways", et "relations" avec le tag "building=house"
        string query = $"[out:json];(node[building=house]({bbox});way[building=house]({bbox});relation[building=house]({bbox}););out center;";

        // Nouvelle URL correcte
        string url = "http://overpass-api.de/api/interpreter";

        // Créer un objet WWWForm pour envoyer les données correctement
        WWWForm form = new WWWForm();
        form.AddField("data", query);
        // http://overpass-turbo.eu/ pour tester la query 
        Debug.Log("Requête Overpass : " + query);


        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            // Ajouter un User-Agent pour respecter la politique OSM
            www.SetRequestHeader("User-Agent", "Unity-OSM-App/1.0");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                // 3. Traiter la réponse JSON
                string jsonResponse = www.downloadHandler.text;
                OverpassResponse response = JsonUtility.FromJson<OverpassResponse>(jsonResponse);
                Debug.Log(response.elements.Count());
                // 4. Placer un préfabriqué pour chaque gare trouvée
                if (response != null && response.elements != null)
                {
                    /*foreach (var element in response.elements)
                    {
                        if (trainStationPrefab != null)
                        {
                            PlacePoint(element.lat, element.lon, trainStationPrefab);
                        }
                    }*/
                    Debug.Log($"Gares trouvées et placées : {response.elements.Length}");
                }
            }
            else
            {
                Debug.LogError($"Erreur de requête Overpass : {www.error}");
            }
        }
    }
    #endregion

    #region FONCTIONNALITE POUR LES GARES
    IEnumerator FetchAndPlaceStations()
    {
        // 1. Définir le périmètre de recherche (bounding box)
        double minLat = TileYToLat(LatToTileY(centerLat, zoomLevel) + tileRadius, zoomLevel);
        double maxLat = TileYToLat(LatToTileY(centerLat, zoomLevel) - tileRadius, zoomLevel);
        double minLon = TileXToLon(LonToTileX(centerLon, zoomLevel) - tileRadius, zoomLevel);
        double maxLon = TileXToLon(LonToTileX(centerLon, zoomLevel) + tileRadius, zoomLevel);

        string bbox = $"{minLat.ToString(CultureInfo.InvariantCulture)},{minLon.ToString(CultureInfo.InvariantCulture)},{maxLat.ToString(CultureInfo.InvariantCulture)},{maxLon.ToString(CultureInfo.InvariantCulture)}";

        // 2. Construire la requête pour l'API Overpass
        // La requête demande tous les "nodes" (points) avec le tag "railway=station"
        //string query = $"[out:json];node[railway=station]({bbox});out;";

        // La requête demande les "nodes", "ways", et "relations" avec le tag "building=house"
        string query = $"[out:json];node[railway=station]({bbox});out;";

        // Nouvelle URL correcte
        string url = "http://overpass-api.de/api/interpreter";

        // Créer un objet WWWForm pour envoyer les données correctement
        WWWForm form = new WWWForm();
        form.AddField("data", query);
        // http://overpass-turbo.eu/ pour tester la query 
        Debug.Log("Requête Overpass : " + query);


        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            // Ajouter un User-Agent pour respecter la politique OSM
            www.SetRequestHeader("User-Agent", "Unity-OSM-App/1.0");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                // 3. Traiter la réponse JSON
                string jsonResponse = www.downloadHandler.text;
                OverpassResponse response = JsonUtility.FromJson<OverpassResponse>(jsonResponse);
                Debug.Log(response.elements.Count());
                // 4. Placer un préfabriqué pour chaque gare trouvée
                if (response != null && response.elements != null)
                {
                    /*foreach (var element in response.elements)
                    {
                        if (trainStationPrefab != null)
                        {
                            PlacePoint(element.lat, element.lon, trainStationPrefab);
                        }
                    }*/
                    Debug.Log($"Gares trouvées et placées : {response.elements.Length}");
                }
            }
            else
            {
                Debug.LogError($"Erreur de requête Overpass : {www.error}");
            }
        }
    }
    #endregion

    [Header("Parametres de generation")]
    public int roadFetchRadius = 5; // Rayon de recherche pour les routes
    public Material roadMaterial; // Matériel pour les lignes des routes
    private OverpassResponse cachedRoadsData;
    private Dictionary<long, Element> cachedNodesDict;
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
        public List<long> nodes; // Ajout pour les "ways" et "relations"
        public Dictionary<string, string> tags;
    }

    #region FONCTIONNALITE POUR LES ROUTES
    IEnumerator FetchAndDrawRoads()
    {
        // 1. Définir le périmètre de recherche (bounding box)
        double minLat = TileYToLat(LatToTileY(centerLat, zoomLevel) + roadFetchRadius, zoomLevel);
        double maxLat = TileYToLat(LatToTileY(centerLat, zoomLevel) - roadFetchRadius, zoomLevel);
        double minLon = TileXToLon(LonToTileX(centerLon, zoomLevel) - roadFetchRadius, zoomLevel);
        double maxLon = TileXToLon(LonToTileX(centerLon, zoomLevel) + roadFetchRadius, zoomLevel);

        // On formate les nombres en utilisant la culture invariante pour garantir l'utilisation du point
        string bbox = $"{minLat.ToString(CultureInfo.InvariantCulture)},{minLon.ToString(CultureInfo.InvariantCulture)},{maxLat.ToString(CultureInfo.InvariantCulture)},{maxLon.ToString(CultureInfo.InvariantCulture)}";

        // 2. Construire la requête pour l'API Overpass
        // La requête demande les 'ways' (routes) avec la balise 'highway' et leurs 'nodes' constitutifs
        string query = $"[out:json];way[highway]({bbox});(._;>;);out;";

        string url = "http://overpass-api.de/api/interpreter";

        WWWForm form = new WWWForm();
        form.AddField("data", query);

        Debug.Log("Requête Overpass : " + query);
        Debug.Log("URL Overpass : " + url);

        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            www.SetRequestHeader("User-Agent", "Unity-OSM-App/1.0");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                // 3. Traiter la réponse JSON
                string jsonResponse = www.downloadHandler.text;
                OverpassResponse response = JsonUtility.FromJson<OverpassResponse>(jsonResponse);

                // Dictionnaire pour stocker les nodes par ID
                Dictionary<long, Element> nodesDict = new Dictionary<long, Element>();
                foreach (var element in response.elements)
                {
                    if (element.type == "node")
                    {
                        nodesDict[element.id] = element;
                    }
                }

                int roadCount = 0;
                // 4. Parcourir les 'ways' et dessiner les lignes
                foreach (var element in response.elements)
                {
                    if (element.type == "way" && element.nodes != null)
                    {
                        // Créer un nouvel objet pour la route
                        GameObject roadObject = new GameObject($"Road_{element.id}");
                        roadObject.transform.parent = this.transform;

                        // Ajouter le composant LineRenderer
                        LineRenderer lineRenderer = roadObject.AddComponent<LineRenderer>();
                        lineRenderer.material = roadMaterial;
                        lineRenderer.startWidth = 0.5f; // Largeur de la ligne
                        lineRenderer.endWidth = 0.5f;

                        // Utiliser l'espace monde pour que les points soient positionnés correctement
                        // par rapport à l'objet parent, même s'il n'est pas à (0,0,0).
                        lineRenderer.useWorldSpace = true;

                        lineRenderer.positionCount = element.nodes.Count;

                        // Créer un tableau de positions
                        Vector3[] positions = new Vector3[element.nodes.Count];
                        for (int i = 0; i < element.nodes.Count; i++)
                        {
                            long nodeId = element.nodes[i];
                            if (nodesDict.ContainsKey(nodeId))
                            {
                                Element node = nodesDict[nodeId];
                                // Appliquer la position du parent (le manager) pour corriger le décalage
                                positions[i] = LatLonToUnityPosition(node.lat, node.lon, zoomLevel) + new Vector3(transform.position.x, 0, transform.position.z);
                            }
                        }
                        lineRenderer.SetPositions(positions);
                        roadCount++;
                    }
                }
                Debug.Log($"Routes trouvées et tracées : {roadCount}");
            }
            else
            {
                Debug.LogError($"Erreur de requête Overpass : {www.error}");
            }
        }
    }
    #endregion

    /*#region FONCTIONNALITE POUR LES ROUTES
    // Étape 1: Récupérer toutes les données de routes dans le périmètre
    IEnumerator FetchAndDrawRoads()
    {
        double minLat = TileYToLat(LatToTileY(centerLat, zoomLevel) + roadFetchRadius, zoomLevel);
        double maxLat = TileYToLat(LatToTileY(centerLat, zoomLevel) - roadFetchRadius, zoomLevel);
        double minLon = TileXToLon(LonToTileX(centerLon, zoomLevel) - roadFetchRadius, zoomLevel);
        double maxLon = TileXToLon(LonToTileX(centerLon, zoomLevel) + roadFetchRadius, zoomLevel);

        string bbox = $"{minLat.ToString(CultureInfo.InvariantCulture)},{minLon.ToString(CultureInfo.InvariantCulture)},{maxLat.ToString(CultureInfo.InvariantCulture)},{maxLon.ToString(CultureInfo.InvariantCulture)}";

        string query = $"[out:json];way[highway]({bbox});(._;>;);out;";
        string url = "http://overpass-api.de/api/interpreter";
        WWWForm form = new WWWForm();
        form.AddField("data", query);

        Debug.Log("Requête Overpass : " + query);

        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            www.SetRequestHeader("User-Agent", "Unity-OSM-App/1.0");
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = www.downloadHandler.text;
                cachedRoadsData = JsonUtility.FromJson<OverpassResponse>(jsonResponse);

                cachedNodesDict = new Dictionary<long, Element>();
                foreach (var element in cachedRoadsData.elements)
                {
                    if (element.type == "node")
                    {
                        cachedNodesDict[element.id] = element;
                    }
                }
            }
            else
            {
                Debug.LogError($"Erreur de requête Overpass : {www.error}");
            }
        }
    }

    // Étape 2: Générer un mesh de routes pour une tuile spécifique
    void CreateRoadsMeshForTile(int tileX, int tileY)
    {
        // Créer un GameObject pour ce mesh de routes
        GameObject roadsMeshObject = new GameObject($"Roads_Mesh_{tileX}_{tileY}");
        roadsMeshObject.transform.parent = this.transform;

        MeshFilter meshFilter = roadsMeshObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = roadsMeshObject.AddComponent<MeshRenderer>();

        meshRenderer.material = roadMaterial;

        // Listes pour stocker les donnees du mesh
        List<Vector3> allVertices = new List<Vector3>();
        List<int> allTriangles = new List<int>();

        float roadWidth = 0.5f;
        int vertexIndexOffset = 0;

        Vector3 tileUnityPos = LatLonToUnityPosition(TileToLat(tileY + 0.5, zoomLevel), TileToLon(tileX + 0.5, zoomLevel), zoomLevel);

        foreach (var element in cachedRoadsData.elements)
        {
            if (element.type == "way" && element.nodes != null && element.nodes.Count >= 2)
            {
                // Vérifier si la route passe par la tuile
                bool roadInTile = false;
                foreach (long nodeId in element.nodes)
                {
                    if (cachedNodesDict.ContainsKey(nodeId))
                    {
                        Element node = cachedNodesDict[nodeId];
                        Vector3 nodeUnityPos = LatLonToUnityPosition(node.lat, node.lon, zoomLevel) + new Vector3(transform.position.x, 0, transform.position.z);
                        if (Mathf.Abs(nodeUnityPos.x - tileUnityPos.x) <= tileSize / 2f && Mathf.Abs(nodeUnityPos.z - tileUnityPos.z) <= tileSize / 2f)
                        {
                            roadInTile = true;
                            break;
                        }
                    }
                }

                if (roadInTile)
                {
                    for (int i = 0; i < element.nodes.Count - 1; i++)
                    {
                        long nodeId1 = element.nodes[i];
                        long nodeId2 = element.nodes[i + 1];

                        if (cachedNodesDict.ContainsKey(nodeId1) && cachedNodesDict.ContainsKey(nodeId2))
                        {
                            Element node1 = cachedNodesDict[nodeId1];
                            Element node2 = cachedNodesDict[nodeId2];

                            Vector3 pos1 = LatLonToUnityPosition(node1.lat, node1.lon, zoomLevel) + new Vector3(transform.position.x, 0.1f, transform.position.z);
                            Vector3 pos2 = LatLonToUnityPosition(node2.lat, node2.lon, zoomLevel) + new Vector3(transform.position.x, 0.1f, transform.position.z);

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
    #endregion*/

    void Awake()
    {
        connect = GetComponent<ConnectStations>();
        if (connect != null)
        {
            connect.Init();
        }
        SuperGlobal.SetStations();
        InitGame();
    }

    void Start()
    {

        validationButtonUI.onClick.AddListener(AddStation);
        closeButtonUI.onClick.AddListener(closeUI);

    }

    private void InitGame()
    {
        InitLocation();
        InitStations();
    }

    // Instancier toutes les stations et les stocker
    private void InitStations()
    {
        foreach (TrainLine line in SuperGlobal.trainlines)
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



    private void AddStation()
    {
        if (SuperGlobal.money - 500 >= 0 && !string.IsNullOrEmpty(stationName.text))
        {
            Station newStation = new Station(stationName.text, newLat, newLon, 1, SuperGlobal.trainlines[0].stations.Count);
            SuperGlobal.trainlines[0].stations.Add(newStation);

            GameObject obj = PlacePoint(newLat, newLon, stationPrefab);
            obj.name = stationName.text;
            newStation.obj = obj;

            StationController controller = obj.GetComponent<StationController>();
            controller.station = newStation;
            foreach (Line line in SuperGlobal.lines)
            {
                foreach (var train in line.trainsList)
                {
                    controller.UpdateTrainPath(train.gameObject);
                }
            }


            connect.CreateLines(SuperGlobal.trainlines[0].stations, SuperGlobal.trainlines[0].lineColor);
            SuperGlobal.money -= 500;
            SuperGlobal.nbStation += 1;
            panelStation.SetActive(false);
            SuperGlobal.isUIOpen = false;
            isEditMode = false;
            editionModeUI.enabled = false;
            stationName.text = "";

        }
        else if (string.IsNullOrEmpty(stationName.text))
        {
            Debug.LogWarning("Le nom de la station ne peut pas être vide !");
        }
        else
        {
            Debug.Log("No such money ! ");
        }
    }

    private void closeUI()
    {
        panelStation.SetActive(false);
        SuperGlobal.isUIOpen = false;
        stationName.text = "";
        newLat = 0f;
        newLon = 0f;
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

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            isEditMode = !isEditMode;
            if (isEditMode)
            {
                editionModeUI.enabled = true;
            }
            else
            {
                editionModeUI.enabled = false;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (isEditMode && !SuperGlobal.isUIOpen)
            {
                HandleMapClick();
            }
        }
    }

    void HandleMapClick()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            Vector3 unityClickPosition = hit.point;

            unityClickPosition.x -= gameObject.transform.position.x;
            unityClickPosition.z -= gameObject.transform.position.z;

            Vector2 latLon = UnityPositionToLatLon(unityClickPosition, zoomLevel);
            newLat = latLon.x;
            newLon = latLon.y;
            panelStation.SetActive(true);
            SuperGlobal.isUIOpen = true;
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
        Debug.Log("Tuiles generees manuellement !");
    }

    [ContextMenu("Get les routes")]
    public void GetLesRoutes()
    {

        StartCoroutine(FetchAndDrawRoads());
    }



    public void LoadTilesAroundCenter()
    {

        // Convertir les coordonnees GPS en coordonnees de tuile
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

        // Verifier le cache d'abord
        if (textureCache.ContainsKey(tileKey))
        {
            CreateTileGameObject(tileKey, x, y, textureCache[tileKey]);
            isLoadingTile = false;
            yield break;
        }

        // Telecharger la tuile
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
                Debug.LogError($"Erreur de telechargement pour la tuile {tileKey}: {www.error}");
            }
        }

        isLoadingTile = false;

        // Petite pause pour eviter de surcharger les serveurs OSM
        yield return new WaitForSeconds(0.1f);
    }

    void CreateTileGameObject(string tileKey, int tileX, int tileY, Texture2D texture)
    {
        // Creer un GameObject pour la tuile
        GameObject tileObj = GameObject.CreatePrimitive(PrimitiveType.Plane);
        tileObj.name = $"Tile_{tileKey}";
        tileObj.transform.parent = this.transform;

        // Utiliser les coordonnees precises de la tuile pour la positionner
        double lat = TileToLat(tileY + 0.5, zoomLevel); // Ajouter 0.5 pour le centre de la tuile
        double lon = TileToLon(tileX + 0.5, zoomLevel); // Ajouter 0.5 pour le centre de la tuile

        // Utiliser la meme methode que pour les points pour garantir l'alignement
        Vector3 position = LatLonToUnityPosition(lat, lon, zoomLevel);

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


    // Convertir latitude/longitude en coordonnees de tuile
    public static Vector2Int LatLonToTile(double lat, double lon, int zoom)
    {
        double latRad = lat * Math.PI / 180.0;
        double n = Math.Pow(2.0, zoom);

        int x = (int)Math.Floor((lon + 180.0) / 360.0 * n);
        int y = (int)Math.Floor((1.0 - Math.Asinh(Math.Tan(latRad)) / Math.PI) / 2.0 * n);

        return new Vector2Int(x, y);
    }

    // Place this inside your OSMTileManager class
    public GameObject PlacePoint(double lat, double lon, GameObject typePoint)
    {
        Vector3 pointPosition = LatLonToUnityPosition(lat, lon, zoomLevel);
        GameObject point = Instantiate(typePoint, this.transform);
        if (typePoint == stationPrefab)
        {
            StationController stationController = stationPrefab.GetComponent<StationController>();
            stationController.trainPrefab = trainPrefab;

        }
        point.transform.localPosition = pointPosition;
        return point;
    }

    public Vector3 LatLonToUnityPosition(double lat, double lon, int zoom)
    {
        double centerTileX = LonToTileX(centerLon, zoom);
        double centerTileY = LatToTileY(centerLat, zoom);

        double pointTileX = LonToTileX(lon, zoom);
        double pointTileY = LatToTileY(lat, zoom);

        double dx = pointTileX - centerTileX;
        double dy = pointTileY - centerTileY;

        return new Vector3(-(float)(dx * tileSize), 0, (float)(dy * tileSize));
    }

    public Vector2 UnityPositionToLatLon(Vector3 unityPosition, int zoom)
    {
        double dx = -unityPosition.x / tileSize;
        double dy = unityPosition.z / tileSize;

        double centerTileX = LonToTileX(centerLon, zoom);
        double centerTileY = LatToTileY(centerLat, zoom);

        double pointTileX = centerTileX + dx;
        double pointTileY = centerTileY + dy;

        double lon = TileXToLon(pointTileX, zoom);
        double lat = TileYToLat(pointTileY, zoom);

        return new Vector2((float)lat, (float)lon);
    }

    #region CONVERSION DES LATITUDES LONGITUDES
    private double LonToTileX(double lon, int zoom)
    {
        return (lon + 180.0) / 360.0 * Math.Pow(2.0, zoom);
    }

    private double LatToTileY(double lat, int zoom)
    {
        double latRad = lat * Math.PI / 180.0;
        return (1.0 - Math.Asinh(Math.Tan(latRad)) / Math.PI) / 2.0 * Math.Pow(2.0, zoom);
    }

    private double TileXToLon(double tileX, int zoom)
    {
        double n = Math.Pow(2.0, zoom);
        return tileX / n * 360.0 - 180.0;
    }

    private double TileYToLat(double tileY, int zoom)
    {
        double n = Math.Pow(2.0, zoom);
        double latRad = Math.Atan(Math.Sinh(Math.PI * (1.0 - 2.0 * tileY / n)));
        return latRad * 180.0 / Math.PI;
    }

    private double TileToLon(double x, int zoom)
    {
        double n = Math.Pow(2.0, zoom);
        return x / n * 360.0 - 180.0;
    }

    private double TileToLat(double y, int zoom)
    {
        double n = Math.Pow(2.0, zoom);
        double latRad = Math.Atan(Math.Sinh(Math.PI * (1 - 2 * y / n)));
        return latRad * 180.0 / Math.PI;
    }
    #endregion
}