using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using GpsGame.Application.FeatureFlags;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GpsGame.Api.Controllers
{
    /// <summary>
    /// Read-only access to feature flags for clients and tools.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces(MediaTypeNames.Application.Json)]
    public sealed class FeatureFlagsController : ControllerBase
    {
        private readonly IFeatureFlagReader _reader;

        public FeatureFlagsController(IFeatureFlagReader reader)
        {
            _reader = reader;
        }

        /// <summary>
        /// Returns all feature flags as key/enabled pairs.
        /// </summary>
        /// <returns>List of feature flags.</returns>
        /// <response code="200">List returned.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<object>>> GetAll(CancellationToken ct)
        {
            var items = await _reader.GetAllAsync(ct);
            var result = items.Select(x => new { key = x.Key, enabled = x.Enabled });
            return Ok(result);
        }

        /// <summary>
        /// Returns a single feature flag by key.
        /// </summary>
        /// <param name="key">Feature flag key.</param>
        /// <returns>Flag object or 404.</returns>
        /// <response code="200">Flag found.</response>
        /// <response code="404">Flag not found.</response>
        [HttpGet("{key}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<object>> GetByKey(string key, CancellationToken ct)
        {
            var enabled = await _reader.IsEnabledAsync(key, ct);
            if (enabled is null)
                return NotFound();

            return Ok(new { key, enabled = enabled.Value });
        }
    }
}
