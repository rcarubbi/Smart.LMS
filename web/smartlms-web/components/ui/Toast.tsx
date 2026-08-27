"use client";

import { createContext, useCallback, useContext, useRef, useState } from "react";
import type { ReactNode } from "react";
import Icon from "./Icon";

type ToastKind = "success" | "error" | "info";
type Toast = { id: number; kind: ToastKind; message: string };

const ToastCtx = createContext<{ toast: (kind: ToastKind, message: string) => void }>({
  toast: () => {},
});

export function useToast() {
  return useContext(ToastCtx);
}

const colors: Record<ToastKind, string> = {
  success: "text-emerald-300",
  error: "text-red-300",
  info: "text-sky-300",
};

export function ToastProvider({ children }: { children: ReactNode }) {
  const [toasts, setToasts] = useState<Toast[]>([]);
  const idRef = useRef(0);

  const toast = useCallback((kind: ToastKind, message: string) => {
    const id = ++idRef.current;
    setToasts((t) => [...t, { id, kind, message }]);
    setTimeout(() => {
      setToasts((t) => t.filter((x) => x.id !== id));
    }, 3500);
  }, []);

  return (
    <ToastCtx.Provider value={{ toast }}>
      {children}
      <div className="pointer-events-none fixed bottom-4 right-4 z-[60] flex flex-col gap-2">
        {toasts.map((t) => (
          <div
            key={t.id}
            className="glass flex items-center gap-2 px-4 py-3 text-sm text-white/90"
          >
            <Icon
              name={t.kind === "success" ? "check_circle" : t.kind === "error" ? "error" : "info"}
              className={colors[t.kind]}
            />
            {t.message}
          </div>
        ))}
      </div>
    </ToastCtx.Provider>
  );
}
