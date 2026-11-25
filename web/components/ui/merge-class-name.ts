type ClassName<State> =
  string | ((state: State) => string | undefined) | undefined;

/**
 * Combines a component's own class with the caller's `className`, which Base
 * UI also accepts as a function of the component's state.
 */
export function mergeClassName<State>(
  ownClassName: string,
  className: ClassName<State>,
) {
  return (state: State) => {
    const callerClassName =
      typeof className === "function" ? className(state) : className;
    return callerClassName
      ? `${ownClassName} ${callerClassName}`
      : ownClassName;
  };
}
