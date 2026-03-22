"use client";

import { useGetApiPlatformPlans } from "@/core/api/generated/platform-plans/platform-plans";
import { PlatformPlanDto } from "@/core/api/generated/platform-plans/platformPlanDto";
import { Button } from "@/shared/ui/button";
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/shared/ui/card";
import { Skeleton } from "@/shared/ui/skeleton";
import { Check } from "lucide-react";
import { cn } from "@/shared/lib/utils";

interface PlanSelectionStepProps {
  selectedPlanId: string | null;
  onSelectPlan: (planId: string) => void;
  onContinue: () => void;
}

export function PlanSelectionStep({
  selectedPlanId,
  onSelectPlan,
  onContinue,
}: PlanSelectionStepProps) {
  const { data: plans, isLoading, error } = useGetApiPlatformPlans();

  if (isLoading) {
    return (
      <div className="space-y-4">
        <h2 className="text-2xl font-bold text-center">Wybierz plan</h2>
        <div className="grid gap-4 md:grid-cols-3">
          {[1, 2, 3].map((i) => (
            <Skeleton key={i} className="h-80" />
          ))}
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="text-center text-red-500">
        Błąd ładowania planów: {error instanceof Error ? error.message : "Nieznany błąd"}
      </div>
    );
  }

  if (!plans?.length) {
    return (
      <div className="text-center text-muted-foreground">
        Brak dostępnych planów
      </div>
    );
  }

  // Sort by sortOrder
  const sortedPlans = [...plans].sort((a, b) => a.sortOrder - b.sortOrder);

  return (
    <div className="space-y-6">
      <div className="text-center space-y-2">
        <h2 className="text-2xl font-bold">Wybierz plan dla Twojej firmy</h2>
        <p className="text-muted-foreground">
          Każdy plan zawiera 14-dniowy okres próbny. Płatność dopiero po okresie próbnym.
        </p>
      </div>

      <div className="grid gap-4 md:grid-cols-3">
        {sortedPlans.map((plan) => (
          <PlanCard
            key={plan.id}
            plan={plan}
            isSelected={selectedPlanId === plan.id}
            onSelect={() => onSelectPlan(plan.id)}
          />
        ))}
      </div>

      <div className="flex justify-center">
        <Button
          size="lg"
          onClick={onContinue}
          disabled={!selectedPlanId}
        >
          Kontynuuj rejestrację
        </Button>
      </div>
    </div>
  );
}

interface PlanCardProps {
  plan: PlatformPlanDto;
  isSelected: boolean;
  onSelect: () => void;
}

function PlanCard({ plan, isSelected, onSelect }: PlanCardProps) {
  const features = parseFeatures(plan.featuresJson);
  const isPro = plan.name.toLowerCase().includes("pro");

  return (
    <Card
      className={cn(
        "relative cursor-pointer transition-all hover:shadow-lg",
        isSelected && "ring-2 ring-primary",
        isPro && "border-primary"
      )}
      onClick={onSelect}
    >
      {isPro && (
        <div className="absolute -top-3 left-1/2 -translate-x-1/2">
          <span className="bg-primary text-primary-foreground text-xs font-medium px-3 py-1 rounded-full">
            Najpopularniejszy
          </span>
        </div>
      )}

      <CardHeader className="text-center">
        <CardTitle className="text-xl">{plan.name}</CardTitle>
        <CardDescription>{plan.description}</CardDescription>
      </CardHeader>

      <CardContent className="text-center space-y-4">
        <div>
          <span className="text-4xl font-bold">
            {formatPrice(plan.priceAmount, plan.priceCurrency)}
          </span>
          <span className="text-muted-foreground">
            /{formatBillingPeriod(plan.billingPeriod)}
          </span>
        </div>

        {plan.trialDays > 0 && (
          <p className="text-sm text-muted-foreground">
            {plan.trialDays} dni za darmo
          </p>
        )}

        <ul className="space-y-2 text-left">
          {features.map((feature, index) => (
            <li key={index} className="flex items-center gap-2 text-sm">
              <Check className="h-4 w-4 text-green-500 shrink-0" />
              <span>{feature}</span>
            </li>
          ))}
        </ul>
      </CardContent>

      <CardFooter>
        <Button
          variant={isSelected ? "default" : "outline"}
          className="w-full"
          onClick={(e) => {
            e.stopPropagation();
            onSelect();
          }}
        >
          {isSelected ? "Wybrano" : "Wybierz"}
        </Button>
      </CardFooter>
    </Card>
  );
}

function parseFeatures(featuresJson: string | null): string[] {
  if (!featuresJson) return [];
  try {
    const parsed = JSON.parse(featuresJson);
    if (Array.isArray(parsed)) return parsed;
    return [];
  } catch {
    return [];
  }
}

function formatPrice(amount: number, currency: string): string {
  return new Intl.NumberFormat("pl-PL", {
    style: "currency",
    currency: currency.toUpperCase(),
    minimumFractionDigits: 0,
    maximumFractionDigits: 2,
  }).format(amount);
}

function formatBillingPeriod(period: string): string {
  const lower = period.toLowerCase();
  if (lower === "monthly" || lower === "month") return "mies.";
  if (lower === "yearly" || lower === "year" || lower === "annual") return "rok";
  return period;
}
