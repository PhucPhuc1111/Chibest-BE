using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chibest.Service.ModelDTOs.Response
{
    public class WarehouseResponse
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = null!;

        public string Address { get; set; } = null!;

        public string? PhoneNumber { get; set; }

        public bool IsMainWarehouse { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }


    }
}
