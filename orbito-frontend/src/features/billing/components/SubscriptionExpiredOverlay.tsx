"use client";

import Link from "next/link";
import { AlertTriangle, CreditCard, Settings } from "lucide-react";
import { Button } from "@/shared/ui/button";
import { useProviderSubscription } from "../hooks/useProviderSubscription";

/**
 * Full-screen overlay displayed when provider's trial or subscription has expired.
 * Blocks access to all dashboard features except /dashboard/billing.
 * Provider must pay to regain access.
 */
export function SubscriptionExpiredOverlay() {
  const { subscription, planName } = useProviderSubscription();

  const planPrice = subscription?.planPrice ?? 0;
  const planCurrency = subscription?.planCurrency ?? "PLN";

  const formattedPrice = new Intl.NumberFormat("pl-PL", {
    style: "currency",
    currency: planCurrency,
  }).format(planPrice);

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-background/95 backdrop-blur-sm">
      <div className="mx-4 max-w-md text-center">
        {/* Warning Icon */}
        <div className="mx-auto mb-6 flex h-16 w-16 items-center justify-center rounded-full bg-red-100 dark:bg-red-950">
          <AlertTriangle className="h-8 w-8 text-red-600 dark:text-red-400" />
        </div>

        {/* Title */}
        <h1 className="mb-2 text-2xl font-bold text-foreground">
          Twój okres próbny się zakończył
        </h1>

        {/* Description */}
        <p className="mb-6 text-muted-foreground">
          Aby kontynuować korzystanie z Orbito, opłać subskrypcję planu{" "}
          <strong className="text-foreground">{planName || "Pro"}</strong>.
        </p>

        {/* Primary CTA */}
        <Button asChild size="lg" className="mb-3 w-full">
          <Link href="/dashboard/billing">
            <CreditCard className="mr-2 h-5 w-5" />
            Opłać teraz – {formattedPrice}/mies.
          </Link>
        </Button>

        {/* Secondary CTA */}
        <Button asChild variant="outline" size="sm" className="w-full">
          <Link href="/dashboard/billing?change=true">
            <Settings className="mr-2 h-4 w-4" />
            Zmień plan
          </Link>
        </Button>

        {/* Fine print */}
        <p className="mt-6 text-xs text-muted-foreground">
          Po opłaceniu subskrypcji natychmiast odzyskasz pełny dostęp do wszystkich
          funkcji platformy.
        </p>
      </div>
    </div>
  );
}
