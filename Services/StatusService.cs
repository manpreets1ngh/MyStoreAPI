using ApplicationToSellThings.APIs.Data;
using ApplicationToSellThings.APIs.Models;
using ApplicationToSellThings.APIs.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace ApplicationToSellThings.APIs.Services
{
    public class StatusService : IStatusService
    { 
        private readonly ApplicationToSellThingsAPIsContext _dbContext;

        public StatusService(ApplicationToSellThingsAPIsContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ResponseModel<StatusModel>> GetStatusById(int id) 
        {
            try
            {
                var status = await _dbContext.Status.FirstOrDefaultAsync(s => s.StatusId == id);

                if (status != null)
                {
                    var response = new ResponseModel<StatusModel>()
                    {
                        StatusCode = 200,
                        Status = "Success",
                        Message = "Status found",
                        Data = status,
                    };

                    return response;
                }
                else
                {
                    return new ResponseModel<StatusModel>
                    {
                        StatusCode = 404,
                        Status = "Not Found",
                        Message = "Status Not Found!",
                        Items = null,
                        Data = null
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<StatusModel>
                {
                    StatusCode = 500,
                    Status = "Error",
                    Message = ex.Message.ToString(),
                };
            }
        }

        public async Task<ResponseModel<StatusModel>> GetAllStatuses()
        {
            List<StatusModel> statusList = new List<StatusModel>();
            var statuses = await _dbContext.Status
                .Select(s => new
                {
                    s.StatusId,
                    s.Alias,
                    s.Name,
                    s.Type
                })
                .ToListAsync();

            if (statuses != null)
            {
                foreach (var status in statuses)
                {
                    var statusData = new StatusModel()
                    {
                        StatusId = status.StatusId,
                        Name = status.Name,
                        Alias = status.Alias,
                        Type = status.Type                        
                    };

                    statusList.Add(statusData);
                }

                var response = new ResponseModel<StatusModel>()
                {
                    StatusCode = 200,
                    Status = "Success",
                    Message = "List of all statuses founded successfully",
                    Items = statusList,
                };

                return response;
            }
            return new ResponseModel<StatusModel>
            {
                StatusCode = 401,
                Status = "Failure",
                Message = "Statuses Not Found"
            };

        }
    }
}
