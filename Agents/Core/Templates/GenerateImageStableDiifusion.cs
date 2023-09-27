using System.Drawing;

namespace Technologai.Templates
{

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
            OutputKeys = new string[] { "diffusedImagePathWithExtension" };
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
                        return Data.Create(diffusedImagePathWithExtension);
                    }
                    else
                    {
                        return Data.Create("Error", "Unable to diffused orginal image");
                    }
                }
                else
                {
                    return Data.Create("Error", "Unable to find orginal image");
                }
            }
            catch (Exception ex)
            {
                return Data.Create(ex);
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