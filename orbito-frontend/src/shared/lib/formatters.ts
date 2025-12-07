/**
 * Formatuje kwotę pieniężną zgodnie z lokalnymi standardami
 * @param amount - Kwota do sformatowania
 * @param currency - Kod waluty (domyślnie PLN)
 * @returns Sformatowany string z walutą
 * @example formatCurrency(1234.56, 'PLN') // "1 234,56 zł"
 */
export function formatCurrency(amount: number, currency: string = 'PLN'): string {
  return new Intl.NumberFormat('pl-PL', {
    style: 'currency',
    currency: currency,
  }).format(amount);
}

/**
 * Formatuje datę na polski format DD.MM.YYYY
 * @param date - Data do sformatowania (string ISO lub Date object)
 * @returns Sformatowany string daty
 * @example formatDate('2024-12-07') // "07.12.2024"
 * @example formatDate(new Date()) // "07.12.2024"
 */
export function formatDate(date: string | Date): string {
  const dateObj = typeof date === 'string' ? new Date(date) : date;

  return new Intl.DateTimeFormat('pl-PL', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
  }).format(dateObj);
}
