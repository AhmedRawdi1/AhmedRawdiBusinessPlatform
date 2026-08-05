using AhmedRawdiBusinessPlatform.Data;
using AhmedRawdiBusinessPlatform.Models;
using Microsoft.EntityFrameworkCore;

namespace AhmedRawdiBusinessPlatform.Services
{
    public class GroupService : IGroupService
    {
        private readonly ApplicationDbContext _context;

        public GroupService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<GroupListItemDto>> GetAllGroupsAsync()
        {
            return await _context.Database
                .SqlQueryRaw<GroupListItemDto>("EXEC dbo.usp_Get_AllGroups")
                .ToListAsync();
        }
    }
}
