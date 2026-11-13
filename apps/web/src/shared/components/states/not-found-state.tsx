import { useNavigate } from '@tanstack/react-router';
import { FileQuestion } from 'lucide-react';

import { EmptyState } from '@/shared/components/states/empty-state';
import { Button } from '@/shared/components/ui/button';
import { PageContainer } from '@/shared/components/layout/page-container';

export function NotFoundState() {
    const navigate = useNavigate();

    return (
        <PageContainer>
            <EmptyState
                className="mt-8"
                icon={<FileQuestion className="size-8" />}
                title="Page not found"
                description="The page you requested does not exist or is no longer available."
                action={<Button onClick={() => navigate({ to: '/' })}>Return home</Button>}
            />
        </PageContainer>
    );
}
