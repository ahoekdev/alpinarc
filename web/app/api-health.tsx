"use client";

import { useEffect, useState } from "react";
import { apiFetch } from "@/lib/api/fetcher";
import styles from "./api-health.module.css";

type Status = "checking" | "healthy" | "unreachable";

export function ApiHealth() {
  const [status, setStatus] = useState<Status>("checking");

  useEffect(() => {
    apiFetch<{ status: number; data: string }>("/health")
      .then((res) =>
        setStatus(
          res.status === 200 && res.data === "Healthy"
            ? "healthy"
            : "unreachable",
        ),
      )
      .catch(() => setStatus("unreachable"));
  }, []);

  return (
    <p className={styles.status} data-status={status}>
      API: {status}
    </p>
  );
}
