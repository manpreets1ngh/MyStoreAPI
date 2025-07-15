using MyStoreAPI.Models;

namespace MyStoreAPI.Services.Interface
{
    public interface IStatusService
    {
        Task<ResponseModel<StatusModel>> GetStatusById(int id);
        Task<ResponseModel<StatusModel>> GetAllStatuses();
    }
}
