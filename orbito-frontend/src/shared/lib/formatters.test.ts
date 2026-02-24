import { describe, it, expect } from 'vitest';
import { formatCurrency, formatDate } from './formatters';

describe('formatters', () => {
  describe('formatCurrency', () => {
    it('formats PLN correctly by default', () => {
      // The exact output might contain non-breaking spaces, so we normalize spaces or use match
      const result = formatCurrency(1234.56).replace(/\s+/g, ' ');
      // pl-PL locale usually outputs "1 234,56 zł"
      expect(result).toMatch(/1\s?234,56\s?z\u0142/);
    });

    it('formats USD correctly', () => {
      const result = formatCurrency(1234.56, 'USD').replace(/\s+/g, ' ');
      // outputs something like "1 234,56 USD" in pl-PL locale
      expect(result).toMatch(/1\s?234,56\s?USD/);
    });
  });

  describe('formatDate', () => {
    it('formats an ISO string date correctly', () => {
      const result = formatDate('2024-12-07T10:00:00Z');
      expect(result).toBe('07.12.2024');
    });

    it('formats a Date object correctly', () => {
      const date = new Date('2024-12-07T10:00:00Z');
      const result = formatDate(date);
      expect(result).toBe('07.12.2024');
    });
  });
});
