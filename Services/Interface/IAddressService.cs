using MyStoreAPI.Models;

namespace MyStoreAPI.Services.Interface
{
    public interface IAddressService
    {
        Task<ResponseModel<AddressResponseViewModel>> AddAddress(AddressRequestApiModel addressRequestApiModel);
        Task<ResponseModel<AddressResponseViewModel>> GetAddressByUser(Guid userId);
        Task<ResponseModel<AddressResponseViewModel>> GetAddressById(Guid addressId);
    }
}
