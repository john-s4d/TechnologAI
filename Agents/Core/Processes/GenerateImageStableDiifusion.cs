using System.Drawing;

namespace Technologai
{
    /*
    //GenerateImage Stable Diffusion
    GenerateImageStableDiifusion generateImageStableDiifusion = new();
    var generateImageStableDiifusion_Dict = new Dictionary<string, object>()
        {
            {"orginalImagePath",@"C:\Users\local\Images\original_image.jpg"},
            {"diffuesedImagePath",@"C:\User\local\Images\diffused_image.jpg"}
        };
    var generateImageStableDiifusionResponse = await generateImageStableDiifusion.Execute(generateImageStableDiifusion_Dict);
    */

    /// <summary>
    /// Generate Image Stable Diffusion
    /// </summary>
    public class GenerateImageStableDiifusion : Process
    {
        public string Description { get; } = "Generate Image Stable Diffusion";
        public string SampleJsonIn { get; set; } = "{\"orginalImagePathWithExtension\":\"string\",\"diffusedImagePathWithExtension\":\"string\"}";
        public string SampleJsonOut { get; set; } = "{\"content\":\"string\"}";

        public async Task<Dictionary<string, object>> Execute(Dictionary<string, object> data)
        {

            var orginalImagePathWithExtension = ((string)data["orginalImagePath"]);
            var diffusedImagePathWithExtension = ((string)data["diffuesedImagePath"]);
            try
            {
                if (orginalImagePathWithExtension != null)
                {
                    Bitmap originalImage = new Bitmap(orginalImagePathWithExtension);
                    Bitmap diffusedImage = GenerateImageStableDiffusion(originalImage);
                    if (diffusedImage != null && diffusedImagePathWithExtension != null)
                    {
                        diffusedImage.Save(diffusedImagePathWithExtension);
                        return new Dictionary<string, object> { { "content", $"Diffused image saved successfully! : '{diffusedImagePathWithExtension}' " } };
                    }
                    else
                    {
                        return new Dictionary<string, object> { { "Error", "Unable to diffused orginal image" } };
                    }
                }
                else
                {
                    return new Dictionary<string, object> { { "Error", "Unable to find orginal image" } };
                }
            }
            catch (Exception ex)
            {
                return new Dictionary<string, object> { { "Exception", $"Something went wrong :  '{ex.Message}'" } };
            }
        }
        static Bitmap GenerateImageStableDiffusion(Bitmap originalImage)
        {
            // Create a new bitmap for the diffused image
            Bitmap diffusedImage = new Bitmap(originalImage.Width, originalImage.Height);

            // Perform diffusion on each pixel
            for (int y = 0; y < originalImage.Height; y++)
            {
                for (int x = 0; x < originalImage.Width; x++)
                {
                    Color originalPixel = originalImage.GetPixel(x, y);

                    // Perform diffusion calculations (modify as needed)
                    int diffusedR = originalPixel.R / 2;
                    int diffusedG = originalPixel.G / 2;
                    int diffusedB = originalPixel.B / 2;

                    // Set the diffused pixel color in the new image
                    diffusedImage.SetPixel(x, y, Color.FromArgb(diffusedR, diffusedG, diffusedB));
                }
            }
            return diffusedImage;
        }
    }
}