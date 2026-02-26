import type { StorybookConfig } from '@storybook/react-vite';

const config: StorybookConfig = {
  stories: ['../src/**/*.mdx', '../src/**/*.stories.@(js|jsx|mjs|ts|tsx)'],
  addons: ['@storybook/addon-docs'],
  framework: {
    name: '@storybook/react-vite',
    options: {},
  },
  viteFinal: (config) => {
    // vite-plugin-pwa fails during Storybook builds because Storybook's internal
    // assets (e.g. sb-manager/globals-runtime.js ~3 MB) exceed the Workbox 2 MiB
    // precache limit. PWA is not needed for Storybook, so we remove it entirely.
    // VitePWA() returns Plugin[] (array), so config.plugins may contain nested
    // arrays – we flat() first before filtering by plugin name.
    const isPwaPlugin = (p: unknown): boolean =>
      !!p &&
      typeof p === 'object' &&
      'name' in p &&
      typeof (p as { name: unknown }).name === 'string' &&
      ((p as { name: string }).name === 'vite-plugin-pwa' ||
        (p as { name: string }).name.startsWith('vite-plugin-pwa:'));

    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    config.plugins = (config.plugins as any[])
      ?.flat(Infinity)
      .filter((p: unknown) => !isPwaPlugin(p));

    return config;
  },
};

export default config;
