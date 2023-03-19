using System;
using QuickFix;
using QuickFix.Transport;

namespace TradeClient
{
    class Program
    {
        
        private static readonly string[] Symbols = {"PETR4", "VALE3", "VIIA4"};
        private static readonly string[] Sides = {"BUY", "SELL"};
        private static readonly Random Random = new Random();
        
        [STAThread]
        static void Main(string[] args)
        {
            string file = "tradeclient.cfg";

            try
            {
                SessionSettings settings = new SessionSettings(file);
                OrderGenerator application = new OrderGenerator();
                IMessageStoreFactory storeFactory = new FileStoreFactory(settings);
                ILogFactory logFactory = new ScreenLogFactory(settings);
                SocketInitiator initiator = new SocketInitiator(application, storeFactory, settings, logFactory);

                application.MyInitiator = initiator;
                Console.WriteLine("OrderGenerator is running!\nReading configuration from file 'tradeclient.cfg'\npress <enter> to quit");
                initiator.Start();
                application.Run();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
            Environment.Exit(1);
        }
    }
}
