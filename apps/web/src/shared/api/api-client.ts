import { getAccessToken } from '@/shared/api/auth-session';
import { ApiError, createApiError } from '@/shared/api/api-error';
import { env } from '@/shared/config/env';

type ControlApiRequestOptions = Omit<RequestInit, 'body' | 'headers'> & {
    authenticated?: boolean;
    headers?: HeadersInit;
    json?: unknown;
};

export async function requestControlApi<T>(
    route: string,
    options: ControlApiRequestOptions = {},
): Promise<T> {
    const { authenticated = true, headers: requestHeaders, json, ...requestInit } = options;

    const headers = new Headers(requestHeaders);

    headers.set('Accept', 'application/json');

    if (json !== undefined) {
        headers.set('Content-Type', 'application/json');
    }

    if (authenticated) {
        const accessToken = getAccessToken();

        if (accessToken) {
            headers.set('Authorization', `Bearer ${accessToken}`);
        }
    }

    try {
        const response = await fetch(createControlApiUrl(route), {
            ...requestInit,
            body: json === undefined ? undefined : JSON.stringify(json),
            credentials: requestInit.credentials ?? 'same-origin',
            headers,
        });

        const payload = await readResponseBody(response);

        if (!response.ok) {
            throw createApiError(response.status, payload);
        }

        return payload as T;
    } catch (error) {
        if (error instanceof ApiError) {
            throw error;
        }

        if (error instanceof DOMException && error.name === 'AbortError') {
            throw error;
        }

        throw createApiError(0, undefined);
    }
}

function createControlApiUrl(route: string): string {
    if (!route.startsWith('/')) {
        throw new Error('Control API route must start with "/".');
    }

    const baseUrl = env.controlApiBaseUrl.replace(/\/$/, '');

    return `${baseUrl}${route}`;
}

async function readResponseBody(response: Response): Promise<unknown> {
    if (response.status === 204) {
        return undefined;
    }

    const text = await response.text();

    if (!text) {
        return undefined;
    }

    try {
        return JSON.parse(text) as unknown;
    } catch {
        return text;
    }
}
