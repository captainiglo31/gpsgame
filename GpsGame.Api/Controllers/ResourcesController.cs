using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GpsGame.Application.FeatureFlags;
using GpsGame.Application.Inventory;
using GpsGame.Application.Resources;
using GpsGame.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;

namespace GpsGame.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResourcesController : ControllerBase
    {
        private readonly IResourceQuery _resourceQuery;
        private readonly IFeatureFlagReader _flags;
        private readonly IResourceCollector _collector;
        private readonly AppDbContext _db;
        private readonly IInventoryService _inventory;
        

        public ResourcesController(IResourceQuery resourceQuery, IFeatureFlagReader flags, IResourceCollector collector,  AppDbContext db, IInventoryService inventory)
        {
            _resourceQuery = resourceQuery;
            _flags = flags;
            _collector = collector;
            _db = db;
            _inventory = inventory;
        }

        /// <summary>
        /// Gets resource nodes within the given bounding box.
        /// </summary>
        /// <param name="minLat">Minimum latitude (-90 to 90).</param>
        /// <param name="minLng">Minimum longitude (-180 to 180).</param>
        /// <param name="maxLat">Maximum latitude (-90 to 90).</param>
        /// <param name="maxLng">Maximum longitude (-180 to 180).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>List of resource nodes.</returns>
        [HttpGet]
        public async Task<IActionResult> GetResources(
            [FromQuery] double minLat,
            [FromQuery] double minLng,
            [FromQuery] double maxLat,
            [FromQuery] double maxLng,
            CancellationToken ct)
        {
            // Feature flag gating
            var enabled = await _flags.IsEnabledAsync("resources_enabled", ct);
            if (enabled != true)
            {
                return NotFound();
            }

            // Validate bounds
            if (minLat < -90 || maxLat > 90 || minLng < -180 || maxLng > 180)
            {
                return BadRequest("Latitude must be between -90 and 90, longitude between -180 and 180.");
            }

            if (minLat >= maxLat || minLng >= maxLng)
            {
                return BadRequest("Minimum latitude/longitude must be less than maximum latitude/longitude.");
            }

            var resources = await _resourceQuery.GetByBoundingBoxAsync(minLat, minLng, maxLat, maxLng, ct);
            return Ok(resources);
        }
        
        /// <summary>
        /// Collects resources from a specific node if within range and not respawning.
        /// </summary>
        /// <param name="id">Resource node id.</param>
        /// <param name="request">Collect request containing player position and requested amount.</param>
        /// <returns>Collect result with collected amount, remaining and optional respawn time.</returns>
        [HttpPost("{id:guid}/collect")]
        [ProducesResponseType(typeof(CollectResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CollectAsync([FromRoute] Guid id, [FromBody] CollectRequestDto request, CancellationToken ct)
        {
            if (!(await _flags.IsEnabledAsync("resources_enabled", ct) ?? false))
                return NotFound();
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await using var tx = await _db.Database.BeginTransactionAsync(ct);

            var result = await _collector.CollectAsync(id, request, ct);

            if (!result.Success)
            {
                await tx.RollbackAsync(ct);
                return result.Reason switch
                {
                    "unauthorized"        => Unauthorized(),
                    "disabled"            => NotFound(),
                    "not_found"           => NotFound(),
                    "respawning"          => BadRequest(result),
                    "too_far"             => BadRequest(result),
                    "depleted_or_race"    => BadRequest(result),
                    "cooldown"            => StatusCode(StatusCodes.Status429TooManyRequests, result),
                    _                     => BadRequest(result)
                };
            }

            // WICHTIG: result muss PlayerId, ResourceType, Collected liefern (siehe unten).
            var amountToAdd = result.Collected;
            if (amountToAdd > 0)
                await _inventory.IncrementAsync(result.PlayerId, result.ResourceType, amountToAdd, ct);

            // Node-Änderungen + Inventar sind im selben DbContext -> jetzt persistieren + committen
            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            return Ok(result);
        }

    }
}