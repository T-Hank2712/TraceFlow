import { Box } from 'lucide-react';
import { createFileRoute } from '@tanstack/react-router';

import { EmptyState } from '@/shared/components/states/empty-state';
import { PageContainer } from '@/shared/components/layout/page-container';
import { PageHeader } from '@/shared/components/layout/page-header';

export const Route = createFileRoute('/')({
  component: HomePage,
});

function HomePage() {
  return (
    <PageContainer>
      <PageHeader
        title="TraceFlow"
        description="Control-plane management console"
      />

      <EmptyState
        className="mt-6"
        icon={<Box className="size-8" />}
        title="Frontend foundation is ready"
        description="Feature screens will be added in upcoming iterations."
      />
    </PageContainer>
  );
}