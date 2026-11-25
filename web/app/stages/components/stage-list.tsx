"use client";

import { EmptyState } from "@/components/ui/state/empty-state";
import { ErrorState } from "@/components/ui/state/error-state";
import { List, ListItem, ListItemContent } from "@/components/ui/list/list";
import { LoadingState } from "@/components/ui/state/loading-state";
import { useListStages } from "@/lib/api/__generated__";
import { formatStage } from "@/lib/format";

const entityName = "stages";

export function StageList() {
  const { data: stages, isPending, isError } = useListStages();

  if (isError) {
    return <ErrorState entityName={entityName} />;
  }

  if (isPending) {
    return <LoadingState entityName={entityName} />;
  }

  if (stages.length === 0) {
    return <EmptyState entityName={entityName} />;
  }

  return (
    <List>
      {stages.map((stage) => (
        <ListItem key={stage.id}>
          <ListItemContent>{formatStage(stage)}</ListItemContent>
        </ListItem>
      ))}
    </List>
  );
}
