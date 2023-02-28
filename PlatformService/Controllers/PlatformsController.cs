using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PlatformService.Data;
using PlatformService.DataServices.Async;
using PlatformService.DataServices.Sync.Http;
using PlatformService.Dtos;
using PlatformService.Models;

namespace PlatformService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PlatformsController : ControllerBase
{
    private readonly IPlatformRepo _repository;
    private readonly IMapper _mapper;
    private readonly ICommandDataClient _commandDataClient;
    private readonly IMessageBusClient _messageBusClient;

    public PlatformsController(
        IPlatformRepo repository, 
        IMapper mapper, 
        ICommandDataClient commandDataClient,
        IMessageBusClient messageBusClient)
    {
        _repository = repository;
        _mapper = mapper;
        _commandDataClient = commandDataClient;
        _messageBusClient = messageBusClient;
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
        _repository.CreatePlatform(platformItem);
        _repository.SaveChanges();

        var platformReadDto = _mapper.Map<PlatformReadDto>(platformItem);


        //send sync message
        try
        {
            await _commandDataClient.SendPlatformToCommand(platformReadDto);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"-- Couldn't send synchronously: {ex.Message}");
        }

        //send async message
        try
        {
            var platformPublishedDto = _mapper.Map<PlatformPublishedDto>(platformReadDto);
            platformPublishedDto.Event = "Platform_Published";
            _messageBusClient.PublishNewPlatform(platformPublishedDto);
        }
        catch (Exception e)
        {
            Console.WriteLine($"-- Couldn't send asynchronously: {e.Message}");
        }

        return CreatedAtRoute(nameof(GetPlatformById), new{ Id = platformReadDto.Id}, platformReadDto);
    }

}