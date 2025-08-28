using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GpsGame.Application.Inventory;
using GpsGame.Application.Players;
using GpsGame.Domain.Entities;
using GpsGame.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GpsGame.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class PlayersController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IInventoryService _inventory;
        public PlayersController(AppDbContext db, IInventoryService inventory)  
        {
            _db = db;
            _inventory = inventory;
        }

        /// <summary>Create a new player.</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PlayerCreateDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var p = new Player
            {
                Id = Guid.NewGuid(),
                Username = dto.Username.Trim(),
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                CreatedUtc = DateTime.UtcNow
            };

            _db.Players.Add(p);
            await _db.SaveChangesAsync(ct);

            return CreatedAtRoute(nameof(GetById), new { id = p.Id }, new
            {
                p.Id,
                p.Username,
                p.Latitude,
                p.Longitude,
                p.CreatedUtc
            });
        }

        /// <summary>Get a player by id.</summary>
        [HttpGet("{id:guid}", Name = nameof(GetById))]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            var p = await _db.Players.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
            if (p is null) return NotFound();
            return Ok(new { p.Id, p.Username, p.Latitude, p.Longitude, p.CreatedUtc });
        }

        /// <summary>List recent players (max 50).</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var list = await _db.Players.AsNoTracking()
                .OrderByDescending(p => p.CreatedUtc)
                .Take(50)
                .Select(p => new { p.Id, p.Username, p.Latitude, p.Longitude, p.CreatedUtc })
                .ToListAsync(ct);

            return Ok(list);
        }
        
        /// <summary>Aggregated inventory by ResourceType for a player.</summary>
        [HttpGet("{id:guid}/inventory")]
        public async Task<IActionResult> GetInventory(Guid id, CancellationToken ct)
        {
            var aggregated = await _inventory.GetAggregatedByPlayerAsync(id, ct);
            return Ok(aggregated);
        }
    }
}
