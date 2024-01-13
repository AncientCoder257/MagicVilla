using System.Net;
using AutoMapper;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;

namespace MagicVilla_VillaAPI.Controllers;

[Route("[controller]")]
[ApiController]
public class VillaNumberApiController : ControllerBase
{
    private readonly ILogger<VillaAPIController> _logger;
    private readonly IVillaNumberRepository _villaNumberRepository;
    private readonly IVillaRepository _villaRepository;
    private readonly IMapper _mapper;
    protected ApiResponse _response;

    public VillaNumberApiController(
        ILogger<VillaAPIController> logger,
        IVillaNumberRepository villaNumberRepository, 
        IVillaRepository villaRepository,
        IMapper mapper
        )
    {
        _logger = logger;
        _villaNumberRepository = villaNumberRepository;
        _villaRepository = villaRepository;
        _mapper = mapper;
        this._response = new();
    }

    [HttpGet("GetVillaNumbers")]
    [ProducesResponseType(typeof(VillaDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse>> GetVillaNumbers()
    {
        try
        {
            IEnumerable<VillaNumber> villaNumbersList = await _villaNumberRepository.GetAllAsync();
            _response.Result = _mapper.Map<List<VillaNumberDto>>(villaNumbersList);
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
    
    [HttpGet("{id:int}", Name = "GetVillaNumber")]
    [ProducesResponseType(typeof(VillaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse?>> GetVillaNumber(int id)
    {
        try
        {
            if (id == 0)
            {
                _logger.LogError("Get Villa Error with Id: " + id);
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }
            var villaNumber = await _villaNumberRepository.GetAsync(v => v.VillaNo == id);
            if (villaNumber == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                return NotFound(_response);
            }
            _response.Result = _mapper.Map<VillaNumberDto>(villaNumber);
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
    [HttpPost("CreateVillaNumber")]
    [ProducesResponseType(typeof(VillaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse>> CreateVillaNumber([FromBody]VillaNumberCreateDto? createDto)
    {
        try
        {
            var villaNumberExist = await _villaNumberRepository
                .GetAsync(v => v.VillaNo == createDto.VillaNo);
            if (villaNumberExist != null)
            {
                ModelState.AddModelError("", "Villa Number already Exists!");
                return BadRequest(ModelState);
            }

            var villa = await _villaRepository
                .GetAsync(v => v.Id == createDto.VillaId);
            if (villa == null)
            {
                ModelState.AddModelError("Invalid Id", "Villa Id is Invalid!");
                return BadRequest(ModelState);
            }
            
            if (createDto == null)
            {
                return BadRequest(createDto);
            }

            VillaNumber villaNumber = _mapper.Map<VillaNumber>(createDto);
            
            await _villaNumberRepository.CreateAsync(villaNumber);

            _response.Result = _mapper.Map<VillaNumberDto>(villaNumber);
            _response.StatusCode = HttpStatusCode.Created;
            return CreatedAtRoute("GetVillaNumber", new {id = villaNumber.VillaNo},_response);
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.ErrorMessages = new List<string>() { ex.ToString() };
        }

        return _response;
    }
    
    [HttpDelete("{id:int}", Name = "DeleteVillaNumber")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> DeleteVillaNumber(int id)
    {
        try
        {
            if (id == 0)
            {
                return BadRequest();
            }

            var villaNumber = await _villaNumberRepository.GetAsync(v => v.VillaNo == id);
            if (villaNumber == null)
            {
                return NotFound();
            }

            await _villaNumberRepository.RemoveAsync(villaNumber);
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> UpdateVillaNumber(
            int id, 
            [FromBody] VillaNumberUpdateDto? updateDto
        )
    {
        try
        {
            if (updateDto == null || id != updateDto.VillaNo)
            {
                return BadRequest();
            }
            
            var villa = await _villaRepository
                .GetAsync(v => v.Id == updateDto.VillaId);
            if (villa == null)
            {
                ModelState.AddModelError("Invalid Id", "Villa Id is Invalid!");
                return BadRequest(ModelState);
            }

            VillaNumber model = _mapper.Map<VillaNumber>(updateDto);

            await _villaNumberRepository.UpdateAsync(model);

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
}