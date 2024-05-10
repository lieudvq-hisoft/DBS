
namespace Data.Models
{
    public class ModelVehicleCreateModel
    {
        public Guid VehicleBrandId { get; set; }
        public string ModelName { get; set; }
    }

    public class ModelVehicleModel
    {
        public Guid Id { get; set; }
        public BrandVehicleModel VehicleBrand { get; set; }
        public string ModelName { get; set; }
    }
}
