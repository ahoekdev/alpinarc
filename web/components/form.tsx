import type { ComponentProps } from "react";
import styles from "./form.module.css";

type FormProps = Omit<ComponentProps<"form">, "className"> & {
  /** A message about the whole form, e.g. when saving failed. */
  error?: string;
};

/** Stacks its fields and buttons, with an optional form-level error below. */
export function Form({ error, children, ...props }: FormProps) {
  return (
    <form className={styles.form} {...props}>
      {children}
      {error && (
        <p role="alert" className={styles.error}>
          {error}
        </p>
      )}
    </form>
  );
}
