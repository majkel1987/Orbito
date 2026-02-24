import { describe, it, expect } from 'vitest';
import { getPaymentStatusVariant, getPaymentStatusLabel } from '../types/payment.types';

describe('PaymentStatusBadge logic', () => {
  it('returns appropriate variant for status Completed', () => {
    expect(getPaymentStatusVariant('Completed')).toBe('default');
  });

  it('returns appropriate variant for status Failed', () => {
    // Usually Failed maps to destructive
    expect(getPaymentStatusVariant('Failed')).toBe('destructive');
  });

  it('returns appropriate label', () => {
    expect(getPaymentStatusLabel('Pending')).toBe('Pending');
  });
});
