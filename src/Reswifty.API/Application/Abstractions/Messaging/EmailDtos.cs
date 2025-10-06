using System.ComponentModel.DataAnnotations;
using Reswifty.API.Application.Files.Dtos;

namespace Reswifty.API.Application.Abstractions.Messaging;

public record EmailDto
{
    [StringLength(254, ErrorMessage = "Sender email address cannot exceed 254 characters")]
    [EmailAddress(ErrorMessage = "Invalid sender email address format")]
    public string? Sender { get; init; }

    [StringLength(254, ErrorMessage = "Recipient email address cannot exceed 254 characters")]
    [Required(ErrorMessage = "Recipient email address is required")]
    [EmailAddress(ErrorMessage = "Invalid recipient email address format")]
    public string Recipient { get; init; } = string.Empty;

    [Required(ErrorMessage = "Subject is required")]
    [StringLength(500, ErrorMessage = "Subject cannot exceed 500 characters")]
    public string Subject { get; init; } = string.Empty;

    [StringLength(50000, ErrorMessage = "Body cannot exceed 50000 characters")]
    public string? Body { get; init; }

    [StringLength(256, ErrorMessage = "Template name cannot exceed 256 characters")]
    public string? TemplateName { get; init; }

    public Dictionary<string, string>? TemplateParameters { get; init; }

    public IEnumerable<FileAsBytesDto>? Attachments { get; init; }

    // Validation methods
    public bool IsTemplateBasedEmail => !string.IsNullOrWhiteSpace(TemplateName);

    public bool IsPlainTextEmail => !IsTemplateBasedEmail && !string.IsNullOrWhiteSpace(Body);

    public IEnumerable<string> Validate()
    {
        var errors = new List<string>();

        if (IsTemplateBasedEmail && TemplateParameters == null)
        {
            errors.Add("Template parameters are required when using a template");
        }

        if (!IsTemplateBasedEmail && string.IsNullOrWhiteSpace(Body))
        {
            errors.Add("Body is required when not using a template");
        }

        return errors;
    }
}