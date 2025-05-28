using PrinterJobInterceptor.Services.Interfaces;

namespace PrinterJobInterceptor.Services;
public class DispatcherService : IDispatcherService
{
    public void Invoke(Action action) => App.Current.Dispatcher.Invoke(action);
}
