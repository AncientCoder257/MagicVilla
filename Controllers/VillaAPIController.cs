using AutoMapper;
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
    private readonly IMapper _mapper;

    public VillaAPIController(ILogger<VillaAPIController> logger, ApplicationDbContext db, IMapper mapper)
    {
        _logger = logger;
        _db = db;
        _mapper = mapper;
    }
    
    [HttpGet("GetVillas")]
    [ProducesResponseType(typeof(VillaDTO), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<VillaDTO>>> GetVillas()
    {
        IEnumerable<Villa> villaList = await _db.Villas.ToListAsync();
        return Ok(_mapper.Map<List<VillaDTO>>(villaList));
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
        return Ok(_mapper.Map<VillaDTO>(villa));
    }

    [HttpPost("CreateVilla")]
    [ProducesResponseType(typeof(VillaDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<VillaDTO>> CreateVilla([FromBody]VillaCreateDto? createDto)
    {
        var villaName = await _db.Villas?
            .FirstOrDefaultAsync(v => v.Name.ToLower() == createDto.Name.ToLower());
        if (villaName != null)
        {
            ModelState.AddModelError("", "Villa already Exists!");
            return BadRequest(ModelState);
        }
        if (createDto == null)
        {
            return BadRequest(createDto);
        }

        Villa model = _mapper.Map<Villa>(createDto);
        
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
    public async Task<IActionResult> UpdateVilla(int id, [FromBody] VillaUpdateDto? updateDto)
    {
        if (updateDto == null || id != updateDto.Id)
        {
            return BadRequest();
        }

        Villa model = _mapper.Map<Villa>(updateDto);
        
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

        VillaUpdateDto villaUpdateDto = _mapper.Map<VillaUpdateDto>(villa);
        
        if(villa == null)
        {
            return BadRequest();
        }
        patchDto.ApplyTo(villaUpdateDto, ModelState);

        Villa model = _mapper.Map<Villa>(villaUpdateDto);
        
        _db.Villas.Update(model);
        await _db.SaveChangesAsync();
        
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        return NoContent();
    }
}