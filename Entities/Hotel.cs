namespace DealManagementSystem.Entities
{
    public class Hotel
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public decimal Rate { get; set; }

        public string? Amenities { get; set; }

        public int DealId { get; set; }   // Foreign Key

        [System.Text.Json.Serialization.JsonIgnore]
        public Deal? Deal { get; set; }
    }
}