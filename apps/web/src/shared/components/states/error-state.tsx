import type { ReactNode } from 'react';

import { AlertCircle } from 'lucide-react';

import { Button } from '@/shared/components/ui/button';
import { cn } from '@/shared/lib/utils';

type ErrorStateProps = {
    title?: string;
    message: string;
    action?: ReactNode;
    onRetry?: () => void;
    className?: string;
};

export function ErrorState({
    title = 'Something went wrong',
    message,
    action,
    onRetry,
    className,
}: ErrorStateProps) {
    return (
        <section
            className={cn(
                'border-destructive/30 bg-destructive/5 flex min-h-56 flex-col items-center justify-center rounded-lg border p-6 text-center',
                className,
            )}
        >
            <AlertCircle className="text-destructive mb-4 size-6" />
            <h2 className="text-sm font-semibold">{title}</h2>
            <p className="text-muted-foreground mt-1 max-w-sm text-sm">{message}</p>

            <div className="mt-4 flex items-center gap-2">
                {onRetry ? (
                    <Button variant="outline" onClick={onRetry}>
                        Try again
                    </Button>
                ) : null}
                {action}
            </div>
        </section>
    );
}
