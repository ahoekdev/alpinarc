import { Input as BaseInput } from "@base-ui/react/input";
import type { ComponentProps } from "react";
import styles from "./input.module.css";

type InputProps = Omit<ComponentProps<typeof BaseInput>, "className">;

/** Passes `ref` through, so React Hook Form's `register` works on it. */
export function Input(props: InputProps) {
  return <BaseInput className={styles.input} {...props} />;
}
