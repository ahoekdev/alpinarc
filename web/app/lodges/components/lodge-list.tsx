"use client";

import { EmptyState } from "@/components/state/empty-state";
import { ErrorState } from "@/components/state/error-state";
import { List, ListItem } from "@/components/list";
import { LoadingState } from "@/components/state/loading-state";
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
    <List>
      {lodges.map((lodge) => (
        <ListItem
          key={lodge.id}
          meta={
            <time dateTime={lodge.createdAt}>
              {new Date(lodge.createdAt).toLocaleDateString()}
            </time>
          }
        >
          {lodge.name}
        </ListItem>
      ))}
    </List>
  );
}
