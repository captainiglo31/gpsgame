// Assets/Scripts/GpsGame/Net/Models.cs
using System;
using UnityEngine;

namespace GpsGame.Net.Models
{
    // Wrapper für Array-Parsing (JsonUtility kann kein Top-Level-Array)
    [Serializable]
    public sealed class ResourceNodeList
    {
        public ResourceDto[] items;
    }

  

    [Serializable]
    public sealed class CollectRequestDto
    {
        public string playerId;       
        public double playerLatitude;
        public double playerLongitude;
        public int amount;
    }

    [Serializable]
    public sealed class CollectResultDto
    {
        public bool success;
        public string reason;        // "cooldown","too_far","respawning","not_found","disabled","depleted_or_race","unauthorized",...
        public int collected;
        public int remaining;
        public string respawnAtUtc;  // optional/null → leere string
    }
    
    [Serializable]
    public sealed class ResourceDto
    {
        public string id;           // Guid als string ist für JsonUtility am stressfreiesten
        public double latitude;
        public double longitude;
        public int amount;
        public int maxAmount;
        public string type;
        public string respawnAtUtc; // als string reicht (nicht nötig fürs Rendering)
    }
}