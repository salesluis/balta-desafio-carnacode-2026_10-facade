namespace DesignPatternChallenge.Facade;

public class OrderFacade
{
    private readonly InventorySystem _inventorySystem = new();
    private readonly PaymentGateway _paymentGateway = new();
    private readonly ShippingService _shippingService = new();
    private readonly CouponSystem _cupomSystem = new();
    private readonly NotificationService _notificationService = new();

    public void ProcessOrder(DTO.OrderDTO order)
    {
        try
        {
                // Problema: Cliente precisa conhecer ordem correta de execução
                // e gerenciar estado de cada subsistema
                
                // Passo 1: Verificar estoque
                if (!_inventorySystem.CheckAvailability(order.ProductId))
                {
                    Console.WriteLine("❌ Produto indisponível");
                    return;
                }

                // Passo 2: Reservar produto
                _inventorySystem.ReserveProduct(order.ProductId, order.Quantity);

                // Passo 3: Validar e aplicar cupom
                decimal discount = 0;
                if (!string.IsNullOrEmpty(order.CouponCode))
                {
                    if (_cupomSystem.ValidateCoupon(order.CouponCode))
                    {
                        discount = _cupomSystem.GetDiscount(order.CouponCode);
                    }
                }

                // Passo 4: Calcular valores
                decimal subtotal = order.ProductPrice * order.Quantity;
                decimal discountAmount = subtotal * discount;
                decimal shippingCost = _shippingService.CalculateShipping(order.ZipCode, order.Quantity * 0.5m);
                decimal total = subtotal - discountAmount + shippingCost;

                // Passo 5: Processar pagamento
                string transactionId = _paymentGateway.InitializeTransaction(total);
                
                if (!_paymentGateway.ValidateCard(order.CreditCard, order.Cvv))
                {
                    _inventorySystem.ReleaseReservation(order.ProductId, order.Quantity);
                    Console.WriteLine("❌ Cartão inválido");
                    return;
                }

                if (!_paymentGateway.ProcessPayment(transactionId, order.CreditCard))
                {
                    _inventorySystem.ReleaseReservation(order.ProductId, order.Quantity);
                    Console.WriteLine("❌ Pagamento recusado");
                    return;
                }

                // Passo 6: Criar envio
                string orderId = $"ORD{DateTime.Now.Ticks}";
                string labelId = _shippingService.CreateShippingLabel(orderId, order.ShippingAddress);
                _shippingService.SchedulePickup(labelId, DateTime.Now.AddDays(1));

                // Passo 7: Marcar cupom como usado
                if (!string.IsNullOrEmpty(order.CouponCode))
                {
                    _cupomSystem.MarkCouponAsUsed(order.CouponCode, order.CustomerEmail);
                }

                // Passo 8: Enviar notificações
                _notificationService.SendOrderConfirmation(order.CustomerEmail, orderId);
                _notificationService.SendPaymentReceipt(order.CustomerEmail, transactionId);
                _notificationService.SendShippingNotification(order.CustomerEmail, labelId);

                Console.WriteLine($"\n✅ Pedido {orderId} finalizado com sucesso!");
                Console.WriteLine($"   Total: R$ {total:N2}");
            }
        catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro ao processar pedido: {ex.Message}");
            }
    }
    
}