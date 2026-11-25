"use client";

import { EmptyState } from "@/components/ui/state/empty-state";
import { ErrorState } from "@/components/ui/state/error-state";
import {
  List,
  ListItem,
  ListItemContent,
  ListItemMeta,
} from "@/components/ui/list/list";
import { LoadingState } from "@/components/ui/state/loading-state";
import { useListLodges } from "@/lib/api/__generated__";

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
        <ListItem key={lodge.id}>
          <ListItemContent>{lodge.name}</ListItemContent>
          <ListItemMeta>
            <time dateTime={lodge.createdAt}>
              {new Date(lodge.createdAt).toLocaleDateString()}
            </time>
          </ListItemMeta>
        </ListItem>
      ))}
    </List>
  );
}
