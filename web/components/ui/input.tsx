import { Input as BaseInput } from "@base-ui/react/input";
import type { ComponentProps } from "react";
import styles from "./input.module.css";
import { mergeClassName } from "./merge-class-name";

/** Passes `ref` through, so React Hook Form's `register` works on it. */
export function Input({
  className,
  ...props
}: ComponentProps<typeof BaseInput>) {
  return (
    <BaseInput className={mergeClassName(styles.input, className)} {...props} />
  );
}
