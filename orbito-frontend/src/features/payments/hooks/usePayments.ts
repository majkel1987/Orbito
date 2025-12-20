"use client";

import { useGetApiPayment } from "@/core/api/generated/payment/payment";
import type { PaymentStatus } from "@/core/api/generated/models/paymentStatus";
import { useState } from "react";

export interface UsePaymentsParams {
  pageSize?: number;
  searchTerm?: string;
  status?: PaymentStatus;
  clientId?: string;
}

export function usePayments(params?: UsePaymentsParams) {
  const [pageNumber, setPageNumber] = useState(1);
  const pageSize = params?.pageSize ?? 10;

  const { data, isLoading, error, refetch } = useGetApiPayment({
    pageNumber,
    pageSize,
    searchTerm: params?.searchTerm,
    status: params?.status,
    clientId: params?.clientId,
  });

  const goToPage = (page: number) => {
    setPageNumber(page);
  };

  const nextPage = () => {
    if (data?.totalPages && pageNumber < data.totalPages) {
      setPageNumber((prev) => prev + 1);
    }
  };

  const prevPage = () => {
    if (pageNumber > 1) {
      setPageNumber((prev) => prev - 1);
    }
  };

  return {
    payments: data?.payments ?? [],
    totalCount: data?.totalCount ?? 0,
    pageNumber: data?.pageNumber ?? pageNumber,
    pageSize: data?.pageSize ?? pageSize,
    totalPages: data?.totalPages ?? 1,
    isLoading,
    error,
    refetch,
    goToPage,
    nextPage,
    prevPage,
  };
}
