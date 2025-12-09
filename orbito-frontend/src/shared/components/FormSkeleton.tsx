import { Skeleton } from "@/shared/ui/skeleton";

interface FormSkeletonProps {
  fields?: number;
}

export function FormSkeleton({ fields = 4 }: FormSkeletonProps) {
  return (
    <div className="space-y-4">
      {Array.from({ length: fields }).map((_, index) => (
        <div key={`field-skeleton-${index}`} className="space-y-2">
          <Skeleton className="h-4 w-24" /> {/* Label */}
          <Skeleton className="h-10 w-full" /> {/* Input */}
        </div>
      ))}
      <div className="flex gap-2 pt-4">
        <Skeleton className="h-10 w-24" /> {/* Submit button */}
        <Skeleton className="h-10 w-24" /> {/* Cancel button */}
      </div>
    </div>
  );
}
