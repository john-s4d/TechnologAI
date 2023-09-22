using System.Drawing;

namespace Technologai.Templates
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
    public class GenerateImageStableDiifusion : Template
    {
        public GenerateImageStableDiifusion()
        {
            Id = "generate_image_stable_diifusion";
            Description = "Generate Image Stable Diffusion";
            InputKeys = new string[] { "orginalImagePathWithExtension", "diffusedImagePathWithExtension" };
            OutputKeys = new string[] { "content" };
        }
        public override Task<bool> Assess(Information information) => Task.FromResult(true);

        public async override Task<Data?> Process(Information information)
        {
            var orginalImagePathWithExtension = ((string)information.Input.Structured["orginalImagePathWithExtension"]);
            var diffusedImagePathWithExtension = ((string)information.Input.Structured["diffusedImagePathWithExtension"]);
            try
            {
                if (orginalImagePathWithExtension != null)
                {
                    Bitmap originalImage = new Bitmap(orginalImagePathWithExtension);
                    Bitmap diffusedImage = GenerateImageStableDiffusion(originalImage);
                    if (diffusedImage != null && diffusedImagePathWithExtension != null)
                    {
                        diffusedImage.Save(diffusedImagePathWithExtension);
                        return await Task.FromResult((Data?)new Data(new Dictionary<string, string> { { "content", $"Diffused image saved successfully! : '{diffusedImagePathWithExtension}' " } }));
                    }
                    else
                    {
                        return await Task.FromResult((Data?)new Data(new Dictionary<string, string> { { "Error", "Unable to diffused orginal image" } }));
                    }
                }
                else
                {
                    return await Task.FromResult((Data?)new Data(new Dictionary<string, string> { { "Error", "Unable to find orginal image" } }));
                }
            }
            catch (Exception ex)
            {
                return await Task.FromResult((Data?)new Data(new Dictionary<string, string> { { "Exception", $"Something went wrong :  '{ex.Message}'" } }));
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