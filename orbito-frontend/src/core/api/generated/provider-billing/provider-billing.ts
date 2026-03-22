/**
 * Manual hook for POST /api/ProviderBilling/create-payment-intent (pending Orval regeneration)
 */
import { useMutation } from "@tanstack/react-query";
import type {
  MutationFunction,
  UseMutationOptions,
  UseMutationResult,
} from "@tanstack/react-query";

import type {
  CreateProviderPaymentIntentCommand,
  CreateProviderPaymentIntentResponse,
} from "./types";
import { customInstance } from "../../client";

type SecondParameter<T extends (...args: never) => unknown> = Parameters<T>[1];

/**
 * @summary Create a Stripe PaymentIntent for Provider platform subscription
 */
export const postApiProviderBillingCreatePaymentIntent = (
  createProviderPaymentIntentCommand: CreateProviderPaymentIntentCommand,
  options?: SecondParameter<typeof customInstance>
) => {
  return customInstance<CreateProviderPaymentIntentResponse>(
    {
      url: `/api/ProviderBilling/create-payment-intent`,
      method: "POST",
      headers: { "Content-Type": "application/json" },
      data: createProviderPaymentIntentCommand,
    },
    options
  );
};

export const getPostApiProviderBillingCreatePaymentIntentMutationOptions = <
  TError = unknown,
  TContext = unknown,
>(options?: {
  mutation?: UseMutationOptions<
    Awaited<ReturnType<typeof postApiProviderBillingCreatePaymentIntent>>,
    TError,
    { data: CreateProviderPaymentIntentCommand },
    TContext
  >;
  request?: SecondParameter<typeof customInstance>;
}): UseMutationOptions<
  Awaited<ReturnType<typeof postApiProviderBillingCreatePaymentIntent>>,
  TError,
  { data: CreateProviderPaymentIntentCommand },
  TContext
> => {
  const { mutation: mutationOptions, request: requestOptions } = options ?? {};

  const mutationFn: MutationFunction<
    Awaited<ReturnType<typeof postApiProviderBillingCreatePaymentIntent>>,
    { data: CreateProviderPaymentIntentCommand }
  > = (props) => {
    const { data } = props ?? {};

    return postApiProviderBillingCreatePaymentIntent(data, requestOptions);
  };

  return { mutationFn, ...mutationOptions };
};

export type PostApiProviderBillingCreatePaymentIntentMutationResult =
  NonNullable<
    Awaited<ReturnType<typeof postApiProviderBillingCreatePaymentIntent>>
  >;
export type PostApiProviderBillingCreatePaymentIntentMutationBody =
  CreateProviderPaymentIntentCommand;
export type PostApiProviderBillingCreatePaymentIntentMutationError = unknown;

/**
 * @summary Create a Stripe PaymentIntent for Provider platform subscription
 */
export const usePostApiProviderBillingCreatePaymentIntent = <
  TError = unknown,
  TContext = unknown,
>(options?: {
  mutation?: UseMutationOptions<
    Awaited<ReturnType<typeof postApiProviderBillingCreatePaymentIntent>>,
    TError,
    { data: CreateProviderPaymentIntentCommand },
    TContext
  >;
  request?: SecondParameter<typeof customInstance>;
}): UseMutationResult<
  Awaited<ReturnType<typeof postApiProviderBillingCreatePaymentIntent>>,
  TError,
  { data: CreateProviderPaymentIntentCommand },
  TContext
> => {
  const mutationOptions =
    getPostApiProviderBillingCreatePaymentIntentMutationOptions(options);

  return useMutation(mutationOptions);
};
