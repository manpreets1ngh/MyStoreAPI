using MyStoreAPI.Static;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyStoreAPI.Areas.Identity.Data;
using MyStoreAPI.Data;
using MyStoreAPI.Models;
using MyStoreAPI.Services.Interface;
using Square;
using Square.Models;
using Square.Exceptions;

namespace MyStoreAPI.Services
{
    public class CardService : ICardService
    {
        private readonly MyStoreAPIIdentityContext _dbContext;
        private readonly UserManager<MyStoreAPIUser> _userManager;
        private readonly SquareSettings _squareSettings;
        public CardService(MyStoreAPIIdentityContext dbContext, UserManager<MyStoreAPIUser> userManager, SquareSettings squareSettings)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _squareSettings = squareSettings;
        }

        public async Task<ResponseModel<CardResponseApiModel>> AddCardDetails(CardRequestApiModel cardRequestApiModel)
        {
            var user = await _userManager.FindByIdAsync(cardRequestApiModel.UserId);
            if (user != null)
            {
                var cardModel = new CardModel
                {
                    CardId = Guid.NewGuid(),
                    UserId = cardRequestApiModel.UserId,
                    CardHolderName = cardRequestApiModel.CardHolderName,
                    Last4Digits = cardRequestApiModel.Last4Digits,
                    CardBrand = cardRequestApiModel.CardBrand,
                    AddedOn = DateTime.Now,
                };

                _dbContext.CardDetails.Add(cardModel);
                try
                {
                    await _dbContext.SaveChangesAsync();
                    var resultResponse = new CardResponseApiModel
                    {
                        CardHolderName = cardModel.CardHolderName,
                        Last4Digits = cardModel.Last4Digits,
                        CardBrand = cardModel.CardBrand,
                        AddedOn = DateTime.Now
                    };
                    var response = new ResponseModel<CardResponseApiModel>()
                    {
                        StatusCode = 200,
                        Status = "Success",
                        Message = "Card added Successfully",
                        Data = resultResponse,
                    };

                    return response;
                }
                catch (Exception ex)
                {
                    return new ResponseModel<CardResponseApiModel>
                    {
                        StatusCode = 500,
                        Message = ex.Message,
                    };
                }
            }
            return new ResponseModel<CardResponseApiModel>
            {
                StatusCode = 401,
                Status = "Failure",
                Message = "User Not Found"
            };
        }

        public async Task<ResponseModel<CardResponseApiModel>> GetCardDetailsForUser(string userId)
        {
            List<CardResponseApiModel> cardResponseApiModel = new List<CardResponseApiModel>();
            var cardDetails = await _dbContext.CardDetails.Where(a => a.UserId == userId).OrderByDescending(c => c.AddedOn).ToListAsync();

            if (cardDetails != null)
            {
                foreach (var card in cardDetails)
                {
                    var cardResponseData = new CardResponseApiModel()
                    {
                        CardId = card.CardId,
                        CardHolderName = card.CardHolderName,
                        Last4Digits = card.Last4Digits,
                        CardBrand = card.CardBrand,
                        AddedOn = card.AddedOn
                    };

                    cardResponseApiModel.Add(cardResponseData);
                }

                var response = new ResponseModel<CardResponseApiModel>()
                {
                    StatusCode = 200,
                    Status = "Success",
                    Message = "Card Founded Successfully",
                    Items = cardResponseApiModel,
                };

                return response;
            }
            return new ResponseModel<CardResponseApiModel>
            {
                StatusCode = 401,
                Status = "Failure",
                Message = "Card Not Found"
            };

        }

        public async Task<ProcessPaymentResponseModel> ProcessPayment(ProcessPaymentRequestModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return new ProcessPaymentResponseModel
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            var client = new SquareClient.Builder()
                .Environment(Square.Environment.Sandbox)
                .AccessToken(_squareSettings.AccessToken)
                .Build();

            var money = new Money.Builder()
            .Amount(model.Amount)
            .Currency("GBP")
            .Build();

            var paymentRequest = new CreatePaymentRequest.Builder(
                model.Nonce,
                Guid.NewGuid().ToString(),
                money // ✅ Required third parameter
            )
            .LocationId(_squareSettings.LocationId)
            .Build();


            try
            {
                var paymentResponse = await client.PaymentsApi.CreatePaymentAsync(paymentRequest);
                var paymentId = paymentResponse.Payment.Id;

                // Optionally store card metadata
                Guid? cardId = null;
                if (model.SaveCard)
                {
                    var cardModel = new CardModel
                    {
                        CardId = Guid.NewGuid(),
                        UserId = model.UserId,
                        CardHolderName = $"{user.FirstName} {user.LastName}",
                        Last4Digits = paymentResponse.Payment.CardDetails.Card.Last4,
                        CardBrand = paymentResponse.Payment.CardDetails.Card.CardBrand,
                        AddedOn = DateTime.UtcNow
                    };

                    _dbContext.CardDetails.Add(cardModel);
                    await _dbContext.SaveChangesAsync();
                    cardId = cardModel.CardId;
                }

                return new ProcessPaymentResponseModel
                {
                    Success = true,
                    Message = "Payment successful",
                    PaymentId = paymentId,
                    CardId = cardId
                };
            }
            catch (ApiException ex)
            {
                return new ProcessPaymentResponseModel
                {
                    Success = false,
                    Message = string.Join(" | ", ex.Errors.Select(e => e.Detail))
                };
            }
        }

    }
}
