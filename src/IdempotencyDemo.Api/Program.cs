using IdempotencyDemo.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IdempotencyStore>();

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new
{
    service = "IdempotencyDemo.Api",
    endpoints = new[] { "POST /transfer" }
}));

app.MapPost("/transfer", (HttpRequest http, TransferRequest req, IdempotencyStore store) =>
{
    const string headerName = "Idempotency-Key";

    if (!http.Headers.TryGetValue(headerName, out var keyValues))
        return Results.BadRequest(new { error = $"{headerName} header gereklidir ." });

    var key = keyValues.ToString().Trim();
    if (string.IsNullOrWhiteSpace(key))
        return Results.BadRequest(new { error = $"{headerName} header boş olamaz" });

    // body validasyon
    if (req.Amount <= 0)
        return Results.BadRequest(new { error = "Amount must be > 0." });

    // ayni key ama farklı request için request hash
    var bodyHash = IdempotencyStore.ComputeBodyHash(req);

    // cache response var mı
    if (store.TryGet(key, out var cached))
    {
        if (!string.Equals(cached.BodyHash, bodyHash, StringComparison.OrdinalIgnoreCase))
            return Results.Conflict(new
            {
                error = "Farklı request body ile aynı Idempotency-Key kullanılamaz.",
                hint = "Farklı transfer için yeni bir Idempotency-Key kullanın."
            });

        return Results.Json(
            data: System.Text.Json.JsonSerializer.Deserialize<object>(cached.JsonBody)!,
            statusCode: cached.StatusCode
        );
        

        // return Results.Text(
        //     cached.JsonBody,
        //     contentType: "application/json",
        //     statusCode: cached.StatusCode
        // );
    }

    var transferId = $"tr_{Guid.NewGuid():N}";
    var response = new TransferResponse(
        TransferId: transferId,
        FromAccountId: req.FromAccountId,
        ToAccountId: req.ToAccountId,
        Amount: req.Amount,
        CreatedAt: DateTimeOffset.UtcNow,
        IdempotencyKey: key
    );

    // Cache response
    var json = System.Text.Json.JsonSerializer.Serialize(response);
    store.TryPut(key, new CachedResult(
        StatusCode: StatusCodes.Status201Created,
        JsonBody: json,
        BodyHash: bodyHash,
        CreatedAt: DateTimeOffset.UtcNow
    ));

    return Results.Created($"/transfer/{transferId}", response);
});

app.Run();
