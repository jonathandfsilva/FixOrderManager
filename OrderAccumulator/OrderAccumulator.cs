using System;
using System.Collections.Generic;
using QuickFix;
using QuickFix.Fields;
using QuickFix.FIX44;
using Message = QuickFix.Message;

namespace SimpleAcceptor
{

    public class OrderAccumulator : MessageCracker, IApplication
    {
        #region QuickFix.Application Methods
        
        private static readonly decimal OrderLimit = 1000000;
        static readonly decimal DEFAULT_MARKET_PRICE = 10;
        int orderID = 0;
        int execID = 0;
        Dictionary<string, decimal> TotalPerSymbol = new Dictionary<string, decimal>();

        public void FromApp(Message message, SessionID sessionID)
        {
            Console.WriteLine("IN:  " + message);
            Crack(message, sessionID);
        }

        public void OnMessage(NewOrderSingle ord, SessionID sessionID)
        {
            bool success = ProcessOrder(ord.Price, ord.OrderQty, ord.Symbol);
            if(success)
            {
                genExecReport(ord, sessionID);
            }
            else
            {
                genReject(ord, sessionID);
            }
        }

        private void genReject(NewOrderSingle n, SessionID s)
        {
            var ordReject = new Reject(new RefSeqNum(++orderID));
            
            try
            {
                Session.SendToTarget(ordReject, s);
            }
            catch (SessionNotFound ex)
            {
                Console.WriteLine("==session not found exception!==");
                Console.WriteLine(ex.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private string GenOrderID() { return (++orderID).ToString(); }
        private string GenExecID() { return (++execID).ToString(); }

        private void genExecReport(NewOrderSingle n, SessionID s)
        {
            Symbol symbol = n.Symbol;
            Side side = n.Side;
            OrdType ordType = n.OrdType;
            OrderQty orderQty = n.OrderQty;
            Price price = new Price(DEFAULT_MARKET_PRICE);
            ClOrdID clOrdID = n.ClOrdID;

            ExecutionReport exReport = new ExecutionReport(
                new OrderID(GenOrderID()),
                new ExecID(GenExecID()),
                new ExecType(ExecType.FILL),
                new OrdStatus(OrdStatus.FILLED),
                symbol, 
                side,
                new LeavesQty(0),
                new CumQty(orderQty.getValue()),
                new AvgPx(price.getValue()));

            exReport.Set(clOrdID);
            exReport.Set(symbol);
            exReport.Set(orderQty);
            exReport.Set(new LastQty(orderQty.getValue()));
            exReport.Set(new LastPx(price.getValue()));

            if (n.IsSetAccount())
                exReport.SetField(n.Account);

            try
            {
                Session.SendToTarget(exReport, s);
            }
            catch (SessionNotFound ex)
            {
                Console.WriteLine("==session not found exception!==");
                Console.WriteLine(ex.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private bool ProcessOrder(Price price, OrderQty quantity, Symbol symbol)
        {
            if (!TotalPerSymbol.ContainsKey(symbol.getValue()))
            {
                TotalPerSymbol.Add(symbol.getValue(), 0);
            }
            var totalOrder = price.getValue() * quantity.getValue();
            if (TotalPerSymbol[symbol.getValue()] + totalOrder > OrderLimit)
            {
                Console.WriteLine("Financial exposure limit reached for " + symbol.getValue());
                return false;
            }
            else
            {
                TotalPerSymbol[symbol.getValue()] += totalOrder;
                Console.WriteLine(symbol.getValue() + " at R$ " + TotalPerSymbol[symbol.getValue()].ToString("C"));
                return true;
            }
        }

        public void ToApp(Message message, SessionID sessionID)
        {
            Console.WriteLine("OUT: " + message);
        }

        public void FromAdmin(Message message, SessionID sessionID) 
        {
            Console.WriteLine("IN:  " + message);
        }

        public void ToAdmin(Message message, SessionID sessionID)
        {
            Console.WriteLine("OUT:  " + message);
        }

        public void OnCreate(SessionID sessionID) { }
        public void OnLogout(SessionID sessionID) { }
        public void OnLogon(SessionID sessionID) { }
        #endregion
    }
}