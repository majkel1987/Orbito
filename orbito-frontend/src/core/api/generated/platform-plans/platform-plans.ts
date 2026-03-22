/**
 * Manual hook for GET /api/PlatformPlans (pending Orval regeneration)
 */
import { useQuery } from "@tanstack/react-query";
import type {
  QueryFunction,
  QueryKey,
  UseQueryOptions,
  UseQueryResult,
} from "@tanstack/react-query";

import type { PlatformPlanDto } from "./platformPlanDto";
import { customInstance } from "../../client";

type SecondParameter<T extends (...args: never) => unknown> = Parameters<T>[1];

/**
 * @summary Get all active platform plans (AllowAnonymous)
 */
export const getApiPlatformPlans = (
  options?: SecondParameter<typeof customInstance>,
  signal?: AbortSignal
) => {
  return customInstance<PlatformPlanDto[]>(
    { url: `/api/PlatformPlans`, method: "GET", signal },
    options
  );
};

export const getGetApiPlatformPlansQueryKey = () => {
  return [`/api/PlatformPlans`] as const;
};

export const getGetApiPlatformPlansQueryOptions = <
  TData = Awaited<ReturnType<typeof getApiPlatformPlans>>,
  TError = unknown,
>(options?: {
  query?: Partial<
    UseQueryOptions<
      Awaited<ReturnType<typeof getApiPlatformPlans>>,
      TError,
      TData
    >
  >;
  request?: SecondParameter<typeof customInstance>;
}) => {
  const { query: queryOptions, request: requestOptions } = options ?? {};

  const queryKey =
    queryOptions?.queryKey ?? getGetApiPlatformPlansQueryKey();

  const queryFn: QueryFunction<
    Awaited<ReturnType<typeof getApiPlatformPlans>>
  > = ({ signal }) => getApiPlatformPlans(requestOptions, signal);

  return { queryKey, queryFn, ...queryOptions } as UseQueryOptions<
    Awaited<ReturnType<typeof getApiPlatformPlans>>,
    TError,
    TData
  > & { queryKey: QueryKey };
};

export type GetApiPlatformPlansQueryResult = NonNullable<
  Awaited<ReturnType<typeof getApiPlatformPlans>>
>;
export type GetApiPlatformPlansQueryError = unknown;

/**
 * @summary Get all active platform plans
 */
export const useGetApiPlatformPlans = <
  TData = Awaited<ReturnType<typeof getApiPlatformPlans>>,
  TError = unknown,
>(options?: {
  query?: Partial<
    UseQueryOptions<
      Awaited<ReturnType<typeof getApiPlatformPlans>>,
      TError,
      TData
    >
  >;
  request?: SecondParameter<typeof customInstance>;
}): UseQueryResult<TData, TError> & { queryKey: QueryKey } => {
  const queryOptions = getGetApiPlatformPlansQueryOptions(options);

  const query = useQuery(queryOptions) as UseQueryResult<TData, TError> & {
    queryKey: QueryKey;
  };

  query.queryKey = queryOptions.queryKey;

  return query;
};
