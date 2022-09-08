// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ArrangeAttributes
// ReSharper disable ArrangeAccessorOwnerBody

namespace Dmz.Models.Portal;

public class ErrorViewModel
{
    public string RequestId { get; set; }
    
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}