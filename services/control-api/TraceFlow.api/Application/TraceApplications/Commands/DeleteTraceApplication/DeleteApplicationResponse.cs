public record DeleteApplicationResponse(
    Ulid Id,
    Ulid ProjectId,
    string DeleteMode,
    string Message);