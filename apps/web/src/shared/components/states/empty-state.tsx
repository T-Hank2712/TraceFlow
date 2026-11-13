import type { ReactNode } from 'react';

import { cn } from '@/shared/lib/utils';

type EmptyStateProps = {
    title: string;
    description?: string;
    icon?: ReactNode;
    action?: ReactNode;
    className?: string;
};

export function EmptyState({ title, description, icon, action, className }: EmptyStateProps) {
    return (
        <section
            className={cn(
                'flex min-h-56 flex-col items-center justify-center rounded-lg border border-dashed p-6 text-center',
                className,
            )}
        >
            {icon ? <div className="text-muted-foreground mb-4">{icon}</div> : null}

            <h2 className="text-sm font-semibold">{title}</h2>

            {description ? (
                <p className="text-muted-foreground mt-1 max-w-sm text-sm">{description}</p>
            ) : null}

            {action ? <div className="mt-4">{action}</div> : null}
        </section>
    );
}
