import { useGetApiClients } from "@/core/api/generated/clients/clients";
import type { ClientDtoPaginatedList } from "@/core/api/generated/models";
import { useTransition } from "react";
import { useSearchParams, useRouter } from "next/navigation";

export function useClients() {
  const searchParams = useSearchParams();
  const router = useRouter();
  const [, startTransition] = useTransition();

  const page = Number(searchParams.get("page")) || 1;
  const search = searchParams.get("search") || "";
  const status = searchParams.get("status") || "";

  // Convert status string to activeOnly boolean
  const activeOnly =
    status === "active" ? true : status === "inactive" ? false : undefined;

  // Type assertion needed due to Orval bug (customInstance<void> instead of ClientDtoPaginatedList)
  const { data, isLoading, error } = useGetApiClients({
    pageNumber: page,
    pageSize: 10,
    searchTerm: search || undefined,
    activeOnly,
  }) as {
    data: ClientDtoPaginatedList | undefined;
    isLoading: boolean;
    error: Error | null;
  };

  function updateParams(updates: Record<string, string | undefined>) {
    const params = new URLSearchParams(searchParams.toString());
    Object.entries(updates).forEach(([key, value]) => {
      if (value) {
        params.set(key, value);
      } else {
        params.delete(key);
      }
    });
    // Reset to page 1 when filters change
    if (!updates.page) {
      params.set("page", "1");
    }
    startTransition(() => {
      router.replace(`?${params.toString()}`);
    });
  }

  return {
    clients: data?.items ?? [],
    totalCount: data?.totalCount ?? 0,
    totalPages: data?.totalPages ?? 0,
    currentPage: page,
    search,
    status,
    isLoading,
    error,
    setSearch: (value: string) => updateParams({ search: value || undefined }),
    setStatus: (value: string) =>
      updateParams({ status: value === "all" ? undefined : value }),
    setPage: (value: number) => updateParams({ page: String(value) }),
    clearFilters: () => startTransition(() => router.replace("?")),
  };
}
