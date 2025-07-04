# Mock Transactions API

Minimal REST API **mock** built with **.NET 9 Preview** to simulate a payment‐hub endpoint that lists transactions filtered by `externalReference`. It is meant for local development, automated tests and demos when the real upstream service is unavailable.

---

## Features

| Behaviour                              | Probability | Details                                                                                                     |
| -------------------------------------- | ----------- | ----------------------------------------------------------------------------------------------------------- |
| **Single _COMPLETED_ transaction**     | **70 %**    | Returns one record with status `COMPLETED`.                                                                 |
| **Two records _PENDING_ ➜ _REVERSED_** | 15 %        | Simulates an in‑flight transaction that was later reversed. First element = `PENDING`, second = `REVERSED`. |
| **HTTP 500 Timeout error**             | 15 %        | Mimics an upstream timeout, returning RFC 9457 Problem+JSON.                                                |

All three scenarios are chosen randomly on each request, giving your client code or QA scripts a realistic mix of happy‑path and failure conditions.

---

## 🚀 Quick Start

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/) Preview (install via `dotnet --version` → `9.0.0-preview`)

### Run locally

```bash
# clone and build
 dotnet restore
 dotnet run --urls http://0.0.0.0:5000
```

> The API listens on **[http://localhost:5000](http://localhost:5000)** and automatically redirects to HTTPS on port `5001` if ASP.NET dev‑certs are present.

### Docker

```bash
# Build image
 docker build -t mock-transactions-api .

# Run container
 docker run -p 5000:5000 mock-transactions-api
```

---

## 🔌 Endpoint reference

### `GET /transactions`

| Query parameter     | Type     | Required | Default | Description                             |
| ------------------- | -------- | -------- | ------- | --------------------------------------- |
| `externalReference` | `string` | ✅       | –       | Identifier used to filter transactions. |
| `offset`            | `int`    | ❌       | `0`     | Zero‑based index for pagination.        |
| `limit`             | `int`    | ❌       | `100`   | Page size.                              |

#### Successful response `200 OK`

```jsonc
{
  "total": 1,
  "offset": 0,
  "limit": 100,
  "data": [
    {
      "transactionId": "d1db6b20‑e2e0‑42b7‑8235‑4245b6251f9e",
      "transactionType": "PAYMENT",
      "status": "COMPLETED",
      "externalReference": "abc‑123",
      "createdAt": "2025‑07‑02T15:04:33Z",
      "completedAt": "2025‑07‑02T15:04:33Z",
      "reversedAt": null,
      "finalizedAt": null
    }
  ]
}
```

#### Error response `500 Internal Server Error`

```jsonc
{
  "type": "about:blank",
  "title": "Timeout contacting upstream service",
  "status": 500,
  "detail": "The external provider did not respond within the allotted time."
}
```

---

## 🛠️ Testing with `curl`

```bash
curl -k "https://localhost:5001/transactions?externalReference=abc-123"
```

Run the command repeatedly to observe the three possible outcomes.

---

## 🤖 Using Postman

1. **Create a new collection** → _Mock Transactions API_.
2. Add a **GET** request to `{{base_url}}/transactions` and set query params:

   - `externalReference` = `abc-123`
   - `offset` = `0`
   - `limit` = `100`

3. Define an environment variable `base_url` pointing to `https://localhost:5001` (or `http://localhost:5000`).
4. Hit **Send** multiple times; Postman will show 200 or 500 responses according to the random logic.

Example test script (Tests tab):

```javascript
pm.test("Valid status code", () => {
  pm.expect(pm.response.code).to.be.oneOf([200, 500]);
});
```

---

## 📜 OpenAPI & Swagger UI

- The contract is auto‑generated by ASP.NET’s **`AddOpenApi`**.

  - **`/openapi.json`** (or `/openapi.yaml`) – machine‑readable spec.

- To add Swagger UI interactively:

  ```csharp
  builder.Services.AddEndpointsApiExplorer();
  builder.Services.AddSwaggerGen();
  app.UseSwagger();
  app.UseSwaggerUI();      // now browse to /swagger
  ```

---

## 👩‍💻 Contributing

Pull requests are welcome. Feel free to open an issue if you need other scenarios (e.g. 401, 429, network delay injectors).

---

## 📝 License

Distributed under the MIT License. See `LICENSE` for more information.
