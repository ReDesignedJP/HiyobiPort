using Api.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;

namespace Api.Lib
{
    public static class SessionLib
    {
        public static async Task<User> GetUser(this ISession session)
        {
            await session.LoadAsync();
            byte[] data = session.Get("user");
            if (data == null) return null;

            return JsonConvert.DeserializeObject<User>(Encoding.UTF8.GetString(data));
        }

        public static async Task SetUser(this ISession session, User user)
        {
            await session.LoadAsync();
            if (user == null)
            {
                session.Remove("user");
            }
            else
            {
                session.SetString("user", JsonConvert.SerializeObject(user));
            }
            await session.CommitAsync();
        }
    }
}
