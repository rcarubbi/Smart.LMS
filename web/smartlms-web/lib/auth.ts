import { currentUser } from "./api";

export type Role = "Admin" | "Teacher" | "Student";

export function useSession() {
  const user = currentUser();
  return {
    user,
    role: (user?.role as Role) ?? null,
    isAdmin: user?.role === "Admin",
    isTeacher: user?.role === "Teacher",
    isStudent: user?.role === "Student",
  };
}
