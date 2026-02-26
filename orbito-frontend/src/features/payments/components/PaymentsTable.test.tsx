import { describe, it, expect, vi } from 'vitest';
import { screen } from '@testing-library/react';
import { renderWithProviders } from '@/test/utils';
import { PaymentsTable } from './PaymentsTable';
import * as usePaymentsModule from '../hooks/usePayments';
import { PaymentDto } from '../types/payment.types';

// Make sure we mock the hook before imports potentially use it
vi.mock('../hooks/usePayments', () => ({
  usePayments: vi.fn()
}));

describe('PaymentsTable Component', () => {
  it('renders loading state when isLoading is true', () => {
    vi.mocked(usePaymentsModule.usePayments).mockReturnValue({
      payments: [],
      totalCount: 0,
      pageNumber: 1,
      pageSize: 10,
      totalPages: 1,
      isLoading: true,
      error: null,
      refetch: vi.fn(),
      goToPage: vi.fn(),
      nextPage: vi.fn(),
      prevPage: vi.fn(),
    });

    renderWithProviders(<PaymentsTable />);
    // Checking for presence of skeleton loaders (they just render divs usually, but we check empty state lack)
    expect(screen.queryByText('No payments yet')).not.toBeInTheDocument();
    expect(screen.queryByText('Transaction ID')).not.toBeInTheDocument();
  });

  it('renders empty state when there are no payments', () => {
    vi.mocked(usePaymentsModule.usePayments).mockReturnValue({
      payments: [],
      totalCount: 0,
      pageNumber: 1,
      pageSize: 10,
      totalPages: 1,
      isLoading: false,
      error: null,
      refetch: vi.fn(),
      goToPage: vi.fn(),
      nextPage: vi.fn(),
      prevPage: vi.fn(),
    });

    renderWithProviders(<PaymentsTable />);
    expect(screen.getByText('No payments yet')).toBeInTheDocument();
  });

  it('renders payments table correctly with data', () => {
    const mockPayments = [
      {
        id: 'pay_1234',
        externalTransactionId: 'ext_987',
        amount: 150.5,
        currency: 'USD',
        status: 'Completed',
        paymentMethod: 'Credit Card',
        createdAt: '2024-12-07T10:00:00Z',
      }
    ] as unknown as PaymentDto[];

    vi.mocked(usePaymentsModule.usePayments).mockReturnValue({
      payments: mockPayments,
      totalCount: 1,
      pageNumber: 1,
      pageSize: 10,
      totalPages: 1,
      isLoading: false,
      error: null,
      refetch: vi.fn(),
      goToPage: vi.fn(),
      nextPage: vi.fn(),
      prevPage: vi.fn(),
    });

    renderWithProviders(<PaymentsTable />);
    
    // Check headers
    expect(screen.getByText('Transaction ID')).toBeInTheDocument();
    
    // Check data row
    expect(screen.getByText('ext_987')).toBeInTheDocument();
    expect(screen.getByText('Completed')).toBeInTheDocument();
    expect(screen.getByText('Credit Card')).toBeInTheDocument();
  });
});
