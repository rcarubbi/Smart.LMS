"use client";

import { useRouter } from "next/navigation";
import { useEffect } from "react";
import { currentUser } from "@/lib/api";

export default function AuthGuard({
  roles,
  children,
}: {
  roles?: Array<"Admin" | "Teacher" | "Student">;
  children: React.ReactNode;
}) {
  const router = useRouter();

  useEffect(() => {
    const user = currentUser();
    if (!user) {
      router.replace("/login");
      return;
    }
    if (roles && !roles.includes(user.role as "Admin" | "Teacher" | "Student")) {
      router.replace("/");
    }
  }, [roles, router]);

  useEffect(() => {
    if (!currentUser()) router.replace("/login");
  }, [router]);

  return <>{children}</>;
}
