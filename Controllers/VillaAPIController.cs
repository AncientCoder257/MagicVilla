using System.Net;
using AutoMapper;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace MagicVilla_VillaAPI.Controllers;

[Route("api/VillaApi")]
[ApiController]
public class VillaAPIController : ControllerBase
{
    private readonly ILogger<VillaAPIController> _logger;
    private readonly IVillaRepository _villaRepository;
    private readonly IMapper _mapper;
    protected ApiResponse _response;

    public VillaAPIController(
        ILogger<VillaAPIController> logger,
        IVillaRepository villaRepository, 
        IMapper mapper
        )
    {
        _logger = logger;
        _villaRepository = villaRepository;
        _mapper = mapper;
        this._response = new();
    }
    
    [HttpGet("GetVillas")]
    [ProducesResponseType(typeof(VillaDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse>> GetVillas()
    {
        try
        {
            IEnumerable<Villa> villaList = await _villaRepository.GetAllAsync();
            _response.Result = _mapper.Map<List<VillaDto>>(villaList);
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(_response);
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.ErrorMessages = new List<string>() { ex.ToString() };
        }

        return _response;
    }
    
    [HttpGet("{id:int}", Name = "GetVilla")]
    [ProducesResponseType(typeof(VillaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse?>> GetVilla(int id)
    {
        try
        {
            if (id == 0)
            {
                _logger.LogError("Get Villa Error with Id: " + id);
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }
            var villa = await _villaRepository.GetAsync(v => v.Id == id);
            if (villa == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                return NotFound(_response);
            }
            _response.Result = _mapper.Map<VillaDto>(villa);
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(_response);
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.ErrorMessages = new List<string>() { ex.ToString() };
        }

        return _response;
        
    }

    [HttpPost("CreateVilla")]
    [ProducesResponseType(typeof(VillaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse>> CreateVilla([FromBody]VillaCreateDto? createDto)
    {
        try
        {
            var villaName = await _villaRepository.GetAsync(v => v.Name.ToLower() == createDto.Name.ToLower());
            if (villaName != null)
            {
                ModelState.AddModelError("ErrorMessages", "Villa already Exists!");
                return BadRequest(ModelState);
            }
            if (createDto == null)
            {
                return BadRequest(createDto);
            }

            Villa villa = _mapper.Map<Villa>(createDto);
            
            await _villaRepository.CreateAsync(villa);

            _response.Result = _mapper.Map<VillaDto>(villa);
            _response.StatusCode = HttpStatusCode.Created;
            return CreatedAtRoute("GetVilla", new {id = villa.Id},_response);
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.ErrorMessages = new List<string>() { ex.ToString() };
        }

        return _response;
    }

    [HttpDelete("{id:int}", Name = "DeleteVilla")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> DeleteVilla(int id)
    {
        try
        {

            if (id == 0)
            {
                return BadRequest();
            }

            var villa = await _villaRepository.GetAsync(v => v.Id == id);
            if (villa == null)
            {
                return NotFound();
            }

            await _villaRepository.RemoveAsync(villa);
            _response.StatusCode = HttpStatusCode.NoContent;
            _response.IsSuccess = true;
            return Ok(_response);
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.ErrorMessages = new List<string>() { ex.ToString() };
        }

        return _response;
    }

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> UpdateVilla(int id, [FromBody] VillaUpdateDto? updateDto)
    {
        try
        {
            if (updateDto == null || id != updateDto.Id)
            {
                return BadRequest();
            }

            Villa model = _mapper.Map<Villa>(updateDto);

            await _villaRepository.UpdateAsync(model);

            _response.StatusCode = HttpStatusCode.NoContent;
            _response.IsSuccess = true;
            return Ok(_response);
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.ErrorMessages = new List<string>() { ex.ToString() };
        }

        return _response;
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

        var villa = await _villaRepository.GetAsync(v => v.Id == id, tracked:false);

        VillaUpdateDto villaUpdateDto = _mapper.Map<VillaUpdateDto>(villa);
        
        if(villa == null)
        {
            return BadRequest();
        }
        patchDto.ApplyTo(villaUpdateDto, ModelState);

        Villa model = _mapper.Map<Villa>(villaUpdateDto);
        
        await _villaRepository.UpdateAsync(model);
        
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        return NoContent();
    }
}