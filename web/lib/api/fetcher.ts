const baseUrl = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5033";

/**
 * Fetch wrapper for the API's JSON endpoints, used by the Orval-generated client.
 * Prefixes the API base URL and parses the JSON body. Throws if the body is not JSON.
 */
export async function apiFetch<T>(
  url: string,
  options?: RequestInit,
): Promise<T> {
  const response = await fetch(`${baseUrl}${url}`, options);

  // 204 No Content has no body to parse.
  const data = response.status === 204 ? undefined : await response.json();

  return {
    data,
    status: response.status,
    headers: response.headers,
  } as T;
}

/**
 * Fetch wrapper for the API's plain-text endpoints, such as the health checks.
 */
export async function apiFetchText(
  url: string,
  options?: RequestInit,
): Promise<{ data: string; status: number; headers: Headers }> {
  const response = await fetch(`${baseUrl}${url}`, options);

  return {
    data: await response.text(),
    status: response.status,
    headers: response.headers,
  };
}
