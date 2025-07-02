using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var rng = new Random();

app.MapGet("/transactions",
    (
        [FromQuery] string externalReference,
        [FromQuery] int offset = 0,
        [FromQuery] int limit  = 100
    ) =>
    {
        int roll = rng.Next(100);        // 0–99

        // ---------- 70 % → COMPLETED ----------
        if (roll < 70)
        {
            var data = new[] {
                new {
                    transactionId     = Guid.NewGuid(),
                    transactionType   = "PAYMENT",
                    status            = "COMPLETED",
                    externalReference,
                    createdAt         = DateTime.UtcNow,
                    completedAt       = DateTime.UtcNow,
                    reversedAt        = (DateTime?)null,
                    finalizedAt       = (DateTime?)null
                }
            };

            return Results.Ok(new { total = data.Length, offset, limit, data });
        }

        // ---------- 15 % → PENDING + REVERSED ----------
        if (roll < 85)                       // 70–84
        {
            var data = new object[] {
                new {
                    transactionId     = Guid.NewGuid(),
                    transactionType   = "PAYMENT",
                    status            = "PENDING",
                    externalReference,
                    createdAt         = DateTime.UtcNow,
                    completedAt       = (DateTime?)null,
                    reversedAt        = (DateTime?)null,
                    finalizedAt       = (DateTime?)null
                },
                new {
                    transactionId     = Guid.NewGuid(),
                    transactionType   = "PAYMENT",
                    status            = "REVERSED",
                    externalReference,
                    createdAt         = DateTime.UtcNow.AddMinutes(-5),
                    completedAt       = DateTime.UtcNow.AddMinutes(-4),
                    reversedAt        = DateTime.UtcNow.AddMinutes(-3),
                    finalizedAt       = DateTime.UtcNow.AddMinutes(-2)
                }
            };

            return Results.Ok(new { total = data.Length, offset, limit, data });
        }

        // ---------- 15 % → Error 500 ----------
        return Results.Problem(
            statusCode: 500,
            title:  "Timeout contacting upstream service",
            detail: "The external provider did not respond within the allotted time."
        );
    })
    .WithName("GetTransactions")
    .WithOpenApi();    // Incluye la descripción en el contrato OpenAPI


app.Run();