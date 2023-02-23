using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PlatformService.Data;
using PlatformService.Dtos;
using PlatformService.Models;
using PlatformService.SyncDataServices.Http;

namespace PlatformService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PlatformsController : ControllerBase
{
    private readonly IPlatformRepo _repository;
    private readonly IMapper _mapper;
    private readonly ICommandDataClient _commandDataClient;

    public PlatformsController(
        IPlatformRepo repository, 
        IMapper mapper, 
        ICommandDataClient commandDataClient)
    {
        _repository = repository;
        _mapper = mapper;
        _commandDataClient = commandDataClient;
    }

    [HttpGet]
    public ActionResult<IEnumerable<PlatformReadDto>> GetPlatforms()
    {
        Console.WriteLine("Getting platforms....");

        var platformItems = _repository.GetAllPlatforms();

        return Ok(_mapper.Map<IEnumerable<PlatformReadDto>>(platformItems));
    }

    [HttpGet("{id}", Name = "GetPlatformById")]
    public ActionResult<PlatformReadDto> GetPlatformById(int id)
    {
        Console.WriteLine("Getting one platform....");

        var platformItem = _repository.GetPlatformById(id);
        if (platformItem is not null)
            return Ok(_mapper.Map<PlatformReadDto>(platformItem));

        return NotFound();
    }

    [HttpPost]
    public async Task<ActionResult<PlatformReadDto>> CreatePlatform(PlatformCreateDto platformCreateDto)
    {

        var platformItem = _mapper.Map<Platform>(platformCreateDto);
        //непонятным образом генерируется id, наверно дело в DataSet Add
        _repository.CreatePlatform(platformItem);
        _repository.SaveChanges();

        var platformReadDto = _mapper.Map<PlatformReadDto>(platformItem);

        try
        {
            await _commandDataClient.SendPlatformToCommand(platformReadDto);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"-- Couldn't send synchronously: {ex.Message}");
        }

        return CreatedAtRoute(nameof(GetPlatformById), new{ Id = platformReadDto.Id}, platformReadDto);
    }

}