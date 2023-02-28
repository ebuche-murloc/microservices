using AutoMapper;
using CommandsService.Data;
using CommandsService.Dtos;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.Design;
using CommandsService.Models;

namespace CommandsService.Controllers;

[Route("api/c/platforms/{platformId}/[controller]")]
[ApiController]
public class CommandsController : ControllerBase
{
    private readonly ICommandRepo _repository;
    private readonly IMapper _mapper;

    public CommandsController(ICommandRepo repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    [HttpGet]
    public ActionResult<IEnumerable<CommandReadDto>> GetCommandsForPlatform(int platformId)
    {
        Console.WriteLine($"-- Getting all commands for platform with id {platformId}");

        if (!_repository.PlatformExists(platformId))
        {
            return NotFound();
        }

        var commands = _repository.GetCommandsForPlatform(platformId);
        return Ok(_mapper.Map<IEnumerable<CommandReadDto>>(commands));
    }

    [HttpGet("{commandId}", Name = "GetCommandForPlatform")]
    public ActionResult<CommandReadDto> GetCommandForPlatform(int platformId, int commandId)
    {
        Console.WriteLine($"-- Getting command for platform with id {platformId} and command id {commandId}");

        if (!_repository.PlatformExists(platformId))
        {
            return NotFound();
        }

        var command = _repository.GetCommand(platformId, commandId);

        if (command == null)
        {
            return NotFound();
        }

        return Ok(_mapper.Map<CommandReadDto>(command));
    }

    [HttpPost]
    public ActionResult<CommandReadDto> CreateCommandForPlatform(int platformId, CommandCreateDto commandDto)
    {
        Console.WriteLine($"-- Creating command for platform with id {platformId}");

        if (!_repository.PlatformExists(platformId))
        {
            return NotFound();
        }

        var command = _mapper.Map<Command>(commandDto);

        _repository.CreateCommand(platformId, command);
        _repository.SaveChanges();

        var commandReadDto = _mapper.Map<CommandReadDto>(command);

        return CreatedAtRoute(nameof(GetCommandForPlatform), new {platformId = platformId, commandId = commandReadDto.Id}, commandReadDto);
    }
}