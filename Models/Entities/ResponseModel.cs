namespace ApplicationToSellThings.APIs.Models
{
    public class ResponseModel<T>
    {
        public int StatusCode { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public T? Data { get; set; }
        public List<T>? Items { get; set; }
    }
}
