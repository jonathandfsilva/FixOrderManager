using System;
using System.Threading;
using QuickFix;
using QuickFix.Fields;
using QuickFix.Fields.Converters;
using QuickFix.FIX44;
using Message = QuickFix.Message;

namespace TradeClient
{
    public class OrderGenerator : MessageCracker, IApplication
    {
        Session _session = null;

        public IInitiator MyInitiator = null;

        #region IApplication interface overrides

        public void OnCreate(SessionID sessionID)
        {
            _session = Session.LookupSession(sessionID);
        }

        public void OnLogon(SessionID sessionID)
        {
            Console.WriteLine("Logon - " + sessionID);
        }

        public void OnLogout(SessionID sessionID)
        {
            Console.WriteLine("Logout - " + sessionID);
        }

        public void FromAdmin(Message message, SessionID sessionID)
        {
        }

        public void ToAdmin(Message message, SessionID sessionID)
        {
        }

        public void FromApp(Message message, SessionID sessionID)
        {
            Console.WriteLine("IN:  " + message);
            try
            {
                Crack(message, sessionID);
            }
            catch (Exception ex)
            {
                Console.WriteLine("==Cracker exception==");
                Console.WriteLine(ex.ToString());
                Console.WriteLine(ex.StackTrace);
            }
        }

        public void ToApp(Message message, SessionID sessionID)
        {
            try
            {
                bool possDupFlag = false;
                if (message.Header.IsSetField(Tags.PossDupFlag))
                {
                    possDupFlag = BoolConverter.Convert(
                        message.Header.GetString(Tags.PossDupFlag)); /// FIXME
                }

                if (possDupFlag)
                    throw new DoNotSend();
            }
            catch (FieldNotFoundException)
            {
            }

            Console.WriteLine("\nOUT: " + message);
        }

        #endregion


        #region MessageCracker handlers

        public void OnMessage(ExecutionReport m, SessionID s)
        {
            Console.WriteLine("----Received execution report----");
        }

        public void OnMessage(OrderCancelReject m, SessionID s)
        {
            Console.WriteLine("----Received order cancel reject----");
        }
        
        public void OnMessage(Reject m, SessionID s)
        {
            Console.WriteLine("----Received order reject----");
        }

        #endregion


        public void Run()
        {
            while (true)
            {
                try
                {
                    while (true)
                        QueryEnterOrder();

                }
                catch (Exception e)
                {
                    Console.WriteLine("Message Not Sent: " + e.Message);
                    Console.WriteLine("StackTrace: " + e.StackTrace);
                }
            }

            Console.WriteLine("Program shutdown.");
        }

        private void SendMessage(Message m)
        {
            if (_session != null)
                _session.Send(m);
            else
            {
                // This probably won't ever happen.
                Console.WriteLine("Can't send message: session not created.");
            }
        }

        private void QueryEnterOrder()
        {
            QuickFix.FIX44.NewOrderSingle m = QueryNewOrderSingle44();
            SendMessage(m);
            Thread.Sleep(1000);
        }


        #region Message creation functions

        private static readonly string[] Symbols = { "PETR4", "VALE3", "VIIA4" };
        private static readonly char[] Sides = { Side.BUY, Side.SELL };
        private static readonly Random Random = new Random();

        private QuickFix.FIX44.NewOrderSingle QueryNewOrderSingle44()
        {
            var newOrder = GenRandomOrder();
            Console.WriteLine(newOrder.ToString());

            QuickFix.FIX44.NewOrderSingle newOrderSingle = new QuickFix.FIX44.NewOrderSingle(
                new ClOrdID(newOrder.ID),
                new Symbol(newOrder.Symbol),
                new Side(newOrder.Side),
                new TransactTime(DateTime.Now),
                new OrdType(OrdType.LIMIT));

            newOrderSingle.Set(new HandlInst('1'));
            newOrderSingle.Set(new OrderQty(newOrder.Amount));
            newOrderSingle.Set(new TimeInForce(TimeInForce.DAY));
            newOrderSingle.Set(new Price(newOrder.Price));

            return newOrderSingle;
        }

        private static NewOrderSingle GenRandomOrder()
        {
            var order = new NewOrderSingle
            {
                ID = Guid.NewGuid().ToString(),
                Symbol = Symbols[Random.Next(Symbols.Length)],
                Side = Sides[Random.Next(Sides.Length)],
                Amount = Random.Next(100000),
                Price = Convert.ToDecimal(Random.NextDouble() * (1.0 - 0.01) + 0.01)
            };
            return order;
        }

        #endregion

    }
}
