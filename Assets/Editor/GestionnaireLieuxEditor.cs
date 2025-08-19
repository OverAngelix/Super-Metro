using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Place))]
public class GestionnaireLieuxEditor : Editor
{
    private PlacesJson placesJson;
    private int _indexSelectionne = 0;
    private string[] _nomsLieux;
    private Place _gestionnaireLieux;
    private OSMTileManager _osmtileManager;

    private void OnEnable()
    {
        _gestionnaireLieux = (Place)target;
        _osmtileManager = _gestionnaireLieux.GetComponent<OSMTileManager>();
        TextAsset jsonFile = Resources.Load<TextAsset>("JSON/places");

        if (jsonFile != null)
        {
            string jsonTexte = jsonFile.text;
            placesJson = JsonUtility.FromJson<PlacesJson>(jsonTexte);
            if (placesJson.lieux != null && placesJson.lieux.Length > 0)
            {
                _nomsLieux = new string[placesJson.lieux.Length];
                for (int i = 0; i < placesJson.lieux.Length; i++)
                {
                    _nomsLieux[i] = placesJson.lieux[i].name;
                }
            }
        }
    }

    public override void OnInspectorGUI()
    {
        if (placesJson.lieux != null && placesJson.lieux.Length > 0)
        {
            int nouvelIndex = EditorGUILayout.Popup("Sélectionner un lieu", _indexSelectionne, _nomsLieux);
            if (nouvelIndex != _indexSelectionne)
            {
                _indexSelectionne = nouvelIndex;
                PlaceJson lieuSelectionne = placesJson.lieux[_indexSelectionne];
                _osmtileManager.zoomLevel = lieuSelectionne.zoom;
                _osmtileManager.centerLat = lieuSelectionne.lat;
                _osmtileManager.centerLon = lieuSelectionne.lon;
                EditorUtility.SetDirty(_osmtileManager);
            }
        }
    }
}