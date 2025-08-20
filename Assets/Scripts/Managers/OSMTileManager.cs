using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System.Globalization;

public class OSMTileManager : MonoBehaviour
{
    [Header("Configuration Carte")]
    public int zoomLevel = 12;
    public double centerLat = 48.8566; // Paris latitude
    public double centerLon = 2.3522;  // Paris longitude
    public int tileRadius = 3; // Reduire le nombre de tuiles pour debugger

    [Header("Parametres Visuels")]
    public Material tileMaterial;
    public int tileSize = 10;

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

    public Button nextLineButton;
    public TMP_Text lineText;
    public Button previousLineButton;
    private int currentLineIndex = 0;

    public Button validationButtonUI;
    public Button closeButtonUI;
    public GameObject trainPrefab;
    public float newLat;
    public float newLon;

    void Awake()
    {
        connect = GetComponent<ConnectStations>();
        if (connect != null)
        {
            connect.Init();
        }
        SuperGlobal.SetStations();
        InitGame();

        nextLineButton.onClick.AddListener(ShowNextLine);
        previousLineButton.onClick.AddListener(ShowPreviousLine);
        RefreshUI();
    }

    private void ShowNextLine()
    {
        if (currentLineIndex < SuperGlobal.trainLines.Count - 1)
        {
            currentLineIndex++;
            RefreshUI();
        }
    }

    private void ShowPreviousLine()
    {
        if (currentLineIndex > 0)
        {
            currentLineIndex--;
            RefreshUI();
        }
    }

    private void RefreshUI()
    {
        currentLineIndex = Mathf.Clamp(currentLineIndex, 0, SuperGlobal.trainLines.Count - 1);

        lineText.text = $"Line {SuperGlobal.trainLines[currentLineIndex].lineNumber}";
        SuperGlobal.Log(lineText.text);
        // Active/désactive navigation selon index
        previousLineButton.interactable = currentLineIndex > 0;
        nextLineButton.interactable = currentLineIndex < SuperGlobal.trainLines.Count - 1;
    }

    void Start()
    {

        validationButtonUI.onClick.AddListener(AddStationOnMap);
        closeButtonUI.onClick.AddListener(CloseUI);

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

    private void AddStationOnMap()
    {
        TrainLine trainLine = SuperGlobal.trainLines[currentLineIndex];
        Station newStation = trainLine.AddStation(stationName.text, newLat, newLon);
        if (newStation != null)
        {

            GameObject obj = PlacePoint(newLat, newLon, stationPrefab);
            obj.name = stationName.text;
            newStation.obj = obj;

            StationController controller = obj.GetComponent<StationController>();
            controller.station = newStation;
            foreach (TrainLine tl in SuperGlobal.trainLines)
            {
                foreach (var train in tl.trains)
                {
                    controller.UpdateTrainPath(train.gameObject, tl);
                }
            }
            connect.CreateLines(trainLine.stations, trainLine.lineColor);
            SuperGlobal.money -= 500;
            SuperGlobal.nbStation += 1;
            panelStation.SetActive(false);
            SuperGlobal.isUIOpen = false;
            isEditMode = false;
            editionModeUI.enabled = false;
            stationName.text = "";
        }
    }

    private void CloseUI()
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
                // Time.timeScale = 0f;
            }
            else
            {
                editionModeUI.enabled = false;
                // Time.timeScale = 1f;
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

            Vector2 latLon = GeoUtils.UnityPositionToLatLon(centerLat,centerLon,unityClickPosition, zoomLevel,tileSize);
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
        double lat = GeoUtils.TileToLat(tileY + 0.5, zoomLevel); // Ajouter 0.5 pour le centre de la tuile
        double lon = GeoUtils.TileToLon(tileX + 0.5, zoomLevel); // Ajouter 0.5 pour le centre de la tuile

        // Utiliser la meme methode que pour les points pour garantir l'alignement
        Vector3 position = GeoUtils.LatLonToUnityPosition(centerLat,centerLon,lat, lon, zoomLevel,tileSize);

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
        Vector3 pointPosition = GeoUtils.LatLonToUnityPosition(centerLat,centerLon,lat, lon, zoomLevel,tileSize);
        GameObject point = Instantiate(typePoint, this.transform);
        if (typePoint == stationPrefab)
        {
            StationController stationController = stationPrefab.GetComponent<StationController>();
            stationController.trainPrefab = trainPrefab;

        }
        point.transform.localPosition = pointPosition;
        return point;
    }
}