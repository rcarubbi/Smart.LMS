"use client";

import { useEffect } from "react";
import Icon from "./Icon";

export default function ConfirmModal({
  open,
  title,
  message,
  confirmLabel = "Confirmar",
  onConfirm,
  onCancel,
}: {
  open: boolean;
  title: string;
  message: React.ReactNode;
  confirmLabel?: string;
  onConfirm: () => void;
  onCancel: () => void;
}) {
  useEffect(() => {
    if (!open) return;
    const onKey = (e: KeyboardEvent) => {
      if (e.key === "Escape") onCancel();
    };
    window.addEventListener("keydown", onKey);
    return () => window.removeEventListener("keydown", onKey);
  }, [open, onCancel]);

  if (!open) return null;

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 p-4 backdrop-blur-sm"
      onClick={onCancel}
    >
      <div
        className="glass w-full max-w-md p-0"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="flex items-center justify-between border-b border-white/10 px-5 py-3">
          <h3 className="text-base font-medium">{title}</h3>
          <button onClick={onCancel} className="text-white/60 hover:text-white">
            <Icon name="close" />
          </button>
        </div>
        <div className="px-5 py-4 text-sm text-white/80">{message}</div>
        <div className="flex justify-end gap-2 border-t border-white/10 px-5 py-3">
          <button className="btn btn-ghost" onClick={onCancel}>
            <Icon name="thumb_down" /> Cancelar
          </button>
          <button
            className="btn bg-red-600 text-white hover:bg-red-500"
            onClick={onConfirm}
          >
            <Icon name="thumb_up" /> {confirmLabel}
          </button>
        </div>
      </div>
    </div>
  );
}
