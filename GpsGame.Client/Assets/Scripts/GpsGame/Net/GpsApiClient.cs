// Assets/Scripts/GpsGame/Net/GpsApiClient.cs
using System;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using UnityEngine;
using UnityEngine.Networking;


namespace GpsGame.Net
{
    /// <summary>
    /// Minimaler HTTP-Client für die GpsGame-API.
    /// - Setzt automatisch X-Player-Token auf alle Requests
    /// - BBox-Fetch und Collect
    /// - Parsen via JsonUtility (Wrapper bei Top-Level-Array)
    /// </summary>
    public sealed class GpsApiClient
    {
		


        public string BaseUrl { get; }
        public string PlayerToken { get; private set; }

        public GpsApiClient(string baseUrl, string playerToken)
        {
            BaseUrl = baseUrl.TrimEnd('/');
            PlayerToken = playerToken ?? string.Empty;
        }

        public void SetToken(string token) => PlayerToken = token ?? string.Empty;

        // --- Resources: GET bounding box ---
        public async Task<Models.ResourceNodeDto[]> GetResourcesAsync(double minLat, double minLng, double maxLat, double maxLng)
        {
            string F(double d) => d.ToString("F6", CultureInfo.InvariantCulture);

            var url = $"{BaseUrl}/api/resources" +
                      $"?minLat={F(minLat)}&minLng={F(minLng)}&maxLat={F(maxLat)}&maxLng={F(maxLng)}";

            using var req = UnityWebRequest.Get(url);
            AddCommonHeaders(req);
            var res = await SendAsync(req);

            if (!IsSuccess(res))
            {
                Debug.LogWarning($"[GpsApiClient] GET /resources failed: {res.responseCode} {res.error}");
                return Array.Empty<Models.ResourceNodeDto>();
            }

            var json = res.downloadHandler.text ?? "[]";
            var wrapped = "{\"items\":" + json + "}";
            var list = JsonUtility.FromJson<Models.ResourceNodeList>(wrapped);
            return list?.items ?? Array.Empty<Models.ResourceNodeDto>();
        }



// --- Collect: POST /api/resources/{id}/collect ---
public async Task<Models.CollectResultDto> CollectAsync(string nodeId, double playerLat, double playerLng, int amount, string playerId)
{
    var url = $"{BaseUrl}/api/resources/{nodeId}/collect";

    // Server verlangt 1..50
    amount = Math.Clamp(amount, 1, 50);

    var payload = new Models.CollectRequestDto
    {
        playerId = playerId,          // WICHTIG: Guid als String
        playerLatitude = playerLat,
        playerLongitude = playerLng,
        amount = amount
    };

    // <-- KEIN Envelope! Direkt das DTO senden.
    var json = JsonUtility.ToJson(payload);
    var body = Encoding.UTF8.GetBytes(json);

    using var req = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
    req.uploadHandler = new UploadHandlerRaw(body);
    req.downloadHandler = new DownloadHandlerBuffer();
    req.SetRequestHeader("Content-Type", "application/json");
    req.SetRequestHeader("Accept", "application/json");
    AddCommonHeaders(req);

    var res = await SendAsync(req);

    var status = res.responseCode;
    var bodyText = res.downloadHandler != null ? res.downloadHandler.text : null;
    Debug.Log($"[GpsApiClient] POST /collect → {status}, body: {bodyText}");

    Models.CollectResultDto parsed = null;
    if (!string.IsNullOrEmpty(bodyText))
    {
        try { parsed = JsonUtility.FromJson<Models.CollectResultDto>(bodyText); }
        catch (Exception e) { Debug.LogWarning($"[GpsApiClient] Collect: JSON parse failed: {e.Message}"); }
    }

    if (parsed != null)
    {
        if (string.IsNullOrWhiteSpace(parsed.reason))
        {
            parsed.reason = status switch
            {
                400 => "bad_request",
                401 => "unauthorized",
                404 => "not_found",
                429 => "cooldown",
                _   => "error"
            };
        }
        return parsed;
    }


    return new Models.CollectResultDto
    {
        success = false,
        reason = status switch
        {
            400 => "bad_request",
            401 => "unauthorized",
            404 => "not_found",
            429 => "cooldown",
            _   => "error"
        }
    };
}



        // --- Helpers ---
        private void AddCommonHeaders(UnityWebRequest req)
        {
            if (!string.IsNullOrEmpty(PlayerToken))
                req.SetRequestHeader("X-Player-Token", PlayerToken);
        }

        private static bool IsSuccess(UnityWebRequest req)
            => req.result == UnityWebRequest.Result.Success && req.responseCode is >= 200 and < 300;

        private static Task<UnityWebRequest> SendAsync(UnityWebRequest req)
        {
            var tcs = new TaskCompletionSource<UnityWebRequest>();
            var op = req.SendWebRequest();
            op.completed += _ => tcs.TrySetResult(req);
            return tcs.Task;
        }
    }
}
