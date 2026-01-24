# Idempotency Key Demo

POST /transfer

```bash
curl -i -X POST "http://localhost:5077/transfer" \
  -H "Content-Type: application/json" \
  -H "Idempotency-Key: abc-123" \
  -d '{"fromAccountId":"A1","toAccountId":"B1","amount":100}'
```

Response:
HTTP/1.1 201 Created

```json
{
  "TransferId": "tr_fbb458287535414f9a8aca1a6ec8fe7b",
  "FromAccountId": "A1",
  "ToAccountId": "B1",
  "Amount": 100,
  "CreatedAt": "2026-01-24T13:12:16.284026+00:00",
  "IdempotencyKey": "abc-123"
}
```

Same key different body: 

```bash
curl -i -X POST "http://localhost:5077/transfer" \
  -H "Content-Type: application/json" \
  -H "Idempotency-Key: abc-123" \
  -d '{"fromAccountId":"A1","toAccountId":"B1","amount":200}'
```

Response:
HTTP/1.1 409 Conflict

```json
{
  "error": "Farklı request body ile aynı Idempotency-Key kullanılamaz.",
  "hint": "Farklı transfer için yeni bir Idempotency-Key kullanın."
}
```
