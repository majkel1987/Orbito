import { z } from "zod";

/**
 * Schema for creating a new subscription
 * Maps to CreateSubscriptionCommand from backend
 */
export const CreateSubscriptionSchema = z.object({
  clientId: z.string().min(1, "Client is required"),
  planId: z.string().min(1, "Plan is required"),
  amount: z.number().min(0, "Amount must be positive"),
  currency: z.string().min(1, "Currency is required"),
  billingPeriodValue: z.number().int().min(1, "Billing period value is required"),
  billingPeriodType: z.string().min(1, "Billing period type is required"),
  trialDays: z.number().int().min(0, "Trial days must be 0 or greater"),
});

export type CreateSubscriptionInput = z.infer<typeof CreateSubscriptionSchema>;

/**
 * Schema for canceling a subscription
 * Maps to CancelSubscriptionRequestDto from backend
 */
export const CancelSubscriptionSchema = z.object({
  clientId: z.string().min(1, "Client ID is required"),
  reason: z.string().optional(),
});

export type CancelSubscriptionInput = z.infer<typeof CancelSubscriptionSchema>;

/**
 * Schema for suspending a subscription
 * Maps to SuspendSubscriptionRequestDto from backend
 */
export const SuspendSubscriptionSchema = z.object({
  clientId: z.string().min(1, "Client ID is required"),
  reason: z.string().optional(),
});

export type SuspendSubscriptionInput = z.infer<typeof SuspendSubscriptionSchema>;

/**
 * Schema for resuming a subscription
 * Maps to ResumeSubscriptionRequestDto from backend
 */
export const ResumeSubscriptionSchema = z.object({
  clientId: z.string().min(1, "Client ID is required"),
});

export type ResumeSubscriptionInput = z.infer<typeof ResumeSubscriptionSchema>;
