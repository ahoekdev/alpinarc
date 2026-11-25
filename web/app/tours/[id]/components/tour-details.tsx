"use client";

import { use } from "react";
import { PageTitle } from "@/components/ui/page-title/page-title";
import {
  List,
  ListItem,
  ListItemContent,
  ListItemMeta,
} from "@/components/ui/list/list";
import { Stack } from "@/components/ui/stack/stack";
import { ErrorState } from "@/components/ui/state/error-state";
import { LoadingState } from "@/components/ui/state/loading-state";
import { useGetTour } from "@/lib/api/__generated__";
import { formatNumbers, formatStageRoute } from "@/lib/format";

type TourDetailsProps = {
  params: Promise<{ id: string }>;
};

export function TourDetails({ params }: TourDetailsProps) {
  const { id } = use(params);
  const {
    data: tour,
    isPending,
    error,
  } = useGetTour(id, {
    // A 404 won't change on a retry.
    query: { retry: (count, error) => error.status !== 404 && count < 3 },
  });

  if (error?.status === 404) {
    return <p>This tour doesn&apos;t exist.</p>;
  }

  if (error) {
    return <ErrorState entityName="tour" />;
  }

  if (isPending) {
    return <LoadingState entityName="tour" />;
  }

  return (
    <Stack>
      <PageTitle>{tour.name}</PageTitle>
      {tour.description && <p>{tour.description}</p>}
      {tour.stages.length === 0 ? (
        <p>No stages yet.</p>
      ) : (
        <List>
          {tour.stages.map(({ position, stage }) => (
            <ListItem key={position}>
              <ListItemContent>
                {position}. {formatStageRoute(stage)}
              </ListItemContent>
              <ListItemMeta>{formatNumbers(stage)}</ListItemMeta>
            </ListItem>
          ))}
        </List>
      )}
      <p>Total: {formatNumbers(tour.totals)}</p>
    </Stack>
  );
}
