namespace EcommerceApi.Models
{
    public class ClientPosSettings
    {
        public int Id { get; set; }
        public string ClientIp { get; set; }
        public string StoreId { get; set; }
        public string TerminalId { get; set; }
    }
}
