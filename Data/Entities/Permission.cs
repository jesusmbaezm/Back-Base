using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Entities
{
    public class Permission
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        /// <summary>Sidebar module label (e.g. "Ventas", "Almacén").</summary>
        public string? Module { get; set; }

        /// <summary>Sidebar child section label (e.g. "Cotizaciones", "Inventario").</summary>
        public string? Submodule { get; set; }

        /// <summary>Short human-readable action label shown in the role form (e.g. "Consultar cotizaciones").</summary>
        public string? Label { get; set; }

        /// <summary>Global display order. Encodes module × 10000 + submodule × 100 + item position.</summary>
        public int SortOrder { get; set; }

        public ICollection<RolePermission> RolePermissions { get; set; }
    }
}
