import type { ReactNode } from "react";

interface AuthLayoutProps {
  children: ReactNode;
}

export default function AuthLayout({ children }: AuthLayoutProps) {
  return (
    <div className="flex min-h-screen items-center justify-center bg-muted/50 p-4">
      <div className="w-full">{children}</div>
    </div>
  );
}
