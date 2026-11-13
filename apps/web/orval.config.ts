import { defineConfig } from 'orval';

export default defineConfig({
    controlApi: {
        input: {
            target: './openapi/control-api.json',
        },
        output: {
            target: './src/shared/api/generated/control-api.ts',
            client: 'fetch',
            mode: 'single',
            clean: true,
        },
    },
});
