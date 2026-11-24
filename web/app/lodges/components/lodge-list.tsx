"use client";

import { EmptyState } from "@/components/empty-state";
import { ErrorState } from "@/components/error-state";
import { LoadingState } from "@/components/loading-state";
import { useListLodges } from "@/lib/api/generated";

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
    <ul>
      {lodges.map((lodge) => (
        <li key={lodge.id}>
          {lodge.name} (
          <time dateTime={lodge.createdAt}>
            {new Date(lodge.createdAt).toLocaleDateString()}
          </time>
          )
        </li>
      ))}
    </ul>
  );
}
