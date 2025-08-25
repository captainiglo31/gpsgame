// Assets/Scripts/GpsGame/Demo/GpsApiDemo.cs
using System;
using System.Linq;
using System.Threading.Tasks;
using GpsGame.Net;
using GpsGame.Net.Models;
using UnityEngine;

public class GpsApiDemo : MonoBehaviour
{
    [Header("API")]
    [Tooltip("z.B. https://localhost:44306 (ohne Slash am Ende)")]
    public string baseUrl = "https://localhost:44306";
    [Tooltip("Token aus Server-Log (DbSeeder Ausgabe)")]
    public string playerToken = "";
     
[Header("Auth")]
		[Tooltip("Player Id (GUID) aus der Datenbank – NICHT der Token!")]
		public string playerId = "";
    
    [Header("Player Position (Test)")]
    public double playerLat = 51.5149;
    public double playerLng = 6.3301;
    public double bboxHalfSizeMeters = 200.0;

    [Header("Collect")]
    public int collectAmount = 5;

    private GpsApiClient _api;

    private void Awake()
    {
        _api = new GpsApiClient(baseUrl, playerToken);
    }

    private async void Start()
    {
        await FetchAndLogNodes();
    }

    private void Update()
    {
        // Drücke 'C' → versuche, den ersten Node in Reichweite zu sammeln
        if (Input.GetKeyDown(KeyCode.C))
        {
            _ = CollectNearestInRange();
        }

        // Drücke 'R' → frisch laden
        if (Input.GetKeyDown(KeyCode.R))
        {
            _ = FetchAndLogNodes();
        }
    }

    private async Task FetchAndLogNodes()
    {
        var bbox = BBoxFromCenter(playerLat, playerLng, bboxHalfSizeMeters);
        var nodes = await _api.GetResourcesAsync(bbox.minLat, bbox.minLng, bbox.maxLat, bbox.maxLng);

        Debug.Log($"[Demo] Loaded {nodes.Length} nodes in bbox.");
        foreach (var n in nodes)
            Debug.Log($"- {n.type} #{n.id} @({n.latitude:F5},{n.longitude:F5}) amt={n.amount} respawn={n.respawnAtUtc}");
    }

    private async Task CollectNearestInRange()
    {
        var bbox = BBoxFromCenter(playerLat, playerLng, bboxHalfSizeMeters);
        var nodes = await _api.GetResourcesAsync(bbox.minLat, bbox.minLng, bbox.maxLat, bbox.maxLng);
        if (nodes.Length == 0)
        {
            Debug.LogWarning("[Demo] Keine Nodes in der Nähe.");
            return;
        }

        // Simplest: nimm den ersten Node (du kannst gerne Distanz sortieren)
        var node = nodes.First();
        var amount = Mathf.Clamp(collectAmount, 1, 50);
        var res = await _api.CollectAsync(node.id, playerLat, playerLng, amount, playerId);

        if (res.success)
        {
            Debug.Log($"[Demo] Collect OK: +{res.collected}, remaining={res.remaining}, respawn={res.respawnAtUtc}");
        }
        else
        {
            Debug.LogWarning($"[Demo] Collect FAIL: reason={res.reason}");
        }
    }

    // --- Hilfen ---
    private static (double minLat, double minLng, double maxLat, double maxLng) BBoxFromCenter(double lat, double lng, double halfSizeMeters)
    {
        // sehr grob: 1° lat ≈ 111_000 m; 1° lon ≈ 111_000 * cos(lat)
        var dLat = halfSizeMeters / 111_000.0;
        var dLng = halfSizeMeters / (111_000.0 * Math.Max(0.1, Math.Cos(lat * Math.PI / 180.0)));
        return (lat - dLat, lng - dLng, lat + dLat, lng + dLng);
    }
}
