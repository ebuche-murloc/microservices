using PlatformService.Dtos;

namespace PlatformService.DataServices.Sync.Http
{
    public interface ICommandDataClient
    {
        Task SendPlatformToCommand(PlatformReadDto plat);
    }
}
