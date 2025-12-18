"use client";

import {
  Card,
  CardContent,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/shared/ui/card";
import { Badge } from "@/shared/ui/badge";
import { Button } from "@/shared/ui/button";
import { Check } from "lucide-react";
import { formatCurrency } from "@/shared/lib/formatters";
import Link from "next/link";
import type { SubscriptionPlanDto } from "../types/plan.types";
import {
  parseBillingPeriod,
  parsePlanFeatures,
} from "../types/plan.types";

interface PlanCardProps {
  plan: SubscriptionPlanDto;
}

export function PlanCard({ plan }: PlanCardProps) {
  const features = parsePlanFeatures(plan.featuresJson);
  const billingPeriod = parseBillingPeriod(plan.billingPeriod);

  return (
    <Card className={!plan.isActive ? "opacity-60" : ""}>
      <CardHeader>
        <div className="flex items-center justify-between">
          <CardTitle>{plan.name}</CardTitle>
          <Badge variant={plan.isActive ? "default" : "secondary"}>
            {plan.isActive ? "Active" : "Inactive"}
          </Badge>
        </div>
        {plan.description && (
          <p className="text-sm text-muted-foreground">{plan.description}</p>
        )}
      </CardHeader>
      <CardContent className="space-y-4">
        <div>
          <span className="text-3xl font-bold">
            {formatCurrency(plan.amount, plan.currency)}
          </span>
          <span className="text-muted-foreground">
            /{billingPeriod.type}
          </span>
        </div>

        {features.length > 0 && (
          <ul className="space-y-2">
            {features.map((feature, index) => (
              <li key={index} className="flex items-center gap-2">
                <Check className="h-4 w-4 text-green-500" />
                <span className="text-sm">{feature}</span>
              </li>
            ))}
          </ul>
        )}

        {plan.trialPeriodDays > 0 && (
          <div className="text-sm text-muted-foreground">
            Includes {plan.trialPeriodDays} days free trial
          </div>
        )}
      </CardContent>
      <CardFooter className="flex gap-2">
        <Link href={`/dashboard/plans/${plan.id}`} className="flex-1">
          <Button variant="outline" className="w-full">
            View
          </Button>
        </Link>
        <Link href={`/dashboard/plans/${plan.id}/edit`} className="flex-1">
          <Button className="w-full">Edit</Button>
        </Link>
      </CardFooter>
    </Card>
  );
}
