import { Button as BaseButton } from "@base-ui/react/button";
import type { ComponentProps } from "react";
import styles from "./button.module.css";
import { mergeClassName } from "./merge-class-name";

export function Button({
  className,
  ...props
}: ComponentProps<typeof BaseButton>) {
  return (
    <BaseButton
      className={mergeClassName(styles.button, className)}
      {...props}
    />
  );
}
