namespace Orbito.Domain.Enums
{
    /// <summary>
    /// Status zwrotu płatności
    /// </summary>
    public enum RefundStatus
    {
        /// <summary>
        /// Zwrot oczekuje na przetworzenie
        /// </summary>
        Pending = 0,

        /// <summary>
        /// Zwrot jest przetwarzany
        /// </summary>
        Processing = 1,

        /// <summary>
        /// Zwrot został zakończony
        /// </summary>
        Completed = 2,

        /// <summary>
        /// Zwrot zakończył się niepowodzeniem
        /// </summary>
        Failed = 3,

        /// <summary>
        /// Zwrot został anulowany
        /// </summary>
        Cancelled = 4,

        /// <summary>
        /// Częściowy zwrot
        /// </summary>
        PartiallyRefunded = 5
    }
}