using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Entities
{
    public class AuditLog
    {
        public int Id { get; set; }
        public string TableName { get; set; }
        public string Action { get; set; }      
        public string? OldValues { get; set; }  
        public string? NewValues { get; set; } 
        public string? AffectedColumns { get; set; }
        public string? PrimaryKey { get; set; }
        public int? UserId { get; set; }
        public string? UserEmail { get; set; }
        public DateTimeOffset Date { get; set; }
        public string? IpAddress { get; set; }

        public User? User { get; set; }
    }
}
