/**
 * Manual DTO for PlatformPlan (pending Orval regeneration)
 */
export interface PlatformPlanDto {
  id: string;
  name: string;
  description: string | null;
  priceAmount: number;
  priceCurrency: string;
  billingPeriod: string;
  trialDays: number;
  isActive: boolean;
  featuresJson: string | null;
  sortOrder: number;
}
