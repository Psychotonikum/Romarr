namespace Romarr.Core.HealthCheck
{
    public interface IProvideHealthCheck
    {
        HealthCheck Check();
        bool CheckOnStartup { get; }
        bool CheckOnSchedule { get; }
    }
}
