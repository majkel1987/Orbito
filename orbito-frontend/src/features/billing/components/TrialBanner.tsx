"use client";

import Link from "next/link";
import { AlertCircle, Clock, Info } from "lucide-react";
import { useProviderSubscription } from "../hooks/useProviderSubscription";
import { Button } from "@/shared/ui/button";
import { cn } from "@/shared/lib/utils";

/**
 * Trial banner displayed at the top of the dashboard for providers on trial.
 * Shows different urgency levels based on days remaining:
 * - > 5 days: Blue (informational)
 * - <= 5 days: Yellow (warning)
 * - <= 1 day: Red (critical)
 */
export function TrialBanner() {
  const { isTrial, daysRemaining, planName, isLoading, error } =
    useProviderSubscription();

  // Don't render if not on trial, loading, or error
  if (!isTrial || isLoading || error) {
    return null;
  }

  const urgency = getUrgencyLevel(daysRemaining);
  const Icon = urgency === "critical" ? AlertCircle : urgency === "warning" ? Clock : Info;

  return (
    <div
      className={cn(
        "flex items-center justify-between gap-4 px-4 py-3 text-sm",
        urgency === "critical" && "bg-red-100 text-red-900 dark:bg-red-950 dark:text-red-100",
        urgency === "warning" && "bg-amber-100 text-amber-900 dark:bg-amber-950 dark:text-amber-100",
        urgency === "info" && "bg-blue-100 text-blue-900 dark:bg-blue-950 dark:text-blue-100"
      )}
    >
      <div className="flex items-center gap-2">
        <Icon className="h-4 w-4 flex-shrink-0" />
        <span>
          {urgency === "critical" ? (
            <>
              <strong>⚠️ Ostatni dzień triala!</strong> Twoje konto zostanie
              ograniczone. Opłać teraz, aby zachować dostęp.
            </>
          ) : urgency === "warning" ? (
            <>
              <strong>Twój trial kończy się za {daysRemaining} {getDaysText(daysRemaining)}!</strong>{" "}
              Przejdź do płatności, aby zachować pełny dostęp do {planName}.
            </>
          ) : (
            <>
              <strong>Okres próbny:</strong> {daysRemaining} {getDaysText(daysRemaining)} pozostało
              {planName && ` (plan ${planName})`}
            </>
          )}
        </span>
      </div>
      <Button
        asChild
        size="sm"
        variant={urgency === "critical" ? "destructive" : urgency === "warning" ? "default" : "outline"}
        className={cn(
          "flex-shrink-0",
          urgency === "info" && "border-blue-300 text-blue-900 hover:bg-blue-200 dark:border-blue-700 dark:text-blue-100 dark:hover:bg-blue-900"
        )}
      >
        <Link href="/dashboard/billing">
          {urgency === "critical" ? "Opłać teraz →" : "Opłać subskrypcję"}
        </Link>
      </Button>
    </div>
  );
}

function getUrgencyLevel(daysRemaining: number): "critical" | "warning" | "info" {
  if (daysRemaining <= 1) return "critical";
  if (daysRemaining <= 5) return "warning";
  return "info";
}

function getDaysText(days: number): string {
  if (days === 1) return "dzień";
  if (days >= 2 && days <= 4) return "dni";
  return "dni";
}
