using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using TowerPortal.Enums;
using TowerPortal.Utilities;

namespace TowerPortal.Interfaces;

public interface IStatusMessageProvider
{
    bool WasSuccessful { get; }

    public string StatusMessage { get; }
    
    public RequestStatus Status { get; }

    public void SetStatus(string message, RequestStatus status);
    public void ClearStatus();
}