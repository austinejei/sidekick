using Ninject.Modules;

namespace Api.Events
{
    public class CashBaggNinjectModule : NinjectModule
    {
        public override void Load()
        {
            Bind<CashBaggEventsManager>().ToSelf().InSingletonScope();
            Bind<SidekickEvents>().ToSelf().InSingletonScope();
            Bind<CashBaggActivitySignaler>().ToSelf().InSingletonScope();
            
        }
    }
}