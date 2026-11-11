"use client";

// TEMPORARY: a minimal lodges list and form that proves the chain end to end
// (Postgres → API → OpenAPI → Orval client → web). Replace it with the real lodges UI.

import { useEffect, useState, type FormEvent } from "react";
import { createLodge, listLodges } from "@/lib/api/generated";
import type {
  LodgeResponse,
  ValidationProblemDetails,
} from "@/lib/api/generated/alpinarcApiV1.schemas";
import styles from "./lodges.module.css";

// Turns the API's 400 response into one line, e.g. "The Name field is required."
function validationMessage(problem: ValidationProblemDetails): string {
  const messages = Object.values(problem.errors ?? {}).flat();
  if (messages.length > 0) {
    return messages.join(" ");
  }
  return problem.title ?? "The lodge could not be saved.";
}

type LodgeListState =
  | { status: "loading" }
  | { status: "loaded"; lodges: LodgeResponse[] }
  | { status: "error"; message: string };

async function fetchLodges(): Promise<LodgeListState> {
  try {
    const res = await listLodges();
    if (res.status !== 200) {
      return { status: "error", message: "Could not load lodges." };
    }
    return { status: "loaded", lodges: res.data };
  } catch {
    return { status: "error", message: "Could not reach the API." };
  }
}

export function Lodges() {
  const [list, setList] = useState<LodgeListState>({ status: "loading" });
  const [name, setName] = useState("");
  const [saving, setSaving] = useState(false);
  const [saveError, setSaveError] = useState<string | null>(null);

  useEffect(() => {
    fetchLodges().then(setList);
  }, []);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setSaving(true);
    setSaveError(null);
    try {
      const res = await createLodge({ name });
      if (res.status === 400) {
        setSaveError(validationMessage(res.data));
        return;
      }
      if (res.status !== 201) {
        setSaveError("The lodge could not be saved.");
        return;
      }
      setName("");
      // Reload rather than insert locally, so the list keeps the API's sort order (by name).
      setList(await fetchLodges());
    } catch {
      setSaveError("Could not reach the API.");
    } finally {
      setSaving(false);
    }
  }

  return (
    <section className={styles.lodges}>
      <h2>Lodges</h2>

      <form className={styles.form} onSubmit={handleSubmit}>
        <input
          aria-label="Lodge name"
          placeholder="Lodge name"
          value={name}
          onChange={(event) => setName(event.target.value)}
          maxLength={200}
          required
        />
        <button type="submit" disabled={saving}>
          Add lodge
        </button>
      </form>
      {saveError && (
        <p className={styles.error} role="alert">
          {saveError}
        </p>
      )}

      <LodgeList list={list} />
    </section>
  );
}

function LodgeList({ list }: { list: LodgeListState }) {
  if (list.status === "error") {
    return <p className={styles.error}>{list.message}</p>;
  }
  if (list.status === "loading") {
    return <p className={styles.muted}>Loading lodges…</p>;
  }
  if (list.lodges.length === 0) {
    return <p className={styles.muted}>No lodges yet.</p>;
  }
  return (
    <ul className={styles.list}>
      {list.lodges.map((lodge) => (
        <li key={lodge.id}>
          <span>{lodge.name}</span>
          <time className={styles.muted} dateTime={lodge.createdAt}>
            {new Date(lodge.createdAt).toLocaleDateString()}
          </time>
        </li>
      ))}
    </ul>
  );
}
