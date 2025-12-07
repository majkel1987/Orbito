import type { Metadata } from "next";
import "./globals.css";

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
        {children}
      </body>
    </html>
  );
}
