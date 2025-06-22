using ApplicationToSellThings.APIs.Models;

namespace ApplicationToSellThings.APIs.Services.Interface
{
    public interface ICardService
    {
        Task<ResponseModel<CardResponseApiModel>> AddCardDetails(CardRequestApiModel cardRequestApiModel);
        Task<ResponseModel<CardResponseApiModel>> GetCardDetailsForUser(string userId);
        Task<ProcessPaymentResponseModel> ProcessPayment(ProcessPaymentRequestModel model);
    }
}
