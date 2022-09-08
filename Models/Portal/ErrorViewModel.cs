// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ArrangeAttributes

namespace Dmz.Models.Portal;

public class ErrorViewModel
{
    public string RequestId { get; set; }

    // ReSharper disable once ArrangeAccessorOwnerBody
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}