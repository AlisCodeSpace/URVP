"use client";

import {
  useCallback,
  useEffect,
  useRef,
  useState,
  type DependencyList,
  type Dispatch,
  type SetStateAction,
} from "react";
import { ApiError } from "@/lib/api";

export type CancellableQueryOptions<T> = {
  /** When false, the request is not started and `loading` is false. */
  enabled?: boolean;
  /** Used when the rejection is not an `ApiError` or `Error`. */
  fallbackError?: string;
  initialData?: T | null;
  /** Defaults to `enabled`, so a disabled query does not start in a loading state. */
  initialLoading?: boolean;
  /** Keep the previous `data` when the request fails. */
  keepDataOnError?: boolean;
  /** Replaces `data` when the request fails. Overrides `keepDataOnError`. */
  dataOnError?: (current: T | null) => T | null;
  /** Drop `data` when `enabled` becomes false. */
  clearDataWhenDisabled?: boolean;
  onSuccess?: (data: T) => void;
};

export type CancellableQueryResult<T> = {
  data: T | null;
  loading: boolean;
  error: string | null;
  /** Runs the query again. Resolves when that attempt settles. Pass `{ silent: true }` to skip the loading flag. */
  reload: (options?: { silent?: boolean }) => Promise<void>;
  setData: Dispatch<SetStateAction<T | null>>;
  setError: (error: string | null) => void;
};

type Snapshot<T> = {
  data: T | null;
  loading: boolean;
  error: string | null;
};

type Pending = {
  resolve: () => void;
};

function sameDeps(left: DependencyList, right: DependencyList) {
  if (left.length !== right.length) return false;
  for (let index = 0; index < left.length; index += 1) {
    if (!Object.is(left[index], right[index])) return false;
  }
  return true;
}

function errorMessage(err: unknown, fallback: string) {
  if (err instanceof ApiError) return err.message;
  if (err instanceof Error && err.message) return err.message;
  return fallback;
}

/**
 * Loads `queryFn` when `deps` change and ignores the result when a newer run
 * or unmount wins. State is applied from the promise callback, so screens can
 * share one fetch instead of setting loading inside an effect.
 */
export function useCancellableQuery<T>(
  queryFn: () => Promise<T>,
  deps: DependencyList,
  options: CancellableQueryOptions<T> = {},
): CancellableQueryResult<T> {
  const enabled = options.enabled ?? true;
  const clearDataWhenDisabled = options.clearDataWhenDisabled ?? false;
  const fallbackError = options.fallbackError ?? "Something went wrong.";
  const keepDataOnError = options.keepDataOnError ?? false;
  const dataOnError = options.dataOnError;
  const onSuccess = options.onSuccess;

  const runRef = useRef(0);
  const pendingRef = useRef<Pending | null>(null);

  const [epoch, setEpoch] = useState(0);
  const [watch, setWatch] = useState({ deps, enabled });
  const [snapshot, setSnapshot] = useState<Snapshot<T>>(() => ({
    data: options.initialData ?? null,
    loading: options.initialLoading ?? enabled,
    error: null,
  }));

  if (!sameDeps(watch.deps, deps) || watch.enabled !== enabled) {
    setWatch({ deps, enabled });
    setSnapshot((current) => ({
      data: enabled || !clearDataWhenDisabled ? current.data : null,
      loading: enabled,
      error: null,
    }));
  }

  useEffect(() => {
    const pending = pendingRef.current;

    if (!enabled) {
      runRef.current += 1;
      if (pending) {
        pendingRef.current = null;
        pending.resolve();
      }
      return;
    }

    const run = ++runRef.current;
    let cancelled = false;

    function settle() {
      if (run !== runRef.current) return;
      if (pending && pendingRef.current === pending) {
        pendingRef.current = null;
        pending.resolve();
      }
    }

    Promise.resolve()
      .then(() => queryFn())
      .then((data) => {
        if (run !== runRef.current) return;
        if (!cancelled) {
          setSnapshot({ data, loading: false, error: null });
          onSuccess?.(data);
        }
        settle();
      })
      .catch((err: unknown) => {
        if (run !== runRef.current) return;
        if (!cancelled) {
          setSnapshot((current) => ({
            data: dataOnError
              ? dataOnError(current.data)
              : keepDataOnError
                ? current.data
                : null,
            loading: false,
            error: errorMessage(err, fallbackError),
          }));
        }
        settle();
      });

    return () => {
      cancelled = true;
    };
    // `deps` is the caller-supplied list. `queryFn` is the loader from that render.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [enabled, epoch, ...deps]);

  const reload = useCallback((reloadOptions?: { silent?: boolean }) => {
    const promise = new Promise<void>((resolve) => {
      pendingRef.current = { resolve };
    });
    setSnapshot((current) => ({
      ...current,
      loading: reloadOptions?.silent ? current.loading : true,
      error: null,
    }));
    setEpoch((current) => current + 1);
    return promise;
  }, []);

  const setData = useCallback<Dispatch<SetStateAction<T | null>>>((value) => {
    setSnapshot((current) => ({
      ...current,
      data:
        typeof value === "function"
          ? (value as (prev: T | null) => T | null)(current.data)
          : value,
    }));
  }, []);

  const setError = useCallback((error: string | null) => {
    setSnapshot((current) => ({ ...current, error }));
  }, []);

  return {
    data: snapshot.data,
    loading: snapshot.loading,
    error: snapshot.error,
    reload,
    setData,
    setError,
  };
}

/**
 * Runs `effect` after commit and ignores the rest of the task when the
 * component unmounts or `deps` change. Requests that store a result should
 * use `useCancellableQuery` instead.
 */
export function useCancellableEffect(
  effect: (isCurrent: () => boolean) => void | Promise<void>,
  deps: DependencyList,
) {
  const effectRef = useRef(effect);

  useEffect(() => {
    effectRef.current = effect;
  }, [effect]);

  useEffect(() => {
    let cancelled = false;
    void Promise.resolve().then(() => {
      if (cancelled) return;
      return effectRef.current(() => !cancelled);
    });
    return () => {
      cancelled = true;
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, deps);
}
