"use client";

import { use } from "react";
import { useGetApiSubscriptionPlansId } from "@/core/api/generated/subscription-plans/subscription-plans";
import type { SubscriptionPlanDto } from "@/features/plans/types/plan.types";
import {
  parseBillingPeriod,
  parsePlanFeatures,
} from "@/features/plans/types/plan.types";
import { Button } from "@/shared/ui/button";
import { Badge } from "@/shared/ui/badge";
import { Skeleton } from "@/shared/ui/skeleton";
import { formatCurrency } from "@/shared/lib/formatters";
import { ArrowLeft, Edit, Trash2, Check } from "lucide-react";
import Link from "next/link";

export default function PlanDetailPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = use(params);

  const { data, isLoading, error } = useGetApiSubscriptionPlansId(id) as {
    data: SubscriptionPlanDto | undefined;
    isLoading: boolean;
    error: Error | null;
  };

  if (isLoading) {
    return <PlanDetailSkeleton />;
  }

  if (error) {
    return (
      <div className="rounded-lg border border-destructive bg-destructive/10 p-4">
        <p className="text-sm text-destructive">
          Error loading plan: {error.message}
        </p>
      </div>
    );
  }

  if (!data) {
    return (
      <div className="text-center py-8 text-muted-foreground">Plan not found</div>
    );
  }

  const features = parsePlanFeatures(data.featuresJson);
  const billingPeriod = parseBillingPeriod(data.billingPeriod);

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Link href="/dashboard/plans">
            <Button variant="ghost" size="icon">
              <ArrowLeft className="h-4 w-4" />
            </Button>
          </Link>
          <div>
            <div className="flex items-center gap-2">
              <h1 className="text-3xl font-bold">{data.name}</h1>
              <Badge variant={data.isActive ? "default" : "secondary"}>
                {data.isActive ? "Active" : "Inactive"}
              </Badge>
            </div>
            {data.description && (
              <p className="text-muted-foreground">{data.description}</p>
            )}
          </div>
        </div>

        <div className="flex gap-2">
          <Link href={`/dashboard/plans/${id}/edit`}>
            <Button>
              <Edit className="mr-2 h-4 w-4" />
              Edit
            </Button>
          </Link>
          <Link href={`/dashboard/plans/${id}/delete`}>
            <Button variant="destructive">
              <Trash2 className="mr-2 h-4 w-4" />
              Delete
            </Button>
          </Link>
        </div>
      </div>

      <div className="grid gap-6 md:grid-cols-2">
        {/* Pricing Card */}
        <div className="rounded-lg border bg-card p-6 space-y-4">
          <h2 className="text-xl font-semibold">Pricing</h2>
          <div>
            <span className="text-4xl font-bold">
              {formatCurrency(data.amount, data.currency)}
            </span>
            <span className="text-xl text-muted-foreground">
              /{billingPeriod.type}
            </span>
          </div>
          {data.trialPeriodDays > 0 && (
            <div className="text-sm text-muted-foreground">
              Includes {data.trialPeriodDays} days free trial
            </div>
          )}
        </div>

        {/* Features Card */}
        <div className="rounded-lg border bg-card p-6 space-y-4">
          <h2 className="text-xl font-semibold">Features</h2>
          {features.length > 0 ? (
            <ul className="space-y-2">
              {features.map((feature, index) => (
                <li key={index} className="flex items-center gap-2">
                  <Check className="h-4 w-4 text-green-500 flex-shrink-0" />
                  <span className="text-sm">{feature}</span>
                </li>
              ))}
            </ul>
          ) : (
            <p className="text-sm text-muted-foreground">No features listed</p>
          )}
        </div>

        {/* Additional Info Card */}
        <div className="rounded-lg border bg-card p-6 space-y-4">
          <h2 className="text-xl font-semibold">Details</h2>
          <dl className="space-y-2">
            <div>
              <dt className="text-sm font-medium text-muted-foreground">
                Visibility
              </dt>
              <dd className="text-sm">
                {data.isPublic ? "Public" : "Private"}
              </dd>
            </div>
            <div>
              <dt className="text-sm font-medium text-muted-foreground">
                Sort Order
              </dt>
              <dd className="text-sm">{data.sortOrder}</dd>
            </div>
            {data.activeSubscriptionsCount !== undefined && (
              <div>
                <dt className="text-sm font-medium text-muted-foreground">
                  Active Subscriptions
                </dt>
                <dd className="text-sm">{data.activeSubscriptionsCount}</dd>
              </div>
            )}
            {data.totalSubscriptionsCount !== undefined && (
              <div>
                <dt className="text-sm font-medium text-muted-foreground">
                  Total Subscriptions
                </dt>
                <dd className="text-sm">{data.totalSubscriptionsCount}</dd>
              </div>
            )}
          </dl>
        </div>
      </div>
    </div>
  );
}

function PlanDetailSkeleton() {
  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Skeleton className="h-10 w-10" />
        <div className="space-y-2 flex-1">
          <Skeleton className="h-8 w-64" />
          <Skeleton className="h-4 w-96" />
        </div>
      </div>
      <div className="grid gap-6 md:grid-cols-2">
        <Skeleton className="h-64" />
        <Skeleton className="h-64" />
        <Skeleton className="h-64" />
      </div>
    </div>
  );
}
