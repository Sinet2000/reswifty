namespace Reswifty.API.Endpoints;

// public static class FilesEndpoints
// {
//     public static IEndpointRouteBuilder MapFilesEndpoints(this IEndpointRouteBuilder app)
//     {
//         var api = app.MapGroup("/api/files").WithTags("Files");
//
//         // POST /api/files (multipart)
//         api.MapPost("/", async Task<Results<Created<FileDto>, BadRequest<string>>> (HttpRequest req, IFileService svc, CancellationToken ct) =>
//         {
//             if (!req.HasFormContentType) return TypedResults.BadRequest("Expected multipart/form-data");
//             var form = await req.ReadFormAsync(ct);
//             var file = form.Files["file"];
//             if (file is null || file.Length == 0) return TypedResults.BadRequest("File missing");
//
//             await using var ms = new MemoryStream();
//             await file.CopyToAsync(ms, ct);
//             var dto = await svc.UploadAsync(new FileUploadDto(file.FileName, file.ContentType, ms.ToArray()), ct);
//             return TypedResults.Created($"/api/files/{dto.Id}", dto);
//         });
//
//         // GET /api/files/{id} (metadata)
//         api.MapGet("/{id:guid}", async Task<Results<Ok<FileDto>, NotFound>> (Guid id, IFileService svc, CancellationToken ct) =>
//         {
//             var m = await svc.GetMetaAsync(id, ct);
//             return m is null ? TypedResults.NotFound() : TypedResults.Ok(m);
//         });
//
//         // GET /api/files/{id}/content (raw)
//         api.MapGet("/{id:guid}/content", async Task<Results<FileContentHttpResult, NotFound>> (Guid id, IFileService svc, CancellationToken ct) =>
//         {
//             var f = await svc.DownloadAsync(id, ct);
//             return f is null
//                 ? TypedResults.NotFound()
//                 : TypedResults.File(f.Bytes, f.ContentType, enableRangeProcessing: true, fileDownloadName: f.FileName);
//         });
//
//         // DELETE /api/files/{id}
//         api.MapDelete("/{id:guid}", async Task<Results<NoContent, NotFound>> (Guid id, IFileService svc, CancellationToken ct) =>
//         {
//             var ok = await svc.DeleteAsync(id, ct);
//             return ok ? TypedResults.NoContent() : TypedResults.NotFound();
//         });
//
//         return api;
//     }
// }