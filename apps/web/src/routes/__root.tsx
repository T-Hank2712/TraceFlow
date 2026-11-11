import { Outlet, createRootRoute } from '@tanstack/react-router';

import { NotFoundState } from '@/shared/components/states/not-found-state';
import { RouteErrorState } from '@/shared/components/states/route-error-state';

export const Route = createRootRoute({
  component: RootComponent,
  errorComponent: RouteErrorState,
  notFoundComponent: NotFoundState,
});

function RootComponent() {
  return <Outlet />;
}