using EcommerceApi.Models;
using System.Linq;
using System.Text;

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
        private static readonly string Note5 = "All returns are subject to a <b>10% restocking fees.</b> We accept return and exchange up to <b>7 Days</b> after the date of purchase in new condition, not energized and original packaging with original Invoice and receipt.";
        private static readonly string Note6 = "All Custom orders: No returns-No exchange.";
        private static readonly string Note7 = "By signing below, you confirm that you have received all the products as per your requirement. Our company does not hold responsibility in case of any wrong order. I also agree with the above store policy.";
        private static readonly string CustomerCopy = "Customer Copy";
        private static readonly string MerchantCopy = "Merchant Copy";

        public static string GetHtmlString(Order order, bool includeMerchantCopy)
        {
            var sbCustomer = new StringBuilder();
            var sbFinal = new StringBuilder();
            var pageBreak = includeMerchantCopy ? "style='page-break-after: always;'" : "";
            var customerName = string.IsNullOrEmpty(order.Customer.CompanyName) ? "WALK-IN" : order.Customer.CompanyName + "<br/>";
            var customerProvince = string.IsNullOrEmpty(order.Customer.Province) ? "" : order.Customer.Province;
            var customerCity = string.IsNullOrEmpty(order.Customer.City) ? "" : order.Customer.City;
            var customerAddress = string.IsNullOrEmpty(order.Customer.Address) ? "" : order.Customer.Address + "<br/>";
            var customerPostalCode = string.IsNullOrEmpty(order.Customer.PostalCode) ? "" : order.Customer.PostalCode + "<br/>";
            var customerPhone = string.IsNullOrEmpty(order.Customer.PhoneNumber) ? "" : "Phone:" + order.Customer.PhoneNumber + "<br/>";

            sbCustomer.Append($@"
                        <html>
                            <head>
                            </head>
                            <body>
                                <div class='center'>
                                   <img src='data:image/png;base64,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' />
                                </div>
                                <div class='center xsmall-font'>{CompanyName}</div>
                                <div class='center xsmall-font'>{PhoneNumbers}</div>
                                <div class='center xsmall-font spaceafter-10'>{Website}</div>
                               
                                <div class='fullwidth center smaller-font spaceafter-10'>{Note1}&nbsp;<span class='red'>{Note2}</span></div>
                                <div class='fullwidth center smaller-font spaceafter-10'>{Note3}</div>
                                <hr/>
                                <div>LED Lights And Parts</div>
                                <div class='fullwidth xsmall-font spaceafter-10'><b>{order.Location.LocationName}:</b><br/> 
                                     {order.Location.LocationAddress}, <br />
                                     {order.Location.LocationName}, {order.Location.Province} {order.Location.PostalCode} <br/>
                                     Phone: {order.Location.PhoneNumber} <br/>
                                </div>
                                <table style='width:100%'>
                                <tr><td style='vertical-align: top; width:50%'>
                                <b>Invoice #{order.OrderId}</b><br/>");
            if (!string.IsNullOrEmpty(order.PoNumber))
            {
                sbCustomer.Append($@"PO Number: {order.PoNumber}<br/>");
            }
            sbCustomer.Append($@"Sale Date: {order.OrderDate}<br/>");
            if (order.OrderPayment != null && order.OrderPayment.Any())
            {
                sbCustomer.Append($@"Payment Date: {order.OrderPayment.FirstOrDefault().CreatedDate}<br/>");
                if (!string.IsNullOrEmpty(order.OrderPayment.FirstOrDefault().ChequeNo))
                {
                    sbCustomer.Append($@"Cheque No: {order.OrderPayment.FirstOrDefault().ChequeNo}<br/>");
                }
            }
            if (!string.IsNullOrEmpty(order.CardAuthCode))
            {
                sbCustomer.Append($@"Auth Code: {order.CardAuthCode}<br/>");
            }
            if (!string.IsNullOrEmpty(order.CardLastFourDigits))
            {
                sbCustomer.Append($@"Card: xxxx xxxx xxxx {order.CardLastFourDigits}<br/>");
            }
            sbCustomer.Append($@"User: {order.CreatedByUserName} <br/>
                </td>
                <td class='right' style='vertical-align: top; width:50%'>
                    <b>Customer:</b> {customerName}
                    {customerAddress}                                    
                    {customerCity} {customerProvince} {customerPostalCode}
                    {customerPhone}
                </td>
                </tr></table>
                <hr class='spaceafter-30'/>");

            if (order.Status.Equals(OrderStatus.Account.ToString(), System.StringComparison.InvariantCultureIgnoreCase)) {
                sbCustomer.Append($@"<p><b>Please Note: Payment is due on {order.OrderDate.AddDays(40).Date.ToString("dd-MMM-yyyy")}. Additional charges of 2% per month are applicable after due date.</b></p>");
            }
                                

                sbCustomer.Append($@"<h3 class='right'>{order.Status}</h3>    
                                <hr/>
                                <table>");

        foreach (var item in order.OrderDetail)
        {
                sbCustomer.AppendFormat(@"<tr>
                                <td style='width:10%'>{0}</td>
                                <td style='width:55%'>X {1}</td>
                                <td style='width:20%' class='right'>${2}</td>
                                <td style='width:15%' class='right'>${3}</td>
                                </tr>", item.Amount.ToString("G29"), item.Product.ProductCode + " - "+ item.Product.ProductName, item.UnitPrice, item.Total);
        }
            sbCustomer.AppendFormat(@"<tr style='border-top: 1pt solid darkgray;padding-top:10px'>
                        <td style='width:10%'></td>
                        <td style='width:55%'></td>
                        <td style='width:20%'>SubTotal:</td>
                        <td style='width:15%' class='right'>${0}</td>
                        </tr>", order.SubTotal);
        foreach (var tax in order.OrderTax)
        {
                sbCustomer.AppendFormat(@"<tr>
                                <td style='width:10%'></td>
                                <td style='width:55%'></td>
                                <td style='width:20%'>{0}:</td>
                                <td style='width:15%' class='right'>${1}</td>
                                </tr>", tax.Tax.TaxName, tax.TaxAmount);
        }

            if (order.RestockingFeeAmount != 0)
            {
                sbCustomer.AppendFormat(@"<tr>
                        <td style='width:10%'></td>
                        <td style='width:55%'></td>
                        <td style='width:20%'>Re-Stocking Fee:</td>
                        <td style='width:15%' class='right'>${0}</td>
                        </tr>", order.RestockingFeeAmount);
            }

            var totalDiscount = order.OrderDetail.Sum(o => o.TotalDiscount);
            if (totalDiscount > 0)
            {
                sbCustomer.AppendFormat(@"<tr>
                            <td style='width:10%'></td>
                            <td style='width:55%'></td>
                            <td style='width:20%'>Discount:</td>
                            <td style='width:15%' class='right'>${0}</td>
                            </tr>", totalDiscount);
            }

            sbCustomer.AppendFormat(@"<tr>
                        <td style='width:10%'></td>
                        <td style='width:55%'></td>
                        <td style='width:20%'><b>To Pay:</b></td>
                        <td style='width:15%' class='right'>${0}</td>
                        </tr>", order.Total);

            sbCustomer.AppendFormat(@"<tr>
                        <td style='width:10%'></td>
                        <td style='width:55%'></td>
                        <td style='width:20%'>Paid Amount:</td>
                        <td style='width:15%' class='right'>${0}</td>
                        </tr>", order.OrderPayment.Sum(p=>p.PaymentAmount));

            sbCustomer.AppendFormat(@"<tr>
                        <td style='width:10%'></td>
                        <td style='width:55%'></td>
                        <td style='width:20%'><b>Remain:</b></td>
                        <td style='width:15%' class='right'>${0}</td>
                        </tr>", order.IsAccountReturn ? 0 : order.Total - order.OrderPayment.Sum(p => p.PaymentAmount));

            if (order.IsAccountReturn)
            {
                sbCustomer.AppendFormat(@"<tr>
                        <td style='width:10%'></td>
                        <td style='width:55%'></td>
                        <td style='width:35%' colspan='2'><b>Added to customer Account</b></td>
                        </tr>");
            }

            var paymentType = order.OrderPayment.FirstOrDefault()?.PaymentType?.PaymentTypeName;
            if (paymentType != null)
            { 
                sbCustomer.AppendFormat(@"<tr>
                        <td style='width:10%'></td>
                        <td style='width:55%'></td>
                        <td style='width:35%' colspan='2'>Paid by: {0}</td>
                        </tr>", paymentType);
            }

            sbFinal.Append(sbCustomer);

            sbFinal.Append($@"
                    </table>

                    <hr class='spaceafter-30'/>

                    <div>{CustomerCopy}</div>
                    <hr class='spaceafter-30'/>");
            if (!string.IsNullOrEmpty(order.Notes))
            {
                sbFinal.Append($@"<div class='header'><p><b>Notes: </b>{order.Notes}</p></div>");
            }
            if (!string.IsNullOrEmpty(order.PhoneNumber))
            {
                sbFinal.Append($@"<div class='header'><p><b>Phone Number: </b>{order.PhoneNumber}</p></div>");
            }
            if (!string.IsNullOrEmpty(order.AuthorizedBy))
            {
                sbFinal.Append($@"<div class='header'><p><b>Authorized By: </b>{order.AuthorizedBy}</p></div>");
            }

            sbFinal.Append($@"
                    <div class='header'><p><b>Attention:</b>{Note4}</p></div>
                    <div class='header'><p><b>Store policy:</b>{Note5}</p></div>
                    <div class='header' {pageBreak}><p><b>{Note6}</b></p></div>");

            if (includeMerchantCopy)
            {
                var sbMerchangt = new StringBuilder(sbCustomer.ToString());
                sbFinal.Append(sbMerchangt.ToString());
                sbFinal.Append($@"
                    </table>
                    <hr class='spaceafter-30'/>
                    <div>{MerchantCopy}</div>
                    <hr class='spaceafter-30'/>");

                if (!string.IsNullOrEmpty(order.Notes))
                {
                    sbFinal.Append($@"<div class='header'><p><b>Notes: </b>{order.Notes}</p></div>");
                }
                if (!string.IsNullOrEmpty(order.PhoneNumber))
                {
                    sbFinal.Append($@"<div class='header'><p><b>Phone Number: </b>{order.PhoneNumber}</p></div>");
                }
                if (!string.IsNullOrEmpty(order.AuthorizedBy))
                {
                    sbFinal.Append($@"<div class='header'><p><b>Authorized By: </b>{order.AuthorizedBy}</p></div>");
                }

                sbFinal.Append($@"
                    <div class='header'><p><b>Attention:</b>{Note4}</p></div>
                    <div class='header'><p><b>Store policy:</b>{Note5}</p></div>
                    <div class='header'><p><b>Agreement: </b>{Note7}</p></div>
                    <div class='header'><p><b>{Note6}</b></p></div>
                    <br />
                    <h4>Customer Signature: ___________________</h4>
                    <br />
                    <h4>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Authorized By: ___________________</h4>");
            }

            sbFinal.Append("</body></ html>");

            return sbFinal.ToString();
        }
    }
}
