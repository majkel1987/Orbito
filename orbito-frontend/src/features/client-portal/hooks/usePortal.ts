import {
  useGetApiPortalSubscriptions,
  useGetApiPortalInvoices,
} from "@/core/api/generated/portal/portal";

export function useMySubscriptions() {
  return useGetApiPortalSubscriptions();
}

export function useMyInvoices(pageNumber = 1, pageSize = 20) {
  return useGetApiPortalInvoices({ pageNumber, pageSize });
}
