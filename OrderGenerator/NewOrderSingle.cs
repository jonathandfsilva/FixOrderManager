namespace TradeClient;

public class NewOrderSingle
{
    public string ID { get; set; }
    public string Symbol { get; set; }
    public char Side { get; set; }
    public int Amount { get; set; }
    public decimal Price { get; set; }

    public override string ToString()
    {
        return $"NewOrderSingle(Symbol={Symbol}, Side={Side}, Amount={Amount}, Price={Price:F2})";
    }
}