import type { ReactNode } from 'react';

import { cn } from '@/shared/lib/utils';

type EmptyStateProps = {
  title: string;
  description?: string;
  icon?: ReactNode;
  action?: ReactNode;
  className?: string;
};

export function EmptyState({
  title,
  description,
  icon,
  action,
  className,
}: EmptyStateProps) {
  return (
    <section
      className={cn(
        'flex min-h-56 flex-col items-center justify-center rounded-lg border border-dashed p-6 text-center',
        className,
      )}
    >
      {icon ? (
        <div className="mb-4 text-muted-foreground">{icon}</div>
      ) : null}

      <h2 className="text-sm font-semibold">{title}</h2>

      {description ? (
        <p className="mt-1 max-w-sm text-sm text-muted-foreground">
          {description}
        </p>
      ) : null}

      {action ? <div className="mt-4">{action}</div> : null}
    </section>
  );
}