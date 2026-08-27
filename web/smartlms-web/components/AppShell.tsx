"use client";

import Nav from "./Nav";
import { ToastProvider } from "./ui/Toast";

export default function AppShell({ children }: { children: React.ReactNode }) {
  return (
    <ToastProvider>
      <div className="min-h-dvh">
        <Nav />
        <div className="mx-auto max-w-6xl px-4 py-6">{children}</div>
      </div>
    </ToastProvider>
  );
}
