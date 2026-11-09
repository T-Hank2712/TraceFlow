import { Spinner } from '@/shared/components/ui/spinner';
import { cn } from '@/shared/lib/utils';

type LoadingStateProps = {
  className?: string;
  message?: string;
};

export function LoadingState({
  className,
  message = 'Loading...',
}: LoadingStateProps) {
  return (
    <div
      className={cn(
        'flex min-h-40 flex-col items-center justify-center gap-3 text-sm text-muted-foreground',
        className,
      )}
    >
      <Spinner className="size-5" />
      <p>{message}</p>
    </div>
  );
}