namespace ChatApi.Services;

public class FileUpload
{
    public static async Task<string> Upload(IFormFile file)
    {
        var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads");

        if (!Directory.Exists(uploadFolder))
        {
            Directory.CreateDirectory(uploadFolder);
            
        }

        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
        var filePatch = Path.Combine(uploadFolder, fileName);
        await using var stream = new FileStream(filePatch, FileMode.Create);
        await file.CopyToAsync(stream);
        return fileName;
    }
}