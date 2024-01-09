using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MagicVilla_VillaAPI.Controllers;

[Route("[controller]")]
[ApiController]
public class VillaAPIController : ControllerBase
{
    private readonly ILogger<VillaAPIController> _logger;
    private readonly ApplicationDbContext _db;

    public VillaAPIController(ILogger<VillaAPIController> logger, ApplicationDbContext db)
    {
        _logger = logger;
        _db = db;
    }
    
    [HttpGet("GetVillas")]
    [ProducesResponseType(typeof(VillaDTO), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<VillaDTO>>> GetVillas()
    {
        return Ok(await _db.Villas.ToListAsync());
    }
    
    [HttpGet("{id:int}", Name = "GetVilla")]
    [ProducesResponseType(typeof(VillaDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VillaDTO?>> GetVilla(int id)
    {
        if (id == 0)
        {
            _logger.LogError("Get Villa Error with Id: " + id);
            return BadRequest();
        }
        var villa = await _db.Villas
            .FirstOrDefaultAsync(v => v.Id == id);
        if (villa == null)
        {
            return NotFound();
        }
        return Ok(villa);
    }

    [HttpPost("CreateVilla")]
    [ProducesResponseType(typeof(VillaDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<VillaDTO>> CreateVilla([FromBody]VillaCreateDto? villaDTO)
    {
        var villaName = await _db.Villas?
            .FirstOrDefaultAsync(v => v.Name.ToLower() == villaDTO.Name.ToLower());
        if (villaName != null)
        {
            ModelState.AddModelError("", "Villa already Exists!");
            return BadRequest(ModelState);
        }
        if (villaDTO == null)
        {
            return BadRequest();
        }

        /*if (villaDTO.Id > 0)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }*/
        
        Villa model = new ()
        {
            Name = villaDTO.Name,
            Details = villaDTO.Details,
            Rate = villaDTO.Rate,
            Sqm = villaDTO.Sqm,
            Occupancy = villaDTO.Occupancy,
            ImageUrl = villaDTO.ImageUrl,
            Amenity = villaDTO.Amenity,
        };
        await _db.Villas.AddAsync(model);
        await _db.SaveChangesAsync();

        return CreatedAtRoute("GetVilla", new {id = model.Id},model);
    }

    [HttpDelete("{id:int}", Name = "DeleteVilla")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteVilla(int id)
    {
        if (id == 0)
        {
            return BadRequest();
        }

        var villa = await _db.Villas
            .FirstOrDefaultAsync(v => v.Id == id);
        if (villa == null)
        {
            return NotFound();
        }

        _db.Villas.Remove(villa);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateVilla(int id, [FromBody] VillaUpdateDto? villaDto)
    {
        if (villaDto == null || id != villaDto.Id)
        {
            return BadRequest();
        }

        Villa model = new ()
        {
            Id = villaDto.Id,
            Name = villaDto.Name,
            Details = villaDto.Details,
            Rate = villaDto.Rate,
            Sqm = villaDto.Sqm,
            Occupancy = villaDto.Occupancy,
            ImageUrl = villaDto.ImageUrl,
            Amenity = villaDto.Amenity,
        };
        _db.Villas.Update(model);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpPatch("{id:int}", Name = "UpdatePartialVilla")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdatePartialVilla(int id, JsonPatchDocument<VillaUpdateDto>? patchDto)
    {
        //https://jsonpatch.com/ 
        //https://learn.microsoft.com/en-us/aspnet/core/web-api/jsonpatch?view=aspnetcore-8.0
        if (patchDto == null || id == 0)
        {
            return BadRequest();
        }

        var villa = await _db.Villas
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == id);
        
        VillaUpdateDto villaDto = new ()
        {
            Id = villa.Id,
            Name = villa.Name,
            Details = villa.Details,
            Rate = villa.Rate,
            Sqm = villa.Sqm,
            Occupancy = villa.Occupancy,
            ImageUrl = villa.ImageUrl,
            Amenity = villa.Amenity,
        };
        
        if(villa == null)
        {
            return BadRequest();
        }
        patchDto.ApplyTo(villaDto, ModelState);
        
        Villa model = new ()
        {
            Id = villaDto.Id,
            Name = villaDto.Name,
            Details = villaDto.Details,
            Rate = villaDto.Rate,
            Sqm = villaDto.Sqm,
            Occupancy = villaDto.Occupancy,
            ImageUrl = villaDto.ImageUrl,
            Amenity = villaDto.Amenity,
        };

        _db.Villas.Update(model);
        await _db.SaveChangesAsync();
        
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        return NoContent();
    }
}