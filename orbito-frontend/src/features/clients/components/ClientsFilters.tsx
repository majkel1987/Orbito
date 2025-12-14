"use client";

import { Input } from "@/shared/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/ui/select";
import { Button } from "@/shared/ui/button";
import { Search, X } from "lucide-react";
import { useState, useEffect, useRef } from "react";

interface Props {
  search: string;
  status: string;
  onSearchChange: (value: string) => void;
  onStatusChange: (value: string) => void;
  onClear: () => void;
}

export function ClientsFilters({
  search,
  status,
  onSearchChange,
  onStatusChange,
  onClear,
}: Props) {
  const hasFilters = search || (status && status !== "all");

  // Local state for immediate UI updates (prevents focus loss)
  const [localSearch, setLocalSearch] = useState(search);
  const timeoutRef = useRef<NodeJS.Timeout | undefined>(undefined);

  // Sync local state with URL state (only when URL changes externally)
  useEffect(() => {
    setLocalSearch(search);
  }, [search]);

  // Debounced update to URL (500ms after user stops typing)
  const handleSearchInput = (value: string) => {
    setLocalSearch(value);

    // Clear previous timeout
    if (timeoutRef.current) {
      clearTimeout(timeoutRef.current);
    }

    // Set new timeout
    timeoutRef.current = setTimeout(() => {
      onSearchChange(value);
    }, 500);
  };

  // Cleanup timeout on unmount
  useEffect(() => {
    return () => {
      if (timeoutRef.current) {
        clearTimeout(timeoutRef.current);
      }
    };
  }, []);

  return (
    <div className="flex flex-col sm:flex-row gap-4">
      <div className="relative flex-1">
        <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
        <Input
          placeholder="Search clients..."
          value={localSearch}
          onChange={(e) => handleSearchInput(e.target.value)}
          className="pl-9"
        />
      </div>

      <Select value={status || "all"} onValueChange={onStatusChange}>
        <SelectTrigger className="w-[180px]">
          <SelectValue placeholder="All statuses" />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value="all">All statuses</SelectItem>
          <SelectItem value="active">Active</SelectItem>
          <SelectItem value="inactive">Inactive</SelectItem>
        </SelectContent>
      </Select>

      {hasFilters && (
        <Button variant="ghost" onClick={onClear}>
          <X className="mr-2 h-4 w-4" />
          Clear
        </Button>
      )}
    </div>
  );
}
