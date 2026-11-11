"use client";

import { useEffect, useState } from "react";
import { apiFetch } from "@/lib/api/fetcher";
import styles from "./api-health.module.css";

type CheckResult = "healthy" | "unhealthy" | "unreachable";
type ApiStatus = "checking" | "healthy" | "unreachable";
// "unknown": the API itself can't be reached, so we can't tell anything about the database.
type DbStatus = "checking" | "healthy" | "unhealthy" | "unknown";

// The health endpoints are not part of the OpenAPI document, so they're called directly.
async function check(path: string): Promise<CheckResult> {
  try {
    const res = await apiFetch<{ status: number; data: string }>(path);
    if (res.status === 200 && res.data === "Healthy") {
      return "healthy";
    }
    return "unhealthy";
  } catch {
    return "unreachable";
  }
}

function toApiStatus(live: CheckResult): ApiStatus {
  if (live === "healthy") {
    return "healthy";
  }
  return "unreachable";
}

function toDbStatus(live: CheckResult, ready: CheckResult): DbStatus {
  if (live !== "healthy") {
    return "unknown";
  }
  if (ready === "healthy") {
    return "healthy";
  }
  return "unhealthy";
}

export function ApiHealth() {
  const [api, setApi] = useState<ApiStatus>("checking");
  const [db, setDb] = useState<DbStatus>("checking");

  useEffect(() => {
    Promise.all([check("/health/live"), check("/health/ready")]).then(
      ([live, ready]) => {
        setApi(toApiStatus(live));
        setDb(toDbStatus(live, ready));
      },
    );
  }, []);

  return (
    <p className={styles.health}>
      <span className={styles.status} data-status={api}>
        API: {api}
      </span>
      <span className={styles.status} data-status={db}>
        DB: {db}
      </span>
    </p>
  );
}
