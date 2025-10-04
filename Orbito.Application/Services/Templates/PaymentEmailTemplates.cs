using Orbito.Domain.ValueObjects;

namespace Orbito.Application.Services.Templates;

/// <summary>
/// Email templates for payment notifications
/// </summary>
public static class PaymentEmailTemplates
{
    /// <summary>
    /// Gets payment confirmation email subject
    /// </summary>
    public static string GetPaymentConfirmationSubject()
    {
        return "Payment Confirmation - Your payment was successful";
    }

    /// <summary>
    /// Gets payment confirmation email body
    /// </summary>
    public static string GetPaymentConfirmationBody(
        string clientName,
        Money amount,
        string transactionId,
        DateTime paymentDate,
        string subscriptionName)
    {
        return $@"
Dear {clientName},

Thank you for your payment. We have successfully processed your payment.

Payment Details:
-----------------
Amount: {amount.Amount:C} {amount.Currency}
Transaction ID: {transactionId}
Payment Date: {paymentDate:yyyy-MM-dd HH:mm:ss} UTC
Subscription: {subscriptionName}

Your subscription is now active and you can continue enjoying our services.

If you have any questions, please don't hesitate to contact our support team.

Best regards,
The Orbito Team
";
    }

    /// <summary>
    /// Gets payment failure email subject
    /// </summary>
    public static string GetPaymentFailureSubject()
    {
        return "Payment Failed - Action Required";
    }

    /// <summary>
    /// Gets payment failure email body
    /// </summary>
    public static string GetPaymentFailureBody(
        string clientName,
        Money amount,
        string reason,
        DateTime attemptDate,
        string subscriptionName)
    {
        return $@"
Dear {clientName},

Unfortunately, we were unable to process your payment.

Payment Details:
-----------------
Amount: {amount.Amount:C} {amount.Currency}
Reason: {reason}
Attempt Date: {attemptDate:yyyy-MM-dd HH:mm:ss} UTC
Subscription: {subscriptionName}

What happens next:
- Please update your payment method or contact your bank
- We will automatically retry the payment in 3 days
- Your subscription may be suspended if payment continues to fail

To update your payment method, please log in to your account.

If you need assistance, please contact our support team.

Best regards,
The Orbito Team
";
    }

    /// <summary>
    /// Gets refund confirmation email subject
    /// </summary>
    public static string GetRefundConfirmationSubject()
    {
        return "Refund Confirmation - Your refund is being processed";
    }

    /// <summary>
    /// Gets refund confirmation email body
    /// </summary>
    public static string GetRefundConfirmationBody(
        string clientName,
        Money refundAmount,
        Money originalAmount,
        string transactionId,
        DateTime refundDate,
        string reason)
    {
        // SECURITY: Validate currency match
        if (refundAmount.Currency != originalAmount.Currency)
        {
            throw new ArgumentException($"Currency mismatch: refund is {refundAmount.Currency}, original is {originalAmount.Currency}");
        }

        // SECURITY: Validate refund doesn't exceed original amount
        if (refundAmount.Amount > originalAmount.Amount)
        {
            throw new ArgumentException($"Refund amount ({refundAmount.Amount}) exceeds original amount ({originalAmount.Amount})");
        }

        return $@"
Dear {clientName},

We have processed your refund request.

Refund Details:
-----------------
Refund Amount: {refundAmount.Amount:C} {refundAmount.Currency}
Original Amount: {originalAmount.Amount:C} {originalAmount.Currency}
Transaction ID: {transactionId}
Refund Date: {refundDate:yyyy-MM-dd HH:mm:ss} UTC
Reason: {reason}

The refund will be credited to your original payment method within 5-10 business days.

If you have any questions about this refund, please contact our support team.

Best regards,
The Orbito Team
";
    }

    /// <summary>
    /// Gets partial refund confirmation email body
    /// </summary>
    public static string GetPartialRefundConfirmationBody(
        string clientName,
        Money refundAmount,
        Money originalAmount,
        string transactionId,
        DateTime refundDate,
        string reason)
    {
        // SECURITY: Validate currency match
        if (refundAmount.Currency != originalAmount.Currency)
        {
            throw new ArgumentException($"Currency mismatch: refund is {refundAmount.Currency}, original is {originalAmount.Currency}");
        }

        // SECURITY: Validate refund doesn't exceed original amount
        if (refundAmount.Amount > originalAmount.Amount)
        {
            throw new ArgumentException($"Refund amount ({refundAmount.Amount}) exceeds original amount ({originalAmount.Amount})");
        }

        var remainingAmount = Money.Create(originalAmount.Amount - refundAmount.Amount, originalAmount.Currency);
        
        return $@"
Dear {clientName},

We have processed your partial refund request.

Partial Refund Details:
-----------------
Refund Amount: {refundAmount.Amount:C} {refundAmount.Currency}
Original Amount: {originalAmount.Amount:C} {originalAmount.Currency}
Remaining Amount: {remainingAmount.Amount:C} {remainingAmount.Currency}
Transaction ID: {transactionId}
Refund Date: {refundDate:yyyy-MM-dd HH:mm:ss} UTC
Reason: {reason}

The partial refund will be credited to your original payment method within 5-10 business days.

If you have any questions about this refund, please contact our support team.

Best regards,
The Orbito Team
";
    }

    /// <summary>
    /// Gets upcoming payment reminder email subject
    /// </summary>
    public static string GetUpcomingPaymentReminderSubject(int daysUntilPayment)
    {
        return $"Upcoming Payment - Payment due in {daysUntilPayment} days";
    }

    /// <summary>
    /// Gets upcoming payment reminder email body
    /// </summary>
    public static string GetUpcomingPaymentReminderBody(
        string clientName,
        Money amount,
        DateTime dueDate,
        string subscriptionName,
        string paymentMethodLast4)
    {
        return $@"
Dear {clientName},

This is a friendly reminder that your subscription payment is coming up soon.

Payment Details:
-----------------
Amount: {amount.Amount:C} {amount.Currency}
Due Date: {dueDate:yyyy-MM-dd}
Subscription: {subscriptionName}
Payment Method: Card ending in {paymentMethodLast4}

The payment will be automatically processed on the due date. Please ensure you have sufficient funds available.

If you need to update your payment method, please log in to your account.

Thank you for your continued subscription!

Best regards,
The Orbito Team
";
    }

    /// <summary>
    /// Gets expired card notification email subject
    /// </summary>
    public static string GetExpiredCardNotificationSubject()
    {
        return "Payment Method Expired - Action Required";
    }

    /// <summary>
    /// Gets expired card notification email body
    /// </summary>
    public static string GetExpiredCardNotificationBody(
        string clientName,
        string cardBrand,
        string last4Digits,
        string expiryDate)
    {
        return $@"
Dear {clientName},

We noticed that your payment method has expired and needs to be updated.

Expired Payment Method:
-----------------
Type: {cardBrand}
Card Number: ****{last4Digits}
Expiry Date: {expiryDate}

Action Required:
- Please update your payment method as soon as possible
- Failure to update may result in service interruption
- Log in to your account to add a new payment method

To avoid any disruption to your service, please update your payment information at your earliest convenience.

If you need assistance, please contact our support team.

Best regards,
The Orbito Team
";
    }

    /// <summary>
    /// Gets card expiring soon notification email subject
    /// </summary>
    public static string GetCardExpiringSoonNotificationSubject(int daysUntilExpiry)
    {
        return $"Payment Method Expiring Soon - Update Required ({daysUntilExpiry} days)";
    }

    /// <summary>
    /// Gets card expiring soon notification email body
    /// </summary>
    public static string GetCardExpiringSoonNotificationBody(
        string clientName,
        string cardBrand,
        string last4Digits,
        string expiryDate,
        int daysUntilExpiry)
    {
        return $@"
Dear {clientName},

Your payment method will expire soon. To ensure uninterrupted service, please update your payment information.

Payment Method Details:
-----------------
Type: {cardBrand}
Card Number: ****{last4Digits}
Expiry Date: {expiryDate}
Days Until Expiry: {daysUntilExpiry}

Action Required:
- Update your payment method before it expires
- Add a new payment method to your account
- Set a new default payment method

To update your payment information, please log in to your account.

If you need assistance, please contact our support team.

Best regards,
The Orbito Team
";
    }

    /// <summary>
    /// Gets payment method added notification email subject
    /// </summary>
    public static string GetPaymentMethodAddedNotificationSubject()
    {
        return "Payment Method Added - Security Notification";
    }

    /// <summary>
    /// Gets payment method added notification email body
    /// </summary>
    public static string GetPaymentMethodAddedNotificationBody(
        string clientName,
        string cardBrand,
        string last4Digits,
        string expiryDate,
        DateTime addedDate)
    {
        return $@"
Dear {clientName},

A new payment method has been added to your account.

Payment Method Details:
-----------------
Type: {cardBrand}
Card Number: ****{last4Digits}
Expiry Date: {expiryDate}
Added On: {addedDate:yyyy-MM-dd HH:mm:ss} UTC

If you did not add this payment method, please contact our support team immediately.

For security reasons, we recommend:
- Reviewing your account activity regularly
- Using strong, unique passwords
- Enabling two-factor authentication

If you have any concerns about your account security, please contact us right away.

Best regards,
The Orbito Team
";
    }

    /// <summary>
    /// Gets payment method removed notification email subject
    /// </summary>
    public static string GetPaymentMethodRemovedNotificationSubject()
    {
        return "Payment Method Removed - Confirmation";
    }

    /// <summary>
    /// Gets payment method removed notification email body
    /// </summary>
    public static string GetPaymentMethodRemovedNotificationBody(
        string clientName,
        string last4Digits,
        DateTime removedDate)
    {
        return $@"
Dear {clientName},

A payment method has been removed from your account.

Removed Payment Method:
-----------------
Card Number: ****{last4Digits}
Removed On: {removedDate:yyyy-MM-dd HH:mm:ss} UTC

If you did not remove this payment method, please contact our support team immediately.

If you have active subscriptions, please ensure you have another valid payment method on file to avoid service interruption.

Best regards,
The Orbito Team
";
    }
}
