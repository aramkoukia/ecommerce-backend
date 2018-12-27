using EcommerceApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceApi.Untilities
{
    public static class OrderTemplateGenerator
    {
        private static readonly string CompanyName = "Pixel Print Ltd. GST: 823338694 RT0001";
        private static readonly string PhoneNumbers = "Tel: 604-559-5000 Fax:604-559-5008";
        private static readonly string Website = "www.LightsAndParts.com";
        private static readonly string Note1 = "Dear Customer, to pay by cheque for an invoice, Please";
        private static readonly string Note2 = "PAY TO THE ORDER OF: PIXEL PRINT LTD.";
        private static readonly string Note3 = "Mention your invoice number on memo.Thank you.";
        private static readonly string Note4 = "All products must be installed by certified electrician. We will not be responsible for any damage caused by incorrectly connecting or improper use of the material.";
        private static readonly string Note5 = "All returns are subject to a 10% restocking fees.We accept return and exchange up to 7 Days after the date of purchase in new condition, not energized and original packaging with original Invoice and receipt.";
        private static readonly string Note6 = "All Customer orders: No returns-No exchange.";

        public static string GetHtmlString(Order order)
        {
            // var employees = DataStorage.GetAllEmployess();
            var sb = new StringBuilder();
            sb.Append($@"
                        <html>
                            <head>
                            </head>
                            <body>
                                <div class='header'>{CompanyName}</div>
                                <div class='header'>{PhoneNumbers}</div>
                                <div class='header'>{Website}</div>
                                <div class='header'>{Note1}<div class='red'>{Note2}</div></div>
                                <div class='header'>{Note3}</div>
                                <hr/>
                                
                                <div>Address goes here</div>
                                <div>Customer Name goes here</div>
                                <div>Invoice #{order.OrderId}</div>
                                <div>Sale Date: {order.OrderDate}</div>
                                <div>User: {order.CreatedByUserId}</div>
                                <div>Card No: ****</div>
                                <div>Auth No: ****</div>
                                <hr/>
                                <h3>{order.Status}</h3>    
                                <hr/>
                                
                                <table>
                                    <tr>

                                    </tr>");

                                        //foreach (var emp in employees)
                                        //{
                                        //    sb.AppendFormat(@"<tr>
                                        //                        <td>{0}</td>
                                        //                        <td>{1}</td>
                                        //                        <td>{2}</td>
                                        //                        <td>{3}</td>
                                        //                      </tr>", emp.Name, emp.LastName, emp.Age, emp.Gender);
                                        //}

                                sb.Append($@"
                                </table>
                                <hr/>
                                <div>Customer Copy</div>
                                <hr/>   
                                <div class='header'><p><b>Attention:</b>{Note4}</p></div>
                                <div class='header'><p><b>Store policy:</b>{Note5}</p></div>
                                <div class='header'><p><b>{Note6}</b></p></div>
                            </body>
                        </html>");

            return sb.ToString();
        }
    }
}
