import type { BillingPeriodType } from "@/core/api/generated/models";

/**
 * SubscriptionPlanDto - własny typ (backend nie zwraca DTO w OpenAPI spec)
 * Użyj type assertion gdy pobierasz dane z useGetApiSubscriptionPlans()
 */
export interface SubscriptionPlanDto {
  id: string;
  name: string;
  description?: string | null;
  amount: number;
  currency: string;
  billingPeriod: string; // Backend zwraca string "1 Monthly", nie enum
  billingPeriodType?: BillingPeriodType; // Opcjonalne, dla kompatybilności
  trialPeriodDays: number;
  featuresJson?: string | null;
  limitationsJson?: string | null;
  isPublic: boolean;
  isActive: boolean;
  sortOrder: number;
  createdAt: string;
  updatedAt?: string | null;
  activeSubscriptionsCount?: number;
  totalSubscriptionsCount?: number;
}

/**
 * Helper functions dla BillingPeriod
 * Backend zwraca string "1 Monthly", "3 Yearly", itp.
 */
export function parseBillingPeriod(billingPeriod: string): {
  type: string;
  label: string;
} {
  // Format: "1 Monthly", "3 Yearly", itp.
  const parts = billingPeriod.split(" ");
  if (parts.length >= 2) {
    return {
      type: parts[1].toLowerCase(),
      label: parts[1],
    };
  }
  return {
    type: "unknown",
    label: "Unknown",
  };
}

export function getBillingPeriodName(period: BillingPeriodType): string {
  switch (period) {
    case 1:
      return "Month";
    case 2:
      return "Quarter";
    case 3:
      return "Year";
    case 4:
      return "Lifetime";
    default:
      return "Unknown";
  }
}

export function getBillingPeriodLabel(period: BillingPeriodType): string {
  switch (period) {
    case 1:
      return "Monthly";
    case 2:
      return "Quarterly";
    case 3:
      return "Yearly";
    case 4:
      return "Lifetime";
    default:
      return "Unknown";
  }
}

/**
 * Parsuje JSON features do array strings
 */
export function parsePlanFeatures(featuresJson?: string | null): string[] {
  if (!featuresJson) return [];
  try {
    const parsed = JSON.parse(featuresJson);
    return Array.isArray(parsed) ? parsed : [];
  } catch {
    return [];
  }
}
