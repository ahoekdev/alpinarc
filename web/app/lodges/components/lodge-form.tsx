"use client";

import { useState, type SubmitEvent } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { getListLodgesQueryKey, useCreateLodge } from "@/lib/api/generated";

export function LodgeForm() {
  const [name, setName] = useState("");

  const queryClient = useQueryClient();

  const createLodge = useCreateLodge({
    mutation: {
      onSuccess: () => {
        setName("");

        return queryClient.invalidateQueries({
          queryKey: getListLodgesQueryKey(),
        });
      },
    },
  });

  function handleSubmit(event: SubmitEvent<HTMLFormElement>) {
    event.preventDefault();
    createLodge.mutate({ data: { name } });
  }

  return (
    <>
      <form onSubmit={handleSubmit}>
        <input
          aria-label="Lodge name"
          placeholder="Lodge name"
          value={name}
          onChange={(event) => setName(event.target.value)}
          maxLength={200}
          required
        />
        <button type="submit" disabled={createLodge.isPending}>
          Add lodge
        </button>
      </form>
      {createLodge.isError && (
        <p role="alert">There was a problem saving the lodge.</p>
      )}
    </>
  );
}
