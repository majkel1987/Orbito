# Implementation Notes for Future Tasks

## 🔄 Outbox Pattern for Distributed Transactions

### Problem
Currently, payment processing has a potential inconsistency:
1. Payment record is created in DB
2. Payment gateway is called (external service)
3. If gateway succeeds but DB update fails → inconsistent state

### Solution: Outbox Pattern

#### Step 1: Create OutboxEvent Table

```csharp
public class OutboxEvent
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string EventType { get; private set; } // "ProcessPayment", "RefundPayment", etc.
    public string Payload { get; private set; } // JSON serialized data
    public OutboxEventStatus Status { get; private set; } // Pending, Processing, Completed, Failed
    public int RetryCount { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public string? ErrorMessage { get; private set; }
}

public enum OutboxEventStatus
{
    Pending,
    Processing,
    Completed,
    Failed
}
```

#### Step 2: Migration

```bash
dotnet ef migrations add AddOutboxEventTable --project Orbito.Infrastructure --startup-project Orbito.API
```

#### Step 3: Modify PaymentProcessingService

```csharp
public async Task<PaymentResult> ProcessSubscriptionPaymentAsync(...)
{
    using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

    try
    {
        // 1. Validate subscription and payment method
        var (isValid, error, subscription) = await ValidateSubscriptionPaymentAsync(...);
        if (!isValid) return error!;

        // 2. Create payment record
        var payment = await CreatePaymentRecordAsync(subscription, amount, cancellationToken);

        // 3. Create outbox event for payment gateway
        var outboxEvent = new OutboxEvent
        {
            EventType = "ProcessPayment",
            Payload = JsonSerializer.Serialize(new ProcessPaymentOutboxData
            {
                PaymentId = payment.Id,
                SubscriptionId = subscriptionId,
                Amount = amount,
                PaymentMethodId = paymentMethodId,
                Description = description
            }),
            Status = OutboxEventStatus.Pending,
            TenantId = subscription.TenantId
        };

        await _unitOfWork.OutboxEvents.AddAsync(outboxEvent, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        // 4. Background worker will pick up and process
        return PaymentResult.Success(payment.Id);
    }
    catch
    {
        await transaction.RollbackAsync(cancellationToken);
        throw;
    }
}
```

#### Step 4: Background Worker (Hangfire/Quartz)

```csharp
public class OutboxEventProcessor : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var pendingEvents = await _unitOfWork.OutboxEvents
                    .GetPendingEventsAsync(batchSize: 10, stoppingToken);

                foreach (var evt in pendingEvents)
                {
                    await ProcessEventAsync(evt, stoppingToken);
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in outbox processor");
            }
        }
    }

    private async Task ProcessEventAsync(OutboxEvent evt, CancellationToken ct)
    {
        try
        {
            evt.MarkAsProcessing();
            await _unitOfWork.SaveChangesAsync(ct);

            switch (evt.EventType)
            {
                case "ProcessPayment":
                    var data = JsonSerializer.Deserialize<ProcessPaymentOutboxData>(evt.Payload);
                    var result = await _paymentGateway.ProcessPaymentAsync(...);

                    if (result.IsSuccess)
                    {
                        // Update payment status
                        var payment = await _unitOfWork.Payments.GetByIdAsync(data.PaymentId, ct);
                        payment.MarkAsCompleted();
                        await _unitOfWork.Payments.UpdateAsync(payment, ct);

                        evt.MarkAsCompleted();
                    }
                    else
                    {
                        evt.MarkAsFailed(result.ErrorMessage);
                    }
                    break;
            }

            await _unitOfWork.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            evt.IncrementRetryCount();
            evt.MarkAsFailed(ex.Message);
            await _unitOfWork.SaveChangesAsync(ct);
        }
    }
}
```

#### Step 5: Register in Program.cs

```csharp
services.AddHostedService<OutboxEventProcessor>();
```

---

## 📧 Email Retry Mechanism

### Problem
Email sending can fail due to:
- Network issues
- SMTP server unavailability
- Rate limiting
- Temporary errors

### Solution: Background Job Queue with Retry

#### Step 1: Install Hangfire (recommended)

```bash
dotnet add package Hangfire.AspNetCore
dotnet add package Hangfire.SqlServer
```

#### Step 2: Configure Hangfire

```csharp
// appsettings.json
{
  "Hangfire": {
    "ConnectionString": "Server=...;Database=Orbito_Hangfire;...",
    "DashboardPath": "/hangfire",
    "ServerName": "Orbito-Production"
  }
}

// Program.cs
builder.Services.AddHangfire(config =>
{
    config.UseSqlServerStorage(builder.Configuration.GetConnectionString("Hangfire"));
});

builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = 5; // Number of concurrent workers
    options.Queues = new[] { "default", "emails" }; // Separate queue for emails
});

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() }
});
```

#### Step 3: Modify PaymentNotificationService

```csharp
public class PaymentNotificationService : IPaymentNotificationService
{
    private readonly IBackgroundJobClient _backgroundJobs;

    public PaymentNotificationService(
        IUnitOfWork unitOfWork,
        IEmailSender emailSender,
        ILogger<PaymentNotificationService> logger,
        ITenantContext tenantContext,
        IMemoryCache cache,
        IBackgroundJobClient backgroundJobs)
    {
        _backgroundJobs = backgroundJobs;
        // ... other dependencies
    }

    public async Task SendPaymentConfirmationAsync(Guid paymentId, CancellationToken cancellationToken = default)
    {
        try
        {
            // ... existing code to build email

            // Send email with retry
            await SendEmailWithRetryAsync(
                email: client.Email,
                subject: subject,
                body: body,
                context: new EmailContext
                {
                    PaymentId = paymentId,
                    EmailType = "PaymentConfirmation"
                },
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending payment confirmation for {PaymentId}", paymentId);
            // Don't throw - already scheduled for retry
        }
    }

    private async Task SendEmailWithRetryAsync(
        string email,
        string subject,
        string body,
        EmailContext context,
        CancellationToken cancellationToken,
        int attempt = 0)
    {
        const int MaxRetries = 3;

        try
        {
            await SendValidatedEmailAsync(email, subject, body, cancellationToken);
            _logger.LogInformation("Email sent successfully: {EmailType} to {Email}",
                context.EmailType, email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Email send failed (attempt {Attempt}/{MaxRetries}): {EmailType}",
                attempt + 1, MaxRetries, context.EmailType);

            if (attempt < MaxRetries)
            {
                // Schedule retry with exponential backoff
                var delay = TimeSpan.FromMinutes(Math.Pow(2, attempt)); // 1min, 2min, 4min

                _backgroundJobs.Schedule(
                    () => SendEmailWithRetryAsync(email, subject, body, context, CancellationToken.None, attempt + 1),
                    delay);

                _logger.LogInformation("Email retry scheduled in {Delay} for {EmailType}",
                    delay, context.EmailType);
            }
            else
            {
                _logger.LogError("Email send failed after {MaxRetries} attempts: {EmailType}. Giving up.",
                    MaxRetries, context.EmailType);

                // Optionally: Store in dead letter queue for manual review
                await StoreFailedEmailAsync(email, subject, body, context, ex.Message, cancellationToken);
            }
        }
    }

    private async Task StoreFailedEmailAsync(
        string email,
        string subject,
        string body,
        EmailContext context,
        string errorMessage,
        CancellationToken cancellationToken)
    {
        // Store in database for manual review/resend
        var failedEmail = new FailedEmail
        {
            Email = email,
            Subject = subject,
            Body = body,
            EmailType = context.EmailType,
            PaymentId = context.PaymentId,
            ErrorMessage = errorMessage,
            CreatedAt = DateTime.UtcNow,
            RetryCount = 3
        };

        await _unitOfWork.FailedEmails.AddAsync(failedEmail, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

public class EmailContext
{
    public string EmailType { get; set; } = string.Empty;
    public Guid? PaymentId { get; set; }
    public Guid? SubscriptionId { get; set; }
}
```

#### Step 4: Create FailedEmail Entity

```csharp
public class FailedEmail
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string EmailType { get; set; } = string.Empty;
    public Guid? PaymentId { get; set; }
    public Guid? SubscriptionId { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public int RetryCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ResentAt { get; set; }
    public bool IsResolved { get; set; }
}
```

#### Step 5: Admin Dashboard for Failed Emails

```csharp
[ApiController]
[Route("api/admin/failed-emails")]
[Authorize(Roles = "PlatformAdmin")]
public class FailedEmailsController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetFailedEmails(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var failedEmails = await _unitOfWork.FailedEmails
            .GetPaginatedAsync(page, pageSize);

        return Ok(failedEmails);
    }

    [HttpPost("{id}/retry")]
    public async Task<IActionResult> RetryFailedEmail(Guid id)
    {
        var failedEmail = await _unitOfWork.FailedEmails.GetByIdAsync(id);
        if (failedEmail == null)
            return NotFound();

        // Schedule for immediate retry
        _backgroundJobs.Enqueue(() =>
            _notificationService.SendEmailWithRetryAsync(
                failedEmail.Email,
                failedEmail.Subject,
                failedEmail.Body,
                new EmailContext { EmailType = failedEmail.EmailType },
                CancellationToken.None,
                0));

        failedEmail.ResentAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return Ok();
    }
}
```

---

## 📊 Benefits Summary

### Outbox Pattern
- ✅ Guarantees eventual consistency
- ✅ Prevents data loss
- ✅ Enables retry logic
- ✅ Audit trail of all payment gateway calls
- ⚠️ Requires background worker infrastructure
- ⚠️ Adds complexity to deployment

### Email Retry Mechanism
- ✅ Handles transient failures
- ✅ Exponential backoff prevents spam
- ✅ Dead letter queue for manual review
- ✅ Admin dashboard for monitoring
- ⚠️ Requires Hangfire/Quartz infrastructure
- ⚠️ Additional database for job storage

---

## 🎯 Recommended Implementation Order

1. **Email Retry** (Easier, immediate value)
   - Install Hangfire
   - Modify PaymentNotificationService
   - Create FailedEmail entity
   - Add admin dashboard

2. **Outbox Pattern** (More complex, architectural change)
   - Design outbox event schema
   - Create migration
   - Modify PaymentProcessingService
   - Implement background processor
   - Testing and monitoring

---

## 📝 Additional Considerations

### Monitoring
- Add Application Insights/Prometheus metrics
- Track outbox event processing time
- Monitor email retry rates
- Alert on high failure rates

### Testing
- Integration tests for outbox processor
- Mock SMTP failures for email retry
- Load testing for concurrent processing
- Chaos engineering for failure scenarios

### Performance
- Index outbox events by (Status, CreatedAt)
- Partition failed emails table by month
- Implement batch processing for emails
- Consider message queue (RabbitMQ/Azure Service Bus) for high volume
