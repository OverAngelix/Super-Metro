using System;
using UnityEngine;

public static class GeoUtils
{

    public static Vector3 LatLonToUnityPosition(double centerLat, double centerLon, double lat, double lon, int zoom, int tileSize)
    {
        double centerTileX = LonToTileX(centerLon, zoom);
        double centerTileY = LatToTileY(centerLat, zoom);

        double pointTileX = LonToTileX(lon, zoom);
        double pointTileY = LatToTileY(lat, zoom);

        double dx = pointTileX - centerTileX;
        double dy = pointTileY - centerTileY;

        return new Vector3(-(float)(dx * tileSize), 0, (float)(dy * tileSize));
    }

    public static Vector2 UnityPositionToLatLon(double centerLat, double centerLon, Vector3 unityPosition, int zoom, int tileSize)
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

    public static double LonToTileX(double lon, int zoom)
    {
        return (lon + 180.0) / 360.0 * Math.Pow(2.0, zoom);
    }

    public static double LatToTileY(double lat, int zoom)
    {
        double latRad = lat * Math.PI / 180.0;
        return (1.0 - Math.Asinh(Math.Tan(latRad)) / Math.PI) / 2.0 * Math.Pow(2.0, zoom);
    }

    public static Vector2Int LatLonToTile(double lat, double lon, int zoom)
    {

        int x = (int)LonToTileX(lon, zoom);
        int y = (int)LatToTileY(lat, zoom);

        return new Vector2Int(x, y);
    }

    public static double TileXToLon(double tileX, int zoom)
    {
        double n = Math.Pow(2.0, zoom);
        return tileX / n * 360.0 - 180.0;
    }

    public static double TileYToLat(double tileY, int zoom)
    {
        double n = Math.Pow(2.0, zoom);
        double latRad = Math.Atan(Math.Sinh(Math.PI * (1.0 - 2.0 * tileY / n)));
        return latRad * 180.0 / Math.PI;
    }

    public static double TileToLon(double x, int zoom)
    {
        double n = Math.Pow(2.0, zoom);
        return x / n * 360.0 - 180.0;
    }

    public static double TileToLat(double y, int zoom)
    {
        double n = Math.Pow(2.0, zoom);
        double latRad = Math.Atan(Math.Sinh(Math.PI * (1 - 2 * y / n)));
        return latRad * 180.0 / Math.PI;
    }
}
