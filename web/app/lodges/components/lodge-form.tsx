"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useQueryClient } from "@tanstack/react-query";
import { useForm } from "react-hook-form";
import { getListLodgesQueryKey, useCreateLodge } from "@/lib/api/generated";
import type { CreateLodgeRequest } from "@/lib/api/generated/alpinarcApiV1.schemas";
import {
  CreateLodgeBody,
  createLodgeBodyNameMax,
} from "@/lib/api/generated/index.zod";

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
    formState: { errors },
  } = useForm({
    resolver: zodResolver(CreateLodgeBody),
    defaultValues: { name: "" },
  });

  function onSubmit(data: CreateLodgeRequest) {
    createLodge.mutate({ data }, { onSuccess: () => reset() });
  }

  return (
    <>
      <form onSubmit={handleSubmit(onSubmit)}>
        <input
          aria-label="Lodge name"
          placeholder="Lodge name"
          maxLength={createLodgeBodyNameMax}
          required
          aria-invalid={errors.name ? true : undefined}
          {...register("name")}
        />
        <button type="submit" disabled={createLodge.isPending}>
          Add lodge
        </button>
        {errors.name && <p>{errors.name.message}</p>}
      </form>
      {createLodge.isError && (
        <p role="alert">There was a problem saving the lodge.</p>
      )}
    </>
  );
}
