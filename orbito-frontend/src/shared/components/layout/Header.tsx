"use client";

import { Menu } from "lucide-react";
import { TenantSwitcher } from "@/features/auth";
import { UserMenu } from "./UserMenu";
import { Button } from "@/shared/ui/button";

interface HeaderProps {
  onMenuClick?: () => void;
}

export function Header({ onMenuClick }: HeaderProps) {
  return (
    <header className="sticky top-0 z-30 flex h-16 items-center gap-4 border-b bg-background px-4 sm:px-6">
      {/* Left side - Mobile menu trigger */}
      <Button
        variant="ghost"
        size="icon"
        className="md:hidden"
        onClick={onMenuClick}
      >
        <Menu className="h-5 w-5" />
        <span className="sr-only">Toggle menu</span>
      </Button>

      {/* Logo (visible on mobile when sidebar is hidden) */}
      <div className="flex items-center gap-2 md:hidden">
        <div className="flex h-8 w-8 items-center justify-center rounded-md bg-primary text-primary-foreground">
          <span className="text-lg font-bold">O</span>
        </div>
        <span className="text-xl font-bold">Orbito</span>
      </div>

      {/* Spacer */}
      <div className="flex-1" />

      {/* Right side - TenantSwitcher + UserMenu */}
      <div className="flex items-center gap-4">
        <TenantSwitcher />
        <UserMenu />
      </div>
    </header>
  );
}
