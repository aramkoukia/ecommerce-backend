namespace EcommerceApi.Models
{
    public partial class UserLocation
    {
        public int UserLocationId { get; set; }
        public string UserId { get; set; }
        public int LocationId { get; set; }
        public Location Location { get; set; }
    }
}
