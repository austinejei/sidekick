using Ninject.Modules;

namespace Api.Events
{
    public class SidekickNinjectModule : NinjectModule
    {
        public override void Load()
        {
            Bind<SidekickEventsManager>().ToSelf().InSingletonScope();
            Bind<SidekickEvents>().ToSelf().InSingletonScope();
            Bind<SidekickActivitySignaler>().ToSelf().InSingletonScope();
            
        }
    }
}