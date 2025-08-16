using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using UnityEngine.UI;
using TMPro;

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



    public float newLat;
    public float newLon;

    void Awake()
    {
        connect = GetComponent<ConnectStations>();
        if (connect != null)
        {
            connect.Init();
        }
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
        for (int i = 0; i < SuperGlobal.stations.Count; i++)
        {
            var sta = SuperGlobal.stations[i];
            GameObject obj = PlacePoint(sta.lat, sta.lon, stationPrefab);
            obj.name = sta.name;
            sta.obj = obj;
            StationController controller = obj.GetComponent<StationController>();
            controller.station = sta;
        }

        // Relier les stations
        connect.CreateLines(SuperGlobal.stations);
    }



    private void AddStation()
    {
        if (SuperGlobal.money - 500 >= 0 && !string.IsNullOrEmpty(stationName.text))
        {
            SuperGlobal.Station newStation = new SuperGlobal.Station(stationName.text, newLat, newLon, 1, 5);
            SuperGlobal.stations.Add(newStation);

            GameObject obj = PlacePoint(newLat, newLon, stationPrefab);
            obj.name = stationName.text;
            newStation.obj = obj;

            StationController controller = obj.GetComponent<StationController>();
            controller.station = newStation;
            foreach (SuperGlobal.Line line in SuperGlobal.lines)
            {
                foreach (var train in line.trainsList)
                {
                    controller.UpdateTrainPath(train.gameObject);
                }
            }

            
            connect.CreateLines(SuperGlobal.stations);
            SuperGlobal.money -= 500;
            panelStation.SetActive(false);
            SuperGlobal.isUIOpen = false;
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