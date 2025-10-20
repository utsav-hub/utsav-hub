namespace Shared.Contracts;

// Commands and events shared across services
public record BookingCreated
(
    Guid BookingId,
    Guid ScheduleId,
    Guid CustomerId,
    DateTime CreatedUtc
);

public record PaymentAuthorized
(
    Guid PaymentId,
    Guid BookingId,
    decimal Amount,
    string Currency,
    DateTime AuthorizedUtc
);

public record InvoiceIssued
(
    Guid InvoiceId,
    Guid BookingId,
    decimal Amount,
    string Currency,
    DateTime IssuedUtc
);
