import { Field as BaseField } from "@base-ui/react/field";
import type { ComponentProps, ReactNode } from "react";
import styles from "./field.module.css";
import { mergeClassName } from "./merge-class-name";

type FieldProps = Omit<ComponentProps<typeof BaseField.Root>, "invalid"> & {
  label: ReactNode;
  /** The error message, e.g. from React Hook Form. Marks the field invalid. */
  error?: string;
  /** The control, e.g. an `Input`. */
  children: ReactNode;
};

/**
 * A label, a control and its error message. Validation happens elsewhere
 * (React Hook Form); this only shows the result and wires up accessibility.
 */
export function Field({
  label,
  error,
  children,
  className,
  ...props
}: FieldProps) {
  const invalid = error !== undefined;

  return (
    <BaseField.Root
      invalid={invalid}
      className={mergeClassName(styles.field, className)}
      {...props}
    >
      <BaseField.Label className={styles.label}>{label}</BaseField.Label>
      {children}
      {/* `invalid` alone doesn't show the error: it only shows by itself for
          native validation, so `match` hands control to the caller. */}
      <BaseField.Error match={invalid} className={styles.error}>
        {error}
      </BaseField.Error>
    </BaseField.Root>
  );
}
