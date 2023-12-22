using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models.Dto;
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
    public ActionResult<VillaDTO> CreateVilla([FromBody]VillaDTO villaDTO)
    {
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
        VillaStore.villaList.Add(villaDTO);

        return CreatedAtRoute("GetVilla", new {id = villaDTO.Id},villaDTO);
    }
}