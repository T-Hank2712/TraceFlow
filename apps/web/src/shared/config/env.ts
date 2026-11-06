export const env = {
  controlApiBaseUrl:
    import.meta.env.VITE_CONTROL_API_BASE_URL ?? '/control',
  enableDevtools:
    import.meta.env.VITE_ENABLE_DEVTOOLS === 'true',
};