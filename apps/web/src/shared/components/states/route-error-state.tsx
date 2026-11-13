import type { ErrorComponentProps } from '@tanstack/react-router';
import { useNavigate } from '@tanstack/react-router';

import { PageContainer } from '@/shared/components/layout/page-container';
import { ErrorState } from '@/shared/components/states/error-state';
import { Button } from '@/shared/components/ui/button';

export function RouteErrorState({ reset }: ErrorComponentProps) {
    const navigate = useNavigate();

    return (
        <PageContainer>
            <ErrorState
                className="mt-8"
                title="Unable to load this page"
                message="An unexpected error occurred while loading this page. Please try again."
                onRetry={reset}
                action={
                    <Button variant="outline" onClick={() => navigate({ to: '/' })}>
                        Return home
                    </Button>
                }
            />
        </PageContainer>
    );
}
