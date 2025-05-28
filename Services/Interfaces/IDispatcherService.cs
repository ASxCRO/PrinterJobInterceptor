namespace PrinterJobInterceptor.Services.Interfaces;
public interface IDispatcherService
{
    void Invoke(Action action);
}
