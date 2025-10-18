using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chibest.Service.ModelDTOs.Response
{
    public class BranchResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public bool IsFranchise { get; set; }
        public string? OwnerName { get; set; }
        public string Status { get; set; } = null!;
        public int UserCount { get; set; }     
        public int WarehouseCount { get; set; }
    }
}
