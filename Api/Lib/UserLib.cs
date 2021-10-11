using Api.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using Serilog;
using SqlKata.Execution;
using System;
using System.Net;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Api.Lib
{
    public class UserLib
    {
        /// <summary>
        /// 유저가 없거나 오류가 발생하면 null, false
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<(User, bool)> GetUserFromIdAsync(int id)
        {
            try
            {
                QueryFactory qf = DBLib.GetQueryBuilder();
                User data = await qf.Query("member").Where("id", id).FirstAsync<User>();

                if (data == null) //데이터가 없을때
                {
                    qf.Dispose();
                    return (null, false);
                }

                qf.Dispose();

                return (data, true);
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                return (null, false);
            }
        }

        /// <summary>
        /// 유저가 없거나 오류가 발생하면 null, false
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public static async Task<(User, bool)> GetUserFromEmailAsync(string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email)) return (null, false); //이메일이 null이거나 공백이면 false

                QueryFactory qf = DBLib.GetQueryBuilder();
                User data = await qf.Query("member").Where("email", email).FirstAsync<User>();

                if (data == null) //데이터가 없을때
                {
                    qf.Dispose();
                    return (null, false);
                }

                qf.Dispose();

                return (data, true);
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                return (null, false);
            }
        }

        public static async Task<(string, bool)> CreateUserAsync(string email, string name, string password, string ip)
        {
            try
            {
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(password) ||
                    string.IsNullOrEmpty(ip)) //4개의 값들중 하나라도 공백이거나 null이면 false
                {
                    return ("Invalid value", false);
                }

                email = email.Trim();
                name = name.Trim();

                //TODO 토르 브라우저 확인
                if (!await IsEmailUniqueAsync(email)) return ("이미 등록된 이메일입니다.", false);
                if (!await IsNameUniqueAsync(name)) return ("이미 등록된 닉네임입니다.", false);
                if (!await EmailDisifyCheckAsync(email)) return ("올바른 이메일을 입력해주세요.", false);

                if (name.Length > 12)
                {
                    return ("닉네임은 12자 내로 입력해주세요.", false);
                }

                if (Regex.IsMatch(name, "^[가-힣a-zA-Z0-9 ]+$"))
                {
                    return ("한글, 영문, 숫자를 제외한 문자는 금지됩니다.(자음, 모음만도 금지)", false);
                }

                string hashedPassword = BCrypt.Net.BCrypt.EnhancedHashPassword(password);

                QueryFactory qf = DBLib.GetQueryBuilder();
                await qf.Query("member").InsertAsync(new User(name, email, hashedPassword, false, false, DateTime.Now));

                return ("처리되었습니다.", true);
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                return ("처리중 오류가 발생했습니다.", false);
            }
        }

        private static async Task<bool> IsEmailUniqueAsync(string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email)) return false; //이메일이 null이거나 공백이면 false

                QueryFactory qf = DBLib.GetQueryBuilder();
                int data = await qf.Query("member").Where("email", email).CountAsync<int>();

                if (data != 0) //데이터가 없을때
                {
                    qf.Dispose();
                    return false;
                }

                qf.Dispose();

                return true;
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                return false;
            }
        }

        private static async Task<bool> IsNameUniqueAsync(string name)
        {
            try
            {
                if (string.IsNullOrEmpty(name)) return false; //닉네임이 null이거나 공백이면 false

                QueryFactory qf = DBLib.GetQueryBuilder();
                int data = await qf.Query("member").Where("name", name).CountAsync<int>();

                if (data != 0) //데이터가 없을때
                {
                    qf.Dispose();
                    return false;
                }

                qf.Dispose();

                return true;
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                return false;
            }
        }

        private static async Task<bool> EmailDisifyCheckAsync(string email)
        {
            try
            {
                WebClient wc = new WebClient();
                string response = await wc.DownloadStringTaskAsync($"https://disify.com/api/email/{email}");
                JObject obj = JObject.Parse(response);
                if (!obj["format"]!.Value<bool>()) return false;
                if (obj["disposable"]!.Value<bool>()) return false;
                return true;
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                return false;
            }
        }

        private static bool IsPasswordCorrect(string password, string hash)
        {
            try
            {
                return BCrypt.Net.BCrypt.EnhancedVerify(password, hash);
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                return false;
            }
        }

        public static async Task<(User, bool)> LoginAsync(string email, string password, HttpContext context)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password)) return (null, false);

            try
            {
                QueryFactory qf = DBLib.GetQueryBuilder();

                User user = await qf.Query("member").Where("email", email).FirstAsync<User>();

                if (user == null) return (null, false);

                if (!IsPasswordCorrect(password, user.Password)) return (null, false);

                //context.Session.SetInt32("id", user.Id);
                //context.Session.SetString("email", user.Email);
                //context.Session.SetString("name", user.Name);
                //context.Session.SetString("banned", user.IsBanned.ToString());
                //context.Session.SetString("admin", user.IsAdmin.ToString());
                //context.Session.SetString("regdate", user.RegDate.ToBinary().ToString());

                await context.Session.SetUser(user);

                return (user, true);
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                return (null, false);
            }
        }

        public static async Task UnsetLoginTokenAsync(string token, HttpContext context)
        {
            try
            {
                QueryFactory qf = DBLib.GetQueryBuilder();

                await qf.Query("logintoken").Where("token", token).DeleteAsync();

                qf.Dispose();

                await context.Session.SetUser(null);
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
            }
        }

        public static async Task<bool> IsLoggedInAsync(HttpContext context)
        {
            User user = await context.Session.GetUser();
            if (user == null) return false;

            if(user.Id == 0) return false;

            return true;
        }

        public static async Task<string> SetLoginTokenAsync(int id)
        {
            try
            {
                QueryFactory qf = DBLib.GetQueryBuilder();
                string token = Guid.NewGuid().ToString("N");
                await qf.Query("logintoken").InsertAsync(new LoginToken(token, id, DateTime.Now));
                return token;
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                return null;
            }
        }

        public static async Task<User> GetUserByTokenAsync(string token)
        {
            try
            {
                QueryFactory qf = DBLib.GetQueryBuilder();
                LoginToken data = await qf.Query("logintoken").Where("token", token).FirstAsync<LoginToken>();

                if(data == null) return null;
                return (await GetUserFromIdAsync(data.UserId)).Item1;
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                return null;
            }
        }
    }
}
