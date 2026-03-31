import { useLayoutEffect, useMemo, useRef } from "react";

export function useDebounce<T extends (...args: any[]) => void>(callback: T | undefined, delay = 300): T | undefined {
    const callbackRef = useRef(callback);

    useLayoutEffect(() => {
        callbackRef.current = callback;
    });

    return useMemo(() => {
        if (!callback) return undefined;

        let timeout: ReturnType<typeof setTimeout>;

        return ((...args: Parameters<T>) => {
            clearTimeout(timeout);
            timeout = setTimeout(() => {
                callbackRef.current?.(...args);
            }, delay);
        }) as T;
    }, [delay, !!callback]);
}