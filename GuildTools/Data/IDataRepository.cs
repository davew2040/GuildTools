using System;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.Data
{
    public interface IDataRepository
    {
        Task<string> SimpleGetAsync();
    }
}
