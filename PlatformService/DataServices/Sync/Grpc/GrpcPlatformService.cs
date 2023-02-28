using AutoMapper;
using Grpc.Core;
using PlatformService.Data;

namespace PlatformService.DataServices.Sync.Grpc;

public class GrpcPlatformService : GrpcPlatform.GrpcPlatformBase
{
    private readonly IPlatformRepo _repository;
    private readonly IMapper _mapper;

    public GrpcPlatformService(IPlatformRepo repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public override Task<PlatformResponse> GetAllPlatforms(GetAllRequest request, ServerCallContext context)
    {
        var platformResponse = new PlatformResponse();
        var platforms = _repository.GetAllPlatforms();

        foreach (var platform in platforms)
        {
            platformResponse.Platform.Add(_mapper.Map<GrpcPlatformModel>(platform));
        }

        return Task.FromResult(platformResponse);
    }
}