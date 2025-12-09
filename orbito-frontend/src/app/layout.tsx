import type { Metadata } from "next";
import "./globals.css";
import { QueryProvider } from "@/core/providers/QueryProvider";
import { Toaster } from "@/shared/ui/sonner";

export const metadata: Metadata = {
  title: "Orbito Platform",
  description: "Subscription Management Platform",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="pl">
      <body className="antialiased">
        <QueryProvider>
          {children}
          <Toaster />
        </QueryProvider>
      </body>
    </html>
  );
}
