const baseUrl = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5033";

/**
 * Fetch wrapper used by the Orval-generated client and by hand-written calls.
 * Prefixes the API base URL and parses JSON responses.
 */
export async function apiFetch<T>(
  url: string,
  options?: RequestInit,
): Promise<T> {
  const response = await fetch(`${baseUrl}${url}`, options);

  const contentType = response.headers.get("content-type") ?? "";
  const body = contentType.includes("application/json")
    ? await response.json()
    : await response.text();

  return {
    data: body,
    status: response.status,
    headers: response.headers,
  } as T;
}
