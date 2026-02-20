using DesignPatternChallenge.DTO;
using DesignPatternChallenge.Facade;

var order = new OrderDTO
{
    ProductId = "PROD001",
    Quantity = 2,
    CustomerEmail = "cliente@email.com",
    CreditCard = "1234567890123456",
    Cvv = "123",
    ShippingAddress = "Rua Exemplo, 123",
    ZipCode = "12345-678",
    CouponCode = "PROMO10",
    ProductPrice = 100.00m
};

var orderFacade = new OrderFacade();
orderFacade.ProcessOrder(order);
