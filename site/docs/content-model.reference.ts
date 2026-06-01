/**
 * Reference types for SITE-003. Not compiled until Vite scaffold (SITE-002).
 * Copy blocks: see content-copy-matrix.md
 */

export type Profile = 'recruiter' | 'developer' | 'executive';
export type Locale = 'es' | 'en';

export type LocalizedCopy = Record<Locale, string>;
export type ProfileCopy = Record<Profile, LocalizedCopy>;

export type ContentBlock = {
  id: string;
  section: string;
  copy: ProfileCopy;
};

export type DecisionCard = {
  id: string;
  title: ProfileCopy;
  context: ProfileCopy;
  decision: ProfileCopy;
  tradeoff: ProfileCopy;
  learning: ProfileCopy;
};

export const STORAGE_KEYS = {
  profile: 'omnicore-site-profile',
  locale: 'omnicore-site-locale',
} as const;

export const DEFAULT_PROFILE: Profile = 'developer';

export function resolveCopy(
  blocks: ContentBlock[],
  id: string,
  profile: Profile,
  locale: Locale,
): string {
  const block = blocks.find((b) => b.id === id);
  if (!block) return '';

  const direct = block.copy[profile]?.[locale];
  if (direct) return direct;

  const fallbackEs = block.copy[profile]?.es;
  if (fallbackEs) return fallbackEs;

  return block.copy.developer[locale] ?? block.copy.developer.es ?? '';
}

export function detectInitialLocale(): Locale {
  if (typeof navigator === 'undefined') return 'es';
  return navigator.language.toLowerCase().startsWith('en') ? 'en' : 'es';
}
