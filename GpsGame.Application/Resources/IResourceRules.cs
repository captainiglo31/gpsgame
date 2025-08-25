namespace GpsGame.Application.Resources;

public interface IResourceRules
{
    /// <summary>Returns respawn minutes for a given resource type (case-insensitive).</summary>
    int GetRespawnMinutes(string resourceType);
}