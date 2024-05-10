using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Models;

public class MomoCreatePaymentResponseModel
{
    public string RequestId { get; set; }
    public int ErrorCode { get; set; }
    public string OrderId { get; set; }
    public string Message { get; set; }
    public string LocalMessage { get; set; }
    public string RequestType { get; set; }
    public string PayUrl { get; set; }
    public string Signature { get; set; }
    public string QrCodeUrl { get; set; }
    public string Deeplink { get; set; }
    public string DeeplinkWebInApp { get; set; }
}

public class MomoExecuteResponseModel
{
    public string OrderId { get; set; }
    public long Amount { get; set; }
    public string OrderInfo { get; set; }
}

public class MomoOptionModel
{
    public string MomoApiUrl { get; set; }
    public string SecretKey { get; set; }
    public string AccessKey { get; set; }
    public string ReturnUrl { get; set; }
    public string NotifyUrl { get; set; }
    public string PartnerCode { get; set; }
    public string RequestType { get; set; }
}

public class OrderInfoModel
{
    public double Amount { get; set; }
}