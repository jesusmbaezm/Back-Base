using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Entities
{
    public class User : AuditableEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public bool IsActive { get; set; }

        public ICollection<UserRole> UserRoles { get; set; } = [];
        public ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = [];
    }
}
