"use client";

import { EmptyState } from "@/components/empty-state";
import { ErrorState } from "@/components/error-state";
import { LoadingState } from "@/components/loading-state";
import { useListLodges } from "@/lib/api/generated";
import styles from "../lodges.module.css";

const entityName = "lodges";

export function LodgeList() {
  const { data: lodges, isPending, isError } = useListLodges();

  if (isError) {
    return <ErrorState entityName={entityName} />;
  }

  if (isPending) {
    return <LoadingState entityName={entityName} />;
  }

  if (lodges.length === 0) {
    return <EmptyState entityName={entityName} />;
  }

  return (
    <ul className={styles.list}>
      {lodges.map((lodge) => (
        <li key={lodge.id} className={styles.item}>
          {lodge.name}
          <time className={styles.date} dateTime={lodge.createdAt}>
            {new Date(lodge.createdAt).toLocaleDateString()}
          </time>
        </li>
      ))}
    </ul>
  );
}
