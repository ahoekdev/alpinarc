import { Field as BaseField } from "@base-ui/react/field";
import type { ComponentProps } from "react";
import styles from "./select.module.css";

type SelectProps = Omit<ComponentProps<"select">, "className">;

/**
 * A native `<select>`, wired to the surrounding `Field` like `Input` (label,
 * invalid state). Passes `ref` through, so React Hook Form's `register` works
 * on it.
 */
export function Select(props: SelectProps) {
  return (
    <BaseField.Control
      render={<select className={styles.select} {...props} />}
    />
  );
}
