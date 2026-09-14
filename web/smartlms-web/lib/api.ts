export const API_URL = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:8080";

export type Tokens = { accessToken: string; refreshToken: string };

const ACCESS_KEY = "smartlms.access";
const REFRESH_KEY = "smartlms.refresh";

export function getAccessToken(): string | null {
  if (typeof window === "undefined") return null;
  return localStorage.getItem(ACCESS_KEY);
}

export function storeTokens(tokens: Tokens) {
  localStorage.setItem(ACCESS_KEY, tokens.accessToken);
  localStorage.setItem(REFRESH_KEY, tokens.refreshToken);
}

export function clearTokens() {
  localStorage.removeItem(ACCESS_KEY);
  localStorage.removeItem(REFRESH_KEY);
}

type JwtPayload = { sub?: string; role?: string; email?: string; exp?: number };

export function currentUser(): JwtPayload | null {
  const token = getAccessToken();
  if (!token) return null;
  try {
    const payload = token.split(".")[1].replace(/-/g, "+").replace(/_/g, "/");
    return JSON.parse(atob(padded(payload))) as JwtPayload;
  } catch {
    return null;
  }
}

function padded(base64: string) {
  return base64 + "=".repeat((4 - (base64.length % 4)) % 4);
}

let refreshing: Promise<boolean> | null = null;

async function refreshTokens(): Promise<boolean> {
  if (!refreshing) {
    refreshing = (async () => {
      const refresh = localStorage.getItem(REFRESH_KEY);
      if (!refresh) return false;

      const response = await fetch(`${API_URL}/api/auth/refresh`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ refreshToken: refresh }),
      });

      if (!response.ok) {
        clearTokens();
        return false;
      }

      const tokens = (await response.json()) as Tokens;
      storeTokens(tokens);
      return true;
    })().finally(() => {
      refreshing = null;
    });
  }

  return refreshing;
}

export class ApiError extends Error {
  constructor(
    public status: number,
    public body?: unknown,
  ) {
    super(`API request failed with status ${status}`);
  }
}

export async function api<T>(
  path: string,
  options: RequestInit & { retry?: boolean } = {},
): Promise<T> {
  const headers = new Headers(options.headers);
  const token = getAccessToken();

  if (token) {
    headers.set("Authorization", `Bearer ${token}`);
  }

  if (options.body && !(options.body instanceof FormData) && !headers.has("Content-Type")) {
    headers.set("Content-Type", "application/json");
  }

  const response = await fetch(`${API_URL}${path}`, { ...options, headers });

  if (response.status === 401 && options.retry !== false && (await refreshTokens())) {
    return api<T>(path, { ...options, retry: false });
  }

  if (!response.ok) {
    throw new ApiError(response.status, await response.json().catch(() => undefined));
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}

/** Extracts a human-readable message from an ApiError thrown against an ErrorOr problem response. */
export function errorMessage(err: unknown, fallback = "Falha na operação."): string {
  if (err instanceof ApiError) {
    const body = err.body as { title?: string; detail?: string; message?: string } | undefined;
    return body?.message ?? body?.detail ?? body?.title ?? fallback;
  }
  return fallback;
}

export type ListItem = { id: string; name: string; order: number };

export type Paged<T> = { items: T[]; total: number; page: number; pageSize: number; totalPages: number };

/** Fetches a paged list endpoint (always returns the Paged<T> envelope). */
export async function apiPage<T>(path: string, options?: RequestInit & { retry?: boolean }): Promise<Paged<T>> {
  return api<Paged<T>>(path, options);
}

/** Fetches a list endpoint and unwraps the Paged<T> envelope to the raw items array. */
export async function apiList<T>(path: string, options?: RequestInit & { retry?: boolean }): Promise<T[]> {
  const paged = await apiPage<T>(path, options);
  return paged.items;
}
