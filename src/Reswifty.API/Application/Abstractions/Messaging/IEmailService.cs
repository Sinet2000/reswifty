namespace Reswifty.API.Application.Abstractions.Messaging;

public interface IEmailService
{
    Task<int> SendAsync(EmailDto emailDto, CancellationToken cancellationToken = default);

    Task<int> SendPlainTextAsync(
        string sender, string recipient, string subject, string body, CancellationToken cancellationToken = default);

    Task<int> SendFromTemplateAsync(
        string sender, string recipient, string subject, string templateName,
        Dictionary<string, string> templateParameters, CancellationToken cancellationToken = default);

    Task<IEnumerable<int>> SendBulkAsync(IEnumerable<EmailDto> emails, CancellationToken cancellationToken = default);
}