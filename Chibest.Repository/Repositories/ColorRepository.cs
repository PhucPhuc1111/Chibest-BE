using Chibest.Repository.Base;
using Chibest.Repository.Interface;
using Chibest.Repository.Models;

namespace Chibest.Repository.Repositories;

public class ColorRepository : GenericRepository<Color>, IColorRepository
{
    public ColorRepository(ChiBestDbContext context) : base(context) { }
}

