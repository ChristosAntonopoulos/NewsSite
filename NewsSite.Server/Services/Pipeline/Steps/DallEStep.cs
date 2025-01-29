using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenAI.Interfaces;
using OpenAI.ObjectModels;
using OpenAI.ObjectModels.RequestModels;
using System.Linq;
using NewsSite.Server.Models.PipelineAggregate;
using System.Net.Http;

namespace NewsSite.Server.Services.Pipeline.Steps
{
    public class DallEStep : BaseStep
    {
        private readonly IOpenAIService _openAiService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _imageStoragePath;
        private static readonly string[] RequiredParams = { "prompt" };
        private const string DefaultModel = "dall-e-3";
        private const string DefaultQuality = "standard";
        private const string DefaultSize = "1024x1024";
        private const string DefaultStyle = "natural";

        public override string StepType => "DallE";

        public DallEStep(
            IOpenAIService openAiService, 
            IHttpClientFactory httpClientFactory,
            ILogger<DallEStep> logger) 
            : base(logger)
        {
            _openAiService = openAiService;
            _httpClientFactory = httpClientFactory;
            _imageStoragePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "dalle");
            Directory.CreateDirectory(_imageStoragePath);
        }

        private async Task<string> DownloadAndSaveImage(string imageUrl)
        {
            try
            {
                using var client = _httpClientFactory.CreateClient();
                var response = await client.GetAsync(imageUrl);
                response.EnsureSuccessStatusCode();

                var fileName = $"{Guid.NewGuid()}.png";
                var filePath = Path.Combine(_imageStoragePath, fileName);
                
                await using var fs = new FileStream(filePath, FileMode.Create);
                await response.Content.CopyToAsync(fs);

                // Return the relative URL
                return $"/images/dalle/{fileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading and saving DALL-E image");
                throw;
            }
        }

        public override async Task<Dictionary<string, object>> ExecuteAsync(
            PipelineExecutionContext context,
            Dictionary<string, object> input,
            Dictionary<string, string> parameters)
        {
            try
            {
                ValidateRequiredParameter("prompt", parameters, context.ExecutionId);
                
                var prompt = ReplaceVariables(parameters["prompt"], context, input);
                var model = GetTypedParameter(parameters, "model", DefaultModel);
                var quality = GetTypedParameter(parameters, "quality", DefaultQuality);
                var size = GetTypedParameter(parameters, "size", DefaultSize);
                var style = GetTypedParameter(parameters, "style", DefaultStyle);
                var n = GetTypedParameter(parameters, "n", 1);

                _logger.LogInformation(
                    "Generating image with DALL-E. Prompt: {Prompt}, Model: {Model}, Quality: {Quality}, Size: {Size}, Style: {Style}, N: {N}",
                    prompt, model, quality, size, style, n);

                var request = new ImageCreateRequest
                {
                    Prompt = prompt,
                    Model = model,
                    Quality = quality,
                    Size = size,
                    Style = style,
                    N = n,
                    ResponseFormat = StaticValues.ImageStatics.ResponseFormat.Url
                };

                var imageResult = await _openAiService.Image.CreateImage(request);
                
                if (!imageResult.Successful)
                {
                    var error = imageResult.Error?.Message ?? "Unknown error";
                    _logger.LogError("DALL-E image generation failed: {Error}", error);
                    throw new StepExecutionException(context.ExecutionId, $"DALL-E image generation failed: {error}");
                }

                // Download and save all generated images
                var savedUrls = new List<string>();
                foreach (var url in imageResult.Results.Select(r=> r.Url))
                {
                    var savedUrl = await DownloadAndSaveImage(url);
                    savedUrls.Add(savedUrl);
                }

                var result = new Dictionary<string, object>
                {
                    { "success", true },
                    { "urls", savedUrls },
                    { "revisedPrompt", imageResult.Results.FirstOrDefault()?.RevisedPrompt },
                    { "prompt", prompt },
                    { "model", model },
                    { "quality", quality },
                    { "size", size },
                    { "style", style },
                    { "generatedAt", DateTime.UtcNow }
                };

                SetOutputValue(context, parameters, result);
                return ApplyTransformations(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DallE step");
                throw new StepExecutionException(context.ExecutionId, "DALL-E step failed", ex);
            }
        }

        public override IEnumerable<string> GetRequiredParameters()
        {
            return RequiredParams;
        }
    }
} 