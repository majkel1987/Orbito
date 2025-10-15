using FluentAssertions;
using FluentAssertions.Execution;
using Orbito.Application.Common.Models.PaymentGateway;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.Events;
using Orbito.Domain.ValueObjects;

namespace Orbito.Tests.Helpers.Assertions;

/// <summary>
/// Custom FluentAssertions extensions for Payment-related tests
/// </summary>
public static class PaymentAssertions
{
    /// <summary>
    /// Asserts that a payment is in successful state
    /// </summary>
    public static AndConstraint<Payment> ShouldBeSuccessfulPayment(this Payment payment, string because = "", params object[] becauseArgs)
    {
        using (new AssertionScope())
        {
            payment.Should().NotBeNull(because, becauseArgs);
            payment.Status.Should().Be(PaymentStatus.Completed, because, becauseArgs);
            payment.ProcessedAt.Should().NotBeNull(because, becauseArgs);
            payment.FailureReason.Should().BeNull(because, becauseArgs);
        }

        return new AndConstraint<Payment>(payment);
    }

    /// <summary>
    /// Asserts that a payment is in failed state
    /// </summary>
    public static AndConstraint<Payment> ShouldBeFailedPayment(this Payment payment, string because = "", params object[] becauseArgs)
    {
        using (new AssertionScope())
        {
            payment.Should().NotBeNull(because, becauseArgs);
            payment.Status.Should().Be(PaymentStatus.Failed, because, becauseArgs);
            payment.FailedAt.Should().NotBeNull(because, becauseArgs);
            payment.FailureReason.Should().NotBeNullOrEmpty(because, becauseArgs);
        }

        return new AndConstraint<Payment>(payment);
    }

    /// <summary>
    /// Asserts that a payment is in pending state
    /// </summary>
    public static AndConstraint<Payment> ShouldBePendingPayment(this Payment payment, string because = "", params object[] becauseArgs)
    {
        using (new AssertionScope())
        {
            payment.Should().NotBeNull(because, becauseArgs);
            payment.Status.Should().Be(PaymentStatus.Pending, because, becauseArgs);
            payment.ProcessedAt.Should().BeNull(because, becauseArgs);
            payment.FailedAt.Should().BeNull(because, becauseArgs);
        }

        return new AndConstraint<Payment>(payment);
    }

    /// <summary>
    /// Asserts that a payment is in processing state
    /// </summary>
    public static AndConstraint<Payment> ShouldBeProcessingPayment(this Payment payment, string because = "", params object[] becauseArgs)
    {
        using (new AssertionScope())
        {
            payment.Should().NotBeNull(because, becauseArgs);
            payment.Status.Should().Be(PaymentStatus.Processing, because, becauseArgs);
            payment.ProcessedAt.Should().BeNull(because, becauseArgs);
            payment.FailedAt.Should().BeNull(because, becauseArgs);
        }

        return new AndConstraint<Payment>(payment);
    }

    /// <summary>
    /// Asserts that a payment is refunded
    /// </summary>
    public static AndConstraint<Payment> ShouldBeRefundedPayment(this Payment payment, string because = "", params object[] becauseArgs)
    {
        using (new AssertionScope())
        {
            payment.Should().NotBeNull(because, becauseArgs);
            payment.Status.Should().BeOneOf(new[] { PaymentStatus.Refunded, PaymentStatus.PartiallyRefunded }, because, becauseArgs);
            payment.RefundedAt.Should().NotBeNull(because, becauseArgs);
            payment.RefundReason.Should().NotBeNullOrEmpty(because, becauseArgs);
        }

        return new AndConstraint<Payment>(payment);
    }

    /// <summary>
    /// Asserts that a payment has a specific domain event
    /// </summary>
    public static AndConstraint<Payment> ShouldHaveDomainEvent<T>(this Payment payment, string because = "", params object[] becauseArgs) where T : class
    {
        using (new AssertionScope())
        {
            payment.Should().NotBeNull(because, becauseArgs);
            payment.DomainEvents.Should().ContainSingle(e => e is T, because, becauseArgs);
        }

        return new AndConstraint<Payment>(payment);
    }

    /// <summary>
    /// Asserts that a payment has no domain events
    /// </summary>
    public static AndConstraint<Payment> ShouldHaveNoDomainEvents(this Payment payment, string because = "", params object[] becauseArgs)
    {
        using (new AssertionScope())
        {
            payment.Should().NotBeNull(because, becauseArgs);
            payment.DomainEvents.Should().BeEmpty(because, becauseArgs);
        }

        return new AndConstraint<Payment>(payment);
    }

    /// <summary>
    /// Asserts that a payment matches Stripe data
    /// </summary>
    public static AndConstraint<Payment> ShouldMatchStripeData(this Payment payment, string externalTransactionId, string because = "", params object[] becauseArgs)
    {
        using (new AssertionScope())
        {
            payment.Should().NotBeNull(because, becauseArgs);
            payment.ExternalTransactionId.Should().Be(externalTransactionId, because, becauseArgs);
        }

        return new AndConstraint<Payment>(payment);
    }

    /// <summary>
    /// Asserts that a payment is idempotent (has idempotency key)
    /// </summary>
    public static AndConstraint<Payment> ShouldBeIdempotent(this Payment payment, string because = "", params object[] becauseArgs)
    {
        using (new AssertionScope())
        {
            payment.Should().NotBeNull(because, becauseArgs);
            payment.IdempotencyKey.Should().NotBeNull(because, becauseArgs);
        }

        return new AndConstraint<Payment>(payment);
    }

    /// <summary>
    /// Asserts that a payment can be retried
    /// </summary>
    public static AndConstraint<Payment> ShouldBeRetryable(this Payment payment, string because = "", params object[] becauseArgs)
    {
        using (new AssertionScope())
        {
            payment.Should().NotBeNull(because, becauseArgs);
            payment.CanBeRetried().Should().BeTrue(because, becauseArgs);
        }

        return new AndConstraint<Payment>(payment);
    }

    /// <summary>
    /// Asserts that a payment cannot be retried
    /// </summary>
    public static AndConstraint<Payment> ShouldNotBeRetryable(this Payment payment, string because = "", params object[] becauseArgs)
    {
        using (new AssertionScope())
        {
            payment.Should().NotBeNull(because, becauseArgs);
            payment.CanBeRetried().Should().BeFalse(because, becauseArgs);
        }

        return new AndConstraint<Payment>(payment);
    }

    /// <summary>
    /// Asserts that a payment can be refunded
    /// </summary>
    public static AndConstraint<Payment> ShouldBeRefundable(this Payment payment, string because = "", params object[] becauseArgs)
    {
        using (new AssertionScope())
        {
            payment.Should().NotBeNull(because, becauseArgs);
            payment.CanBeRefunded().Should().BeTrue(because, becauseArgs);
        }

        return new AndConstraint<Payment>(payment);
    }

    /// <summary>
    /// Asserts that a payment cannot be refunded
    /// </summary>
    public static AndConstraint<Payment> ShouldNotBeRefundable(this Payment payment, string because = "", params object[] becauseArgs)
    {
        using (new AssertionScope())
        {
            payment.Should().NotBeNull(because, becauseArgs);
            payment.CanBeRefunded().Should().BeFalse(because, becauseArgs);
        }

        return new AndConstraint<Payment>(payment);
    }

    /// <summary>
    /// Asserts that a payment method is valid and not expired
    /// </summary>
    public static AndConstraint<PaymentMethod> ShouldBeValidPaymentMethod(this PaymentMethod paymentMethod, string because = "", params object[] becauseArgs)
    {
        using (new AssertionScope())
        {
            paymentMethod.Should().NotBeNull(because, becauseArgs);
            paymentMethod.CanBeUsed().Should().BeTrue(because, becauseArgs);
            paymentMethod.IsExpired().Should().BeFalse(because, becauseArgs);
        }

        return new AndConstraint<PaymentMethod>(paymentMethod);
    }

    /// <summary>
    /// Asserts that a payment method is expired
    /// </summary>
    public static AndConstraint<PaymentMethod> ShouldBeExpiredPaymentMethod(this PaymentMethod paymentMethod, string because = "", params object[] becauseArgs)
    {
        using (new AssertionScope())
        {
            paymentMethod.Should().NotBeNull(because, becauseArgs);
            paymentMethod.IsExpired().Should().BeTrue(because, becauseArgs);
            paymentMethod.CanBeUsed().Should().BeFalse(because, becauseArgs);
        }

        return new AndConstraint<PaymentMethod>(paymentMethod);
    }

    /// <summary>
    /// Asserts that a payment method is set as default
    /// </summary>
    public static AndConstraint<PaymentMethod> ShouldBeDefaultPaymentMethod(this PaymentMethod paymentMethod, string because = "", params object[] becauseArgs)
    {
        using (new AssertionScope())
        {
            paymentMethod.Should().NotBeNull(because, becauseArgs);
            paymentMethod.IsDefault.Should().BeTrue(because, becauseArgs);
        }

        return new AndConstraint<PaymentMethod>(paymentMethod);
    }

    /// <summary>
    /// Asserts that a payment retry schedule is in scheduled state
    /// </summary>
    public static AndConstraint<PaymentRetrySchedule> ShouldBeScheduledRetry(this PaymentRetrySchedule retrySchedule, string because = "", params object[] becauseArgs)
    {
        using (new AssertionScope())
        {
            retrySchedule.Should().NotBeNull(because, becauseArgs);
            retrySchedule.Status.Should().Be(RetryStatus.Scheduled, because, becauseArgs);
        }

        return new AndConstraint<PaymentRetrySchedule>(retrySchedule);
    }

    /// <summary>
    /// Asserts that a payment retry schedule can be retried
    /// </summary>
    public static AndConstraint<PaymentRetrySchedule> ShouldBeRetryable(this PaymentRetrySchedule retrySchedule, string because = "", params object[] becauseArgs)
    {
        using (new AssertionScope())
        {
            retrySchedule.Should().NotBeNull(because, becauseArgs);
            retrySchedule.CanRetry().Should().BeTrue(because, becauseArgs);
        }

        return new AndConstraint<PaymentRetrySchedule>(retrySchedule);
    }

    /// <summary>
    /// Asserts that a payment retry schedule is overdue
    /// </summary>
    public static AndConstraint<PaymentRetrySchedule> ShouldBeOverdue(this PaymentRetrySchedule retrySchedule, string because = "", params object[] becauseArgs)
    {
        using (new AssertionScope())
        {
            retrySchedule.Should().NotBeNull(because, becauseArgs);
            retrySchedule.IsOverdue().Should().BeTrue(because, becauseArgs);
        }

        return new AndConstraint<PaymentRetrySchedule>(retrySchedule);
    }

    /// <summary>
    /// Asserts that a payment result is successful
    /// </summary>
    public static AndConstraint<PaymentResult> ShouldBeSuccessful(this PaymentResult result, string because = "", params object[] becauseArgs)
    {
        using (new AssertionScope())
        {
            result.Should().NotBeNull(because, becauseArgs);
            result.IsSuccess.Should().BeTrue(because, becauseArgs);
            result.TransactionId.Should().NotBeNullOrEmpty(because, becauseArgs);
            result.ErrorMessage.Should().BeNull(because, becauseArgs);
        }

        return new AndConstraint<PaymentResult>(result);
    }

    /// <summary>
    /// Asserts that a payment result is failed
    /// </summary>
    public static AndConstraint<PaymentResult> ShouldBeFailed(this PaymentResult result, string because = "", params object[] becauseArgs)
    {
        using (new AssertionScope())
        {
            result.Should().NotBeNull(because, becauseArgs);
            result.IsSuccess.Should().BeFalse(because, becauseArgs);
            result.ErrorMessage.Should().NotBeNullOrEmpty(because, becauseArgs);
            result.ErrorCode.Should().NotBeNullOrEmpty(because, becauseArgs);
        }

        return new AndConstraint<PaymentResult>(result);
    }

    /// <summary>
    /// Asserts that a refund result is successful
    /// </summary>
    public static AndConstraint<RefundResult> ShouldBeSuccessful(this RefundResult result, string because = "", params object[] becauseArgs)
    {
        using (new AssertionScope())
        {
            result.Should().NotBeNull(because, becauseArgs);
            result.IsSuccess.Should().BeTrue(because, becauseArgs);
            result.TransactionId.Should().NotBeNullOrEmpty(because, becauseArgs);
            result.ErrorMessage.Should().BeNull(because, becauseArgs);
        }

        return new AndConstraint<RefundResult>(result);
    }

    /// <summary>
    /// Asserts that a refund result is failed
    /// </summary>
    public static AndConstraint<RefundResult> ShouldBeFailed(this RefundResult result, string because = "", params object[] becauseArgs)
    {
        using (new AssertionScope())
        {
            result.Should().NotBeNull(because, becauseArgs);
            result.IsSuccess.Should().BeFalse(because, becauseArgs);
            result.ErrorMessage.Should().NotBeNullOrEmpty(because, becauseArgs);
            result.ErrorCode.Should().NotBeNullOrEmpty(because, becauseArgs);
        }

        return new AndConstraint<RefundResult>(result);
    }

    /// <summary>
    /// Asserts that a customer result is successful
    /// </summary>
    public static AndConstraint<CustomerResult> ShouldBeSuccessful(this CustomerResult result, string because = "", params object[] becauseArgs)
    {
        using (new AssertionScope())
        {
            result.Should().NotBeNull(because, becauseArgs);
            result.IsSuccess.Should().BeTrue(because, becauseArgs);
            result.ExternalCustomerId.Should().NotBeNullOrEmpty(because, becauseArgs);
            result.ErrorMessage.Should().BeNull(because, becauseArgs);
        }

        return new AndConstraint<CustomerResult>(result);
    }

    /// <summary>
    /// Asserts that a customer result is failed
    /// </summary>
    public static AndConstraint<CustomerResult> ShouldBeFailed(this CustomerResult result, string because = "", params object[] becauseArgs)
    {
        using (new AssertionScope())
        {
            result.Should().NotBeNull(because, becauseArgs);
            result.IsSuccess.Should().BeFalse(because, becauseArgs);
            result.ErrorMessage.Should().NotBeNullOrEmpty(because, becauseArgs);
            result.ErrorCode.Should().NotBeNullOrEmpty(because, becauseArgs);
        }

        return new AndConstraint<CustomerResult>(result);
    }

}
