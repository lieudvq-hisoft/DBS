
namespace Data.Models
{
    public class ModelVehicleCreateModel
    {
        public Guid BrandVehicleId { get; set; }
        public string ModelName { get; set; }
    }

    public class ModelVehicleUpdateModel
    {
        public Guid ModelVehicleId { get; set; }
        public string ModelName { get; set; }
    }

    public class ModelVehicleModel
    {
        public Guid Id { get; set; }
        public string ModelName { get; set; }
    }
}
