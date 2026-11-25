"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useQueryClient } from "@tanstack/react-query";
import { useForm } from "react-hook-form";
import { Button } from "@/components/ui/button/button";
import { Field } from "@/components/ui/field/field";
import { Form } from "@/components/ui/form/form";
import { Input } from "@/components/ui/input/input";
import { Select } from "@/components/ui/select/select";
import {
  getListStagesQueryKey,
  useCreateStage,
  useListLodges,
} from "@/lib/api/__generated__";
import { getFieldError } from "@/lib/api/problem-details";
import type { CreateStageRequest } from "@/lib/api/__generated__/alpinarcApiV1.schemas";
import {
  CreateStageBody,
  createStageBodyAscentMetersMax,
  createStageBodyAscentMetersMin,
  createStageBodyDescentMetersMax,
  createStageBodyDescentMetersMin,
  createStageBodyDistanceMetersMax,
  createStageBodyDurationMinutesMax,
} from "@/lib/api/__generated__/index.zod";

/** The fields the API can report an error on, by JSON property name. */
const fields = [
  "fromLodgeId",
  "toLodgeId",
  "distanceMeters",
  "ascentMeters",
  "descentMeters",
  "durationMinutes",
] as const satisfies readonly (keyof CreateStageRequest)[];

export function StageForm() {
  const queryClient = useQueryClient();
  const lodges = useListLodges();

  const createStage = useCreateStage({
    mutation: {
      onSuccess: () =>
        queryClient.invalidateQueries({ queryKey: getListStagesQueryKey() }),
    },
  });

  const {
    register,
    handleSubmit,
    reset,
    setError,
    formState: { errors },
  } = useForm({
    resolver: zodResolver(CreateStageBody),
    defaultValues: { fromLodgeId: "", toLodgeId: "" },
  });

  function onSubmit(data: CreateStageRequest) {
    createStage.mutate(
      { data },
      {
        onSuccess: () => reset(),
        // Show the API's messages on their fields, e.g. when both lodges are the same.
        onError: (error) => {
          for (const field of fields) {
            const message = getFieldError(error, field);
            if (message) {
              setError(field, { message });
            }
          }
        },
      },
    );
  }

  const hasFieldError = fields.some((field) =>
    getFieldError(createStage.error, field),
  );

  const lodgeOptions = (
    <>
      <option value="" disabled>
        {lodges.isPending ? "Loading lodges…" : "Choose a lodge"}
      </option>
      {lodges.data?.map((lodge) => (
        <option key={lodge.id} value={lodge.id}>
          {lodge.name}
        </option>
      ))}
    </>
  );

  // valueAsNumber: the inputs hold text, the API takes numbers. An empty input
  // becomes NaN, which the Zod schema rejects.
  return (
    <Form
      onSubmit={handleSubmit(onSubmit)}
      error={
        lodges.isError
          ? "Could not load lodges."
          : createStage.isError && !hasFieldError
            ? "There was a problem saving the stage."
            : undefined
      }
    >
      <Field label="From lodge" error={errors.fromLodgeId?.message}>
        <Select required {...register("fromLodgeId")}>
          {lodgeOptions}
        </Select>
      </Field>
      <Field label="To lodge" error={errors.toLodgeId?.message}>
        <Select required {...register("toLodgeId")}>
          {lodgeOptions}
        </Select>
      </Field>
      <Field label="Distance (m)" error={errors.distanceMeters?.message}>
        <Input
          type="number"
          min={1}
          max={createStageBodyDistanceMetersMax}
          required
          {...register("distanceMeters", { valueAsNumber: true })}
        />
      </Field>
      <Field label="Ascent (m)" error={errors.ascentMeters?.message}>
        <Input
          type="number"
          min={createStageBodyAscentMetersMin}
          max={createStageBodyAscentMetersMax}
          required
          {...register("ascentMeters", { valueAsNumber: true })}
        />
      </Field>
      <Field label="Descent (m)" error={errors.descentMeters?.message}>
        <Input
          type="number"
          min={createStageBodyDescentMetersMin}
          max={createStageBodyDescentMetersMax}
          required
          {...register("descentMeters", { valueAsNumber: true })}
        />
      </Field>
      <Field label="Hiking time (min)" error={errors.durationMinutes?.message}>
        <Input
          type="number"
          min={1}
          max={createStageBodyDurationMinutesMax}
          required
          {...register("durationMinutes", { valueAsNumber: true })}
        />
      </Field>
      <Button type="submit" disabled={createStage.isPending}>
        Add stage
      </Button>
    </Form>
  );
}
