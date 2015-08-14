using Ninject;

namespace Api.Events
{
    public class SidekickEventsManager
    {
        private static StandardKernel _kernel;


        public static void Start()
        {
            _kernel = new StandardKernel(new SidekickNinjectModule());
     
        }

        

        public static SidekickEventsManager Instance
        {
            get { return _kernel.Get<SidekickEventsManager>(); }
        }

        public SidekickEvents Events
        {
            get { return _kernel.Get<SidekickEvents>(); }
        }

        public SidekickActivitySignaler ActivitySignaler
        {
            get { return _kernel.Get<SidekickActivitySignaler>(); }
        }
    }
}