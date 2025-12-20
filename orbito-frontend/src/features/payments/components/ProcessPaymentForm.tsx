"use client";

import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Button } from "@/shared/ui/button";
import { Input } from "@/shared/ui/input";
import { Label } from "@/shared/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/ui/select";
import { useGetApiSubscriptions } from "@/core/api/generated/subscriptions/subscriptions";
import { useGetApiClients } from "@/core/api/generated/clients/clients";
import { useProcessPayment } from "../hooks/usePaymentMutations";
import {
  processPaymentSchema,
  type ProcessPaymentInput,
} from "../schemas/payment.schemas";

export function ProcessPaymentForm() {
  const {
    register,
    handleSubmit,
    setValue,
    watch,
    formState: { errors, isSubmitting },
  } = useForm<ProcessPaymentInput>({
    resolver: zodResolver(processPaymentSchema),
    defaultValues: {
      currency: "PLN",
      amount: 0,
    },
  });

  const processPaymentMutation = useProcessPayment();

  // Fetch subscriptions and clients for dropdowns
  const { data: subscriptionsData } = useGetApiSubscriptions({
    pageNumber: 1,
    pageSize: 100,
  }) as {
    data:
      | {
          items: Array<{
            id: string;
            planName?: string;
            amount: number;
            currency: string;
            clientId: string;
          }>;
        }
      | undefined;
  };

  const { data: clientsData } = useGetApiClients({
    pageNumber: 1,
    pageSize: 100,
  }) as {
    data: { items: Array<{ id: string; fullName?: string; email?: string }> } | undefined;
  };

  const subscriptions = subscriptionsData?.items || [];
  const clients = clientsData?.items || [];

  const selectedSubscriptionId = watch("subscriptionId");
  const selectedClientId = watch("clientId");

  // Auto-fill amount, currency, and clientId based on selected subscription
  useEffect(() => {
    if (selectedSubscriptionId) {
      const selectedSubscription = subscriptions.find(
        (sub) => sub.id === selectedSubscriptionId
      );
      if (selectedSubscription) {
        setValue("amount", selectedSubscription.amount || 0);
        setValue("currency", selectedSubscription.currency || "PLN");
        setValue("clientId", selectedSubscription.clientId);
      }
    }
  }, [selectedSubscriptionId, subscriptions, setValue]);

  const onSubmit = async (data: ProcessPaymentInput) => {
    await processPaymentMutation.mutateAsync({
      data: {
        subscriptionId: data.subscriptionId,
        clientId: data.clientId,
        amount: data.amount,
        currency: data.currency,
        externalTransactionId: data.externalTransactionId,
        paymentMethod: data.paymentMethod,
        externalPaymentId: data.externalPaymentId,
      },
    });
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
      {/* Subscription Selection */}
      <div className="space-y-2">
        <Label htmlFor="subscriptionId">Subscription *</Label>
        <Select
          value={selectedSubscriptionId}
          onValueChange={(value) => setValue("subscriptionId", value)}
        >
          <SelectTrigger>
            <SelectValue placeholder="Select subscription" />
          </SelectTrigger>
          <SelectContent>
            {subscriptions.map((sub) => (
              <SelectItem key={sub.id} value={sub.id}>
                {sub.planName || sub.id}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
        {errors.subscriptionId && (
          <p className="text-sm text-red-500">{errors.subscriptionId.message}</p>
        )}
      </div>

      {/* Client Selection */}
      <div className="space-y-2">
        <Label htmlFor="clientId">Client *</Label>
        <Select
          value={selectedClientId}
          onValueChange={(value) => setValue("clientId", value)}
          disabled={!!selectedSubscriptionId}
        >
          <SelectTrigger>
            <SelectValue placeholder="Select client" />
          </SelectTrigger>
          <SelectContent>
            {clients.map((client) => (
              <SelectItem key={client.id} value={client.id}>
                {client.fullName || client.email || client.id}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
        {selectedSubscriptionId && (
          <p className="text-xs text-gray-500">
            Auto-filled from selected subscription
          </p>
        )}
        {errors.clientId && (
          <p className="text-sm text-red-500">{errors.clientId.message}</p>
        )}
      </div>

      {/* Amount */}
      <div className="space-y-2">
        <Label htmlFor="amount">Amount *</Label>
        <Input
          id="amount"
          type="number"
          step="0.01"
          {...register("amount", { valueAsNumber: true })}
          placeholder="0.00"
          disabled={!!selectedSubscriptionId}
        />
        {selectedSubscriptionId && (
          <p className="text-xs text-gray-500">
            Auto-filled from subscription plan price
          </p>
        )}
        {errors.amount && (
          <p className="text-sm text-red-500">{errors.amount.message}</p>
        )}
      </div>

      {/* Currency */}
      <div className="space-y-2">
        <Label htmlFor="currency">Currency *</Label>
        <Input
          id="currency"
          {...register("currency")}
          placeholder="PLN"
          disabled={!!selectedSubscriptionId}
        />
        {selectedSubscriptionId && (
          <p className="text-xs text-gray-500">
            Auto-filled from subscription plan price
          </p>
        )}
        {errors.currency && (
          <p className="text-sm text-red-500">{errors.currency.message}</p>
        )}
      </div>

      {/* Optional Fields */}
      <div className="space-y-2">
        <Label htmlFor="externalTransactionId">
          External Transaction ID (Optional)
        </Label>
        <Input
          id="externalTransactionId"
          {...register("externalTransactionId")}
          placeholder="e.g., TXN-123456"
        />
      </div>

      <div className="space-y-2">
        <Label htmlFor="paymentMethod">Payment Method (Optional)</Label>
        <Input
          id="paymentMethod"
          {...register("paymentMethod")}
          placeholder="e.g., Credit Card, Bank Transfer"
        />
      </div>

      <div className="space-y-2">
        <Label htmlFor="externalPaymentId">
          External Payment ID (Optional)
        </Label>
        <Input
          id="externalPaymentId"
          {...register("externalPaymentId")}
          placeholder="e.g., PAY-123456"
        />
      </div>

      {/* Submit Button */}
      <div className="flex gap-4">
        <Button type="submit" disabled={isSubmitting}>
          {isSubmitting ? "Processing..." : "Process Payment"}
        </Button>
      </div>
    </form>
  );
}
