using Rumble.Platform.Common.Services;

namespace Dmz.Services;

public class CleanupService : PlatformTimerService
{
    private readonly OtpService _otpService;
    public CleanupService(OtpService otpService) : base(intervalMS: 3_600_000, startImmediately: true)
        => _otpService = otpService;

    protected override void OnElapsed() => _otpService.Cleanup();
}