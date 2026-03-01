using System.Collections.Generic;

namespace DealManagementSystem.Entities
{
    public class Deal
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string Slug { get; set; } = null!;

        public string? Video { get; set; }

        public List<Hotel> Hotels { get; set; } = new();
    }
}