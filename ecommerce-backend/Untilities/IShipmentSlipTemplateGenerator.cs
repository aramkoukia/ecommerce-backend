using EcommerceApi.Models;

namespace EcommerceApi.Untilities
{
    public interface IShipmentSlipTemplateGenerator
    {
        string GetHtmlString(Order order);
    }
}