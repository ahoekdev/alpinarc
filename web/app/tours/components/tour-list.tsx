"use client";

import Link from "next/link";
import { EmptyState } from "@/components/ui/state/empty-state";
import { ErrorState } from "@/components/ui/state/error-state";
import {
  List,
  ListItem,
  ListItemContent,
  ListItemMeta,
} from "@/components/ui/list/list";
import { LoadingState } from "@/components/ui/state/loading-state";
import { useListTours } from "@/lib/api/__generated__";
import { formatNumbers } from "@/lib/format";

const entityName = "tours";

export function TourList() {
  const { data: tours, isPending, isError } = useListTours();

  if (isError) {
    return <ErrorState entityName={entityName} />;
  }

  if (isPending) {
    return <LoadingState entityName={entityName} />;
  }

  if (tours.length === 0) {
    return <EmptyState entityName={entityName} />;
  }

  return (
    <List>
      {tours.map((tour) => (
        <ListItem key={tour.id}>
          <ListItemContent>
            <Link href={`/tours/${tour.id}`}>{tour.name}</Link>
          </ListItemContent>
          <ListItemMeta>
            {tour.stageCount} {tour.stageCount === 1 ? "stage" : "stages"} ·{" "}
            {formatNumbers(tour.totals)}
          </ListItemMeta>
        </ListItem>
      ))}
    </List>
  );
}
