import { createFileRoute } from '@tanstack/react-router';

export const Route = createFileRoute('/')({
  component: HomePage,
});

function HomePage() {
  return (
    <main className="flex min-h-screen items-center justify-center bg-background p-6">
      <section className="space-y-2 text-center">
        <h1 className="text-3xl font-semibold tracking-tight">TraceFlow</h1>
        <p className="text-sm text-muted-foreground">
          Frontend foundation is ready for feature development.
        </p>
      </section>
    </main>
  );
}