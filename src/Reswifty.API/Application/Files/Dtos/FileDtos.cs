namespace Reswifty.API.Application.Files.Dtos;

public record FileAsBytesDto(byte[] Data, string FileName, string ContentType);

public record FileAsStreamDto(Stream Data, string FileName, string ContentType);
