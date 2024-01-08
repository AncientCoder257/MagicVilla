using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models.Dto;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace MagicVilla_VillaAPI.Controllers;

[Route("[controller]")]
[ApiController]
public class VillaAPIController : ControllerBase
{
    [HttpGet("GetVillas")]
    [ProducesResponseType(typeof(VillaDTO), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<VillaDTO>> GetVillas()
    {
        return Ok(VillaStore.villaList);
    }
    
    [HttpGet("{id:int}", Name = "GetVilla")]
    [ProducesResponseType(typeof(VillaDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<VillaDTO?> GetVilla(int id)
    {
        if (id == 0)
        {
            return BadRequest();
        }
        var villa = VillaStore.villaList.FirstOrDefault(v => v.Id == id);
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
    public ActionResult<VillaDTO> CreateVilla([FromBody]VillaDTO? villaDTO)
    {
        var villaName = VillaStore.villaList
            .FirstOrDefault(v => v.Name?.ToLower() == villaDTO.Name.ToLower());
        if (villaName != null)
        {
            ModelState.AddModelError("", "Villa already Exists!");
            return BadRequest(ModelState);
        }
        if (villaDTO == null)
        {
            return BadRequest();
        }

        if (villaDTO.Id > 0)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        villaDTO.Id = VillaStore.villaList.Any()
            ? VillaStore.villaList.MaxBy(v => v.Id)!.Id + 1 
            : 1;
        /* this is the given code in the course
         villaDTO.Id = VillaStore.villaList
            .OrderByDescending(v => v.Id)
            .FirstOrDefault().Id + 1;
            */
        
        VillaStore.villaList.Add(villaDTO);

        return CreatedAtRoute("GetVilla", new {id = villaDTO.Id},villaDTO);
    }

    [HttpDelete("{id:int}", Name = "DeleteVilla")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult DeleteVilla(int id)
    {
        if (id == 0)
        {
            return BadRequest();
        }

        var villa = VillaStore.villaList
            .FirstOrDefault(v => v.Id == id);
        if (villa == null)
        {
            return NotFound();
        }

        VillaStore.villaList.Remove(villa);
        return NoContent();
    }

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult UpdateVilla(int id, [FromBody] VillaDTO? villaDto)
    {
        if (villaDto == null || id != villaDto.Id)
        {
            return BadRequest();
        }

        var villa = VillaStore.villaList
            .FirstOrDefault(v => v.Id == id);
        villa.Name = villaDto.Name;
        villa.Sqm = villaDto.Sqm;
        villa.Occupancy = villaDto.Occupancy;

        return NoContent();
    }

    [HttpPatch("{id:int}", Name = "UpdatePartialVilla")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult UpdatePartialVilla(int id, JsonPatchDocument<VillaDTO>? patchDto)
    {
        //https://jsonpatch.com/ 
        //https://learn.microsoft.com/en-us/aspnet/core/web-api/jsonpatch?view=aspnetcore-8.0
        if (patchDto == null || id == 0)
        {
            return BadRequest();
        }

        var villa = VillaStore.villaList
            .FirstOrDefault(v => v.Id == id);
        if(villa == null)
        {
            return BadRequest();
        }
        patchDto.ApplyTo(villa, ModelState);
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        return NoContent();
    }
}