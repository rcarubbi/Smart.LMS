"use client";

import Nav from "./Nav";

export default function AppShell({ children }: { children: React.ReactNode }) {
  return (
    <div className="min-h-dvh">
      <Nav />
      <div className="mx-auto max-w-6xl px-4 py-6">{children}</div>
    </div>
  );
}