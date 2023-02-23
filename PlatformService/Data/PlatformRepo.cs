using PlatformService.Models;

namespace PlatformService.Data;

public class PlatformRepo : IPlatformRepo
{
    private readonly AppDbContext _context;

    public PlatformRepo(AppDbContext context)
    {
        _context = context;
    }

    public bool SaveChanges()
    {
        //if _context.SaveChanges returns something greater that zero, than it means we have some changes
        return _context.SaveChanges() >= 0;
    }

    public IEnumerable<Platform> GetAllPlatforms()
    {
        //maybe better return by batches 
        return _context.Platforms.ToList();
    }

    public Platform GetPlatformById(int id)
    {
        return _context.Platforms.FirstOrDefault(p => p.Id == id); 
    }

    public void CreatePlatform(Platform plat)
    {
        if (plat is null)
            throw new ApplicationException("the string is null"); 
        _context.Platforms.Add(plat);
    }
}