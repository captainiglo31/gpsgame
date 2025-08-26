// Assets/Scripts/GpsGame/Net/GpsApiClient.cs
using System;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using UnityEngine;
using UnityEngine.Networking;
using GpsGame.Net.Models;


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
        [Serializable]
        private sealed class ResourceArrayWrapper { public ResourceDto[] items; }
        // bypass https local
        private sealed class DevCertBypass : CertificateHandler
        {
            protected override bool ValidateCertificate(byte[] certificateData) => true;
        }
        
        public GpsApiClient(string baseUrl, string playerToken)
        {
            BaseUrl = baseUrl.TrimEnd('/');
            PlayerToken = playerToken ?? string.Empty;
        }

        public void SetToken(string token) => PlayerToken = token ?? string.Empty;

        public async Task<ResourceDto[]> GetResourcesAsync(double minLat, double minLng, double maxLat, double maxLng)
        {
            var url =
                $"{BaseUrl}/api/resources?minLat={minLat.ToString(CultureInfo.InvariantCulture)}&minLng={minLng.ToString(CultureInfo.InvariantCulture)}&maxLat={maxLat.ToString(CultureInfo.InvariantCulture)}&maxLng={maxLng.ToString(CultureInfo.InvariantCulture)}";

            using var req = UnityWebRequest.Get(url);
            req.SetRequestHeader("X-Player-Token", PlayerToken);

            #if UNITY_EDITOR
            req.certificateHandler = new DevCertBypass();
            #endif
            
            var op = req.SendWebRequest();
            while (!op.isDone) await Task.Yield();

            if (req.result != UnityWebRequest.Result.Success)
                throw new Exception($"HTTP {req.responseCode}: {req.error}");

            var json = req.downloadHandler.text;
            Debug.Log($"[API] GET resources -> {url}\nJSON: {json}");

            // Top-Level-Array via Wrapper für JsonUtility
            var wrapped = $"{{\"items\":{json}}}";
            var arr = JsonUtility.FromJson<ResourceArrayWrapper>(wrapped)?.items;
            return arr ?? Array.Empty<ResourceDto>();
        }

        // --- Collect: POST /api/resources/{id}/collect ---
        public async Task<Models.CollectResultDto> CollectAsync(string nodeId, double playerLat, double playerLng, int amount, string playerId)
        {
            var url = $"{BaseUrl}/api/resources/{nodeId}/collect";
            amount = Math.Clamp(amount, 1, 50);

            var payload = new Models.CollectRequestDto
            {
                playerId = playerId,
                playerLatitude = playerLat,
                playerLongitude = playerLng,
                amount = amount
            };

            var json = JsonUtility.ToJson(payload);
            var body = Encoding.UTF8.GetBytes(json);

            using var req = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST)
            {
                uploadHandler = new UploadHandlerRaw(body),
                downloadHandler = new DownloadHandlerBuffer()
            };
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
            
            #if UNITY_EDITOR
            req.certificateHandler = new DevCertBypass();
            #endif
            
            var op = req.SendWebRequest();
            op.completed += _ => tcs.TrySetResult(req);
            return tcs.Task;
        }
    }
    
}
