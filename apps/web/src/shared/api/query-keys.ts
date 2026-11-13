export const queryKeys = {
    workspaces: ['workspaces'] as const,

    projects: (workspaceId: string) => ['workspaces', workspaceId, 'projects'] as const,

    applications: (projectId: string) => ['projects', projectId, 'applications'] as const,

    apiKeys: (applicationId: string) => ['applications', applicationId, 'api-keys'] as const,

    logs: (workspaceId: string, projectId: string, filters: unknown) =>
        ['logs', workspaceId, projectId, filters] as const,
};
