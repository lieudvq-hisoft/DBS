using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Model
{
    public class RoleModel
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public bool IsDeactive { get; set; }
    }

    public class RoleCreateModel
    {
        public string? Description { get; set; }
        public string? NormalizedName { get; set; }
        public string Name { get; set; }
    }

    public class AssignRoleModel
    {
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }
    }
}