using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyStoreAPI.Areas.Identity.Data;
using MyStoreAPI.Data;
using MyStoreAPI.Models;
using MyStoreAPI.Services.Interface;

namespace MyStoreAPI.Services
{
    public class AddressService : IAddressService
    {
        private readonly MyStoreAPIIdentityContext _dbContext;
        private readonly UserManager<MyStoreAPIUser> _userManager;

        public AddressService(MyStoreAPIIdentityContext dbContext, UserManager<MyStoreAPIUser> userManager)
        { 
            _dbContext = dbContext;
            _userManager = userManager;
        }

        public async Task<ResponseModel<AddressResponseViewModel>> AddAddress(AddressRequestApiModel addressRequestApiModel)
        {
            var user = await _userManager.FindByIdAsync(addressRequestApiModel.UserId);
            if(user != null)
            {
                var address = new AddressModel
                {
                    Id = Guid.NewGuid(),
                    UserId = addressRequestApiModel.UserId,
                    Street = addressRequestApiModel.Street,
                    City = addressRequestApiModel.City,
                    State = addressRequestApiModel.State,
                    PostCode = addressRequestApiModel.PostCode,
                    Country = addressRequestApiModel.Country,
                };

                _dbContext.Addresses.Add(address);
                try
                {
                    await _dbContext.SaveChangesAsync();
                    var response = new ResponseModel<AddressResponseViewModel>()
                    {
                        StatusCode = 200,
                        Status = "Success",
                        Message = "Address added Successfully",
                        Data = new AddressResponseViewModel
                        {
                            Id = address.Id,
                            UserId = address.UserId,
                            Street = address.Street,
                            City = address.City,
                            State = address.State,
                            PostCode = address.PostCode,
                            Country = address.Country
                        },
                    };

                    return response;
                }
                catch(Exception ex)
                {
                    return new ResponseModel<AddressResponseViewModel>
                    {
                        StatusCode = 500,
                        Message = ex.Message,
                    };
                }
            }
            return new ResponseModel<AddressResponseViewModel>
            {
                StatusCode = 401,
                Status = "Failure",
                Message = "User Not Found"
            };
        }

        public async Task<ResponseModel<AddressResponseViewModel>> GetAddressByUser(Guid userId)
        {
            List<AddressResponseViewModel> addressResponseViewModel = new List<AddressResponseViewModel>();
            string id = userId.ToString();
            var addresses = await _dbContext.Addresses.Where(a => a.UserId == id).ToListAsync();

            if(addresses != null)
            {
                foreach (var address in addresses)
                {
                    var addressResponseData = new AddressResponseViewModel()
                    {
                        Id = address.Id,
                        UserId = address.UserId,
                        Street = address.Street,
                        City = address.City,
                        State = address.State,
                        PostCode = address.PostCode,
                        Country = address.Country
                    };

                    addressResponseViewModel.Add(addressResponseData);
                }

                var response = new ResponseModel<AddressResponseViewModel>()
                {
                    StatusCode = 200,
                    Status = "Success",
                    Message = "Address Founded Successfully",
                    Items = addressResponseViewModel,
                };

                return response;
            }
            return new ResponseModel<AddressResponseViewModel>
            {
                StatusCode = 401,
                Status = "Failure",
                Message = "Address Not Found"
            };

        }

        public async Task<ResponseModel<AddressResponseViewModel>> GetAddressById(Guid addressId)
        {
            var address = await _dbContext.Addresses.FindAsync(addressId);

            if (address != null)
            {
                var addressResponseData = new AddressResponseViewModel()
                {
                    Id = address.Id,
                    UserId = address.UserId,
                    Street = address.Street,
                    City = address.City,
                    State = address.State,
                    PostCode = address.PostCode,
                    Country = address.Country
                };

                var response = new ResponseModel<AddressResponseViewModel>()
                {
                    StatusCode = 200,
                    Status = "Success",
                    Message = "Address Founded Successfully",
                    Data = addressResponseData,
                };

                return response;
            }
            return new ResponseModel<AddressResponseViewModel>
            {
                StatusCode = 401,
                Status = "Failure",
                Message = "Address Not Found"
            };

        }

    }
}
