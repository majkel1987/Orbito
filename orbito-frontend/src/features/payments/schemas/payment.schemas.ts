import { z } from "zod";

/**
 * Schema for ProcessPaymentCommand (Manual Payment)
 * POST /api/Payment/process
 */
export const processPaymentSchema = z.object({
  subscriptionId: z.string().uuid("Invalid subscription ID"),
  clientId: z.string().uuid("Invalid client ID"),
  amount: z
    .number()
    .positive("Amount must be positive")
    .min(0.01, "Amount must be at least 0.01"),
  currency: z
    .string()
    .min(3, "Currency must be 3 characters")
    .max(3, "Currency must be 3 characters")
    .regex(/^[A-Z]{3}$/, "Currency must be uppercase (e.g., PLN, USD, EUR)")
    .transform((val) => val.toUpperCase()),
  externalTransactionId: z
    .string()
    .transform((val) => (val?.trim() === "" ? undefined : val))
    .optional(),
  paymentMethod: z
    .string()
    .transform((val) => (val?.trim() === "" ? undefined : val))
    .optional(),
  externalPaymentId: z
    .string()
    .transform((val) => (val?.trim() === "" ? undefined : val))
    .optional(),
});

export type ProcessPaymentInput = z.infer<typeof processPaymentSchema>;

/**
 * Schema for RefundPaymentCommand
 * POST /api/Payment/{id}/refund
 */
export const refundPaymentSchema = z.object({
  paymentId: z.string().uuid("Invalid payment ID"),
  clientId: z.string().uuid("Invalid client ID"),
  amount: z
    .number()
    .positive("Amount must be positive")
    .min(0.01, "Amount must be at least 0.01"),
  currency: z.string().min(3).max(3),
  reason: z.string().min(5, "Please provide a reason (min 5 characters)"),
});

export type RefundPaymentInput = z.infer<typeof refundPaymentSchema>;
