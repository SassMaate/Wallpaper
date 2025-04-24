declare module 'aos' {
  interface AosOptions {
    duration?: number;
    easing?: string;
    once?: boolean;
    offset?: number;
    delay?: number;
    disableMutationObserver?: boolean;
    throttleDelay?: number;
    debounceDelay?: number;
  }

  function init(options?: AosOptions): void;
  function refresh(force?: boolean): void;
  function refreshHard(): void;

  export = {
    init,
    refresh,
    refreshHard
  };
} 