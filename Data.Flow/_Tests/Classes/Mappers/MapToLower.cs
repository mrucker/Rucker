using Data.Core;

namespace Data.Flow._Tests.Classes.Mappers
{
    public class MapToLower: IMap<string, string>
    {
        public string Map(string page)
        {
            return page.ToLower();
        }
    }
}