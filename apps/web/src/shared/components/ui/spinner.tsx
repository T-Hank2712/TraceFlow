import { LoaderCircle } from 'lucide-react';
import type React from 'react';

import { cn } from '@/shared/lib/utils';

type SpinnerProps = React.ComponentProps<typeof LoaderCircle>;

export function Spinner({ className, ...props }: SpinnerProps) {
  return (
    <LoaderCircle
      aria-label="Loading"
      className={cn('size-4 animate-spin', className)}
      role="status"
      {...props}
    />
  );
}