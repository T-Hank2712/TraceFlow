import type { ComponentProps } from 'react';

import { cn } from '@/shared/lib/utils';

type PageContainerProps = ComponentProps<'main'>;

export function PageContainer({
  className,
  ...props
}: PageContainerProps) {
  return (
    <main
      className={cn('mx-auto w-full max-w-7xl px-4 py-6 sm:px-6 lg:px-8', className)}
      {...props}
    />
  );
}