"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useQueryClient } from "@tanstack/react-query";
import { useFieldArray, useForm } from "react-hook-form";
import { z } from "zod";
import { Button } from "@/components/ui/button/button";
import { Field } from "@/components/ui/field/field";
import { Form } from "@/components/ui/form/form";
import { Input } from "@/components/ui/input/input";
import { Select } from "@/components/ui/select/select";
import {
  getListToursQueryKey,
  useCreateTour,
  useListStages,
} from "@/lib/api/__generated__";
import { getFieldError } from "@/lib/api/problem-details";
import {
  CreateTourBody,
  createTourBodyDescriptionMax,
  createTourBodyNameMax,
} from "@/lib/api/__generated__/index.zod";
import { formatStage } from "@/lib/format";
import styles from "./tour-form.module.css";

// React Hook Form's field arrays hold objects, so the form keeps each stage as
// `{ stageId }` and turns them into the API's `stageIds` on submit.
const TourFormValues = z.object({
  name: CreateTourBody.shape.name,
  description: CreateTourBody.shape.description,
  stages: z.array(z.object({ stageId: z.uuid("Choose a stage") })),
});

type TourFormValues = z.infer<typeof TourFormValues>;

export function TourForm() {
  const queryClient = useQueryClient();
  const stages = useListStages();

  const createTour = useCreateTour({
    mutation: {
      onSuccess: () =>
        queryClient.invalidateQueries({ queryKey: getListToursQueryKey() }),
    },
  });

  const {
    register,
    control,
    handleSubmit,
    reset,
    setError,
    formState: { errors },
  } = useForm({
    resolver: zodResolver(TourFormValues),
    defaultValues: { name: "", description: "", stages: [] },
  });

  const tourStages = useFieldArray({ control, name: "stages" });

  /** The API's errors, by form field: `stageIds[i]` belongs to `stages.i.stageId`. */
  function apiErrors(error: unknown) {
    const found: [
      `stages.${number}.stageId` | "name" | "description",
      string,
    ][] = [];

    for (const field of ["name", "description"] as const) {
      const message = getFieldError(error, field);
      if (message) found.push([field, message]);
    }

    tourStages.fields.forEach((_, index) => {
      const message = getFieldError(error, `stageIds[${index}]`);
      if (message) found.push([`stages.${index}.stageId`, message]);
    });

    return found;
  }

  function onSubmit({ name, description, stages }: TourFormValues) {
    createTour.mutate(
      {
        data: {
          name,
          description,
          stageIds: stages.map((stage) => stage.stageId),
        },
      },
      {
        onSuccess: () => reset(),
        onError: (error) => {
          for (const [field, message] of apiErrors(error)) {
            setError(field, { message });
          }
        },
      },
    );
  }

  const hasFieldError =
    createTour.isError && apiErrors(createTour.error).length > 0;

  return (
    <Form
      onSubmit={handleSubmit(onSubmit)}
      error={
        stages.isError
          ? "Could not load stages."
          : createTour.isError && !hasFieldError
            ? "There was a problem saving the tour."
            : undefined
      }
    >
      <Field label="Tour name" error={errors.name?.message}>
        <Input
          maxLength={createTourBodyNameMax}
          required
          {...register("name")}
        />
      </Field>
      <Field label="Description" error={errors.description?.message}>
        <Input
          maxLength={createTourBodyDescriptionMax}
          {...register("description")}
        />
      </Field>
      {tourStages.fields.map((field, index) => (
        <div key={field.id} className={styles.stage}>
          <Field
            label={`Stage ${index + 1}`}
            error={errors.stages?.[index]?.stageId?.message}
          >
            <Select required {...register(`stages.${index}.stageId`)}>
              <option value="" disabled>
                {stages.isPending ? "Loading stages…" : "Choose a stage"}
              </option>
              {stages.data?.map((stage) => (
                <option key={stage.id} value={stage.id}>
                  {formatStage(stage)}
                </option>
              ))}
            </Select>
          </Field>
          <Button type="button" onClick={() => tourStages.remove(index)}>
            Remove
          </Button>
        </div>
      ))}
      <Button type="button" onClick={() => tourStages.append({ stageId: "" })}>
        Add stage
      </Button>
      <Button type="submit" disabled={createTour.isPending}>
        Add tour
      </Button>
    </Form>
  );
}
