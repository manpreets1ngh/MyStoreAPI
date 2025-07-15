public interface IImageService
{
    Task<string> SaveImageAsync(string base64Image);
    string GetImageUrl(string fileName);
}
