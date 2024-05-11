using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Models
{
    public class BrandVehicleCreateModel
    {
        public string BrandName { get; set; }
    }

    public class BrandVehicleUpdateModel
    {
        public Guid BrandVehicleId { get; set; }
        public string BrandName { get; set; }
    }

    public class BrandVehicleModel
    {
        public Guid Id { get; set; }
        public string BrandName { get; set; }
    }

}
