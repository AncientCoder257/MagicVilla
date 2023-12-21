using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
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
    
    [HttpGet("GetVillas/{id}")]
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
}