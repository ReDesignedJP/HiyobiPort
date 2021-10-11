using System;
using SqlKata;

namespace Api.Models
{
    public class LoginToken
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("token")]
        public string Token { get; set; }
        [Column("userid")]
        public int UserId { get; set; }
        [Column("date")]
        public DateTime CreatedAt { get; set; }

        public LoginToken(int id, string token, int userid, DateTime date)
        {
            Id = id;
            Token = token;
            UserId = userid;
            CreatedAt = date;
        }

        public LoginToken(string token, int userid, DateTime date)
        {
            Token = token;
            UserId = userid;
            CreatedAt = date;
        }


    }
}
