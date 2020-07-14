using EcommerceApi.Models;
using System.Linq;
using System.Text;

namespace EcommerceApi.Untilities
{
    public interface IOrderTemplateGenerator
    { 
      string GetHtmlString(Order order, bool includeMerchantCopy);
    }
}
