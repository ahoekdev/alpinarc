"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useQueryClient } from "@tanstack/react-query";
import { useForm } from "react-hook-form";
import { Form } from "@/components/ui/form/form";
import { Button } from "@/components/ui/button/button";
import { Field } from "@/components/ui/field/field";
import { Input } from "@/components/ui/input/input";
import { getListLodgesQueryKey, useCreateLodge } from "@/lib/api/__generated__";
import { getFieldError } from "@/lib/api/problem-details";
import type { CreateLodgeRequest } from "@/lib/api/__generated__/alpinarcApiV1.schemas";
import {
  CreateLodgeBody,
  createLodgeBodyNameMax,
} from "@/lib/api/__generated__/index.zod";

export function LodgeForm() {
  const queryClient = useQueryClient();

  const createLodge = useCreateLodge({
    mutation: {
      onSuccess: () =>
        queryClient.invalidateQueries({ queryKey: getListLodgesQueryKey() }),
    },
  });

  const {
    register,
    handleSubmit,
    reset,
    setError,
    formState: { errors },
  } = useForm({
    resolver: zodResolver(CreateLodgeBody),
    defaultValues: { name: "" },
  });

  function onSubmit(data: CreateLodgeRequest) {
    createLodge.mutate(
      { data },
      {
        onSuccess: () => reset(),
        // Show the API's message on the field, e.g. when the name is taken.
        // React Hook Form clears it again once the name is edited.
        onError: (error) => {
          const message = getFieldError(error, "name");
          if (message) {
            setError("name", { message });
          }
        },
      },
    );
  }

  return (
    <Form
      onSubmit={handleSubmit(onSubmit)}
      error={
        createLodge.isError && !getFieldError(createLodge.error, "name")
          ? "There was a problem saving the lodge."
          : undefined
      }
    >
      <Field label="Lodge name" error={errors.name?.message}>
        <Input
          maxLength={createLodgeBodyNameMax}
          required
          {...register("name")}
        />
      </Field>
      <Button type="submit" disabled={createLodge.isPending}>
        Add lodge
      </Button>
    </Form>
  );
}
