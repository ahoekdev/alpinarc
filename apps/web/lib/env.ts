export function apiUrl() {
  const url = process.env.API_URL;

  if (!url) {
    throw new Error("API_URL is not set, see .env.example");
  }

  return url;
}
