using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chibest.Service.ModelDTOs.Request
{
    public class WarehouseRequest
    {
        public string Name { get; set; } = null!;

        public string Address { get; set; } = null!;

        public string? PhoneNumber { get; set; }

        public string? FaxNumber { get; set; }

        public bool IsMainWarehouse { get; set; }
    }
}
