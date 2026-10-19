"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { currentUser } from "@/lib/api";
import AppShell from "@/components/AppShell";

export default function ClassroomsPage() {
  const router = useRouter();

  useEffect(() => {
    const user = currentUser();
    router.replace(!user ? "/login" : user.role === "Admin" ? "/admin/classrooms" : user.role === "Teacher" ? "/teaching" : "/me");
  }, [router]);

  return <AppShell><p className="text-sm text-white/50">Redirecionando...</p></AppShell>;
}
