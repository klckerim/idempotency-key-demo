namespace IdempotencyDemo.Api;

public sealed record TransferRequest(string FromAccountId, string ToAccountId, decimal Amount);

public sealed record TransferResponse(
    string TransferId,
    string FromAccountId,
    string ToAccountId,
    decimal Amount,
    DateTimeOffset CreatedAt,
    string IdempotencyKey
);
