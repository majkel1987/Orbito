using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace Orbito.Infrastructure.PaymentGateways.Stripe.Extensions
{
    /// <summary>
    /// Extension methods for database exceptions
    /// </summary>
    public static class DbExceptionExtensions
    {
        /// <summary>
        /// Checks if the exception is related to unique constraint violation
        /// </summary>
        public static bool IsUniqueConstraintViolation(this DbUpdateException exception)
        {
            if (exception.InnerException is DbException dbException)
            {
                // SQL Server
                if (dbException.Message.Contains("duplicate key", StringComparison.OrdinalIgnoreCase) ||
                    dbException.Message.Contains("unique constraint", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                // PostgreSQL
                if (dbException.Message.Contains("unique_violation", StringComparison.OrdinalIgnoreCase) ||
                    dbException.Message.Contains("duplicate key value", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                // MySQL
                if (dbException.Message.Contains("duplicate entry", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                // SQLite
                if (dbException.Message.Contains("constraint failed", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
