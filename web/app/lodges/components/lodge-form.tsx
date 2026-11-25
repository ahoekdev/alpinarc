"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useQueryClient } from "@tanstack/react-query";
import { useForm } from "react-hook-form";
import { Button } from "@/components/ui/button";
import { Field } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { getListLodgesQueryKey, useCreateLodge } from "@/lib/api/generated";
import type { CreateLodgeRequest } from "@/lib/api/generated/alpinarcApiV1.schemas";
import {
  CreateLodgeBody,
  createLodgeBodyNameMax,
} from "@/lib/api/generated/index.zod";
import styles from "../lodges.module.css";

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
    <form className={styles.form} onSubmit={handleSubmit(onSubmit)}>
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
      {createLodge.isError && (
        <p role="alert" className={styles.formError}>
          There was a problem saving the lodge.
        </p>
      )}
    </form>
  );
}
