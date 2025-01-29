using FluentValidation;

namespace NewsSite.Server.Configuration
{
    public class OpenAISettingsValidator : AbstractValidator<OpenAISettings>
    {
        public OpenAISettingsValidator()
        {
            RuleFor(x => x.ApiKey)
                .NotEmpty().WithMessage("OpenAI API key is required");

            RuleFor(x => x.Model)
                .NotEmpty().WithMessage("Model name is required");

            RuleFor(x => x.MaxTokens)
                .GreaterThan(0).WithMessage("MaxTokens must be greater than 0")
                .LessThan(8000).WithMessage("MaxTokens must be less than 8000");

            RuleFor(x => x.Temperature)
                .InclusiveBetween(0, 1).WithMessage("Temperature must be between 0 and 1");
        }
    }
} 