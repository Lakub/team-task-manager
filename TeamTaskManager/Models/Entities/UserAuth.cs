using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamTaskManager.Models.Entities
{
    public class UserAuth
    {
        [Key, ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public string Password { get; set; } = string.Empty;
        public string PasswordSalt { get; set; } = string.Empty;
    }
}
