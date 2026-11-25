import type {
  StageResponse,
  TourTotals,
} from "@/lib/api/__generated__/alpinarcApiV1.schemas";

/** Kilometres with one decimal, e.g. `8.2 km`. */
export function formatDistance(meters: number): string {
  return `${(meters / 1000).toFixed(1)} km`;
}

/** Hours and minutes, e.g. `3 h 30 min`, `4 h` or `45 min`. */
export function formatDuration(minutes: number): string {
  const hours = Math.floor(minutes / 60);
  const rest = minutes % 60;

  if (hours === 0) return `${rest} min`;
  if (rest === 0) return `${hours} h`;
  return `${hours} h ${rest} min`;
}

/** Ascent and descent, e.g. `+650 m / −1320 m`. */
export function formatElevation(ascent: number, descent: number): string {
  return `+${ascent} m / −${descent} m`;
}

type Numbers = Pick<
  TourTotals,
  "distanceMeters" | "ascentMeters" | "descentMeters" | "durationMinutes"
>;

/** A stage's or tour's numbers, e.g. `8.2 km · +650 m / −1320 m · 3 h 30 min`. */
export function formatNumbers(numbers: Numbers): string {
  return [
    formatDistance(numbers.distanceMeters),
    formatElevation(numbers.ascentMeters, numbers.descentMeters),
    formatDuration(numbers.durationMinutes),
  ].join(" · ");
}

/** A stage's start and end, e.g. `Hut A → Hut B`. */
export function formatStageRoute(stage: StageResponse): string {
  return `${stage.fromLodge.name} → ${stage.toLodge.name}`;
}

/**
 * A stage's route and numbers, e.g.
 * `Hut A → Hut B · 8.2 km · +650 m / −1320 m · 3 h 30 min`. The numbers tell
 * variants between the same two lodges apart.
 */
export function formatStage(stage: StageResponse): string {
  return `${formatStageRoute(stage)} · ${formatNumbers(stage)}`;
}
