using System;
using System.IO;
using System.Threading.Tasks;

public class ImageService : IImageService
{
    private readonly string _imageFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

    public async Task<string> SaveImageAsync(string base64Image)
    {
        if (string.IsNullOrEmpty(base64Image))
            return null;

        try
        {
            byte[] imageBytes = Convert.FromBase64String(base64Image);
            string fileName = $"{Guid.NewGuid()}.jpg";
            string filePath = Path.Combine(_imageFolder, fileName);

            // Ensure directory exists
            Directory.CreateDirectory(_imageFolder);

            // Save file
            await File.WriteAllBytesAsync(filePath, imageBytes);

            return fileName; // Return only the file name, not the full path
        }
        catch (Exception ex)
        {
            throw new Exception("Error saving image: " + ex.Message);
        }
    }

    public string GetImageUrl(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return null;

        // Ensure we are not adding "/images/" twice
        if (!fileName.StartsWith("/images/"))
        {
            fileName = $"/images/{fileName}";
        }

        return $"http://192.168.1.106:5000{fileName}"; // Now it always has the correct format
    }     

}
