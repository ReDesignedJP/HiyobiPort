using System;
using SqlKata;

namespace Api.Models
{
    public class User
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("name")]
        public string Name {  get; set; }
        [Column("email")]
        public string Email {  get; set; }
        [Column("password")]
        public string Password {  get; set; }
        [Column("banned")]
        public bool IsBanned { get; set; } = false;
        [Column("admin")]
        public bool IsAdmin { get; set; } = false;
        [Column("regdate")]
        public DateTime RegDate { get; set; }

        public User(int id, string name, string email, string password, bool banned, bool admin, DateTime regdate)
        {
            Id = id;
            Name = name;
            Email = email;
            Password = password;
            IsBanned = banned;
            IsAdmin = admin;
            RegDate = regdate;
        }

        public User(string name, string email, string password, bool banned, bool admin, DateTime regdate)
        {
            Name = name;
            Email = email;
            Password = password;
            IsBanned = banned;
            IsAdmin = admin;
            RegDate = regdate;
        }
    }
}
