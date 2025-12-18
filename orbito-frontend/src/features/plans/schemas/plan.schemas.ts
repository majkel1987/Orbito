import { z } from "zod";
import type {
  CreateSubscriptionPlanCommand,
  UpdateSubscriptionPlanCommand,
} from "@/core/api/generated/models";
import type { BillingPeriodType } from "@/core/api/generated/models/billingPeriodType";

/**
 * Frontend form schema - używa prostych typów dla UI
 * Wszystkie pola są wymagane, aby uniknąć konfliktów typów z react-hook-form
 * Wartości domyślne są ustawiane w defaultValues formularza
 */
export const PlanFormSchema = z.object({
  name: z.string().min(2, "Name must be at least 2 characters"),
  description: z.string(),
  amount: z.number().min(0, "Amount must be positive"),
  currency: z.string(),
  interval: z.enum(["Monthly", "Quarterly", "Yearly"]),
  trialPeriodDays: z.number().min(0),
  features: z
    .array(
      z.object({
        value: z.string().min(1, "Feature cannot be empty"),
      })
    )
    .min(1, "At least one feature required"),
  isActive: z.boolean(),
  isPublic: z.boolean(),
  sortOrder: z.number(),
});

export type PlanFormInput = z.infer<typeof PlanFormSchema>;

/**
 * Helper: konwertuje string interval na BillingPeriodType enum
 */
export function intervalToEnumType(
  interval: "Monthly" | "Quarterly" | "Yearly"
): BillingPeriodType {
  switch (interval) {
    case "Monthly":
      return 1 as BillingPeriodType;
    case "Quarterly":
      return 2 as BillingPeriodType;
    case "Yearly":
      return 3 as BillingPeriodType;
  }
}

/**
 * Helper: konwertuje BillingPeriodType enum na string interval
 */
export function enumTypeToInterval(
  type: BillingPeriodType
): "Monthly" | "Quarterly" | "Yearly" {
  switch (type) {
    case 1:
      return "Monthly";
    case 2:
      return "Quarterly";
    case 3:
      return "Yearly";
    default:
      return "Monthly";
  }
}

/**
 * Konwertuje form input na CreateSubscriptionPlanCommand
 */
export function formInputToCreateCommand(
  input: PlanFormInput
): CreateSubscriptionPlanCommand {
  return {
    name: input.name,
    description: input.description || null,
    amount: input.amount,
    currency: input.currency,
    billingPeriodType: intervalToEnumType(input.interval),
    trialPeriodDays: input.trialPeriodDays,
    featuresJson: JSON.stringify(input.features.map((f) => f.value)),
    limitationsJson: null,
    isPublic: input.isPublic,
    sortOrder: input.sortOrder,
  };
}

/**
 * Konwertuje form input na UpdateSubscriptionPlanCommand
 */
export function formInputToUpdateCommand(
  id: string,
  input: PlanFormInput
): UpdateSubscriptionPlanCommand {
  return {
    id,
    name: input.name,
    description: input.description || null,
    amount: input.amount,
    currency: input.currency,
    billingPeriodType: intervalToEnumType(input.interval),
    trialPeriodDays: input.trialPeriodDays,
    featuresJson: JSON.stringify(input.features.map((f) => f.value)),
    limitationsJson: null,
    isActive: input.isActive,
    isPublic: input.isPublic,
    sortOrder: input.sortOrder,
  };
}
