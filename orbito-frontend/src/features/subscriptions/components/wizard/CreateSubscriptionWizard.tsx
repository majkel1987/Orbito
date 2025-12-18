"use client";

import { useState } from "react";
import { useGetApiClients } from "@/core/api/generated/clients/clients";
import { useGetApiSubscriptionPlans } from "@/core/api/generated/subscription-plans/subscription-plans";
import { useCreateSubscription } from "../../hooks/useSubscriptionMutations";
import { Button } from "@/shared/ui/button";
import { Card } from "@/shared/ui/card";
import { Skeleton } from "@/shared/ui/skeleton";
import { parseBillingPeriod, parsePlanFeatures } from "@/features/plans/types/plan.types";
import { formatCurrency } from "@/shared/lib/formatters";
import type { SubscriptionPlanDto } from "@/features/plans/types/plan.types";
import type { CreateSubscriptionCommand } from "@/core/api/generated/models";

interface ClientDtoPaginatedList {
  items: Array<{
    id: string;
    fullName?: string | null;
    email?: string | null;
    directEmail?: string | null;
    companyName?: string | null;
  }>;
}

interface PlansResponse {
  items: SubscriptionPlanDto[];
}

type WizardStep = "select-client" | "select-plan" | "confirm";

export function CreateSubscriptionWizard() {
  const [currentStep, setCurrentStep] = useState<WizardStep>("select-client");
  const [selectedClientId, setSelectedClientId] = useState<string>("");
  const [selectedPlanId, setSelectedPlanId] = useState<string>("");

  // Fetch clients
  const { data: clientsData, isLoading: isLoadingClients } = useGetApiClients({
    pageNumber: 1,
    pageSize: 100,
  }) as { data: ClientDtoPaginatedList | undefined; isLoading: boolean };

  // Fetch plans
  const { data: plansData, isLoading: isLoadingPlans } = useGetApiSubscriptionPlans() as {
    data: PlansResponse | undefined;
    isLoading: boolean;
  };

  const createMutation = useCreateSubscription();

  const clients = clientsData?.items || [];
  const plans = plansData?.items || [];

  const selectedClient = clients.find((c) => c.id === selectedClientId);
  const selectedPlan = plans.find((p) => p.id === selectedPlanId);

  const handleNext = () => {
    if (currentStep === "select-client" && selectedClientId) {
      setCurrentStep("select-plan");
    } else if (currentStep === "select-plan" && selectedPlanId) {
      setCurrentStep("confirm");
    }
  };

  const handleBack = () => {
    if (currentStep === "select-plan") {
      setCurrentStep("select-client");
    } else if (currentStep === "confirm") {
      setCurrentStep("select-plan");
    }
  };

  const handleSubmit = async () => {
    if (!selectedClient || !selectedPlan) return;

    const billingPeriod = parseBillingPeriod(selectedPlan.billingPeriod);

    // Map billing period type to enum number
    let billingPeriodTypeValue = 1; // default monthly
    if (billingPeriod.type === "monthly") billingPeriodTypeValue = 1;
    else if (billingPeriod.type === "quarterly") billingPeriodTypeValue = 2;
    else if (billingPeriod.type === "yearly") billingPeriodTypeValue = 3;
    else if (billingPeriod.type === "lifetime") billingPeriodTypeValue = 4;

    const command: CreateSubscriptionCommand = {
      clientId: selectedClient.id,
      planId: selectedPlan.id,
      amount: selectedPlan.amount,
      currency: selectedPlan.currency,
      billingPeriodValue: parseInt(selectedPlan.billingPeriod.split(" ")[0]) || 1,
      billingPeriodType: billingPeriodTypeValue.toString(),
      trialDays: selectedPlan.trialPeriodDays,
    };

    await createMutation.mutateAsync({ data: command });
  };

  // Step 1: Select Client
  if (currentStep === "select-client") {
    if (isLoadingClients) {
      return (
        <Card className="p-6">
          <Skeleton className="h-8 w-48 mb-4" />
          <Skeleton className="h-40" />
        </Card>
      );
    }

    return (
      <Card className="p-6">
        <h2 className="text-2xl font-semibold mb-4">Select Client</h2>
        <p className="text-muted-foreground mb-4">
          Choose the client for this subscription
        </p>

        {clients.length === 0 ? (
          <p className="text-muted-foreground">No clients available</p>
        ) : (
          <div className="space-y-2">
            {clients.map((client) => (
              <button
                key={client.id}
                onClick={() => setSelectedClientId(client.id)}
                className={`w-full p-4 text-left border rounded-lg transition-colors ${
                  selectedClientId === client.id
                    ? "border-primary bg-primary/5"
                    : "border-border hover:border-primary/50"
                }`}
              >
                <div className="font-medium">
                  {client.fullName || client.email || client.directEmail || "Unknown"}
                </div>
                {client.companyName && (
                  <div className="text-sm text-muted-foreground">{client.companyName}</div>
                )}
              </button>
            ))}
          </div>
        )}

        <div className="flex justify-end gap-2 mt-6">
          <Button onClick={handleNext} disabled={!selectedClientId}>
            Next
          </Button>
        </div>
      </Card>
    );
  }

  // Step 2: Select Plan
  if (currentStep === "select-plan") {
    if (isLoadingPlans) {
      return (
        <Card className="p-6">
          <Skeleton className="h-8 w-48 mb-4" />
          <Skeleton className="h-40" />
        </Card>
      );
    }

    return (
      <Card className="p-6">
        <h2 className="text-2xl font-semibold mb-4">Select Plan</h2>
        <p className="text-muted-foreground mb-4">
          Choose a subscription plan for {selectedClient?.fullName || selectedClient?.email}
        </p>

        {plans.length === 0 ? (
          <p className="text-muted-foreground">No plans available</p>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            {plans
              .filter((plan) => plan.isActive && plan.isPublic)
              .map((plan) => {
                const billingPeriod = parseBillingPeriod(plan.billingPeriod);
                const features = parsePlanFeatures(plan.featuresJson);

                return (
                  <button
                    key={plan.id}
                    onClick={() => setSelectedPlanId(plan.id)}
                    className={`p-6 text-left border rounded-lg transition-colors ${
                      selectedPlanId === plan.id
                        ? "border-primary bg-primary/5"
                        : "border-border hover:border-primary/50"
                    }`}
                  >
                    <h3 className="text-xl font-semibold mb-2">{plan.name}</h3>
                    <div className="text-3xl font-bold mb-2">
                      {formatCurrency(plan.amount)}
                      <span className="text-sm font-normal text-muted-foreground">
                        /{billingPeriod.type}
                      </span>
                    </div>
                    {plan.trialPeriodDays > 0 && (
                      <p className="text-sm text-muted-foreground mb-4">
                        Includes {plan.trialPeriodDays} days free trial
                      </p>
                    )}
                    {features.length > 0 && (
                      <ul className="space-y-1 text-sm">
                        {features.slice(0, 3).map((feature, idx) => (
                          <li key={idx} className="flex items-center gap-2">
                            <span className="text-primary">✓</span>
                            {feature}
                          </li>
                        ))}
                        {features.length > 3 && (
                          <li className="text-muted-foreground">
                            +{features.length - 3} more
                          </li>
                        )}
                      </ul>
                    )}
                  </button>
                );
              })}
          </div>
        )}

        <div className="flex justify-between gap-2 mt-6">
          <Button variant="outline" onClick={handleBack}>
            Back
          </Button>
          <Button onClick={handleNext} disabled={!selectedPlanId}>
            Next
          </Button>
        </div>
      </Card>
    );
  }

  // Step 3: Confirm
  if (currentStep === "confirm") {
    if (!selectedClient || !selectedPlan) return null;

    const billingPeriod = parseBillingPeriod(selectedPlan.billingPeriod);

    return (
      <Card className="p-6">
        <h2 className="text-2xl font-semibold mb-4">Confirm Subscription</h2>
        <p className="text-muted-foreground mb-6">
          Please review the details before creating the subscription
        </p>

        <div className="space-y-6">
          <div>
            <h3 className="font-semibold mb-2">Client</h3>
            <div className="p-4 border rounded-lg">
              <div className="font-medium">
                {selectedClient.fullName || selectedClient.email || selectedClient.directEmail}
              </div>
              {selectedClient.companyName && (
                <div className="text-sm text-muted-foreground">
                  {selectedClient.companyName}
                </div>
              )}
            </div>
          </div>

          <div>
            <h3 className="font-semibold mb-2">Plan</h3>
            <div className="p-4 border rounded-lg">
              <div className="flex justify-between items-start mb-2">
                <div>
                  <div className="font-semibold">{selectedPlan.name}</div>
                  <div className="text-sm text-muted-foreground">{selectedPlan.description}</div>
                </div>
                <div className="text-right">
                  <div className="text-2xl font-bold">
                    {formatCurrency(selectedPlan.amount)}
                  </div>
                  <div className="text-sm text-muted-foreground">/{billingPeriod.type}</div>
                </div>
              </div>
              {selectedPlan.trialPeriodDays > 0 && (
                <div className="text-sm text-muted-foreground">
                  Trial period: {selectedPlan.trialPeriodDays} days
                </div>
              )}
            </div>
          </div>
        </div>

        <div className="flex justify-between gap-2 mt-6">
          <Button variant="outline" onClick={handleBack}>
            Back
          </Button>
          <Button
            onClick={handleSubmit}
            disabled={createMutation.isPending}
          >
            {createMutation.isPending ? "Creating..." : "Create Subscription"}
          </Button>
        </div>
      </Card>
    );
  }

  return null;
}
