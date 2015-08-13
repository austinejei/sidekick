using Ninject;

namespace Api.Events
{
    public class CashBaggEventsManager
    {
        private static StandardKernel _kernel;


        public static void Start()
        {
            _kernel = new StandardKernel(new CashBaggNinjectModule());
     
        }

        

        public static CashBaggEventsManager Instance
        {
            get { return _kernel.Get<CashBaggEventsManager>(); }
        }

        public SidekickEvents Events
        {
            get { return _kernel.Get<SidekickEvents>(); }
        }

        public CashBaggActivitySignaler ActivitySignaler
        {
            get { return _kernel.Get<CashBaggActivitySignaler>(); }
        }
    }
}