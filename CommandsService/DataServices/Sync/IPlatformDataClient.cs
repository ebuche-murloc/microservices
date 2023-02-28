using CommandsService.Models;

namespace CommandsService.DataServices.Sync;

public interface IPlatformDataClient
{
    IEnumerable<Platform> ReturnAllPlatforms();
}