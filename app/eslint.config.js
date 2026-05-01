import js from '@eslint/js'
import globals from 'globals'
import reactHooks from 'eslint-plugin-react-hooks'
import reactRefresh from 'eslint-plugin-react-refresh'
import tseslint from 'typescript-eslint'
import { defineConfig, globalIgnores } from 'eslint/config'

const featureInternalPattern = [
  'artists', 'auth', 'concerts', 'contracts', 'customer',
  'messaging', 'notifications', 'payments', 'reviews',
  'search', 'user', 'venues',
].map((f) => ({
  group: [`@/features/${f}/**`],
  message: `Import from @/features/${f} (barrel), not from internal paths.`,
}))

export default defineConfig([
  globalIgnores(['dist']),
  {
    files: ['**/*.{ts,tsx}'],
    extends: [
      js.configs.recommended,
      tseslint.configs.strictTypeChecked,
      reactHooks.configs.flat.recommended,
      reactRefresh.configs.vite,
    ],
    languageOptions: {
      ecmaVersion: 2020,
      globals: globals.browser,
      parserOptions: {
        projectService: true,
        tsconfigRootDir: import.meta.dirname,
      },
    },
    rules: {
      'no-restricted-imports': ['error', { patterns: featureInternalPattern }],
    },
  },
])
