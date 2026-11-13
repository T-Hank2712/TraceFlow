export type ApiProblemDetails = {
    type?: string;
    title?: string;
    status?: number;
    detail?: string;
    instance?: string;
    traceId?: string;
};

type ApiErrorOptions = {
    status: number;
    problem?: ApiProblemDetails;
};

export class ApiError extends Error {
    readonly status: number;
    readonly type?: string;
    readonly instance?: string;
    readonly traceId?: string;

    constructor({ status, problem }: ApiErrorOptions) {
        super(
            problem?.detail ??
                problem?.title ??
                (status === 0
                    ? 'Không thể kết nối tới Control API.'
                    : 'Không thể hoàn tất yêu cầu.'),
        );

        this.name = 'ApiError';
        this.status = status;
        this.type = problem?.type;
        this.instance = problem?.instance;
        this.traceId = problem?.traceId;
    }
}

export function isApiError(error: unknown): error is ApiError {
    return error instanceof ApiError;
}

export function createApiError(status: number, payload: unknown): ApiError {
    const problem = isProblemDetails(payload)
        ? {
              type: readString(payload, 'type'),
              title: readString(payload, 'title'),
              status: readNumber(payload, 'status') ?? status,
              detail: readString(payload, 'detail'),
              instance: readString(payload, 'instance'),
              traceId: readString(payload, 'traceId'),
          }
        : undefined;

    return new ApiError({ status, problem });
}

function isProblemDetails(value: unknown): value is Record<string, unknown> {
    return typeof value === 'object' && value !== null;
}

function readString(value: Record<string, unknown>, key: string): string | undefined {
    const field = value[key];

    return typeof field === 'string' ? field : undefined;
}

function readNumber(value: Record<string, unknown>, key: string): number | undefined {
    const field = value[key];

    return typeof field === 'number' ? field : undefined;
}
